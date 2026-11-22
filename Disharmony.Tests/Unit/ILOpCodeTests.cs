using System.Reflection.Metadata;

namespace Disharmony.Tests.Unit;

[TestFixture]
public sealed class ILOpCodeTests
{
    [Test]
    public void ValuesMatchReflectionEmitOpCodes()
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.Static;

#if NETFRAMEWORK
        // The test host also references System.Reflection.Metadata.
        Type ilOpCode = typeof(OpCodeData).Assembly.GetType("System.Reflection.Metadata.ILOpCode", throwOnError: true)!;
#else
        Type ilOpCode = typeof(ILOpCode);
#endif
        Assert.That(Enum.GetUnderlyingType(ilOpCode), Is.EqualTo(typeof(ushort)));

        Dictionary<string, OpCode> opCodes = new(StringComparer.OrdinalIgnoreCase);
        HashSet<OpCode> seenOpCodes = [];

        foreach (FieldInfo opCodeField in typeof(OpCodes).GetFields(flags))
            opCodes[opCodeField.Name] = (OpCode)opCodeField.GetValue(null)!;

        foreach (FieldInfo valueField in ilOpCode.GetFields(flags))
        {
            string reflectionEmitName = valueField.Name == "Tail" ? nameof(OpCodes.Tailcall) : valueField.Name;
            var opCode = opCodes[reflectionEmitName];
            seenOpCodes.Add(opCode);

            ushort expected = unchecked((ushort)opCode.Value);
            Assert.That(valueField.GetRawConstantValue(), Is.EqualTo(expected),
                $"ILOpCode.{valueField.Name} does not match OpCodes.{reflectionEmitName}.Value");
        }

        foreach (var opCode in opCodes.Values.Where(o => o.OpCodeType != OpCodeType.Nternal))
        {
            Assert.That(seenOpCodes.Contains(opCode), Is.True, $"{opCode.Name} is missing an ILOpCode member");
        }
    }

    [Test]
    public void OpCodeDataFilled()
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.Static;

        foreach (FieldInfo opCodeField in typeof(OpCodes).GetFields(flags))
        {
            var opCode = (OpCode)opCodeField.GetValue(null)!;

            if (opCode.OpCodeType == OpCodeType.Nternal || opCode.FlowControl != FlowControl.Next)
                continue;

            var data = OpCodeData.Get(opCode);
            Assert.That(data.flags != 0, Is.True, $"{opCodeField.Name} has no OpCodeData");
            if (opCode.StackBehaviourPush == StackBehaviour.Push0)
                Assert.That(data.resultType, Is.EqualTo(typeof(void)), $"{opCodeField.Name} has bad resultType");
            else
                Assert.That(data.resultType != typeof(void), $"{opCodeField.Name} has bad resultType");
        }
    }
}
