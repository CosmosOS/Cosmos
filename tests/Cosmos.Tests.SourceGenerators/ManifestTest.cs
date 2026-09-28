// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Text;
using Cosmos.Kernel.SourceGenerators;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;

namespace Cosmos.Tests.SourceGenerators;

/// <summary>
/// One run of <see cref="CosmosEntryPointGenerator"/> over a kernel
/// compilation: the ring and kit stubs, a kernel class, the test's own
/// sources, and the build properties fed through a global analyzer config.
/// The harness fails when a generated file, or a diagnostic, differs from
/// what the test declared, or when the compilation does not build.
/// </summary>
internal sealed class ManifestTest : CSharpSourceGeneratorTest<CosmosEntryPointGenerator, DefaultVerifier>
{
    private const string ConfigPath = "/.globalconfig";

    private readonly StringBuilder _config = new("is_global = true\n");

    /// <summary>Creates a test whose compilation holds the ring stubs and the kernel class, and nothing else yet.</summary>
    /// <param name="includeKitStubs">Whether the kit stubs are compiled as sources; false when a test references them as an assembly instead.</param>
    public ManifestTest(bool includeKitStubs = true)
    {
        ReferenceAssemblies = ReferenceAssemblies.Net.Net90;
        TestState.Sources.Add(("/stubs/System.cs", KitStubs.System));
        TestState.Sources.Add(("/stubs/Kernel.cs", KitStubs.Kernel));
        if (includeKitStubs)
        {
            TestState.Sources.Add(("/stubs/DriverKit.cs", KitStubs.Hal("internal")));
        }
    }

    /// <summary>Sets <c>CosmosKernelClass</c>.</summary>
    /// <param name="kernelClass">The kernel type's full name.</param>
    public ManifestTest WithKernelClass(string kernelClass)
    {
        _config.Append("build_property.CosmosKernelClass = ").Append(kernelClass).Append('\n');
        return this;
    }

    /// <summary>Sets <c>CosmosDriverExcludeList</c>, as the SDK target joins the <c>CosmosDriverExclude</c> items.</summary>
    /// <param name="list">Comma-separated full type names.</param>
    public ManifestTest WithExcludeList(string list)
    {
        _config.Append("build_property.CosmosDriverExcludeList = ").Append(list).Append('\n');
        return this;
    }

    /// <summary>Sets <c>CosmosDriverIncludeList</c>, as the SDK target joins the <c>CosmosDriverInclude</c> items.</summary>
    /// <param name="list">Comma-separated full type names.</param>
    public ManifestTest WithIncludeList(string list)
    {
        _config.Append("build_property.CosmosDriverIncludeList = ").Append(list).Append('\n');
        return this;
    }

    /// <summary>Adds a source file of the kernel under test; the path is the first ordering key of its drivers.</summary>
    /// <param name="path">The file's path in the test compilation.</param>
    /// <param name="source">Its text, with <c>{|#n:...|}</c> markup for diagnostic locations.</param>
    public ManifestTest WithSource(string path, string source)
    {
        TestState.Sources.Add((path, source));
        return this;
    }

    /// <summary>Declares that <c>CosmosEntryPoint.g.cs</c> is generated for <paramref name="kernelClass"/>.</summary>
    /// <param name="kernelClass">The kernel type the entry point constructs.</param>
    public ManifestTest ExpectEntryPoint(string kernelClass)
    {
        TestState.GeneratedSources.Add((typeof(CosmosEntryPointGenerator), "CosmosEntryPoint.g.cs", SourceText.From(GeneratedText.EntryPoint(kernelClass), Encoding.UTF8)));
        return this;
    }

    /// <summary>Declares that <c>DriverManifest.g.cs</c> is generated with exactly <paramref name="statements"/> in its <c>Register</c> body.</summary>
    /// <param name="statements">Statements from <see cref="GeneratedText.Registration"/> and <see cref="GeneratedText.GuardedRegistration"/>.</param>
    public ManifestTest ExpectManifest(params string[] statements)
    {
        TestState.GeneratedSources.Add((typeof(CosmosEntryPointGenerator), "DriverManifest.g.cs", SourceText.From(GeneratedText.Manifest(statements), Encoding.UTF8)));
        return this;
    }

    /// <summary>Declares an expected diagnostic.</summary>
    /// <param name="diagnostic">Its id, severity, message and location.</param>
    public ManifestTest ExpectDiagnostic(DiagnosticResult diagnostic)
    {
        TestState.ExpectedDiagnostics.Add(diagnostic);
        return this;
    }

    /// <summary>Runs the generator and verifies everything declared.</summary>
    public Task RunAsync()
    {
        TestState.AnalyzerConfigFiles.Add((ConfigPath, _config.ToString()));
        return RunAsync(CancellationToken.None);
    }
}
