using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Core;
using IL2CPU.API;

namespace Cosmos.Core
{
    /// <summary>
    /// This Class is created for Add Add-ons methods to object by add this to base class
    /// </summary>
    public class AdvancedObject
    {
        /// <summary>
        /// Get the type id dierectly instead of Run GetType
        /// </summary>
        /// <returns>Type id</returns>
        public unsafe uint GetTypeId()
        {
            return GCImplementation.GetType(this);
        }
        /// <summary>
        /// If you stuck on a problem run this function
        /// <c>Waring: Runnig this function may get to crash</c>
        /// </summary>
        public void FixStackPoint()
        {
            if (NeedToFixStackPoint())
            {
                Global.mDebugger.Send("Stack is need to be fixed");
                FixStackPointTo(GetTypeId());
            }
        }

        /// <summary>
        /// This function is set the stack point to the target type
        /// <c>Waring: Runnig this function may get to crash</c>
        /// </summary>
        /// <param name="stack">The target type id</param>
        public void FixStackPointTo(uint stack)
        {
            CPU.DisableInterrupts();
            CPU.SetESPValue(stack);
            CPU.EnableInterrupts();
        }

        /// <summary>
        /// Check if the stack type is match to the object type
        /// </summary>
        public bool NeedToFixStackPoint()
        {
            return CPU.GetESPValue() != GetTypeId();
        }
    }
}
