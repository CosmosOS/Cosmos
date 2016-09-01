namespace Cosmos.HAL
{
    public class KeyMapping
    {
        public uint Scancode;
        public char Value;
        public char Shift;
        public char Num;
        public char Caps;
        public char ShiftCaps;
        public char ShiftNum;
        public char Control;
        public char ControlAlt;
        public char ControlShift;
        public char ControlAltShift;
        public ConsoleKeyEx Key;
        public ConsoleKeyEx NumLockKey;

        public KeyMapping(uint aScanCode, char norm, char shift, char num, char caps, char shiftcaps, char shiftnum, char altgr, char shiftaltgr, char ctrl, char shiftctrl, ConsoleKeyEx aKey, ConsoleKeyEx numKey)
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

        public KeyMapping(uint aScanCode, char norm, char shift, char num, char caps, char shiftcaps, char shiftnum, char altgr, char shiftaltgr, ConsoleKeyEx aKey, ConsoleKeyEx numKey)
            : this(aScanCode, norm, shift, num, caps, shiftcaps, shiftnum, altgr, shiftaltgr, '\0', '\0', aKey, numKey)
        {
        }

        public KeyMapping(uint aScanCode, char norm, char shift, char num, char caps, char shiftcaps, char shiftnum, char altgr, ConsoleKeyEx aKey, ConsoleKeyEx numKey)
            : this(aScanCode, norm, shift, num, caps, shiftcaps, shiftnum, altgr, '\0', '\0', '\0', aKey, numKey)
        {
        }

        public KeyMapping(uint aScanCode, char norm, char shift, char num, char caps, char shiftcaps, char shiftnum, char altgr, char shiftaltgr, char ctrl, char shiftctrl, ConsoleKeyEx aKey)
            : this(aScanCode, norm, shift, num, caps, shiftcaps, shiftnum, altgr, shiftaltgr, ctrl, shiftctrl, aKey, aKey)
        {
        }

        public KeyMapping(uint aScanCode, char norm, char shift, char num, char caps, char shiftcaps, char shiftnum, char altgr, char shiftaltgr, ConsoleKeyEx aKey)
            : this (aScanCode, norm, shift, num, caps, shiftcaps, shiftnum, altgr, shiftaltgr, '\0', '\0', aKey)
        {
        }

        public KeyMapping(uint aScanCode, char norm, char shift, char num, char caps, char shiftcaps, char shiftnum, char altgr, ConsoleKeyEx aKey)
            : this(aScanCode, norm, shift, num, caps, shiftcaps, shiftnum, altgr, '\0', '\0', '\0', aKey)
        {
        }

        public KeyMapping(uint aScanCode, char norm, char shift, char num, char caps, char shiftcaps, char shiftnum, ConsoleKeyEx aKey)
            : this(aScanCode, norm, shift, num, caps, shiftcaps, shiftnum, '\0', '\0', '\0', '\0', aKey)
        {
        }

        public KeyMapping(uint aScanCode, char norm, char shift, char num, char caps, char shiftcaps, char shiftnum, ConsoleKeyEx aKey, ConsoleKeyEx numKey)
            : this(aScanCode, norm, shift, num, caps, shiftcaps, shiftnum, '\0', aKey, numKey)
        {
        }

        public KeyMapping(uint aScanCode, int norm, int shift, int num, int caps, int shiftcaps, int shiftnum, ConsoleKeyEx aKey)
            : this(aScanCode, (char)norm, (char)shift, (char)num, (char)caps, (char)shiftcaps, (char)shiftnum, aKey)
        {
        }

        public KeyMapping(uint aScanCode, char num, ConsoleKeyEx aKey, ConsoleKeyEx numKey)
            : this(aScanCode, '\0', '\0', num, '\0', '\0', '\0', aKey, numKey)
        {
        }

        public KeyMapping(uint aScanCode, char n, ConsoleKeyEx aKey) 
            : this(aScanCode, n, n, n, n, n, n, aKey)
        {
        }

        public KeyMapping(uint aScanCode, ConsoleKeyEx aKey)
            : this(aScanCode, '\0', '\0', '\0', '\0', '\0', '\0', aKey)
        {
        }
    }
}