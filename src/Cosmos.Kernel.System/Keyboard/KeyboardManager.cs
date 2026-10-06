// This code is licensed under the BSD 3-Clause license (see LICENSE for details)
// Ported from Cosmos.System2/Keyboard/KeyboardManager.cs

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Devices;
using Cosmos.Kernel.HAL.DriverKit.Engine;
using Cosmos.Kernel.HAL.Platform;
using Cosmos.Kernel.System.Keyboard.ScanMaps;

namespace Cosmos.Kernel.System.Keyboard;

/// <summary>
/// Manages keyboard input from every keyboard a driver kit driver publishes
/// (the PS/2 keyboard on x64, virtio and USB keyboards), which the manager's
/// <see cref="KitKeyboardConsumer"/> registers from the kit worker when it is
/// published and unregisters when it is withdrawn.
/// </summary>
public static class KeyboardManager
{
    /// <summary>
    /// Whether keyboard support is enabled. Uses centralized feature flag.
    /// </summary>
    public static bool IsEnabled => CosmosFeatures.KeyboardEnabled;

    /// <summary>
    /// The published keyboards. Replaced on every change, never changed in
    /// place: a keyboard comes or goes on the kit worker, in the probe that
    /// published it or the teardown that withdrew it, while a key report
    /// walks nothing (the consumer hands the scan code straight to the
    /// handler) and the indicator item walks the list on the worker. Changed
    /// by the kit worker only.
    /// </summary>
    private static PublishedDevice[]? s_keyboards;

    /// <summary>
    /// The one kit-owned work item that lights the indicators on every
    /// keyboard; created by <see cref="Initialize"/>, scheduled by the toggle
    /// of a lock key.
    /// </summary>
    private static WorkItem? s_ledsWork;

    /// <summary>Whether the refused indicator schedule of a kernel with no kit worker has been logged.</summary>
    private static bool s_ledsRefusedLogged;

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
    /// driver stage runs, so the keyboard consumer it installs sees every
    /// keyboard a kit driver publishes.
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
        s_keyboards = [];
        s_ledsWork = new WorkItem(ApplyLeds, binding: null);
        DeviceRegistry.SetConsumer(DeviceKind.Keyboard, new KitKeyboardConsumer());
    }

    /// <summary>
    /// Registers a published keyboard with the manager. Thread context, the
    /// kit worker when a published keyboard is consumed; the list is
    /// replaced, never changed in place.
    /// </summary>
    /// <param name="keyboard">The keyboard to register; nothing when the manager is not initialized.</param>
    internal static void RegisterKeyboard(PublishedDevice keyboard)
    {
        if (s_keyboards is null)
        {
            return;
        }

        s_keyboards = [.. s_keyboards, keyboard];

        Core.IO.Serial.Write("[KeyboardManager] Registered keyboard, total: ");
        Core.IO.Serial.WriteNumber((uint)s_keyboards.Length);
        Core.IO.Serial.Write("\n");
    }

    /// <summary>
    /// Forgets a keyboard that is gone (withdrawn by its driver's teardown, a
    /// USB keyboard pulled out among them). Its keys stop arriving; a
    /// modifier it held down stays down until pressed on another keyboard.
    /// Thread context, the kit worker in a teardown; the list is replaced,
    /// never changed in place.
    /// </summary>
    /// <param name="keyboard">The keyboard to remove; nothing when it is not registered.</param>
    internal static void UnregisterKeyboard(PublishedDevice keyboard)
    {
        if (s_keyboards is null)
        {
            return;
        }

        List<PublishedDevice> kept = new(s_keyboards.Length);
        foreach (PublishedDevice other in s_keyboards)
        {
            if (!ReferenceEquals(other, keyboard))
            {
                kept.Add(other);
            }
        }

        s_keyboards = kept.ToArray();

        Core.IO.Serial.Write("[KeyboardManager] Unregistered keyboard, total: ");
        Core.IO.Serial.WriteNumber((uint)s_keyboards.Length);
        Core.IO.Serial.Write("\n");
    }

    /// <summary>
    /// Enqueues the given key-press event to the internal keyboard buffer.
    /// Runs in the sink caller's context, an interrupt included, against
    /// readers that are always in thread context, so every touch of the
    /// queue masks interrupts for its duration. <see cref="Queue{T}"/> is not
    /// reentrant: an enqueue that grows the queue reallocates the backing
    /// array and rehomes its head while a reader may be indexing the old one.
    /// </summary>
    private static void Enqueue(KeyEvent keyEvent)
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            s_queuedKeys?.Enqueue(keyEvent);
        }
    }

    /// <summary>
    /// Handles one key report by its set 1 scan code: toggles the lock keys
    /// and lights the indicators, tracks the modifiers, converts a make
    /// through the active layout and queues the event. Sink caller's context,
    /// an interrupt included; called by <see cref="KitKeyboardConsumer.OnKey"/>.
    /// </summary>
    /// <param name="scanCode">The set 1 scan code, the right Alt as <see cref="ScanMapBase.RightAltScanCode"/>.</param>
    /// <param name="released">True for a key release.</param>
    internal static void HandleScanCode(byte scanCode, bool released)
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
                if (GetKey(key, out KeyEvent? keyInfo))
                {
                    Enqueue(keyInfo);
                }
            }
        }
    }

    /// <summary>
    /// Schedules the indicator item. Any context, an interrupt included;
    /// allocation-free. A schedule refused because the item is already
    /// queued is the coalescing the indicator item relies on and is ignored;
    /// a kernel with no kit worker is logged once and otherwise ignored.
    /// </summary>
    private static void UpdateLeds()
    {
        WorkItem? work = s_ledsWork;
        if (work is null)
        {
            return;
        }

        if (work.Schedule() || DriverEngine.HasWorker)
        {
            return;
        }

        if (!s_ledsRefusedLogged)
        {
            s_ledsRefusedLogged = true;
            Core.IO.Serial.Write("[KeyboardManager] No kit worker: the indicators stay as they are\n");
        }
    }

    /// <summary>
    /// The indicator work item. Thread context on the kit worker. Reads the
    /// lock states the manager holds now, so coalesced schedules apply the
    /// latest, and skips a keyboard withdrawn meanwhile, since its contract
    /// object must not be used after that.
    /// </summary>
    private static void ApplyLeds()
    {
        KeyboardLeds leds = KeyboardLeds.None;
        if (NumLock)
        {
            leds |= KeyboardLeds.NumLock;
        }

        if (CapsLock)
        {
            leds |= KeyboardLeds.CapsLock;
        }

        if (ScrollLock)
        {
            leds |= KeyboardLeds.ScrollLock;
        }

        PublishedDevice[]? keyboards = s_keyboards;
        if (keyboards is null)
        {
            return;
        }

        for (int i = 0; i < keyboards.Length; i++)
        {
            PublishedDevice device = keyboards[i];
            if (device.IsWithdrawn)
            {
                continue;
            }

            ((IKeyboard)device.Device).SetLeds(leds);
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
    private static bool GetKey(byte scanCode, [NotNullWhen(true)] out KeyEvent? keyInfo)
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
            CapsLock);
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
            // The mask covers the take and nothing else: the halt below waits
            // for the very interrupt this scope masks.
            using (InternalCpu.DisableInterruptsScope())
            {
                if (s_queuedKeys.TryDequeue(out KeyEvent? key))
                {
                    return key;
                }
            }

            // The halt waits for the interrupt, or the worker's tick, that
            // fills the queue.
            PlatformHAL.CpuOps?.Halt();
        }
    }

    /// <summary>
    /// Gets the scan map that turns scan codes into characters.
    /// </summary>
    /// <returns>The active layout, or <see langword="null"/> before the
    /// manager was initialized and when keyboard support is compiled out;
    /// initialization installs <see cref="ScanMaps.USStandardLayout"/>.</returns>
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
