using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using WaylithClr;

namespace Disharmony.WaylithCLR;

internal sealed class InlineSignature(MethodBase method, byte[] bytes) : IInlineSignature
{
    private readonly MethodSignature<SignatureCodec.TypeSignature> signature = new SignatureCodec().ReadMethod(bytes);

    public int ParameterCount => signature.ParameterTypes.Length;
    public bool HasImplicitThis => signature.Header.IsInstance && !signature.Header.HasExplicitThis;
    public Type ReturnType => signature.ReturnType.Resolve(method);

    public int GetToken(MethodBodyBuilder builder) => builder.GetTokenForSignature(bytes);

    public static InlineSignature FromHelper(MethodBase method, MethodBodyBuilder builder, SignatureHelper helper)
    {
        byte[] bytes = helper.GetSignature();
        if (SignatureModule(helper) is { } source && source != method.Module)
        {
            int ImportType(int token) => builder.GetTokenFor(source.ResolveType(token));
            bytes = new SignatureCodec(ImportType).CopyMethod(bytes);
        }
        return new InlineSignature(method, bytes);
    }

    // SignatureHelper exposes its bytes publicly, but resolving embedded tokens also
    // requires the ModuleBuilder that assigned them.
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "m_module")]
    private static extern ref ModuleBuilder? SignatureModule(SignatureHelper helper);
}
