// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Scheduler;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.Input;

namespace Cosmos.Kernel.System.Sessions;

/// <summary>
/// Keeps the console sessions: numbers them, shows one on the display, and
/// binds each thread to one, which is what <see cref="Console"/> reads and
/// writes on that thread.
/// </summary>
/// <remarks>
/// <para>The primary console, tty1, is <see cref="KernelConsole.Default"/>.
/// It is the session of every thread that was not started on another one,
/// the kernel's main loop among them, and it cannot be closed.</para>
/// <para>Once a second session exists, the keyboard is routed: each key
/// goes to the session on the display, and Alt with F1 to F12 shows session
/// 1 to 12, Alt with the left or right arrow the previous or next one. From
/// then on <see cref="KeyboardManager"/>'s read members, like
/// <see cref="Console"/>, read the calling thread's session.</para>
/// </remarks>
public static class SessionManager
{
    /// <summary>Most sessions at once, one per function key.</summary>
    public const int MaxSessions = 12;

    /// <summary>A switch request for the session before the one on the display.</summary>
    private const int ShowPreviousRequest = -1;

    /// <summary>A switch request for the session after the one on the display.</summary>
    private const int ShowNextRequest = -2;

    /// <summary>The session of the calling thread, when it was started on one.</summary>
    [ThreadStatic]
    private static ConsoleSession? s_threadSession;

    /// <summary>
    /// The session table, indexed by session number minus one. Filled
    /// lazily rather than by a static initializer, which would make a class
    /// constructor that the first console write runs.
    /// </summary>
    private static ConsoleSession?[]? s_slots;

    private static ConsoleSession? s_primary;
    private static ConsoleSession? s_active;

    /// <summary>
    /// The latest switch key pressed and not acted on yet: a session number,
    /// <see cref="ShowPreviousRequest"/> or <see cref="ShowNextRequest"/>, or
    /// zero. Set where the key is decoded, taken by the switching thread.
    /// </summary>
    private static int s_switchRequest;

    /// <summary>Wakes the switching thread; null until keys are routed.</summary>
    private static InterruptEvent? s_switchSignal;

    /// <summary>
    /// The primary console, tty1, or null while there is no
    /// <see cref="KernelConsole.Default"/> to show it on.
    /// </summary>
    public static ConsoleSession? Primary
    {
        get
        {
            if (s_primary is null && KernelConsole.IsInitialized)
            {
                CreatePrimary(KernelConsole.Default);
            }

            return s_primary;
        }
    }

    /// <summary>The calling thread's session, or null while there is no console.</summary>
    public static ConsoleSession? Current => s_threadSession ?? Primary;

    /// <summary>The session on the display, which the local keyboard types into, or null while there is no console.</summary>
    public static ConsoleSession? Active => s_active ?? Primary;

    /// <summary>Whether keys are routed to the session on the display, rather than left in the keyboard's queue for the primary console to take.</summary>
    internal static bool IsRoutingKeyboard { get; private set; }

    /// <summary>
    /// Opens a local console of the primary console's size. It starts
    /// hidden, and nothing runs on it until
    /// <see cref="Start(ConsoleSession, Action)"/> starts a thread there.
    /// </summary>
    /// <returns>The new session.</returns>
    /// <exception cref="InvalidOperationException">There is no console, or <see cref="MaxSessions"/> sessions are open.</exception>
    public static ConsoleSession CreateVirtualConsole()
    {
        KernelConsole.ThrowIfKernelConsoleNotInitialized();

        KernelConsole primary = KernelConsole.Default;
        VirtualConsole session = new(new KernelConsole(primary.Canvas, primary.Font, primary.Cols, primary.Rows));
        if (!TryRegister(session))
        {
            throw new InvalidOperationException($"All {MaxSessions} console sessions are in use.");
        }

        return session;
    }

    /// <summary>
    /// Lists the open sessions. A snapshot, taken anew on every call: a
    /// session that opens or closes afterwards is not reflected in it.
    /// </summary>
    /// <returns>The open sessions, by number.</returns>
    public static IReadOnlyList<ConsoleSession> GetSessions()
    {
        _ = Primary;

        List<ConsoleSession> sessions = [];
        using (InternalCpu.DisableInterruptsScope())
        {
            if (s_slots is not null)
            {
                foreach (ConsoleSession? session in s_slots)
                {
                    if (session is not null)
                    {
                        sessions.Add(session);
                    }
                }
            }
        }

        return sessions;
    }

    /// <summary>Finds a session by number.</summary>
    /// <param name="id">The session number, from 1.</param>
    /// <returns>The session, or null when no open session has that number.</returns>
    public static ConsoleSession? Get(int id)
    {
        _ = Primary;

        using (InternalCpu.DisableInterruptsScope())
        {
            return s_slots is not null && id >= 1 && id <= MaxSessions ? s_slots[id - 1] : null;
        }
    }

    /// <summary>
    /// Shows a session on the display; the local keyboard types into it from
    /// then on. The kernel does this for Alt with a function key.
    /// </summary>
    /// <param name="id">The session number, from 1.</param>
    /// <returns>False when no open session has that number.</returns>
    public static bool Switch(int id)
    {
        ConsoleSession? target = Get(id);
        if (target is null || target.IsClosed)
        {
            return false;
        }

        Show(target);
        return true;
    }

    /// <summary>
    /// Starts a thread bound to a session: <see cref="Console"/> on that
    /// thread reads and writes the session. The session is closed when
    /// <paramref name="body"/> returns, so a remote session hangs up and a
    /// virtual console goes away; an exception that escapes the body is
    /// logged and ends it the same way.
    /// </summary>
    /// <param name="session">An open session.</param>
    /// <param name="body">What the thread runs, typically a shell.</param>
    /// <exception cref="InvalidOperationException">The session is closed or is the primary console, or the thread did not start because the scheduler is not running.</exception>
    public static void Start(ConsoleSession session, Action body)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(body);

        if (session.IsClosed || session.Id == 0)
        {
            throw new InvalidOperationException($"The console session {session.Name} is not open.");
        }

        if (ReferenceEquals(session, Primary))
        {
            throw new InvalidOperationException("The primary console runs on the kernel's own thread.");
        }

        if (!KernelFeatures.Scheduler)
        {
            throw new InvalidOperationException("A console session needs a thread of its own. Set CosmosEnableScheduler=true in your csproj to enable it.");
        }

        if (!KernelThread.TryStart(() => Run(session, body)))
        {
            throw new InvalidOperationException($"The thread of console session {session.Name} did not start: the scheduler is not running.");
        }
    }

    /// <summary>
    /// The session standard input reads on the calling thread, or null when
    /// it has no input: a local session when keyboard support is compiled
    /// out, whose standard input was <see cref="TextReader.Null"/> before
    /// sessions. A remote session reads the network either way.
    /// </summary>
    /// <exception cref="InvalidOperationException">Keyboard support is on and there is no console yet.</exception>
    internal static ConsoleSession? CurrentInput => KernelFeatures.Keyboard
        ? RequireCurrent()
        : Current is { IsRemote: true } remote ? remote : null;

    /// <summary>The calling thread's session.</summary>
    /// <exception cref="InvalidOperationException">There is no console yet.</exception>
    internal static ConsoleSession RequireCurrent() => Current ?? throw new InvalidOperationException($"{nameof(KernelConsole)} is not initialized");

    /// <summary>
    /// Gives a session the lowest free number, and starts routing the
    /// keyboard once there are two sessions.
    /// </summary>
    /// <returns>False when <see cref="MaxSessions"/> sessions are open.</returns>
    internal static bool TryRegister(ConsoleSession session)
    {
        _ = Primary;

        int count = 0;
        bool registered = false;
        using (InternalCpu.DisableInterruptsScope())
        {
            s_slots ??= new ConsoleSession?[MaxSessions];
            for (int i = 0; i < s_slots.Length; i++)
            {
                if (s_slots[i] is null && !registered)
                {
                    session.Id = i + 1;
                    s_slots[i] = session;
                    registered = true;
                }

                if (s_slots[i] is not null)
                {
                    count++;
                }
            }
        }

        if (registered && count > 1)
        {
            StartKeyboardRouting();
        }

        return registered;
    }

    /// <summary>Takes a closed session out of the table; the primary console is shown instead if it was on the display.</summary>
    internal static void Unregister(ConsoleSession session)
    {
        bool wasActive;
        using (InternalCpu.DisableInterruptsScope())
        {
            if (s_slots is not null && session.Id >= 1 && ReferenceEquals(s_slots[session.Id - 1], session))
            {
                s_slots[session.Id - 1] = null;
            }

            wasActive = ReferenceEquals(s_active, session);
        }

        if (wasActive && Primary is { } primary)
        {
            Show(primary);
        }
    }

    /// <summary>Makes <see cref="KernelConsole.Default"/> the primary console, tty1, on the display.</summary>
    private static void CreatePrimary(KernelConsole screen)
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            if (s_primary is not null)
            {
                return;
            }

            VirtualConsole primary = new(screen) { Id = 1 };
            s_slots ??= new ConsoleSession?[MaxSessions];
            s_slots[0] = primary;
            s_active = primary;
            s_primary = primary;
        }
    }

    /// <summary>Hides the session on the display and shows <paramref name="target"/>, repainting the display from its screen.</summary>
    private static void Show(ConsoleSession target)
    {
        // Masked throughout, so two switches cannot interleave and leave two
        // screens painting the one canvas.
        using (InternalCpu.DisableInterruptsScope())
        {
            ConsoleSession? previous = Active;
            if (ReferenceEquals(previous, target))
            {
                return;
            }

            previous?.Screen.IsVisible = false;
            target.Screen.IsVisible = true;
            s_active = target;
        }

        target.Screen.Canvas.Display();

        // Its reader may be blocked for keys only; on the display it also
        // polls the keyboards.
        target.WakeReader();
    }

    /// <summary>Shows the open session <paramref name="step"/> numbers away from the one on the display, wrapping around.</summary>
    private static void ShowNeighbour(int step)
    {
        int start = Active?.Id ?? 1;
        for (int i = 1; i < MaxSessions; i++)
        {
            int id = (start - 1 + step * i + MaxSessions * MaxSessions) % MaxSessions + 1;
            if (Switch(id))
            {
                return;
            }
        }
    }

    /// <summary>The body of a thread bound to a session.</summary>
    private static void Run(ConsoleSession session, Action body)
    {
        s_threadSession = session;
        try
        {
            body();
        }
        catch (Exception exception)
        {
            Serial.WriteString($"[Sessions] {session.Name} ended with {exception.GetType().Name}: {exception.Message}\n");
        }
        finally
        {
            s_threadSession = null;
            session.Close();
        }
    }

    /// <summary>
    /// Starts routing the keyboard, once: a thread that switches sessions is
    /// started, then every key goes to <see cref="RouteKey"/>, and the keys
    /// already waiting in the keyboard's queue move to the session on the
    /// display.
    /// </summary>
    private static void StartKeyboardRouting()
    {
        if (!KernelFeatures.Keyboard || !KernelFeatures.Scheduler)
        {
            return;
        }

        using (InternalCpu.DisableInterruptsScope())
        {
            if (s_switchSignal is not null)
            {
                return;
            }

            s_switchSignal = new InterruptEvent();
        }

        if (!KernelThread.TryStart(SwitchOnRequest))
        {
            // Without the switching thread the primary console keeps taking
            // the keyboard's keys itself; other sessions get keys remotely.
            Serial.WriteString("[Sessions] Keyboard routing did not start: the scheduler is not running\n");
            return;
        }

        // Masked, so no key lands in the keyboard's queue between the drain
        // and the router taking over.
        using (InternalCpu.DisableInterruptsScope())
        {
            while (KeyboardManager.TryReadKey(out KeyEvent? pending))
            {
                s_active?.EnqueueInput(pending);
            }

            KeyboardManager.KeyRouter = RouteKey;
            IsRoutingKeyboard = true;
        }
    }

    /// <summary>
    /// Takes one key from the keyboard, where it is decoded, an interrupt
    /// handler included: a switch key is handed to the switching thread,
    /// any other key is queued on the session on the display.
    /// </summary>
    private static void RouteKey(KeyEvent key)
    {
        int request = SwitchRequestFor(key);
        if (request == 0)
        {
            s_active?.EnqueueInput(key);
            return;
        }

        Volatile.Write(ref s_switchRequest, request);
        s_switchSignal?.Signal();
    }

    /// <summary>The switching thread: waits for a switch key, and shows the session it asks for.</summary>
    private static void SwitchOnRequest()
    {
        InterruptEvent? signal = s_switchSignal;
        if (signal is null)
        {
            return;
        }

        while (true)
        {
            signal.Wait();

            switch (Interlocked.Exchange(ref s_switchRequest, 0))
            {
                case 0:
                    break;
                case ShowPreviousRequest:
                    ShowNeighbour(-1);
                    break;
                case ShowNextRequest:
                    ShowNeighbour(1);
                    break;
                case int id:
                    Switch(id);
                    break;
            }
        }
    }

    /// <summary>What a key asks for: a session number for Alt and a function key, <see cref="ShowPreviousRequest"/> or <see cref="ShowNextRequest"/> for Alt and an arrow, zero for any other key.</summary>
    private static int SwitchRequestFor(KeyEvent key)
    {
        if ((key.Modifiers & ConsoleModifiers.Alt) == 0)
        {
            return 0;
        }

        return key.Key switch
        {
            Key.F1 => 1,
            Key.F2 => 2,
            Key.F3 => 3,
            Key.F4 => 4,
            Key.F5 => 5,
            Key.F6 => 6,
            Key.F7 => 7,
            Key.F8 => 8,
            Key.F9 => 9,
            Key.F10 => 10,
            Key.F11 => 11,
            Key.F12 => 12,
            Key.LeftArrow => ShowPreviousRequest,
            Key.RightArrow => ShowNextRequest,
            _ => 0,
        };
    }
}
