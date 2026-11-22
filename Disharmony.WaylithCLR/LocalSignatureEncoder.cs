using System;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using WaylithClr;

namespace Disharmony.WaylithCLR;

internal static class LocalSignatureEncoder
{
    public static void Write(LocalVariableTypeEncoder encoder, LocalBuilder local, MethodBodyBuilder builder)
    {
        Type type = local.LocalType;
        if (type == typeof(TypedReference) && !local.IsPinned)
            encoder.TypedReference();
        else
            WriteType(encoder.Type(type.IsByRef, local.IsPinned), StripByRef(type), builder);
    }

    private static Type StripByRef(Type type) => type.IsByRef ? type.GetElementType()! : type;

    private static void WriteType(SignatureTypeEncoder encoder, Type type, MethodBodyBuilder builder)
    {
        if (type.IsSZArray)
        {
            WriteType(encoder.SZArray(), type.GetElementType()!, builder);
        }
        else if (type.IsArray)
        {
            encoder.Array(out var element, out var shape);
            WriteType(element, type.GetElementType()!, builder);
            shape.Shape(type.GetArrayRank(), [], []);
        }
        else if (type.IsPointer)
        {
            Type element = type.GetElementType()!;
            if (element == typeof(void))
                encoder.VoidPointer();
            else
                WriteType(encoder.Pointer(), element, builder);
        }
        else if (type.IsFunctionPointer)
        {
            WriteFunctionPointer(encoder, type, builder);
        }
        else if (type.IsGenericParameter)
        {
            if (type.DeclaringMethod is null)
                encoder.GenericTypeParameter(type.GenericParameterPosition);
            else
                encoder.GenericMethodTypeParameter(type.GenericParameterPosition);
        }
        else if (type.IsConstructedGenericType)
        {
            Type[] arguments = type.GetGenericArguments();
            var definition = MetadataTokens.EntityHandle(builder.GetTokenFor(type.GetGenericTypeDefinition()));
            var generic = encoder.GenericInstantiation(definition, arguments.Length, type.IsValueType);
            foreach (Type argument in arguments)
                WriteType(generic.AddArgument(), argument, builder);
        }
        else if (type == typeof(TypedReference))
        {
            encoder.TypedReference();
        }
        else if (GetPrimitive(type) is { } primitive)
        {
            encoder.PrimitiveType(primitive);
        }
        else
        {
            encoder.Type(MetadataTokens.EntityHandle(builder.GetTokenFor(type)), type.IsValueType);
        }
    }

    private static void WriteFunctionPointer(SignatureTypeEncoder encoder, Type type, MethodBodyBuilder builder)
    {
        Type returnType = type.GetFunctionPointerReturnType();
        Type[] parameters = type.GetFunctionPointerParameterTypes();
        Type[] conventions = type.GetFunctionPointerCallingConventions();
        var convention = type.IsUnmanagedFunctionPointer ? SignatureCallingConvention.Unmanaged : SignatureCallingConvention.Default;
        encoder.FunctionPointer(convention, FunctionPointerAttributes.None, 0)
            .Parameters(parameters.Length, out var result, out var arguments);
        foreach (Type modifier in conventions)
            result.CustomModifiers().AddModifier(MetadataTokens.EntityHandle(builder.GetTokenFor(modifier)), isOptional: true);
        if (returnType == typeof(void))
            result.Void();
        else if (returnType == typeof(TypedReference))
            result.TypedReference();
        else
            WriteType(result.Type(returnType.IsByRef), StripByRef(returnType), builder);
        foreach (Type parameter in parameters)
        {
            var argument = arguments.AddParameter();
            if (parameter == typeof(TypedReference))
                argument.TypedReference();
            else
                WriteType(argument.Type(parameter.IsByRef), StripByRef(parameter), builder);
        }
    }

    private static PrimitiveTypeCode? GetPrimitive(Type type)
    {
        if (type == typeof(object)) return PrimitiveTypeCode.Object;
        if (type == typeof(IntPtr)) return PrimitiveTypeCode.IntPtr;
        if (type == typeof(UIntPtr)) return PrimitiveTypeCode.UIntPtr;
        if (type.IsEnum) return null;
        return Type.GetTypeCode(type) switch
        {
            TypeCode.Boolean => PrimitiveTypeCode.Boolean,
            TypeCode.Char => PrimitiveTypeCode.Char,
            TypeCode.SByte => PrimitiveTypeCode.SByte,
            TypeCode.Byte => PrimitiveTypeCode.Byte,
            TypeCode.Int16 => PrimitiveTypeCode.Int16,
            TypeCode.UInt16 => PrimitiveTypeCode.UInt16,
            TypeCode.Int32 => PrimitiveTypeCode.Int32,
            TypeCode.UInt32 => PrimitiveTypeCode.UInt32,
            TypeCode.Int64 => PrimitiveTypeCode.Int64,
            TypeCode.UInt64 => PrimitiveTypeCode.UInt64,
            TypeCode.Single => PrimitiveTypeCode.Single,
            TypeCode.Double => PrimitiveTypeCode.Double,
            TypeCode.String => PrimitiveTypeCode.String,
            _ => null,
        };
    }
}
