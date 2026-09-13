// This code is licensed under the BSD 3-Clause license (see LICENSE for details)
// Ported from Cosmos.System2/Keyboard/KeyboardManager.cs

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Core;
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
    private static bool s_initialized;

    /// <summary>
    /// The num-lock state.
    /// </summary>
    public static bool NumLock { get; set; }

    /// <summary>
    /// The caps-lock state.
    /// </summary>
    public static bool CapsLock { get; set; }

    /// <summary>
    /// The scroll-lock state.
    /// </summary>
    public static bool ScrollLock { get; set; }

    /// <summary>
    /// Whether the Control (Ctrl) key is currently pressed.
    /// </summary>
    public static bool ControlPressed { get; set; }

    /// <summary>
    /// Whether the Shift key is currently pressed.
    /// </summary>
    public static bool ShiftPressed { get; set; }

    /// <summary>
    /// Whether the Alt key is currently pressed.
    /// </summary>
    public static bool AltPressed { get; set; }

    /// <summary>
    /// Whether a keyboard input is pending to be processed.
    /// </summary>
    public static bool KeyAvailable => s_queuedKeys != null && s_queuedKeys.Count > 0;

    private static void ThrowIfDisabled()
    {
        if (!IsEnabled)
        {
            throw new InvalidOperationException("Keyboard support is disabled. Set CosmosEnableKeyboard=true in your csproj to enable it.");
        }
    }

    /// <summary>
    /// Initializes the keyboard manager.
    /// Call RegisterKeyboard() after this to add keyboards.
    /// </summary>
    public static void Initialize()
    {
        ThrowIfDisabled();

        if (s_initialized)
        {
            return;
        }

        s_keyboards = new List<IKeyboardDevice>();
        s_queuedKeys = new Queue<KeyEvent>();
        s_scanMap = new USStandardLayout();

        s_initialized = true;
    }

    /// <summary>
    /// Registers a keyboard device with the manager.
    /// </summary>
    public static void RegisterKeyboard(IKeyboardDevice keyboard)
    {
        if (s_keyboards == null || keyboard == null)
        {
            return;
        }

        keyboard.OnKeyPressed = HandleScanCode;
        s_keyboards.Add(keyboard);

        // Enable keyboard after callback is set (this registers IRQ handler)
        keyboard.Enable();

        Core.IO.Serial.Write("[KeyboardManager] Registered keyboard, total: ");
        Core.IO.Serial.WriteNumber((uint)s_keyboards.Count);
        Core.IO.Serial.Write("\n");
    }

    /// <summary>
    /// Enqueues the given key-press event to the internal keyboard buffer.
    /// </summary>
    private static void Enqueue(KeyEvent keyEvent)
    {
        s_queuedKeys?.Enqueue(keyEvent);
    }

    /// <summary>
    /// Handles a key-press by its physical key scan-code.
    /// </summary>
    public static void HandleScanCode(byte scanCode, bool released)
    {
        if (s_scanMap == null)
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
        if (s_keyboards == null)
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
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static KeyEvent Peek()
    {
        if (s_queuedKeys == null)
        {
            throw new InvalidOperationException("KeyboardManager not initialized!");
        }

        return s_queuedKeys.Peek();
    }

    /// <summary>
    /// Attempts to convert the given physical key scan-code to a KeyEvent.
    /// </summary>
    public static bool GetKey(byte scanCode, out KeyEvent? keyInfo)
    {
        if (s_scanMap == null)
        {
            keyInfo = null;
            return false;
        }
        keyInfo = s_scanMap.ConvertScanCode(scanCode, ControlPressed, ShiftPressed, AltPressed, NumLock, CapsLock, ScrollLock);
        return keyInfo != null;
    }

    /// <summary>
    /// If available, reads the next key from the pending key-press buffer.
    /// </summary>
    public static bool TryReadKey([NotNullWhen(true)] out KeyEvent? key)
    {
        ThrowIfDisabled();

        if (s_queuedKeys != null && s_queuedKeys.Count > 0)
        {
            key = s_queuedKeys.Dequeue();
            return true;
        }

        key = default;
        return false;
    }

    private static bool s_readKeyEntered = false;

    /// <summary>
    /// Reads the next key from the pending key-press buffer, blocking until available.
    /// </summary>
    public static KeyEvent ReadKey()
    {
        ThrowIfDisabled();

        if (!s_readKeyEntered)
        {
            s_readKeyEntered = true;
            Core.IO.Serial.Write("[KeyboardManager] ReadKey() entered\n");
        }

        while (s_queuedKeys == null || s_queuedKeys.Count == 0)
        {
            // Poll all keyboards for events (in case interrupts aren't working)
            PollKeyboards();

            // Halt CPU until interrupt (key press)
            HAL.PlatformHAL.CpuOps?.Halt();
        }

        return s_queuedKeys.Dequeue();
    }

    private static uint s_pollCallCount = 0;

    private static bool s_pollEntered = false;

    /// <summary>
    /// Polls all registered keyboards for events.
    /// </summary>
    private static void PollKeyboards()
    {
        if (!s_pollEntered)
        {
            s_pollEntered = true;
            Core.IO.Serial.Write("[KeyboardManager] PollKeyboards() first call\n");
        }

        if (s_keyboards == null)
        {
            return;
        }

        s_pollCallCount++;
        if (s_pollCallCount % 100 == 0)
        {
            Core.IO.Serial.Write("[KeyboardManager] PollKeyboards #");
            Core.IO.Serial.WriteNumber(s_pollCallCount);
            Core.IO.Serial.Write(" keyboards=");
            Core.IO.Serial.WriteNumber((uint)s_keyboards.Count);
            Core.IO.Serial.Write("\n");
        }

        foreach (var keyboard in s_keyboards)
        {
            keyboard.Poll();
        }
    }

    /// <summary>
    /// Gets the currently used keyboard layout.
    /// </summary>
    public static ScanMapBase? GetKeyLayout() => s_scanMap;

    /// <summary>
    /// Sets the currently used keyboard layout.
    /// </summary>
    public static void SetKeyLayout(ScanMapBase scanMap)
    {
        if (scanMap != null)
        {
            s_scanMap = scanMap;
        }
    }

}
