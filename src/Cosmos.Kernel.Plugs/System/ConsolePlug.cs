using System.Text;
using Cosmos.Build.API.Attributes;
using Cosmos.Kernel.Plugs.System.IO;
using Cosmos.Kernel.System;
using Cosmos.Kernel.System.Audio;
using Cosmos.Kernel.System.Input;
using Cosmos.Kernel.System.Sessions;

namespace Cosmos.Kernel.Plugs.System;

[Plug(typeof(Console))]
public class ConsolePlug
{
    /// <summary>Pitch in Hz of <see cref="Beep()"/>, the one Windows plays.</summary>
    private const int DefaultBeepFrequency = 800;

    /// <summary>Length in milliseconds of <see cref="Beep()"/>, the one Windows plays.</summary>
    private const int DefaultBeepDuration = 200;

    /// <summary>Lowest pitch in Hz <see cref="Beep(int, int)"/> accepts, as on Windows.</summary>
    private const int MinBeepFrequency = 37;

    /// <summary>Highest pitch in Hz <see cref="Beep(int, int)"/> accepts, as on Windows.</summary>
    private const int MaxBeepFrequency = 32767;

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

    /// <summary>Plays the Windows default tone: 800 Hz for 200 ms.</summary>
    [PlugMember]
    public static void Beep()
    {
        Beep(DefaultBeepFrequency, DefaultBeepDuration);
    }

    /// <summary>
    /// Plays a square wave through the primary audio output and returns once
    /// it has played, as on Windows, the one platform .NET implements this
    /// overload on. With nothing to play on (no output published, audio
    /// compiled out, or another player holding the output) it returns at once
    /// and plays nothing.
    /// </summary>
    /// <param name="frequency">The pitch in Hz, from 37 to 32767.</param>
    /// <param name="duration">How long the tone lasts, in milliseconds.</param>
    /// <exception cref="ArgumentOutOfRangeException">The pitch is out of range, or the duration is zero or negative.</exception>
    [PlugMember]
    public static void Beep(int frequency, int duration)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(frequency, MinBeepFrequency);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(frequency, MaxBeepFrequency);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(duration);

        if (!KernelFeatures.Audio)
        {
            return;
        }

        AudioManager.Play(new ToneAudioStream(frequency, duration));
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

        ConsoleKey consoleKey = MapToConsoleKey(keyEvent.Key);

        return new ConsoleKeyInfo(keyEvent.KeyChar, consoleKey, shift, alt, control);
    }

    /// <summary>
    /// Maps Key to System.ConsoleKey.
    /// </summary>
    private static ConsoleKey MapToConsoleKey(Key key)
    {
        return key switch
        {
            Key.Backspace => ConsoleKey.Backspace,
            Key.Tab => ConsoleKey.Tab,
            Key.Enter => ConsoleKey.Enter,
            Key.Escape => ConsoleKey.Escape,
            Key.Spacebar => ConsoleKey.Spacebar,
            Key.Delete => ConsoleKey.Delete,
            Key.D0 => ConsoleKey.D0,
            Key.D1 => ConsoleKey.D1,
            Key.D2 => ConsoleKey.D2,
            Key.D3 => ConsoleKey.D3,
            Key.D4 => ConsoleKey.D4,
            Key.D5 => ConsoleKey.D5,
            Key.D6 => ConsoleKey.D6,
            Key.D7 => ConsoleKey.D7,
            Key.D8 => ConsoleKey.D8,
            Key.D9 => ConsoleKey.D9,
            Key.A => ConsoleKey.A,
            Key.B => ConsoleKey.B,
            Key.C => ConsoleKey.C,
            Key.D => ConsoleKey.D,
            Key.E => ConsoleKey.E,
            Key.F => ConsoleKey.F,
            Key.G => ConsoleKey.G,
            Key.H => ConsoleKey.H,
            Key.I => ConsoleKey.I,
            Key.J => ConsoleKey.J,
            Key.K => ConsoleKey.K,
            Key.L => ConsoleKey.L,
            Key.M => ConsoleKey.M,
            Key.N => ConsoleKey.N,
            Key.O => ConsoleKey.O,
            Key.P => ConsoleKey.P,
            Key.Q => ConsoleKey.Q,
            Key.R => ConsoleKey.R,
            Key.S => ConsoleKey.S,
            Key.T => ConsoleKey.T,
            Key.U => ConsoleKey.U,
            Key.V => ConsoleKey.V,
            Key.W => ConsoleKey.W,
            Key.X => ConsoleKey.X,
            Key.Y => ConsoleKey.Y,
            Key.Z => ConsoleKey.Z,
            Key.F1 => ConsoleKey.F1,
            Key.F2 => ConsoleKey.F2,
            Key.F3 => ConsoleKey.F3,
            Key.F4 => ConsoleKey.F4,
            Key.F5 => ConsoleKey.F5,
            Key.F6 => ConsoleKey.F6,
            Key.F7 => ConsoleKey.F7,
            Key.F8 => ConsoleKey.F8,
            Key.F9 => ConsoleKey.F9,
            Key.F10 => ConsoleKey.F10,
            Key.F11 => ConsoleKey.F11,
            Key.F12 => ConsoleKey.F12,
            Key.UpArrow => ConsoleKey.UpArrow,
            Key.DownArrow => ConsoleKey.DownArrow,
            Key.LeftArrow => ConsoleKey.LeftArrow,
            Key.RightArrow => ConsoleKey.RightArrow,
            Key.Home => ConsoleKey.Home,
            Key.End => ConsoleKey.End,
            Key.PageUp => ConsoleKey.PageUp,
            Key.PageDown => ConsoleKey.PageDown,
            Key.Insert => ConsoleKey.Insert,
            _ => ConsoleKey.NoName
        };
    }
}
