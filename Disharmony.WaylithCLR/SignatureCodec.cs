using System;
using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using WaylithClr;

namespace Disharmony.WaylithCLR;

// SignatureDecoder reads local-variable and call-site signature blobs through this provider.
// Each decoded type remains encoded as a signature fragment, preserving modifiers and array
// shapes during token remapping. TypeSignature.Resolve obtains the Type needed by ILGenerator.
internal sealed class SignatureCodec(Func<int, int>? importType = null) : ISignatureTypeProvider<SignatureCodec.TypeSignature, object?>
{
    internal sealed record TypeSignature(byte[] Bytes, bool Pinned = false)
    {
        public Type Resolve(MethodBase method)
        {
            if (Bytes.Length == 1 && Bytes[0] == (byte)SignatureTypeCode.Void)
                return typeof(void);
            int token = MethodBodyUpdater.GetTokenForTypeSpec(method.Module, Bytes);
            return (Type)MethodBodyUpdater.ResolveToken(method, token)!;
        }

        public void WriteTo(BlobBuilder blob)
        {
            if (Pinned)
                blob.WriteByte((byte)SignatureTypeCode.Pinned);
            blob.WriteBytes(Bytes);
        }
    }

    public static unsafe TypeSignature[] ReadLocals(byte[] bytes, Func<int, int>? importType = null)
    {
        if (bytes.Length == 0)
            return [];
        fixed (byte* start = bytes)
        {
            var reader = new BlobReader(start, bytes.Length);
            if (reader.ReadSignatureHeader().Kind != SignatureKind.LocalVariables)
                throw new BadImageFormatException("Expected a local variable signature.");
            int count = reader.ReadCompressedInteger();
            if (count < 0 || count > reader.RemainingBytes)
                throw new BadImageFormatException("Invalid local variable count.");
            var decoder = new SignatureDecoder<TypeSignature, object?>(new SignatureCodec(importType), null!, null);
            var locals = new TypeSignature[count];
            for (int i = 0; i < count; i++)
                locals[i] = decoder.DecodeType(ref reader);
            if (reader.RemainingBytes != 0)
                throw new BadImageFormatException("Trailing data in local variable signature.");
            return locals;
        }
    }

    public static unsafe MethodSignature<TypeSignature> ReadMethod(byte[] bytes, Func<int, int>? importType = null)
    {
        fixed (byte* start = bytes)
        {
            var reader = new BlobReader(start, bytes.Length);
            var decoder = new SignatureDecoder<TypeSignature, object?>(new SignatureCodec(importType), null!, null);
            var signature = decoder.DecodeMethodSignature(ref reader);
            if (reader.RemainingBytes != 0)
                throw new BadImageFormatException("Trailing data in call-site signature.");
            return signature;
        }
    }

    public static byte[] WriteMethod(MethodSignature<TypeSignature> signature)
    {
        var blob = new BlobBuilder();
        blob.WriteByte(signature.Header.RawValue);
        if (signature.Header.IsGeneric)
            blob.WriteCompressedInteger(signature.GenericParameterCount);
        blob.WriteCompressedInteger(signature.ParameterTypes.Length);
        signature.ReturnType.WriteTo(blob);
        for (int i = 0; i < signature.ParameterTypes.Length; i++)
        {
            if (i == signature.RequiredParameterCount)
                blob.WriteByte((byte)SignatureTypeCode.Sentinel);
            signature.ParameterTypes[i].WriteTo(blob);
        }
        return blob.ToArray();
    }

    private static TypeSignature Prefix(SignatureTypeCode code, TypeSignature element)
    {
        var blob = new BlobBuilder();
        blob.WriteByte((byte)code);
        blob.WriteBytes(element.Bytes);
        return new TypeSignature(blob.ToArray(), element.Pinned);
    }

    private TypeSignature NamedType(EntityHandle handle, byte kind)
    {
        int token = MetadataTokens.GetToken(handle);
        if (importType is not null)
            token = importType(token);
        var blob = new BlobBuilder();
        // SignatureDecoder passes kind == 0 for custom-modifier type handles. The class tag
        // is a placeholder that GetModifiedType discards when it writes the modifier's token.
        blob.WriteByte(kind == 0 ? (byte)SignatureTypeKind.Class : kind);
        blob.WriteCompressedInteger(CodedIndex.TypeDefOrRefOrSpec(MetadataTokens.EntityHandle(token)));
        return new TypeSignature(blob.ToArray());
    }

    public TypeSignature GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind) =>
        NamedType(handle, rawTypeKind);

    public TypeSignature GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind) =>
        NamedType(handle, rawTypeKind);

    public TypeSignature GetTypeFromSpecification(MetadataReader reader, object? context, TypeSpecificationHandle handle, byte rawTypeKind) =>
        NamedType(handle, rawTypeKind);

    public TypeSignature GetPrimitiveType(PrimitiveTypeCode code) => new([(byte)code]);
    public TypeSignature GetByReferenceType(TypeSignature element) => Prefix(SignatureTypeCode.ByReference, element);
    public TypeSignature GetPointerType(TypeSignature element) => Prefix(SignatureTypeCode.Pointer, element);
    public TypeSignature GetSZArrayType(TypeSignature element) => Prefix(SignatureTypeCode.SZArray, element);
    public TypeSignature GetPinnedType(TypeSignature element) => element with { Pinned = true };

    public TypeSignature GetArrayType(TypeSignature element, ArrayShape shape)
    {
        var blob = new BlobBuilder();
        blob.WriteByte((byte)SignatureTypeCode.Array);
        blob.WriteBytes(element.Bytes);
        blob.WriteCompressedInteger(shape.Rank);
        blob.WriteCompressedInteger(shape.Sizes.Length);
        foreach (int size in shape.Sizes)
            blob.WriteCompressedInteger(size);
        blob.WriteCompressedInteger(shape.LowerBounds.Length);
        foreach (int lowerBound in shape.LowerBounds)
            blob.WriteCompressedSignedInteger(lowerBound);
        return new TypeSignature(blob.ToArray());
    }

    public TypeSignature GetGenericInstantiation(TypeSignature genericType, ImmutableArray<TypeSignature> arguments)
    {
        var blob = new BlobBuilder();
        blob.WriteByte((byte)SignatureTypeCode.GenericTypeInstance);
        blob.WriteBytes(genericType.Bytes);
        blob.WriteCompressedInteger(arguments.Length);
        foreach (var argument in arguments)
            argument.WriteTo(blob);
        return new TypeSignature(blob.ToArray());
    }

    public TypeSignature GetGenericMethodParameter(object? context, int index) => GenericParameter(SignatureTypeCode.GenericMethodParameter, index);
    public TypeSignature GetGenericTypeParameter(object? context, int index) => GenericParameter(SignatureTypeCode.GenericTypeParameter, index);

    private static TypeSignature GenericParameter(SignatureTypeCode code, int index)
    {
        var blob = new BlobBuilder();
        blob.WriteByte((byte)code);
        blob.WriteCompressedInteger(index);
        return new TypeSignature(blob.ToArray());
    }

    public TypeSignature GetModifiedType(TypeSignature modifier, TypeSignature type, bool isRequired)
    {
        var blob = new BlobBuilder();
        blob.WriteByte((byte)(isRequired ? SignatureTypeCode.RequiredModifier : SignatureTypeCode.OptionalModifier));
        blob.WriteBytes(modifier.Bytes, 1, modifier.Bytes.Length - 1);
        blob.WriteBytes(type.Bytes);
        return new TypeSignature(blob.ToArray(), type.Pinned);
    }

    public TypeSignature GetFunctionPointerType(MethodSignature<TypeSignature> signature)
    {
        return Prefix(SignatureTypeCode.FunctionPointer, new TypeSignature(WriteMethod(signature)));
    }
}
