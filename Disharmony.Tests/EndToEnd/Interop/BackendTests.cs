namespace Disharmony.Tests.EndToEnd.Interop;

[TestFixture]
public sealed class BackendTests
{
    [Test]
    public void Instance_OnWaylithClr_SelectsWaylithBackend()
    {
        if (!TestRunSetup.IsWaylithClr)
            Assert.Ignore("Requires WaylithCLR and the Disharmony.WaylithCLR plugin.");

        Type implementation = Backend.Instance.GetType();
        Assert.That(implementation.FullName, Is.EqualTo("Disharmony.WaylithCLR.WaylithBackend"));
        Assert.That(implementation.Assembly.GetName().Name, Is.EqualTo("Disharmony.WaylithCLR"));

        MethodInfo create = typeof(Backend).GetMethod("Create", BindingFlags.NonPublic | BindingFlags.Static)!;
        object backend = create.Invoke(null, null)!;
        Assert.That(backend.GetType(), Is.SameAs(implementation), "The factory must reuse the loaded plugin assembly.");
    }
}
