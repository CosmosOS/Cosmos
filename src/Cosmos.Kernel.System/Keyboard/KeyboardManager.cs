// This code is licensed under the BSD 3-Clause license (see LICENSE for details)
// Ported from Cosmos.System2/Keyboard/KeyboardManager.cs

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.System.Keyboard.ScanMaps;

namespace Cosmos.Kernel.System.Keyboard;

/// <summary>
/// Manages keyboard input from physical keyboards.
/// </summary>
public static class KeyboardManager
{
    /// <summary>
    /// Whether keyboard support is enabled. Uses centralized feature flag.
    /// </summary>
    public static bool IsEnabled => CosmosFeatures.KeyboardEnabled;

    private static List<IKeyboardDevice>? s_keyboards;
    private static Queue<KeyEvent>? s_queuedKeys;
    private static ScanMapBase? s_scanMap;

    /// <summary>
    /// Whether the key the active layout maps to <see cref="ConsoleKeyEx.AltGr"/>
    /// is held. It converts as Control and Alt together, the way Windows
    /// reports it, so a layout's third level lives in its Control+Alt column.
    /// </summary>
    private static bool s_altGrPressed;

    /// <summary>
    /// The num-lock state.
    /// </summary>
    public static bool NumLock { get; private set; }

    /// <summary>
    /// The caps-lock state.
    /// </summary>
    public static bool CapsLock { get; private set; }

    /// <summary>
    /// The scroll-lock state.
    /// </summary>
    public static bool ScrollLock { get; private set; }

    /// <summary>
    /// Whether the Control (Ctrl) key is currently pressed.
    /// </summary>
    public static bool ControlPressed { get; private set; }

    /// <summary>
    /// Whether the Shift key is currently pressed.
    /// </summary>
    public static bool ShiftPressed { get; private set; }

    /// <summary>
    /// Whether the Alt key is currently pressed.
    /// </summary>
    public static bool AltPressed { get; private set; }

    /// <summary>
    /// Whether a keyboard input is pending to be processed.
    /// </summary>
    public static bool KeyAvailable => s_queuedKeys is not null && s_queuedKeys.Count > 0;

    /// <summary>
    /// Throws when keyboard support is compiled out. Guards actions, not reads:
    /// a read answers honestly (0, null, false, empty) so a kernel can branch
    /// on it, and an action names the switch to set instead of failing
    /// silently. <see cref="Peek"/> and <see cref="ReadKey"/> are the two
    /// exceptions: they return a non-nullable <see cref="KeyEvent"/> and so
    /// have no value for "no key".
    /// </summary>
    private static void ThrowIfDisabled()
    {
        if (!IsEnabled)
        {
            throw new InvalidOperationException("Keyboard support is disabled. Set CosmosEnableKeyboard=true in your csproj to enable it.");
        }
    }

    /// <summary>
    /// Initializes the keyboard manager. Called once during boot, before the
    /// platform keyboards are registered.
    /// </summary>
    internal static void Initialize()
    {
        ThrowIfDisabled();

        if (s_keyboards is not null)
        {
            return;
        }

        s_queuedKeys = new Queue<KeyEvent>();
        s_scanMap = new USStandardLayout();
        s_keyboards = new List<IKeyboardDevice>();
    }

    /// <summary>
    /// Registers a keyboard device with the manager.
    /// </summary>
    internal static void RegisterKeyboard(IKeyboardDevice keyboard)
    {
        if (s_keyboards is null || keyboard is null)
        {
            return;
        }

        keyboard.OnKeyPressed = HandleScanCode;
        s_keyboards.Add(keyboard);

        // Enable keyboard after callback is set (this registers IRQ handler)
        keyboard.Enable();

        Cosmos.Kernel.Core.IO.Serial.Write("[KeyboardManager] Registered keyboard, total: ");
        Cosmos.Kernel.Core.IO.Serial.WriteNumber((uint)s_keyboards.Count);
        Cosmos.Kernel.Core.IO.Serial.Write("\n");
    }

    /// <summary>
    /// Enqueues the given key-press event to the internal keyboard buffer.
    /// Runs in interrupt context on the IRQ path and in thread context from
    /// <see cref="PollKeyboards"/>, against readers that are always in thread
    /// context, so every touch of the queue masks interrupts for its
    /// duration. <see cref="Queue{T}"/> is not reentrant: an enqueue that
    /// grows the queue reallocates the backing array and rehomes its head
    /// while a reader may be indexing the old one.
    /// </summary>
    private static void Enqueue(KeyEvent keyEvent)
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            s_queuedKeys?.Enqueue(keyEvent);
        }
    }

    /// <summary>
    /// Handles a key-press by its physical key scan-code.
    /// </summary>
    private static void HandleScanCode(byte scanCode, bool released)
    {
        if (s_scanMap is null)
        {
            return;
        }

        byte key = scanCode;

        if (s_scanMap.ScanCodeMatchesKey(key, ConsoleKeyEx.CapsLock) && !released)
        {
            CapsLock = !CapsLock;
            UpdateLeds();
        }
        else if (s_scanMap.ScanCodeMatchesKey(key, ConsoleKeyEx.NumLock) && !released)
        {
            NumLock = !NumLock;
            UpdateLeds();
        }
        else if (s_scanMap.ScanCodeMatchesKey(key, ConsoleKeyEx.ScrollLock) && !released)
        {
            ScrollLock = !ScrollLock;
            UpdateLeds();
        }
        else if (s_scanMap.ScanCodeMatchesKey(key, ConsoleKeyEx.LCtrl) || s_scanMap.ScanCodeMatchesKey(key, ConsoleKeyEx.RCtrl))
        {
            ControlPressed = !released;
        }
        else if (s_scanMap.ScanCodeMatchesKey(key, ConsoleKeyEx.LShift) || s_scanMap.ScanCodeMatchesKey(key, ConsoleKeyEx.RShift))
        {
            ShiftPressed = !released;
        }
        else if (s_scanMap.ScanCodeMatchesKey(key, ConsoleKeyEx.LAlt) || s_scanMap.ScanCodeMatchesKey(key, ConsoleKeyEx.RAlt))
        {
            AltPressed = !released;
        }
        else if (s_scanMap.ScanCodeMatchesKey(key, ConsoleKeyEx.AltGr))
        {
            s_altGrPressed = !released;
        }
        else
        {
            if (!released)
            {
                if (GetKey(key, out var keyInfo))
                {
                    Enqueue(keyInfo!);
                }
            }
        }
    }

    /// <summary>
    /// Updates the keyboard LEDs.
    /// </summary>
    private static void UpdateLeds()
    {
        if (s_keyboards is null)
        {
            return;
        }

        foreach (IKeyboardDevice keyboard in s_keyboards)
        {
            keyboard.UpdateLeds();
        }
    }

    /// <summary>
    /// Returns the KeyEvent at the beginning of the key queue without removing it.
    /// </summary>
    /// <returns>The next pending key event, which stays in the queue.</returns>
    /// <exception cref="InvalidOperationException">Keyboard support is disabled, no keyboard has been registered, or the queue is empty. Check <see cref="KeyAvailable"/> first.</exception>
    public static KeyEvent Peek()
    {
        ThrowIfDisabled();

        if (s_queuedKeys is null)
        {
            throw new InvalidOperationException("KeyboardManager not initialized!");
        }

        using (InternalCpu.DisableInterruptsScope())
        {
            return s_queuedKeys.Peek();
        }
    }

    /// <summary>
    /// Attempts to convert the given physical key scan-code to a KeyEvent.
    /// </summary>
    private static bool GetKey(byte scanCode, out KeyEvent? keyInfo)
    {
        if (s_scanMap is null)
        {
            keyInfo = null;
            return false;
        }
        keyInfo = s_scanMap.ConvertScanCode(
            scanCode,
            ControlPressed || s_altGrPressed,
            ShiftPressed,
            AltPressed || s_altGrPressed,
            NumLock,
            CapsLock,
            ScrollLock);
        return keyInfo is not null;
    }

    /// <summary>
    /// If available, reads the next key from the pending key-press buffer.
    /// </summary>
    /// <param name="key">The key that was taken off the queue.</param>
    /// <returns><see langword="false"/> when no key is pending, no keyboard
    /// has been registered, or keyboard support is compiled out. This is the
    /// member to use when either answer is normal, since it is the only one
    /// here that does not throw for an empty queue.</returns>
    public static bool TryReadKey([NotNullWhen(true)] out KeyEvent? key)
    {
        // One call rather than Count-then-Dequeue, so an empty queue is the
        // bool this member returns rather than a throw out of it. The mask is
        // what makes the read safe against the interrupt that fills the queue.
        using (InternalCpu.DisableInterruptsScope())
        {
            if (s_queuedKeys is not null && s_queuedKeys.TryDequeue(out KeyEvent? pending))
            {
                key = pending;
                return true;
            }
        }

        key = default;
        return false;
    }

    /// <summary>
    /// Reads the next key from the pending key-press buffer, blocking until available.
    /// </summary>
    /// <returns>The next key, once one arrives.</returns>
    /// <exception cref="InvalidOperationException">Keyboard support is disabled,
    /// or no keyboard has been registered. This member returns a non-nullable
    /// <see cref="KeyEvent"/> and so has no value meaning "no key", and with
    /// the feature off it would otherwise wait forever on an interrupt no
    /// keyboard will raise.</exception>
    public static KeyEvent ReadKey()
    {
        ThrowIfDisabled();

        if (s_queuedKeys is null)
        {
            throw new InvalidOperationException("KeyboardManager not initialized!");
        }

        while (true)
        {
            // The mask covers the take and nothing else: polling allocates and
            // the halt below waits for the very interrupt this scope masks.
            using (InternalCpu.DisableInterruptsScope())
            {
                if (s_queuedKeys.TryDequeue(out KeyEvent? key))
                {
                    return key;
                }
            }

            // Poll all keyboards for events (in case interrupts aren't working)
            PollKeyboards();

            // Halt CPU until interrupt (key press)
            HAL.PlatformHAL.CpuOps?.Halt();
        }
    }

    /// <summary>
    /// Polls all registered keyboards for events.
    /// </summary>
    private static void PollKeyboards()
    {
        if (s_keyboards is null)
        {
            return;
        }

        foreach (var keyboard in s_keyboards)
        {
            keyboard.Poll();
        }
    }

    /// <summary>
    /// Gets the scan map that turns scan codes into characters.
    /// </summary>
    /// <returns>The active layout, or <see langword="null"/> before a keyboard
    /// has been registered and when keyboard support is compiled out;
    /// registration installs <see cref="ScanMaps.USStandardLayout"/>.</returns>
    public static ScanMapBase? GetKeyLayout() => s_scanMap;

    /// <summary>
    /// Sets the scan map that turns scan codes into characters. This is a
    /// method rather than a settable property beside
    /// <see cref="GetKeyLayout"/> because the two halves cannot share a type:
    /// the read is honestly nullable, while a null layout would leave the
    /// interrupt path with nothing to decode with. Both forms refuse null at
    /// run time; only a non-nullable parameter also diagnoses it at compile
    /// time, which a nullable property cannot.
    /// </summary>
    /// <param name="scanMap">The layout to use.</param>
    /// <exception cref="InvalidOperationException">Keyboard support is disabled.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="scanMap"/> is null.</exception>
    public static void SetKeyLayout(ScanMapBase scanMap)
    {
        // The switch first, so a compiled-out keyboard names the switch to set
        // rather than reporting whatever else is wrong with the call.
        ThrowIfDisabled();
        ArgumentNullException.ThrowIfNull(scanMap);

        s_scanMap = scanMap;
    }

}
