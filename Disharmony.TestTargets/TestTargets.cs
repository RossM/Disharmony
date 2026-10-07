namespace Disharmony.Tests
{
    public sealed class BindingReference
    {
        public int Value;
    }

    public struct BindingStruct
    {
        public int Value;
    }

    public static class PatcherRegistrationInlinePatches
    {
        public static MethodBase? ObservedMethod;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Patch_PatchConfig_UsesInlineOption() => ObservedMethod = MethodBase.GetCurrentMethod();
    }

    public static class AlwaysRunInlinePatches
    {
        public static Exception? ObservedException;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ExceptionBinding_InlinePostfix_Suppresses(ref Exception? __exception)
        {
            ObservedException = __exception;
            __exception = null;
        }
    }

    public static class StaticMethodTargets
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static IEnumerable<string> EnumerateDescription(DerivedMethodTargets inner, int value)
        {
            yield return inner.DescribeNonVirtual(value);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static IEnumerable<int> EnumerateIdentity(int outerValue)
        {
            yield return InnerStaticMethodTargets.IntIdentity(outerValue);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static IEnumerable<int> EnumeratePair(int first, int second)
        {
            yield return InnerStaticMethodTargets.IntIdentity(first);
            yield return second;
        }

        public static List<string> PriorityEvents { get; } = [];

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void PriorityTarget() => PriorityEvents.Add("target");

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void PriorityOuter() => PriorityInner();

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void PriorityInner() => PriorityEvents.Add("inner-target");

        public static int MutableProperty
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            get;
            [MethodImpl(MethodImplOptions.NoInlining)]
            set;
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Void() { }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void IntArgument(int value) { }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void StringArgument(string value) { }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void StructArgument(BindingStruct value) { }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void RefIntArgument(ref int value) { }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void RefStringArgument(ref string value) { }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void RefStructArgument(ref BindingStruct value) { }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int IntIdentity(int value) => value;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string StringIdentity(string value) => value;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static BindingStruct StructIdentity(BindingStruct value) => value;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static object ObjectIdentity(object value) => value;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int IntResult() => 1;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string StringResult() => "original";
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static BindingStruct StructResult() => new BindingStruct { Value = 1 };
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int RegistrationResultA() => 1;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int RegistrationResultB() => 2;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void OverloadedVoid(int value) { }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void OverloadedVoid(string value) { }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static T GenericIdentity<T>(T value) => value;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowingIntArgument(int value) =>
            Assert.Fail("The target should have been skipped.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int ThrowingIntResult()
        {
            Assert.Fail("The target should have been skipped.");
            return 1;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowInvalidOperationException() =>
            throw new InvalidOperationException("target");

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int ThrowInvalidOperationExceptionWithResult() =>
            throw new InvalidOperationException("target-result");

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static IEnumerable<int> EnumerateThenThrow()
        {
            yield return 1;
            throw new InvalidOperationException("iterator");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string ThrowingStringResult()
        {
            Assert.Fail("The target should have been skipped.");
            return "original";
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static BindingStruct ThrowingStructResult()
        {
            Assert.Fail("The target should have been skipped.");
            return new BindingStruct { Value = 1 };
        }
    }

    public sealed class ClassMethodTargets
    {
        public int foo;
        public int primitiveField;
        public BindingReference referenceField = new BindingReference();
        public BindingStruct structField;
        public object objectField = new object();
        public int AutoProperty { get; set; }
        public int Value { get; private set; }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Void() { }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public ClassMethodTargets Self() => this;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public int IntIdentity(int value)
        {
            Value = value;
            return Value;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public int IntSum(int first, int second)
        {
            Value = first + second;
            return Value;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public int IntResult()
        {
            Value = 1;
            return Value;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void CallStaticVoid() => InnerStaticMethodTargets.Void();

        [MethodImpl(MethodImplOptions.NoInlining)]
        public int CallStaticVoidAndReturnValue()
        {
            InnerStaticMethodTargets.Void();
            return Value;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void CallInnerWithoutField(InstanceMethodTargetsWithoutFields inner) => inner.Void();
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void CallInnerWithField(InnerInstanceMethodTargets inner) => inner.Void();

        [MethodImpl(MethodImplOptions.NoInlining)]
        public IEnumerable<int> EnumerateIdentity(int outerValue)
        {
            _ = InnerStaticMethodTargets.IntIdentity(outerValue + foo);
            yield return outerValue;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public IEnumerable<BindingReference> EnumerateReferenceIdentity(BindingReference outerValue)
        {
            _ = InnerStaticMethodTargets.StringIdentity(outerValue.Value.ToString());
            yield return outerValue;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public IEnumerable<BindingStruct> EnumerateStructIdentity(BindingStruct outerValue)
        {
            _ = InnerStaticMethodTargets.StructIdentity(outerValue);
            yield return outerValue;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public IEnumerable<int> EnumerateDeclaringInstanceValue()
        {
            _ = InnerStaticMethodTargets.IntResult();
            yield return foo;
        }
    }

    public struct StructMethodTargets
    {
        public int foo;
        public int Value { get; private set; }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override string ToString() => $"StructMethodTargets:{foo}";

        [MethodImpl(MethodImplOptions.NoInlining)]
        public int IntIdentity(int value)
        {
            Value = value;
            return Value;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public int IntResult()
        {
            Value = 1;
            return Value;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void CallInnerWithoutField(InstanceMethodTargetsWithoutFields inner) => inner.Void();
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void CallInnerWithField(ref InnerStructMethodTargets inner) => inner.Void();
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void CallInnerWithFieldByValue(InnerStructMethodTargets inner) => inner.Void();

        [MethodImpl(MethodImplOptions.NoInlining)]
        public IEnumerable<int> EnumerateDeclaringInstanceValue()
        {
            _ = InnerStaticMethodTargets.IntResult();
            yield return foo;
        }
    }

    public sealed class InnerInstanceMethodTargets
    {
        public int foo;
        public int AutoProperty { get; set; }
        public int Property
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            get => foo;
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Void() { }
    }

    public struct InnerStructMethodTargets
    {
        public static int FieldObserved;
        public int foo;
        public int Property
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            get => foo;
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Void() => FieldObserved = foo;
    }

    public sealed class InstanceMethodTargetsWithoutFields
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Void() { }
    }

    public static class InnerStaticMethodTargets
    {
        public static int Property
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            get => 1;
        }
        public static int Field = 1;
        public static int MutableProperty
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            get;
            [MethodImpl(MethodImplOptions.NoInlining)]
            set;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Void() { }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void IntArgument(int value) { }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void StringArgument(string value) { }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void StructArgument(BindingStruct value) { }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void RefIntArgument(ref int value) { }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void RefStringArgument(ref string value) { }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void RefStructArgument(ref BindingStruct value) { }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int IntIdentity(int value) => value;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string StringIdentity(string value) => value;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static BindingStruct StructIdentity(BindingStruct value) => value;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int IntResult() => 1;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string StringResult() => "original";
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static BindingStruct StructResult() => new BindingStruct { Value = 1 };
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowInvalidOperationException() =>
            throw new InvalidOperationException("inner");
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int ThrowInvalidOperationExceptionWithResult() =>
            throw new InvalidOperationException("inner-result");
    }

    public static class ExceptionHandlingTargets
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void CallInTryBlock(bool throwException)
        {
            try
            {
                if (throwException)
                    throw new InvalidOperationException();

                InnerStaticMethodTargets.Void();
            }
            catch (InvalidOperationException) { }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void CallInCatchBlock(bool throwException)
        {
            try
            {
                if (throwException)
                    throw new InvalidOperationException();
            }
            catch (InvalidOperationException)
            {
                InnerStaticMethodTargets.Void();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void CallInFinallyBlock(bool throwException)
        {
            try
            {
                if (throwException)
                    throw new InvalidOperationException();
            }
            finally
            {
                InnerStaticMethodTargets.Void();
            }
        }
    }

    public static class ConstantTargets
    {
        public const int IntValue = 42;
        public const int IntReplacement = 43;
        public const long LongValue = 42000000000L;
        public const long LongReplacement = 43000000000L;
        public const float FloatValue = 4.25f;
        public const float FloatReplacement = 5.25f;
        public const double DoubleValue = 8.5;
        public const double DoubleReplacement = 9.5;
        public const string StringValue = "original";
        public const string StringReplacement = "patched";

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int IntResult() => IntValue;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int Int_SpecialEncoding_ValueMinus1_Result() => -1;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int Int_SpecialEncoding_Value0_Result() => 0;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int Int_SpecialEncoding_Value1_Result() => 1;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int Int_SpecialEncoding_Value2_Result() => 2;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int Int_SpecialEncoding_Value3_Result() => 3;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int Int_SpecialEncoding_Value4_Result() => 4;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int Int_SpecialEncoding_Value5_Result() => 5;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int Int_SpecialEncoding_Value6_Result() => 6;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int Int_SpecialEncoding_Value7_Result() => 7;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int Int_SpecialEncoding_Value8_Result() => 8;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int Int_SignedByteEncoding_ValueMinus128_Result() => -128;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int Int_Int32Encoding_ValueMinus129_Result() => -129;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int Int_SignedByteEncoding_Value127_Result() => 127;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int Int_Int32Encoding_Value128_Result() => 128;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static long LongResult() => LongValue;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static float FloatResult() => FloatValue;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static double DoubleResult() => DoubleValue;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string StringResult() => StringValue;
    }

    public sealed class ConstructorTargets
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public ConstructorTargets()
        {
            ConstructorExecuted = true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public ConstructorTargets(int value)
        {
            ConstructorExecuted = true;
            Value = value;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public ConstructorTargets(bool throwException)
        {
            if (throwException)
                throw new InvalidOperationException("constructor");
            ConstructorExecuted = true;
        }

        public bool ConstructorExecuted { get; }
        public int Value { get; }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ConstructorTargets Create() => new ConstructorTargets();
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ConstructorTargets Create(int value) => new ConstructorTargets(value);
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ConstructorTargets Create(bool throwException) => new ConstructorTargets(throwException);
    }

    public static class StaticConstructorObservation
    {
        public static bool ConstructorExecuted;
    }

    public static class StaticConstructorTargets
    {
        static StaticConstructorTargets()
        {
            StaticConstructorObservation.ConstructorExecuted = true;
            Value = 42;
        }

        public static int Value { get; }
    }

    public class BaseMethodTargets
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public string DescribeNonVirtual(int value) => $"base:{value}:{InstanceValue}";

        [MethodImpl(MethodImplOptions.NoInlining)]
        public virtual string CallInner(DerivedMethodTargets inner, int value) => $"outer-base:{value}:{InstanceValue}";

        [MethodImpl(MethodImplOptions.NoInlining)]
        public virtual IEnumerable<string> EnumerateDescription(int value)
        {
            yield return $"base:{value}:{InstanceValue}";
        }

        public int InstanceValue { get; set; }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public virtual string Describe(int value) => $"base:{value}:{InstanceValue}";

        [MethodImpl(MethodImplOptions.NoInlining)]
        public virtual string DescribeWithInnerCall(int value) => $"base:{value}:{InstanceValue}";
    }

    public sealed class DerivedMethodTargets : BaseMethodTargets
    {
        // The inner call must name this declaration in IL, rather than a base virtual slot.
        [MethodImpl(MethodImplOptions.NoInlining)]
        public new string DescribeNonVirtual(int value) => $"derived:{value}:{InstanceValue}";

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override string CallInner(DerivedMethodTargets inner, int value) => inner.DescribeNonVirtual(value);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override IEnumerable<string> EnumerateDescription(int value)
        {
            InnerStaticMethodTargets.Void();
            yield return $"derived:{value}:{InstanceValue}";
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override string Describe(int value) => $"derived:{value}:{InstanceValue}";

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override string DescribeWithInnerCall(int value)
        {
            InnerStaticMethodTargets.Void();
            return $"derived:{value}:{InstanceValue}";
        }
    }

    public static class OuterStaticMethodTargets
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int IntResult() => InnerStaticMethodTargets.IntResult();
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int CallThrowingVoid()
        {
            InnerStaticMethodTargets.ThrowInvalidOperationException();
            return 42;
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int CallThrowingIntResult() =>
            InnerStaticMethodTargets.ThrowInvalidOperationExceptionWithResult();
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void CallVoidThenThrow()
        {
            InnerStaticMethodTargets.Void();
            throw new InvalidOperationException("outer");
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static IEnumerable<int> EnumerateThrowingInnerResult()
        {
            yield return InnerStaticMethodTargets.ThrowInvalidOperationExceptionWithResult();
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string StringResult() => InnerStaticMethodTargets.StringResult();
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static BindingStruct StructResult() => InnerStaticMethodTargets.StructResult();
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int FieldResult() => InnerStaticMethodTargets.Field;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int PropertyResult() => InnerStaticMethodTargets.Property;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int ReadInstanceField(InnerInstanceMethodTargets inner) => inner.foo;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int ReadStructField(InnerStructMethodTargets inner) => inner.foo;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int ReadInstanceProperty(InnerInstanceMethodTargets inner) => inner.Property;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int ReadStructProperty(InnerStructMethodTargets inner) => inner.Property;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void SetStaticField(int value) => InnerStaticMethodTargets.Field = value;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int MutablePropertyResult() => InnerStaticMethodTargets.MutableProperty;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void SetStaticProperty(int value) => InnerStaticMethodTargets.MutableProperty = value;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void SetInstanceField(InnerInstanceMethodTargets inner, int value) => inner.foo = value;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void SetStructField(ref InnerStructMethodTargets inner, int value) => inner.foo = value;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void IntArgument(int value) => InnerStaticMethodTargets.IntArgument(value);
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void StringArgument(string value) => InnerStaticMethodTargets.StringArgument(value);
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void StructArgument(BindingStruct value) => InnerStaticMethodTargets.StructArgument(value);
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int IntIdentity(int value) => InnerStaticMethodTargets.IntIdentity(value);
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string StringIdentity(string value) => InnerStaticMethodTargets.StringIdentity(value);
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static BindingStruct StructIdentity(BindingStruct value) => InnerStaticMethodTargets.StructIdentity(value);
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void RefIntArgument(ref int value) => InnerStaticMethodTargets.RefIntArgument(ref value);
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void RefStringArgument(ref string value) => InnerStaticMethodTargets.RefStringArgument(ref value);
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void RefStructArgument(ref BindingStruct value) => InnerStaticMethodTargets.RefStructArgument(ref value);
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void OuterArgument(int outerValue) => InnerStaticMethodTargets.Void();
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void OuterReferenceTypeArgument(string outerValue) => InnerStaticMethodTargets.Void();
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void OuterStructArgument(BindingStruct outerValue) => InnerStaticMethodTargets.Void();
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void SameNamedArgument(int value) => InnerStaticMethodTargets.IntArgument(value + 41);
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void SameNamedReferenceTypeArgument(string value) => InnerStaticMethodTargets.StringArgument("inner");

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void SameNamedStructArgument(BindingStruct value) =>
            InnerStaticMethodTargets.StructArgument(new BindingStruct { Value = 42 });

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int SameNamedRefArgument(ref int value)
        {
            int innerValue = 1;
            InnerStaticMethodTargets.RefIntArgument(ref innerValue);
            return innerValue;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string SameNamedRefReferenceTypeArgument(ref string value)
        {
            string innerValue = "inner";
            InnerStaticMethodTargets.RefStringArgument(ref innerValue);
            return innerValue;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static BindingStruct SameNamedRefStructArgument(ref BindingStruct value)
        {
            var innerValue = new BindingStruct { Value = 1 };
            InnerStaticMethodTargets.RefStructArgument(ref innerValue);
            return innerValue;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static IEnumerable<int> EnumerateIntResult()
        {
            yield return InnerStaticMethodTargets.IntResult();
        }
    }

    public static class LocalFunctionTargets
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int InvokeAnonymousLambda(int value)
        {
            Func<int, int> lambda =
                [MethodImpl(MethodImplOptions.NoInlining)]
                (lambdaParameter) => lambdaParameter;
            return lambda(value);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int CapturedVariableMethod(int value)
        {
            int captured = value;
            _ = LocalMethod();
            return captured;

            [MethodImpl(MethodImplOptions.NoInlining)]
            int LocalMethod() => captured;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static BindingReference CapturedReferenceVariableMethod(BindingReference value)
        {
            BindingReference captured = value;
            _ = LocalMethod();
            return captured;

            [MethodImpl(MethodImplOptions.NoInlining)]
            BindingReference LocalMethod() => captured;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static BindingStruct CapturedStructVariableMethod(BindingStruct value)
        {
            BindingStruct captured = value;
            _ = LocalMethod();
            return captured;

            [MethodImpl(MethodImplOptions.NoInlining)]
            BindingStruct LocalMethod() => captured;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static object CapturedObjectVariableMethod(object value)
        {
            object captured = value;
            _ = LocalMethod();
            return captured;

            [MethodImpl(MethodImplOptions.NoInlining)]
            object LocalMethod() => captured;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static IEnumerable<int> PrimitiveLocalIterator(int enclosingValue)
        {
            return LocalIterator();

            [MethodImpl(MethodImplOptions.NoInlining)]
            IEnumerable<int> LocalIterator()
            {
                _ = InnerStaticMethodTargets.IntIdentity(enclosingValue);
                yield return enclosingValue;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static IEnumerable<BindingReference> ReferenceTypeLocalIterator(BindingReference enclosingValue)
        {
            return LocalIterator();

            [MethodImpl(MethodImplOptions.NoInlining)]
            IEnumerable<BindingReference> LocalIterator()
            {
                _ = InnerStaticMethodTargets.StringIdentity(enclosingValue.Value.ToString());
                yield return enclosingValue;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static IEnumerable<BindingStruct> StructLocalIterator(BindingStruct enclosingValue)
        {
            return LocalIterator();

            [MethodImpl(MethodImplOptions.NoInlining)]
            IEnumerable<BindingStruct> LocalIterator()
            {
                _ = InnerStaticMethodTargets.StructIdentity(enclosingValue);
                yield return enclosingValue;
            }
        }
    }
}

namespace Disharmony.Tests.ReflectionFixtures
{
    public static class LookupTarget
    {
        public static class NestedTarget
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void Method(int value) { }
        }

        public static int ReadOnlyProperty => 1;

        public static int Field;
        public static int Property { get; set; }

        public static int WriteOnlyProperty
        {
            set { }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Method(int value) { }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void RefMethod(ref int value) { }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void InMethod(in int value) { }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void OutMethod(out int value) => value = 0;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void OverloadedMethod(int value) { }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void OverloadedMethod(string value) { }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void GenericMethod<T>(T value) { }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void NonGenericMethod(int value) { }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void MixedMethod(int value) { }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void MixedMethod<T>(T value) { }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Func<int, int> StaticLocalMethodContainer()
        {
            return StaticLocalMethod;

            static int StaticLocalMethod(int value) => value;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Func<int> CapturedLocalMethodContainer(int value)
        {
            return CapturedLocalMethod;

            int CapturedLocalMethod() => value;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Func<int, int> LambdaContainer() => value => value;
    }
}
