namespace Disharmony.Optimizer;

internal static class TypeLattice
{
    /// <summary>
    ///     Represents a type that can be any type. This is used in the type lattice to represent the top of the lattice, where
    ///     any type is compatible with it.
    /// </summary>
    public static class AnyType;

    /// <summary>
    ///     Represents a type that has no known information. This is used in the type lattice to represent the bottom of the
    ///     lattice.
    /// </summary>
    public static class UnknownType;

    /// <summary>
    ///     Represents a null pointer. The CLI treats this as an IntPtr, but allows it to be compatible with any reference
    ///     type.
    /// </summary>
    public static class NullPtr;

    public static readonly Type Any = typeof(AnyType);
    public static readonly Type AnyRef = Any.MakeByRefType();
    public static readonly Type Unknown = typeof(UnknownType);
    public static readonly Type UnknownRef = Unknown.MakeByRefType();
    public static readonly Type Null = typeof(NullPtr);

    public static List<Type> GetBaseTypes(Type type)
    {
        if (type.IsInterface)
            return [typeof(object), type];

        List<Type> types = [];
        for (Type? t = type; t != null; t = t.BaseType)
            types.Add(t);
        types.Reverse();
        return types;
    }

    public static bool IsInteger(Type type)
    {
        var typeCode = Type.GetTypeCode(type);
        return typeCode is >= TypeCode.Boolean and <= TypeCode.UInt64;
    }

    private static bool IsObjectReference(Type type)
    {
        return type is { IsValueType: false, IsByRef: false, IsPointer: false };
    }

    private static Type GetReducedType(Type type)
    {
        if (type.IsEnum)
            type = type.GetEnumUnderlyingType();

        return Type.GetTypeCode(type) switch
        {
            TypeCode.Boolean or TypeCode.SByte or TypeCode.Byte => typeof(sbyte),
            TypeCode.Char or TypeCode.Int16 or TypeCode.UInt16 => typeof(short),
            TypeCode.Int32 or TypeCode.UInt32 => typeof(int),
            TypeCode.Int64 or TypeCode.UInt64 => typeof(long),
            _ when type == typeof(UIntPtr) => typeof(IntPtr),
            _ => type,
        };
    }

    private static bool IsVector(Type type)
    {
        return type.IsArray && type == type.GetElementType()!.MakeArrayType();
    }

    private static bool IsArrayElementAssignableTo(Type source, Type target)
    {
        if (GetReducedType(source) == GetReducedType(target))
            return true;

        return IsObjectReference(source) && IsObjectReference(target) && IsAssignableTo(source, target);
    }

    private static bool IsArrayAssignableTo(Type source, Type target)
    {
        if (source.IsArray && target.IsArray && source.GetArrayRank() == target.GetArrayRank())
            return IsArrayElementAssignableTo(source.GetElementType()!, target.GetElementType()!);

        if (!IsVector(source) || !target.IsGenericType ||
            target.GetGenericTypeDefinition() != typeof(IList<>))
            return false;

        return IsArrayElementAssignableTo(source.GetElementType()!, target.GetGenericArguments()[0]);
    }

    private static bool IsAssignableTo(Type source, Type target)
    {
        if (source == target)
            return true;

        // The stack-state rules treat I4 and native int as mutually assignable.
        if ((source == typeof(int) && target == typeof(IntPtr)) ||
            (source == typeof(IntPtr) && target == typeof(int)))
            return true;

        if (!IsObjectReference(source) || !IsObjectReference(target))
            return false;

        return target.IsAssignableFrom(source) || IsArrayAssignableTo(source, target);
    }

    private static Type MergeManagedPointers(Type left, Type right)
    {
        Type leftElement = GetReducedType(left.GetElementType()!);
        Type rightElement = GetReducedType(right.GetElementType()!);
        return leftElement == rightElement ? leftElement.MakeByRefType() : AnyRef;
    }

    /// <summary>
    ///     Calculates the result type of merging two stack slots of the given type.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The rules for merging stack slots are given in ECMA 335 III.1.8.2.3 and III.1.8.3. When
    ///         merging types which are not verifier-compatible, the result is the underlying CIL stack
    ///         slot type: <see langword="int" /> (I4), <see langword="long" /> (I8), <see cref="IntPtr" /> (I),
    ///         <see langword="double" /> (F), <see langword="object" /> (O), or <see cref="AnyRef" /> (&amp;).
    ///     </para>
    ///     <para>
    ///         We use a type lattice extended with Any and Unknown types. The result of a merge involving
    ///         Any is the best (most specific) available representation of the union of all possible results
    ///         from replacing Any with a concrete type. The result of a merge involving Unknown is the best
    ///         available representation of the intersection of all possible results from replacing Unknown
    ///         with a concrete type.
    ///     </para>
    ///     <para>
    ///         Numeric stack types that are inputs to this function should already have been normalized to
    ///         <see langword="int" /> (I4), <see langword="long" /> (I8), <see cref="IntPtr" /> (I), and
    ///         <see langword="double" /> (F).
    ///     </para>
    /// </remarks>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static Type Merge(Type left, Type right)
    {
        if (left == right)
            return left;

        if (left == Any || right == Any)
            return Any;

        if (left == Unknown)
            return right;
        if (right == Unknown)
            return left;

        if ((left == AnyRef && right.IsByRef) || (right == AnyRef && left.IsByRef))
            return AnyRef;

        if (left == UnknownRef && right.IsByRef)
            return right;
        if (right == UnknownRef && left.IsByRef)
            return left;

        if (left == Null && IsObjectReference(right))
            return right;
        if (right == Null && IsObjectReference(left))
            return left;

        // Null has stack category O when its special verifier compatibility does not apply.
        if (left == Null)
            left = typeof(object);
        if (right == Null)
            right = typeof(object);

        if (left.IsByRef && right.IsByRef)
            return MergeManagedPointers(left, right);

        // ECMA's merge is ordered: retain the existing (left) type when both
        // assignment directions are valid.
        if (IsAssignableTo(right, left))
            return left;
        if (IsAssignableTo(left, right))
            return right;

        if (!IsObjectReference(left) || !IsObjectReference(right))
            return Any;

        List<Type> leftBaseTypes = GetBaseTypes(left);
        List<Type> rightBaseTypes = GetBaseTypes(right);

        for (int i = Math.Min(leftBaseTypes.Count, rightBaseTypes.Count) - 1; i >= 0; i--)
        {
            if (leftBaseTypes[i] == rightBaseTypes[i])
                return leftBaseTypes[i];
        }

        return Any;
    }
}
