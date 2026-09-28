// This code is licensed under the BSD 3-Clause license (see LICENSE for details)
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Cosmos.Build.Analyzer.Patcher;

/// <summary>
/// Roslyn diagnostic analyzer that keeps a driver assembly on the public driver kit seam.
/// A driver assembly is a project that declares <c>&lt;CosmosDriverAssembly&gt;true&lt;/CosmosDriverAssembly&gt;</c>;
/// the compiler is then the proof that it calls only what a third-party driver library can
/// call, and this analyzer closes the two hatches that would let it reach internals anyway:
/// <list type="bullet">
///   <item>NAOT0008: any <c>[UnsafeAccessor]</c> or <c>[UnsafeAccessorType]</c> in the compilation.</item>
///   <item>NAOT0009: a referenced <c>Cosmos.*</c> assembly whose <c>InternalsVisibleTo</c> names this assembly.</item>
/// </list>
/// Both are errors. A compilation that is not a driver assembly is left alone.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DriverAssemblyAnalyzer : DiagnosticAnalyzer
{
    private const string UnsafeAccessorAttributeShortName = "UnsafeAccessorAttribute";
    private const string UnsafeAccessorTypeAttributeShortName = "UnsafeAccessorTypeAttribute";
    private const string UnsafeAccessorAttributeName = "System.Runtime.CompilerServices." + UnsafeAccessorAttributeShortName;
    private const string UnsafeAccessorTypeAttributeName = "System.Runtime.CompilerServices." + UnsafeAccessorTypeAttributeShortName;
    private const string CosmosAssemblyPrefix = "Cosmos.";

    /// <summary>What the diagnostic names when the attribute decorates nothing it can name.</summary>
    private const string UnnamedMember = "(unnamed)";

    /// <summary>
    /// The diagnostics this analyzer reports.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            DiagnosticMessages.DriverAssemblyUsesUnsafeAccessor,
            DiagnosticMessages.DriverAssemblyGrantedInternals);

    /// <summary>
    /// Registers the analysis, which runs only in a driver assembly. Generated code is
    /// analyzed too: an accessor a source generator emits reaches internals exactly as a
    /// hand-written one does.
    /// </summary>
    /// <param name="context">The analysis context.</param>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(startContext =>
        {
            if (!LayerAnalyzer.IsDriverAssembly(startContext.Options))
            {
                return;
            }

            startContext.RegisterSyntaxNodeAction(AnalyzeAttribute, SyntaxKind.Attribute);
            startContext.RegisterCompilationEndAction(ReportInternalsGrants);
        });
    }

    /// <summary>
    /// Binds one attribute, so a polyfilled attribute type and one from the runtime are
    /// caught alike. The diagnostic sits on the attribute and names the member it
    /// decorates, which for <c>[UnsafeAccessorType]</c> on a parameter or a return type
    /// is the enclosing method.
    /// </summary>
    private static void AnalyzeAttribute(SyntaxNodeAnalysisContext context)
    {
        AttributeSyntax attribute = (AttributeSyntax)context.Node;
        SemanticModel model = context.SemanticModel;

        INamedTypeSymbol? attributeType = model.GetTypeInfo(attribute, context.CancellationToken).Type as INamedTypeSymbol
            ?? model.GetSymbolInfo(attribute, context.CancellationToken).Symbol?.ContainingType;
        if (attributeType == null)
        {
            return;
        }

        // The short name rules out almost every attribute before the full name is built.
        if (attributeType.Name != UnsafeAccessorAttributeShortName && attributeType.Name != UnsafeAccessorTypeAttributeShortName)
        {
            return;
        }

        string attributeTypeName = attributeType.ToDisplayString();
        if (attributeTypeName != UnsafeAccessorAttributeName && attributeTypeName != UnsafeAccessorTypeAttributeName)
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            DiagnosticMessages.DriverAssemblyUsesUnsafeAccessor,
            attribute.GetLocation(),
            DecoratedMemberName(attribute, model, context.CancellationToken),
            attributeType.Name));
    }

    /// <summary>
    /// The name of the nearest enclosing member or local function; a field's first
    /// declarator when the member is a field; the attribute target (<c>assembly</c>,
    /// <c>module</c>) when the attribute decorates the assembly itself.
    /// </summary>
    private static string DecoratedMemberName(AttributeSyntax attribute, SemanticModel model, System.Threading.CancellationToken cancellationToken)
    {
        foreach (SyntaxNode ancestor in attribute.Ancestors())
        {
            if (ancestor is BaseFieldDeclarationSyntax field)
            {
                return field.Declaration.Variables.Count > 0
                    ? field.Declaration.Variables[0].Identifier.ValueText
                    : UnnamedMember;
            }

            if (ancestor is MemberDeclarationSyntax || ancestor is LocalFunctionStatementSyntax)
            {
                return model.GetDeclaredSymbol(ancestor, cancellationToken)?.Name ?? UnnamedMember;
            }
        }

        if (attribute.Parent is AttributeListSyntax list && list.Target != null)
        {
            return list.Target.Identifier.ValueText;
        }

        return UnnamedMember;
    }

    /// <summary>
    /// Reports every referenced <c>Cosmos.*</c> assembly whose <c>InternalsVisibleTo</c> names
    /// this compilation. Roslyn already resolves the grant, name and public key alike, so
    /// <see cref="IAssemblySymbol.GivesAccessTo"/> is the single test.
    /// </summary>
    private static void ReportInternalsGrants(CompilationAnalysisContext context)
    {
        Compilation compilation = context.Compilation;

        foreach (IAssemblySymbol referenced in compilation.SourceModule.ReferencedAssemblySymbols)
        {
            if (!referenced.Name.StartsWith(CosmosAssemblyPrefix, System.StringComparison.Ordinal))
            {
                continue;
            }

            if (referenced.GivesAccessTo(compilation.Assembly))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticMessages.DriverAssemblyGrantedInternals,
                    Location.None,
                    referenced.Name));
            }
        }
    }
}
