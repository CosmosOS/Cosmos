using System;
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
    private static KernelConsole? _default;

    /// <summary>
    /// Gets the default (global) console instance.
    /// </summary>
    public static KernelConsole? Default => _default;

    // Lock for thread-safe console access
    private Cosmos.Kernel.Core.Scheduler.SpinLock _lock;

    private Canvas _canvas;

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
    private static readonly uint[] _palette = new uint[16]
    {
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
    };

    private Font _font;

    /// <summary>
    /// Creates a new KernelConsole on the given canvas.
    /// </summary>
    /// <param name="canvas">The canvas to render to.</param>
    /// <param name="font">The font to use (defaults to PCScreenFont.DefaultFont).</param>
    public KernelConsole(Canvas canvas, Font? font = null)
    {
        _canvas = canvas;
        _font = font ?? PCScreenFont.DefaultFont;
        _charWidth = _font.Width;
        _charHeight = _font.Height;

        _cols = canvas.Width / _charWidth;
        _rows = canvas.Height / _charHeight;
        _cells = new Cell[_cols * _rows];

        ClearCells();
    }

    /// <summary>
    /// Gets whether this console is available (has a valid canvas).
    /// </summary>
    public bool IsAvailable => _canvas != null;

    /// <summary>
    /// Pixel rect of the cursor's current row (full width, one character tall).
    /// A caller that just wrote a single character with no line wrap/scroll (the
    /// common case — see ConsolePlug.ReadKey) can present just this rect via
    /// <see cref="Canvas"/>.Display(x,y,w,h) instead of the whole screen.
    /// </summary>
    public (int x, int y, int width, int height) CursorRowRect =>
        (0, _cursorY * _charHeight, _cols * _charWidth, _charHeight);

    /// <summary>
    /// Gets or sets the font used in this console.
    /// </summary>
    public Font Font
    {
        get => _font;
        set
        {
            _charWidth = value.Width;
            _charHeight = value.Height;

            CursorX = 0;
            CursorY = 0;

            _cols = _canvas.Width / _charWidth;
            _rows = _canvas.Height / _charHeight;
            _cells = new Cell[_cols * _rows];

            ClearCells();

            _canvas.Clear((int)_backgroundColor);
            _canvas.Display();

            _font = value;
        }
    }

    /// <summary>
    /// Gets or sets the cursor X position (column).
    /// </summary>
    public int CursorX
    {
        get => _cursorX;
        set
        {
            if (value >= 0 && value < _cols)
            {
                EraseCursor();
                _cursorX = value;
                DrawCursor();
            }
        }
    }

    /// <summary>
    /// Gets or sets the cursor Y position (row).
    /// </summary>
    public int CursorY
    {
        get => _cursorY;
        set
        {
            if (value >= 0 && value < _rows)
            {
                EraseCursor();
                _cursorY = value;
                DrawCursor();
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
    /// Gets or sets whether the cursor is visible.
    /// </summary>
    public bool CursorVisible
    {
        get => _cursorVisible;
        set
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
    }

    /// <summary>
    /// Gets or sets the foreground color.
    /// </summary>
    public uint ForegroundColor
    {
        get => _foregroundColor;
        set => _foregroundColor = value;
    }

    /// <summary>
    /// Gets or sets the background color.
    /// </summary>
    public uint BackgroundColor
    {
        get => _backgroundColor;
        set => _backgroundColor = value;
    }

    /// <summary>
    /// Gets the canvas this console renders to.
    /// </summary>
    public Canvas Canvas => _canvas;

    /// <summary>
    /// Sets the foreground color from ConsoleColor enum.
    /// </summary>
    public void SetForegroundColor(ConsoleColor color)
    {
        _foregroundColor = _palette[(int)color];
    }

    /// <summary>
    /// Sets the background color from ConsoleColor enum.
    /// </summary>
    public void SetBackgroundColor(ConsoleColor color)
    {
        _backgroundColor = _palette[(int)color];
    }

    /// <summary>
    /// Converts ConsoleColor to uint color.
    /// </summary>
    public static uint ConsoleColorToUint(ConsoleColor color)
    {
        return _palette[(int)color];
    }

    /// <summary>
    /// Initializes the default (global) console on the hardware framebuffer.
    /// </summary>
    public static bool Initialize()
    {
        if (!Core.CosmosFeatures.GraphicsEnabled)
        {
            return false;
        }

        if (_default != null)
        {
            return false;
        }

        var canvas = Canvas.GetFullScreen();

        _default = new KernelConsole(canvas);

        /* Clear the Screen with the color 'Blue' */
        canvas.Clear(Color.Blue);

        // Clear screen
        canvas.Clear((int)_default._backgroundColor);
        canvas.Display();

        return true;
    }

    /// <summary>
    /// Gets whether the default console has been initialized.
    /// </summary>
    public static bool IsInitialized => _default != null;

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
        if (_cells == null)
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
    public void SetCursorPosition(int x, int y)
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            if (x >= 0 && x < _cols && y >= 0 && y < _rows)
            {
                _lock.Acquire();
                try
                {
                    EraseCursor();
                    _cursorX = x;
                    _cursorY = y;
                    DrawCursor();
                }
                finally
                {
                    _lock.Release();
                }
            }
        }
    }

    /// <summary>
    /// Draws the cursor at the current position.
    /// </summary>
    private void DrawCursor()
    {
        if (!IsAvailable || !_cursorVisible || _cursorDrawn)
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
        if (!IsAvailable || !_cursorDrawn)
        {
            return;
        }

        // Erase cursor by redrawing background
        int pixelX = _cursorX * _charWidth;
        int pixelY = _cursorY * _charHeight + _charHeight - 2;

        // Get the background color of the current cell
        uint bgColor = _backgroundColor;
        if (_cells != null && _cursorY < _rows && _cursorX < _cols)
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
        if (!IsAvailable || _cells == null)
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
    /// Redraws the entire screen from the cell buffer.
    /// Thread-safe.
    /// </summary>
    public void Redraw()
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            if (!IsAvailable || _cells == null)
            {
                return;
            }

            _lock.Acquire();
            try
            {
                RedrawInternal();
            }
            finally
            {
                _lock.Release();
            }
        }
    }

    /// <summary>
    /// Internal redraw (must be called with lock held).
    /// </summary>
    private void RedrawInternal()
    {
        if (_cells == null)
        {
            return;
        }

        EraseCursor();

        // Clear screen with background color
        _canvas.Clear((int)_backgroundColor);

        // Draw all cells
        for (int row = 0; row < _rows; row++)
        {
            for (int col = 0; col < _cols; col++)
            {
                int index = GetIndex(row, col);
                ref Cell cell = ref _cells[index];

                if (cell.Char != '\0' && cell.Char != '\n')
                {
                    int pixelX = col * _charWidth;
                    int pixelY = row * _charHeight;
                    _canvas.DrawChar(cell.Char, Font, Color.FromArgb((int)cell.ForegroundColor), pixelX, pixelY);
                }
            }
        }

        DrawCursor();
    }

    /// <summary>
    /// Writes a character at the current cursor position.
    /// Thread-safe: uses spinlock with interrupt protection.
    /// </summary>
    public void Write(char c)
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            if (!IsAvailable || _cells == null)
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
    public void Write(string text)
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            if (!IsAvailable || _cells == null)
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
    public void Write(ReadOnlySpan<char> buffer)
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            if (!IsAvailable || _cells == null)
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
    /// Writes a character followed by a newline.
    /// Thread-safe.
    /// </summary>
    public void WriteLine(char c)
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            if (!IsAvailable || _cells == null)
            {
                return;
            }

            _lock.Acquire();
            try
            {
                WriteInternal(c);
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
    /// Writes a string followed by a newline.
    /// Thread-safe.
    /// </summary>
    public void WriteLine(string text)
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            if (!IsAvailable || _cells == null)
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
    /// Writes a newline.
    /// Thread-safe.
    /// </summary>
    public void WriteLine()
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            if (!IsAvailable)
            {
                return;
            }

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
    /// </summary>
    private void DoLineFeed()
    {
        _cursorX = 0;
        _cursorY++;

        if (_cursorY >= _rows)
        {
            Scroll();
            _cursorY = _rows - 1;
        }
    }

    /// <summary>
    /// Performs a carriage return (move to column 0).
    /// </summary>
    private void DoCarriageReturn()
    {
        _cursorX = 0;
    }

    /// <summary>
    /// Performs a backspace (move cursor back and clear character).
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
    /// Moves the cursor left by one position.
    /// </summary>
    public void MoveCursorLeft()
    {
        if (_cursorX > 0)
        {
            EraseCursor();
            _cursorX--;
            DrawCursor();
        }
    }

    /// <summary>
    /// Moves the cursor right by one position.
    /// </summary>
    public void MoveCursorRight()
    {
        if (_cursorX < _cols - 1)
        {
            EraseCursor();
            _cursorX++;
            DrawCursor();
        }
    }

    /// <summary>
    /// Moves the cursor up by one position.
    /// </summary>
    public void MoveCursorUp()
    {
        if (_cursorY > 0)
        {
            EraseCursor();
            _cursorY--;
            DrawCursor();
        }
    }

    /// <summary>
    /// Moves the cursor down by one position.
    /// </summary>
    public void MoveCursorDown()
    {
        if (_cursorY < _rows - 1)
        {
            EraseCursor();
            _cursorY++;
            DrawCursor();
        }
    }

    /// <summary>
    /// Scrolls the terminal up by one line.
    /// Must be called with lock held.
    /// </summary>
    private void Scroll()
    {
        if (_cells == null)
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
    public void Clear()
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            if (!IsAvailable)
            {
                return;
            }

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
    public void ResetColors()
    {
        _foregroundColor = (uint)Color.White.ToArgb();
        _backgroundColor = (uint)Color.Black.ToArgb();
    }

    /// <summary>
    /// Gets the character at the specified position.
    /// </summary>
    public char GetCharAt(int col, int row)
    {
        if (_cells == null || col < 0 || col >= _cols || row < 0 || row >= _rows)
        {
            return '\0';
        }

        int index = GetIndex(row, col);
        return _cells[index].Char;
    }

    /// <summary>
    /// Gets the cell at the specified position.
    /// </summary>
    public Cell GetCellAt(int col, int row)
    {
        if (_cells == null || col < 0 || col >= _cols || row < 0 || row >= _rows)
        {
            return Cell.Empty(_foregroundColor, _backgroundColor);
        }

        int index = GetIndex(row, col);
        return _cells[index];
    }

    /// <summary>
    /// Sets the cell at the specified position.
    /// </summary>
    public void SetCellAt(int col, int row, Cell cell)
    {
        if (_cells == null || col < 0 || col >= _cols || row < 0 || row >= _rows)
        {
            return;
        }

        int index = GetIndex(row, col);
        _cells[index] = cell;
        DrawCharAt(col, row);
    }
}
