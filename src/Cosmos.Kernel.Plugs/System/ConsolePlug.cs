using System.Text;
using Cosmos.Build.API.Attributes;
using Cosmos.Kernel.Plugs.System.IO;
using Cosmos.Kernel.System;
using Cosmos.Kernel.System.Audio;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.Input;

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

    // Track the start position for current input line (for proper backspace/delete handling)
    private static int s_inputStartX;
    private static int s_inputStartY;

    private static void ThrowIfKeyboardDisabled()
    {
        if (!KernelFeatures.Keyboard)
        {
            throw new InvalidOperationException("Console input requires keyboard support. Set CosmosEnableKeyboard=true in your csproj to enable it.");
        }
    }

    /// <summary>
    /// Flushes the console back buffer to the screen. The console only draws
    /// into the canvas back buffer, so every mutation visible to the user
    /// (writes, cursor moves) must be followed by a flush.
    /// </summary>
    private static void DisplayCanvas()
    {
        KernelConsole.ThrowIfKernelConsoleNotInitialized();

        KernelConsole.Default.Canvas.Display();
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
        KernelConsole.ThrowIfKernelConsoleNotInitialized();

        KernelConsole.Default.Clear();
        DisplayCanvas();
    }

    [PlugMember]
    public static ConsoleColor get_ForegroundColor()
    {
        // Return white as default - we don't track the reverse mapping
        return ConsoleColor.White;
    }

    [PlugMember]
    public static void set_ForegroundColor(ConsoleColor value)
    {
        KernelConsole.ThrowIfKernelConsoleNotInitialized();

        KernelConsole.Default.SetForegroundColor(value);
    }

    [PlugMember]
    public static ConsoleColor get_BackgroundColor()
    {
        // Return black as default - we don't track the reverse mapping
        return ConsoleColor.Black;
    }

    [PlugMember]
    public static void set_BackgroundColor(ConsoleColor value)
    {
        KernelConsole.ThrowIfKernelConsoleNotInitialized();

        KernelConsole.Default.SetBackgroundColor(value);
    }

    [PlugMember]
    public static void ResetColor()
    {
        KernelConsole.ThrowIfKernelConsoleNotInitialized();

        KernelConsole.Default.ResetColors();
    }

    [PlugMember]
    public static int get_CursorLeft()
    {
        KernelConsole.ThrowIfKernelConsoleNotInitialized();

        return KernelConsole.Default.CursorX;
    }

    [PlugMember]
    public static void set_CursorLeft(int value)
    {
        KernelConsole.ThrowIfKernelConsoleNotInitialized();

        KernelConsole.Default.CursorX = value;
        DisplayCanvas();
    }

    [PlugMember]
    public static int get_CursorTop()
    {
        KernelConsole.ThrowIfKernelConsoleNotInitialized();

        return KernelConsole.Default.CursorY;
    }

    [PlugMember]
    public static void set_CursorTop(int value)
    {
        KernelConsole.ThrowIfKernelConsoleNotInitialized();

        KernelConsole.Default.CursorY = value;
        DisplayCanvas();
    }

    [PlugMember]
    public static void SetCursorPosition(int left, int top)
    {
        KernelConsole.ThrowIfKernelConsoleNotInitialized();

        KernelConsole.Default.SetCursorPosition(left, top);
        DisplayCanvas();
    }

    [PlugMember]
    public static bool get_CursorVisible()
    {
        KernelConsole.ThrowIfKernelConsoleNotInitialized();

        return KernelConsole.Default.CursorVisible;
    }

    [PlugMember]
    public static void set_CursorVisible(bool value)
    {
        KernelConsole.ThrowIfKernelConsoleNotInitialized();

        KernelConsole.Default.CursorVisible = value;
        DisplayCanvas();
    }

    [PlugMember]
    public static int get_WindowWidth()
    {
        KernelConsole.ThrowIfKernelConsoleNotInitialized();

        return KernelConsole.Default.Cols;
    }

    [PlugMember]
    public static int get_WindowHeight()
    {
        KernelConsole.ThrowIfKernelConsoleNotInitialized();

        return KernelConsole.Default.Rows;
    }

    [PlugMember]
    public static int get_BufferWidth()
    {
        KernelConsole.ThrowIfKernelConsoleNotInitialized();

        return KernelConsole.Default.Cols;
    }

    [PlugMember]
    public static int get_BufferHeight()
    {
        KernelConsole.ThrowIfKernelConsoleNotInitialized();

        return KernelConsole.Default.Rows;
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
        ThrowIfKeyboardDisabled();
        return KeyboardManager.KeyAvailable;
    }

    [PlugMember]
    public static ConsoleKeyInfo ReadKey() => ReadKey(false);

    [PlugMember]
    public static ConsoleKeyInfo ReadKey(bool intercept)
    {
        KernelConsole.ThrowIfKernelConsoleNotInitialized();
        ThrowIfKeyboardDisabled();

        var keyEvent = KeyboardManager.ReadKey();

        if (!intercept && keyEvent.KeyChar != '\0')
        {
            KernelConsole.Default.Write(keyEvent.KeyChar);
            DisplayCanvas();
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
