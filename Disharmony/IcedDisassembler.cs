#if ENABLE_DISASSEMBLY
using System.Diagnostics.CodeAnalysis;

namespace Disharmony;

/// <summary>
///     Reflection adapter for the optional Iced.dll dependency. Disharmony deliberately has no assembly reference
///     to Iced, so a missing or incompatible copy cannot prevent the main library from loading.
/// </summary>
internal sealed class IcedDisassembler
{
    private const string IcedAssemblyName = "Iced";
    private const int MaximumInstructionLength = 15;

    private readonly MethodInfo createDecoder;
    private readonly object decoderOptionsNone;
    private readonly MethodInfo decode;
    private readonly PropertyInfo instructionIP;
    private readonly PropertyInfo instructionLength;
    private readonly object formatter;
    private readonly MethodInfo format;
    private readonly object formatterOutput;
    private readonly MethodInfo clearFormatterOutput;

    private IcedDisassembler(Assembly assembly)
    {
        Type decoderType = RequireType(assembly, "Iced.Intel.Decoder");
        Type decoderOptionsType = RequireType(assembly, "Iced.Intel.DecoderOptions");
        Type instructionType = RequireType(assembly, "Iced.Intel.Instruction");
        Type formatterType = RequireType(assembly, "Iced.Intel.FastFormatter");
        Type formatterOutputType = RequireType(assembly, "Iced.Intel.FastStringOutput");

        createDecoder = decoderType.GetMethod(
                            "Create",
                            BindingFlags.Public | BindingFlags.Static,
                            null,
                            [typeof(int), typeof(byte[]), typeof(ulong), decoderOptionsType],
                            null)
                        ?? throw new MissingMethodException(decoderType.FullName, "Create");
        decoderOptionsNone = Enum.ToObject(decoderOptionsType, 0);
        decode = decoderType.GetMethod("Decode", BindingFlags.Public | BindingFlags.Instance, null, [], null)
                 ?? throw new MissingMethodException(decoderType.FullName, "Decode");

        instructionIP = instructionType.GetProperty("IP")
                        ?? throw new MissingMemberException(instructionType.FullName, "IP");
        instructionLength = instructionType.GetProperty("Length")
                            ?? throw new MissingMemberException(instructionType.FullName, "Length");

        formatter = Activator.CreateInstance(formatterType)
                    ?? throw new InvalidOperationException($"Could not create {formatterType.FullName}");
        formatterOutput = Activator.CreateInstance(formatterOutputType)
                          ?? throw new InvalidOperationException($"Could not create {formatterOutputType.FullName}");
        format = formatterType.GetMethod(
                     "Format",
                     BindingFlags.Public | BindingFlags.Instance,
                     null,
                     [instructionType.MakeByRefType(), formatterOutputType],
                     null)
                 ?? throw new MissingMethodException(formatterType.FullName, "Format");
        clearFormatterOutput = formatterOutputType.GetMethod(
                                   "Clear",
                                   BindingFlags.Public | BindingFlags.Instance,
                                   null,
                                   [],
                                   null)
                               ?? throw new MissingMethodException(formatterOutputType.FullName, "Clear");

        ConfigureFormatter(formatterType);
    }

    public static bool TryCreate([NotNullWhen(true)] out IcedDisassembler? disassembler, out string error)
    {
        try
        {
            Assembly? assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(candidate => candidate.GetName().Name == IcedAssemblyName);
            assembly ??= Assembly.Load(IcedAssemblyName);

            disassembler = new IcedDisassembler(assembly);
            error = "";
            return true;
        }
        catch (Exception exception)
        {
            disassembler = null;
            error = $"{exception.GetType().Name}: {exception.Message}";
            return false;
        }
    }

    public List<string> Disassemble(IntPtr codeStart, byte[] code)
    {
        int bitness = GetX86Bitness();

        ulong startIP = unchecked((ulong)codeStart.ToInt64());
        object decoder = createDecoder.Invoke(null, [bitness, code, startIP, decoderOptionsNone])
                         ?? throw new InvalidOperationException("Iced did not create a decoder");

        int addressWidth = bitness / 4;
        var result = new List<string>();
        int offset = 0;
        while (offset < code.Length)
        {
            object instruction = decode.Invoke(decoder, null)
                                 ?? throw new InvalidOperationException("Iced did not decode an instruction");
            int length = (int)(instructionLength.GetValue(instruction)
                               ?? throw new InvalidOperationException("Iced returned no instruction length"));
            if (length <= 0 || length > MaximumInstructionLength || length > code.Length - offset)
                throw new InvalidOperationException($"Iced returned an invalid instruction length of {length}");

            ulong instructionAddress = (ulong)(instructionIP.GetValue(instruction)
                                               ?? throw new InvalidOperationException("Iced returned no instruction address"));
            clearFormatterOutput.Invoke(formatterOutput, null);
            format.Invoke(formatter, [instruction, formatterOutput]);

            string bytes = BitConverter.ToString(code, offset, length).Replace('-', ' ');
            string assembly = formatterOutput.ToString();
            result.Add(
                $"{instructionAddress.ToString($"X{addressWidth}")}  {bytes,-(MaximumInstructionLength * 3 - 1)}  {assembly}");

            offset += length;
        }

        return result;
    }

    private static Type RequireType(Assembly assembly, string name) =>
        assembly.GetType(name, false)
        ?? throw new TypeLoadException($"{name} was not found in {assembly.FullName}");

    private static int GetX86Bitness()
    {
        Type? runtimeInformation = typeof(object).Assembly.GetType("System.Runtime.InteropServices.RuntimeInformation");
        object? architecture = runtimeInformation?.GetProperty("ProcessArchitecture")?.GetValue(null);
        return architecture?.ToString() switch
        {
            "X86" => 32,
            "X64" => 64,
            null => IntPtr.Size * 8,
            _ => throw new PlatformNotSupportedException($"Iced cannot decode {architecture} machine code"),
        };
    }

    private void ConfigureFormatter(Type formatterType)
    {
        object? options = formatterType.GetProperty("Options")?.GetValue(formatter);
        if (options == null)
            return;

        Type optionsType = options.GetType();
        optionsType.GetProperty("UseHexPrefix")?.SetValue(options, true);
        optionsType.GetProperty("UppercaseHex")?.SetValue(options, false);
        optionsType.GetProperty("SpaceAfterOperandSeparator")?.SetValue(options, true);
    }
}
#endif
