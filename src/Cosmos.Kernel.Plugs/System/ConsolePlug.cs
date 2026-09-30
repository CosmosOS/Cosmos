using System.Text;
using Cosmos.Build.API.Attributes;
using Cosmos.Kernel.System;
using Cosmos.Kernel.System.IO;
using Cosmos.Kernel.System.Keyboard;
using Cosmos.Kernel.System.Sessions;

namespace Cosmos.Kernel.Plugs.System;

[Plug(typeof(Console))]
public class ConsolePlug
{
    private static void ThrowIfKeyboardDisabled(ConsoleSession session)
    {
        // A remote session's keys come from the network, not the keyboard.
        if (!session.IsRemote && !KernelFeatures.Keyboard)
        {
            throw new InvalidOperationException("Console input requires keyboard support. Set CosmosEnableKeyboard=true in your csproj to enable it.");
        }
    }

    [PlugMember]
    private static TextWriter CreateOutputWriter(Stream outputStream)
    {
        if (outputStream != Stream.Null)
        {
            if (Console.OutputEncoding != Encoding.Default)
            {
                //TODO: Once lock keyword works, call 'TextWriter.Synchronize' to get a thread save reader.
                return new StreamWriter(outputStream, Console.OutputEncoding, 256, leaveOpen: true)
                {
                    AutoFlush = true
                };
            }
            else
            {
                return new ConsoleTextWriter();
            }
        }
        return TextWriter.Null;
    }

    [PlugMember]
    public static void Clear()
    {
        ConsoleSession session = SessionManager.RequireCurrent();
        session.Clear();
        session.Flush();
    }

    [PlugMember]
    public static ConsoleColor get_ForegroundColor()
    {
        return SessionManager.RequireCurrent().ForegroundColor;
    }

    [PlugMember]
    public static void set_ForegroundColor(ConsoleColor value)
    {
        SessionManager.RequireCurrent().ForegroundColor = value;
    }

    [PlugMember]
    public static ConsoleColor get_BackgroundColor()
    {
        return SessionManager.RequireCurrent().BackgroundColor;
    }

    [PlugMember]
    public static void set_BackgroundColor(ConsoleColor value)
    {
        SessionManager.RequireCurrent().BackgroundColor = value;
    }

    [PlugMember]
    public static void ResetColor()
    {
        SessionManager.RequireCurrent().ResetColors();
    }

    [PlugMember]
    public static int get_CursorLeft()
    {
        return SessionManager.RequireCurrent().CursorLeft;
    }

    [PlugMember]
    public static void set_CursorLeft(int value)
    {
        ConsoleSession session = SessionManager.RequireCurrent();
        session.SetCursorPosition(value, session.CursorTop);
        session.Flush();
    }

    [PlugMember]
    public static int get_CursorTop()
    {
        return SessionManager.RequireCurrent().CursorTop;
    }

    [PlugMember]
    public static void set_CursorTop(int value)
    {
        ConsoleSession session = SessionManager.RequireCurrent();
        session.SetCursorPosition(session.CursorLeft, value);
        session.Flush();
    }

    [PlugMember]
    public static void SetCursorPosition(int left, int top)
    {
        ConsoleSession session = SessionManager.RequireCurrent();
        session.SetCursorPosition(left, top);
        session.Flush();
    }

    [PlugMember]
    public static bool get_CursorVisible()
    {
        return SessionManager.RequireCurrent().CursorVisible;
    }

    [PlugMember]
    public static void set_CursorVisible(bool value)
    {
        ConsoleSession session = SessionManager.RequireCurrent();
        session.CursorVisible = value;
        session.Flush();
    }

    [PlugMember]
    public static int get_WindowWidth()
    {
        return SessionManager.RequireCurrent().Cols;
    }

    [PlugMember]
    public static int get_WindowHeight()
    {
        return SessionManager.RequireCurrent().Rows;
    }

    [PlugMember]
    public static int get_BufferWidth()
    {
        return SessionManager.RequireCurrent().Cols;
    }

    [PlugMember]
    public static int get_BufferHeight()
    {
        return SessionManager.RequireCurrent().Rows;
    }

    [PlugMember]
    public static bool get_KeyAvailable()
    {
        ConsoleSession session = SessionManager.RequireCurrent();
        ThrowIfKeyboardDisabled(session);
        return session.KeyAvailable;
    }

    [PlugMember]
    public static ConsoleKeyInfo ReadKey() => ReadKey(false);

    [PlugMember]
    public static ConsoleKeyInfo ReadKey(bool intercept)
    {
        ConsoleSession session = SessionManager.RequireCurrent();
        ThrowIfKeyboardDisabled(session);

        KeyEvent keyEvent = session.ReadKey();

        if (!intercept && keyEvent.KeyChar != '\0')
        {
            session.Write(keyEvent.KeyChar);
            session.Flush();
        }

        return ToConsoleKeyInfo(keyEvent);
    }

    /// <summary>
    /// Converts a KeyEvent to ConsoleKeyInfo.
    /// </summary>
    private static ConsoleKeyInfo ToConsoleKeyInfo(KeyEvent keyEvent)
    {
        bool shift = (keyEvent.Modifiers & ConsoleModifiers.Shift) != 0;
        bool alt = (keyEvent.Modifiers & ConsoleModifiers.Alt) != 0;
        bool control = (keyEvent.Modifiers & ConsoleModifiers.Control) != 0;

        // Map ConsoleKeyEx to ConsoleKey
        ConsoleKey consoleKey = MapToConsoleKey(keyEvent.Key);

        return new ConsoleKeyInfo(keyEvent.KeyChar, consoleKey, shift, alt, control);
    }

    /// <summary>
    /// Maps ConsoleKeyEx to System.ConsoleKey.
    /// </summary>
    private static ConsoleKey MapToConsoleKey(ConsoleKeyEx key)
    {
        return key switch
        {
            ConsoleKeyEx.Backspace => ConsoleKey.Backspace,
            ConsoleKeyEx.Tab => ConsoleKey.Tab,
            ConsoleKeyEx.Enter => ConsoleKey.Enter,
            ConsoleKeyEx.Escape => ConsoleKey.Escape,
            ConsoleKeyEx.Spacebar => ConsoleKey.Spacebar,
            ConsoleKeyEx.Delete => ConsoleKey.Delete,
            ConsoleKeyEx.D0 => ConsoleKey.D0,
            ConsoleKeyEx.D1 => ConsoleKey.D1,
            ConsoleKeyEx.D2 => ConsoleKey.D2,
            ConsoleKeyEx.D3 => ConsoleKey.D3,
            ConsoleKeyEx.D4 => ConsoleKey.D4,
            ConsoleKeyEx.D5 => ConsoleKey.D5,
            ConsoleKeyEx.D6 => ConsoleKey.D6,
            ConsoleKeyEx.D7 => ConsoleKey.D7,
            ConsoleKeyEx.D8 => ConsoleKey.D8,
            ConsoleKeyEx.D9 => ConsoleKey.D9,
            ConsoleKeyEx.A => ConsoleKey.A,
            ConsoleKeyEx.B => ConsoleKey.B,
            ConsoleKeyEx.C => ConsoleKey.C,
            ConsoleKeyEx.D => ConsoleKey.D,
            ConsoleKeyEx.E => ConsoleKey.E,
            ConsoleKeyEx.F => ConsoleKey.F,
            ConsoleKeyEx.G => ConsoleKey.G,
            ConsoleKeyEx.H => ConsoleKey.H,
            ConsoleKeyEx.I => ConsoleKey.I,
            ConsoleKeyEx.J => ConsoleKey.J,
            ConsoleKeyEx.K => ConsoleKey.K,
            ConsoleKeyEx.L => ConsoleKey.L,
            ConsoleKeyEx.M => ConsoleKey.M,
            ConsoleKeyEx.N => ConsoleKey.N,
            ConsoleKeyEx.O => ConsoleKey.O,
            ConsoleKeyEx.P => ConsoleKey.P,
            ConsoleKeyEx.Q => ConsoleKey.Q,
            ConsoleKeyEx.R => ConsoleKey.R,
            ConsoleKeyEx.S => ConsoleKey.S,
            ConsoleKeyEx.T => ConsoleKey.T,
            ConsoleKeyEx.U => ConsoleKey.U,
            ConsoleKeyEx.V => ConsoleKey.V,
            ConsoleKeyEx.W => ConsoleKey.W,
            ConsoleKeyEx.X => ConsoleKey.X,
            ConsoleKeyEx.Y => ConsoleKey.Y,
            ConsoleKeyEx.Z => ConsoleKey.Z,
            ConsoleKeyEx.F1 => ConsoleKey.F1,
            ConsoleKeyEx.F2 => ConsoleKey.F2,
            ConsoleKeyEx.F3 => ConsoleKey.F3,
            ConsoleKeyEx.F4 => ConsoleKey.F4,
            ConsoleKeyEx.F5 => ConsoleKey.F5,
            ConsoleKeyEx.F6 => ConsoleKey.F6,
            ConsoleKeyEx.F7 => ConsoleKey.F7,
            ConsoleKeyEx.F8 => ConsoleKey.F8,
            ConsoleKeyEx.F9 => ConsoleKey.F9,
            ConsoleKeyEx.F10 => ConsoleKey.F10,
            ConsoleKeyEx.F11 => ConsoleKey.F11,
            ConsoleKeyEx.F12 => ConsoleKey.F12,
            ConsoleKeyEx.UpArrow => ConsoleKey.UpArrow,
            ConsoleKeyEx.DownArrow => ConsoleKey.DownArrow,
            ConsoleKeyEx.LeftArrow => ConsoleKey.LeftArrow,
            ConsoleKeyEx.RightArrow => ConsoleKey.RightArrow,
            ConsoleKeyEx.Home => ConsoleKey.Home,
            ConsoleKeyEx.End => ConsoleKey.End,
            ConsoleKeyEx.PageUp => ConsoleKey.PageUp,
            ConsoleKeyEx.PageDown => ConsoleKey.PageDown,
            ConsoleKeyEx.Insert => ConsoleKey.Insert,
            _ => ConsoleKey.NoName
        };
    }
}
