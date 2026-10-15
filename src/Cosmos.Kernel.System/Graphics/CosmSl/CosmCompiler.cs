// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;

namespace Cosmos.Kernel.System.Graphics.CosmSl;

/// <summary>
/// Entry point of the CosmSl shader compiler. <see cref="Parse"/> turns a
/// shader source string into a <see cref="ShaderProgramNode"/>; the 3D
/// drivers lower that tree to their native shader representation. A shader
/// can live in a .cosl file — read the file and hand its text to
/// <see cref="Parse"/>, the compiler only ever sees strings.
/// </summary>
public static class CosmCompiler
{
    /// <summary>
    /// Parses a CosmSl shader source into its abstract syntax tree.
    /// </summary>
    /// <param name="source">The shader source text (UTF-8 decoded).</param>
    /// <returns>The program root node, ready for a driver backend.</returns>
    /// <exception cref="CosmSlException">The source is not valid CosmSl.</exception>
    public static ShaderProgramNode Parse(string source)
    {
        return CosmSlParser.Parse(source);
    }
}

/// <summary>
/// Raised when a CosmSl source cannot be tokenized or parsed. Carries the
/// 1-based source line the problem was detected on.
/// </summary>
public class CosmSlException : Exception
{
    /// <summary>The 1-based line in the source the error was detected at.</summary>
    public int Line { get; }

    /// <summary>Creates a compile error for the given source line.</summary>
    /// <param name="message">The human readable problem description.</param>
    /// <param name="line">The 1-based line the problem was detected on.</param>
    public CosmSlException(string message, int line)
        : base($"CosmSl error on line {line}: {message}")
    {
        Line = line;
    }
}
