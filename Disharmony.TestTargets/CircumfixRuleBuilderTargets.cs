namespace Disharmony.Tests;

public static class CircumfixRuleBuilderTargets
{
    public static int Target(int value) => value;
    public static int RefTarget(ref int value) => value;
    public static void VoidTarget() { }
    public static void PrefixLow() { }
    public static void PrefixHigh() { }
    public static void PostfixLow() { }
    public static void PostfixHigh() { }
    public static bool BooleanPrefix() => true;
    public static bool SecondBooleanPrefix() => false;
    public static void WriteArgument(ref int value) { }
    public static void ReadArgument(int value) { }
    public static void WriteResult(ref int result) { }
    public static void ReadResult(int result) { }
    public static void WriteState(ref int state) { }
    public static void ReadState(int state) { }
    public static void AlwaysPrefix() { }
    public static void AlwaysPostfix(ref Exception exception) { }
}
