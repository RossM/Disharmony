namespace Disharmony.Tests;

public sealed class OptimizerNullPropagationNode
{
    public OptimizerNullPropagationNode? Next;
    public int Value;
}
public sealed class OptimizerDataObject
{
    public int Number;
    public string Text { get; set; } = "";
}

public struct OptimizerDataStruct
{
    public int Number;
    public string Text { get; set; }
}

public interface IOptimizerDataReader
{
    int Read();
}

public sealed class OptimizerDataReader : IOptimizerDataReader
{
    private readonly int value;

    public OptimizerDataReader(int value) => this.value = value;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public int Read() => value;
}

public sealed class OptimizerAlternateDataReader : IOptimizerDataReader
{
    private readonly int value;

    public OptimizerAlternateDataReader(int value) => this.value = value;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public int Read() => value;
}

public abstract class OptimizerBranchValue
{
    protected OptimizerBranchValue(int number) => Number = number;

    public int Number { get; }
}

public sealed class OptimizerFirstBranchValue : OptimizerBranchValue
{
    public OptimizerFirstBranchValue(int number) : base(number) { }
}

public sealed class OptimizerSecondBranchValue : OptimizerBranchValue
{
    public OptimizerSecondBranchValue(int number) : base(number) { }
}

public sealed class OptimizerInstanceDataTargets
{
    public int Number;
    public string Text { get; private set; } = "";

    [MethodImpl(MethodImplOptions.NoInlining)]
    public (int Number, string Text) SetMembers(int number, string text)
    {
        Number = number;
        Text = text;
        return (Number, Text);
    }
}
public static class OptimizerDataTargets
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static (int Sum, long Product, double Quotient, byte Narrowed)
        PrimitiveArithmeticAndNumericConversions(int left, int right) =>
        (left + right, (long)left * right, (double)left / right, unchecked((byte)left));

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int CheckedNumericConversion(long value) =>
        checked((int)value);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int[] Arrays(int first, int second)
    {
        var result = new int[2];
        result[0] = second;
        result[1] = first;
        return result;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static OptimizerDataObject Objects(int number, string text)
    {
        var result = new OptimizerDataObject
        {
            Number = number,
            Text = text,
        };

        return result;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static (OptimizerDataStruct Original, OptimizerDataStruct Copy)
        StructCopyAndMutation(int number, string text)
    {
        var original = new OptimizerDataStruct
        {
            Number = number,
            Text = text,
        };
        OptimizerDataStruct copy = original;
        copy.Number = 42;
        copy.Text = "copy";
        return (original, copy);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static (object Boxed, int Unboxed) BoxingAndUnboxing(int value)
    {
        object boxed = value;
        int unboxed = (int)boxed;
        return (boxed, unboxed);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static (bool HasValue, int Value) NullableValueOperations(int? value)
    {
        int? copy = value;
        return (copy.HasValue, copy.GetValueOrDefault());
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static (int Primitive, string Reference, OptimizerDataStruct Structure)
        GenericMethodCalls(int primitive, string reference, OptimizerDataStruct structure) =>
        (Identity(primitive), Identity(reference), Identity(structure));

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static T Identity<T>(T value) => value;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int CapturingLambda(int value, int offset)
    {
        Func<int, int> addOffset = input => input + offset;
        return addOffset(value);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static string StringInterpolation(string label, int value) =>
        $"{label}: {value:D4}";

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static (string Text, int Number) TupleConstructionAndDeconstruction(int number, string text)
    {
        var tuple = (Number: number, Text: text);
        (int extractedNumber, string extractedText) = tuple;
        return (extractedText, extractedNumber);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int InterfaceDispatch(int value)
    {
        IOptimizerDataReader reader = new OptimizerDataReader(value);
        return reader.Read();
    }
}

public static class OptimizerMixedTargets
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static OptimizerBranchValue ConditionalReferenceType(bool selectFirst)
    {
        OptimizerBranchValue result = selectFirst
            ? new OptimizerFirstBranchValue(7)
            : new OptimizerSecondBranchValue(11);
        return result;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int ConditionalInterfaceImplementation(bool selectFirst)
    {
        IOptimizerDataReader reader = selectFirst
            ? new OptimizerDataReader(7)
            : new OptimizerAlternateDataReader(11);
        return reader.Read();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static object ConditionalBoxing(bool selectNumber)
    {
        object result = selectNumber
            ? 42
            : "text";
        return result;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static OptimizerDataStruct ConditionalStructCopy(bool selectFirst)
    {
        var first = new OptimizerDataStruct
        {
            Number = 7,
            Text = "first",
        };
        var second = new OptimizerDataStruct
        {
            Number = 11,
            Text = "second",
        };
        OptimizerDataStruct result = selectFirst ? first : second;
        result.Number++;
        result.Text = result.Text.ToUpperInvariant();
        return result;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static double SwitchWithNumericConversions(int mode, int value)
    {
        double result = mode switch
        {
            0 => value,
            1 => (long)value * 2,
            2 => (float)value / 2,
            _ => (double)value / 4,
        };
        return result;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static OptimizerDataObject LoopOverArray(int[] values)
    {
        var result = new OptimizerDataObject();
        for (int index = 0; index < values.Length; index++)
        {
            result.Number += values[index];
            result.Text = $"{index}: {values[index]}";
        }

        return result;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static OptimizerDataObject TryCatchWithReferenceLocal(string value)
    {
        OptimizerDataObject result;
        try
        {
            result = new OptimizerDataObject
            {
                Number = int.Parse(value),
                Text = "parsed",
            };
        }
        catch (FormatException)
        {
            result = new OptimizerDataObject
            {
                Number = -1,
                Text = "fallback",
            };
        }

        return result;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static OptimizerDataStruct TryFinallyWithStructLocal(bool useFirst)
    {
        var result = new OptimizerDataStruct();
        try
        {
            if (useFirst)
            {
                result.Number = 7;
                result.Text = "first";
            }
            else
            {
                result.Number = 11;
                result.Text = "second";
            }
        }
        finally
        {
            result.Number++;
            result.Text = result.Text.ToUpperInvariant();
        }

        return result;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int ConditionalDelegate(bool addOffset, int value)
    {
        Func<int, int> operation;
        if (addOffset)
        {
            int offset = 2;
            operation = input => input + offset;
        }
        else
        {
            operation = static input => input * 2;
        }

        return operation(value);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static (int First, int Second) ConditionalRefToObjectField(bool selectFirst)
    {
        var first = new OptimizerDataObject { Number = 7 };
        var second = new OptimizerDataObject { Number = 11 };
        ref int selected = ref (selectFirst ? ref first.Number : ref second.Number);
        selected = 42;
        return (first.Number, second.Number);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int ExplicitReferenceCastOnLocal(bool compatibleType)
    {
        OptimizerBranchValue value;
        if (compatibleType)
            value = new OptimizerFirstBranchValue(7);
        else
            value = new OptimizerSecondBranchValue(11);

        var first = (OptimizerFirstBranchValue)value;
        return first.Number;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int ExplicitReferenceCastOnEvaluationStack(bool compatibleType)
    {
        var first = (OptimizerFirstBranchValue)(compatibleType
            ? (object)new OptimizerFirstBranchValue(7)
            : new OptimizerSecondBranchValue(11));
        return first.Number;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int ExplicitUnboxingCastOnLocal(bool boxedInt)
    {
        object value;
        if (boxedInt)
            value = 42;
        else
            value = (short)7;

        return (int)value;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int ExplicitUnboxingCastOnEvaluationStack(bool boxedInt) =>
        (int)(boxedInt ? (object)42 : (short)7);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static (bool IsString, bool IsInt, bool IsDataObject) IsOperatorsOnLocal(int kind)
    {
        object value;
        switch (kind)
        {
            case 0:
                value = "text";
                break;
            case 1:
                value = 42;
                break;
            default:
                value = new OptimizerDataObject { Number = 7 };
                break;
        }

        return (value is string, value is int, value is OptimizerDataObject);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int? AsClassOnLocal(bool compatibleType)
    {
        OptimizerBranchValue value;
        if (compatibleType)
            value = new OptimizerFirstBranchValue(7);
        else
            value = new OptimizerSecondBranchValue(11);

        OptimizerFirstBranchValue? first = value as OptimizerFirstBranchValue;
        return first?.Number;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int? AsInterfaceOnLocal(bool implementsInterface)
    {
        object value;
        if (implementsInterface)
            value = new OptimizerDataReader(7);
        else
            value = new OptimizerDataObject { Number = 11 };

        IOptimizerDataReader? reader = value as IOptimizerDataReader;
        return reader?.Read();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static bool IsOperatorOnLocal(bool selectFirst)
    {
        object value;
        if (selectFirst)
            value = new OptimizerFirstBranchValue(7);
        else
            value = new OptimizerSecondBranchValue(11);

        return value is OptimizerFirstBranchValue;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int? AsOperatorOnLocal(bool selectFirst)
    {
        object value;
        if (selectFirst)
            value = new OptimizerFirstBranchValue(7);
        else
            value = new OptimizerSecondBranchValue(11);

        OptimizerFirstBranchValue? first = value as OptimizerFirstBranchValue;
        return first?.Number;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static bool IsOperatorOnEvaluationStack(bool selectFirst) =>
        (selectFirst
            ? (object)new OptimizerFirstBranchValue(7)
            : new OptimizerSecondBranchValue(11))
        is OptimizerFirstBranchValue;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int? AsOperatorOnEvaluationStack(bool selectFirst)
    {
        OptimizerFirstBranchValue? first =
            (selectFirst
                ? (object)new OptimizerFirstBranchValue(7)
                : new OptimizerSecondBranchValue(11))
            as OptimizerFirstBranchValue;
        return first?.Number;
    }
}

public static class OptimizerControlFlowTargets
{
    public static int RightOperandCalls;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static string ConditionalBranches(int value)
    {
        if (value < 0)
            return "negative";
        if (value == 0)
            return "zero";
        return "positive";
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int DenseSwitch(int value)
    {
        switch (value)
        {
            case 0: return 10;
            case 1: return 11;
            case 2: return 12;
            case 3: return 13;
            default: return 99;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int LoopWithBreakAndContinue(int limit)
    {
        int total = 0;
        for (int value = 0; value < limit; value++)
        {
            if (value % 2 == 0)
                continue;
            if (value > 7)
                break;
            total += value;
        }

        return total;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int ConditionalInfiniteLoop(bool loopForever)
    {
        if (loopForever)
        {
            while (true) { }
        }

        return 42;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static bool ShortCircuit(bool left, bool right) =>
        left && EvaluateRight(right);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool EvaluateRight(bool value)
    {
        RightOperandCalls++;
        return value;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int RefLocalConditional(bool selectFirst)
    {
        int first = 1;
        int second = 2;
        ref int selected = ref (selectFirst ? ref first : ref second);
        selected = 42;
        return selectFirst ? first : second;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static string PatternMatching(object? value) =>
        value switch
        {
            null => "null",
            int number when number > 0 => "positive integer",
            int => "non-positive integer",
            string { Length: 0 } => "empty string",
            string text => text,
            BindingReference { Value: 42 } => "reference with value 42",
            _ => "other",
        };

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int? NullPropagation(OptimizerNullPropagationNode? node) =>
        node?.Next?.Value;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static string NullCoalescingAssignment(string? value)
    {
        value ??= "fallback";
        return value;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int ForeachWithContinueAndEarlyReturn(IEnumerable<int> values)
    {
        int total = 0;
        foreach (int value in values)
        {
            if (value == 0)
                continue;
            if (value < 0)
                return value;
            total += value;
        }

        return total;
    }
}

public static class OptimizerExceptionTargets
{
    private sealed class TestDisposable : IDisposable
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Dispose() => DisposalCount++;
    }

    private static readonly object SyncRoot = new();

    public static int FinallyExecutions;
    public static int DisposalCount;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int TryCatch(bool throwException)
    {
        try
        {
            if (throwException)
                throw new InvalidOperationException();
            return 1;
        }
        catch (InvalidOperationException)
        {
            return 2;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int TryFinally(bool returnEarly)
    {
        try
        {
            if (returnEarly)
                return 1;
            return 2;
        }
        finally
        {
            FinallyExecutions++;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int NestedTryFinallyAndCatch(int mode)
    {
        try
        {
            try
            {
                if (mode == 0)
                    return 10;
                if (mode == 1)
                    throw new InvalidOperationException();
                throw new ArgumentException();
            }
            finally
            {
                FinallyExecutions++;
            }
        }
        catch (InvalidOperationException)
        {
            return 20;
        }
        catch (ArgumentException)
        {
            return 30;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int ExceptionFilter(bool filterMatches)
    {
        try
        {
            throw new InvalidOperationException(filterMatches ? "match" : "other");
        }
        catch (InvalidOperationException exception) when (exception.Message == "match")
        {
            return 1;
        }
        catch (InvalidOperationException)
        {
            return 2;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int UsingWithEarlyReturn(bool returnEarly)
    {
        using (new TestDisposable())
        {
            if (returnEarly)
                return 1;
            return 2;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int LockWithConditionalReturn(bool returnEarly)
    {
        lock (SyncRoot)
        {
            if (returnEarly)
                return 1;
            return 2;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int CatchAndRethrow(bool rethrow)
    {
        try
        {
            throw new InvalidOperationException("original");
        }
        catch (InvalidOperationException)
        {
            if (rethrow)
                throw;
            return 42;
        }
    }
}

