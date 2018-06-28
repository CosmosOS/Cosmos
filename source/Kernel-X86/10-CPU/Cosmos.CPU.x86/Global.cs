using Cosmos.Debug.Kernel;

namespace Cosmos.CPU.x86 {
    public static class Global {
        public static readonly Debugger mDebugger = new Debugger("Processor", "Global");

        public static BaseIOGroups BaseIOGroups = new BaseIOGroups();

        // These are used by Bootstrap.. but also called to signal end of interrupt etc...
        // Need to chagne this.. I dont like how this is.. maybe isolate or split into to classes... one for boostrap one for
        // later user
        static public PIC PIC {
            get {
                return Boot.PIC;
            }
        }

        static public Processor Processor {
            get {
                return Boot.Processor;
            }
        }

        static public void Init() {
            // See note in Bootstrap about these

            // DONT transform the properties in fields, as then they remain null somehow.
        }
    }
}
