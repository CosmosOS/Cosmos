using System.Text;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.Keyboard;

namespace Cosmos.Kernel.System.IO;

internal sealed class KeyboardTextReader : TextReader
{
    public override int Read()
    {
        if (KeyboardManager.TryReadKey(out KeyEvent? result))
        {
            return result.KeyChar;
        }
        else
        {
            return -1;
        }
    }

    public override int Peek()
    {
        return KeyboardManager.KeyAvailable ? KeyboardManager.Peek().KeyChar : -1;
    }

    public override string? ReadLine()
    {
        KernelConsole.ThrowIfKernelConsoleNotInitialized();

        var sb = new StringBuilder();

        // Track cursor position within input string
        int cursorPos = 0;

        while (true)
        {
            var keyEvent = KeyboardManager.ReadKey();

            switch (keyEvent.Key)
            {
                case ConsoleKeyEx.Enter:
                    KernelConsole.Default.WriteLine();
                    return sb.ToString();

                case ConsoleKeyEx.Backspace:
                    if (cursorPos > 0)
                    {
                        // Remove character from string at cursor position
                        sb.Remove(cursorPos - 1, 1);
                        cursorPos--;

                        // Move cursor back
                        KernelConsole.Default.MoveCursorLeft();

                        // If we're not at the end, shift remaining chars left
                        if (cursorPos < sb.Length)
                        {
                            // Save current position
                            int savedX = KernelConsole.Default.CursorX;
                            int savedY = KernelConsole.Default.CursorY;

                            // Redraw remaining characters
                            for (int i = cursorPos; i < sb.Length; i++)
                            {
                                KernelConsole.Default.Write(sb[i]);
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
                    if (cursorPos < sb.Length)
                    {
                        // Remove character at cursor position
                        sb.Remove(cursorPos, 1);

                        // Save current position
                        int savedX = KernelConsole.Default.CursorX;
                        int savedY = KernelConsole.Default.CursorY;

                        // Redraw remaining characters
                        for (int i = cursorPos; i < sb.Length; i++)
                        {
                            KernelConsole.Default.Write(sb[i]);
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
                    if (cursorPos < sb.Length)
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
                    while (cursorPos < sb.Length)
                    {
                        cursorPos++;
                        KernelConsole.Default.MoveCursorRight();
                        KernelConsole.Default.Canvas.Display();
                    }
                    break;

                default:
                    if (keyEvent.KeyChar != '\0')
                    {
                        if (cursorPos < sb.Length)
                        {
                            // Insert character in the middle
                            sb.Insert(cursorPos, keyEvent.KeyChar);
                            cursorPos++;

                            // Save current position after typing the new char
                            int afterTyping = KernelConsole.Default.CursorX + 1;
                            int savedY = KernelConsole.Default.CursorY;

                            // Redraw from current position
                            for (int i = cursorPos - 1; i < sb.Length; i++)
                            {
                                KernelConsole.Default.Write(sb[i]);
                            }

                            // Move cursor to correct position
                            KernelConsole.Default.SetCursorPosition(afterTyping, savedY);
                        }
                        else
                        {
                            // Append character at end
                            sb.Append(keyEvent.KeyChar);
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
