namespace Cosmos.System
{
    /// <summary>
    /// KeyMapping class. Used to map keyboard.
    /// </summary>
    public class KeyMapping
    {
        /// <summary>
        /// Scan code.
        /// </summary>
        public byte Scancode;

        /// <summary>
        /// Value.
        /// </summary>
        public char Value;

        /// <summary>
        /// Shift.
        /// </summary>
        public char Shift;

        /// <summary>
        /// Num.
        /// </summary>
        public char Num;

        /// <summary>
        /// Caps.
        /// </summary>
        public char Caps;

        /// <summary>
        /// Shift and Caps.
        /// </summary>
        public char ShiftCaps;

        /// <summary>
        /// Shift and Num.
        /// </summary>
        public char ShiftNum;

        /// <summary>
        /// Ctrl.
        /// </summary>
        public char Control;

        /// <summary>
        /// Ctrl and Alt.
        /// </summary>
        public char ControlAlt;

        /// <summary>
        /// Ctrl and Shift.
        /// </summary>
        public char ControlShift;

        /// <summary>
        /// Ctrl, Alt and Shift.
        /// </summary>
        public char ControlAltShift;

        /// <summary>
        /// Key.
        /// </summary>
        public ConsoleKeyEx Key;

        /// <summary>
        /// NumLock key.
        /// </summary>
        public ConsoleKeyEx NumLockKey;

        /// <summary>
        /// Create new instance of the <see cref="KeyMapping"/> class.
        /// </summary>
        /// <param name="aScanCode">A scan code.</param>
        /// <param name="norm">Norm.</param>
        /// <param name="shift">Shift.</param>
        /// <param name="num">Num.</param>
        /// <param name="caps">Caps.</param>
        /// <param name="shiftcaps">Shift and Caps.</param>
        /// <param name="shiftnum">Shift and Num</param>
        /// <param name="altgr">Ctrl and Alt.</param>
        /// <param name="shiftaltgr">Ctrl, Alt and Shift.</param>
        /// <param name="ctrl">Ctrl.</param>
        /// <param name="shiftctrl">Shift and Ctrl.</param>
        /// <param name="aKey">A key.</param>
        /// <param name="numKey">NumLock key.</param>
        public KeyMapping(byte aScanCode, char norm, char shift, char num, char caps, char shiftcaps, char shiftnum, char altgr, char shiftaltgr, char ctrl, char shiftctrl, ConsoleKeyEx aKey, ConsoleKeyEx numKey)
        {
            Scancode = aScanCode;
            Value = norm;
            Shift = shift;
            Num = num;
            Caps = caps;
            ShiftCaps = shiftcaps;
            ShiftNum = shiftnum;
            Key = aKey;
            NumLockKey = aKey;
            ControlAlt = altgr;
            Control = ctrl;
            ControlAltShift = shiftaltgr;
            ControlShift = shiftctrl;
            NumLockKey = numKey;
        }

        /// <summary>
        /// Create new instance of the <see cref="KeyMapping"/> class.
        /// </summary>
        /// <param name="aScanCode">A scan code.</param>
        /// <param name="norm">Norm.</param>
        /// <param name="shift">Shift.</param>
        /// <param name="num">Num.</param>
        /// <param name="caps">Caps.</param>
        /// <param name="shiftcaps">Shift and Caps.</param>
        /// <param name="shiftnum">Shift and Num</param>
        /// <param name="altgr">Ctrl and Alt.</param>
        /// <param name="shiftaltgr">Ctrl, Alt and Shift.</param>
        /// <param name="aKey">A key.</param>
        /// <param name="numKey">NumLock key.</param>
        public KeyMapping(byte aScanCode, char norm, char shift, char num, char caps, char shiftcaps, char shiftnum, char altgr, char shiftaltgr, ConsoleKeyEx aKey, ConsoleKeyEx numKey)
            : this(aScanCode, norm, shift, num, caps, shiftcaps, shiftnum, altgr, shiftaltgr, '\0', '\0', aKey, numKey)
        {
        }

        /// <summary>
        /// Create new instance of the <see cref="KeyMapping"/> class.
        /// </summary>
        /// <param name="aScanCode">A scan code.</param>
        /// <param name="norm">Norm.</param>
        /// <param name="shift">Shift.</param>
        /// <param name="num">Num.</param>
        /// <param name="caps">Caps.</param>
        /// <param name="shiftcaps">Shift and Caps.</param>
        /// <param name="shiftnum">Shift and Num</param>
        /// <param name="altgr">Ctrl and Alt.</param>
        /// <param name="aKey">A key.</param>
        /// <param name="numKey">NumLock key.</param>
        public KeyMapping(byte aScanCode, char norm, char shift, char num, char caps, char shiftcaps, char shiftnum, char altgr, ConsoleKeyEx aKey, ConsoleKeyEx numKey)
            : this(aScanCode, norm, shift, num, caps, shiftcaps, shiftnum, altgr, '\0', '\0', '\0', aKey, numKey)
        {
        }

        /// <summary>
        /// Create new instance of the <see cref="KeyMapping"/> class.
        /// </summary>
        /// <param name="aScanCode">A scan code.</param>
        /// <param name="norm">Norm.</param>
        /// <param name="shift">Shift.</param>
        /// <param name="num">Num.</param>
        /// <param name="caps">Caps.</param>
        /// <param name="shiftcaps">Shift and Caps.</param>
        /// <param name="shiftnum">Shift and Num</param>
        /// <param name="altgr">Ctrl and Alt.</param>
        /// <param name="shiftaltgr">Ctrl, Alt and Shift.</param>
        /// <param name="ctrl">Ctrl.</param>
        /// <param name="shiftctrl">Shift and Ctrl.</param>
        /// <param name="aKey">A key.</param>
        public KeyMapping(byte aScanCode, char norm, char shift, char num, char caps, char shiftcaps, char shiftnum, char altgr, char shiftaltgr, char ctrl, char shiftctrl, ConsoleKeyEx aKey)
            : this(aScanCode, norm, shift, num, caps, shiftcaps, shiftnum, altgr, shiftaltgr, ctrl, shiftctrl, aKey, aKey)
        {
        }

        /// <summary>
        /// Create new instance of the <see cref="KeyMapping"/> class.
        /// </summary>
        /// <param name="aScanCode">A scan code.</param>
        /// <param name="norm">Norm.</param>
        /// <param name="shift">Shift.</param>
        /// <param name="num">Num.</param>
        /// <param name="caps">Caps.</param>
        /// <param name="shiftcaps">Shift and Caps.</param>
        /// <param name="shiftnum">Shift and Num</param>
        /// <param name="altgr">Ctrl and Alt.</param>
        /// <param name="shiftaltgr">Ctrl, Alt and Shift.</param>
        /// <param name="aKey">A key.</param>
        public KeyMapping(byte aScanCode, char norm, char shift, char num, char caps, char shiftcaps, char shiftnum, char altgr, char shiftaltgr, ConsoleKeyEx aKey)
            : this(aScanCode, norm, shift, num, caps, shiftcaps, shiftnum, altgr, shiftaltgr, '\0', '\0', aKey)
        {
        }

        /// <summary>
        /// Create new instance of the <see cref="KeyMapping"/> class.
        /// </summary>
        /// <param name="aScanCode">A scan code.</param>
        /// <param name="norm">Norm.</param>
        /// <param name="shift">Shift.</param>
        /// <param name="num">Num.</param>
        /// <param name="caps">Caps.</param>
        /// <param name="shiftcaps">Shift and Caps.</param>
        /// <param name="shiftnum">Shift and Num</param>
        /// <param name="altgr">Ctrl and Alt.</param>
        /// <param name="aKey">A key.</param>
        public KeyMapping(byte aScanCode, char norm, char shift, char num, char caps, char shiftcaps, char shiftnum, char altgr, ConsoleKeyEx aKey)
            : this(aScanCode, norm, shift, num, caps, shiftcaps, shiftnum, altgr, '\0', '\0', '\0', aKey)
        {
        }

        /// <summary>
        /// Create new instance of the <see cref="KeyMapping"/> class.
        /// </summary>
        /// <param name="aScanCode">A scan code.</param>
        /// <param name="norm">Norm.</param>
        /// <param name="shift">Shift.</param>
        /// <param name="num">Num.</param>
        /// <param name="caps">Caps.</param>
        /// <param name="shiftcaps">Shift and Caps.</param>
        /// <param name="shiftnum">Shift and Num</param>
        /// <param name="aKey">A key.</param>
        public KeyMapping(byte aScanCode, char norm, char shift, char num, char caps, char shiftcaps, char shiftnum, ConsoleKeyEx aKey)
            : this(aScanCode, norm, shift, num, caps, shiftcaps, shiftnum, '\0', '\0', '\0', '\0', aKey)
        {
        }

        /// <summary>
        /// Create new instance of the <see cref="KeyMapping"/> class.
        /// </summary>
        /// <param name="aScanCode">A scan code.</param>
        /// <param name="norm">Norm.</param>
        /// <param name="shift">Shift.</param>
        /// <param name="num">Num.</param>
        /// <param name="caps">Caps.</param>
        /// <param name="shiftcaps">Shift and Caps.</param>
        /// <param name="shiftnum">Shift and Num</param>
        /// <param name="aKey">A key.</param>
        /// <param name="numKey">NumLock key.</param>
        public KeyMapping(byte aScanCode, char norm, char shift, char num, char caps, char shiftcaps, char shiftnum, ConsoleKeyEx aKey, ConsoleKeyEx numKey)
            : this(aScanCode, norm, shift, num, caps, shiftcaps, shiftnum, '\0', aKey, numKey)
        {
        }

        /// <summary>
        /// Create new instance of the <see cref="KeyMapping"/> class.
        /// </summary>
        /// <param name="aScanCode">A scan code.</param>
        /// <param name="norm">Norm.</param>
        /// <param name="shift">Shift.</param>
        /// <param name="num">Num.</param>
        /// <param name="caps">Caps.</param>
        /// <param name="shiftcaps">Shift and Caps.</param>
        /// <param name="shiftnum">Shift and Num</param>
        /// <param name="aKey">A key.</param>
        public KeyMapping(byte aScanCode, int norm, int shift, int num, int caps, int shiftcaps, int shiftnum, ConsoleKeyEx aKey)
            : this(aScanCode, (char)norm, (char)shift, (char)num, (char)caps, (char)shiftcaps, (char)shiftnum, aKey)
        {
        }

        /// <summary>
        /// Create new instance of the <see cref="KeyMapping"/> class.
        /// </summary>
        /// <param name="aScanCode">A scan code.</param>
        /// <param name="num">Num.</param>
        /// <param name="aKey">A key.</param>
        /// <param name="numKey">NumLock key.</param>
        public KeyMapping(byte aScanCode, char num, ConsoleKeyEx aKey, ConsoleKeyEx numKey)
            : this(aScanCode, '\0', '\0', num, '\0', '\0', '\0', aKey, numKey)
        {
        }

        /// <summary>
        /// Create new instance of the <see cref="KeyMapping"/> class.
        /// </summary>
        /// <param name="aScanCode">A scan code.</param>
        /// <param name="n">All control keys char.</param>
        /// <param name="aKey">A key.</param>
        public KeyMapping(byte aScanCode, char n, ConsoleKeyEx aKey)
            : this(aScanCode, n, n, n, n, n, n, aKey)
        {
        }

        /// <summary>
        /// Create new instance of the <see cref="KeyMapping"/> class.
        /// </summary>
        /// <param name="aScanCode">A scan code.</param>
        /// <param name="aKey">A key.</param>
        public KeyMapping(byte aScanCode, ConsoleKeyEx aKey)
            : this(aScanCode, '\0', '\0', '\0', '\0', '\0', '\0', aKey)
        {
        }
    }
}
