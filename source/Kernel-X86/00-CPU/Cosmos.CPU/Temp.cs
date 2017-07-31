using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.CPU {
    public static class Temp {
        public static void ShowText() {
            unsafe {
                byte* xTest = (byte*)0xB8000;
                *xTest = 65;

                xTest = (byte*)0xB8001;
                *xTest = 90;
            }
        }
    }
}
