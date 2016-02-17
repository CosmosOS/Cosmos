﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;


namespace Cosmos.Debug.Kernel
{
    public class Debugger
    {
        public string mRing { get; set; }

        public string mSection { get; set; }

        public Debugger(string aRing, string aSection)
        {
            mRing = aRing;
            mSection = aSection;
        }

        public void Break()
        {
        } // Plugged

        private static unsafe void ActualSend(int aLength, char* aText)
        {
        } // Plugged

        public void SendPtr(object aObject)
        {
        } // plugged


        internal static void DoSendNumber(uint aNumber)
        {
            // plugged
        }

        internal static void DoSendNumber(float aNumber)
        {
            // plugged
        }

        internal static void DoSendNumber(double aNumber)
        {
            // plugged
        }

        public void SendNumber(uint aNumber)
        {
            DoSendNumber(aNumber);
        }

        public void SendNumber(float aNumber)
        {
            DoSendNumber(aNumber);
        }

        public void SendNumber(double aNumber)
        {
            DoSendNumber(aNumber);
        }

        public unsafe void SendChannelCommand(byte aChannel, byte aCommand, byte[] aData)
        {
            fixed (byte* xPtr = &aData[0])
            {
                SendChannelCommand(aChannel, aCommand, aData.Length, xPtr);
            }
        }

        public static unsafe void SendChannelCommand(byte aChannel, byte aCommand, int aByteCount, byte* aData)
        {
            //
        }

        public static unsafe void SendChannelCommand(byte aChannel, byte aCommand)
        {
            //
        }

        internal static void DoSend(string aText)
        {
            // plugged
        }

        public void Send(string aText)
        {
            if (!string.IsNullOrEmpty(aText))
            {
                string xText = aText;
                if (!string.IsNullOrEmpty(mRing))
                {
                    string xPrepend = string.Concat("[", mRing);
                    if (!string.IsNullOrEmpty(mSection))
                    {
                        xPrepend = string.Concat(xPrepend, ":", mSection);
                    }
                    xPrepend = string.Concat(xPrepend, "]: ");
                    xText = string.Concat(xPrepend, xText);
                }
                DoSend(xText);
            }
        }

        [Conditional("COSMOSDEBUG")]
        public void SendInternal(string aText)
        {
            DoSend(aText);
        }

        //public void OldSend(string aText) {
        //    // TODO: Need to fix this so it can send empty strings.
        //    // Sending empty strings locks it up right now
        //    if (aText.Length == 0)
        //    {
        //        return;
        //    }

        //    var xChars = aText.ToCharArray();
        //    fixed (char* xPtr = &xChars[0])
        //    {
        //        ActualSend(xChars.Length, xPtr);
        //    }
        //}

        public unsafe void SendMessageBox(int aLength, char* aText) { } // Plugged
        public unsafe void SendMessageBox(string aText)
        {
            // TODO: Need to fix this so it can send empty strings.
            // Sending empty strings locks it up right now
            if (aText.Length == 0)
            {
                return;
            }

            var xChars = aText.ToCharArray();
            fixed (char* xPtr = &xChars[0])
            {
                SendMessageBox(xChars.Length, xPtr);
            }
        }

        // TODO: Kudzu repacement methods for Cosmos.HAL.DebugUtil
        public unsafe void SendMessage(string aModule, string aData)
        {
            //string xSingleString;
            //xSingleString = "Message Module: \"" + aModule + "\"";
            //xSingleString += " Data: \"" + aData + "\"";
            //Send(xSingleString);

            DoSend("Message Module:");
            DoSend(aModule);
            DoSend("Data:");
            DoSend(aData);
        }

        public unsafe void SendError(string aModule, string aData)
        {
            //string xSingleString;
            //xSingleString = "Error Module: \"" + aModule + "\"";
            //xSingleString += " Data: \"" + aData + "\"";
            //Send(xSingleString);
        }

        public unsafe void SendNumber(string aModule, string aDescription, uint aNumber, byte aBits)
        {
            //string xSingleString;
            //xSingleString = "Number Module: \"" + aModule + "\"";
            //xSingleString += " Description: \"" + aDescription + "\"";
            //xSingleString += " Number: \"" + CreateNumber(aNumber, aBits) + "\"";
        }

        public unsafe void WriteNumber(uint aNumber, byte aBits)
        {
            WriteNumber(aNumber, aBits, true);
        }

        public unsafe void WriteNumber(uint aNumber, byte aBits, bool aWritePrefix)
        {
            Send(CreateNumber(aNumber, aBits, aWritePrefix));
        }

        public unsafe string CreateNumber(uint aNumber, byte aBits)
        {
            return CreateNumber(aNumber, aBits, true);
        }

        public unsafe string CreateNumber(uint aNumber, byte aBits, bool aWritePrefix)
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

        public unsafe void WriteBinary(string aModule, string aMessage, byte[] aValue)
        {
            WriteBinary(aModule, aMessage, aValue, 0, aValue.Length);
        }

        public unsafe void WriteBinary(string aModule, string aMessage, byte[] aValue, int aIndex, int aLength)
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

        public unsafe void WriteBinary(string aModule, string aMessage, byte* aValue, int aIndex, int aLength)
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

        public unsafe void ViewMemory()
        {
            ViewMemory(0);
        }

        public unsafe void ViewMemory(int addr)
        {
            //while (true) {
            //    Console.Clear();
            //    Console.WriteLine();

            //    for (int j = 0; j < 20; j++) {
            //        int line = addr + j * 16;
            //        Console.Write(line.ToHex(8));
            //        Console.Write(": ");

            //        for (int i = 0; i < 16; i++) {
            //            if (i == 8) Console.Write("  ");
            //            Console.Write((*(byte*)(line + i)).ToHex(2) + " ");
            //        }
            //        Console.Write(" ");

            //        for (int i = 0; i < 16; i++) {
            //            byte b = (*(byte*)(line + i));
            //            if (i == 8) Console.Write(" ");
            //            if (b < 32 || b > 127)
            //                Console.Write(".");
            //            else
            //                Console.Write((char)b);
            //        }

            //        Console.WriteLine();
            //    }

            //    Console.WriteLine();

            //    Console.Write("Enter Hex Address (q to quit): ");
            //    string s = Console.ReadLine();
            //    if (s == "q")
            //        break;

            //    addr = FromHex(s);
            //}
        }

        private int FromHex(string p)
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
