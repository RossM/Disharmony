namespace Disharmony.Tests.EndToEnd.RuleBuilders;

public static class InnerPostfixConstantPatches
{
    public static int intObserved;
    public static long longObserved;
    public static float floatObserved;
    public static double doubleObserved;
    public static string? stringObserved;

    [Postfix] [InnerConstant(ConstantTargets.IntValue)]
    [Target(typeof(ConstantTargets), nameof(ConstantTargets.IntResult))]
    public static void InnerPostfixConstant_Int_Result_ReadByValue(int __result) => intObserved = __result;

    [Postfix] [InnerConstant(ConstantTargets.IntValue)]
    [Target(typeof(ConstantTargets), nameof(ConstantTargets.IntResult))]
    public static void InnerPostfixConstant_Int_Result_ReadByReference(ref int __result) => intObserved = __result;

    [Postfix] [InnerConstant(ConstantTargets.IntValue)]
    [Target(typeof(ConstantTargets), nameof(ConstantTargets.IntResult))]
    public static void InnerPostfixConstant_Int_Result_WriteByReference(ref int __result) =>
        __result = ConstantTargets.IntReplacement;

    [Postfix] [InnerConstant(ConstantTargets.LongValue)]
    [Target(typeof(ConstantTargets), nameof(ConstantTargets.LongResult))]
    public static void InnerPostfixConstant_Long_Result_ReadByValue(long __result) => longObserved = __result;

    [Postfix] [InnerConstant(ConstantTargets.LongValue)]
    [Target(typeof(ConstantTargets), nameof(ConstantTargets.LongResult))]
    public static void InnerPostfixConstant_Long_Result_ReadByReference(ref long __result) => longObserved = __result;

    [Postfix] [InnerConstant(ConstantTargets.LongValue)]
    [Target(typeof(ConstantTargets), nameof(ConstantTargets.LongResult))]
    public static void InnerPostfixConstant_Long_Result_WriteByReference(ref long __result) =>
        __result = ConstantTargets.LongReplacement;

    [Postfix] [InnerConstant(ConstantTargets.FloatValue)]
    [Target(typeof(ConstantTargets), nameof(ConstantTargets.FloatResult))]
    public static void InnerPostfixConstant_Float_Result_ReadByValue(float __result) => floatObserved = __result;

    [Postfix] [InnerConstant(ConstantTargets.FloatValue)]
    [Target(typeof(ConstantTargets), nameof(ConstantTargets.FloatResult))]
    public static void InnerPostfixConstant_Float_Result_ReadByReference(ref float __result) => floatObserved = __result;

    [Postfix] [InnerConstant(ConstantTargets.FloatValue)]
    [Target(typeof(ConstantTargets), nameof(ConstantTargets.FloatResult))]
    public static void InnerPostfixConstant_Float_Result_WriteByReference(ref float __result) =>
        __result = ConstantTargets.FloatReplacement;

    [Postfix] [InnerConstant(ConstantTargets.DoubleValue)]
    [Target(typeof(ConstantTargets), nameof(ConstantTargets.DoubleResult))]
    public static void InnerPostfixConstant_Double_Result_ReadByValue(double __result) => doubleObserved = __result;

    [Postfix] [InnerConstant(ConstantTargets.DoubleValue)]
    [Target(typeof(ConstantTargets), nameof(ConstantTargets.DoubleResult))]
    public static void InnerPostfixConstant_Double_Result_ReadByReference(ref double __result) => doubleObserved = __result;

    [Postfix] [InnerConstant(ConstantTargets.DoubleValue)]
    [Target(typeof(ConstantTargets), nameof(ConstantTargets.DoubleResult))]
    public static void InnerPostfixConstant_Double_Result_WriteByReference(ref double __result) =>
        __result = ConstantTargets.DoubleReplacement;

    [Postfix] [InnerConstant(ConstantTargets.StringValue)]
    [Target(typeof(ConstantTargets), nameof(ConstantTargets.StringResult))]
    public static void InnerPostfixConstant_String_Result_ReadByValue(string __result) => stringObserved = __result;

    [Postfix] [InnerConstant(ConstantTargets.StringValue)]
    [Target(typeof(ConstantTargets), nameof(ConstantTargets.StringResult))]
    public static void InnerPostfixConstant_String_Result_ReadByReference(ref string __result) => stringObserved = __result;

    [Postfix] [InnerConstant(ConstantTargets.StringValue)]
    [Target(typeof(ConstantTargets), nameof(ConstantTargets.StringResult))]
    public static void InnerPostfixConstant_String_Result_WriteByReference(ref string __result) =>
        __result = ConstantTargets.StringReplacement;

    [Postfix] [InnerConstant(-1)]
    [Target(typeof(ConstantTargets), nameof(ConstantTargets.Int_SpecialEncoding_ValueMinus1_Result))]
    public static void InnerPostfixConstant_Int_SpecialEncoding_ValueMinus1_Result_ReadByValue(int __result) =>
        intObserved = __result;

    [Postfix] [InnerConstant(0)]
    [Target(typeof(ConstantTargets), nameof(ConstantTargets.Int_SpecialEncoding_Value0_Result))]
    public static void InnerPostfixConstant_Int_SpecialEncoding_Value0_Result_ReadByValue(int __result) => intObserved = __result;

    [Postfix] [InnerConstant(1)]
    [Target(typeof(ConstantTargets), nameof(ConstantTargets.Int_SpecialEncoding_Value1_Result))]
    public static void InnerPostfixConstant_Int_SpecialEncoding_Value1_Result_ReadByValue(int __result) => intObserved = __result;

    [Postfix] [InnerConstant(2)]
    [Target(typeof(ConstantTargets), nameof(ConstantTargets.Int_SpecialEncoding_Value2_Result))]
    public static void InnerPostfixConstant_Int_SpecialEncoding_Value2_Result_ReadByValue(int __result) => intObserved = __result;

    [Postfix] [InnerConstant(3)]
    [Target(typeof(ConstantTargets), nameof(ConstantTargets.Int_SpecialEncoding_Value3_Result))]
    public static void InnerPostfixConstant_Int_SpecialEncoding_Value3_Result_ReadByValue(int __result) => intObserved = __result;

    [Postfix] [InnerConstant(4)]
    [Target(typeof(ConstantTargets), nameof(ConstantTargets.Int_SpecialEncoding_Value4_Result))]
    public static void InnerPostfixConstant_Int_SpecialEncoding_Value4_Result_ReadByValue(int __result) => intObserved = __result;

    [Postfix] [InnerConstant(5)]
    [Target(typeof(ConstantTargets), nameof(ConstantTargets.Int_SpecialEncoding_Value5_Result))]
    public static void InnerPostfixConstant_Int_SpecialEncoding_Value5_Result_ReadByValue(int __result) => intObserved = __result;

    [Postfix] [InnerConstant(6)]
    [Target(typeof(ConstantTargets), nameof(ConstantTargets.Int_SpecialEncoding_Value6_Result))]
    public static void InnerPostfixConstant_Int_SpecialEncoding_Value6_Result_ReadByValue(int __result) => intObserved = __result;

    [Postfix] [InnerConstant(7)]
    [Target(typeof(ConstantTargets), nameof(ConstantTargets.Int_SpecialEncoding_Value7_Result))]
    public static void InnerPostfixConstant_Int_SpecialEncoding_Value7_Result_ReadByValue(int __result) => intObserved = __result;

    [Postfix] [InnerConstant(8)]
    [Target(typeof(ConstantTargets), nameof(ConstantTargets.Int_SpecialEncoding_Value8_Result))]
    public static void InnerPostfixConstant_Int_SpecialEncoding_Value8_Result_ReadByValue(int __result) => intObserved = __result;

    [Postfix] [InnerConstant(-128)]
    [Target(typeof(ConstantTargets), nameof(ConstantTargets.Int_SignedByteEncoding_ValueMinus128_Result))]
    public static void InnerPostfixConstant_Int_SignedByteEncoding_ValueMinus128_Result_ReadByValue(int __result) =>
        intObserved = __result;

    [Postfix] [InnerConstant(-129)]
    [Target(typeof(ConstantTargets), nameof(ConstantTargets.Int_Int32Encoding_ValueMinus129_Result))]
    public static void InnerPostfixConstant_Int_Int32Encoding_ValueMinus129_Result_ReadByValue(int __result) =>
        intObserved = __result;

    [Postfix] [InnerConstant(127)]
    [Target(typeof(ConstantTargets), nameof(ConstantTargets.Int_SignedByteEncoding_Value127_Result))]
    public static void InnerPostfixConstant_Int_SignedByteEncoding_Value127_Result_ReadByValue(int __result) =>
        intObserved = __result;

    [Postfix] [InnerConstant(128)]
    [Target(typeof(ConstantTargets), nameof(ConstantTargets.Int_Int32Encoding_Value128_Result))]
    public static void InnerPostfixConstant_Int_Int32Encoding_Value128_Result_ReadByValue(int __result) => intObserved = __result;
}

[TestFixture]
public sealed class InnerPostfixConstantTests : PatchTestBase
{
    [Test]
    public void InnerPostfixConstant_Int_Result_ReadByValue()
    {
        InnerPostfixConstantPatches.intObserved = 0;
        ApplyPatch(
            typeof(InnerPostfixConstantPatches),
            nameof(InnerPostfixConstantPatches.InnerPostfixConstant_Int_Result_ReadByValue));

        int result = ConstantTargets.IntResult();

        Assert.That(result, Is.EqualTo(ConstantTargets.IntValue));
        Assert.That(InnerPostfixConstantPatches.intObserved, Is.EqualTo(ConstantTargets.IntValue));
    }

    [Test]
    public void InnerPostfixConstant_Int_Result_ReadByReference()
    {
        InnerPostfixConstantPatches.intObserved = 0;
        ApplyPatch(
            typeof(InnerPostfixConstantPatches),
            nameof(InnerPostfixConstantPatches.InnerPostfixConstant_Int_Result_ReadByReference));

        int result = ConstantTargets.IntResult();

        Assert.That(result, Is.EqualTo(ConstantTargets.IntValue));
        Assert.That(InnerPostfixConstantPatches.intObserved, Is.EqualTo(ConstantTargets.IntValue));
    }

    [Test]
    public void InnerPostfixConstant_Int_Result_WriteByReference()
    {
        ApplyPatch(
            typeof(InnerPostfixConstantPatches),
            nameof(InnerPostfixConstantPatches.InnerPostfixConstant_Int_Result_WriteByReference));

        int result = ConstantTargets.IntResult();

        Assert.That(result, Is.EqualTo(ConstantTargets.IntReplacement));
    }

    [Test]
    public void InnerPostfixConstant_Long_Result_ReadByValue()
    {
        InnerPostfixConstantPatches.longObserved = 0;
        ApplyPatch(
            typeof(InnerPostfixConstantPatches),
            nameof(InnerPostfixConstantPatches.InnerPostfixConstant_Long_Result_ReadByValue));

        long result = ConstantTargets.LongResult();

        Assert.That(result, Is.EqualTo(ConstantTargets.LongValue));
        Assert.That(InnerPostfixConstantPatches.longObserved, Is.EqualTo(ConstantTargets.LongValue));
    }

    [Test]
    public void InnerPostfixConstant_Long_Result_ReadByReference()
    {
        InnerPostfixConstantPatches.longObserved = 0;
        ApplyPatch(
            typeof(InnerPostfixConstantPatches),
            nameof(InnerPostfixConstantPatches.InnerPostfixConstant_Long_Result_ReadByReference));

        long result = ConstantTargets.LongResult();

        Assert.That(result, Is.EqualTo(ConstantTargets.LongValue));
        Assert.That(InnerPostfixConstantPatches.longObserved, Is.EqualTo(ConstantTargets.LongValue));
    }

    [Test]
    public void InnerPostfixConstant_Long_Result_WriteByReference()
    {
        ApplyPatch(
            typeof(InnerPostfixConstantPatches),
            nameof(InnerPostfixConstantPatches.InnerPostfixConstant_Long_Result_WriteByReference));

        long result = ConstantTargets.LongResult();

        Assert.That(result, Is.EqualTo(ConstantTargets.LongReplacement));
    }

    [Test]
    public void InnerPostfixConstant_Float_Result_ReadByValue()
    {
        InnerPostfixConstantPatches.floatObserved = 0;
        ApplyPatch(
            typeof(InnerPostfixConstantPatches),
            nameof(InnerPostfixConstantPatches.InnerPostfixConstant_Float_Result_ReadByValue));

        float result = ConstantTargets.FloatResult();

        Assert.That(result, Is.EqualTo(ConstantTargets.FloatValue));
        Assert.That(InnerPostfixConstantPatches.floatObserved, Is.EqualTo(ConstantTargets.FloatValue));
    }

    [Test]
    public void InnerPostfixConstant_Float_Result_ReadByReference()
    {
        InnerPostfixConstantPatches.floatObserved = 0;
        ApplyPatch(
            typeof(InnerPostfixConstantPatches),
            nameof(InnerPostfixConstantPatches.InnerPostfixConstant_Float_Result_ReadByReference));

        float result = ConstantTargets.FloatResult();

        Assert.That(result, Is.EqualTo(ConstantTargets.FloatValue));
        Assert.That(InnerPostfixConstantPatches.floatObserved, Is.EqualTo(ConstantTargets.FloatValue));
    }

    [Test]
    public void InnerPostfixConstant_Float_Result_WriteByReference()
    {
        ApplyPatch(
            typeof(InnerPostfixConstantPatches),
            nameof(InnerPostfixConstantPatches.InnerPostfixConstant_Float_Result_WriteByReference));

        float result = ConstantTargets.FloatResult();

        Assert.That(result, Is.EqualTo(ConstantTargets.FloatReplacement));
    }

    [Test]
    public void InnerPostfixConstant_Double_Result_ReadByValue()
    {
        InnerPostfixConstantPatches.doubleObserved = 0;
        ApplyPatch(
            typeof(InnerPostfixConstantPatches),
            nameof(InnerPostfixConstantPatches.InnerPostfixConstant_Double_Result_ReadByValue));

        double result = ConstantTargets.DoubleResult();

        Assert.That(result, Is.EqualTo(ConstantTargets.DoubleValue));
        Assert.That(InnerPostfixConstantPatches.doubleObserved, Is.EqualTo(ConstantTargets.DoubleValue));
    }

    [Test]
    public void InnerPostfixConstant_Double_Result_ReadByReference()
    {
        InnerPostfixConstantPatches.doubleObserved = 0;
        ApplyPatch(
            typeof(InnerPostfixConstantPatches),
            nameof(InnerPostfixConstantPatches.InnerPostfixConstant_Double_Result_ReadByReference));

        double result = ConstantTargets.DoubleResult();

        Assert.That(result, Is.EqualTo(ConstantTargets.DoubleValue));
        Assert.That(InnerPostfixConstantPatches.doubleObserved, Is.EqualTo(ConstantTargets.DoubleValue));
    }

    [Test]
    public void InnerPostfixConstant_Double_Result_WriteByReference()
    {
        ApplyPatch(
            typeof(InnerPostfixConstantPatches),
            nameof(InnerPostfixConstantPatches.InnerPostfixConstant_Double_Result_WriteByReference));

        double result = ConstantTargets.DoubleResult();

        Assert.That(result, Is.EqualTo(ConstantTargets.DoubleReplacement));
    }

    [Test]
    public void InnerPostfixConstant_String_Result_ReadByValue()
    {
        InnerPostfixConstantPatches.stringObserved = null;
        ApplyPatch(
            typeof(InnerPostfixConstantPatches),
            nameof(InnerPostfixConstantPatches.InnerPostfixConstant_String_Result_ReadByValue));

        string result = ConstantTargets.StringResult();

        Assert.That(result, Is.EqualTo(ConstantTargets.StringValue));
        Assert.That(InnerPostfixConstantPatches.stringObserved, Is.EqualTo(ConstantTargets.StringValue));
    }

    [Test]
    public void InnerPostfixConstant_String_Result_ReadByReference()
    {
        InnerPostfixConstantPatches.stringObserved = null;
        ApplyPatch(
            typeof(InnerPostfixConstantPatches),
            nameof(InnerPostfixConstantPatches.InnerPostfixConstant_String_Result_ReadByReference));

        string result = ConstantTargets.StringResult();

        Assert.That(result, Is.EqualTo(ConstantTargets.StringValue));
        Assert.That(InnerPostfixConstantPatches.stringObserved, Is.EqualTo(ConstantTargets.StringValue));
    }

    [Test]
    public void InnerPostfixConstant_String_Result_WriteByReference()
    {
        ApplyPatch(
            typeof(InnerPostfixConstantPatches),
            nameof(InnerPostfixConstantPatches.InnerPostfixConstant_String_Result_WriteByReference));

        string result = ConstantTargets.StringResult();

        Assert.That(result, Is.EqualTo(ConstantTargets.StringReplacement));
    }

    [Test]
    public void InnerPostfixConstant_Int_SpecialEncoding_ValueMinus1_Result_ReadByValue()
    {
        InnerPostfixConstantPatches.intObserved = 0;
        ApplyPatch(
            typeof(InnerPostfixConstantPatches),
            nameof(InnerPostfixConstantPatches.InnerPostfixConstant_Int_SpecialEncoding_ValueMinus1_Result_ReadByValue));

        int result = ConstantTargets.Int_SpecialEncoding_ValueMinus1_Result();

        Assert.That(result, Is.EqualTo(-1));
        Assert.That(InnerPostfixConstantPatches.intObserved, Is.EqualTo(-1));
    }

    [Test]
    public void InnerPostfixConstant_Int_SpecialEncoding_Value0_Result_ReadByValue()
    {
        InnerPostfixConstantPatches.intObserved = -1;
        ApplyPatch(
            typeof(InnerPostfixConstantPatches),
            nameof(InnerPostfixConstantPatches.InnerPostfixConstant_Int_SpecialEncoding_Value0_Result_ReadByValue));

        int result = ConstantTargets.Int_SpecialEncoding_Value0_Result();

        Assert.That(result, Is.Zero);
        Assert.That(InnerPostfixConstantPatches.intObserved, Is.Zero);
    }

    [Test]
    public void InnerPostfixConstant_Int_SpecialEncoding_Value1_Result_ReadByValue()
    {
        InnerPostfixConstantPatches.intObserved = 0;
        ApplyPatch(
            typeof(InnerPostfixConstantPatches),
            nameof(InnerPostfixConstantPatches.InnerPostfixConstant_Int_SpecialEncoding_Value1_Result_ReadByValue));

        int result = ConstantTargets.Int_SpecialEncoding_Value1_Result();

        Assert.That(result, Is.EqualTo(1));
        Assert.That(InnerPostfixConstantPatches.intObserved, Is.EqualTo(1));
    }

    [Test]
    public void InnerPostfixConstant_Int_SpecialEncoding_Value2_Result_ReadByValue()
    {
        InnerPostfixConstantPatches.intObserved = 0;
        ApplyPatch(
            typeof(InnerPostfixConstantPatches),
            nameof(InnerPostfixConstantPatches.InnerPostfixConstant_Int_SpecialEncoding_Value2_Result_ReadByValue));

        int result = ConstantTargets.Int_SpecialEncoding_Value2_Result();

        Assert.That(result, Is.EqualTo(2));
        Assert.That(InnerPostfixConstantPatches.intObserved, Is.EqualTo(2));
    }

    [Test]
    public void InnerPostfixConstant_Int_SpecialEncoding_Value3_Result_ReadByValue()
    {
        InnerPostfixConstantPatches.intObserved = 0;
        ApplyPatch(
            typeof(InnerPostfixConstantPatches),
            nameof(InnerPostfixConstantPatches.InnerPostfixConstant_Int_SpecialEncoding_Value3_Result_ReadByValue));

        int result = ConstantTargets.Int_SpecialEncoding_Value3_Result();

        Assert.That(result, Is.EqualTo(3));
        Assert.That(InnerPostfixConstantPatches.intObserved, Is.EqualTo(3));
    }

    [Test]
    public void InnerPostfixConstant_Int_SpecialEncoding_Value4_Result_ReadByValue()
    {
        InnerPostfixConstantPatches.intObserved = 0;
        ApplyPatch(
            typeof(InnerPostfixConstantPatches),
            nameof(InnerPostfixConstantPatches.InnerPostfixConstant_Int_SpecialEncoding_Value4_Result_ReadByValue));

        int result = ConstantTargets.Int_SpecialEncoding_Value4_Result();

        Assert.That(result, Is.EqualTo(4));
        Assert.That(InnerPostfixConstantPatches.intObserved, Is.EqualTo(4));
    }

    [Test]
    public void InnerPostfixConstant_Int_SpecialEncoding_Value5_Result_ReadByValue()
    {
        InnerPostfixConstantPatches.intObserved = 0;
        ApplyPatch(
            typeof(InnerPostfixConstantPatches),
            nameof(InnerPostfixConstantPatches.InnerPostfixConstant_Int_SpecialEncoding_Value5_Result_ReadByValue));

        int result = ConstantTargets.Int_SpecialEncoding_Value5_Result();

        Assert.That(result, Is.EqualTo(5));
        Assert.That(InnerPostfixConstantPatches.intObserved, Is.EqualTo(5));
    }

    [Test]
    public void InnerPostfixConstant_Int_SpecialEncoding_Value6_Result_ReadByValue()
    {
        InnerPostfixConstantPatches.intObserved = 0;
        ApplyPatch(
            typeof(InnerPostfixConstantPatches),
            nameof(InnerPostfixConstantPatches.InnerPostfixConstant_Int_SpecialEncoding_Value6_Result_ReadByValue));

        int result = ConstantTargets.Int_SpecialEncoding_Value6_Result();

        Assert.That(result, Is.EqualTo(6));
        Assert.That(InnerPostfixConstantPatches.intObserved, Is.EqualTo(6));
    }

    [Test]
    public void InnerPostfixConstant_Int_SpecialEncoding_Value7_Result_ReadByValue()
    {
        InnerPostfixConstantPatches.intObserved = 0;
        ApplyPatch(
            typeof(InnerPostfixConstantPatches),
            nameof(InnerPostfixConstantPatches.InnerPostfixConstant_Int_SpecialEncoding_Value7_Result_ReadByValue));

        int result = ConstantTargets.Int_SpecialEncoding_Value7_Result();

        Assert.That(result, Is.EqualTo(7));
        Assert.That(InnerPostfixConstantPatches.intObserved, Is.EqualTo(7));
    }

    [Test]
    public void InnerPostfixConstant_Int_SpecialEncoding_Value8_Result_ReadByValue()
    {
        InnerPostfixConstantPatches.intObserved = 0;
        ApplyPatch(
            typeof(InnerPostfixConstantPatches),
            nameof(InnerPostfixConstantPatches.InnerPostfixConstant_Int_SpecialEncoding_Value8_Result_ReadByValue));

        int result = ConstantTargets.Int_SpecialEncoding_Value8_Result();

        Assert.That(result, Is.EqualTo(8));
        Assert.That(InnerPostfixConstantPatches.intObserved, Is.EqualTo(8));
    }

    [Test]
    public void InnerPostfixConstant_Int_SignedByteEncoding_ValueMinus128_Result_ReadByValue()
    {
        InnerPostfixConstantPatches.intObserved = 0;
        ApplyPatch(
            typeof(InnerPostfixConstantPatches),
            nameof(InnerPostfixConstantPatches.InnerPostfixConstant_Int_SignedByteEncoding_ValueMinus128_Result_ReadByValue));

        int result = ConstantTargets.Int_SignedByteEncoding_ValueMinus128_Result();

        Assert.That(result, Is.EqualTo(-128));
        Assert.That(InnerPostfixConstantPatches.intObserved, Is.EqualTo(-128));
    }

    [Test]
    public void InnerPostfixConstant_Int_Int32Encoding_ValueMinus129_Result_ReadByValue()
    {
        InnerPostfixConstantPatches.intObserved = 0;
        ApplyPatch(
            typeof(InnerPostfixConstantPatches),
            nameof(InnerPostfixConstantPatches.InnerPostfixConstant_Int_Int32Encoding_ValueMinus129_Result_ReadByValue));

        int result = ConstantTargets.Int_Int32Encoding_ValueMinus129_Result();

        Assert.That(result, Is.EqualTo(-129));
        Assert.That(InnerPostfixConstantPatches.intObserved, Is.EqualTo(-129));
    }

    [Test]
    public void InnerPostfixConstant_Int_SignedByteEncoding_Value127_Result_ReadByValue()
    {
        InnerPostfixConstantPatches.intObserved = 0;
        ApplyPatch(
            typeof(InnerPostfixConstantPatches),
            nameof(InnerPostfixConstantPatches.InnerPostfixConstant_Int_SignedByteEncoding_Value127_Result_ReadByValue));

        int result = ConstantTargets.Int_SignedByteEncoding_Value127_Result();

        Assert.That(result, Is.EqualTo(127));
        Assert.That(InnerPostfixConstantPatches.intObserved, Is.EqualTo(127));
    }

    [Test]
    public void InnerPostfixConstant_Int_Int32Encoding_Value128_Result_ReadByValue()
    {
        InnerPostfixConstantPatches.intObserved = 0;
        ApplyPatch(
            typeof(InnerPostfixConstantPatches),
            nameof(InnerPostfixConstantPatches.InnerPostfixConstant_Int_Int32Encoding_Value128_Result_ReadByValue));

        int result = ConstantTargets.Int_Int32Encoding_Value128_Result();

        Assert.That(result, Is.EqualTo(128));
        Assert.That(InnerPostfixConstantPatches.intObserved, Is.EqualTo(128));
    }
}
