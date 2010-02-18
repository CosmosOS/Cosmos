using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ben.Kernel.Capabilities
{
    public class ProcessorCapability :ICapability
    {
        private Object proc;
        private ulong whichProcessorFlags;

        //TODO proc MUST be ProcessorScheduleQueue
        internal ProcessorCapability(Object proc , ulong whichProcessor, int cpuTime)
        {
            this.proc = proc;
            this.whichProcessorFlags = whichProcessor; 
        }

   




        public void Revoke()
        {
            proc = null; 
        }

        public bool IsNull
        {
            get
            {
                if (proc == null)
                    return true;

                return false; 
            }
        
        }


    }
}
