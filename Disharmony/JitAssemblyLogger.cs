#if ENABLE_DISASSEMBLY
namespace Disharmony;

internal static class JitAssemblyLogger
{
    private static readonly object logLock = new();

    public static void TryLog(MethodBase original, MethodInfo replacement)
    {
        try
        {
            if (!MonoJitCode.TrySnapshot(replacement, out IntPtr codeStart, out byte[] code, out string snapshotError))
            {
                LogUnavailable(original, snapshotError);
                return;
            }

            if (!IcedDisassembler.TryCreate(out IcedDisassembler? disassembler, out string disassemblerError))
            {
                LogUnavailable(original, $"the optional Iced disassembler is unavailable ({disassemblerError})");
                return;
            }

            List<string> instructions = disassembler.Disassemble(codeStart, code);

            lock (logLock)
            {
                FileLog.LogBuffered($"### Mono JIT assembly for {original.FullName}");
                FileLog.LogBuffered($"### Replacement: {replacement.FullName}");
                FileLog.LogBuffered(
                    $"### Native range: 0x{codeStart.ToInt64():X} - 0x{codeStart.ToInt64() + code.Length:X} ({code.Length} bytes)");
                foreach (string instruction in instructions)
                    FileLog.LogBuffered(instruction);
                FileLog.LogBuffered("");
                FileLog.FlushBuffer();
            }
        }
        catch (Exception exception)
        {
            LogUnavailable(original, $"{exception.GetType().Name}: {exception.Message}");
        }
    }

    private static void LogUnavailable(MethodBase original, string reason)
    {
        try
        {
            lock (logLock)
            {
                FileLog.LogBuffered($"### Mono JIT assembly unavailable for {original.FullName}: {reason}");
                FileLog.LogBuffered("");
                FileLog.FlushBuffer();
            }
        }
        catch
        {
            // Diagnostics must never interfere with patch application.
        }
    }
}
#endif
