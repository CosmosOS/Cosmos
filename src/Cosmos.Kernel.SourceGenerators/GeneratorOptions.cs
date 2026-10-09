// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Cosmos.Kernel.SourceGenerators;

/// <summary>
/// What the generators read from the build: the kernel compilation's language,
/// and the build properties made visible to them by the
/// <c>CompilerVisibleProperty</c> items in <c>build/Cosmos.Kernel.SourceGenerators.props</c>.
/// </summary>
/// <param name="Language">The compilation's <see cref="LanguageNames"/> value, which picks the <see cref="SourceWriter"/>.</param>
/// <param name="KernelClass"><c>CosmosKernelClass</c>: the kernel type the entry point constructs; nothing is generated without it.</param>
/// <param name="ExcludeList"><c>CosmosDriverExcludeList</c>: comma-separated full names of drivers to leave out.</param>
/// <param name="IncludeList"><c>CosmosDriverIncludeList</c>: comma-separated full names of opt-in drivers to register.</param>
internal sealed record GeneratorOptions(string Language, string? KernelClass, string ExcludeList, string IncludeList)
{
    private const string KernelClassProperty = "build_property.CosmosKernelClass";
    private const string ExcludeListProperty = "build_property.CosmosDriverExcludeList";
    private const string IncludeListProperty = "build_property.CosmosDriverIncludeList";

    /// <summary>
    /// The options as a pipeline value. It follows the global analyzer options
    /// and the parse options, neither of which an edit to the kernel's sources
    /// changes.
    /// </summary>
    /// <param name="context">The generator's initialization context.</param>
    public static IncrementalValueProvider<GeneratorOptions> Provider(IncrementalGeneratorInitializationContext context)
    {
        return context.AnalyzerConfigOptionsProvider
            .Combine(context.ParseOptionsProvider)
            .Select(static (value, _) => From(value.Right.Language, value.Left.GlobalOptions));
    }

    /// <summary>Reads the three properties from the global analyzer options.</summary>
    /// <param name="language">The compilation's language.</param>
    /// <param name="options">The compilation's global options.</param>
    private static GeneratorOptions From(string language, AnalyzerConfigOptions options)
    {
        string? kernelClass = options.TryGetValue(KernelClassProperty, out string? kernelValue) && !string.IsNullOrWhiteSpace(kernelValue)
            ? kernelValue.Trim()
            : null;
        string excludeList = options.TryGetValue(ExcludeListProperty, out string? excludeValue) ? excludeValue : string.Empty;
        string includeList = options.TryGetValue(IncludeListProperty, out string? includeValue) ? includeValue : string.Empty;
        return new GeneratorOptions(language, kernelClass, excludeList, includeList);
    }
}
