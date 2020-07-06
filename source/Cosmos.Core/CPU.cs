using System;
using System.Collections.Generic;
using IL2CPU.API.Attribs;

namespace Cosmos.Core
{
    // Non hardware class, only used by core and hardware drivers for ports etc.
    /// <summary>
    /// CPU class. Non hardware class, only used by core and hardware drivers for ports etc.
    /// </summary>
    public class CPU
    {
        // Amount of RAM in MB's.
        // needs to be static, as Heap needs it before we can instantiate objects
        /// <summary>
        /// Get amount of RAM in MB's. Plugged.
        /// </summary>
        [PlugMethod(PlugRequired = true)]
        public static uint GetAmountOfRAM() => throw null;

        // needs to be static, as Heap needs it before we can instantiate objects
        /// <summary>
        /// Get end of the kernel. Plugged.
        /// </summary>
        [PlugMethod(PlugRequired = true)]
        public static uint GetEndOfKernel() => throw null;

        /// <summary>
        /// Update IDT. Plugged.
        /// </summary>
        [PlugMethod(PlugRequired = true)]
        public void UpdateIDT(bool aEnableInterruptsImmediately) => throw null;

        /// <summary>
        /// Init float. Plugged.
        /// </summary>
        [PlugMethod(PlugRequired = true)]
        public void InitFloat() => throw null;

        /// <summary>
        /// Init SSE. Plugged.
        /// </summary>
        [PlugMethod(PlugRequired = true)]
        public void InitSSE() => throw null;

        /// <summary>
        /// Zero fill. Plugged.
        /// </summary>
        [PlugMethod(PlugRequired = true)]
        public static void ZeroFill(uint aStartAddress, uint aLength) => throw null;

        /// <summary>
        /// Hult the CPU. Plugged.
        /// </summary>
        [PlugMethod(PlugRequired = true)]
        public void Halt() => throw null;

        /// <summary>
        /// Reboot the CPU.
        /// </summary>
        public void Reboot()
        {
            // Disable all interrupts
            DisableInterrupts();

            var myPort = new IOPort(0x64);
            while ((myPort.Byte & 0x02) != 0)
            {
            }
            myPort.Byte = 0xFE;
            Halt(); // If it didn't work, Halt the CPU
        }

        /// <summary>
        /// Enable interrupts. Plugged.
        /// </summary>
        [PlugMethod(PlugRequired = true)]
        private static void DoEnableInterrupts() => throw null;

        /// <summary>
        /// Disable interrupts. Plugged.
        /// </summary>
        [PlugMethod(PlugRequired = true)]
        private static void DoDisableInterrupts() => throw null;

        /// <summary>
        /// Check if interrupts enabled.
        /// </summary>
        [AsmMarker(AsmMarker.Type.Processor_IntsEnabled)]
        public static bool mInterruptsEnabled;

        /// <summary>
        /// Enable interrupts.
        /// </summary>
        public static void EnableInterrupts()
        {
            mInterruptsEnabled = true;
            DoEnableInterrupts();
        }

        /// <summary>
        /// Returns if the interrupts were actually enabled.
        /// </summary>
        /// <returns>bool value.</returns>
        public static bool DisableInterrupts()
        {
            DoDisableInterrupts();
            var xResult = mInterruptsEnabled;
            mInterruptsEnabled = false;
            return xResult;
        }

        public static string GetCPUVendorName()
        {
            if (CanReadCPUID() != 0)
            {
                int eax = 0;
                int ebx = 0;
                int ecx = 0;
                int edx = 0;
                ReadCPUID(0, ref eax, ref ebx, ref ecx, ref edx); // 0 is vendor name

                string s = "";
                s += (char)(ebx);
                s += (char)(ebx >> 8);
                s += (char)(ebx >> 16);
                s += (char)(ebx >> 24);
                s += (char)(edx);
                s += (char)(edx >> 8);
                s += (char)(edx >> 16);
                s += (char)(edx >> 24);
                s += (char)(ecx);
                s += (char)(ecx >> 8);
                s += (char)(ecx >> 16);
                s += (char)(ecx >> 24);

                return s;
            }

            throw new NotSupportedException();
        }

        public static ulong GetCPUUptime()
        {
            // TODO Divide by cpu clock speed
            return ReadTimestampCounter();
        }

        public static long GetCPUCycleSpeed()
        {
            if (CanReadCPUID() != 0)
            {
                // See https://c9x.me/x86/html/file_module_x86_id_45.html

                int eax = 0;
                int ebx = 0;
                int ecx = 0;
                int edx = 0;
                string s = "";

                for (uint i = 0; i < 3; i++)
                {
                    ReadCPUID(0x80000002 + i, ref eax, ref ebx, ref ecx, ref edx);
                    s += (char)(ebx % 256);
                    s += (char)((ebx >> 8)  % 256);
                    s += (char)((ebx>> 16)  % 256);
                    s += (char)((ebx >> 24)  % 256);
                    s += (char)(edx % 256);
                    s += (char)((edx >> 8)  % 256);
                    s += (char)((edx >> 16)  % 256);
                    s += (char)((edx >> 24)  % 256);
                    s += (char)(ecx % 256);
                    s += (char)((ecx >> 8)  % 256);
                    s += (char)((ecx >> 16) % 256);
                    s += (char)((ecx >> 24) % 256);
                }
                var _words = new List<string>();
                string curr = "";
                for (int i = 0; i < s.Length; i++)
                {
                    if (s[i] == ' ' || (byte)s[i] == 0)
                    {
                        if (curr != "")
                        {
                            _words.Add(curr);
                        }
                        curr = "";
                    }
                    else
                    {
                        curr += s[i];
                    }
                }
                _words.Add(curr);
                string[] words = _words.ToArray();
                string[] w = new string[words.Length];
                for (int i = 0; i < words.Length; i++)
                {
                    w[i] = words[words.Length - i - 1];
                }
                words = w;
                double multiplier = 0;
                double value = 0;
                for (int i = 0; i < words.Length; i++)
                {
                    if (words[i] == "MHz")
                    {
                        multiplier = 10e6;
                        break;
                    }
                    else if (words[i] == "GHz")
                    {
                        multiplier = 10e9;
                        break;
                    }
                    else if (words[i] == "THz")
                    {
                        multiplier = 10e12;
                        break;
                    }
                    else if (value == 0)
                    {
                        Double.TryParse(words[i], out value);
                    }
                }
                value *= multiplier;
                return (long)value;
            }

            throw new NotSupportedException();
        }

        internal static int CanReadCPUID() => throw new NotImplementedException();

        internal static void ReadCPUID(uint type, ref int eax, ref int ebx, ref int ecx, ref int edx) => throw new NotImplementedException();

        internal static ulong ReadTimestampCounter() => throw new NotImplementedException();

        internal static ulong ReadFromModelSpecificRegister() => throw new NotImplementedException();
    }
}
