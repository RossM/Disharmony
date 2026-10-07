namespace Disharmony.RulesEngine;

/// <summary>
///     Combines declarative IL rewrite rules to transform a method's Harmony instruction sequence.
/// </summary>
/// <remarks>
///     <para>
///         Use a ruleset to express a transpiler as instruction patterns and output templates. Rules can replace or
///         surround matched instructions, or add code at the start or end of the method. Disharmony uses these operations
///         to wrap calls with patches, coordinate method entry and exit code, and inline patch method bodies.
///     </para>
///     <para>
///         Rules are processed in ascending <see cref="Rule.Phase" /> order. All rules in a phase match against the
///         same input; their combined output becomes the input to the next phase. This lets a later phase transform code
///         introduced by an earlier one. Matched ranges that emit output must not overlap, including for insertion rules.
///         Unmatched instructions are retained.
///     </para>
///     <para>
///         Locals and labels in rule templates are remapped for each match, so templates can reuse matched locals and
///         branch targets or introduce new ones. <see cref="CrossRuleLocals" /> and <see cref="CrossRuleLabels" /> allow
///         separate rules to share emitted state and branch targets within one application of the ruleset.
///     </para>
///     <para>
///         Required matches are checked before a phase emits output. Replacement matches are rejected if they would
///         discard labels or exception-block boundaries inside the matched range; boundary metadata is preserved.
///         Rules must still supply output with valid stack behavior and control flow.
///     </para>
/// </remarks>
public class Ruleset
{
    /// <summary>
    ///     Gets or sets whether debug logging is enabled for every ruleset, regardless of the value passed to
    ///     <see cref="MatchAndReplace(MethodBase, ref List{CodeInstruction}, ILGenerator, bool)" />.
    /// </summary>
    internal static bool forceDebug = false;

    /// <summary>
    ///     Gets or initializes the collection of rules to apply. The default is an empty collection.
    /// </summary>
    public List<Rule> Rules { get; init; } = [];

    /// <summary>
    ///     Gets or initializes output local-variable placeholders that share one newly declared local across matches,
    ///     rules, and phases within a call to <see cref="MatchAndReplace(MethodBase, ref List{CodeInstruction}, ILGenerator, bool)" />.
    ///     The default is an empty collection.
    /// </summary>
    public List<LocalBuilder> CrossRuleLocals { get; init; } = [];

    /// <summary>
    ///     Gets or initializes output label placeholders that share one newly defined label across matches, rules, and
    ///     phases within a call to <see cref="MatchAndReplace(MethodBase, ref List{CodeInstruction}, ILGenerator, bool)" />.
    ///     The default is an empty collection.
    /// </summary>
    public List<Label> CrossRuleLabels { get; init; } = [];

    /// <summary>
    ///     Initializes an empty ruleset.
    /// </summary>
    public Ruleset() { }

    /// <summary>
    ///     Initializes a ruleset with the specified rules.
    /// </summary>
    /// <param name="rules">The rules to apply.</param>
    public Ruleset(params List<Rule> rules)
    {
        this.Rules = rules;
    }

    /// <summary>
    ///     Applies this ruleset to an instruction list.
    /// </summary>
    /// <param name="method">The method whose instructions are being transformed.</param>
    /// <param name="instructionsList">
    ///     The instruction list to transform. On success, it is replaced with the transformed list; on failure, the
    ///     original reference is preserved.
    /// </param>
    /// <param name="generator">The IL generator used to define replacement locals and labels.</param>
    /// <param name="debug">Whether to write matching and output details to Harmony's file log.</param>
    /// <exception cref="InvalidOperationException">
    ///     A rule is invalid, does not meet its required match count, overlaps another match, or refers to a local that
    ///     cannot be resolved; or a phase has no matches with a non-null <see cref="Rule.Output" />.
    /// </exception>
    public void MatchAndReplace(
        MethodBase method,
        ref List<CodeInstruction> instructionsList,
        ILGenerator generator,
        bool debug = false)
    {
        var worker = new Processor(this, method, instructionsList, generator, debug || forceDebug);
        worker.MatchAndReplace();
        instructionsList = worker.outInstructions;
    }

    /// <summary>
    ///     Applies a set of rules to an instruction sequence and returns the transformed instructions.
    /// </summary>
    /// <param name="rules">The rules to apply.</param>
    /// <param name="method">The method whose instructions are being transformed.</param>
    /// <param name="instructions">The input instruction sequence.</param>
    /// <param name="generator">The IL generator used to define replacement locals and labels.</param>
    /// <param name="debug">Whether to write matching and output details to Harmony's file log.</param>
    /// <returns>A new list containing the transformed instruction sequence.</returns>
    /// <exception cref="InvalidOperationException">
    ///     A rule is invalid, does not meet its required match count, overlaps another match, or refers to a local that
    ///     cannot be resolved; or a phase has no matches with a non-null <see cref="Rule.Output" />.
    /// </exception>
    public static List<CodeInstruction> MatchAndReplace(
        List<Rule> rules,
        MethodBase method,
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator,
        bool debug = false)
    {
        var instructionsList = new List<CodeInstruction>(instructions);
        new Ruleset(rules).MatchAndReplace(method, ref instructionsList, generator, debug: debug);
        return instructionsList;
    }
}

/// <summary>
///     Specifies where a rule emits its <see cref="Rule.Output" /> relative to a match.
/// </summary>
public enum OutputMode
{
    /// <summary>
    ///     <see cref="Rule.Output" /> replaces the instructions that match <see cref="Rule.Pattern" />.
    /// </summary>
    Replace,

    /// <summary>
    ///     <see cref="Rule.Output" /> is inserted before the first instruction of each match of the
    ///     <see cref="Rule.Pattern" />.
    /// </summary>
    InsertBefore,

    /// <summary>
    ///     <see cref="Rule.Output" /> is inserted after the last instruction of each match of the <see cref="Rule.Pattern" />.
    /// </summary>
    InsertAfter,

    /// <summary>
    ///     <see cref="Rule.Output" /> is inserted at the start of the instruction sequence. No pattern matching is done.
    /// </summary>
    MethodPrefix,

    /// <summary>
    ///     <see cref="Rule.Output" /> is inserted at the end of the instruction sequence. No pattern matching is done.
    /// </summary>
    MethodPostfix,
}

/// <summary>
///     Describes one IL rewrite using an instruction pattern, an output template, and where to emit that output.
/// </summary>
/// <remarks>
///     <para>
///         A <see cref="Ruleset" /> finds contiguous occurrences of <see cref="Pattern" /> and emits a remapped copy of
///         <see cref="Output" /> for each selected match. <see cref="Mode" /> selects replacement or insertion before or
///         after the match. For <see cref="OutputMode.MethodPrefix" /> and <see cref="OutputMode.MethodPostfix" />, leave
///         the pattern null or empty: output is emitted once at the start or end of the instruction sequence, without
///         matching or checking <see cref="Min" /> and <see cref="Max" />. Appending output does not redirect existing returns.
///     </para>
///     <para>
///         Matching accepts compact and expanded forms of equivalent IL opcodes; a call pattern also accepts callvirt
///         to the same method. Local indexes and branch labels in the pattern are placeholders: repeated uses must
///         resolve consistently within a match. Other operands, including argument indexes, must match their values.
///     </para>
///     <para>
///         Output references to pattern locals and labels reuse the matched locals and branch targets. A new output local
///         must use a <see cref="LocalBuilder" /> operand so its type is known; new output labels are defined as needed.
///         These mappings are scoped to each match unless the output placeholder is registered in
///         <see cref="Ruleset.CrossRuleLocals" /> or <see cref="Ruleset.CrossRuleLabels" />, in which case it resolves to
///         a new local or label shared across rules.
///     </para>
///     <para>
///         For rules with a pattern, null output checks for required matches without changing them. Such a rule must
///         share its phase with a rule that emits output at least once. Empty output removes matched instructions in
///         <see cref="OutputMode.Replace" /> mode.
///     </para>
/// </remarks>
public class Rule
{
    /// <summary>
    ///     Gets or initializes the minimum required match count. Fewer matches cause an
    ///     <see cref="InvalidOperationException" />. Use 0 to allow no matches. The default is 1.
    ///     Ignored for <see cref="OutputMode.MethodPrefix" /> and <see cref="OutputMode.MethodPostfix" />.
    /// </summary>
    public int Min { get; init; } = 1;

    /// <summary>
    ///     Gets or initializes the maximum number of matches to process, scanning from the start of the phase's input.
    ///     Matching stops at this limit; additional occurrences do not cause an error. Zero or a negative value means
    ///     no limit. The default is 1.
    ///     Ignored for <see cref="OutputMode.MethodPrefix" /> and <see cref="OutputMode.MethodPostfix" />.
    /// </summary>
    public int Max { get; init; } = 1;

    /// <summary>
    ///     Gets or initializes the ordering priority for method prefix or postfix rules at the same insertion point
    ///     within a phase. Higher values are emitted first; ties retain rule collection order. The default is 0.
    ///     Priority does not resolve overlapping pattern matches.
    /// </summary>
    public int Priority { get; init; } = 0;

    /// <summary>
    ///     Gets or initializes the processing phase. Lower-numbered phases run first; later phases can match earlier
    ///     output. The default is 1.
    /// </summary>
    public int Phase { get; init; } = 1;

    /// <summary>
    ///     Gets or initializes where <see cref="Output" /> is emitted relative to each match.
    ///     The default is <see cref="OutputMode.Replace" />.
    /// </summary>
    public OutputMode Mode { get; init; } = OutputMode.Replace;

    /// <summary>
    ///     Gets or initializes the contiguous instruction pattern. A non-empty pattern is required except for
    ///     <see cref="OutputMode.MethodPrefix" /> and <see cref="OutputMode.MethodPostfix" />, where it must be null or empty.
    /// </summary>
    public CodeInstruction[]? Pattern { get; init; }

    /// <summary>
    ///     Gets or initializes the instruction template to emit with locals and labels remapped for each match.
    ///     For rules with a pattern, <see langword="null" /> validates matches without emitting output.
    ///     An empty array removes matches in <see cref="OutputMode.Replace" /> mode.
    ///     Method prefix and postfix rules require non-null output.
    /// </summary>
    public required CodeInstruction[]? Output { get; init; }

    /// <summary>
    ///     Gets or initializes the name used in debug output and errors, or <see langword="null" /> for no name.
    /// </summary>
    public string? Name { get; init; }
}
