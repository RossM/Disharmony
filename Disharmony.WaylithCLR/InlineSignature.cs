using System;
using System.Reflection;
using System.Reflection.Metadata;
using WaylithClr;

namespace Disharmony.WaylithCLR;

internal sealed class InlineSignature(MethodBase method, byte[] bytes) : IInlineSignature
{
    private readonly MethodSignature<SignatureCodec.TypeSignature> signature = new SignatureCodec().ReadMethod(bytes);

    public int ParameterCount => signature.ParameterTypes.Length;
    public bool HasImplicitThis => signature.Header is { IsInstance: true, HasExplicitThis: false };
    public Type ReturnType => signature.ReturnType.Resolve(method);

    public int GetToken(MethodBodyBuilder builder) => builder.GetTokenForSignature(bytes);
}
