using System;

namespace Cosmos.CPU.x86 {
    static public class TempDebug {
        unsafe static byte* mPtr = (byte*)(0xB8000 - 1);
        static public void ShowText(char aChar) {
            unsafe {
                mPtr++;
                *mPtr = (byte)aChar;

                mPtr++;
                *mPtr = 0x0A;
            }
        }
    }
}
