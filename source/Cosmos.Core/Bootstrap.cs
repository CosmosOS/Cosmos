namespace Cosmos.Core
{
    public static class Bootstrap
    {
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
        public static void Init()
        {
            // Drag this stuff in to the compiler manually until we add the always include attrib
            INTs.Dummy();

            PIC = new PIC();
            CPU.UpdateIDT(true);

            /* TODO check using CPUID that SSE2 is supported */
            CPU.InitSSE();

            /*
             * We liked to use SSE for all floating point operation and end to mix SSE / x87 in Cosmos code
             * but sadly in x86 this resulte impossible as Intel not implemented some needed instruction (for example conversion
             * for long to double) so - in some rare cases - x87 continue to be used. I hope passing to the x32 or x64 IA will solve
             * definively this problem.
             */
            CPU.InitFloat();

            // Managed_Memory_System.ManagedMemory.Initialize();
            // Managed_Memory_System.ManagedMemory.SetUpMemoryArea();
        }
    }
}
