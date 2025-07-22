using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace JonSkeet.RoslynAnalyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DangerousWithTargetAnalyzer : DiagnosticAnalyzer
{
    internal const string DangerousWithTargetAttributeShortName = "DangerousWithTarget";
    internal const string DangerousWithTargetAttributeFullName = "DangerousWithTargetAttribute";

    public const string DiagnosticId = "JS0001";

    private static readonly string Title = $"Record parameters used during initialization should be annotated with [{DangerousWithTargetAttributeShortName}]";
    private static readonly string MessageFormat = $"Record parameter '{{0}}' is used during initialization; it should be annotated with [{DangerousWithTargetAttributeShortName}]";
    private static readonly string Description = "Record parameters used during initialization can introduce inconsistencies when set using the 'with' operator." +
        $" Such parameters should be annotated with a [{DangerousWithTargetAttributeShortName}] attribute (which may be internal) to indicate that this is deliberate." +
        " Setting such parameters using the 'with' operator triggers a warning via another analyzer.";
    private const string Category = "Reliability";

    public static DiagnosticDescriptor Rule { get; } = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(AnalyzeRecordDeclaration, SyntaxKind.RecordDeclaration);
        context.EnableConcurrentExecution();
    }

    private static void AnalyzeRecordDeclaration(SyntaxNodeAnalysisContext context)
    {
        var syntax = (RecordDeclarationSyntax) context.Node;
        if (syntax.ParameterList is not ParameterListSyntax parameterList)
        {
            return;
        }
        var model = context.SemanticModel;
        var parameterSymbols = parameterList.Parameters.Select(p => model.GetDeclaredSymbol(p)).ToList();

        var node = context.Node;
        var declaredSymbol = model.GetDeclaredSymbol(node);
        if (declaredSymbol is not ITypeSymbol typeSymbol)
        {
            return;
        }
        if (!typeSymbol.IsRecord)
        {
            return;
        }
        // Null out any parameters which already have the annotation.
        for (int i = 0; i < parameterSymbols.Count; i++)
        {
            if (HasDangerousWithTargetAttribute(parameterSymbols[i], model))
            {
                parameterSymbols[i] = null;
            }
        }

        var recordMembers = typeSymbol.GetMembers();

        foreach (var recordMember in recordMembers)
        {
            var declaringReferences = recordMember.DeclaringSyntaxReferences;
            foreach (var decl in declaringReferences)
            {
                var declNode = decl.GetSyntax();
                // TODO: Figure out a nicer way of doing this. (And if we even need all of the options here.)
                if (declNode is PropertyDeclarationSyntax prop && prop.Initializer is not null)
                {
                    MaybeReportDiagnostic(context, parameterSymbols, prop.Initializer);
                }
                else if (declNode is FieldDeclarationSyntax field)
                {
                    foreach (var subDecl in field.Declaration.Variables)
                    {
                        MaybeReportDiagnostic(context, parameterSymbols, subDecl.Initializer);
                    }
                }
                else if (declNode is VariableDeclaratorSyntax variableDeclarator)
                {
                    MaybeReportDiagnostic(context, parameterSymbols, variableDeclarator.Initializer);
                }
            }
        }
    }

    private static void MaybeReportDiagnostic(SyntaxNodeAnalysisContext context, List<IParameterSymbol> parameterSymbols, EqualsValueClauseSyntax initializer)
    {
        var dataFlow = context.SemanticModel.AnalyzeDataFlow(initializer.Value);
        var readSymbols = dataFlow.ReadInside;
        for (int i = 0; i < readSymbols.Length; i++)
        {
            if (readSymbols[i] is not IParameterSymbol readParameterSymbol)
            {
                continue;
            }
            var parameterIndex = parameterSymbols.IndexOf(readParameterSymbol);
            if (parameterIndex != -1)
            {
                // Avoid reporting the same parameter multiple times.
                parameterSymbols[parameterIndex] = null;
                var firstDeclaration = readParameterSymbol.DeclaringSyntaxReferences.FirstOrDefault();                
                var diagnostic = Diagnostic.Create(Rule, firstDeclaration?.GetSyntax()?.GetLocation(), readParameterSymbol.Name);
                context.ReportDiagnostic(diagnostic);
                return;
            }
        }
        return;
    }

    internal static bool HasDangerousWithTargetAttribute(IParameterSymbol symbol, SemanticModel model) =>
        symbol.GetAttributes().Any(attr => attr.AttributeClass?.Name == DangerousWithTargetAttributeFullName);

    internal static bool HasDangerousWithTargetAttribute(IPropertySymbol symbol, SemanticModel model) =>
        symbol.GetAttributes().Any(attr => attr.AttributeClass?.Name == DangerousWithTargetAttributeFullName);
}
