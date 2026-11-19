// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Collections.Immutable;
using System.Text;
using Cosmos.Kernel.SourceGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Testing;

namespace Cosmos.Tests.SourceGenerators;

/// <summary>
/// One run of both generators over a Visual Basic kernel, the counterpart of
/// <see cref="ManifestTest"/>. A Visual Basic compilation cannot hold the C#
/// stubs, so the ring and the kit reach it as assemblies, as the real ones
/// reach a kernel. The kernel is compiled as an executable under the root
/// namespace every Visual Basic project has, with the startup object
/// Cosmos.Sdk sets, so the harness fails unless the generated <c>Main</c>
/// lands in <c>Cosmos.Kernel.System.Internal</c> itself rather than under
/// the root namespace.
/// </summary>
internal sealed class VisualBasicManifestTest : VisualBasicSourceGeneratorTest<CosmosEntryPointGenerator, DefaultVerifier>
{
    private const string HalAssemblyName = "Cosmos.Kernel.HAL";
    private const string RingAssemblyName = "Cosmos.Kernel.System";
    private const string StartupObject = "Cosmos.Kernel.System.Internal.CosmosEntryPoint";
    private const string ConfigPath = "/.globalconfig";

    /// <summary>The root namespace, the namespace of <see cref="KitStubs.KernelClass"/>.</summary>
    private const string RootNamespace = "MyOS";

    /// <summary>The kernel class, declared as a Visual Basic project declares it: in the root namespace, with no namespace block.</summary>
    private const string Kernel = """
        Public NotInheritable Class Kernel
            Inherits Cosmos.Kernel.System.Kernel
        End Class
        """;

    private readonly StringBuilder _config = new("is_global = true\n");

    /// <summary>The framework references, for compiling a driver library the kernel references.</summary>
    public ImmutableArray<MetadataReference> Framework { get; }

    /// <summary>The kit assembly the kernel references, for compiling a driver library against it.</summary>
    public MetadataReference Hal { get; }

    private VisualBasicManifestTest(ImmutableArray<MetadataReference> framework, MetadataReference hal, MetadataReference ring)
    {
        Framework = framework;
        Hal = hal;
        ReferenceAssemblies = ReferenceAssemblies.Net.Net90;
        TestState.AdditionalReferences.Add(hal);
        TestState.AdditionalReferences.Add(ring);
        TestState.Sources.Add(("/k/Kernel.vb", Kernel));
    }

    /// <summary>Compiles the ring and the kit, public, and creates a test whose compilation holds the kernel class and nothing else yet.</summary>
    public static async Task<VisualBasicManifestTest> CreateAsync()
    {
        ImmutableArray<MetadataReference> framework = await StubAssemblies.FrameworkAsync();
        MetadataReference hal = StubAssemblies.Compile(HalAssemblyName, framework, KitStubs.Hal("public"));
        MetadataReference ring = StubAssemblies.Compile(RingAssemblyName, framework, KitStubs.System);
        return new VisualBasicManifestTest(framework, hal, ring);
    }

    /// <summary>Sets <c>CosmosKernelClass</c>.</summary>
    /// <param name="kernelClass">The kernel type's full name.</param>
    public VisualBasicManifestTest WithKernelClass(string kernelClass)
    {
        _config.Append("build_property.CosmosKernelClass = ").Append(kernelClass).Append('\n');
        return this;
    }

    /// <summary>Adds a source file of the kernel under test; the path is the first ordering key of its drivers.</summary>
    /// <param name="path">The file's path in the test compilation.</param>
    /// <param name="source">Its text, with <c>{|#n:...|}</c> markup for diagnostic locations.</param>
    public VisualBasicManifestTest WithSource(string path, string source)
    {
        TestState.Sources.Add((path, source));
        return this;
    }

    /// <summary>Adds an assembly the kernel references, such as a driver library compiled against <see cref="Hal"/>.</summary>
    /// <param name="reference">The assembly.</param>
    public VisualBasicManifestTest WithReference(MetadataReference reference)
    {
        TestState.AdditionalReferences.Add(reference);
        return this;
    }

    /// <summary>Declares that <c>CosmosEntryPoint.g.vb</c> is generated for <paramref name="kernelClass"/>.</summary>
    /// <param name="kernelClass">The kernel type the entry point constructs.</param>
    public VisualBasicManifestTest ExpectEntryPoint(string kernelClass)
    {
        TestState.GeneratedSources.Add((typeof(CosmosEntryPointGenerator), "CosmosEntryPoint.g.vb", SourceText.From(GeneratedText.VisualBasicEntryPoint(kernelClass), Encoding.UTF8)));
        return this;
    }

    /// <summary>Declares that <c>DriverManifest.g.vb</c> is generated with exactly <paramref name="statements"/> in its <c>Register</c> body.</summary>
    /// <param name="statements">Statements from <see cref="GeneratedText.VisualBasicRegistration"/> and <see cref="GeneratedText.VisualBasicGuardedRegistration"/>.</param>
    public VisualBasicManifestTest ExpectManifest(params string[] statements)
    {
        TestState.GeneratedSources.Add((typeof(DriverManifestGenerator), "DriverManifest.g.vb", SourceText.From(GeneratedText.VisualBasicManifest(statements), Encoding.UTF8)));
        return this;
    }

    /// <summary>Declares an expected diagnostic.</summary>
    /// <param name="diagnostic">Its id, severity, message and location.</param>
    public VisualBasicManifestTest ExpectDiagnostic(DiagnosticResult diagnostic)
    {
        TestState.ExpectedDiagnostics.Add(diagnostic);
        return this;
    }

    /// <summary>Runs the generators and verifies everything declared.</summary>
    public Task RunAsync()
    {
        TestState.AnalyzerConfigFiles.Add((ConfigPath, _config.ToString()));
        return RunAsync(CancellationToken.None);
    }

    /// <summary>An executable under <see cref="RootNamespace"/> whose startup object is the generated entry point, as Cosmos.Sdk builds a kernel.</summary>
    protected override CompilationOptions CreateCompilationOptions()
    {
        VisualBasicCompilationOptions options = (VisualBasicCompilationOptions)base.CreateCompilationOptions();
        return options
            .WithOutputKind(OutputKind.ConsoleApplication)
            .WithMainTypeName(StartupObject)
            .WithRootNamespace(RootNamespace);
    }

    /// <summary>Both generators, in place of the base class's single type argument.</summary>
    protected override IEnumerable<Type> GetSourceGenerators()
    {
        return [typeof(CosmosEntryPointGenerator), typeof(DriverManifestGenerator)];
    }
}
