// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Cosmos.Kernel.SourceGenerators;

/// <summary>
/// Generates <c>DriverManifest.g.cs</c>, the list of <c>[Driver]</c> classes
/// the kernel carries, guarded by the feature switches they depend on and
/// filtered by the project's <c>CosmosDriverExclude</c> and
/// <c>CosmosDriverInclude</c> items. The <c>Main</c>
/// <see cref="CosmosEntryPointGenerator"/> writes calls it before the kernel
/// starts. Nothing is generated when the <c>CosmosKernelClass</c> build
/// property is empty, since no entry point calls it then; the manifest is
/// generated, with an empty <c>Register</c>, when no driver survives.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class DriverManifestGenerator : IIncrementalGenerator
{
    private const string HintName = "DriverManifest.g.cs";

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValueProvider<GeneratorOptions> options = context.AnalyzerConfigOptionsProvider
            .Select(static (provider, _) => GeneratorOptions.From(provider.GlobalOptions));

        IncrementalValueProvider<EquatableArray<DriverCandidate>> sourceDrivers = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                DriverDiscovery.DriverAttributeMetadataName,
                static (node, _) => node is ClassDeclarationSyntax or RecordDeclarationSyntax,
                static (syntaxContext, cancellationToken) => DriverDiscovery.FromSource(syntaxContext, cancellationToken))
            .Collect()
            .Select(static (candidates, _) => new EquatableArray<DriverCandidate>(candidates));

        IncrementalValueProvider<EquatableArray<DriverCandidate>> referencedDrivers = context.CompilationProvider
            .Select(static (compilation, cancellationToken) => DriverDiscovery.FromReferences(compilation, cancellationToken));

        IncrementalValueProvider<((GeneratorOptions Options, EquatableArray<DriverCandidate> Source) Left, EquatableArray<DriverCandidate> Referenced)> input =
            options.Combine(sourceDrivers).Combine(referencedDrivers);

        context.RegisterSourceOutput(input, static (productionContext, value) =>
            Emit(productionContext, value.Left.Options, value.Left.Source, value.Referenced));
    }

    private static void Emit(
        SourceProductionContext context,
        GeneratorOptions options,
        EquatableArray<DriverCandidate> sourceDrivers,
        EquatableArray<DriverCandidate> referencedDrivers)
    {
        if (options.KernelClass is null)
        {
            return;
        }

        string manifest = DriverManifestBuilder.Build(context, options, sourceDrivers, referencedDrivers, GeneratedCode.Attribute(typeof(DriverManifestGenerator)));
        context.AddSource(HintName, SourceText.From(manifest, Encoding.UTF8));
    }
}
