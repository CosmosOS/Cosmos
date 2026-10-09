// This code is licensed under the BSD 3-Clause license (see LICENSE for details)
using System.Collections.Immutable;
using Cosmos.Build.Analyzer.Patcher;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Cosmos.Tests.Build.Analyzer.Patcher;

/// <summary>
/// The layer analyzer judges a project on the types and members its code names. Every fake
/// assembly here declares a <c>Marker</c> type, and a test names the markers of the
/// assemblies it means to use; the compilation may reference more, as a real one does once
/// restore has added the transitive references, and the fake HAL carries an assembly
/// reference to the fake Core in its metadata, as the real one does.
/// </summary>
public class LayerAnalyzerTests
{
    private const string CoreName = "Cosmos.Kernel.Core";
    private const string HalName = "Cosmos.Kernel.HAL";
    private const string HalX64Name = "Cosmos.Kernel.HAL.X64";
    private const string HalArm64Name = "Cosmos.Kernel.HAL.ARM64";
    private const string SystemName = "Cosmos.Kernel.System";
    private const string NativeX64Name = "Cosmos.Kernel.Native.X64";

    private static readonly MetadataReference s_corlibReference =
        MetadataReference.CreateFromFile(typeof(object).Assembly.Location);

    // Built the way the tree is: the HAL against Core, the ring against the HAL.
    private static readonly MetadataReference s_nativeX64Reference = FakeAssembly.EmitWithMarker(NativeX64Name);
    private static readonly MetadataReference s_coreReference = FakeAssembly.EmitWithMarker(CoreName, s_nativeX64Reference);
    private static readonly MetadataReference s_halReference = FakeAssembly.Emit(
        HalName,
        $$"""
        namespace {{HalName}}
        {
            public class Marker
            {
                public static void Touch({{CoreName}}.Marker marker)
                {
                }
            }
        }
        """,
        s_coreReference);
    private static readonly MetadataReference s_halX64Reference = FakeAssembly.EmitWithMarker(HalX64Name, s_coreReference, s_halReference);
    private static readonly MetadataReference s_halArm64Reference = FakeAssembly.EmitWithMarker(HalArm64Name, s_coreReference, s_halReference);
    private static readonly MetadataReference s_systemReference = FakeAssembly.EmitWithMarker(SystemName, s_coreReference, s_halReference);

    /// <summary>The full reference list a kernel or a driver library ends up with after restore.</summary>
    private static readonly MetadataReference[] s_everyReference =
    [
        s_systemReference, s_halReference, s_halX64Reference, s_halArm64Reference, s_coreReference, s_nativeX64Reference
    ];

    /// <summary>Doc comments are parsed and bound, as they are in the tree (GenerateDocumentationFile is on).</summary>
    private static readonly CSharpParseOptions s_parseOptions =
        CSharpParseOptions.Default.WithDocumentationMode(DocumentationMode.Diagnose);

    [Theory]
    [InlineData("true")]
    [InlineData("True")]
    [InlineData("TRUE")]
    public async Task DriverAssembly_UsingHalAndSystem_NoLayerViolation(string driverAssemblyValue)
    {
        ImmutableArray<Diagnostic> diagnostics = await GetDiagnosticsAsync(
            "Cosmos.Kernel.Tests.Drivers.Library",
            s_everyReference,
            driverAssemblyValue,
            SystemName, HalName);

        Assert.DoesNotContain(diagnostics, d => d.Id == DiagnosticMessages.LayerViolation.Id);
    }

    [Fact]
    public async Task DriverAssembly_NamedCosmos_UsingCore_LayerViolation()
    {
        // A Cosmos.* name that is not a layer is skipped today; a driver assembly must not be.
        ImmutableArray<Diagnostic> diagnostics = await GetDiagnosticsAsync(
            "Cosmos.Kernel.Drivers",
            s_everyReference,
            "true",
            HalName, CoreName);

        Diagnostic diagnostic = Assert.Single(diagnostics, d => d.Id == DiagnosticMessages.LayerViolation.Id);
        Assert.Contains($"'{CoreName}'", diagnostic.GetMessage());
        Assert.Contains("'User'", diagnostic.GetMessage());
    }

    [Fact]
    public async Task DriverAssembly_TransitiveCoreReferenceItNeverBindsTo_NoLayerViolation()
    {
        // The reference list carries Core (the HAL was built against it); the code never names it.
        ImmutableArray<Diagnostic> diagnostics = await GetDiagnosticsAsync(
            "Cosmos.Kernel.Drivers",
            [s_halReference, s_coreReference],
            "true",
            HalName);

        Assert.DoesNotContain(diagnostics, d => d.Id == DiagnosticMessages.LayerViolation.Id);
    }

    [Theory]
    [InlineData(HalX64Name)]
    [InlineData(HalArm64Name)]
    public async Task DriverAssembly_UsingAnArchHalAssembly_LayerViolation(string usedName)
    {
        ImmutableArray<Diagnostic> diagnostics = await GetDiagnosticsAsync(
            "Cosmos.Kernel.Drivers",
            s_everyReference,
            "true",
            HalName, usedName);

        Diagnostic diagnostic = Assert.Single(diagnostics, d => d.Id == DiagnosticMessages.LayerViolation.Id);
        Assert.Contains($"'{usedName}'", diagnostic.GetMessage());
    }

    [Fact]
    public async Task DriverAssembly_NamedCosmos_WithoutTheProperty_IsSkipped()
    {
        // Without the marker the old rule holds: a Cosmos.* name outside the layers is not checked.
        ImmutableArray<Diagnostic> diagnostics = await GetDiagnosticsAsync(
            "Cosmos.Kernel.Drivers",
            s_everyReference,
            driverAssemblyValue: null,
            SystemName, CoreName);

        Assert.DoesNotContain(diagnostics, d => d.Id == DiagnosticMessages.LayerViolation.Id);
    }

    [Theory]
    [InlineData(SystemName)]
    [InlineData(HalName)]
    public async Task UserKernel_UsingTheRingOrTheSeam_NoLayerViolation(string usedName)
    {
        ImmutableArray<Diagnostic> diagnostics = await GetDiagnosticsAsync(
            "MyKernel",
            s_everyReference,
            driverAssemblyValue: null,
            SystemName, usedName);

        Assert.DoesNotContain(diagnostics, d => d.Id == DiagnosticMessages.LayerViolation.Id);
    }

    [Theory]
    [InlineData(HalX64Name)]
    [InlineData(CoreName)]
    public async Task UserKernel_UsingBelowTheSeam_LayerViolation(string usedName)
    {
        ImmutableArray<Diagnostic> diagnostics = await GetDiagnosticsAsync(
            "MyKernel",
            s_everyReference,
            driverAssemblyValue: null,
            SystemName, usedName);

        Diagnostic diagnostic = Assert.Single(diagnostics, d => d.Id == DiagnosticMessages.LayerViolation.Id);
        Assert.Contains($"'{usedName}'", diagnostic.GetMessage());
        Assert.Contains("'User'", diagnostic.GetMessage());
    }

    [Fact]
    public async Task UserKernel_TransitiveReferencesItNeverBindsTo_NoLayerViolation()
    {
        ImmutableArray<Diagnostic> diagnostics = await GetDiagnosticsAsync(
            "MyKernel",
            s_everyReference,
            driverAssemblyValue: null,
            SystemName);

        Assert.DoesNotContain(diagnostics, d => d.Id == DiagnosticMessages.LayerViolation.Id);
    }

    [Fact]
    public async Task UserKernel_UsingCore_ReportsOnceAtTheFirstUse()
    {
        const string code = """
            using Cosmos.Kernel.Core;

            namespace MyKernel
            {
                public class Kernel
                {
                    private readonly object _first = new Marker();
                    private readonly object _second = new Marker();

                    public static Marker Make() => new();
                }
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = await GetDiagnosticsForCodeAsync("MyKernel", s_everyReference, null, code);

        Diagnostic diagnostic = Assert.Single(diagnostics, d => d.Id == DiagnosticMessages.LayerViolation.Id);
        Assert.NotEqual(Location.None, diagnostic.Location);
        Assert.Equal("Marker", diagnostic.Location.SourceTree!.GetText().ToString(diagnostic.Location.SourceSpan));
        Assert.Equal(6, diagnostic.Location.GetLineSpan().StartLinePosition.Line);
    }

    [Fact]
    public async Task UserKernel_UsingDirectiveAlone_NoLayerViolation()
    {
        // A using directive names a namespace, not a type: nothing is reached until a type is.
        const string code = """
            using Cosmos.Kernel.Core;

            namespace MyKernel
            {
                public class Kernel
                {
                    private readonly object _used = new Cosmos.Kernel.System.Marker();
                }
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = await GetDiagnosticsForCodeAsync("MyKernel", s_everyReference, null, code);

        Assert.DoesNotContain(diagnostics, d => d.Id == DiagnosticMessages.LayerViolation.Id);
    }

    [Fact]
    public async Task UserKernel_CallingAHalMemberWithACoreParameter_NamesNothingFromCore()
    {
        // The HAL member's signature reaches Core; the kernel's code does not name Core.
        const string code = """
            namespace MyKernel
            {
                public class Kernel
                {
                    public static void Run() => Cosmos.Kernel.HAL.Marker.Touch(null!);
                }
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = await GetDiagnosticsForCodeAsync("MyKernel", s_everyReference, null, code);

        Assert.DoesNotContain(diagnostics, d => d.Id == DiagnosticMessages.LayerViolation.Id);
    }

    [Fact]
    public async Task UserKernel_PassingACoreObjectToAHalMember_LayerViolation()
    {
        const string code = """
            namespace MyKernel
            {
                public class Kernel
                {
                    public static void Run() => Cosmos.Kernel.HAL.Marker.Touch(new Cosmos.Kernel.Core.Marker());
                }
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = await GetDiagnosticsForCodeAsync("MyKernel", s_everyReference, null, code);

        Diagnostic diagnostic = Assert.Single(diagnostics, d => d.Id == DiagnosticMessages.LayerViolation.Id);
        Assert.Contains($"'{CoreName}'", diagnostic.GetMessage());
    }

    [Fact]
    public async Task UserKernel_UsingCoreInTwoFiles_ReportsOnceAtTheFirstFile()
    {
        // b.cs uses Core on its first line, a.cs further down: source order is by path first.
        ImmutableArray<Diagnostic> diagnostics = await GetDiagnosticsForTreesAsync(
            "MyKernel",
            s_everyReference,
            null,
            ("b.cs", "class B { object _b = new Cosmos.Kernel.Core.Marker(); }"),
            ("a.cs", "class A\n{\n    object _a = new Cosmos.Kernel.Core.Marker();\n}"));

        Diagnostic diagnostic = Assert.Single(diagnostics, d => d.Id == DiagnosticMessages.LayerViolation.Id);
        Assert.Equal("a.cs", diagnostic.Location.SourceTree!.FilePath);
        Assert.Equal(2, diagnostic.Location.GetLineSpan().StartLinePosition.Line);
    }

    [Fact]
    public async Task UserKernel_CrefToCore_NoLayerViolation()
    {
        // A cref binds the name but emits nothing: not a dependency.
        const string code = """
            namespace MyKernel
            {
                /// <summary>See <see cref="Cosmos.Kernel.Core.Marker"/> and <see cref="Cosmos.Kernel.System.Marker"/>.</summary>
                public class Kernel
                {
                    private readonly object _used = new Cosmos.Kernel.System.Marker();
                }
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = await GetDiagnosticsForCodeAsync("MyKernel", s_everyReference, null, code);

        Assert.DoesNotContain(diagnostics, d => d.Id == DiagnosticMessages.LayerViolation.Id);
    }

    [Fact]
    public async Task DriverAssembly_GeneratedFileUsingCore_LayerViolation()
    {
        // What a generator emits reaches a lower layer exactly as hand-written code would.
        const string generated = """
            // <auto-generated/>
            namespace Cosmos.Kernel.Drivers
            {
                internal static class Hatch
                {
                    public static object Make() => new Cosmos.Kernel.Core.Marker();
                }
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = await GetDiagnosticsForTreesAsync(
            "Cosmos.Kernel.Drivers",
            s_everyReference,
            "true",
            ("Hatch.g.cs", generated));

        Diagnostic diagnostic = Assert.Single(diagnostics, d => d.Id == DiagnosticMessages.LayerViolation.Id);
        Assert.Contains($"'{CoreName}'", diagnostic.GetMessage());
    }

    [Theory]
    [InlineData("Cosmos.Kernel.Plugs")]
    [InlineData("MyKernel.Plugs")]
    public async Task PlugAssembly_UsingCore_IsExempt(string assemblyName)
    {
        ImmutableArray<Diagnostic> diagnostics = await GetDiagnosticsAsync(
            assemblyName,
            s_everyReference,
            driverAssemblyValue: null,
            SystemName, CoreName, NativeX64Name);

        Assert.DoesNotContain(diagnostics, d => d.Id == DiagnosticMessages.LayerViolation.Id);
    }

    [Theory]
    [InlineData(HalX64Name, CoreName)]
    [InlineData(HalX64Name, HalName)]
    [InlineData(CoreName, NativeX64Name)]
    public async Task LowerLayer_UsingTheLayerBelow_NoLayerViolation(string assemblyName, string usedName)
    {
        ImmutableArray<Diagnostic> diagnostics = await GetDiagnosticsAsync(
            assemblyName,
            s_everyReference,
            driverAssemblyValue: null,
            usedName);

        Assert.DoesNotContain(diagnostics, d => d.Id == DiagnosticMessages.LayerViolation.Id);
    }

    [Theory]
    [InlineData(SystemName, CoreName, "System")]
    [InlineData(HalName, NativeX64Name, "Hal")]
    [InlineData(CoreName, HalName, "Core")]
    public async Task LowerLayer_SkippingALayer_LayerViolation(string assemblyName, string usedName, string layerName)
    {
        ImmutableArray<Diagnostic> diagnostics = await GetDiagnosticsAsync(
            assemblyName,
            s_everyReference,
            driverAssemblyValue: null,
            usedName);

        Diagnostic diagnostic = Assert.Single(diagnostics, d => d.Id == DiagnosticMessages.LayerViolation.Id);
        Assert.Contains($"'{usedName}'", diagnostic.GetMessage());
        Assert.Contains($"'{layerName}'", diagnostic.GetMessage());
    }

    /// <summary>
    /// A kernel class that binds to the <c>Marker</c> type of each named assembly, and to
    /// nothing else.
    /// </summary>
    private static string KernelUsing(params string[] usedAssemblyNames)
    {
        string fields = string.Concat(usedAssemblyNames.Select(
            (name, index) => $"        private readonly object _used{index} = new global::{name}.Marker();\n"));

        return $$"""
            namespace MyKernel
            {
                public class Kernel
                {
            {{fields}}    }
            }
            """;
    }

    private static Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(
        string assemblyName,
        MetadataReference[] cosmosReferences,
        string? driverAssemblyValue,
        params string[] usedAssemblyNames) =>
        GetDiagnosticsForCodeAsync(assemblyName, cosmosReferences, driverAssemblyValue, KernelUsing(usedAssemblyNames));

    private static Task<ImmutableArray<Diagnostic>> GetDiagnosticsForCodeAsync(
        string assemblyName,
        MetadataReference[] cosmosReferences,
        string? driverAssemblyValue,
        string code) =>
        GetDiagnosticsForTreesAsync(assemblyName, cosmosReferences, driverAssemblyValue, ("Kernel.cs", code));

    private static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsForTreesAsync(
        string assemblyName,
        MetadataReference[] cosmosReferences,
        string? driverAssemblyValue,
        params (string path, string code)[] files)
    {
        CSharpCompilation compilation = CSharpCompilation.Create(assemblyName)
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .AddReferences(s_corlibReference)
            .AddReferences(cosmosReferences)
            .AddSyntaxTrees(files.Select(f => CSharpSyntaxTree.ParseText(f.code, s_parseOptions, f.path)));

        Assert.Empty(compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));

        List<(string key, string value)> options = [("build_property.CosmosArch", "X64")];
        if (driverAssemblyValue is not null)
        {
            options.Add(("build_property.CosmosDriverAssembly", driverAssemblyValue));
        }

        LayerAnalyzer analyzer = new();
        CompilationWithAnalyzers compilationWithAnalyzers = compilation.WithAnalyzers(
            [analyzer],
            new AnalyzerOptions([], new AnalyzerTestConfigOptionsProvider(new AnalyzerTestConfigOptions([.. options]))));
        return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
    }
}
