#if WAYLITHCLR_TESTS
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Disharmony.RulesEngine;

namespace Disharmony.Tests.EndToEnd.Interop;

// Controlled rewrites keep the expected offsets independent of the public patch builders.
[TestFixture]
public sealed class WaylithLoggingTests : PatchTestBase
{
    [Test]
    public void Locals_IncludeOriginalAndRewriteDeclarations()
    {
        Assert.That(Backend.Instance.GetType().FullName, Is.EqualTo("Disharmony.WaylithCLR.WaylithBackend"));
        var assembly = new PersistedAssemblyBuilder(new AssemblyName(nameof(Locals_IncludeOriginalAndRewriteDeclarations)), typeof(object).Assembly);
        var type = assembly.DefineDynamicModule("Tests").DefineType("Target", TypeAttributes.Public);
        var observed = type.DefineField("Observed", typeof(double), FieldAttributes.Public | FieldAttributes.Static);
        var method = type.DefineMethod("Value", MethodAttributes.Public | MethodAttributes.Static, typeof(int), Type.EmptyTypes);
        method.SetImplementationFlags(MethodImplAttributes.NoInlining);
        var il = method.GetILGenerator();
        var text = il.DeclareLocal(typeof(string));
        var number = il.DeclareLocal(typeof(int));
        il.Emit(OpCodes.Ldstr, "original");
        il.Emit(OpCodes.Stloc, text);
        il.Emit(OpCodes.Ldc_I4_7);
        il.Emit(OpCodes.Stloc, number);
        il.Emit(OpCodes.Ldloc, number);
        il.Emit(OpCodes.Ret);
        type.CreateType();
        using var stream = new MemoryStream();
        assembly.Save(stream);
        Type targetType = Assembly.Load(stream.ToArray()).GetType("Target")!;
        MethodInfo target = targetType.GetMethod("Value")!;
        FieldInfo observedField = targetType.GetField("Observed")!;
        var template = new DynamicMethod("Template", typeof(void), Type.EmptyTypes).GetILGenerator();
        LocalBuilder added = template.DeclareLocal(typeof(double));
        var rules = new Ruleset(new Rule
        {
            Mode = OutputMode.MethodPrefix,
            Output = [
                new CodeInstruction(OpCodes.Ldc_R8, 2.5),
                new CodeInstruction(OpCodes.Stloc, added),
                new CodeInstruction(OpCodes.Ldloc, added),
                new CodeInstruction(OpCodes.Stsfld, observedField),
            ],
        });
        FileLog.FlushBuffer();
        using var logStream = new MemoryStream();
        using var writer = new StreamWriter(logStream) { AutoFlush = true };
        var previousWriter = FileLog.LogWriter;
        FileLog.LogWriter = writer;
        try
        {
            Backend.Instance.Patch(new MethodInvocation(target), rules, false, debug: true, optimize: false);
            Backend.Instance.Apply();
            Assert.That(target.Invoke(null, null), Is.EqualTo(7));
            Assert.That(observedField.GetValue(null), Is.EqualTo(2.5));
            FileLog.FlushBuffer();
            string log = Encoding.UTF8.GetString(logStream.ToArray());
            Assert.Multiple(() =>
            {
                Assert.That(log, Does.Contain("Local var 0: System.String"));
                Assert.That(log, Does.Contain("Local var 1: System.Int32"));
                Assert.That(log, Does.Contain("Local var 2: System.Double"));
                Assert.That(Regex.Matches(log, "Local var [0-9]+:"), Has.Count.EqualTo(3), log);
            });
        }
        finally
        {
            try
            {
                Backend.Instance.Unpatch(target);
                Backend.Instance.Apply();
            }
            finally { FileLog.LogWriter = previousWriter; }
        }
    }

    [Test]
    public void Labels_MatchBranchOperandsAtEmittedOffsets()
    {
        Assert.That(Backend.Instance.GetType().FullName, Is.EqualTo("Disharmony.WaylithCLR.WaylithBackend"));
        var assembly = new PersistedAssemblyBuilder(new AssemblyName(nameof(Labels_MatchBranchOperandsAtEmittedOffsets)), typeof(object).Assembly);
        var type = assembly.DefineDynamicModule("Tests").DefineType("Target", TypeAttributes.Public);
        var method = type.DefineMethod("Value", MethodAttributes.Public | MethodAttributes.Static, typeof(int), [typeof(bool)]);
        method.SetImplementationFlags(MethodImplAttributes.NoInlining);
        var il = method.GetILGenerator();
        Label alternate = il.DefineLabel();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Brtrue_S, alternate);
        il.Emit(OpCodes.Ldc_I4_1);
        il.Emit(OpCodes.Ret);
        il.MarkLabel(alternate);
        il.Emit(OpCodes.Ldc_I4_2);
        il.Emit(OpCodes.Ret);
        type.CreateType();
        using var stream = new MemoryStream();
        assembly.Save(stream);
        Type targetType = Assembly.Load(stream.ToArray()).GetType("Target")!;
        MethodInfo target = targetType.GetMethod("Value")!;
        var invoke = (Func<bool, int>)target.CreateDelegate(typeof(Func<bool, int>));
        FileLog.FlushBuffer();
        using var logStream = new MemoryStream();
        using var writer = new StreamWriter(logStream) { AutoFlush = true };
        var previousWriter = FileLog.LogWriter;
        FileLog.LogWriter = writer;
        try
        {
            Backend.Instance.Patch(new MethodInvocation(target), new Ruleset(), false, debug: true, optimize: false);
            Backend.Instance.Apply();
            Assert.That(invoke(false), Is.EqualTo(1));
            Assert.That(invoke(true), Is.EqualTo(2));
            FileLog.FlushBuffer();
            string log = Encoding.UTF8.GetString(logStream.ToArray());
            Match branch = Regex.Match(log, @"IL_0001:.*brtrue.*(Label[0-9]+)");
            Assert.That(branch.Success, Is.True, log);
            string label = branch.Groups[1].Value;
            // The branch is widened to five bytes, putting its target at byte eight.
            Assert.That(log, Does.Match(@"(?m)^\s*IL_0008:\s+" + Regex.Escape(label) + @"\s*$"));
            Assert.That(log, Does.Match(@"IL_0008:\s+ldc\.i4\.2"));
            Assert.That(Regex.Matches(log, Regex.Escape(label) + @"\b"), Has.Count.EqualTo(2), log);
        }
        finally
        {
            try
            {
                Backend.Instance.Unpatch(target);
                Backend.Instance.Apply();
            }
            finally { FileLog.LogWriter = previousWriter; }
        }
    }

    [Test]
    public void ExceptionBlocks_DescribeNestedCatchAndFinally()
    {
        Assert.That(Backend.Instance.GetType().FullName, Is.EqualTo("Disharmony.WaylithCLR.WaylithBackend"));
        var assembly = new PersistedAssemblyBuilder(new AssemblyName(nameof(ExceptionBlocks_DescribeNestedCatchAndFinally)), typeof(object).Assembly);
        var type = assembly.DefineDynamicModule("Tests").DefineType("Target", TypeAttributes.Public);
        var method = type.DefineMethod("Value", MethodAttributes.Public | MethodAttributes.Static, typeof(int), [typeof(bool)]);
        method.SetImplementationFlags(MethodImplAttributes.NoInlining);
        var il = method.GetILGenerator();
        LocalBuilder result = il.DeclareLocal(typeof(int));
        Label normal = il.DefineLabel();
        il.BeginExceptionBlock();
        Label end = il.BeginExceptionBlock();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Brfalse_S, normal);
        il.Emit(OpCodes.Newobj, typeof(InvalidOperationException).GetConstructor(Type.EmptyTypes)!);
        il.Emit(OpCodes.Throw);
        il.MarkLabel(normal);
        il.Emit(OpCodes.Ldc_I4_1);
        il.Emit(OpCodes.Stloc, result);
        il.Emit(OpCodes.Leave, end);
        il.BeginCatchBlock(typeof(InvalidOperationException));
        il.Emit(OpCodes.Pop);
        il.Emit(OpCodes.Ldc_I4_2);
        il.Emit(OpCodes.Stloc, result);
        il.EndExceptionBlock();
        il.BeginFinallyBlock();
        il.Emit(OpCodes.Ldloc, result);
        il.Emit(OpCodes.Ldc_I4, 10);
        il.Emit(OpCodes.Add);
        il.Emit(OpCodes.Stloc, result);
        il.EndExceptionBlock();
        il.Emit(OpCodes.Ldloc, result);
        il.Emit(OpCodes.Ret);
        type.CreateType();
        using var stream = new MemoryStream();
        assembly.Save(stream);
        Type targetType = Assembly.Load(stream.ToArray()).GetType("Target")!;
        MethodInfo target = targetType.GetMethod("Value")!;
        var invoke = (Func<bool, int>)target.CreateDelegate(typeof(Func<bool, int>));
        FileLog.FlushBuffer();
        using var logStream = new MemoryStream();
        using var writer = new StreamWriter(logStream) { AutoFlush = true };
        var previousWriter = FileLog.LogWriter;
        int previousIndent = FileLog.indentLevel;
        FileLog.LogWriter = writer;
        try
        {
            Backend.Instance.Patch(new MethodInvocation(target), new Ruleset(), false, debug: true, optimize: false);
            Backend.Instance.Apply();
            Assert.That(invoke(false), Is.EqualTo(11));
            Assert.That(invoke(true), Is.EqualTo(12));
            FileLog.FlushBuffer();
            string log = Encoding.UTF8.GetString(logStream.ToArray());
            Assert.That(Regex.Matches(log, @"\.try\b"), Has.Count.EqualTo(2), log);
            Assert.That(log, Does.Match(@"\.catch.*InvalidOperationException"));
            Assert.That(Regex.Matches(log, @"\.finally\b"), Has.Count.EqualTo(1), log);
            Assert.That(Regex.Matches(log, "end handler"), Has.Count.EqualTo(2), log);
            int outerTry = log.IndexOf(".try", StringComparison.Ordinal);
            int innerTry = log.IndexOf(".try", outerTry + 4, StringComparison.Ordinal);
            int catchBlock = log.IndexOf(".catch", StringComparison.Ordinal);
            int innerEnd = log.IndexOf("end handler", StringComparison.Ordinal);
            int finallyBlock = log.IndexOf(".finally", StringComparison.Ordinal);
            int outerEnd = log.LastIndexOf("end handler", StringComparison.Ordinal);
            Assert.That(new[] { outerTry, innerTry, catchBlock, innerEnd, finallyBlock, outerEnd }, Is.Ordered.Ascending, log);
            Assert.That(FileLog.indentLevel, Is.EqualTo(previousIndent), log);
        }
        finally
        {
            try
            {
                Backend.Instance.Unpatch(target);
                Backend.Instance.Apply();
            }
            finally
            {
                FileLog.LogWriter = previousWriter;
                FileLog.indentLevel = previousIndent;
            }
        }
    }

    [Test]
    public void Comments_RetainOrderAndZeroByteOffsets()
    {
        Assert.That(Backend.Instance.GetType().FullName, Is.EqualTo("Disharmony.WaylithCLR.WaylithBackend"));
        var assembly = new PersistedAssemblyBuilder(new AssemblyName(nameof(Comments_RetainOrderAndZeroByteOffsets)), typeof(object).Assembly);
        var type = assembly.DefineDynamicModule("Tests").DefineType("Target", TypeAttributes.Public);
        var method = type.DefineMethod("Value", MethodAttributes.Public | MethodAttributes.Static, typeof(int), Type.EmptyTypes);
        method.SetImplementationFlags(MethodImplAttributes.NoInlining);
        method.GetILGenerator().Emit(OpCodes.Ldc_I4_7);
        method.GetILGenerator().Emit(OpCodes.Ret);
        type.CreateType();
        using var stream = new MemoryStream();
        assembly.Save(stream);
        Type targetType = Assembly.Load(stream.ToArray()).GetType("Target")!;
        MethodInfo target = targetType.GetMethod("Value")!;
        var rules = new Ruleset(
            new Rule
            {
                Mode = OutputMode.MethodPrefix,
                Output = [CodeInstruction.Annotation("First comment"), CodeInstruction.Annotation("Second comment")],
            },
            new Rule
            {
                Mode = OutputMode.MethodPostfix,
                Output = [CodeInstruction.Annotation("Trailing comment")],
            });
        FileLog.FlushBuffer();
        using var logStream = new MemoryStream();
        using var writer = new StreamWriter(logStream) { AutoFlush = true };
        var previousWriter = FileLog.LogWriter;
        FileLog.LogWriter = writer;
        try
        {
            Backend.Instance.Patch(new MethodInvocation(target), rules, false, debug: true, optimize: false);
            Backend.Instance.Apply();
            Assert.That(target.Invoke(null, null), Is.EqualTo(7));
            FileLog.FlushBuffer();
            string log = Encoding.UTF8.GetString(logStream.ToArray());
            Assert.That(log, Does.Match(@"IL_0000:\s+// First comment"));
            Assert.That(log, Does.Match(@"IL_0000:\s+// Second comment"));
            Assert.That(log, Does.Match(@"IL_0000:\s+ldc\.i4\.7"));
            Assert.That(log, Does.Match(@"IL_0001:\s+ret"));
            Assert.That(log, Does.Match(@"IL_0002:\s+// Trailing comment"));
            Assert.That(Regex.Matches(log, @"(?m)^\s*IL_[0-9A-F]+:\s+nop\s*$"), Is.Empty, log);
            int first = log.IndexOf("First comment", StringComparison.Ordinal);
            int second = log.IndexOf("Second comment", StringComparison.Ordinal);
            int body = log.IndexOf("ldc.i4.7", StringComparison.Ordinal);
            int trailing = log.IndexOf("Trailing comment", StringComparison.Ordinal);
            Assert.That(new[] { first, second, body, trailing }, Is.Ordered.Ascending, log);
        }
        finally
        {
            try
            {
                Backend.Instance.Unpatch(target);
                Backend.Instance.Apply();
            }
            finally { FileLog.LogWriter = previousWriter; }
        }
    }
}
#endif
