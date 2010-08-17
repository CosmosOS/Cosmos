using System;
using Cosmos.Kernel;

namespace Cosmos.Debug {
	public static class Debugger {
        public static void Break() { } // Plugged
        public static unsafe void Send(int aLength, char* aText) { } // Plugged
        //public static void TraceOff() { } // Plugged
        //public static void TraceOn() { } // Plugged
        public static void SendPtr(object aObject) { } // plugged
        
        public static unsafe void Send(string aText) {
            var xChars = aText.ToCharArray();
            fixed(char* xPtr = &xChars[0]) {
                Send(xChars.Length, xPtr);
            }
        }

        // TODO: Kudzu repacement methods for Cosmos.Hardware.DebugUtil
        public static unsafe void SendMessage(string aModule, string aData) {
            //string xSingleString;
            //xSingleString = "Message Module: \"" + aModule + "\"";
            //xSingleString += " Data: \"" + aData + "\"";
            //Send(xSingleString);
        }                                          

        public static unsafe void SendError(string aModule, string aData)
        {
            //string xSingleString;
            //xSingleString = "Error Module: \"" + aModule + "\"";
            //xSingleString += " Data: \"" + aData + "\"";
            //Send(xSingleString);
        }

        public static unsafe void SendNumber(string aModule, string aDescription, uint aNumber, byte aBits)
        {
            //string xSingleString;
            //xSingleString = "Number Module: \"" + aModule + "\"";
            //xSingleString += " Description: \"" + aDescription + "\"";
            //xSingleString += " Number: \"" + CreateNumber(aNumber, aBits) + "\"";
        }

        public static unsafe void WriteNumber(uint aNumber, byte aBits)
        {
            WriteNumber(aNumber, aBits, true);
        }

        public static unsafe void WriteNumber(uint aNumber, byte aBits, bool aWritePrefix)
        {
            Send(CreateNumber(aNumber, aBits, aWritePrefix));
        }

        public static unsafe string CreateNumber(uint aNumber, byte aBits)
        {
            return CreateNumber(aNumber, aBits, true);
        }

        public static unsafe string CreateNumber(uint aNumber, byte aBits, bool aWritePrefix)
        {
            return "Cosmos.Debug.Debugger.CreateNumber(aNumber, aBits, aWritePrefix) not implemented";
            //string xNumberString = null;
            //uint xValue = aNumber;
            //byte xCurrentBits = aBits;
            //if (aWritePrefix)
            //{
            //    xNumberString += "0x";
            //}
            //while (xCurrentBits >= 4)
            //{
            //    xCurrentBits -= 4;
            //    byte xCurrentDigit = (byte)((xValue >> xCurrentBits) & 0xF);
            //    string xDigitString = null;
            //    switch (xCurrentDigit)
            //    {
            //        case 0:
            //            xDigitString = "0";
            //            goto default;
            //        case 1:
            //            xDigitString = "1";
            //            goto default;
            //        case 2:
            //            xDigitString = "2";
            //            goto default;
            //        case 3:
            //            xDigitString = "3";
            //            goto default;
            //        case 4:
            //            xDigitString = "4";
            //            goto default;
            //        case 5:
            //            xDigitString = "5";
            //            goto default;
            //        case 6:
            //            xDigitString = "6";
            //            goto default;
            //        case 7:
            //            xDigitString = "7";
            //            goto default;
            //        case 8:
            //            xDigitString = "8";
            //            goto default;
            //        case 9:
            //            xDigitString = "9";
            //            goto default;
            //        case 10:
            //            xDigitString = "A";
            //            goto default;
            //        case 11:
            //            xDigitString = "B";
            //            goto default;
            //        case 12:
            //            xDigitString = "C";
            //            goto default;
            //        case 13:
            //            xDigitString = "D";
            //            goto default;
            //        case 14:
            //            xDigitString = "E";
            //            goto default;
            //        case 15:
            //            xDigitString = "F";
            //            goto default;
            //        default:
            //            xNumberString += xDigitString;
            //            break;
            //    }
            //}
            //return xNumberString;
        }

        public static unsafe void WriteBinary(string aModule, string aMessage, byte[] aValue)
        {
            WriteBinary(aModule, aMessage, aValue, 0, aValue.Length);
        }

        public static unsafe void WriteBinary(string aModule, string aMessage, byte[] aValue, int aIndex, int aLength)
        {
            //string xSingleString;
            //xSingleString = "Binary Module = \"" + aModule + "\"";
            //xSingleString += " Message = " + aMessage + "\"";
            //xSingleString += " Value = \"";
            //for (int i = 0; i < aLength; i++)
            //{
            //    xSingleString += CreateNumber(aValue[aIndex + i], 8, false);
            //}
            //xSingleString += "\"";
            //Send(xSingleString);
        }

        public static unsafe void WriteBinary(string aModule, string aMessage, byte* aValue, int aIndex, int aLength)
        {
            //string xSingleString;
            //xSingleString = "Binary Module = \"" + aModule + "\"";
            //xSingleString += " Message = " + aMessage + "\"";
            //xSingleString += " Value = \"";
            //for (int i = 0; i < aLength; i++)
            //{
            //    xSingleString += CreateNumber(aValue[aIndex + i], 8, false);
            //}
            //xSingleString += "\"";
            //Send(xSingleString);
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