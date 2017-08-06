using System;

namespace Cosmos.CPU {
    public static class Temp {
        public static void ShowText() {
            unsafe {
                byte* xTest = (byte*)0xB8000;
                *xTest = 65;
                xTest = (byte*)0xB8001;
                *xTest = 0x0A;

                xTest = (byte*)0xB8002;
                *xTest = 90;
                xTest = (byte*)0xB8003;
                *xTest = 0x0A;
            }
        }
    }
}
