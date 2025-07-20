using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace JonSkeet.RoslynAnalyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DangerousWithOperatorAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "JS0001";

    // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
    // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
    private static readonly string Title = "With operator sets a record parameter used during initialization";
    private static readonly string MessageFormat = "Record parameter '{0}' is used during initialization";
    private static readonly string Description = "With operator sets a record parameter used during initialization.";
    private const string Category = "Reliability";

    // TODO: Should this actually be private? Harder to test.
    public static DiagnosticDescriptor Rule { get; } = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(AnalyzeWithOperator, SyntaxKind.WithExpression);
        context.EnableConcurrentExecution();
    }

    private static void AnalyzeWithOperator(SyntaxNodeAnalysisContext context)
    {
        WithExpressionSyntax syntax = (WithExpressionSyntax) context.Node;
        var model = context.SemanticModel;
        var node = context.Node;
        var assignedParameters = syntax.Initializer
            .Expressions
            .Select(exp => model.GetSymbolInfo(((AssignmentExpressionSyntax) exp).Left).Symbol?.Name)
            .ToList();
        var typeInfo = model.GetTypeInfo(syntax.Expression);
        if (!typeInfo.ConvertedType.IsRecord)
        {
            return;
        }
        var recordMembers = typeInfo.ConvertedType.GetMembers();
        foreach (var recordMember in recordMembers)
        {
            var declaringReferences = recordMember.DeclaringSyntaxReferences;
            foreach (var decl in declaringReferences)
            {
                var declNode = decl.GetSyntax();
                if (declNode is PropertyDeclarationSyntax prop && prop.Initializer is not null)
                {
                    MaybeReportDiagnostic(context, assignedParameters, prop.Initializer);
                }
                else if (declNode is FieldDeclarationSyntax field)
                {
                    foreach (var subDecl in field.Declaration.Variables)
                    {
                        MaybeReportDiagnostic(context, assignedParameters, subDecl.Initializer);
                    }
                }
                else if (declNode is VariableDeclaratorSyntax variableDeclarator)
                {
                    MaybeReportDiagnostic(context, assignedParameters, variableDeclarator.Initializer);
                }
                else
                {
                    Debugger.Break();
                }
            }
        }
    }

    private static void MaybeReportDiagnostic(SyntaxNodeAnalysisContext context, List<string> assignedParameters, EqualsValueClauseSyntax initializer)
    {
        var dataFlow = context.SemanticModel.AnalyzeDataFlow(initializer.Value);
        var readSymbols = dataFlow.ReadInside;
        for (int i = 0; i < readSymbols.Length; i++)
        {
            var parameterIndex = assignedParameters.IndexOf(readSymbols[i].Name);
            if (parameterIndex != -1)
            {
                // Avoid reporting the same parameter multiple times.
                assignedParameters[parameterIndex] = null;
                var diagnostic = Diagnostic.Create(Rule, context.Node.GetLocation(), readSymbols[i].Name);
                context.ReportDiagnostic(diagnostic);
                return;
            }
        }
        return;
    }
}
