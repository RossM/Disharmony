using Disharmony.RulesEngine;
using Disharmony.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using WaylithClr;

namespace Disharmony.WaylithCLR;

internal class Transformer() : MethodBodyTransformer(100)
{
    private struct MethodPatch
    {
        public required Ruleset ruleset;
        public bool optimize;
        public bool debug;
    }

    protected override bool StaysAttached => true;
    protected override bool CanTransformConcurrently => true;

    private readonly Lock sync = new();

    private readonly Dictionary<RuntimeMethodHandle, MethodPatch> methodPatches = [];

    protected override bool CanTransform(RuntimeMethodHandle method)
    {
        lock (sync)
        {
            return methodPatches.ContainsKey(method);
        }
    }

    protected override bool TryTransform(
        RuntimeMethodHandle methodHandle,
        in MethodBodyView body,
        ReadOnlySpan<MethodBodyClause> clauses,
        MethodBodyBuilder result)
    {
        MethodPatch patch;
        lock (sync)
        {
            if (!methodPatches.TryGetValue(methodHandle, out patch))
                return false;
        }

        var method = MethodBase.GetMethodFromHandle(methodHandle)!;
        var methodView = MethodView.Create(method, body, clauses);

        var instructionsList = methodView.CodeInstructions;
        var generator = methodView.Generator;

        try
        {
            patch.ruleset.MatchAndReplace(method, ref instructionsList, generator);
            if (instructionsList.Any(inst => inst.blocks.Count > 0))
                ExceptionFixup.Fix(method, ref instructionsList, generator);
        }
        catch (Exception e)
        {
            Patcher.ReportException(e);
            return false;
        }

        if (Backend.optimizerEnabled && patch.optimize)
            try
            {
                var optimizer = new Optimizer.Optimizer(method, instructionsList, generator, debug: patch.debug);
                instructionsList = optimizer.Optimize();
            }
            catch (Exception e)
            {
                Patcher.ReportException(e);
            }

        methodView.CodeInstructions = instructionsList;
        methodView.Emit(result);

        return true;
    }

    public void ApplyPatch(RuntimeMethodHandle methodHandle, Ruleset ruleset, bool useTrampolines, bool debug, bool optimize)
    {
        lock (sync)
        {
            methodPatches[methodHandle] = new MethodPatch { ruleset = ruleset, debug = debug, optimize = optimize };
        }

        Rebuild(methodHandle);
    }

    public void RemovePatch(RuntimeMethodHandle methodHandle)
    {
        lock (sync)
        {
            methodPatches.Remove(methodHandle);
        }

        Rebuild(methodHandle);
    }
}
