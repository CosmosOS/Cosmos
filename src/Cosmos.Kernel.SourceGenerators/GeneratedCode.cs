// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.SourceGenerators;

/// <summary>The <c>[GeneratedCode]</c> arguments each generator stamps on the type it emits.</summary>
internal static class GeneratedCode
{
    /// <summary>The attribute's two arguments, naming <paramref name="generator"/> and its assembly version, as both languages spell them.</summary>
    /// <param name="generator">The generator that emits the type.</param>
    public static string Arguments(Type generator)
    {
        return $"\"{generator.FullName}\", \"{generator.Assembly.GetName().Version}\"";
    }
}
