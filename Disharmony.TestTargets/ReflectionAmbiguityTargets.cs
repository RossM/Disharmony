namespace Disharmony.Tests.ReflectionFixtures.Ambiguity;

public static class Target
{
    public static void Method(int value) { }
}

public static class Outer
{
    public static class Target
    {
        public static void Method(int value) { }
        public static Func<int> Container()
        {
            int Local() => 42;
            return Local;
        }
    }
}
