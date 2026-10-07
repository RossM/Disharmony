namespace Disharmony.Tests;

public interface IMethodLookupTarget
{
    int Convert(int value);
}

public class MethodLookupBase
{
    public int Inherited(int value) => value;
}

public class MethodLookupTarget : MethodLookupBase, IMethodLookupTarget
{
    public class Nested : MethodLookupBase
    {
        public static int StaticMethod(int value) => value + 2;

        public class DeepNested
        {
            public static int StaticMethod(int value) => value + 3;
        }
    }

    public void Target() { }
    public static void StaticTarget() { }
    public int Convert(int value) => value + 1;
    public string Convert(string value) => value;
    public int Convert() => 42;
    public int ByRef(ref int value) => ++value;
    public int ByIn(in int value) => value;
    public int ByOut(out int value) => value = 7;
}

public class MethodLookupDerived : MethodLookupTarget
{
    public int DerivedOnly(int value) => value;
}
