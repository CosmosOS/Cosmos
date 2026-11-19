// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Cosmos.Kernel.SourceGenerators;

/// <summary>
/// Generates <c>CosmosEntryPoint.g.cs</c> (<c>.g.vb</c> in a Visual Basic
/// kernel), the kernel's <c>Main</c>: it registers the drivers
/// <see cref="DriverManifestGenerator"/> lists, then constructs the kernel
/// the <c>CosmosKernelClass</c> build property names and starts it. Nothing
/// is generated when the property is empty. The pipeline reads that property
/// and the kernel's language alone, so an edit to the kernel's sources or to
/// its driver policy leaves the generated file cached.
/// </summary>
[Generator(LanguageNames.CSharp, LanguageNames.VisualBasic)]
public sealed class CosmosEntryPointGenerator : IIncrementalGenerator
{
    private const string HintName = "CosmosEntryPoint";

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValueProvider<(string Language, string? KernelClass)> input = GeneratorOptions.Provider(context)
            .Select(static (options, _) => (options.Language, options.KernelClass));

        context.RegisterSourceOutput(input, static (productionContext, value) =>
        {
            if (value.KernelClass is not null)
            {
                SourceWriter writer = SourceWriter.For(value.Language);
                productionContext.AddSource($"{HintName}{writer.FileExtension}", SourceText.From(writer.EntryPoint(value.KernelClass), Encoding.UTF8));
            }
        });
    }
}
