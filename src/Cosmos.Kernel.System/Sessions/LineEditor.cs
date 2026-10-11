// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Text;
using Cosmos.Kernel.System.Input;

namespace Cosmos.Kernel.System.Sessions;

/// <summary>
/// Reads one line from a session with the console's line editing: typing
/// inserts at the cursor, Backspace and Delete remove around it, the arrows
/// and Home and End move it. What is typed is echoed to the session, so a
/// remote terminal sees it the way the screen does.
/// </summary>
internal static class LineEditor
{
    /// <summary>Reads a line, echoing it, and ends it with a new line once Enter is pressed.</summary>
    /// <param name="session">The session to read from and echo to.</param>
    /// <returns>The line without its terminator, or null when the session closed first.</returns>
    public static string? ReadLine(ConsoleSession session)
    {
        StringBuilder line = new();

        // Position of the cursor within the line.
        int cursor = 0;

        while (true)
        {
            KeyEvent? key = session.WaitForKey();
            if (key is null)
            {
                return null;
            }

            switch (key.Key)
            {
                case Key.Enter:
                    session.Write('\n');
                    session.Flush();
                    return line.ToString();

                case Key.Backspace:
                    if (cursor > 0)
                    {
                        line.Remove(cursor - 1, 1);
                        cursor--;
                        session.MoveCursorLeft();
                        RedrawTail(session, line, cursor);
                        session.Flush();
                    }
                    break;

                case Key.Delete:
                    if (cursor < line.Length)
                    {
                        line.Remove(cursor, 1);
                        RedrawTail(session, line, cursor);
                        session.Flush();
                    }
                    break;

                case Key.LeftArrow:
                    if (cursor > 0)
                    {
                        cursor--;
                        session.MoveCursorLeft();
                        session.Flush();
                    }
                    break;

                case Key.RightArrow:
                    if (cursor < line.Length)
                    {
                        cursor++;
                        session.MoveCursorRight();
                        session.Flush();
                    }
                    break;

                case Key.Home:
                    while (cursor > 0)
                    {
                        cursor--;
                        session.MoveCursorLeft();
                    }

                    session.Flush();
                    break;

                case Key.End:
                    while (cursor < line.Length)
                    {
                        cursor++;
                        session.MoveCursorRight();
                    }

                    session.Flush();
                    break;

                default:
                    if (key.KeyChar != '\0')
                    {
                        Insert(session, line, ref cursor, key.KeyChar);
                        session.Flush();
                    }
                    break;
            }
        }
    }

    /// <summary>Inserts a character at the cursor and moves the cursor past it.</summary>
    private static void Insert(ConsoleSession session, StringBuilder line, ref int cursor, char value)
    {
        if (cursor == line.Length)
        {
            line.Append(value);
            cursor++;
            session.Write(value);
            return;
        }

        line.Insert(cursor, value);
        cursor++;

        // Where the cursor goes once the rest of the line is redrawn.
        int left = session.CursorLeft + 1;
        int top = session.CursorTop;

        for (int i = cursor - 1; i < line.Length; i++)
        {
            session.Write(line[i]);
        }

        session.SetCursorPosition(left, top);
    }

    /// <summary>
    /// Redraws the line from the cursor on after a character was removed:
    /// the rest of the line moves one column left, and the column it leaves
    /// is blanked. The cursor stays where it was.
    /// </summary>
    private static void RedrawTail(ConsoleSession session, StringBuilder line, int cursor)
    {
        int left = session.CursorLeft;
        int top = session.CursorTop;

        for (int i = cursor; i < line.Length; i++)
        {
            session.Write(line[i]);
        }

        session.Write(' ');
        session.SetCursorPosition(left, top);
    }
}
