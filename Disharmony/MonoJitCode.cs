#if ENABLE_DISASSEMBLY
using System.Runtime.InteropServices;

namespace Disharmony;

internal static class MonoJitCode
{
    private const string MonoLibrary = "mono-2.0-bdwgc";
    private const int MaximumCodeSize = 16 * 1024 * 1024;

    [DllImport(MonoLibrary, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr mono_domain_get();

    [DllImport(MonoLibrary, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr mono_jit_info_table_find(IntPtr domain, IntPtr address);

    [DllImport(MonoLibrary, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr mono_jit_info_get_code_start(IntPtr jitInfo);

    [DllImport(MonoLibrary, CallingConvention = CallingConvention.Cdecl)]
    private static extern int mono_jit_info_get_code_size(IntPtr jitInfo);

    public static bool TrySnapshot(
        MethodInfo replacement,
        out IntPtr codeStart,
        out byte[] code,
        out string error)
    {
        codeStart = IntPtr.Zero;
        code = [];
        error = "";

        if (typeof(object).Assembly.GetType("Mono.Runtime") == null)
        {
            error = "the current runtime is not Mono";
            return false;
        }

        try
        {
            IntPtr address = replacement.MethodHandle.GetFunctionPointer();
            IntPtr domain = mono_domain_get();
            if (domain == IntPtr.Zero)
            {
                error = "Mono returned no current application domain";
                return false;
            }

            IntPtr jitInfo = mono_jit_info_table_find(domain, address);
            if (jitInfo == IntPtr.Zero)
            {
                error = $"Mono has no JIT record containing 0x{address.ToInt64():X}";
                return false;
            }

            codeStart = mono_jit_info_get_code_start(jitInfo);
            int codeSize = mono_jit_info_get_code_size(jitInfo);
            if (codeStart == IntPtr.Zero || codeSize <= 0 || codeSize > MaximumCodeSize)
            {
                error = $"Mono returned an invalid JIT range (start 0x{codeStart.ToInt64():X}, size {codeSize})";
                codeStart = IntPtr.Zero;
                return false;
            }

            code = new byte[codeSize];
            Marshal.Copy(codeStart, code, 0, codeSize);
            return true;
        }
        catch (Exception exception)
        {
            error = $"{exception.GetType().Name}: {exception.Message}";
            codeStart = IntPtr.Zero;
            code = [];
            return false;
        }
    }
}
#endif
