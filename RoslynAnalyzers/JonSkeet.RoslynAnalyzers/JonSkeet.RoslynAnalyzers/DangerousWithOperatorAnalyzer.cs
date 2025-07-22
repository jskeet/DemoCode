using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace JonSkeet.RoslynAnalyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DangerousWithOperatorAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "JS0002";

    private const string Title = $"Record parameters annotated with [{DangerousWithTargetAnalyzer.DangerousWithTargetAttributeShortName}] should not be set using the 'with' operator.";
    private const string MessageFormat = $"Record parameter '{{0}}' is annotated with [{DangerousWithTargetAnalyzer.DangerousWithTargetAttributeShortName}]";
    private const string Description = $"Record parameters are annotated with [{DangerousWithTargetAnalyzer.DangerousWithTargetAttributeShortName}] if they are dangerous to set using the 'with' operator." +
        " This is usually due to computations during initialization using the parameter, which aren't performed again using the new value. Using the 'with' operator with such parameters can lead to inconsistent state.";
    private const string Category = "Reliability";

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
            .Select(exp => model.GetSymbolInfo(((AssignmentExpressionSyntax) exp).Left).Symbol)
            .ToList();

        foreach (var assignedParameter in assignedParameters)
        {
            // The assigned parameter refers to a property, but we need to get at the parameter declaration.
            // We check each declaring syntax to see if it's actually declaring a parameter.
            if (assignedParameter is not IPropertySymbol propertySymbol)
            {
                continue;
            }
            foreach (var syntaxReference in propertySymbol.DeclaringSyntaxReferences)
            {
                var declarationSyntax = syntaxReference.GetSyntax();
                if (declarationSyntax is not ParameterSyntax parameterSyntax)
                {
                    continue;
                }
                var declaredSymbol = model.GetDeclaredSymbol(parameterSyntax);
                if (declaredSymbol is IParameterSymbol parameterSymbol &&
                    DangerousWithTargetAnalyzer.HasDangerousWithTargetAttribute(parameterSymbol, model))
                {
                    var diagnostic = Diagnostic.Create(Rule, context.Node.GetLocation(), parameterSymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
