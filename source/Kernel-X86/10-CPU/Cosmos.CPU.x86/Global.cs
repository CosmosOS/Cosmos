using Cosmos.Debug.Kernel;

namespace Cosmos.CPU.x86;

public static class Global
{
    public static readonly Debugger mDebugger = new("Processor", "Global");

    public static BaseIOGroups BaseIOGroups = new();

    // These are used by Bootstrap.. but also called to signal end of interrupt etc...
    // Need to chagne this.. I dont like how this is.. maybe isolate or split into to classes... one for boostrap one for
    // later user
    public static PIC PIC => Boot.PIC;

    public static Processor Processor => Boot.Processor;

    public static void Init()
    {
        // See note in Bootstrap about these

        // DONT transform the properties in fields, as then they remain null somehow.
    }
}
