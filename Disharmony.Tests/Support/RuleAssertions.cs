using Disharmony.RulesEngine;

namespace Disharmony.Tests.Support;

internal static class RuleAssertions
{
    public static void AssertInstructions(CodeInstruction[] actual, CodeInstruction[] expected) =>
        Assert.That(actual, Is.EqualTo(expected).Using<CodeInstruction>(SameInstruction));

    private static bool SameInstruction(CodeInstruction a, CodeInstruction b) =>
        a.opcode == b.opcode &&
        (a.operand is Label[] aLabels && b.operand is Label[] bLabels
            ? aLabels.SequenceEqual(bLabels)
            : Equals(a.operand, b.operand)) &&
        a.labels.SequenceEqual(b.labels) &&
        a.blocks.Select(block => (block.blockType, block.catchType))
            .SequenceEqual(b.blocks.Select(block => (block.blockType, block.catchType)));

    // Compare complete rules, including labels and exception markers at each instruction.
    // Expected instructions remain inline in each test so the generated control flow is visible.
    public static void AssertRules(Rule[] actual, Rule[] expected)
    {
        Assert.That(actual, Has.Length.EqualTo(expected.Length));
        Assert.Multiple(() =>
        {
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.That((actual[i].Mode, actual[i].Name, actual[i].Min, actual[i].Max, actual[i].Priority, actual[i].Phase),
                    Is.EqualTo((expected[i].Mode, expected[i].Name, expected[i].Min, expected[i].Max, expected[i].Priority, expected[i].Phase)),
                    $"Rule {i} metadata");
                Assert.That(actual[i].Pattern, Is.EqualTo(expected[i].Pattern).Using<CodeInstruction>(SameInstruction),
                    $"Rule {i} pattern");
                Assert.That(actual[i].Output, Is.EqualTo(expected[i].Output).Using<CodeInstruction>(SameInstruction),
                    $"Rule {i} output");
            }
        });
    }
}
