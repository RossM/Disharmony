namespace Disharmony.Tests;

public class InfixRuleBuilderTargets
{
    public static int Outer(int outerValue) => outerValue;

    public static void InnerVoid() { }

    public static int InnerInt(int value) => value;

    public static string Combine(int number, string text) => $"{number}:{text}";

    public static int Increment(ref int value) => ++value;

    public virtual int InstanceInner(int value) => value;

    public static void PrefixLow() { }

    public static void PrefixHigh() { }

    public static void PostfixLow() { }

    public static void PostfixHigh() { }

    public static bool BooleanPrefix() => true;

    public static void InnerArgumentsPrefix(int number, ref string text) { }

    public static void ReadIntPrefix(int value) { }

    public static void ReadOuterPrefix(int outerValue) { }

    public static void ReadInstancePrefix(InfixRuleBuilderTargets instance) { }

    public static void ResultPostfix(ref int result) { }

    public static void AlwaysPrefix() { }

    public static void AlwaysPostfix(Exception exception) { }
}
