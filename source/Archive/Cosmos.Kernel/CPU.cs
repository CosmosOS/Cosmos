using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Kernel {
	public class CPU {

        public static void ClearInterruptsTable() {
            //plugged
        }

        // Amount of RAM in MB's.
        protected static uint GetAmountOfRAM() { return 0; } // Plugged
        protected static uint GetEndOfKernel() { return 0; } // Plugged
        public static void UpdateIDT(bool aEnableInterruptsImmediately) { } // Plugged
        public static void InitFloat() { } // Plugged

        public static uint AmountOfMemory {
			get {
				return GetAmountOfRAM();
			}
		}

		public static uint EndOfKernel {
			get {
				return GetEndOfKernel();
			}
		}

        // Plugged
        public static void ZeroFill(uint aStartAddress, uint aLength) {
		}

        // Plugged
        public static uint GetCurrentESP() {
			return 0;
		}

        // Plugged
        public static uint GetEndOfStack() {
			return 0;
		}

        // Plugged
        public static void DoTest()
        {
        }



        private static string CPUIDuint2string(uint a)
        {
            return new string(new char[] { (char)(a), (char)(a >> 8), (char)(a >> 16), (char)(a >> 24) });
        }

        private static bool HaveCheckedCPUID = false;
        private static bool HasCPUID = false;
        private static string CPUString = null;

        public static string CPUVendor
        {
            get
            {
                if (CPUString == null)
                {
                    if (CPUIDSupport)
                    {
                        uint d,c,b,a;
                        GetCPUId(out d, out c, out b, out a, 0);
                        CPUString = CPUIDuint2string(b) + CPUIDuint2string(d) + CPUIDuint2string(c);
                    }
                    else
                    {
                        CPUString = string.Empty;
                    }
                }
                return CPUString;
            }
        }

        public static bool CPUIDSupport
        {
            get
            {
                if (!HaveCheckedCPUID)
                {
                    HaveCheckedCPUID = true;
                    HasCPUID = (HasCPUIDSupport() != 0);
                }
                return HasCPUID;
            }
        }

        // Plugged
        public static uint HasCPUIDSupport()
        {
            return 0;
        }

        // Plugged
        public static void GetCPUId(out uint d, out uint c, out uint b, out uint a, uint v)
        {
            d = 0;
            c = 0;
            b = 0;
            a = 0;
        }

        /// <summary>
        /// Forced simple reboot of PC
        /// </summary>
        public static void Reboot()
        {
            // Disable all interrupts
            DisableInterrupts();

            byte temp;

            // Clear all keyboard buffers
            do
            {
                temp = CPUBus.Read8(0x64); // Empty user data
                if ((temp & 0x01) != 0)
                {
                    CPUBus.Read8(0x60); // Empty keyboard data
                }
            } while ((temp & 0x02) != 0);

            CPUBus.Write8(0x64, 0xFE); // Pulse CPU Reset line
            Halt(); // If it didn't work, Halt the CPU
        }

        //Plugged
        public static void Halt()
        {
            
        }

        public static void DisableInterrupts() {
            // plugged
        }

        public static void EnableInterrupts()
        {
            // plugged
        }

        // plugged
        public static void Interrupt30(ref uint aEAX, ref uint aEBX, ref uint aECX, ref uint aEDX) {
            aEAX = 0;
        }

        // plugged
        public static uint GetMBIAddress()
        {
            return 0;
        }
	}
}