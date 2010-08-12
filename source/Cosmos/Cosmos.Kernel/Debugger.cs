using System;
using Cosmos.Kernel;

namespace Cosmos.Debug {
	public static class Debugger {
        public static void Break() { } // Plugged
        public static unsafe void Send(int aLength, char* aText) { } // Plugged
        public static void TraceOff() { } // Plugged
        public static void TraceOn() { } // Plugged
        public static void SendPtr(object aObject) { } // plugged
        
        public static unsafe void Send(string aText) {
            var xChars = aText.ToCharArray();
            fixed(char* xPtr = &xChars[0]) {
                Send(xChars.Length, xPtr);
            }
        }

        // TODO: Kudzu repacement methods for Cosmos.Hardware.DebugUtil
        public static unsafe void SendMessage(string aModule, string aData) {
            string xSingleString = "Message Module: " + aModule + " Data: " + aData;
            Send(xSingleString);
        }

        public static unsafe void SendError(string aModule, string aData)
        {
            string xSingleString = "Error Module: " + aModule + " Data: " + aData;
            Send(xSingleString);
        }

        public unsafe static void ViewMemory()
        {
            ViewMemory(0);
        }

        public unsafe static void ViewMemory(int addr)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine();

                for (int j = 0; j < 20; j++)
                {
                    int line = addr + j * 16;
                    Console.Write(line.ToHex(8));
                    Console.Write(": ");

                    for (int i = 0; i < 16; i++)
                    {
                        if (i == 8) Console.Write("  ");
                        Console.Write((*(byte*)(line + i)).ToHex(2) + " ");
                    }
                    Console.Write(" ");

                    for (int i = 0; i < 16; i++)
                    {
                        byte b = (*(byte*)(line + i));
                        if (i == 8) Console.Write(" ");
                        if (b < 32 || b > 127)
                            Console.Write(".");
                        else
                            Console.Write((char)b);
                    }

                    Console.WriteLine();
                }

                Console.WriteLine();

                Console.Write("Enter Hex Address (q to quit): ");
                string s = Console.ReadLine();
                if (s == "q")
                    break;

                addr = FromHex(s);
            }
        }

        private static int FromHex(string p)
        {
            p = p.ToLower();
            string hex = "0123456789abcdef";

            int ret = 0;

            for (int i = 0; i < p.Length; i++)
            {
                ret = ret * 16 + hex.IndexOf(p[i]);
            }
            return ret;

        }
	}
}