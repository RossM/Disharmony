using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using WaylithClr;

namespace Disharmony.WaylithCLR;

internal class MethodView
{
    public List<CodeInstruction> CodeInstructions { get; set; } = null!;

    public ILGenerator Generator { get; } = null!;

    private MethodView() { }

    // TODO

    public static MethodView Create(MethodBase method, in MethodBodyView body, ReadOnlySpan<MethodBodyClause> clauses)
    {
        var generator = PatchProcessor.CreateILGenerator();

        // Create CodeInstructions by parsing the method IL bytes.
        // All accesses to locals must use forms that take LocalBuilders - ldloc, ldloca, and stloc. Translate ldloc.0 etc to the LocalBuilder forms.
        // Calli operands must be a type that implements IInlineSignature.
        // An example disassembler in my style is here: https://github.com/RossM/RimworldMods/blob/4cb24e775480252b446e07e535e7eda6ebd80430/Source_Disassembler/Disassembler.cs
        // Refer to what Harmony does for setting up exception blocks.

        // Yes, the harmony CodeInstruction interface is awful, and I would like to make something better, but for now I want to get the
        // WaylithCLR backend working.

        throw new NotImplementedException();
    }

    public void Emit(MethodBodyBuilder builder)
    {
        // Write the method to the MethodBodyBuilder using the transformed CodeInstructions.
        // Use System.Reflection.Emit to do the actual work of converting opcodes to bytes as much as possible.

        throw new NotImplementedException();
    }
}
