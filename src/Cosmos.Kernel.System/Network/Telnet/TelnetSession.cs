// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Buffers;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.Keyboard;
using Cosmos.Kernel.System.Network.TCP;
using Cosmos.Kernel.System.Sessions;

namespace Cosmos.Kernel.System.Network.Telnet;

/// <summary>
/// The console session of one Telnet connection. What is written goes to
/// the session's screen, like any session's, and to the client as the
/// characters and VT100 sequences that make its terminal show the same
/// thing; the keys come from what the client sends.
/// </summary>
/// <remarks>
/// The terminal follows the screen by being told each change as it
/// happens, so the two agree on where every line wraps: the screen wraps
/// as soon as a line is full, a terminal only when the next character
/// comes, so a full line is ended explicitly. A cursor move sends the
/// screen's cursor position, and a colour is sent before the first
/// character written in it.
/// </remarks>
internal sealed class TelnetSession : ConsoleSession
{
    /// <summary>Screen size until the client reports its window: the classic terminal's.</summary>
    internal const int DefaultCols = 80;

    /// <summary>Screen rows until the client reports its window.</summary>
    internal const int DefaultRows = 24;

    /// <summary>Bytes taken from the connection at a time.</summary>
    private const int ReceiveChunkSize = 512;

    /// <summary>Initial size of the buffer that holds output until it is flushed.</summary>
    private const int InitialOutputCapacity = 256;

    private readonly TcpStream _stream;
    private readonly TelnetDecoder _decoder = new();
    private readonly List<KeyEvent> _decodedKeys = [];
    private readonly List<byte> _replies = [];

    // Output held until it is flushed: rented, and returned when the session closes.
    private byte[] _output = ArrayPool<byte>.Shared.Rent(InitialOutputCapacity);
    private int _outputLength;

    // The colours the terminal draws in; null is the terminal's default.
    private ConsoleColor? _sentForeground;
    private ConsoleColor? _sentBackground;

    /// <inheritdoc/>
    public override string Name { get; }

    /// <inheritdoc/>
    public override bool IsRemote => true;

    /// <summary>ANSI colour codes by <see cref="ConsoleColor"/>; a background's code is ten more than its foreground's.</summary>
    private static ReadOnlySpan<byte> AnsiForegrounds => [30, 34, 32, 36, 31, 35, 33, 37, 90, 94, 92, 96, 91, 95, 93, 97];

    internal TelnetSession(TcpStream stream, KernelConsole screen)
        : base(screen)
    {
        _stream = stream;
        Name = $"{stream.RemoteEndPoint.Address}:{stream.RemoteEndPoint.Port}";

        // What the client sends, and its hanging up, wake the reader.
        _stream.ReceiveSignal = InputSignal;
    }

    /// <summary>
    /// Opens the option negotiation and waits, for at most
    /// <paramref name="timeoutMs"/>, until the client reports its window
    /// size, so the screen has the terminal's size before anything is
    /// written. Runs on the thread that accepted the connection, before the
    /// session is started.
    /// </summary>
    internal void Negotiate(uint timeoutMs)
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            _decoder.Negotiate(_replies);
            SendReplies();
        }

        long ticksPerMillisecond = Stopwatch.Frequency / 1000;
        long deadline = Stopwatch.GetTimestamp() + ticksPerMillisecond * timeoutMs;
        while (true)
        {
            PollInput();

            long remaining = deadline - Stopwatch.GetTimestamp();
            if (_decoder.WindowColumns != 0 || IsClosed || remaining <= 0)
            {
                return;
            }

            InputSignal.Wait((uint)Math.Max(remaining / ticksPerMillisecond, 1));
        }
    }

    /// <summary>
    /// Takes in what the client sent: queues its keys, answers its
    /// negotiation, and resizes the screen to its window. Closes the session
    /// once the connection is gone.
    /// </summary>
    private protected override void PollInput()
    {
        if (IsClosed)
        {
            return;
        }

        Span<byte> received = stackalloc byte[ReceiveChunkSize];
        bool open;
        using (InternalCpu.DisableInterruptsScope())
        {
            int count;
            while ((count = _stream.Read(received)) > 0)
            {
                _decoder.Decode(received.Slice(0, count), _decodedKeys, _replies);
            }

            foreach (KeyEvent key in _decodedKeys)
            {
                EnqueueInput(key);
            }

            _decodedKeys.Clear();
            SendReplies();

            int cols = _decoder.WindowColumns;
            int rows = _decoder.WindowRows;
            if (cols > 0 && (cols != Screen.Cols || rows != Screen.Rows))
            {
                Screen.Resize(cols, rows);
                AppendCursorPosition();
                SendOutput();
            }

            open = _stream.IsOpen;
        }

        if (!open)
        {
            Close();
        }
    }

    /// <inheritdoc/>
    private protected override void OnWritten(char value, bool wrapped)
    {
        switch (value)
        {
            case '\n':
                // The screen's line feed also returns the carriage.
                Append("\r\n");
                return;

            case '\r':
                // A carriage return alone is CR NUL on a Telnet connection (RFC 854).
                Append("\r\0");
                return;

            case '\b':
                // The screen moved back, possibly to the end of the line
                // above, and blanked that cell: blank it at the position the
                // screen gives, which a terminal's own backspace would not
                // reach across a line.
                SyncColors();
                AppendCursorPosition();
                Append(' ');
                AppendCursorPosition();
                return;
        }

        SyncColors();
        AppendCharacter(value);
        if (wrapped)
        {
            Append("\r\n");
        }
    }

    /// <inheritdoc/>
    private protected override void OnCleared()
    {
        SyncColors();
        Append("\x1b[2J\x1b[H");
    }

    /// <inheritdoc/>
    private protected override void OnCursorMoved() => AppendCursorPosition();

    /// <inheritdoc/>
    private protected override void OnCursorVisibilityChanged(bool visible) => Append(visible ? "\x1b[?25h" : "\x1b[?25l");

    /// <inheritdoc/>
    private protected override void OnFlush()
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            SendOutput();
        }
    }

    /// <inheritdoc/>
    private protected override void OnClosed()
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            SendOutput();
            _stream.Close();

            // Nothing is appended once the session is closed.
            ArrayPool<byte>.Shared.Return(_output);
            _output = [];
        }
    }

    /// <summary>Sends the answers to the client's negotiation. The caller masks interrupts.</summary>
    private void SendReplies()
    {
        if (_replies.Count == 0)
        {
            return;
        }

        _stream.Write(CollectionsMarshal.AsSpan(_replies));
        _replies.Clear();
    }

    /// <summary>Sends the output held since the last flush. The caller masks interrupts.</summary>
    private void SendOutput()
    {
        if (_outputLength == 0)
        {
            return;
        }

        _stream.Write(_output.AsSpan(0, _outputLength));
        _outputLength = 0;
    }

    /// <summary>Sends the colours the session writes in, if the terminal does not have them yet.</summary>
    private void SyncColors()
    {
        ConsoleColor? foreground = ExplicitForeground;
        ConsoleColor? background = ExplicitBackground;
        if (foreground == _sentForeground && background == _sentBackground)
        {
            return;
        }

        // Reset first, so a colour put back to the default is the
        // terminal's default rather than white on black.
        Append("\x1b[0");
        if (foreground is { } foregroundColor)
        {
            Append(';');
            AppendNumber(AnsiForegrounds[(int)foregroundColor]);
        }

        if (background is { } backgroundColor)
        {
            Append(';');
            AppendNumber(AnsiForegrounds[(int)backgroundColor] + 10);
        }

        Append('m');
        _sentForeground = foreground;
        _sentBackground = background;
    }

    /// <summary>Moves the terminal's cursor to the screen's.</summary>
    private void AppendCursorPosition()
    {
        Append("\x1b[");
        AppendNumber(Screen.CursorY + 1);
        Append(';');
        AppendNumber(Screen.CursorX + 1);
        Append('H');
    }

    /// <summary>Appends a character as UTF-8. A control character, which the screen draws as a glyph but a terminal would act on, goes as a space.</summary>
    private void AppendCharacter(char value)
    {
        if (value < 0x20 || value == 0x7F)
        {
            Append(' ');
        }
        else if (value < 0x80)
        {
            Append((byte)value);
        }
        else if (value < 0x800)
        {
            Append((byte)(0xC0 | (value >> 6)));
            Append((byte)(0x80 | (value & 0x3F)));
        }
        else if (char.IsSurrogate(value))
        {
            Append('?');
        }
        else
        {
            Append((byte)(0xE0 | (value >> 12)));
            Append((byte)(0x80 | ((value >> 6) & 0x3F)));
            Append((byte)(0x80 | (value & 0x3F)));
        }
    }

    private void AppendNumber(int value)
    {
        Span<byte> digits = stackalloc byte[10];
        int count = 0;
        do
        {
            digits[count++] = (byte)('0' + value % 10);
            value /= 10;
        }
        while (value > 0);

        while (count > 0)
        {
            Append(digits[--count]);
        }
    }

    /// <summary>Appends ASCII text.</summary>
    private void Append(string text)
    {
        foreach (char c in text)
        {
            Append((byte)c);
        }
    }

    private void Append(char value) => Append((byte)value);

    private void Append(byte value)
    {
        if (IsClosed)
        {
            return;
        }

        if (_outputLength == _output.Length)
        {
            byte[] grown = ArrayPool<byte>.Shared.Rent(_output.Length * 2);
            _output.AsSpan(0, _outputLength).CopyTo(grown);
            ArrayPool<byte>.Shared.Return(_output);
            _output = grown;
        }

        _output[_outputLength++] = value;
    }
}
