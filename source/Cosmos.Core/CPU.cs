//#define COSMOSDEBUG
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
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
        /// Halt the CPU. Plugged.
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

        /// <summary>
        /// Get CPU vendor name.
        /// </summary>
        /// <returns>string value.</returns>
        /// <exception cref="NotImplementedException">Thrown on fatal error, contact support.</exception>
        /// <exception cref="NotSupportedException">Thrown if can not read CPU vendor name.</exception>
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
                s += (char)(ebx & 0xff);
                s += (char)((ebx >> 8) & 0xff);
                s += (char)((ebx >> 16) & 0xff);
                s += (char)(ebx >> 24);
                s += (char)((edx) & 0xff);
                s += (char)((edx >> 8) & 0xff);
                s += (char)((edx >> 16) & 0xff);
                s += (char)(edx >> 24);
                s += (char)((ecx) & 0xff);
                s += (char)((ecx >> 8) & 0xff);
                s += (char)((ecx >> 16) & 0xff);
                s += (char)(ecx >> 24);

                return s;
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// Get CPU up time. 
        /// </summary>
        /// <returns>ulong value.</returns>
        /// <exception cref="NotImplementedException">Thrown on fatal error, contact support.</exception>
        public static ulong GetCPUUptime()
        {
            // TODO Divide by cpu clock speed
            return ReadTimestampCounter();
        }

        /// <summary>
        /// Get CPU cycle speed.
        /// </summary>
        /// <returns>long value.</returns>
        /// <exception cref="NotImplementedException">Thrown on fatal error, contact support.</exception>
        /// <exception cref="NotSupportedException">Thrown if can not read CPU ID.</exception>
        public static long GetCPUCycleSpeed()
        {
            if (CanReadCPUID() != 0)
            {
                string s = GetCPUBrandString();
                return EstimateCPUSpeedFromName(s);
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// This is only public for testing purposes
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static long EstimateCPUSpeedFromName(string s)
        {
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
            for (int i = 0; i < words.Length; i++) // Switch order
            {
                w[i] = words[words.Length - i - 1];
            }
            words = w;
            double multiplier = 0;
            double value = 0;
            for (int i = 0; i < words.Length; i++)
            {
                var word = words[i];
                var wordEnd = word.Substring(word.Length - 3, 3);
                if (word == "MHz" || wordEnd == "MHz")
                {
                    multiplier = 1e6;
                }
                else if (word == "GHz" || wordEnd == "GHz")
                {
                    multiplier = 1e9;
                }
                else if (word == "THz" || wordEnd == "THz")
                {
                    multiplier = 1e12;
                }
                if (value == 0)
                {
                    if (Double.TryParse(word, out value) || Double.TryParse(word.Substring(0, word.Length - 3), out value))
                    {
                        break;
                    }
                }
            }
            value *= multiplier;
            if ((long)value == 0)
            {
                Global.mDebugger.Send("Unable to calculate cycle speed from " + s);
                throw new NotSupportedException("Unable to calculate cycle speed from " + s);
            }
            return (long)value;
        }

        /// <summary>
        /// Get CPU cycle speed.
        /// </summary>
        /// <returns>long value.</returns>
        /// <exception cref="NotImplementedException">Thrown on fatal error, contact support.</exception>
        /// <exception cref="NotSupportedException">Thrown if can not read CPU ID.</exception>
        public static string GetCPUBrandString()
        {
            if (CanReadCPUID() != 0)
            {
                // See https://c9x.me/x86/html/file_module_x86_id_45.html

                int eax = 0;
                int ebx = 0;
                int ecx = 0;
                int edx = 0;
                int[] s = new int[64];
                string rs = "";

                for (uint i = 0; i < 3; i++)
                {
                    ReadCPUID(0x80000002 + i, ref eax, ref ebx, ref ecx, ref edx);
                    s[(i * 16) + 0] = (eax % 256);
                    s[(i * 16) + 1] = ((eax >> 8) % 256);
                    s[(i * 16) + 2] = ((eax >> 16) % 256);
                    s[(i * 16) + 3] = ((eax >> 24) % 256);
                    s[(i * 16) + 4] = (ebx % 256);
                    s[(i * 16) + 5] = ((ebx >> 8) % 256);
                    s[(i * 16) + 6] = ((ebx >> 16) % 256);
                    s[(i * 16) + 7] = ((ebx >> 24) % 256);
                    s[(i * 16) + 8] = (ecx % 256);
                    s[(i * 16) + 9] = ((ecx >> 8) % 256);
                    s[(i * 16) + 10] = ((ecx >> 16) % 256);
                    s[(i * 16) + 11] = ((ecx >> 24) % 256);
                    s[(i * 16) + 12] = (edx % 256);
                    s[(i * 16) + 13] = ((edx >> 8) % 256);
                    s[(i * 16) + 14] = ((edx >> 16) % 256);
                    s[(i * 16) + 15] = ((edx >> 24) % 256);
                }
                for (int i = 0; i < s.Length; i++)
                {
                    if (s[i] == 0x00)
                    {
                        continue;
                    }
                    rs += (char)s[i];
                }

                if (!(rs == ""))
                {
                    return rs.Trim();
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            throw new NotSupportedException();
        }

        /// <summary>
        /// Check if can read CPU ID. Plugged.
        /// </summary>
        /// <returns>non-zero if can read.</returns>
        /// <exception cref="NotImplementedException">Thrown on fatal error, contact support.</exception>
        public static int CanReadCPUID() => throw new NotImplementedException();

        /// <summary>
        /// Read CPU ID. Plugged.
        /// </summary>
        /// <param name="type">type.</param>
        /// <param name="eax">eax.</param>
        /// <param name="ebx">ebx.</param>
        /// <param name="ecx">ecx.</param>
        /// <param name="edx">edx.</param>
        /// <exception cref="NotImplementedException">Thrown on fatal error, contact support.</exception>
        public static void ReadCPUID(uint type, ref int eax, ref int ebx, ref int ecx, ref int edx) => throw new NotImplementedException();

        /// <summary>
        /// Read timestamp counter. Plugged.
        /// </summary>
        /// <returns>ulong value.</returns>
        /// <exception cref="NotImplementedException">Thrown on fatal error, contact support.</exception>
        internal static ulong ReadTimestampCounter() => throw new NotImplementedException();

        /// <summary>
        /// Read from mode specific register. Plugged.
        /// </summary>
        /// <returns>ulong value.</returns>
        /// <exception cref="NotImplementedException">Thrown on fatal error, contact support.</exception>
        internal static ulong ReadFromModelSpecificRegister() => throw new NotImplementedException();

        /// <summary>
        /// Checks if Multiboot returned a memory map
        /// </summary>
        /// <returns></returns>
        public static unsafe bool MemoryMapExists()
        {
            return (Bootstrap.MultibootHeader->Flags & 1 << 6) == 64;
        }

        /// <summary>
        /// Get the Memory Map Information from Multiboot
        /// </summary>
        /// <returns>Returns an array of MemoryMaps containing the Multiboot Memory Map information. The array may have empty values at the end.</returns>
        public static unsafe MemoryMap[] GetMemoryMap()
        {
            if (!MemoryMapExists())
            {
                throw new Exception("No Memory Map was returned by Multiboot");
            }
            var rawMap = new RawMemoryMap[64];
            var currentMap = (RawMemoryMap*)Bootstrap.MultibootHeader->memMapAddress;
            int counter = 0;
            while ((uint)currentMap < (Bootstrap.MultibootHeader->memMapAddress + Bootstrap.MultibootHeader->memMapLength) && counter < 64)
            {
                rawMap[counter++] = *currentMap;
                currentMap = (RawMemoryMap*)((uint*)currentMap + ((currentMap->Size + 4 )>> 2)); //The size is in bits, not bytes
                if (currentMap->Size == 0)
                {
                    break;
                }
            }

            if (counter >= 64)
            {
                throw new Exception("Memory Map returned too many segments");
            }

            var entireMap = new MemoryMap[counter];
            for (int i = 0; i < counter; i++)
            {
                var rawMemoryMap = rawMap[i];
                entireMap[i] = new MemoryMap
                {
                    Address = (ulong)rawMemoryMap.HighBaseAddr << 32 | rawMemoryMap.LowBaseAddr,
                    Length = (ulong)rawMemoryMap.HighLength << 32 | rawMemoryMap.LowLength,
                    Type = rawMemoryMap.Type
                };
            }
            return entireMap;
        }
    }

    public class MemoryMap
    {
        /// <summary>
        /// Base Address of the memory region
        /// </summary>
        public ulong Address;
        /// <summary>
        /// Length in bytes of the region
        /// </summary>
        public ulong Length;
        /// <summary>
        /// Type of RAM in region. 1 is available. 3 is for ACPI. All other is unavailable
        /// </summary>
        public uint Type;
    }

    [StructLayout(LayoutKind.Explicit, Size = 24)]
    public struct RawMemoryMap
    {
        /// <summary>
        /// Size of this entry
        /// </summary>
        [FieldOffset(0)]
        public uint Size;
        /// <summary>
        /// Low 32 bits of the base address
        /// </summary>
        [FieldOffset(4)]
        public uint LowBaseAddr;
        /// <summary>
        /// High 32 bits of the base address
        /// </summary>
        [FieldOffset(8)]
        public uint HighBaseAddr;
        /// <summary>
        /// Low 32 bits of the length of memory block in bytes
        /// </summary>
        [FieldOffset(12)]
        public uint LowLength;
        /// <summary>
        /// High 32 bits of the length of memory block in bytes
        /// </summary>
        [FieldOffset(16)]
        public uint HighLength;
        /// <summary>
        /// Type of memory area, 1 if usable RAM, everything else unusable.
        /// </summary>
        [FieldOffset(20)]
        public uint Type;
    }
}
