// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.SourceGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace Cosmos.Tests.SourceGenerators;

/// <summary>
/// What the generators redo when the kernel changes between two runs of one
/// generator driver, as an incremental build and the IDE run them: an edit
/// to the kernel's drivers regenerates the manifest, and leaves the entry
/// point, which reads the build properties alone, cached.
/// </summary>
public sealed class CachingTests
{
    private const string KernelClassProperty = "build_property.CosmosKernelClass";

    private const string Driver = """
        using Cosmos.Kernel.HAL.DriverKit;

        namespace MyOS.Drivers
        {
            [Driver]
            internal sealed class BoardDriver : Driver
            {
                public override string Name => "board";
            }
        }
        """;

    [Fact]
    public async Task WhenADriverIsAdded_RegeneratesTheManifestAndKeepsTheEntryPoint()
    {
        ImmutableArray<MetadataReference> framework = await ReferenceAssemblies.Net.Net90.ResolveAsync(LanguageNames.CSharp, CancellationToken.None);
        CSharpCompilation compilation = CSharpCompilation.Create(
            "TestProject",
            [CSharpSyntaxTree.ParseText(KitStubs.System), CSharpSyntaxTree.ParseText(KitStubs.Kernel), CSharpSyntaxTree.ParseText(KitStubs.Hal("internal"))],
            framework,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new CosmosEntryPointGenerator().AsSourceGenerator(), new DriverManifestGenerator().AsSourceGenerator()],
            optionsProvider: new GlobalOptionsProvider(new BuildProperty(KernelClassProperty, KitStubs.KernelClass)),
            driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true));

        driver = driver.RunGenerators(compilation);
        driver = driver.RunGenerators(compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(Driver)));

        GeneratorDriverRunResult run = driver.GetRunResult();
        Assert.Equal(IncrementalStepRunReason.Cached, OutputReason(run, typeof(CosmosEntryPointGenerator)));
        Assert.Equal(IncrementalStepRunReason.Modified, OutputReason(run, typeof(DriverManifestGenerator)));
    }

    /// <summary>Why <paramref name="generator"/>'s source output was produced, or reused, on the driver's last run.</summary>
    /// <param name="run">The driver's last run.</param>
    /// <param name="generator">The generator, which has one source output.</param>
    private static IncrementalStepRunReason OutputReason(GeneratorDriverRunResult run, Type generator)
    {
        return run.Results
            .Single(result => result.Generator.GetGeneratorType() == generator)
            .TrackedOutputSteps[WellKnownGeneratorOutputs.SourceOutput]
            .SelectMany(step => step.Outputs)
            .Single()
            .Reason;
    }

    /// <summary>Hands the generators one build property, as the global analyzer config the SDK writes would.</summary>
    private sealed class GlobalOptionsProvider(AnalyzerConfigOptions globalOptions) : AnalyzerConfigOptionsProvider
    {
        public override AnalyzerConfigOptions GlobalOptions { get; } = globalOptions;

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
        {
            return BuildProperty.None;
        }

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
        {
            return BuildProperty.None;
        }
    }

    /// <summary>Options holding one key; <see cref="None"/> holds none.</summary>
    private sealed class BuildProperty(string key, string value) : AnalyzerConfigOptions
    {
        public static readonly BuildProperty None = new(string.Empty, string.Empty);

        public override bool TryGetValue(string name, [NotNullWhen(true)] out string? result)
        {
            result = key.Length != 0 && name == key ? value : null;
            return result is not null;
        }
    }
}
