using Cosmos.Core;
using Cosmos.Debug.Kernel;

namespace Cosmos.Core
{
    public static class Global
    {
        /// <summary>
        /// Core ring debugger instance, with the Global tag.
        /// </summary>
        public static readonly Debugger mDebugger = new Debugger("Core", "Global");

        /// <summary>
        /// Base IO gruops.
        /// </summary>
        public static BaseIOGroups BaseIOGroups = new BaseIOGroups();

        // These are used by Bootstrap.. but also called to signal end of interrupt etc...
        // Need to chagne this.. I dont like how this is.. maybe isolate or split into to classes... one for boostrap one for
        // later user
        /// <summary>
        /// Get PIC.
        /// </summary>
        static public PIC PIC
        {
            get
            {
                return Bootstrap.PIC;
            }
        }

        /// <summary>
        /// Get CPU.
        /// </summary>
        static public CPU CPU
        {
            get
            {
                return Bootstrap.CPU;
            }
        }

        /// <summary>
        /// Init <see cref="Global"/> instance.
        /// </summary>
        static public void Init()
        {
            // See note in Bootstrap about these

            // DONT transform the properties in fields, as then they remain null somehow.
        }
    }
}
