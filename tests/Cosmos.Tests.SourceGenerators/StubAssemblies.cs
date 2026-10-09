// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace Cosmos.Tests.SourceGenerators;

/// <summary>
/// Compiles C# stubs into in-memory libraries a kernel compilation under test
/// references, the way the kit, the ring and driver libraries reach a real
/// kernel: as assemblies, whatever language the kernel is written in.
/// </summary>
internal static class StubAssemblies
{
    /// <summary>The reference assemblies of the framework the stubs and the kernels compile against.</summary>
    public static Task<ImmutableArray<MetadataReference>> FrameworkAsync()
    {
        return ReferenceAssemblies.Net.Net90.ResolveAsync(LanguageNames.CSharp, CancellationToken.None);
    }

    /// <summary>Compiles <paramref name="sources"/> into an in-memory library named <paramref name="assemblyName"/>, failing the test on any compile error.</summary>
    /// <param name="assemblyName">The library's simple name, which the generator and the grants match on.</param>
    /// <param name="references">The framework and the libraries it references.</param>
    /// <param name="sources">One compilation unit per string.</param>
    public static MetadataReference Compile(string assemblyName, ImmutableArray<MetadataReference> references, params string[] sources)
    {
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName,
            sources.Select(source => CSharpSyntaxTree.ParseText(source)),
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using MemoryStream stream = new();
        EmitResult result = compilation.Emit(stream);
        Assert.True(result.Success, string.Join("\n", result.Diagnostics));
        return MetadataReference.CreateFromImage(stream.ToArray());
    }
}
