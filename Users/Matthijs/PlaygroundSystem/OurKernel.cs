using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.HAL;
using Cosmos.System;
using PlaygroundHAL;

namespace PlaygroundSystem
{
    public abstract class OurKernel: Kernel
    {
        protected override TextScreenBase GetTextScreen()
        {
            return new DebugTextScreen();
        }

        public void DoIt()
        {
        }
    }
}
