// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.Scheduler;
using Cosmos.Kernel.HAL.Boot;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.Input;

namespace Cosmos.Kernel.System.Sessions;

/// <summary>
/// One console that <see cref="Console"/> can be bound to: a screen, and the
/// keys typed at it. The local virtual consoles and the connections of a
/// Telnet server are all sessions. <see cref="SessionManager"/> numbers
/// them, shows one of them on the display, and binds every thread to one,
/// so the same <see cref="Console"/> calls read and write whichever session
/// the calling thread belongs to.
/// </summary>
/// <remarks>
/// Every session keeps its screen as a <see cref="KernelConsole"/> grid, on
/// the display's canvas but hidden until the session is shown, so a session
/// can be switched to at any time, a remote one included. A remote session
/// also sends every change to its terminal as it happens. Output runs with
/// interrupts masked, so the screen and the remote terminal see one
/// thread's changes in the order it made them.
/// </remarks>
public abstract class ConsoleSession
{
    /// <summary>Columns between two tab stops, as the screen expands a tab.</summary>
    private const int TabWidth = 4;

    /// <summary>
    /// Keys the queue holds before it grows. The keyboard's interrupt handler
    /// queues onto it, and growing allocates, so it is sized for a burst of
    /// typing the reader has not caught up with rather than left to grow
    /// from empty, as <see cref="InterruptEvent"/> pre-sizes its waiter list.
    /// </summary>
    private const int InputQueueCapacity = 64;

    private readonly Queue<KeyEvent> _input = new(InputQueueCapacity);
    private int _closed;

    /// <summary>
    /// The session's number, from 1: its slot in the session table, and the
    /// function key that shows it with Alt. Zero until the session is
    /// registered.
    /// </summary>
    public int Id { get; internal set; }

    /// <summary>
    /// The name the session is listed under: <c>tty</c> and the number for a
    /// virtual console, the peer's address for a remote session.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>Whether the session's keys come from a remote terminal rather than the local keyboard.</summary>
    public abstract bool IsRemote { get; }

    /// <summary>Whether the session is the one on the display, the one the local keyboard types into.</summary>
    public bool IsActive => ReferenceEquals(SessionManager.Active, this);

    /// <summary>
    /// Whether the session was closed. A closed session drops what is
    /// written to it, and its reads report the end of input.
    /// </summary>
    public bool IsClosed => Volatile.Read(ref _closed) != 0;

    /// <summary>Width of the session's screen, in characters.</summary>
    public int Cols => Screen.Cols;

    /// <summary>Height of the session's screen, in characters.</summary>
    public int Rows => Screen.Rows;

    /// <summary>The grid the session draws into, shown while the session is active.</summary>
    internal KernelConsole Screen { get; }

    /// <summary>The text colour a program set, or null while the colours are the defaults.</summary>
    private protected ConsoleColor? ExplicitForeground { get; private set; }

    /// <summary>The background colour a program set, or null while the colours are the defaults.</summary>
    private protected ConsoleColor? ExplicitBackground { get; private set; }

    /// <summary>What a reader blocks on: signaled for every key queued, when the session closes, and by whatever else brings input, such as a remote session's connection.</summary>
    private protected InterruptEvent InputSignal { get; } = new();

    /// <summary>The text colour; white until set.</summary>
    internal ConsoleColor ForegroundColor
    {
        get => ExplicitForeground ?? ConsoleColor.White;
        set
        {
            using (InternalCpu.DisableInterruptsScope())
            {
                Screen.SetForegroundColor(value);
                ExplicitForeground = value;
                OnColorsChanged();
            }
        }
    }

    /// <summary>The background colour; black until set.</summary>
    internal ConsoleColor BackgroundColor
    {
        get => ExplicitBackground ?? ConsoleColor.Black;
        set
        {
            using (InternalCpu.DisableInterruptsScope())
            {
                Screen.SetBackgroundColor(value);
                ExplicitBackground = value;
                OnColorsChanged();
            }
        }
    }

    /// <summary>The cursor's column.</summary>
    internal int CursorLeft => Screen.CursorX;

    /// <summary>The cursor's row.</summary>
    internal int CursorTop => Screen.CursorY;

    /// <summary>Whether the cursor is drawn.</summary>
    internal bool CursorVisible
    {
        get => Screen.CursorVisible;
        set
        {
            using (InternalCpu.DisableInterruptsScope())
            {
                Screen.CursorVisible = value;
                OnCursorVisibilityChanged(value);
            }
        }
    }

    /// <summary>
    /// Whether a key is waiting. True as well once the session is closed, so
    /// a loop that polls for a key goes on to read it and learns that the
    /// input ended.
    /// </summary>
    internal bool KeyAvailable
    {
        get
        {
            PollInput();
            using (InternalCpu.DisableInterruptsScope())
            {
                return _input.Count > 0 || IsClosed;
            }
        }
    }

    private protected ConsoleSession(KernelConsole screen)
    {
        Screen = screen;
    }

    /// <summary>
    /// Closes the session. A remote session hangs up, and a session on the
    /// display gives it back to the primary console. The primary console
    /// itself cannot be closed.
    /// </summary>
    /// <exception cref="InvalidOperationException">The session is the primary console.</exception>
    public void Close()
    {
        if (ReferenceEquals(this, SessionManager.Primary))
        {
            throw new InvalidOperationException("The primary console cannot be closed.");
        }

        if (Interlocked.Exchange(ref _closed, 1) != 0)
        {
            return;
        }

        OnClosed();
        SessionManager.Unregister(this);

        // A reader blocked on the session learns that its input ended.
        InputSignal.Signal();
    }

    /// <summary>Writes a character at the cursor.</summary>
    internal void Write(char value)
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            WriteCore(value);
        }
    }

    /// <summary>Writes characters at the cursor.</summary>
    internal void Write(ReadOnlySpan<char> text)
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            foreach (char c in text)
            {
                WriteCore(c);
            }
        }
    }

    /// <summary>Clears the screen and homes the cursor.</summary>
    internal void Clear()
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            Screen.Clear();
            OnCleared();
        }
    }

    /// <summary>Puts back the default colours.</summary>
    internal void ResetColors()
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            Screen.ResetColors();
            ExplicitForeground = null;
            ExplicitBackground = null;
            OnColorsChanged();
        }
    }

    /// <summary>Moves the cursor; a position off the screen is ignored.</summary>
    internal void SetCursorPosition(int left, int top)
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            Screen.SetCursorPosition(left, top);
            OnCursorMoved();
        }
    }

    /// <summary>Moves the cursor one column left; a no-op in the first column.</summary>
    internal void MoveCursorLeft()
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            Screen.MoveCursorLeft();
            OnCursorMoved();
        }
    }

    /// <summary>Moves the cursor one column right; a no-op in the last column.</summary>
    internal void MoveCursorRight()
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            Screen.MoveCursorRight();
            OnCursorMoved();
        }
    }

    /// <summary>
    /// Shows what was written: flushes the canvas while the session is on
    /// the display, and sends a remote session's pending output.
    /// </summary>
    internal void Flush()
    {
        if (Screen.IsVisible)
        {
            Screen.Canvas.Display();
        }

        OnFlush();
    }

    /// <summary>
    /// Queues a key for the session's reader and wakes it. Any context, the
    /// keyboard's interrupt handler included: the queue only grows, and so
    /// allocates, once <see cref="InputQueueCapacity"/> keys wait unread.
    /// </summary>
    internal void EnqueueInput(KeyEvent key)
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            _input.Enqueue(key);
        }

        InputSignal.Signal();
    }

    /// <summary>Wakes the session's reader to look at its input again: the session was just shown, so it now polls the keyboards.</summary>
    internal void WakeReader() => InputSignal.Signal();

    /// <summary>Takes the next key if one is waiting.</summary>
    internal bool TryReadKey([NotNullWhen(true)] out KeyEvent? key)
    {
        PollInput();
        using (InternalCpu.DisableInterruptsScope())
        {
            return _input.TryDequeue(out key);
        }
    }

    /// <summary>Looks at the next key, if one is waiting, without taking it.</summary>
    internal bool TryPeekKey([NotNullWhen(true)] out KeyEvent? key)
    {
        PollInput();
        using (InternalCpu.DisableInterruptsScope())
        {
            return _input.TryPeek(out key);
        }
    }

    /// <summary>Waits for the next key.</summary>
    /// <returns>The key, or null once the session is closed.</returns>
    internal KeyEvent? WaitForKey()
    {
        while (true)
        {
            if (TryReadKey(out KeyEvent? key))
            {
                return key;
            }

            if (IsClosed)
            {
                return null;
            }

            WaitForInput();
        }
    }

    /// <summary>Waits for the next key.</summary>
    /// <exception cref="EndOfStreamException">The session was closed.</exception>
    internal KeyEvent ReadKey() => WaitForKey() ?? throw new EndOfStreamException($"The console session {Name} was closed.");

    /// <summary>Takes in whatever input arrived: keys from the keyboard or the network, window size changes. Called by every read.</summary>
    private protected virtual void PollInput()
    {
    }

    /// <summary>A character went to the screen: a printable one, a line feed, a carriage return or a backspace.</summary>
    /// <param name="value">The character.</param>
    /// <param name="wrapped">Whether writing a printable character filled the line, so the cursor went on to the next one.</param>
    private protected virtual void OnWritten(char value, bool wrapped)
    {
    }

    /// <summary>The screen was cleared.</summary>
    private protected virtual void OnCleared()
    {
    }

    /// <summary>The cursor was moved without writing.</summary>
    private protected virtual void OnCursorMoved()
    {
    }

    /// <summary>The cursor was shown or hidden.</summary>
    private protected virtual void OnCursorVisibilityChanged(bool visible)
    {
    }

    /// <summary>The colours changed.</summary>
    private protected virtual void OnColorsChanged()
    {
    }

    /// <summary>A write finished and should be shown.</summary>
    private protected virtual void OnFlush()
    {
    }

    /// <summary>The session was closed; releases what it holds.</summary>
    private protected virtual void OnClosed()
    {
    }

    /// <summary>Writes one character. The caller masks interrupts.</summary>
    private void WriteCore(char value)
    {
        if (value == '\t')
        {
            // Expanded here rather than by the screen, so a remote terminal,
            // whose tab stops are 8 columns apart, receives spaces and ends
            // up where the screen's 4-column stops put the cursor.
            int spaces = TabWidth - Screen.CursorX % TabWidth;
            for (int i = 0; i < spaces; i++)
            {
                WriteCore(' ');
            }

            return;
        }

        Screen.Write(value);

        bool printable = value is not ('\n' or '\r' or '\b');
        OnWritten(value, printable && Screen.CursorX == 0);
    }

    /// <summary>
    /// Waits for input to arrive. A thread the scheduler may block sleeps on
    /// the session's signal until a key is queued. The boot thread, which is
    /// the scheduler's idle thread and must stay runnable, and a kernel
    /// without the scheduler wait for the next interrupt instead, as
    /// <see cref="KeyboardManager.ReadKey"/> does.
    /// </summary>
    private void WaitForInput()
    {
        SchedulerThread? current = SchedulerManager.IsReady
            ? SchedulerManager.CurrentCpuState?.CurrentThread
            : null;

        if (current is null || (current.Flags & SchedulerThreadFlags.IdleThread) != 0)
        {
            PlatformHAL.CpuOps?.Halt();
        }
        else
        {
            InputSignal.Wait();
        }
    }
}
