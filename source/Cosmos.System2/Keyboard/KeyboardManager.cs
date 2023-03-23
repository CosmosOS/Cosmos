using System.Collections.Generic;
using Cosmos.HAL;
using Cosmos.System.ScanMaps;
using System;

namespace Cosmos.System
{
    /// <summary>
    /// Manages the physical keyboard.
    /// </summary>
    public static class KeyboardManager
    {
        readonly static List<KeyboardBase> keyboardList = new();
        readonly static Queue<KeyEvent> queuedKeys = new();
        static ScanMapBase scanMap = new USStandardLayout();

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
        /// Whether a keyboard input is pending to be processed; i.e, whether the queued
        /// key-press buffer is not empty.
        /// </summary>
        public static bool KeyAvailable => queuedKeys.Count > 0;

        static KeyboardManager()
        {
            foreach (var keyboard in HAL.Global.GetKeyboardDevices())
            {
                AddKeyboard(keyboard);
            }
        }

        /// <summary>
        /// Enqueues the given key-press event to the internal keyboard buffer.
        /// </summary>
        /// <param name="keyEvent">The <see cref="KeyEvent"/> to enqueue.</param>
        private static void Enqueue(KeyEvent keyEvent)
        {
            queuedKeys.Enqueue(keyEvent);
        }

        /// <summary>
        /// Handles an emulated key-press by its scan-code. Used for test kernels
        /// </summary>
        /// <param name="scanCode">The scan code of the virtual key-press.</param>
        /// <param name="released">Whether the key has been pressed or released.</param>
        /// <exception cref="global::System.IO.IOException">An I/O error has occurred.</exception>
        internal static void HandleFakeScanCode(byte scanCode, bool released)
        {
            HandleScanCode(scanCode, released);
        }

        /// <summary>
        /// Handles a key-press by its physical key scan-code.
        /// </summary>
        /// <param name="aScanCode">The physical scan code of the key-press.</param>
        /// <param name="aReleased">Whether the key has been pressed or released.</param>
        /// <exception cref="global::System.IO.IOException">An I/O error occurred.</exception>
        private static void HandleScanCode(byte aScanCode, bool aReleased)
        {
            byte key = aScanCode;
            if (scanMap.ScanCodeMatchesKey(key, ConsoleKeyEx.CapsLock) && !aReleased) {
                // caps lock
                CapsLock = !CapsLock;
                UpdateLeds();
            }
            else if (scanMap.ScanCodeMatchesKey(key, ConsoleKeyEx.NumLock) && !aReleased) {
                // num lock
                NumLock = !NumLock;
                UpdateLeds();
            }
            else if (scanMap.ScanCodeMatchesKey(key, ConsoleKeyEx.ScrollLock) && !aReleased) {
                // scroll lock
                ScrollLock = !ScrollLock;
                UpdateLeds();
            }
            else if (scanMap.ScanCodeMatchesKey(key, ConsoleKeyEx.LCtrl) || scanMap.ScanCodeMatchesKey(key, ConsoleKeyEx.RCtrl)) {
                ControlPressed = !aReleased;
            }
            else if (scanMap.ScanCodeMatchesKey(key, ConsoleKeyEx.LShift) || scanMap.ScanCodeMatchesKey(key, ConsoleKeyEx.RShift)) {
                ShiftPressed = !aReleased;
            }
            else if (scanMap.ScanCodeMatchesKey(key, ConsoleKeyEx.LAlt) || scanMap.ScanCodeMatchesKey(key, ConsoleKeyEx.RAlt)) {
                AltPressed = !aReleased;
            }
            else {
                if (!aReleased) {
                    if (GetKey(key, out var keyInfo)) {
                        Enqueue(keyInfo);
                    }
                }
            }
        }

        /// <summary>
        /// Updates the keyboard's LEDs.
        /// </summary>
        private static void UpdateLeds()
        {
            foreach (KeyboardBase keyboard in keyboardList)
            {
                keyboard.UpdateLeds();
            }
        }

        /// <summary>
        /// Attempts to convert the given physical key scan-code to a
        /// <see cref="KeyEvent"/> instance.
        /// </summary>
        /// <param name="scanCode">The scan-code of the physical key.</param>
        /// <param name="keyInfo">The resulting <see cref="KeyEvent"/>.</param>
        /// <returns><see langword="true"/> if the operation succeded and <paramref name="keyInfo"/> holds a non-null value; otherwise, <see langword="false"/>.</returns>
        public static bool GetKey(byte scanCode, out KeyEvent keyInfo)
        {
            keyInfo = scanMap.ConvertScanCode(scanCode, ControlPressed, ShiftPressed, AltPressed, NumLock, CapsLock, ScrollLock);
            return keyInfo != null;
        }

        /// <summary>
        /// If available, reads the next key from the pending key-press keyboard buffer,
        /// and removes it from said buffer.
        /// </summary>
        /// <param name="key">The pending key-press.</param>
        /// <returns><see langword="true"/> if a key-press was pending and has been dequeued to <paramref name="key"/>; otherwise, <see langword="false"/>.</returns>
        public static bool TryReadKey(out KeyEvent key)
        {
            if (queuedKeys.Count > 0)
            {
                key = queuedKeys.Dequeue();
                return true;
            }

            key = default;
            return false;
        }

        /// <summary>
        /// Reads the next key from the pending key-press keyboard buffer, and
        /// removes it from said buffer.
        /// </summary>
        /// <returns>The pending key-press.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the queue is empty.</exception>
        public static KeyEvent ReadKey()
        {
            while (queuedKeys.Count == 0)
            {
                KeyboardBase.WaitForKey();
            }

            return queuedKeys.Dequeue();
        }

        /// <summary>
        /// Gets the currently used keyboard layout.
        /// </summary>
        public static ScanMapBase GetKeyLayout()
        {
            return scanMap;
        }

        /// <summary>
        /// Sets the currently used keyboard layout.
        /// </summary>
        /// <param name="scanMap">The keyboard scan map to use.</param>
        public static void SetKeyLayout(ScanMapBase scanMap)
        {
            if (scanMap != null)
            {
                KeyboardManager.scanMap = scanMap;
            }
        }

        /// <summary>
        /// Registers the given physical keyboard device.
        /// </summary>
        /// <param name="keyboard">The keyboard device to add.</param>
        private static void AddKeyboard(KeyboardBase keyboard)
        {
            Global.Debugger.Send($"Registering physical keyboard device #{keyboardList.Count + 1}.");
            keyboard.OnKeyPressed = HandleScanCode;
            keyboardList.Add(keyboard);
        }
    }
}
