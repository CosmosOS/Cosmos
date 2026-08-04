using System.Text;
using Cosmos.Build.API.Attributes;
using Cosmos.Kernel.Core;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.IO;
using Cosmos.Kernel.System.Keyboard;

namespace Cosmos.Kernel.Plugs.System;

[Plug(typeof(Console))]
public class ConsolePlug
{
    // Track the start position for current input line (for proper backspace/delete handling)
    private static int _inputStartX;
    private static int _inputStartY;

    private static void ThrowIfKeyboardDisabled()
    {
        if (!CosmosFeatures.KeyboardEnabled)
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
                //TODO: Once lock keyword works, call 'TextWriter.Syncronize' to get a thread save reader.
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
        KernelConsole.Default.Clear();
        if (KernelConsole.Default.IsAvailable)
        {
            KernelConsole.Default.Canvas.Display();
        }
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
        KernelConsole.Default.SetBackgroundColor(value);
    }

    [PlugMember]
    public static void ResetColor()
    {
        KernelConsole.Default.ResetColors();
    }

    [PlugMember]
    public static int get_CursorLeft()
    {
        return KernelConsole.Default.CursorX;
    }

    [PlugMember]
    public static void set_CursorLeft(int value)
    {
        KernelConsole.Default.CursorX = value;
    }

    [PlugMember]
    public static int get_CursorTop()
    {
        return KernelConsole.Default.CursorY;
    }

    [PlugMember]
    public static void set_CursorTop(int value)
    {
        KernelConsole.Default.CursorY = value;
    }

    [PlugMember]
    public static void SetCursorPosition(int left, int top)
    {
        KernelConsole.Default.SetCursorPosition(left, top);
    }

    [PlugMember]
    public static bool get_CursorVisible()
    {
        return KernelConsole.Default.CursorVisible;
    }

    [PlugMember]
    public static void set_CursorVisible(bool value)
    {
        KernelConsole.Default.CursorVisible = value;
    }

    [PlugMember]
    public static int get_WindowWidth()
    {
        return KernelConsole.Default.Cols;
    }

    [PlugMember]
    public static int get_WindowHeight()
    {
        return KernelConsole.Default.Rows;
    }

    [PlugMember]
    public static int get_BufferWidth()
    {
        return KernelConsole.Default.Cols;
    }

    [PlugMember]
    public static int get_BufferHeight()
    {
        return KernelConsole.Default.Rows;
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
        ThrowIfKeyboardDisabled();

        var keyEvent = KeyboardManager.ReadKey();

        if (!intercept && keyEvent.KeyChar != '\0')
        {
            KernelConsole.Default.Write(keyEvent.KeyChar);
            if (KernelConsole.Default.IsAvailable)
            {
                // A full-screen Display() per keystroke is a multi-megabyte MMIO
                // copy for a one-character change; under a virtualized display
                // device that's slow enough that fast typing visibly tears. Scope
                // the present to the cursor's row (covers the common no-wrap case);
                // DoLineFeed/Scroll paths (Enter, wrapping) go through a full
                // Console.WriteLine/Write elsewhere which still does a full Display.
                var (rx, ry, rw, rh) = KernelConsole.Default.CursorRowRect;
                KernelConsole.Default.Canvas.Display(rx, ry, rw, rh);
            }
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
