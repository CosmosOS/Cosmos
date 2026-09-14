using System.Text;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.Keyboard;

namespace Cosmos.Kernel.System.IO;

internal sealed class ConsoleStream : Stream
{
    private readonly StringBuilder _readLineSB;
    private bool _canRead, _canWrite;

    public ConsoleStream(FileAccess access)
    {
        _readLineSB = new();
        _canRead = (access & FileAccess.Read) == FileAccess.Read;
        _canWrite = (access & FileAccess.Write) == FileAccess.Write;
    }

    public override void Flush()
    {

    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        KernelConsole.ThrowIfKernelConsoleNotInitialized();

        var value = Console.OutputEncoding.GetString(buffer);
        KernelConsole.Default.Write(value);
        KernelConsole.Default.Canvas.Display();
    }
    public override int Read(Span<byte> buffer)
    {
        if (buffer.IsEmpty)
        {
            return 0;
        }

        if (_readLineSB.Length == 0)
        {
            var isEnter = ReadLineCore();
            if (isEnter)
            {
                _readLineSB.Append(Environment.NewLine);
            }
        }

        // Encode line into buffer.
        Encoder encoder = Console.InputEncoding.GetEncoder();
        int bytesUsedTotal = 0;
        int charsUsedTotal = 0;
        foreach (ReadOnlyMemory<char> chunk in _readLineSB.GetChunks())
        {
            encoder.Convert(chunk.Span, buffer, flush: false, out int charsUsed, out int bytesUsed, out bool completed);
            buffer = buffer.Slice(bytesUsed);
            bytesUsedTotal += bytesUsed;
            charsUsedTotal += charsUsed;
            if (!completed || buffer.IsEmpty)
            {
                break;
            }
        }
        _readLineSB.Remove(0, charsUsedTotal);
        return bytesUsedTotal;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        ValidateWrite(buffer, offset, count);
        Write(new ReadOnlySpan<byte>(buffer, offset, count));
    }

    public override void WriteByte(byte value)
    {
        KernelConsole.ThrowIfKernelConsoleNotInitialized();

        KernelConsole.Default?.Write((char)value);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        ValidateRead(buffer, offset, count);
        return Read(new Span<byte>(buffer, offset, count));
    }

    public override int ReadByte()
    {
        byte b = 0;
        int result = Read(new Span<byte>(ref b));
        return result != 0 ? b : -1;
    }

    protected override void Dispose(bool disposing)
    {
        _readLineSB.Clear();
        _canRead = false;
        _canWrite = false;
        base.Dispose(disposing);
    }

    public sealed override bool CanRead => _canRead;

    public sealed override bool CanWrite => _canWrite;

    public sealed override bool CanSeek => false;

    public sealed override long Length => throw new NotSupportedException();

    public sealed override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public sealed override void SetLength(long value) => throw new NotSupportedException();

    public sealed override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    private void ValidateRead(byte[] buffer, int offset, int count)
    {
        ValidateBufferArguments(buffer, offset, count);

        if (!_canRead)
        {
            throw new NotSupportedException();
        }
    }

    private void ValidateWrite(byte[] buffer, int offset, int count)
    {
        ValidateBufferArguments(buffer, offset, count);

        if (!_canWrite)
        {
            throw new NotSupportedException();
        }
    }

    private bool ReadLineCore()
    {
        KernelConsole.ThrowIfKernelConsoleNotInitialized();

        int cursorPos = 0;

        while (true)
        {
            var keyEvent = KeyboardManager.ReadKey();

            switch (keyEvent.Key)
            {
                case ConsoleKeyEx.Enter:
                    KernelConsole.Default.WriteLine();
                    KernelConsole.Default.Canvas.Display();
                    return true;

                case ConsoleKeyEx.Backspace:
                    if (cursorPos > 0)
                    {
                        // Remove character from string at cursor position
                        _readLineSB.Remove(cursorPos - 1, 1);
                        cursorPos--;

                        // Move cursor back
                        KernelConsole.Default.MoveCursorLeft();

                        // If we're not at the end, shift remaining chars left
                        if (cursorPos < _readLineSB.Length)
                        {
                            // Save current position
                            int savedX = KernelConsole.Default.CursorX;
                            int savedY = KernelConsole.Default.CursorY;

                            // Redraw remaining characters
                            for (int i = cursorPos; i < _readLineSB.Length; i++)
                            {
                                KernelConsole.Default.Write(_readLineSB[i]);
                            }
                            // Clear the last position (now empty)
                            KernelConsole.Default.Write(' ');

                            // Restore cursor position
                            KernelConsole.Default.SetCursorPosition(savedX, savedY);
                        }
                        else
                        {
                            // Simple case: at end of string
                            KernelConsole.Default.Write(' ');
                            KernelConsole.Default.MoveCursorLeft();
                        }
                        KernelConsole.Default.Canvas.Display();
                    }
                    break;

                case ConsoleKeyEx.Delete:
                    if (cursorPos < _readLineSB.Length)
                    {
                        // Remove character at cursor position
                        _readLineSB.Remove(cursorPos, 1);

                        // Save current position
                        int savedX = KernelConsole.Default.CursorX;
                        int savedY = KernelConsole.Default.CursorY;

                        // Redraw remaining characters
                        for (int i = cursorPos; i < _readLineSB.Length; i++)
                        {
                            KernelConsole.Default.Write(_readLineSB[i]);
                        }
                        // Clear the last position (now empty)
                        KernelConsole.Default.Write(' ');

                        // Restore cursor position
                        KernelConsole.Default.SetCursorPosition(savedX, savedY);

                        KernelConsole.Default.Canvas.Display();
                    }
                    break;

                case ConsoleKeyEx.LeftArrow:
                    if (cursorPos > 0)
                    {
                        cursorPos--;
                        KernelConsole.Default.MoveCursorLeft();
                        KernelConsole.Default.Canvas.Display();
                    }
                    break;

                case ConsoleKeyEx.RightArrow:
                    if (cursorPos < _readLineSB.Length)
                    {
                        cursorPos++;
                        KernelConsole.Default.MoveCursorRight();
                        KernelConsole.Default.Canvas.Display();
                    }
                    break;

                case ConsoleKeyEx.Home:
                    // Move cursor to start of input
                    while (cursorPos > 0)
                    {
                        cursorPos--;
                        KernelConsole.Default.MoveCursorLeft();
                        KernelConsole.Default.Canvas.Display();
                    }
                    break;

                case ConsoleKeyEx.End:
                    // Move cursor to end of input
                    while (cursorPos < _readLineSB.Length)
                    {
                        cursorPos++;
                        KernelConsole.Default.MoveCursorRight();
                        KernelConsole.Default.Canvas.Display();
                    }
                    break;

                default:
                    if (keyEvent.KeyChar != '\0')
                    {
                        if (cursorPos < _readLineSB.Length)
                        {
                            // Insert character in the middle
                            _readLineSB.Insert(cursorPos, keyEvent.KeyChar);
                            cursorPos++;

                            // Save current position after typing the new char
                            int afterTyping = KernelConsole.Default.CursorX + 1;
                            int savedY = KernelConsole.Default.CursorY;

                            // Redraw from current position
                            for (int i = cursorPos - 1; i < _readLineSB.Length; i++)
                            {
                                KernelConsole.Default.Write(_readLineSB[i]);
                            }

                            // Move cursor to correct position
                            KernelConsole.Default.SetCursorPosition(afterTyping, savedY);
                        }
                        else
                        {
                            // Append character at end
                            _readLineSB.Append(keyEvent.KeyChar);
                            cursorPos++;
                            KernelConsole.Default.Write(keyEvent.KeyChar);
                        }
                        KernelConsole.Default.Canvas.Display();
                    }
                    break;
            }
        }
    }
}
