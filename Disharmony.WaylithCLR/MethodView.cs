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
        throw new NotImplementedException();
    }

    public void Emit(MethodBodyBuilder builder)
    {
        throw new NotImplementedException();
    }
}
