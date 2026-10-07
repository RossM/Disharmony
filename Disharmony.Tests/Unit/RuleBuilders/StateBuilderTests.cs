using Disharmony.RuleBuilders;
using Disharmony.RulesEngine;
using BoundParameter = Disharmony.ParameterBinding;

namespace Disharmony.Tests.Unit.RuleBuilders;

[TestFixture]
public sealed class StateBuilderTests
{
    private static readonly ParameterInfo[] Parameters = typeof(StateBuilderTargets)
        .GetMethod(nameof(StateBuilderTargets.Parameters))!
        .GetParameters();

    private static readonly ParameterInfo PrimitiveParameter = Parameters.Single(p => p.Name == "primitive");
    private static readonly ParameterInfo PrimitiveByRefParameter = Parameters.Single(p => p.Name == "primitiveByRef");
    private static readonly ParameterInfo ReferenceParameter = Parameters.Single(p => p.Name == "reference");
    private static readonly ParameterInfo StructureParameter = Parameters.Single(p => p.Name == "structure");

    private static readonly MethodInvocation FirstPatch = new(
        typeof(StateBuilderTargets).GetMethod(nameof(StateBuilderTargets.FirstPatch))!);

    private static readonly MethodInvocation SecondPatch = new(
        typeof(StateBuilderTargets).GetMethod(nameof(StateBuilderTargets.SecondPatch))!);

    private static PatchInfo CreatePatch(BoundParameter[] parameters, Invocation? patch = null) => new()
    {
        unpatchKey = 0,
        inner = EmptyInvocation.Instance,
        patch = patch ?? FirstPatch,
        patchKind = PatchKind.Prefix,
        parameters = parameters,
        options = PatchOptions.Default,
        priority = 0,
    };

    [Test]
    public void AssignStateVariableIndexes_NonStateBinding_DoesNotAllocateLocal()
    {
        var context = new RuleBuilderContext();
        var builder = new StateBuilder(context);
        var parameter = new BoundParameter
        {
            parameter = PrimitiveParameter,
            bindingType = BindingType.Argument,
            scope = Scope.Outer,
        };

        builder.AssignStateVariableIndexes([CreatePatch([parameter])]);

        Assert.Multiple(() =>
        {
            Assert.That(parameter.local, Is.Null);
            Assert.That(context.locals, Is.Empty);
        });
    }

    [Test]
    public void AssignStateVariableIndexes_SameNormalizedKeyAndType_ReusesLocalAndPreservesAllocationOrder()
    {
        var context = new RuleBuilderContext();
        var builder = new StateBuilder(context);
        var primitive = new BoundParameter
        {
            parameter = PrimitiveParameter,
            bindingType = BindingType.State,
            scope = Scope.Outer,
            stateKey = "shared",
        };
        var reference = new BoundParameter
        {
            parameter = ReferenceParameter,
            bindingType = BindingType.State,
            scope = Scope.Outer,
            stateKey = "reference",
        };
        var primitiveByRef = new BoundParameter
        {
            parameter = PrimitiveByRefParameter,
            bindingType = BindingType.State,
            scope = Scope.Outer,
            stateKey = "shared",
        };

        builder.AssignStateVariableIndexes(
        [
            CreatePatch([primitive, reference]),
            CreatePatch([primitiveByRef], patch: SecondPatch),
        ]);

        Assert.Multiple(() =>
        {
            Assert.That(primitive.local, Is.SameAs(primitiveByRef.local));
            Assert.That(primitive.local!.Type, Is.EqualTo(typeof(int)));
            Assert.That(reference.local!.Type, Is.EqualTo(typeof(string)));
            Assert.That(reference.local, Is.Not.SameAs(primitive.local));
            Assert.That(context.locals, Has.Count.EqualTo(2));
            Assert.That(context.locals[0], Is.SameAs(primitive.local));
            Assert.That(context.locals[1], Is.SameAs(reference.local));
        });
    }

    [Test]
    public void AssignStateVariableIndexes_SameKeyWithDifferentTypes_ThrowsInvalidOperationException()
    {
        var builder = new StateBuilder(new RuleBuilderContext());
        var primitive = new BoundParameter
        {
            parameter = PrimitiveParameter,
            bindingType = BindingType.State,
            scope = Scope.Outer,
            stateKey = "shared",
        };
        var reference = new BoundParameter
        {
            parameter = ReferenceParameter,
            bindingType = BindingType.State,
            scope = Scope.Outer,
            stateKey = "shared",
        };

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            builder.AssignStateVariableIndexes([CreatePatch([primitive, reference])]))!;

        Assert.That(exception.Message,
            Is.EqualTo("Incompatible state types: System.String and System.Int32"));
    }

    [Test]
    public void AssignStateVariableIndexes_NullKey_ThrowsInvalidOperationException()
    {
        var builder = new StateBuilder(new RuleBuilderContext());
        var parameter = new BoundParameter
        {
            parameter = PrimitiveParameter,
            bindingType = BindingType.State,
            scope = Scope.Outer,
            stateKey = null,
        };

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            builder.AssignStateVariableIndexes([CreatePatch([parameter])]))!;

        Assert.That(exception.Message, Is.EqualTo("Null StateKey"));
    }

    [Test]
    public void BuildRules_NoStateBindings_ReturnsNoRules()
    {
        var context = new RuleBuilderContext();
        var builder = new StateBuilder(context);
        var parameter = new BoundParameter
        {
            parameter = PrimitiveParameter,
            bindingType = BindingType.Argument,
            scope = Scope.Outer,
        };
        builder.AssignStateVariableIndexes([CreatePatch([parameter])]);

        Rule[] rules = [.. builder.BuildRules()];

        Assert.That(rules, Is.Empty);
    }

    [Test]
    public void BuildRules_PrimitiveReferenceAndStructStates_EmitOrderedDefaultInitializers()
    {
        var context = new RuleBuilderContext();
        var builder = new StateBuilder(context);
        var primitive = new BoundParameter
        {
            parameter = PrimitiveParameter,
            bindingType = BindingType.State,
            scope = Scope.Outer,
            stateKey = "primitive",
        };
        var reference = new BoundParameter
        {
            parameter = ReferenceParameter,
            bindingType = BindingType.State,
            scope = Scope.Outer,
            stateKey = "reference",
        };
        var structure = new BoundParameter
        {
            parameter = StructureParameter,
            bindingType = BindingType.State,
            scope = Scope.Outer,
            stateKey = "structure",
        };
        builder.AssignStateVariableIndexes([CreatePatch([primitive, reference, structure])]);

        Rule rule = builder.BuildRules().Single();
        CodeInstruction[] output = rule.Output!;

        Assert.Multiple(() =>
        {
            Assert.That(rule.Name, Is.EqualTo("state variable initialization"));
            Assert.That(rule.Mode, Is.EqualTo(OutputMode.MethodPrefix));
            Assert.That(rule.Priority, Is.EqualTo(100));
            Assert.That(rule.Pattern, Is.Null);
            Assert.That(output.Select(i => (i.opcode, i.operand)), Is.EqualTo(new (OpCode, object?)[]
            {
                (OpCodes.Ldc_I4_0, null),
                (OpCodes.Stloc_S, primitive.local!.Builder),
                (OpCodes.Ldnull, null),
                (OpCodes.Stloc_S, reference.local!.Builder),
                (OpCodes.Ldloca_S, structure.local!.Builder),
                (OpCodes.Initobj, typeof(BindingStruct)),
            }));
        });
    }

    [Test]
    public void BuildRules_SharedState_EmitsOneInitializer()
    {
        var context = new RuleBuilderContext();
        var builder = new StateBuilder(context);
        var primitive = new BoundParameter
        {
            parameter = PrimitiveParameter,
            bindingType = BindingType.State,
            scope = Scope.Outer,
            stateKey = "shared",
        };
        var primitiveByRef = new BoundParameter
        {
            parameter = PrimitiveByRefParameter,
            bindingType = BindingType.State,
            scope = Scope.Outer,
            stateKey = "shared",
        };
        builder.AssignStateVariableIndexes(
        [
            CreatePatch([primitive]),
            CreatePatch([primitiveByRef], patch: SecondPatch),
        ]);

        Rule rule = builder.BuildRules().Single();
        CodeInstruction[] output = rule.Output!;

        Assert.Multiple(() =>
        {
            Assert.That(primitive.local, Is.SameAs(primitiveByRef.local));
            Assert.That(context.locals, Has.Count.EqualTo(1));
            Assert.That(output.Select(i => (i.opcode, i.operand)), Is.EqualTo(new (OpCode, object?)[]
            {
                (OpCodes.Ldc_I4_0, null),
                (OpCodes.Stloc_S, primitive.local!.Builder),
            }));
        });
    }

    [Test]
    public void ValidateState_CompatibleAndUnrelatedBindings_DoesNotThrow()
    {
        var primitive = new BoundParameter
        {
            parameter = PrimitiveParameter,
            bindingType = BindingType.State,
            scope = Scope.Outer,
            stateKey = "shared",
        };
        var primitiveByRef = new BoundParameter
        {
            parameter = PrimitiveByRefParameter,
            bindingType = BindingType.State,
            scope = Scope.Outer,
            stateKey = "shared",
        };
        var reference = new BoundParameter
        {
            parameter = ReferenceParameter,
            bindingType = BindingType.State,
            scope = Scope.Outer,
            stateKey = "reference",
        };
        var nonState = new BoundParameter
        {
            parameter = StructureParameter,
            bindingType = BindingType.Argument,
            scope = Scope.Outer,
        };

        Assert.DoesNotThrow(() => StateBuilder.ValidateState(
        [
            CreatePatch([primitive, nonState]),
            CreatePatch([primitiveByRef, reference], patch: SecondPatch),
        ]));
    }

    [Test]
    public void ValidateState_NullKey_ThrowsPatchExceptionWithParameterContext()
    {
        var parameter = new BoundParameter
        {
            parameter = PrimitiveParameter,
            bindingType = BindingType.State,
            scope = Scope.Outer,
            stateKey = null,
        };

        PatchException exception = Assert.Throws<PatchException>(() =>
            StateBuilder.ValidateState([CreatePatch([parameter], patch: SecondPatch)]))!;

        Assert.Multiple(() =>
        {
            Assert.That(exception.Message, Does.Contain(nameof(StateBuilderTargets.SecondPatch)));
            Assert.That(exception.InnerException, Is.TypeOf<ParameterBindingException>()
                .With.Message.EqualTo("primitive: Null StateKey"));
        });
    }

    [Test]
    public void ValidateState_SameKeyWithDifferentTypes_ThrowsPatchExceptionForConflictingPatch()
    {
        var primitive = new BoundParameter
        {
            parameter = PrimitiveParameter,
            bindingType = BindingType.State,
            scope = Scope.Outer,
            stateKey = "shared",
        };
        var reference = new BoundParameter
        {
            parameter = ReferenceParameter,
            bindingType = BindingType.State,
            scope = Scope.Outer,
            stateKey = "shared",
        };

        PatchException exception = Assert.Throws<PatchException>(() => StateBuilder.ValidateState(
        [
            CreatePatch([primitive]),
            CreatePatch([reference], patch: SecondPatch),
        ]))!;

        Assert.Multiple(() =>
        {
            Assert.That(exception.Message, Does.Contain(nameof(StateBuilderTargets.SecondPatch)));
            Assert.That(exception.InnerException, Is.TypeOf<ParameterBindingException>()
                .With.Message.EqualTo("reference: Incompatible state types: System.String and System.Int32"));
        });
    }
}
