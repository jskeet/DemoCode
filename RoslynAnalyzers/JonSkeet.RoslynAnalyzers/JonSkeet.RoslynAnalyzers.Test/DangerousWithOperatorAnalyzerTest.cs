using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;
using System.Threading.Tasks;
using VerifyCS = JonSkeet.RoslynAnalyzers.Test.Verifiers.CSharpAnalyzerVerifier<JonSkeet.RoslynAnalyzers.DangerousWithOperatorAnalyzer>;

namespace JonSkeet.RoslynAnalyzers.Test;

public class DangerousWithOperatorAnalyzerTest
{
    [Test]
    public async Task TestNoInitializers()
    {
        var test = @"public record Simple(int X, int Y);

        class Test
        {
            static void M()
            {
                Simple s1 = new Simple(10, 10);
                Simple s2 = s1 with { X = 10 };
            }
        }";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task TestInitializerUsedInUnsetParameter()
    {
        var test = @"public record Simple(int X, int Y)
        {
            public int Z { get; } = Y * 2;
        }

        class Test
        {
            static void M()
            {
                Simple s1 = new Simple(10, 10);
                Simple s2 = s1 with { X = 10 };
            }
        }";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task TestPropertyInitializerUsedInSetParameter()
    {
        var test = @"public record Simple(int X, int Y)
        {
            public int Z { get; } = X * 2;
        }

        class Test
        {
            static void M()
            {
                Simple s1 = new Simple(10, 10);
                Simple s2 = s1 with { X = 10 };
            }
        }";

        var diagnostic = new DiagnosticResult(DangerousWithOperatorAnalyzer.Rule)
            .WithSpan(11, 29, 11, 47)
            .WithArguments("X");
        await VerifyCS.VerifyAnalyzerAsync(test, diagnostic);
    }

    [Test]
    public async Task TestInitializerCallingMethodUsedInSetParameter()
    {
        var test = @"public record Simple(int X, int Y)
        {
            public int Z { get; } = M(X);
            private static int M(int value) => value;
        }

        class Test
        {
            static void M()
            {
                Simple s1 = new Simple(10, 10);
                Simple s2 = s1 with { X = 10 };
            }
        }";

        var diagnostic = new DiagnosticResult(DangerousWithOperatorAnalyzer.Rule)
            .WithSpan(12, 29, 12, 47)
            .WithArguments("X");
        await VerifyCS.VerifyAnalyzerAsync(test, diagnostic);
    }

    [Test]
    public async Task TestMultipleInitializersSingleDiagnostic()
    {
        var test = @"public record Simple(int X, int Y)
        {
            public int Z1 { get; } = X * 2;
            public int Z2 { get; } = X * 2;
        }

        class Test
        {
            static void M()
            {
                Simple s1 = new Simple(10, 10);
                Simple s2 = s1 with { X = 10 };
            }
        }";

        var diagnostic = new DiagnosticResult(DangerousWithOperatorAnalyzer.Rule)
            .WithSpan(12, 29, 12, 47)
            .WithArguments("X");
        await VerifyCS.VerifyAnalyzerAsync(test, diagnostic);
    }

    [Test]
    public async Task TestMultipleParametersDiagnostics()
    {
        var test = @"public record Simple(int X, int Y)
        {
            public int Z1 { get; } = X * 2;
            public int Z2 { get; } = Y * 2;
        }

        class Test
        {
            static void M()
            {
                Simple s1 = new Simple(10, 10);
                Simple s2 = s1 with { X = 10, Y = 20 };
            }
        }";

        var d1 = new DiagnosticResult(DangerousWithOperatorAnalyzer.Rule)
            .WithSpan(12, 29, 12, 55)
            .WithArguments("X");
        var d2 = new DiagnosticResult(DangerousWithOperatorAnalyzer.Rule)
            .WithSpan(12, 29, 12, 55)
            .WithArguments("Y");
        await VerifyCS.VerifyAnalyzerAsync(test, d1, d2);
    }

    [Test]
    public async Task TestFieldInitializerUsedInSetParameter()
    {
        var test = @"public record Simple(int X, int Y)
        {
            private readonly int z = X * 2;

            public int Z => z;
        }

        class Test
        {
            static void M()
            {
                Simple s1 = new Simple(10, 10);
                Simple s2 = s1 with { X = 10 };
            }
        }";

        var diagnostic = new DiagnosticResult(DangerousWithOperatorAnalyzer.Rule)
            .WithSpan(13, 29, 13, 47)
            .WithArguments("X");
        await VerifyCS.VerifyAnalyzerAsync(test, diagnostic);
    }

    [Test]
    public async Task TestMultipleFieldInitializerUsedInSetParameter()
    {
        var test = @"public record Simple(int X, int Y)
        {
            private readonly int z = Y * 2, zz = X * 2;

            public int Z => z;
            public int ZZ => z;
        }

        class Test
        {
            static void M()
            {
                Simple s1 = new Simple(10, 10);
                Simple s2 = s1 with { X = 10 };
            }
        }";

        var diagnostic = new DiagnosticResult(DangerousWithOperatorAnalyzer.Rule)
            .WithSpan(14, 29, 14, 47)
            .WithArguments("X");
        await VerifyCS.VerifyAnalyzerAsync(test, diagnostic);
    }
}
