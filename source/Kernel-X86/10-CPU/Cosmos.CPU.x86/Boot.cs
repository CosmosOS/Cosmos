using System;
using System.Collections.Generic;
using System.Text;
using IL2CPU.API.Attribs;

namespace Cosmos.CPU.x86 {
    static public class Boot {

        // OLD CODE pasted.. .still cleaning/porting


        // See note in Global - these are a "hack" for now so
        // we dont force static init of Global, and it "pulls" these later till
        // we eventually eliminate them
        static public PIC PIC;
        // Has to be static for now, ZeroFill gets called before the Init.
        static public readonly Processor Processor = new Processor();

        // Bootstrap is a class designed only to get the essentials done.
        // ie the stuff needed to "pre boot". Do only the very minimal here.
        // IDT, PIC, and Float
        // Note: This is changing a bit GDT (already) and IDT are moving to a real preboot area.

        [BootEntry(10)]
        static private void Init() {
            PIC = new PIC();
            Processor.UpdateIDT(true);

            /* TODO check using CPUID that SSE2 is supported */
            Processor.InitSSE();

            /*
             * We liked to use SSE for all floating point operation and end to mix SSE / x87 in Cosmos code
             * but sadly in x86 this resulte impossible as Intel not implemented some needed instruction (for example conversion
             * for long to double) so - in some rare cases - x87 continue to be used. I hope passing to the x32 or x64 IA will solve
             * definively this problem.
             */
            Processor.InitFloat();

            // Managed_Memory_System.ManagedMemory.Initialize();
            // Managed_Memory_System.ManagedMemory.SetUpMemoryArea();
        }
    }
}
