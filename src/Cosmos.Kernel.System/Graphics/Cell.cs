// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.Graphics;

/// <summary>
/// Represents a single cell in the terminal grid.
/// Each cell contains a character and its associated colors.
/// </summary>
internal struct Cell
{
    /// <summary>
    /// The character displayed in this cell.
    /// </summary>
    public readonly char Char;

    /// <summary>
    /// The foreground (text) color of this cell.
    /// </summary>
    public readonly uint ForegroundColor;

    /// <summary>
    /// The background color of this cell.
    /// </summary>
    public readonly uint BackgroundColor;

    /// <summary>
    /// Creates a new cell with the specified character and colors.
    /// </summary>
    public Cell(char c, uint foreground, uint background)
    {
        Char = c;
        ForegroundColor = foreground;
        BackgroundColor = background;
    }

    /// <summary>
    /// Creates an empty cell (a null character) in the given colors.
    /// </summary>
    public static Cell Empty(uint foreground, uint background) => new('\0', foreground, background);
}
