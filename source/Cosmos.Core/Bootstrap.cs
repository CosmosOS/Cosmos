namespace Cosmos.Core;

/// <summary>
///     Bootstrap class. Used to invoke pre-boot methods.
/// </summary>
/// <remarks>Bootstrap is a class designed only to get the essentials done.</remarks>
public static class Bootstrap
{
    // See note in Global - these are a "hack" for now so
    // we dont force static init of Global, and it "pulls" these later till
    // we eventually eliminate them


    /// <summary>
    ///     PIC interrupt.
    /// </summary>
    public static PIC PIC;

    // Bootstrap is a class designed only to get the essentials done.
    // ie the stuff needed to "pre boot". Do only the very minimal here.
    // IDT, PIC, and Float
    // Note: This is changing a bit GDT (already) and IDT are moving to a real preboot area.
    /// <summary>
    ///     Init the boot strap. Invoke pre-boot methods.
    /// </summary>
    public static void Init()
    {
        // Drag this stuff in to the compiler manually until we add the always include attrib
        Multiboot2.Init();
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
    }
}
