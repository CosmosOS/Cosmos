using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Core.DeviceInformation;
using Cosmos.Debug.Kernel;

namespace Cosmos.HAL.PCInfo
{
    /// <summary>
    /// This class represents one processor of the machine
    /// There is a LOT of information in this class
    /// This contains the same information as "dmidecode -t 4"
    /// TODO: parse the rest of the information
    /// </summary>
    public class Processor
    {
        /// <summary>
        /// Type of processor (e.g Inter i5)
        /// </summary>
        public string ProcessorVersion { get; set; }
        /// <summary>
        /// Manufacturer of the processor
        /// </summary>
        public string Manufacturer { get; set; }
        /// <summary>
        /// Number of the processor inside the cpu
        /// </summary>
        public string SocketDesignation { get; set; }
        /// <summary>
        /// Current speed in mhz
        /// </summary>
        public int Speed { get; set; }
        /// <summary>
        /// Processor type (central, math, dsp or video)
        /// </summary>
        public string ProcessorType { get; set; }
        /// <summary>
        /// Processor family
        /// </summary>
        /// <param name="ProcessorFamily"></param>
        public string ProcessorFamily { get; set; }
        /// <summary>
        /// Flags of the processor (sse, fpu and so on)
        /// </summary>
        /// <param name="Flags"></param>
        public List<int> Flags { get; set; }

        public Processor(CPUInfo SMBIOSProcessor)
        {
            Speed = SMBIOSProcessor.CurrentSpeed;
            ProcessorType = ParseType(SMBIOSProcessor.ProcessorType);
            ProcessorFamily = ParseFamily(SMBIOSProcessor.ProcessorFamily);
            SocketDesignation = SMBIOSProcessor.SocketDesignation;
            Manufacturer = SMBIOSProcessor.ProcessorManufacturer;
            ProcessorVersion = SMBIOSProcessor.ProcessorVersion;
            Flags = ParseFlags(SMBIOSProcessor.CPUIDEAX, SMBIOSProcessor.CPUIDEDX);
        }

        /// <summary>
        /// Parse the flags for an x86 machine
        /// TODO: in the future will need to overload this method with a variant for amr32 and arm64.
        /// </summary>
        /// <returns></returns>
        public List<int> ParseFlags(uint eax, uint edx)
        {
            //List of the every possible flag
            //Its impossible to do a list of enums (il2cpu errors). 
            //You cannot cast by using methods like ToList()...
            //So we use the old friend "int".
            List<int> listOfFlags = new List<int>();

            //We need to convert edx to something that can be traslated to a bit array safely
            //We can't do (int)eax
            var edxBytes = BitConverter.GetBytes(edx);
            BitArray bitArrayEdx = new BitArray(edxBytes);
            var eaxBytes = BitConverter.GetBytes(eax);
            BitArray bitArrayEax = new BitArray(eaxBytes);

            //See: https://en.wikipedia.org/wiki/CPUID (this is where i got the information).

            Debugger.DoSend("Bits parsed. Adding enums to table");
            //TODO: this gives ilcpu error
            //Go byte by byte in eax
            int offset = 0;
            for (int i = 0; i < 32; i++)
            {
                if (i == 10 || i == 20)
                {
                    offset--; 
                    continue;
                }
                if (bitArrayEdx[i]) listOfFlags.Add(i+offset);
            }
            Debugger.DoSend("Strings added and parsed.");
            return listOfFlags;
        }

        public string ParseType(byte type)
        {
            var typeInt = Convert.ToInt32(type);
            switch (typeInt)
            {
                case 1:
                    return "Other";
                case 2:
                    return "Unknown";
                case 3:
                    return "Central processor";
                case 4:
                    return "Math processor";
                case 5:
                    return "DSP processor";
                case 6:
                    return "Video processor";
            }
            return "Unknown";
        }

        public string ParseFamily(byte family)
        {
            //Switch case was the preferred method because we don't need to store the strings in memory: 
            //We need to compare a limited bunch of times (afaik: 512 is top).
            //Other alternative would be to store this in a hashmap (too much memory).
            switch (Convert.ToInt32(family))
            {
                //Lets copy the table from the SMBIOS specificacion and do some vim and sed magic...
                case 1: return "Other";
                case 2: return "Unknown";
                case 3: return "8086";
                case 4: return "80286";
                case 5: return "Intel386™ processor";
                case 6: return "Intel486™ processor";
                case 7: return "8087";
                case 8: return "80287";
                case 9: return "80387";
                case 10: return "80487";
                case 11: return "Intel® Pentium® processor";
                case 12: return "Pentium® Pro processor";
                case 13: return "Pentium® II processor";
                case 14: return "Pentium® processor with MMX™ technology";
                case 15: return "Intel® Celeron® processor";
                case 16: return "Pentium® II Xeon™ processor";
                case 17: return "Pentium® III processor";
                case 18: return "M1 Family";
                case 19: return "M2 Family";
                case 20: return "Intel® Celeron® M processor";
                case 21: return "Intel® Pentium® 4 HT processor";
                case 24: return "AMD Duron™ Processor Family [1]";
                case 25: return "K5 Family [1]";
                case 26: return "K6 Family [1]";
                case 27: return "K6-2 [1]";
                case 28: return "K6-3 [1]";
                case 29: return "AMD Athlon™ Processor Family [1]";
                case 30: return "AMD29000 Family";
                case 31: return "K6-2+";
                case 32: return "Power PC Family";
                case 33: return "Power PC 601";
                case 34: return "Power PC 603";
                case 35: return "Power PC 603+";
                case 36: return "Power PC 604";
                case 37: return "Power PC 620";
                case 38: return "Power PC x704";
                case 39: return "Power PC 750";
                case 40: return "Intel® Core™ Duo processor";
                case 41: return "Intel® Core™ Duo mobile processor";
                case 42: return "Intel® Core™ Solo mobile processor";
                case 43: return "Intel® Atom™ processor";
                case 44: return "Intel® Core™ M processor";
                case 45: return "Intel(R) Core(TM) m3 processor";
                case 46: return "Intel(R) Core(TM) m5 processor";
                case 47: return "Intel(R) Core(TM) m7 processor";
                case 48: return "Alpha Family [2]";
                case 49: return "Alpha 21064";
                case 50: return "Alpha 21066";
                case 51: return "Alpha 21164";
                case 52: return "Alpha 21164PC";
                case 53: return "Alpha 21164a";
                case 54: return "Alpha 21264";
                case 55: return "Alpha 21364";
                case 56: return "AMD Turion™ II Ultra Dual-Core Mobile M Processor Family";
                case 57: return "AMD Turion™ II Dual-Core Mobile M Processor Family";
                case 58: return "AMD Athlon™ II Dual-Core M Processor Family";
                case 59: return "AMD Opteron™ 6100 Series Processor";
                case 60: return "AMD Opteron™ 4100 Series Processor";
                case 61: return "AMD Opteron™ 6200 Series Processor";
                case 62: return "AMD Opteron™ 4200 Series Processor";
                case 63: return "AMD FX™ Series Processor";
                case 64: return "MIPS Family";
                case 65: return "MIPS R4000";
                case 66: return "MIPS R4200";
                case 67: return "MIPS R4400";
                case 68: return "MIPS R4600";
                case 69: return "MIPS R10000";
                case 70: return "AMD C-Series Processor";
                case 71: return "AMD E-Series Processor";
                case 72: return "AMD A-Series Processor";
                case 73: return "AMD G-Series Processor";
                case 74: return "AMD Z-Series Processor";
                case 75: return "AMD R-Series Processor";
                case 76: return "AMD Opteron™ 4300 Series Processor";
                case 77: return "AMD Opteron™ 6300 Series Processor";
                case 78: return "AMD Opteron™ 3300 Series Processor";
                case 79: return "AMD FirePro™ Series Processor";
                case 80: return "SPARC Family";
                case 81: return "SuperSPARC";
                case 82: return "microSPARC II";
                case 83: return "microSPARC IIep";
                case 84: return "UltraSPARC";
                case 85: return "UltraSPARC II";
                case 86: return "UltraSPARC Iii";
                case 87: return "UltraSPARC III";
                case 88: return "UltraSPARC IIIi";
                case 96: return "68040 Family";
                case 97: return "68xxx";
                case 98: return "68000";
                case 99: return "68010";
                case 100: return "68020";
                case 101: return "68030";
                case 102: return "AMD Athlon(TM) X4 Quad-Core Processor Family";
                case 103: return "AMD Opteron(TM) X1000 Series Processor";
                case 104: return "AMD Opteron(TM) X2000 Series APU";
                case 105: return "AMD Opteron(TM) A-Series Processor";
                case 106: return "AMD Opteron(TM) X3000 Series APU";
                case 107: return "AMD Zen Processor Family";
                case 112: return "Hobbit Family";
                case 120: return "Crusoe™ TM5000 Family";
                case 121: return "Crusoe™ TM3000 Family";
                case 122: return "Efficeon™ TM8000 Family";
                case 128: return "Weitek";
                case 129: return "Available for assignment";
                case 130: return "Itanium™ processor";
                case 131: return "AMD Athlon™ 64 Processor Family";
                case 132: return "AMD Opteron™ Processor Family";
                case 133: return "AMD Sempron™ Processor Family";
                case 134: return "AMD Turion™ 64 Mobile Technology";
                case 135: return "Dual-Core AMD Opteron™ Processor Family";
                case 136: return "AMD Athlon™ 64 X2 Dual-Core Processor Family";
                case 137: return "AMD Turion™ 64 X2 Mobile Technology";
                case 138: return "Quad-Core AMD Opteron™ Processor Family";
                case 139: return "Third-Generation AMD Opteron™ Processor Family";
                case 140: return "AMD Phenom™ FX Quad-Core Processor Family";
                case 141: return "AMD Phenom™ X4 Quad-Core Processor Family";
                case 142: return "AMD Phenom™ X2 Dual-Core Processor Family";
                case 143: return "AMD Athlon™ X2 Dual-Core Processor Family";
                case 144: return "PA-RISC Family";
                case 145: return "PA-RISC 8500";
                case 146: return "PA-RISC 8000";
                case 147: return "PA-RISC 7300LC";
                case 148: return "PA-RISC 7200";
                case 149: return "PA-RISC 7100LC";
                case 150: return "PA-RISC 7100";
                case 160: return "V30 Family";
                case 161: return "Quad-Core Intel® Xeon® processor 3200 Series";
                case 162: return "Dual-Core Intel® Xeon® processor 3000 Series";
                case 163: return "Quad-Core Intel® Xeon® processor 5300 Series";
                case 164: return "Dual-Core Intel® Xeon® processor 5100 Series";
                case 165: return "Dual-Core Intel® Xeon® processor 5000 Series";
                case 166: return "Dual-Core Intel® Xeon® processor LV";
                case 167: return "Dual-Core Intel® Xeon® processor ULV";
                case 168: return "Dual-Core Intel® Xeon® processor 7100 Series";
                case 169: return "Quad-Core Intel® Xeon® processor 5400 Series";
                case 170: return "Quad-Core Intel® Xeon® processor";
                case 171: return "Dual-Core Intel® Xeon® processor 5200 Series";
                case 172: return "Dual-Core Intel® Xeon® processor 7200 Series";
                case 173: return "Quad-Core Intel® Xeon® processor 7300 Series";
                case 174: return "Quad-Core Intel® Xeon® processor 7400 Series";
                case 175: return "Multi-Core Intel® Xeon® processor 7400 Series";
                case 176: return "Pentium® III Xeon™ processor";
                case 177: return "Pentium® III Processor with Intel® SpeedStep™ Technology";
                case 178: return "Pentium® 4 Processor";
                case 179: return "Intel® Xeon® processor";
                case 180: return "AS400 Family";
                case 181: return "Intel® Xeon™ processor MP";
                case 182: return "AMD Athlon™ XP Processor Family";
                case 183: return "AMD Athlon™ MP Processor Family";
                case 184: return "Intel® Itanium® 2 processor";
                case 185: return "Intel® Pentium® M processor";
                case 186: return "Intel® Celeron® D processor";
                case 187: return "Intel® Pentium® D processor";
                case 188: return "Intel® Pentium® Processor Extreme Edition";
                case 189: return "Intel® Core™ Solo Processor";
                case 190: return "Reserved [3]";
                case 191: return "Intel® Core™ 2 Duo Processor";
                case 192: return "Intel® Core™ 2 Solo processor";
                case 193: return "Intel® Core™ 2 Extreme processor";
                case 194: return "Intel® Core™ 2 Quad processor";
                case 195: return "Intel® Core™ 2 Extreme mobile processor";
                case 196: return "Intel® Core™ 2 Duo mobile processor";
                case 197: return "Intel® Core™ 2 Solo mobile processor";
                case 198: return "Intel® Core™ i7 processor";
                case 199: return "Dual-Core Intel® Celeron® processor";
                case 200: return "IBM390 Family";
                case 201: return "G4";
                case 202: return "G5";
                case 203: return "ESA/390 G6";
                case 204: return "z/Architecture base";
                case 205: return "Intel® Core™ i5 processor";
                case 206: return "Intel® Core™ i3 processor";
                case 210: return "VIA C7™-M Processor Family";
                case 211: return "VIA C7™-D Processor Family";
                case 212: return "VIA C7™ Processor Family";
                case 213: return "VIA Eden™ Processor Family";
                case 214: return "Multi-Core Intel® Xeon® processor";
                case 215: return "Dual-Core Intel® Xeon® processor 3xxx Series";
                case 216: return "Quad-Core Intel® Xeon® processor 3xxx Series";
                case 217: return "VIA Nano™ Processor Family";
                case 218: return "Dual-Core Intel® Xeon® processor 5xxx Series";
                case 219: return "Quad-Core Intel® Xeon® processor 5xxx Series";
                case 220: return "Available for assignment";
                case 221: return "Dual-Core Intel® Xeon® processor 7xxx Series";
                case 222: return "Quad-Core Intel® Xeon® processor 7xxx Series";
                case 223: return "Multi-Core Intel® Xeon® processor 7xxx Series";
                case 224: return "Multi-Core Intel® Xeon® processor 3400 Series";
                case 228: return "AMD Opteron™ 3000 Series Processor";
                case 229: return "AMD Sempron™ II Processor";
                case 230: return "Embedded AMD Opteron™ Quad-Core Processor Family";
                case 231: return "AMD Phenom™ Triple-Core Processor Family";
                case 232: return "AMD Turion™ Ultra Dual-Core Mobile Processor Family";
                case 233: return "AMD Turion™ Dual-Core Mobile Processor Family";
                case 234: return "AMD Athlon™ Dual-Core Processor Family";
                case 235: return "AMD Sempron™ SI Processor Family";
                case 236: return "AMD Phenom™ II Processor Family";
                case 237: return "AMD Athlon™ II Processor Family";
                case 238: return "Six-Core AMD Opteron™ Processor Family";
                case 239: return "AMD Sempron™ M Processor Family";
                case 250: return "i860";
                case 251: return "i960";
                case 254: return "Indicator to obtain the processor family from the Processor Family 2 field";
                case 255: return "Reserved";
                case 256: return "ARMv7";
                case 257: return "ARMv8";
                case 260: return "SH-3";
                case 261: return "SH-4";
                case 280: return "ARM";
                case 281: return "StrongARM";
                case 300: return "6x86";
                case 301: return "MediaGX";
                case 302: return "MII";
                case 320: return "WinChip";
                case 350: return "DSP";
                case 500: return "Video Processor";
                default:
                    return "Unknown";
            }
        }
    }
}
