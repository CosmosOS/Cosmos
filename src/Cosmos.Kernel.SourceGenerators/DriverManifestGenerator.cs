// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Cosmos.Kernel.SourceGenerators;

/// <summary>
/// Generates <c>DriverManifest.g.cs</c> (<c>.g.vb</c> in a Visual Basic
/// kernel), the list of <c>[Driver]</c> classes the kernel carries, guarded
/// by the feature switches they depend on and filtered by the project's
/// <c>CosmosDriverExclude</c> and <c>CosmosDriverInclude</c> items. The
/// <c>Main</c> <see cref="CosmosEntryPointGenerator"/> writes calls it before
/// the kernel starts. Nothing is generated when the <c>CosmosKernelClass</c>
/// build property is empty, since no entry point calls it then; the manifest
/// is generated, with an empty <c>Register</c>, when no driver survives.
/// </summary>
[Generator(LanguageNames.CSharp, LanguageNames.VisualBasic)]
public sealed class DriverManifestGenerator : IIncrementalGenerator
{
    private const string HintName = "DriverManifest";

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValueProvider<GeneratorOptions> options = GeneratorOptions.Provider(context);

        // Any declaration carrying the attribute is a candidate; DriverDiscovery
        // drops one that is not a type, which the attribute's usage already
        // makes a compile error. Testing the symbol, not the syntax, keeps the
        // pipeline the same in both languages.
        IncrementalValueProvider<EquatableArray<DriverCandidate>> sourceDrivers = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                DriverDiscovery.DriverAttributeMetadataName,
                static (_, _) => true,
                static (syntaxContext, cancellationToken) => DriverDiscovery.FromSource(syntaxContext, cancellationToken))
            .Collect()
            .Select(static (candidates, _) => new EquatableArray<DriverCandidate>(candidates.OfType<DriverCandidate>().ToImmutableArray()));

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

        List<DriverCandidate> registered = DriverManifestBuilder.Build(context, options, sourceDrivers, referencedDrivers);
        SourceWriter writer = SourceWriter.For(options.Language);
        context.AddSource($"{HintName}{writer.FileExtension}", SourceText.From(writer.Manifest(registered), Encoding.UTF8));
    }
}
