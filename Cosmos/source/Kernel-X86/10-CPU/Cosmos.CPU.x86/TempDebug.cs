using System;

namespace Cosmos.CPU.x86 {
    static public class TempDebug {
        static public void ShowText(byte aChar, int aOffset) {
            unsafe {
                byte* xTest = (byte*)0xB8000 + aOffset * 2;
                *xTest = aChar;

                xTest++;
                *xTest = 0x0A;
            }
        }
    }
}
