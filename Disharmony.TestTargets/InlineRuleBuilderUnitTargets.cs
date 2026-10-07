namespace Disharmony.Tests;

public class InlineRuleBuilderUnitTargets
{
    public int Value;
    public int InstanceValue() => Value;
    public static void Empty() { }

    // Only signature/local metadata is used by the EmitReplacement unit tests, not these instruction bodies.
    public static byte IntToByte(int value) => 0;
    public static string StringIdentity(string value) => value;
    public static void MixedArguments(int number, long wide, string text) { }
    public static void RefInt(ref int value) { }
    public static int FiveIntArguments(int first, int second, int third, int fourth, int fifth) => 0;
    public static string BoolToString(bool condition) => "";
    public static int IntIdentity(int value) => value;

    public static int FiveIntLocals()
    {
        int a = 0, b = 0, c = 0, d = 0, e = 0;
        // Taking their addresses retains five distinct local declarations in the Release-built body.
        RefInt(ref a);
        RefInt(ref b);
        RefInt(ref c);
        RefInt(ref d);
        RefInt(ref e);
        return a;
    }

    public static void Catch()
    {
        try { Empty(); }
        catch (InvalidOperationException) { Empty(); }
    }

    public static void Finally()
    {
        try { Empty(); }
        finally { Empty(); }
    }
}

public struct InlineRuleBuilderStructTarget
{
    public int Value;
    public int InstanceValue() => Value;
}

public abstract class InlineRuleBuilderAbstractTarget
{
    public abstract int Method();
}
