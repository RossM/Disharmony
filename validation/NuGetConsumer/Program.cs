using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Disharmony;

public sealed class Calculator
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public int Price(int quantity) { return 10 * quantity; }
}

public sealed class Checkout
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public int Total(Calculator calculator, int quantity) { return calculator.Price(quantity) + 2; }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public int Estimate(Calculator calculator, int quantity) { return calculator.Price(quantity) + 2; }
}

public static class OrdinaryTarget
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int Value() { return 1; }
}

[Patch]
public static class ConsumerPatches
{
    [Postfix, Target(typeof(Checkout), nameof(Checkout.Total))]
    [Inner(typeof(Calculator), nameof(Calculator.Price), typeof(int))]
    public static void Discount([ReturnValue] ref int price) { price = price * 9 / 10; }

    [Postfix, Target(typeof(OrdinaryTarget), nameof(OrdinaryTarget.Value))]
    public static void ChangeValue([ReturnValue] ref int result) { result = 2; }
}

public static class Program
{
    public static int Main(string[] args)
    {
        var harmonyVersion = typeof(HarmonyLib.Harmony).Assembly.GetName().Version;
        var disharmonyVersion = typeof(Patcher).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        Console.WriteLine("Disharmony " + disharmonyVersion + "; Harmony " + harmonyVersion);
        if (args.Length != 2 || disharmonyVersion.Split('+')[0] != args[0] || harmonyVersion.Major != int.Parse(args[1]))
            throw new Exception("The consumer loaded an unexpected Disharmony version or Harmony major.");

        var calculator = new Calculator();
        var checkout = new Checkout();
        if (checkout.Total(calculator, 2) != 22 || checkout.Estimate(calculator, 2) != 22 || OrdinaryTarget.Value() != 1)
            throw new Exception("Original results were incorrect.");

        PatchHandle handle = Patcher.PatchAll(typeof(ConsumerPatches));
        try
        {
            // Exercise lazy patch generation by calling the targets before ForceApply.
            int total = checkout.Total(calculator, 2);
            int estimate = checkout.Estimate(calculator, 2);
            int direct = calculator.Price(2);
            int whole = OrdinaryTarget.Value();
            Console.WriteLine("Patched: total=" + total + ", estimate=" + estimate + ", direct=" + direct + ", whole=" + whole);
            if (total != 20 || estimate != 22 || direct != 20 || whole != 2)
                throw new Exception("Patches changed incorrect results.");
            Patcher.ForceApply();
        }
        finally
        {
            Patcher.Unpatch(handle);
        }

        if (checkout.Total(calculator, 2) != 22 || checkout.Estimate(calculator, 2) != 22 || calculator.Price(2) != 20 || OrdinaryTarget.Value() != 1)
            throw new Exception("Unpatch did not restore the original results.");
        Console.WriteLine("PASS: inner and whole-method patches, unchanged callers, unpatch.");
        return 0;
    }
}
