using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Performance {
    class Program {
        [DllImport("kernel32.dll")]
        static extern uint GetTickCount();

        static int Do (int aOne, int aTwo) {
            var xOne = aOne;
            var xResult = xOne + aTwo;
            return xResult;
        }

        private static string IntToHex(uint aValue) {
            string xResult = "";
            uint xValue = aValue;
            byte xCurrentBits = 32;
            while (xCurrentBits >= 4) {
                xCurrentBits -= 4;
                byte xCurrentDigit = (byte)((xValue >> xCurrentBits) & 0xF);
                string xDigit = null;
                switch (xCurrentDigit) {
                    case 0:
                        xDigit = "0";
                        break;
                    case 1:
                        xDigit = "1";
                        break;
                    case 2:
                        xDigit = "2";
                        break;
                    case 3:
                        xDigit = "3";
                        break;
                    case 4:
                        xDigit = "4";
                        break;
                    case 5:
                        xDigit = "5";
                        break;
                    case 6:
                        xDigit = "6";
                        break;
                    case 7:
                        xDigit = "7";
                        break;
                    case 8:
                        xDigit = "8";
                        break;
                    case 9:
                        xDigit = "9";
                        break;
                    case 10:
                        xDigit = "A";
                        break;
                    case 11:
                        xDigit = "B";
                        break;
                    case 12:
                        xDigit = "C";
                        break;
                    case 13:
                        xDigit = "D";
                        break;
                    case 14:
                        xDigit = "E";
                        break;
                    case 15:
                        xDigit = "F";
                        break;
                }
                Console.Write(xDigit);
                xResult = xResult + xDigit;
            }
            Console.WriteLine();
            return xResult;
        }

        const int LoopSize = 50 * 1000 * 1000;
        static void Main (string[] args) {
            Console.WriteLine("Hello");
            //for JIT
            //Also to kick a out SpeedStep
            for (int j = 1; j <= LoopSize; j++) {
                var x = Do(j, 5);
            }
            Console.WriteLine("Starting Test");

            //Now do test run
            var xStart = GetTickCount();

            for (int j = 1; j < LoopSize; j++) {
                var x = Do(j, 5);
            }

            var xFinish = GetTickCount();
            var xTime = xFinish - xStart;
            IntToHex(xStart);
            IntToHex(xFinish);
            IntToHex(xTime);
            Console.WriteLine("Bye");
        }
    }
}
