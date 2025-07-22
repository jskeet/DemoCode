using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;
using System.Threading.Tasks;
using VerifyCS = JonSkeet.RoslynAnalyzers.Test.Verifiers.CSharpAnalyzerVerifier<JonSkeet.RoslynAnalyzers.DangerousWithOperatorAnalyzer>;

namespace JonSkeet.RoslynAnalyzers.Test;

public class DangerousWithOperatorAnalyzerTest
{
    private const string AttributeDeclaration = "[System.AttributeUsage(System.AttributeTargets.Parameter)] internal class DangerousWithTargetAttribute : System.Attribute {}\n";
    private const string OtherAttributeDeclaration = "[System.AttributeUsage(System.AttributeTargets.Parameter)] internal class OtherAttribute : System.Attribute {}\n";

    [Test]
    public async Task NoDangerousParameters()
    {
        var test = AttributeDeclaration + @"public record Simple(int X, int Y);

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
    public async Task DangerousParameterUnset()
    {
        var test = AttributeDeclaration + @"public record Simple(int X, [DangerousWithTarget] int Y);

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
    public async Task DangerousParameterSet()
    {
        var test = AttributeDeclaration + @"public record Simple([DangerousWithTarget] int X, int Y);

        class Test
        {
            static void M()
            {
                Simple s1 = new Simple(10, 10);
                Simple s2 = s1 with { X = 10 };
            }
        }";

        var diagnostic = new DiagnosticResult(DangerousWithOperatorAnalyzer.Rule)
            .WithSpan(9, 29, 9, 47)
            .WithArguments("X");
        await VerifyCS.VerifyAnalyzerAsync(test, diagnostic);
    }

    [Test]
    public async Task MultipleDangerousParameters()
    {
        var test = AttributeDeclaration + @"public record Simple([DangerousWithTarget] int X, [DangerousWithTarget] int Y);

        class Test
        {
            static void M()
            {
                Simple s1 = new Simple(10, 10);
                Simple s2 = s1 with { X = 20, Y = 20 };
            }
        }";

        var d1 = new DiagnosticResult(DangerousWithOperatorAnalyzer.Rule)
            .WithSpan(9, 29, 9, 55)
            .WithArguments("X");
        var d2 = new DiagnosticResult(DangerousWithOperatorAnalyzer.Rule)
            .WithSpan(9, 29, 9, 55)
            .WithArguments("Y");
        await VerifyCS.VerifyAnalyzerAsync(test, d1, d2);
    }

    [Test]
    public async Task OtherAttributesIgnored()
    {
        var test = OtherAttributeDeclaration + @"public record Simple([Other] int X, int Y);

        class Test
        {
            static void M()
            {
                Simple s1 = new Simple(10, 10);
                Simple s2 = s1 with { X = 10, Y = 20 };
            }
        }";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }
}
