using System.IO;

namespace Disharmony.Tests.Unit.Optimizer;

[TestFixture]
internal sealed class TypeLatticeMergeTests
{
    internal sealed record MergeCase(string Name, Type Left, Type Right, Type Expected);

    private enum IntEnum
    {
        // ReSharper disable once UnusedMember.Local
        Value,
    }

    private static readonly Type ObjectType = typeof(OpcodeUtilitiesClass);
    private static readonly Type ObjectRef = ObjectType.MakeByRefType();
    private static readonly Type ValueType = typeof(OpcodeUtilitiesStruct);
    private static readonly Type ValueRef = ValueType.MakeByRefType();
    private static readonly Type Unknown = TypeLattice.Unknown;
    private static readonly Type UnknownRef = TypeLattice.UnknownRef;
    private static readonly Type Any = TypeLattice.Any;
    private static readonly Type AnyRef = TypeLattice.AnyRef;
    private static readonly Type Null = TypeLattice.Null;

    // Merge's inputs are already CIL evaluation-stack types. Bare numeric slots must therefore be int (I4), long
    // (I8), IntPtr (I), or double (F); normalization of uint, float, enums, and smaller integers belongs upstream.
    // Those storage/signature types can still legitimately occur inside managed pointers, arrays, or value types.
    // Expected results first apply the ECMA verifier merge rules when they provide a more specific type. If verifier
    // compatibility cannot merge the inputs, Merge falls back to correctness-level stack categories: O becomes Object,
    // & becomes AnyRef, and incompatible correctness categories become Any. Correct but unverifiable CIL is therefore
    // still represented as precisely as the evaluation-stack guarantees allow.
    private static readonly MergeCase[] Cases =
    [
        // Complete ordered cross-product of the concrete object/value categories and the four lattice categories.
        // Any and AnyRef represent the best common upper bound, while Unknown and UnknownRef represent a lack of
        // information constrained to all types or managed-reference types respectively. Null is a special O reference:
        // apply its specific compatibility rules first, then treat it as Object for any remaining merge rules.
        new("Object_Object", ObjectType, ObjectType, ObjectType),
        new("Object_ObjectRef", ObjectType, ObjectRef, Any),
        new("Object_Value", ObjectType, ValueType, Any),
        new("Object_ValueRef", ObjectType, ValueRef, Any),
        new("Object_Unknown", ObjectType, Unknown, ObjectType),
        new("Object_UnknownRef", ObjectType, UnknownRef, Any),
        new("Object_Any", ObjectType, Any, Any),
        new("Object_AnyRef", ObjectType, AnyRef, Any),
        new("Object_Null", ObjectType, Null, ObjectType),

        new("ObjectRef_Object", ObjectRef, ObjectType, Any),
        new("ObjectRef_ObjectRef", ObjectRef, ObjectRef, ObjectRef),
        new("ObjectRef_Value", ObjectRef, ValueType, Any),
        new("ObjectRef_ValueRef", ObjectRef, ValueRef, AnyRef),
        new("ObjectRef_Unknown", ObjectRef, Unknown, ObjectRef),
        new("ObjectRef_UnknownRef", ObjectRef, UnknownRef, ObjectRef),
        new("ObjectRef_Any", ObjectRef, Any, Any),
        new("ObjectRef_AnyRef", ObjectRef, AnyRef, AnyRef),
        new("ObjectRef_Null", ObjectRef, Null, Any),

        new("Value_Object", ValueType, ObjectType, Any),
        new("Value_ObjectRef", ValueType, ObjectRef, Any),
        new("Value_Value", ValueType, ValueType, ValueType),
        new("Value_ValueRef", ValueType, ValueRef, Any),
        new("Value_Unknown", ValueType, Unknown, ValueType),
        new("Value_UnknownRef", ValueType, UnknownRef, Any),
        new("Value_Any", ValueType, Any, Any),
        new("Value_AnyRef", ValueType, AnyRef, Any),
        new("Value_Null", ValueType, Null, Any),

        new("ValueRef_Object", ValueRef, ObjectType, Any),
        new("ValueRef_ObjectRef", ValueRef, ObjectRef, AnyRef),
        new("ValueRef_Value", ValueRef, ValueType, Any),
        new("ValueRef_ValueRef", ValueRef, ValueRef, ValueRef),
        new("ValueRef_Unknown", ValueRef, Unknown, ValueRef),
        new("ValueRef_UnknownRef", ValueRef, UnknownRef, ValueRef),
        new("ValueRef_Any", ValueRef, Any, Any),
        new("ValueRef_AnyRef", ValueRef, AnyRef, AnyRef),
        new("ValueRef_Null", ValueRef, Null, Any),

        new("Unknown_Object", Unknown, ObjectType, ObjectType),
        new("Unknown_ObjectRef", Unknown, ObjectRef, ObjectRef),
        new("Unknown_Value", Unknown, ValueType, ValueType),
        new("Unknown_ValueRef", Unknown, ValueRef, ValueRef),
        new("Unknown_Unknown", Unknown, Unknown, Unknown),
        new("Unknown_UnknownRef", Unknown, UnknownRef, UnknownRef),
        new("Unknown_Any", Unknown, Any, Any),
        new("Unknown_AnyRef", Unknown, AnyRef, AnyRef),
        new("Unknown_Null", Unknown, Null, Null),

        new("UnknownRef_Object", UnknownRef, ObjectType, Any),
        new("UnknownRef_ObjectRef", UnknownRef, ObjectRef, ObjectRef),
        new("UnknownRef_Value", UnknownRef, ValueType, Any),
        new("UnknownRef_ValueRef", UnknownRef, ValueRef, ValueRef),
        new("UnknownRef_Unknown", UnknownRef, Unknown, UnknownRef),
        new("UnknownRef_UnknownRef", UnknownRef, UnknownRef, UnknownRef),
        new("UnknownRef_Any", UnknownRef, Any, Any),
        new("UnknownRef_AnyRef", UnknownRef, AnyRef, AnyRef),
        new("UnknownRef_Null", UnknownRef, Null, Any),

        new("Any_Object", Any, ObjectType, Any),
        new("Any_ObjectRef", Any, ObjectRef, Any),
        new("Any_Value", Any, ValueType, Any),
        new("Any_ValueRef", Any, ValueRef, Any),
        new("Any_Unknown", Any, Unknown, Any),
        new("Any_UnknownRef", Any, UnknownRef, Any),
        new("Any_Any", Any, Any, Any),
        new("Any_AnyRef", Any, AnyRef, Any),
        new("Any_Null", Any, Null, Any),

        new("AnyRef_Object", AnyRef, ObjectType, Any),
        new("AnyRef_ObjectRef", AnyRef, ObjectRef, AnyRef),
        new("AnyRef_Value", AnyRef, ValueType, Any),
        new("AnyRef_ValueRef", AnyRef, ValueRef, AnyRef),
        new("AnyRef_Unknown", AnyRef, Unknown, AnyRef),
        new("AnyRef_UnknownRef", AnyRef, UnknownRef, AnyRef),
        new("AnyRef_Any", AnyRef, Any, Any),
        new("AnyRef_AnyRef", AnyRef, AnyRef, AnyRef),
        new("AnyRef_Null", AnyRef, Null, Any),

        new("Null_Object", Null, ObjectType, ObjectType),
        new("Null_ObjectRef", Null, ObjectRef, Any),
        new("Null_Value", Null, ValueType, Any),
        new("Null_ValueRef", Null, ValueRef, Any),
        new("Null_Unknown", Null, Unknown, Null),
        new("Null_UnknownRef", Null, UnknownRef, Any),
        new("Null_Any", Null, Any, Any),
        new("Null_AnyRef", Null, AnyRef, Any),
        new("Null_Null", Null, Null, Null),

        // ECMA III.1.8.1.3 first preserves either input when the other is verifier-assignable to it. These cases cover
        // direct and transitive base classes, a shared closest base class, and unrelated object types.
        new("ClassDerived_ClassBase", typeof(MemoryStream), typeof(Stream), typeof(Stream)),
        new("ClassBase_ClassDerived", typeof(Stream), typeof(MemoryStream), typeof(Stream)),
        new("ClassSibling_ClassSibling", typeof(MemoryStream), typeof(BufferedStream), typeof(Stream)),
        new("ClassUnrelated_ClassUnrelated", typeof(MemoryStream), typeof(Exception), typeof(object)),

        // Interface implementation and derivation participate in verifier assignment compatibility. Interfaces have
        // System.Object as their direct base class, so otherwise-incompatible interface/object types merge to Object.
        new("Class_InterfaceImplemented", typeof(List<string>), typeof(IEnumerable<string>),
            typeof(IEnumerable<string>)),
        new("InterfaceImplemented_Class", typeof(IEnumerable<string>), typeof(List<string>),
            typeof(IEnumerable<string>)),
        new("Class_InterfaceImplementedTransitively", typeof(List<string>), typeof(IEnumerable<object>),
            typeof(IEnumerable<object>)),
        new("InterfaceDerived_InterfaceBase", typeof(IList<string>), typeof(ICollection<string>),
            typeof(ICollection<string>)),
        new("InterfaceBase_InterfaceDerived", typeof(ICollection<string>), typeof(IList<string>),
            typeof(ICollection<string>)),
        new("InterfaceUnrelated_InterfaceUnrelated", typeof(IDisposable), typeof(ICloneable), typeof(object)),
        new("InterfaceUnrelatedReversed_InterfaceUnrelated", typeof(ICloneable), typeof(IDisposable), typeof(object)),
        new("Class_InterfaceUnrelated", typeof(MemoryStream), typeof(ICloneable), typeof(object)),
        new("InterfaceUnrelated_Class", typeof(ICloneable), typeof(MemoryStream), typeof(object)),

        // Array compatibility includes covariance, equal reduced integral element types, matching rank, and the
        // special vector-to-IList<T> rule. When covariance does not apply, arrays still share System.Array.
        new("ArrayDerived_ArrayBase", typeof(string[]), typeof(object[]), typeof(object[])),
        new("ArrayBase_ArrayDerived", typeof(object[]), typeof(string[]), typeof(object[])),
        new("ArrayInt_ArrayUInt", typeof(int[]), typeof(uint[]), typeof(int[])),
        new("ArrayUInt_ArrayInt", typeof(uint[]), typeof(int[]), typeof(uint[])),
        new("ArrayInt_ArrayLong", typeof(int[]), typeof(long[]), typeof(Array)),
        new("ArrayVector_ArrayRectangular", typeof(string[]), typeof(string[,]), typeof(Array)),
        new("ArrayRectangularDerived_ArrayRectangularBase", typeof(string[,]), typeof(object[,]), typeof(object[,])),
        new("ArrayVector_IListCompatible", typeof(string[]), typeof(IList<object>), typeof(IList<object>)),
        new("IListCompatible_ArrayVector", typeof(IList<object>), typeof(string[]), typeof(IList<object>)),

        // Generic interface and delegate variance is part of signature compatibility. Invariant interface
        // instantiations are incompatible with one another but still share the direct base class System.Object.
        new("CovariantInterfaceDerived_CovariantInterfaceBase", typeof(IEnumerable<string>),
            typeof(IEnumerable<object>), typeof(IEnumerable<object>)),
        new("CovariantInterfaceBase_CovariantInterfaceDerived", typeof(IEnumerable<object>),
            typeof(IEnumerable<string>), typeof(IEnumerable<object>)),
        new("ContravariantInterfaceBase_ContravariantInterfaceDerived", typeof(IComparer<object>),
            typeof(IComparer<string>), typeof(IComparer<string>)),
        new("ContravariantInterfaceDerived_ContravariantInterfaceBase", typeof(IComparer<string>),
            typeof(IComparer<object>), typeof(IComparer<string>)),
        new("InvariantInterfaceString_InvariantInterfaceObject", typeof(IList<string>), typeof(IList<object>),
            typeof(object)),
        new("InvariantInterfaceObject_InvariantInterfaceString", typeof(IList<object>), typeof(IList<string>),
            typeof(object)),
        new("CovariantDelegateDerived_CovariantDelegateBase", typeof(Func<string>), typeof(Func<object>),
            typeof(Func<object>)),
        new("ContravariantDelegateBase_ContravariantDelegateDerived", typeof(Action<object>), typeof(Action<string>),
            typeof(Action<string>)),

        // Stack-state assignment compatibility treats int32 and native int as mutually assignable. Because both
        // directions are valid, ECMA's ordered merge rule preserves the previously stored (left) stack type.
        new("Int_Int", typeof(int), typeof(int), typeof(int)),
        new("Long_Long", typeof(long), typeof(long), typeof(long)),
        new("IntPtr_IntPtr", typeof(IntPtr), typeof(IntPtr), typeof(IntPtr)),
        new("Double_Double", typeof(double), typeof(double), typeof(double)),
        new("Int_IntPtr", typeof(int), typeof(IntPtr), typeof(int)),
        new("IntPtr_Int", typeof(IntPtr), typeof(int), typeof(IntPtr)),
        new("Int_Long", typeof(int), typeof(long), Any),
        new("Long_Double", typeof(long), typeof(double), Any),

        // Managed-pointer element compatibility uses verification types, but it does not apply class inheritance.
        // When that verifier rule cannot merge the element types, the correctness-level & category still guarantees
        // that the merged value is a managed reference, represented by AnyRef.
        new("IntRef_UIntRef", typeof(int).MakeByRefType(), typeof(uint).MakeByRefType(),
            typeof(int).MakeByRefType()),
        new("UIntRef_IntRef", typeof(uint).MakeByRefType(), typeof(int).MakeByRefType(),
            typeof(int).MakeByRefType()),
        new("ClassDerivedRef_ClassBaseRef", typeof(MemoryStream).MakeByRefType(), typeof(Stream).MakeByRefType(),
            AnyRef),

        // System.Type assignability includes boxing conversions that do not apply to unboxed values on the CIL
        // evaluation stack. In particular, Nullable<T> and ordinary value types are reflection-assignable to Object,
        // ValueType, and their implemented interfaces, but their unboxed verification types cannot merge with those
        // object/interface types. Although ECMA distinguishes the exact verification type of boxed values (and boxes
        // Nullable<T> as boxed T), the optimizer currently represents every boxed value simply as Object. It therefore
        // cannot track that distinction, and every Nullable<T> below represents the unboxed value type.
        new("NullableInt_Object", typeof(int?), typeof(object), Any),
        new("Object_NullableInt", typeof(object), typeof(int?), Any),
        new("NullableInt_ValueType", typeof(int?), typeof(ValueType), Any),
        new("ValueType_NullableInt", typeof(ValueType), typeof(int?), Any),
        new("NullableInt_Int", typeof(int?), typeof(int), Any),
        new("Int_NullableInt", typeof(int), typeof(int?), Any),
        new("NullableInt_NullableLong", typeof(int?), typeof(long?), Any),
        new("NullableIntRef_IntRef", typeof(int?).MakeByRefType(), typeof(int).MakeByRefType(), AnyRef),
        new("IntRef_NullableIntRef", typeof(int).MakeByRefType(), typeof(int?).MakeByRefType(), AnyRef),
        new("Int_IComparable", typeof(int), typeof(IComparable), Any),
        new("IComparable_Int", typeof(IComparable), typeof(int), Any),
        new("DateTime_IComparable", typeof(DateTime), typeof(IComparable), Any),
        new("IComparable_DateTime", typeof(IComparable), typeof(DateTime), Any),
        new("Int_Object", typeof(int), typeof(object), Any),
        new("Object_Int", typeof(object), typeof(int), Any),
        new("Int_ValueType", typeof(int), typeof(ValueType), Any),
        new("ValueType_Int", typeof(ValueType), typeof(int), Any),

        // Bare enum values have already become I4 stack slots before Merge is called. Arrays retain their signature
        // type while using underlying/reduced element types; managed pointers expose the normalized element type.
        new("IntEnumRef_IntRef", typeof(IntEnum).MakeByRefType(), typeof(int).MakeByRefType(),
            typeof(int).MakeByRefType()),
        new("IntRef_IntEnumRef", typeof(int).MakeByRefType(), typeof(IntEnum).MakeByRefType(),
            typeof(int).MakeByRefType()),
        new("ArrayIntEnum_ArrayInt", typeof(IntEnum[]), typeof(int[]), typeof(IntEnum[])),
        new("ArrayInt_ArrayIntEnum", typeof(int[]), typeof(IntEnum[]), typeof(int[])),
        new("ArrayInt_IListUInt", typeof(int[]), typeof(IList<uint>), typeof(IList<uint>)),
        new("IListUInt_ArrayInt", typeof(IList<uint>), typeof(int[]), typeof(IList<uint>)),
        new("ArrayRectangularInt_ArrayRectangularUInt", typeof(int[,]), typeof(uint[,]), typeof(int[,])),
        new("ArrayRectangularUInt_ArrayRectangularInt", typeof(uint[,]), typeof(int[,]), typeof(uint[,])),

        // The special null verification type is assignable to object, interface, delegate, and array reference types.
        // For value types, native integers, and managed pointers, falling back to Object produces the same result as
        // merging Object with the other operand (usually Any), rather than making Null an unrelated type of its own.
        new("Null_Class", Null, typeof(string), typeof(string)),
        new("Class_Null", typeof(string), Null, typeof(string)),
        new("Null_Interface", Null, typeof(IEnumerable<string>), typeof(IEnumerable<string>)),
        new("Interface_Null", typeof(IEnumerable<string>), Null, typeof(IEnumerable<string>)),
        new("Null_Array", Null, typeof(string[]), typeof(string[])),
        new("Array_Null", typeof(string[]), Null, typeof(string[])),
        new("Null_Delegate", Null, typeof(Action), typeof(Action)),
        new("Delegate_Null", typeof(Action), Null, typeof(Action)),
        new("Null_Int", Null, typeof(int), Any),
        new("Int_Null", typeof(int), Null, Any),
        new("Null_NullableInt", Null, typeof(int?), Any),
        new("NullableInt_Null", typeof(int?), Null, Any),
        new("Null_IntPtr", Null, typeof(IntPtr), Any),
        new("IntPtr_Null", typeof(IntPtr), Null, Any),
        new("Null_IntRef", Null, typeof(int).MakeByRefType(), Any),
        new("IntRef_Null", typeof(int).MakeByRefType(), Null, Any),
    ];

    private static IEnumerable<TestCaseData> MergeCases()
    {
        foreach (MergeCase testCase in Cases)
            yield return new TestCaseData(testCase).SetName(testCase.Name);
    }

    [TestCaseSource(nameof(MergeCases))]
    public void ReturnsTheMergedStackType(MergeCase testCase)
    {
        Assert.That(TypeLattice.Merge(testCase.Left, testCase.Right), Is.EqualTo(testCase.Expected));
    }
}
