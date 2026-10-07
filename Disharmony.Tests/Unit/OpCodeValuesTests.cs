namespace Disharmony.Tests.Unit;

[TestFixture]
public sealed class OpCodeValuesTests
{
    [Test]
    public void ConstantsMatchRuntimeOpCodes()
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.Static;

        Dictionary<string, OpCode> opCodes = [];
        HashSet<OpCode> seenOpCodes = [];

        foreach (FieldInfo opCodeField in typeof(OpCodes).GetFields(flags))
            opCodes[opCodeField.Name] = (OpCode)opCodeField.GetValue(null)!;

        foreach (FieldInfo valueField in typeof(OpCodeValues).GetFields(flags))
        {
            var opCode = opCodes[valueField.Name];
            seenOpCodes.Add(opCode);

            int expected = unchecked((ushort)opCode.Value);
            Assert.That(valueField.GetRawConstantValue(), Is.EqualTo(expected),
                $"OpCodeValues.{valueField.Name} does not match OpCodes.{valueField.Name}.Value");
        }

        foreach (var opCode in opCodes.Values.Where(o => o.OpCodeType != OpCodeType.Nternal))
        {
            Assert.That(seenOpCodes.Contains(opCode), Is.True, $"{opCode.Name} is missing an OpCodeValues constant");
        }
    }

    [Test]
    public void OpCodeDataFilled()
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.Static;

        Dictionary<string, OpCode> opCodes = [];

        foreach (FieldInfo opCodeField in typeof(OpCodes).GetFields(flags))
            opCodes[opCodeField.Name] = (OpCode)opCodeField.GetValue(null)!;

        foreach (FieldInfo valueField in typeof(OpCodeValues).GetFields(flags))
        {
            var opCode = opCodes[valueField.Name];

            var value = (int)valueField.GetRawConstantValue();

            var data = OpCodeData.Get((ushort)value);

            if (opCode.FlowControl != FlowControl.Next)
                continue;

            Assert.That(data.flags != 0, Is.True, $"{valueField.Name} has no OpCodeData");
            if (opCode.StackBehaviourPush == StackBehaviour.Push0)
                Assert.That(data.resultType, Is.EqualTo(typeof(void)), $"{valueField.Name} has bad resultType");
            else
                Assert.That(data.resultType != typeof(void), $"{valueField.Name} has bad resultType");
        }
    }
}
