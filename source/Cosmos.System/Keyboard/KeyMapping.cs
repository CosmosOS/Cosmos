namespace Cosmos.System
{
    public class KeyMapping
    {
        public byte Scancode;
        public char Value;
        public char Shift;
        public char Num;
        public char Caps;
        public char ShiftCaps;
        public char ShiftNum;
        public ConsoleKeyEx Key;
        public ConsoleKeyEx NumLockKey;

        public KeyMapping(byte aScanCode, char norm, char shift, char num, char caps, char shiftcaps, char shiftnum, ConsoleKeyEx aKey)
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
        }

        public KeyMapping(byte aScanCode, char norm, char shift, char num, char caps, char shiftcaps, char shiftnum, ConsoleKeyEx aKey, ConsoleKeyEx numKey)
            : this(aScanCode, norm, shift, num, caps, shiftcaps, shiftnum, aKey)
        {
            NumLockKey = numKey;
        }

        public KeyMapping(byte aScanCode, char num, ConsoleKeyEx aKey, ConsoleKeyEx numKey)
            : this(aScanCode, '\0', '\0', num, '\0', '\0', '\0', aKey, numKey)
        {
        }

        public KeyMapping(byte aScanCode, int norm, int shift, int num, int caps, int shiftcaps, int shiftnum, ConsoleKeyEx aKey)
            : this(aScanCode, (char)norm, (char)shift, (char)num, (char)caps, (char)shiftcaps, (char)shiftnum, aKey)
        {
        }

        public KeyMapping(byte aScanCode, char n, ConsoleKeyEx aKey)
            : this(aScanCode, n, n, n, n, n, n, aKey)
        {
        }

        public KeyMapping(byte aScanCode, ConsoleKeyEx aKey)
            : this(aScanCode, '\0', '\0', '\0', '\0', '\0', '\0', aKey)
        {
        }
    }
}
