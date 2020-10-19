using Cosmos.Debug.Kernel;

namespace Cosmos.Core
{
    /// <summary>
    /// Bootstrap class. Used to invoke pre-boot methods.
    /// </summary>
    /// <remarks>Bootstrap is a class designed only to get the essentials done.</remarks>
    public unsafe static class Bootstrap
    {
        // See note in Global - these are a "hack" for now so
        // we dont force static init of Global, and it "pulls" these later till
        // we eventually eliminate them
        /// <summary>
        /// PIC interrupt.
        /// </summary>
        static public PIC PIC;
        // Has to be static for now, ZeroFill gets called before the Init.
        /// <summary>
        /// CPU.
        /// </summary>
        static public readonly CPU CPU = new CPU();

        /// <summary>
        /// Multiboot header pointer.
        /// </summary>
        public static Multiboot.Header* MultibootHeader;

        /// <summary>
        /// VBE mode info pointer.
        /// </summary>
        public static VBE.ModeInfo* modeinfo;
        /// <summary>
        /// VBE controller info pointer.
        /// </summary>
        public static VBE.ControllerInfo* controllerinfo;

        // Bootstrap is a class designed only to get the essentials done.
        // ie the stuff needed to "pre boot". Do only the very minimal here.
        // IDT, PIC, and Float
        // Note: This is changing a bit GDT (already) and IDT are moving to a real preboot area.
        /// <summary>
        /// Init the boot strap. Invoke pre-boot methods.
        /// </summary>
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

            MultibootHeader = (Multiboot.Header*)Multiboot.GetMBIAddress();

            modeinfo = (Core.VBE.ModeInfo*)MultibootHeader->vbeModeInfo;
            controllerinfo = (Core.VBE.ControllerInfo*)MultibootHeader->vbeControlInfo;
        }
    }
}
