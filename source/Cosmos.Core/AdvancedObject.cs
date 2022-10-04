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
    /// This Class was Created by TheFocusMan to help pepole
    /// </summary>
    public class AdvancedObject
    {
        public unsafe uint GetTypeId()
        {
            return GCImplementation.GetType(this);
        }
        /// <summary>
        /// Do this function to help yourself
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
        /// Do this function to help yourself
        /// </summary>
        public void FixStackPointTo(uint stack)
        {
            CPU.DisableInterrupts();
            CPU.SetESPValue(stack);
            CPU.EnableInterrupts();
        }

        public bool NeedToFixStackPoint()
        {
            return CPU.GetESPValue() != GetTypeId();
        }
    }
}
