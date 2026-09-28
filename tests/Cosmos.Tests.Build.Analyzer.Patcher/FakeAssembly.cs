// This code is licensed under the BSD 3-Clause license (see LICENSE for details)
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cosmos.Tests.Build.Analyzer.Patcher;

/// <summary>
/// Compiles a small library in memory and hands it back as a metadata reference, so a test
/// can reference an assembly by the name the analyzers key on (Cosmos.Kernel.HAL, a fake
/// Cosmos.Kernel.* with an InternalsVisibleTo grant) without a project on disk.
/// </summary>
internal static class FakeAssembly
{
    private static readonly MetadataReference s_corlibReference =
        MetadataReference.CreateFromFile(typeof(object).Assembly.Location);

    /// <summary>
    /// Emits a library named <paramref name="assemblyName"/> holding <paramref name="source"/>
    /// and returns a reference to the emitted image, as a build's ProjectReference would give.
    /// <paramref name="references"/> are the assemblies the library itself is built against,
    /// so a fake HAL can depend on a fake Core the way the real one does.
    /// </summary>
    public static MetadataReference Emit(string assemblyName, string source = "", params MetadataReference[] references)
    {
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName,
            [CSharpSyntaxTree.ParseText(source)],
            [s_corlibReference, .. references],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using MemoryStream stream = new();
        Microsoft.CodeAnalysis.Emit.EmitResult result = compilation.Emit(stream);
        if (!result.Success)
        {
            throw new InvalidOperationException(
                $"Fake assembly '{assemblyName}' did not compile: {string.Join("; ", result.Diagnostics)}");
        }

        return MetadataReference.CreateFromImage(stream.ToArray());
    }

    /// <summary>
    /// Emits a library named <paramref name="assemblyName"/> that declares one public
    /// <c>Marker</c> class in the namespace of the same name, so a test can bind to the
    /// assembly by naming that type.
    /// </summary>
    public static MetadataReference EmitWithMarker(string assemblyName, params MetadataReference[] references) =>
        Emit(assemblyName, $"namespace {assemblyName} {{ public class Marker {{ }} }}", references);
}
