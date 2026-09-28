// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Microsoft.CodeAnalysis.Diagnostics;

namespace Cosmos.Kernel.SourceGenerators;

/// <summary>
/// The build properties the generator reads, made visible to it by the
/// <c>CompilerVisibleProperty</c> items in <c>build/Cosmos.Kernel.SourceGenerators.props</c>.
/// </summary>
/// <param name="KernelClass"><c>CosmosKernelClass</c>: the kernel type the entry point constructs; nothing is generated without it.</param>
/// <param name="ExcludeList"><c>CosmosDriverExcludeList</c>: comma-separated full names of drivers to leave out.</param>
/// <param name="IncludeList"><c>CosmosDriverIncludeList</c>: comma-separated full names of opt-in drivers to register.</param>
internal sealed record GeneratorOptions(string? KernelClass, string ExcludeList, string IncludeList)
{
    private const string KernelClassProperty = "build_property.CosmosKernelClass";
    private const string ExcludeListProperty = "build_property.CosmosDriverExcludeList";
    private const string IncludeListProperty = "build_property.CosmosDriverIncludeList";

    /// <summary>Reads the three properties from the global analyzer options.</summary>
    /// <param name="options">The compilation's global options.</param>
    public static GeneratorOptions From(AnalyzerConfigOptions options)
    {
        string? kernelClass = options.TryGetValue(KernelClassProperty, out string? kernelValue) && !string.IsNullOrWhiteSpace(kernelValue)
            ? kernelValue.Trim()
            : null;
        string excludeList = options.TryGetValue(ExcludeListProperty, out string? excludeValue) ? excludeValue : string.Empty;
        string includeList = options.TryGetValue(IncludeListProperty, out string? includeValue) ? includeValue : string.Empty;
        return new GeneratorOptions(kernelClass, excludeList, includeList);
    }
}
