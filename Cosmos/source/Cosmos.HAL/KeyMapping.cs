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
        public ConsoleKeyEx Key;
        public ConsoleKeyEx NumLockKey;

        public KeyMapping(uint aScanCode, char norm, char shift, char num, char caps, char shiftcaps, char shiftnum, ConsoleKeyEx aKey)
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

        public KeyMapping(uint aScanCode, char norm, char shift, char num, char caps, char shiftcaps, char shiftnum, ConsoleKeyEx aKey, ConsoleKeyEx numKey)
            : this(aScanCode, norm, shift, num, caps, shiftcaps, shiftnum, aKey)
        {
            NumLockKey = numKey;
        }

        public KeyMapping(uint aScanCode, char num, ConsoleKeyEx aKey, ConsoleKeyEx numKey) 
            : this(aScanCode, '\0', '\0', num, '\0', '\0', '\0', aKey, numKey)
        {
        }

        public KeyMapping(uint aScanCode, int norm, int shift, int num, int caps, int shiftcaps, int shiftnum, ConsoleKeyEx aKey)
            : this(aScanCode, (char)norm, (char)shift, (char)num, (char)caps, (char)shiftcaps, (char)shiftnum, aKey)
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