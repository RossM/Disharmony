using NUnitLite;

namespace Disharmony.Tests;

internal static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "--require-waylith-clr")
        {
            Assembly coreLib = typeof(object).Assembly;
            if (!TestRunSetup.IsWaylithClr)
            {
                Console.Error.WriteLine($"Expected WaylithCLR. Loaded CoreLib from {coreLib.Location}.");
                return 1;
            }

            Console.WriteLine($"WaylithCLR: {coreLib.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion} ({coreLib.Location})");
            args = args.Skip(1).ToArray();
        }

        return new AutoRun().Execute(args);
    }
}
