// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.SourceGenerators;

/// <summary>The <c>[GeneratedCode]</c> line each generator stamps on the class it emits.</summary>
internal static class GeneratedCode
{
    /// <summary>The attribute line naming <paramref name="generator"/> and its assembly version.</summary>
    /// <param name="generator">The generator that emits the class.</param>
    public static string Attribute(Type generator)
    {
        return $"[global::System.CodeDom.Compiler.GeneratedCode(\"{generator.FullName}\", \"{generator.Assembly.GetName().Version}\")]";
    }
}
