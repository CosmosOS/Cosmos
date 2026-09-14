using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.System.Graphics.Fonts;

namespace Cosmos.Kernel.System.Graphics;

/// <summary>
/// Cell-based graphics console for kernel output.
/// Uses a character grid (cells) similar to Aura OS for efficient terminal rendering.
/// Can be instantiated on any canvas (hardware or virtual).
/// </summary>
public class KernelConsole
{
    // The default (global) instance, created by Initialize()
    /// <summary>
    /// Gets the default (global) console instance, or <see langword="null"/>
    /// until <see cref="Initialize"/> has returned true. Test
    /// <see cref="IsInitialized"/> rather than this, which tells the compiler
    /// the same thing.
    /// </summary>
    public static KernelConsole? Default { get; private set; }

    // Lock for thread-safe console access
    private Cosmos.Kernel.Core.Scheduler.SpinLock _lock;

    private readonly Canvas _canvas;

    // Cursor position in character coordinates (column, row)
    private int _cursorX;
    private int _cursorY;

    // Terminal dimensions in characters
    private int _cols;
    private int _rows;

    // Character dimensions from font
    private int _charWidth;
    private int _charHeight;

    // Cell buffer - stores all characters and their colors
    private Cell[]? _cells;

    // Current colors
    private uint _foregroundColor = (uint)Color.White.ToArgb();
    private uint _backgroundColor = (uint)Color.Black.ToArgb();

    // Cursor visibility
    private bool _cursorVisible = true;
    private bool _cursorDrawn = false;

    // Console color palette (standard 16 colors)
    private static readonly uint[] s_palette =
    [
        0xFF000000, // Black
        0xFF000080, // DarkBlue
        0xFF008000, // DarkGreen
        0xFF008080, // DarkCyan
        0xFF800000, // DarkRed
        0xFF800080, // DarkMagenta
        0xFF808000, // DarkYellow
        0xFFC0C0C0, // Gray
        0xFF808080, // DarkGray
        0xFF0000FF, // Blue
        0xFF00FF00, // Green
        0xFF00FFFF, // Cyan
        0xFFFF0000, // Red
        0xFFFF00FF, // Magenta
        0xFFFFFF00, // Yellow
        0xFFFFFFFF  // White
    ];

    private Font _font;

    /// <summary>
    /// Creates a new KernelConsole on the given canvas.
    /// </summary>
    /// <param name="canvas">The canvas to render to.</param>
    /// <param name="font">The font to use (defaults to PCScreenFont.DefaultFont).</param>
    internal KernelConsole(Canvas canvas, Font? font = null)
    {
        _canvas = canvas;
        _font = font ?? PCScreenFont.DefaultFont;
        ApplyFontMetrics(_font);

        _cols = canvas.Width / _charWidth;
        _rows = canvas.Height / _charHeight;
        _cells = new Cell[_cols * _rows];

        ClearCells();
    }

    /// <summary>
    /// Throws when <see cref="Initialize"/> has not run yet, guaranteeing
    /// <see cref="Default"/> is non-null to callers that return normally.
    /// </summary>
    /// <exception cref="InvalidOperationException">The kernel console is not initialized.</exception>
    [MemberNotNull(nameof(Default))]
    internal static void ThrowIfKernelConsoleNotInitialized()
    {
        if (Default is null)
        {
            throw new InvalidOperationException($"{nameof(KernelConsole)} is not initialized");
        }
    }


    /// <summary>
    /// Derives the character cell size from a font. Bitmap fonts have a fixed
    /// cell; TrueType fonts report Width/Height as zero, so the cell is taken
    /// from the line metrics and the widest ASCII glyph at the font's SizePx.
    /// </summary>
    /// <param name="font">The font to measure.</param>
    /// <exception cref="ArgumentException">Thrown when no usable cell size can
    /// be derived, or when a cell does not fit the canvas. Either would leave
    /// a terminal with no cells at all, which every guard in this class reads
    /// as a live buffer because it tests for null, not for length.</exception>
    private void ApplyFontMetrics(Font font)
    {
        _charWidth = font.GetMaxAdvance();
        _charHeight = font.GetLineHeight();

        if (_charWidth <= 0 || _charHeight <= 0)
        {
            throw new ArgumentException($"Font provides no usable character cell ({_charWidth}x{_charHeight}).", nameof(font));
        }

        if (_charWidth > _canvas.Width || _charHeight > _canvas.Height)
        {
            throw new ArgumentException($"Font cell {_charWidth}x{_charHeight} does not fit the {_canvas.Width}x{_canvas.Height} canvas.", nameof(font));
        }
    }

    /// <summary>
    /// Gets or sets the font used in this console. Setting it resizes the
    /// terminal grid to the new cell size, clearing the screen and homing the
    /// cursor. Thread-safe.
    /// </summary>
    /// <exception cref="ArgumentException">The font yields no usable character
    /// cell, or a cell larger than the canvas.</exception>
    public Font Font
    {
        get => _font;
        set
        {
            // Under the lock, like every other mutator: this replaces the cell
            // buffer and both grid dimensions at once, and a concurrent write
            // indexes _cells with a position computed against the old grid.
            using (InternalCpu.DisableInterruptsScope())
            {
                _lock.Acquire();
                try
                {
                    ApplyFontMetrics(value);

                    _cursorX = 0;
                    _cursorY = 0;
                    _cursorDrawn = false;

                    _cols = _canvas.Width / _charWidth;
                    _rows = _canvas.Height / _charHeight;
                    _cells = new Cell[_cols * _rows];

                    ClearCells();

                    _canvas.Clear((int)_backgroundColor);
                    _canvas.Display();

                    _font = value;
                }
                finally
                {
                    _lock.Release();
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets the cursor X position (column). Thread-safe; a column
    /// outside the terminal is ignored.
    /// </summary>
    internal int CursorX
    {
        get => _cursorX;
        set
        {
            using (InternalCpu.DisableInterruptsScope())
            {
                _lock.Acquire();
                try
                {
                    if (value >= 0 && value < _cols)
                    {
                        SetCursorLocked(value, _cursorY);
                    }
                }
                finally
                {
                    _lock.Release();
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets the cursor Y position (row). Thread-safe; a row outside
    /// the terminal is ignored.
    /// </summary>
    internal int CursorY
    {
        get => _cursorY;
        set
        {
            using (InternalCpu.DisableInterruptsScope())
            {
                _lock.Acquire();
                try
                {
                    if (value >= 0 && value < _rows)
                    {
                        SetCursorLocked(_cursorX, value);
                    }
                }
                finally
                {
                    _lock.Release();
                }
            }
        }
    }

    /// <summary>
    /// Gets the number of columns in the terminal.
    /// </summary>
    public int Cols => _cols;

    /// <summary>
    /// Gets the number of rows in the terminal.
    /// </summary>
    public int Rows => _rows;

    /// <summary>
    /// Gets or sets whether the cursor is visible. Thread-safe.
    /// </summary>
    internal bool CursorVisible
    {
        get => _cursorVisible;
        set
        {
            using (InternalCpu.DisableInterruptsScope())
            {
                _lock.Acquire();
                try
                {
                    if (_cursorVisible != value)
                    {
                        if (_cursorVisible)
                        {
                            EraseCursor();
                        }

                        _cursorVisible = value;
                        if (_cursorVisible)
                        {
                            DrawCursor();
                        }
                    }
                }
                finally
                {
                    _lock.Release();
                }
            }
        }
    }

    /// <summary>
    /// Gets the canvas this console renders to.
    /// </summary>
    public Canvas Canvas => _canvas;

    /// <summary>
    /// Sets the foreground color from ConsoleColor enum.
    /// </summary>
    internal void SetForegroundColor(ConsoleColor color)
    {
        _foregroundColor = s_palette[(int)color];
    }

    /// <summary>
    /// Sets the background color from ConsoleColor enum.
    /// </summary>
    internal void SetBackgroundColor(ConsoleColor color)
    {
        _backgroundColor = s_palette[(int)color];
    }

    /// <summary>
    /// Converts ConsoleColor to uint color.
    /// </summary>
    internal static uint ConsoleColorToUint(ConsoleColor color)
    {
        return s_palette[(int)color];
    }

    /// <summary>
    /// Initializes the default (global) console on the hardware framebuffer.
    /// Idempotent: a second call leaves the existing console in place, so a
    /// kernel that overrides <see cref="Kernel.OnBoot"/> may call this whether
    /// or not it also called <c>base.OnBoot()</c>.
    /// </summary>
    /// <returns>True when <see cref="Default"/> is available, false when
    /// graphics are compiled out.</returns>
    [MemberNotNullWhen(true, nameof(Default))]
    public static bool Initialize()
    {
        if (!Core.CosmosFeatures.GraphicsEnabled)
        {
            return false;
        }

        if (Default is not null)
        {
            return true;
        }

        var canvas = Canvas.GetFullScreen();

        Default = new KernelConsole(canvas);

        /* Clear the Screen with the color 'Blue' */
        canvas.Clear(Color.Blue);

        // Clear screen
        canvas.Clear((int)Default._backgroundColor);
        canvas.Display();

        return true;
    }

    /// <summary>
    /// Gets whether the default console has been initialized. When true,
    /// <see cref="Default"/> is non-null.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Default))]
    public static bool IsInitialized => Default is not null;

    /// <summary>
    /// Gets the cell index for a given row and column.
    /// </summary>
    private int GetIndex(int row, int col)
    {
        return row * _cols + col;
    }

    /// <summary>
    /// Clears all cells to empty with current colors.
    /// </summary>
    private void ClearCells()
    {
        if (_cells is null)
        {
            return;
        }

        for (int i = 0; i < _cells.Length; i++)
        {
            _cells[i] = Cell.Empty(_foregroundColor, _backgroundColor);
        }
    }

    /// <summary>
    /// Sets the cursor position.
    /// Thread-safe.
    /// </summary>
    internal void SetCursorPosition(int x, int y)
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            _lock.Acquire();
            try
            {
                if (x >= 0 && x < _cols && y >= 0 && y < _rows)
                {
                    SetCursorLocked(x, y);
                }
            }
            finally
            {
                _lock.Release();
            }
        }
    }

    /// <summary>
    /// Moves the cursor and repaints it at the new cell. The caller holds
    /// <see cref="_lock"/> with interrupts disabled, and has already checked
    /// that the position is inside the terminal.
    /// </summary>
    private void SetCursorLocked(int x, int y)
    {
        EraseCursor();
        _cursorX = x;
        _cursorY = y;
        DrawCursor();
    }

    /// <summary>
    /// Moves the cursor by a relative offset, reading and writing the position
    /// in a single locked section. An offset that would leave the terminal is
    /// ignored.
    /// </summary>
    private void MoveCursorBy(int dx, int dy)
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            _lock.Acquire();
            try
            {
                int x = _cursorX + dx;
                int y = _cursorY + dy;
                if (x >= 0 && x < _cols && y >= 0 && y < _rows)
                {
                    SetCursorLocked(x, y);
                }
            }
            finally
            {
                _lock.Release();
            }
        }
    }

    /// <summary>
    /// Draws the cursor at the current position.
    /// </summary>
    private void DrawCursor()
    {
        if (!_cursorVisible || _cursorDrawn)
        {
            return;
        }

        // Draw cursor as an underline bar at the bottom of the character cell
        int pixelX = _cursorX * _charWidth;
        int pixelY = _cursorY * _charHeight + _charHeight - 2;

        _canvas.DrawFilledRectangle(Color.FromArgb((int)_foregroundColor), pixelX, pixelY, _charWidth, 2);
        _cursorDrawn = true;
    }

    /// <summary>
    /// Erases the cursor at the current position.
    /// </summary>
    private void EraseCursor()
    {
        if (!_cursorDrawn)
        {
            return;
        }

        // Erase cursor by redrawing background
        int pixelX = _cursorX * _charWidth;
        int pixelY = _cursorY * _charHeight + _charHeight - 2;

        // Get the background color of the current cell
        uint bgColor = _backgroundColor;
        if (_cells is not null && _cursorY < _rows && _cursorX < _cols)
        {
            int index = GetIndex(_cursorY, _cursorX);
            bgColor = _cells[index].BackgroundColor;
        }

        _canvas.DrawFilledRectangle(Color.FromArgb((int)bgColor), pixelX, pixelY, _charWidth, 2);
        _cursorDrawn = false;
    }

    /// <summary>
    /// Draws a character at a specific cell position.
    /// </summary>
    private void DrawCharAt(int col, int row)
    {
        if (_cells is null)
        {
            return;
        }

        int index = GetIndex(row, col);
        if (index < 0 || index >= _cells.Length)
        {
            return;
        }

        ref Cell cell = ref _cells[index];
        int pixelX = col * _charWidth;
        int pixelY = row * _charHeight;

        // Draw background
        _canvas.DrawFilledRectangle(Color.FromArgb((int)cell.BackgroundColor), pixelX, pixelY, _charWidth, _charHeight);

        // Draw character if not empty
        if (cell.Char != '\0' && cell.Char != '\n')
        {
            _canvas.DrawChar(cell.Char, Font, Color.FromArgb((int)cell.ForegroundColor), pixelX, pixelY);
        }
    }

    /// <summary>
    /// Internal redraw (must be called with lock held).
    /// </summary>
    private void RedrawInternal()
    {
        if (_cells is null)
        {
            return;
        }

        EraseCursor();

        // Clear screen with background color
        _canvas.Clear((int)_backgroundColor);

        // Draw all cells. DrawCharAt repaints the cell background from the
        // cell, not from the console's current one: a full repaint that used
        // _backgroundColor for every cell erased the per-cell colours that the
        // incremental painter had put there.
        for (int row = 0; row < _rows; row++)
        {
            for (int col = 0; col < _cols; col++)
            {
                DrawCharAt(col, row);
            }
        }

        DrawCursor();
    }

    /// <summary>
    /// Writes a character at the current cursor position.
    /// Thread-safe: uses spinlock with interrupt protection.
    /// </summary>
    internal void Write(char c)
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            if (_cells is null)
            {
                return;
            }

            _lock.Acquire();
            try
            {
                WriteInternal(c);
            }
            finally
            {
                _lock.Release();
            }
        }
    }

    /// <summary>
    /// Internal write implementation (must be called with lock held).
    /// </summary>
    private void WriteInternal(char c)
    {
        EraseCursor();

        switch (c)
        {
            case '\n':
                DoLineFeed();
                break;
            case '\r':
                DoCarriageReturn();
                break;
            case '\t':
                DoTabInternal();
                break;
            case '\b':
                DoBackspace();
                break;
            default:
                // Write character to cell buffer
                int index = GetIndex(_cursorY, _cursorX);
                _cells![index] = new Cell(c, _foregroundColor, _backgroundColor);

                // Draw the character
                DrawCharAt(_cursorX, _cursorY);

                // Advance cursor
                _cursorX++;
                if (_cursorX >= _cols)
                {
                    DoLineFeed();
                }
                break;
        }

        DrawCursor();
    }

    /// <summary>
    /// Internal tab (called with lock held, avoids recursive Write).
    /// </summary>
    private void DoTabInternal()
    {
        int spaces = 4 - (_cursorX % 4);
        for (int i = 0; i < spaces; i++)
        {
            WriteInternal(' ');
        }
    }

    /// <summary>
    /// Writes a string at the current cursor position.
    /// Thread-safe: uses spinlock with interrupt protection.
    /// </summary>
    internal void Write(string text)
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            if (_cells is null)
            {
                return;
            }

            _lock.Acquire();
            try
            {
                foreach (char c in text)
                {
                    WriteInternal(c);
                }
            }
            finally
            {
                _lock.Release();
            }
        }
    }

    /// <summary>
    /// Writes a Span of character at the current cursor position
    /// </summary>
    /// <param name="buffer">Span of characters to write</param>
    internal void Write(ReadOnlySpan<char> buffer)
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            if (_cells is null)
            {
                return;
            }

            _lock.Acquire();
            try
            {
                foreach (char c in buffer)
                {
                    WriteInternal(c);
                }
            }
            finally
            {
                _lock.Release();
            }
        }
    }

    /// <summary>
    /// Writes a newline.
    /// Thread-safe.
    /// </summary>
    internal void WriteLine()
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            _lock.Acquire();
            try
            {
                EraseCursor();
                DoLineFeed();
                DrawCursor();
            }
            finally
            {
                _lock.Release();
            }
        }
    }

    /// <summary>
    /// Performs a line feed (move to next line, column 0).
    /// Must be called with the lock held.
    /// </summary>
    private void DoLineFeed()
    {
        _cursorX = 0;
        _cursorY++;

        if (_cursorY >= _rows)
        {
            // Home the cursor before scrolling. Scroll repaints the screen,
            // which repaints the cursor, and an out-of-range row put it one
            // line below the canvas: clipped away, but still marked drawn, so
            // the caller's own repaint was then skipped and the caret vanished.
            _cursorY = _rows - 1;
            Scroll();
        }
    }

    /// <summary>
    /// Performs a carriage return (move to column 0).
    /// Must be called with the lock held.
    /// </summary>
    private void DoCarriageReturn()
    {
        _cursorX = 0;
    }

    /// <summary>
    /// Performs a backspace (move cursor back and clear character).
    /// Must be called with the lock held.
    /// </summary>
    private void DoBackspace()
    {
        if (_cursorX > 0)
        {
            _cursorX--;
        }
        else if (_cursorY > 0)
        {
            // Move to end of previous line
            _cursorY--;
            _cursorX = _cols - 1;
        }

        // Clear the character at cursor position
        int index = GetIndex(_cursorY, _cursorX);
        _cells![index] = Cell.Empty(_foregroundColor, _backgroundColor);
        DrawCharAt(_cursorX, _cursorY);
    }

    /// <summary>
    /// Moves the cursor left by one position. Thread-safe; a no-op in the
    /// first column.
    /// </summary>
    internal void MoveCursorLeft()
    {
        MoveCursorBy(-1, 0);
    }

    /// <summary>
    /// Moves the cursor right by one position. Thread-safe; a no-op in the
    /// last column.
    /// </summary>
    internal void MoveCursorRight()
    {
        MoveCursorBy(1, 0);
    }

    /// <summary>
    /// Scrolls the terminal up by one line.
    /// Must be called with lock held.
    /// </summary>
    private void Scroll()
    {
        if (_cells is null)
        {
            return;
        }

        // Shift all rows up by one
        for (int row = 0; row < _rows - 1; row++)
        {
            for (int col = 0; col < _cols; col++)
            {
                int currentIndex = GetIndex(row, col);
                int nextIndex = GetIndex(row + 1, col);
                _cells[currentIndex] = _cells[nextIndex];
            }
        }

        // Clear the last row
        for (int col = 0; col < _cols; col++)
        {
            int index = GetIndex(_rows - 1, col);
            _cells[index] = Cell.Empty(_foregroundColor, _backgroundColor);
        }

        // Redraw the entire screen (lock already held)
        RedrawInternal();
    }

    /// <summary>
    /// Clears the entire screen.
    /// Thread-safe.
    /// </summary>
    internal void Clear()
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            _lock.Acquire();
            try
            {
                EraseCursor();
                ClearCells();
                _canvas.Clear((int)_backgroundColor);
                _cursorX = 0;
                _cursorY = 0;
                DrawCursor();
            }
            finally
            {
                _lock.Release();
            }
        }
    }

    /// <summary>
    /// Resets colors to default (white on black).
    /// </summary>
    internal void ResetColors()
    {
        _foregroundColor = (uint)Color.White.ToArgb();
        _backgroundColor = (uint)Color.Black.ToArgb();
    }

}
