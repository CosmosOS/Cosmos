using System;
using System.Collections.Generic;
using System.Text;

namespace SteveKernel
{
    class VBE3
    {

        public static unsafe uint FindPM()
        {
            Console.Write("searching for vbe3... searching BIOS...");
            // check bios
            for (uint addr = 0xE0000; addr < 0x100000; addr += 4)
                if (CheckForPM(addr))
                    return addr;

            Console.Write("searching EBD...");

            // check extended bios
            uint ebda_address = *((uint*)0x040E);

            ebda_address = (ebda_address * 0x10) & 0x000fffff;

            for (uint addr = ebda_address; addr < ebda_address + 1024*32; addr += 4)
                if (CheckForPM(addr))
                    return addr;

            Console.Write("not found");
            // not found
            return 0;
        }

        public static unsafe bool CheckForPM(uint addr)
        {
            // check signature
            byte* ch = (byte*)addr;

            if (*(ch++) != (byte)'P') return false;
            if (*(ch++) != (byte)'M') return false;
            if (*(ch++) != (byte)'I') return false;
            if (*(ch++) != (byte)'D') return false;

            return true;
            // check checksum
            byte sum = 0;
            byte* check = (byte*)addr;

            for (int i = 0; i < 34; i++)
                sum += *(check++);

            return (sum == 0);
        }
    }
}
