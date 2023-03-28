#pragma warning disable IDE0049 // Use framework type

namespace Cosmos.System
{
    /// <summary>
    /// Represents a physical to virtual key mapping.
    /// </summary>
    public class KeyMapping
    {
        /// <summary>
        /// The physical scan-code that the mapping refers to.
        /// </summary>
        public byte ScanCode;

        /// <summary>
        /// The text character value of the key with no modifiers active.
        /// </summary>
        public char Value;

        /// <summary>
        /// The text character value of the key with the Control
        /// (Ctrl) key modifier being active.
        /// </summary>
        public char Control;

        /// <summary>
        /// The text character value of the key with the Shift
        /// key modifier being active.
        /// </summary>
        public char Shift;

        /// <summary>
        /// The text character value of the key with the Num Lock
        /// key modifier being active.
        /// </summary>
        public char NumLock;

        /// <summary>
        /// The text character value of the key with the Caps Lock
        /// key modifier being active.
        /// </summary>
        public char CapsLock;

        /// <summary>
        /// The text character value of the key with both the Caps Lock
        /// and Num Lock key modifiers being active.
        /// </summary>
        public char ShiftCapsLock;

        /// <summary>
        /// The text character value of the key with both the Shift
        /// and Num Lock key modifiers being active.
        /// </summary>
        public char ShiftNumLock;

        /// <summary>
        /// The text character value of the key with both the Control
        /// and Alt key modifiers being active.
        /// </summary>
        public char ControlAlt;

        /// <summary>
        /// The text character value of the key with both the Control
        /// and Shift key modifiers being active.
        /// </summary>
        public char ControlShift;

        /// <summary>
        /// The text character value of the key with both the Control,
        /// Alt, and Shift key modifiers being active.
        /// </summary>
        public char ControlAltShift;

        /// <summary>
        /// The virtual key that the physical key-press maps to.
        /// </summary>
        public ConsoleKeyEx Key;

        /// <summary>
        /// The virtual key that the physical key-press maps to when
        /// the Num Lock modifier is active.
        /// </summary>
        public ConsoleKeyEx NumLockKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyMapping"/> class.
        /// </summary>
        /// <param name="scanCode">The physical scan code of the key.</param>
        /// <param name="normal">The text character value of the key with no modifiers being active.</param>
        /// <param name="shift">The text character value of the key with the Shift modifier being active.</param>
        /// <param name="num">The text character value of the key with the Num Lock modifier being active.</param>
        /// <param name="caps">The text character value of the key with the Caps Lock modifier being active.</param>
        /// <param name="shiftCapsLock">The text character value of the key with the Shift and Caps Lock modifiers being active.</param>
        /// <param name="shiftNumLock">The text character value of the key with the Shift and Num Lock modifiers being active</param>
        /// <param name="ctrlAlt">The text character value of the key with the Control and Alt modifiers being active.</param>
        /// <param name="ctrlAltShift">The text character value of the key with the Control, Alt, and Shift modifiers being active</param>
        /// <param name="ctrl">The text character value of the key with the Control modifier being active.</param>
        /// <param name="shiftCtrl">The text character value of the key with the Shift and Control modifiers being active.</param>
        /// <param name="key">The virtual key that the physical key-press maps to.</param>
        /// <param name="numKey">The virtual key that the physical key-press maps to when the Num Lock modifier is active..</param>
        public KeyMapping(byte scanCode, char normal, char shift, char num, char caps, char shiftCapsLock, char shiftNumLock, char ctrlAlt, char ctrlAltShift, char ctrl, char shiftCtrl, ConsoleKeyEx key, ConsoleKeyEx numKey)
        {
            ScanCode = scanCode;
            Value = normal;
            Shift = shift;
            NumLock = num;
            CapsLock = caps;
            ShiftCapsLock = shiftCapsLock;
            ShiftNumLock = shiftNumLock;
            Key = key;
            NumLockKey = key;
            ControlAlt = ctrlAlt;
            Control = ctrl;
            ControlAltShift = ctrlAltShift;
            ControlShift = shiftCtrl;
            NumLockKey = numKey;
        }

        /// <inheritdoc cref="KeyMapping(byte, char, char, char, char, char, char, char, char, char, char, ConsoleKeyEx, ConsoleKeyEx)"/>
        public KeyMapping(byte scanCode, char normal, char shift, char numLock, char capsLock, char shiftCapsLock, char shiftNumLock, char ctrlAlt, char ctrlAltShift, ConsoleKeyEx key, ConsoleKeyEx numKey)
            : this(scanCode, normal, shift, numLock, capsLock, shiftCapsLock, shiftNumLock, ctrlAlt, ctrlAltShift, '\0', '\0', key, numKey)
        {
        }

        /// <inheritdoc cref="KeyMapping(byte, char, char, char, char, char, char, char, char, char, char, ConsoleKeyEx, ConsoleKeyEx)"/>
        public KeyMapping(byte scanCode, char normal, char shift, char numLock, char capsLock, char shiftCapsLock, char shiftNumLock, char ctrlAlt, ConsoleKeyEx key, ConsoleKeyEx numKey)
            : this(scanCode, normal, shift, numLock, capsLock, shiftCapsLock, shiftNumLock, ctrlAlt, '\0', '\0', '\0', key, numKey)
        {
        }

        /// <inheritdoc cref="KeyMapping(byte, char, char, char, char, char, char, char, char, char, char, ConsoleKeyEx, ConsoleKeyEx)"/>
        public KeyMapping(byte scanCode, char normal, char shift, char numLock, char capsLock, char shiftCapsLock, char shiftNumLock, char ctrlAlt, char ctrlAltShift, char ctrl, char shiftCtrl, ConsoleKeyEx key)
            : this(scanCode, normal, shift, numLock, capsLock, shiftCapsLock, shiftNumLock, ctrlAlt, ctrlAltShift, ctrl, shiftCtrl, key, key)
        {
        }

        /// <inheritdoc cref="KeyMapping(byte, char, char, char, char, char, char, char, char, char, char, ConsoleKeyEx, ConsoleKeyEx)"/>
        public KeyMapping(byte scanCode, char normal, char shift, char numLock, char capsLock, char shiftCapsLock, char shiftNumLock, char ctrlAlt, char ctrlAltShift, ConsoleKeyEx aKey)
            : this(scanCode, normal, shift, numLock, capsLock, shiftCapsLock, shiftNumLock, ctrlAlt, ctrlAltShift, '\0', '\0', aKey)
        {
        }


        /// <inheritdoc cref="KeyMapping(byte, char, char, char, char, char, char, char, char, char, char, ConsoleKeyEx, ConsoleKeyEx)"/>
        public KeyMapping(byte scanCode, char normal, char shift, char num, char capsLock, char shiftCapsLock, char shiftNumLock, char ctrlAlt, ConsoleKeyEx key)
            : this(scanCode, normal, shift, num, capsLock, shiftCapsLock, shiftNumLock, ctrlAlt, '\0', '\0', '\0', key)
        {
        }

        /// <inheritdoc cref="KeyMapping(byte, char, char, char, char, char, char, char, char, char, char, ConsoleKeyEx, ConsoleKeyEx)"/>
        public KeyMapping(byte scanCode, char normal, char shift, char numLock, char capsLock, char shiftCapsLock, char shiftNumLock, ConsoleKeyEx key)
            : this(scanCode, normal, shift, numLock, capsLock, shiftCapsLock, shiftNumLock, '\0', '\0', '\0', '\0', key)
        {
        }

        /// <inheritdoc cref="KeyMapping(byte, char, char, char, char, char, char, char, char, char, char, ConsoleKeyEx, ConsoleKeyEx)"/>
        public KeyMapping(byte scanCode, char normal, char shift, char numLock, char capsLock, char shiftCapsLock, char shiftNumLock, ConsoleKeyEx key, ConsoleKeyEx numKey)
            : this(scanCode, normal, shift, numLock, capsLock, shiftCapsLock, shiftNumLock, '\0', key, numKey)
        {
        }

        /// <inheritdoc cref="KeyMapping(byte, char, char, char, char, char, char, char, char, char, char, ConsoleKeyEx, ConsoleKeyEx)"/>
        public KeyMapping(byte scanCode, char numLock, ConsoleKeyEx key, ConsoleKeyEx numKey)
            : this(scanCode, '\0', '\0', numLock, '\0', '\0', '\0', key, numKey)
        {
        }

        /// <inheritdoc cref="KeyMapping(byte, char, char, char, char, char, char, char, char, char, char, ConsoleKeyEx, ConsoleKeyEx)"/>
        /// <param name="n">The character to use for all of the text character fields.</param>
        public KeyMapping(byte scanCode, char n, ConsoleKeyEx key)
            : this(scanCode, n, n, n, n, n, n, key)
        {
        }

        /// <inheritdoc cref="KeyMapping(byte, char, char, char, char, char, char, char, char, char, char, ConsoleKeyEx, ConsoleKeyEx)"/>
        public KeyMapping(byte scanCode, ConsoleKeyEx key)
            : this(scanCode, '\0', '\0', '\0', '\0', '\0', '\0', key)
        {
        }
    }
}
