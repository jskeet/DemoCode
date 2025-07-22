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

        var initalizerExpressions = syntax.Initializer.Expressions;
        foreach (AssignmentExpressionSyntax initializerExpression in initalizerExpressions)
        {
            var assignedParameter = model.GetSymbolInfo(initializerExpression.Left).Symbol;
            // The assigned parameter refers to a property, but we need to get at the parameter declaration.
            if (assignedParameter is not IPropertySymbol propertySymbol)
            {
                continue;
            }
            if (IsDangerous(propertySymbol))
            {
                var diagnostic = Diagnostic.Create(Rule, context.Node.GetLocation(), assignedParameter.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }

        bool IsDangerous(IPropertySymbol propertySymbol)
        {
            // This is awful - there must be a better way of getting the parameter symbol for a record.
            // I don't even known how to tell which constructor is the primary constructor...
            var containingRecord = propertySymbol.ContainingType;
            var constructors = containingRecord.InstanceConstructors;
            foreach (var ctor in constructors)
            {
                foreach (var parameter in ctor.Parameters)
                {
                    if (parameter.Name != propertySymbol.Name)
                    {
                        continue;
                    }
                    if (DangerousWithTargetAnalyzer.HasDangerousWithTargetAttribute(parameter))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
