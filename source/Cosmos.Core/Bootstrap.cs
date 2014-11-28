using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Core {
    public static class Bootstrap {
        // See note in Global - these are a "hack" for now so
        // we dont force static init of Global, and it "pulls" these later till
        // we eventually eliminate them
        static public PIC PIC;
        // Has to be static for now, ZeroFill gets called before the Init.
        static public readonly CPU CPU = new CPU();

        // Bootstrap is a class designed only to get the essentials done.
        // ie the stuff needed to "pre boot". Do only the very minimal here.
        // IDT, PIC, and Float
        // Note: This is changing a bit GDT (already) and IDT are moving to a real preboot area.
        public static void Init() {
            // Drag this stuff in to the compiler manually until we add the always include attrib
            INTs.Dummy();

            PIC = new PIC();
            CPU.UpdateIDT(true);
            CPU.InitFloat();

            // Not sure if this is necessary, heap is already used before we get here
            // and it seems to be fully (or at least partially) self initializing
            Heap.Initialize();

           // Managed_Memory_System.ManagedMemory.Initialize();
           // Managed_Memory_System.ManagedMemory.SetUpMemoryArea();
        }
    }
}
