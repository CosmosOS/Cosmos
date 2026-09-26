// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Collections.Immutable;
using Cosmos.Build.Analyzer.Patcher;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Cosmos.Tests.Build.Analyzer.Patcher;

/// <summary>
/// The layer rules NAOT0007 enforces. Each case compiles a project of a given
/// assembly name against small in-memory assemblies named like the kernel's
/// layers, since the analyzer knows a layer by assembly name only.
/// </summary>
public class LayerAnalyzerTests
{
    private const string SystemAssembly = "Cosmos.Kernel.System";
    private const string HalAssembly = "Cosmos.Kernel.HAL";
    private const string HalInterfacesAssembly = "Cosmos.Kernel.HAL.Interfaces";
    private const string HalX64Assembly = "Cosmos.Kernel.HAL.X64";
    private const string CoreAssembly = "Cosmos.Kernel.Core";
    private const string NativeX64Assembly = "Cosmos.Kernel.Native.X64";

    private static readonly MetadataReference s_corlibReference =
        MetadataReference.CreateFromFile(typeof(object).Assembly.Location);

    [Theory]
    [InlineData(SystemAssembly)]
    [InlineData(HalAssembly)]
    [InlineData(HalInterfacesAssembly)]
    [InlineData(HalX64Assembly)]
    public async Task WhenUserKernelReferencesSystemOrHal_ReportsNothing(string referenced)
    {
        ImmutableArray<Diagnostic> diagnostics = await GetLayerDiagnosticsAsync("MyKernel", "", referenced);

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData(CoreAssembly)]
    [InlineData(NativeX64Assembly)]
    public async Task WhenUserKernelReferencesBelowHal_ReportsViolation(string referenced)
    {
        ImmutableArray<Diagnostic> diagnostics = await GetLayerDiagnosticsAsync("MyKernel", "", SystemAssembly, HalAssembly, referenced);

        Diagnostic violation = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticMessages.LayerViolation.Id, violation.Id);
        Assert.Contains($"'{referenced}'", violation.GetMessage());
        Assert.Contains("'User'", violation.GetMessage());
    }

    [Fact]
    public async Task WhenUserKernelImportsTheDriverKit_ReportsNothing()
    {
        const string code = """
            using Cosmos.Kernel.HAL.Drivers;
            using Cosmos.Kernel.System.Drivers;

            namespace MyKernel;

            public sealed class Driver
            {
                public Cosmos.Kernel.HAL.Drivers.Marker? Device { get; set; }
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = await GetLayerDiagnosticsAsync("MyKernel", code, SystemAssembly, HalAssembly);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task WhenUserKernelImportsCore_ReportsViolationAtTheUsing()
    {
        const string code = """
            using Cosmos.Kernel.Core.Drivers;

            namespace MyKernel;

            public sealed class Kernel
            {
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = await GetLayerDiagnosticsAsync("MyKernel", code, SystemAssembly, CoreAssembly);

        Diagnostic violation = Assert.Single(diagnostics);
        Assert.True(violation.Location.IsInSource);
        Assert.Equal(0, violation.Location.GetLineSpan().StartLinePosition.Line);
    }

    [Theory]
    [InlineData(SystemAssembly, CoreAssembly)]
    [InlineData(HalAssembly, SystemAssembly)]
    [InlineData(CoreAssembly, HalAssembly)]
    [InlineData(NativeX64Assembly, CoreAssembly)]
    public async Task WhenLayerReferencesOutsideItsRules_ReportsViolation(string current, string referenced)
    {
        ImmutableArray<Diagnostic> diagnostics = await GetLayerDiagnosticsAsync(current, "", referenced);

        Diagnostic violation = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticMessages.LayerViolation.Id, violation.Id);
    }

    [Theory]
    [InlineData(SystemAssembly, HalAssembly)]
    [InlineData(HalAssembly, CoreAssembly)]
    [InlineData(HalX64Assembly, HalAssembly)]
    [InlineData(CoreAssembly, NativeX64Assembly)]
    public async Task WhenLayerReferencesWithinItsRules_ReportsNothing(string current, string referenced)
    {
        ImmutableArray<Diagnostic> diagnostics = await GetLayerDiagnosticsAsync(current, "", referenced);

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("Cosmos.Kernel.Tests.Drivers")]
    [InlineData("Cosmos.Kernel.Plugs")]
    public async Task WhenCosmosAssemblyIsNoLayer_ReportsNothing(string current)
    {
        ImmutableArray<Diagnostic> diagnostics = await GetLayerDiagnosticsAsync(current, "", SystemAssembly, HalAssembly, CoreAssembly);

        Assert.Empty(diagnostics);
    }

    /// <summary>
    /// Runs the layer analyzer over a project named <paramref name="assemblyName"/>
    /// holding <paramref name="code"/> and referencing one in-memory assembly per
    /// name in <paramref name="references"/>, each declaring a public
    /// <c>Marker</c> class in the namespace <c>{name}.Drivers</c>.
    /// </summary>
    private static async Task<ImmutableArray<Diagnostic>> GetLayerDiagnosticsAsync(
        string assemblyName, string code, params string[] references)
    {
        List<MetadataReference> metadata = [s_corlibReference];
        foreach (string reference in references)
        {
            metadata.Add(CreateLayerAssembly(reference));
        }

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName,
            [CSharpSyntaxTree.ParseText(code)],
            metadata,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        CompilationWithAnalyzers withAnalyzers = compilation.WithAnalyzers([new LayerAnalyzer()]);
        return await withAnalyzers.GetAnalyzerDiagnosticsAsync();
    }

    private static MetadataReference CreateLayerAssembly(string assemblyName)
    {
        CSharpCompilation layer = CSharpCompilation.Create(
            assemblyName,
            [CSharpSyntaxTree.ParseText($"namespace {assemblyName}.Drivers {{ public class Marker {{ }} }}")],
            [s_corlibReference],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        return layer.ToMetadataReference();
    }
}
