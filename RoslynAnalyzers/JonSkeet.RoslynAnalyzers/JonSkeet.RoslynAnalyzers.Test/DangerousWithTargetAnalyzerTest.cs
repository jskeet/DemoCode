using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;
using VerifyCS = JonSkeet.RoslynAnalyzers.Test.Verifiers.CSharpAnalyzerVerifier<JonSkeet.RoslynAnalyzers.DangerousWithTargetAnalyzer>;

namespace JonSkeet.RoslynAnalyzers.Test;

public class DangerousWithTargetAnalyzerTest
{
    private const string AttributeDeclaration = "[System.AttributeUsage(System.AttributeTargets.Parameter)] internal class DangerousWithTargetAttribute : System.Attribute {}\n";
    private const string OtherAttributeDeclaration = "[System.AttributeUsage(System.AttributeTargets.Parameter)] internal class OtherAttribute : System.Attribute {}\n";

    [Test]
    public async Task NoParameters()
    {
        var test = AttributeDeclaration + @"public record Simple;";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task NoInitializers()
    {
        var test = AttributeDeclaration + @"public record Simple(int X, int Y);";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task PropertyInitializer()
    {
        var test = AttributeDeclaration + @"public record Simple(int X, int Y)
        {
            public int Z { get; } = X * 2;
        }";

        var diagnostic = new DiagnosticResult(DangerousWithTargetAnalyzer.Rule)
            .WithSpan(2, 22, 2, 27)
            .WithArguments("X");
        await VerifyCS.VerifyAnalyzerAsync(test, diagnostic);
    }

    [Test]
    public async Task InitializerCallingMethod()
    {
        var test = AttributeDeclaration + @"public record Simple(int X, int Y)
        {
            public int Z { get; } = M(X);
            private static int M(int value) => value;
        }";

        var diagnostic = new DiagnosticResult(DangerousWithTargetAnalyzer.Rule)
            .WithSpan(2, 22, 2, 27)
            .WithArguments("X");
        await VerifyCS.VerifyAnalyzerAsync(test, diagnostic);
    }

    [Test]
    public async Task MultipleInitializersSingleDiagnostic()
    {
        var test = AttributeDeclaration + @"public record Simple(int X, int Y)
        {
            public int Z1 { get; } = X * 2;
            public int Z2 { get; } = X * 2;
        }";

        var diagnostic = new DiagnosticResult(DangerousWithTargetAnalyzer.Rule)
            .WithSpan(2, 22, 2, 27)
            .WithArguments("X");
        await VerifyCS.VerifyAnalyzerAsync(test, diagnostic);
    }

    [Test]
    public async Task MultipleInitializerMultipleDiagnostics()
    {
        var test = AttributeDeclaration + @"public record Simple(int X, int Y, int Safe)
        {
            public int Z1 { get; } = X * 2;
            public int Z2 { get; } = Y * 2;
        }";

        var d1 = new DiagnosticResult(DangerousWithTargetAnalyzer.Rule)
            .WithSpan(2, 22, 2, 27)
            .WithArguments("X");
        var d2 = new DiagnosticResult(DangerousWithTargetAnalyzer.Rule)
            .WithSpan(2, 29, 2, 34)
            .WithArguments("Y");
        await VerifyCS.VerifyAnalyzerAsync(test, d1, d2);
    }

    [Test]
    public async Task SingleFieldInitializer()
    {
        var test = AttributeDeclaration + @"public record Simple(int X, int Y)
        {
            private readonly int z = X * 2;

            public int Z => z;
        }";

        var diagnostic = new DiagnosticResult(DangerousWithTargetAnalyzer.Rule)
            .WithSpan(2, 22, 2, 27)
            .WithArguments("X");
        await VerifyCS.VerifyAnalyzerAsync(test, diagnostic);
    }

    [Test]
    public async Task MultipleFieldInitializers()
    {
        var test = AttributeDeclaration + @"public record Simple(int X, int Y, int Safe)
        {
            private readonly int z = Y * 2, zz = X * 2;

            public int Z => z;
            public int ZZ => z;
        }";

        var d1 = new DiagnosticResult(DangerousWithTargetAnalyzer.Rule)
            .WithSpan(2, 29, 2, 34)
            .WithArguments("Y");
        var d2 = new DiagnosticResult(DangerousWithTargetAnalyzer.Rule)
            .WithSpan(2, 22, 2, 27)
            .WithArguments("X");
        await VerifyCS.VerifyAnalyzerAsync(test, d1, d2);
    }

    [Test]
    public async Task AttributedParameter()
    {
        var test = AttributeDeclaration + @"public record Simple([DangerousWithTarget] int X, int Y)
        {
            public int Z { get; } = X * 2;
        }";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task OtherAttributedParameter()
    {
        var test = AttributeDeclaration + OtherAttributeDeclaration + @"public record Simple([Other] int X, int Y)
        {
            public int Z { get; } = X * 2;
        }";

        var diagnostic = new DiagnosticResult(DangerousWithTargetAnalyzer.Rule)
            .WithSpan(3, 22, 3, 35)
            .WithArguments("X");
        await VerifyCS.VerifyAnalyzerAsync(test, diagnostic);
    }

    [Test]
    public async Task MixedAttribution()
    {
        var test = AttributeDeclaration + @"public record Simple([DangerousWithTarget] int X, int Y)
        {
            public int Z { get; } = X * 2;
            public int ZZ { get; } = Y * 2;
        }";

        var diagnostic = new DiagnosticResult(DangerousWithTargetAnalyzer.Rule)
            .WithSpan(2, 51, 2, 56)
            .WithArguments("Y");
        await VerifyCS.VerifyAnalyzerAsync(test, diagnostic);
    }

    [Test]
    public async Task PartialClassInMultipleFiles()
    {
        var source1 = @"
using System.Collections.Immutable;
internal partial record InputChannelHashes(int Id, uint Name, uint Fader, uint Mute, uint StereoMode, ImmutableList<uint> OutputLevels);
";
        var source2 = @"
using System.Collections.Immutable;
internal partial record InputChannelHashes
{
    internal static ImmutableList<InputChannelHashes> AllInputs { get; } =
    [
        new(1, 2627760978, 950957506, 4111428088, 3884397269, [3137631695, 2388890104]),
    ];

    internal static ImmutableDictionary<int, InputChannelHashes> AllInputsByChannelId { get; } =
        InputChannelHashes.AllInputs.ToImmutableDictionary(ch => ch.Id);
}
";
        var test = new VerifyCS.Test
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestState =
            {
                Sources = { source1, source2 }
            },
        };

        await test.RunAsync(CancellationToken.None);
    }
}
