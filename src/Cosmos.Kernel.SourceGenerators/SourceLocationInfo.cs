// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Cosmos.Kernel.SourceGenerators;

/// <summary>
/// A source location carried through the incremental pipeline by value.
/// A <see cref="Location"/> holds its syntax tree, which would keep every
/// compilation alive in the generator's cache; this record keeps only what
/// <see cref="Location.Create(string, TextSpan, LinePositionSpan)"/> needs
/// to rebuild one when a diagnostic is reported.
/// </summary>
/// <param name="FilePath">Path of the source file.</param>
/// <param name="Span">The character span in that file.</param>
/// <param name="LineSpan">The line and column span in that file.</param>
internal sealed record SourceLocationInfo(string FilePath, TextSpan Span, LinePositionSpan LineSpan)
{
    /// <summary>Captures <paramref name="location"/>, or returns null when it is not in a source file.</summary>
    /// <param name="location">A location from a syntax node or a symbol.</param>
    public static SourceLocationInfo? From(Location? location)
    {
        if (location is null || !location.IsInSource || location.SourceTree is null)
        {
            return null;
        }

        return new SourceLocationInfo(location.SourceTree.FilePath, location.SourceSpan, location.GetLineSpan().Span);
    }

    /// <summary>Rebuilds the <see cref="Location"/> for a diagnostic.</summary>
    public Location ToLocation() => Location.Create(FilePath, Span, LineSpan);
}
