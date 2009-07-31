using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Cosmos.Kernel.IPC
{

    /// </summary>
    public class KernelErrorMessage : KernelMessage
    {
        public Exception ex; 

        /// <summary>
        /// messages must be created by the kernel
        /// </summary>
        public KernelErrorMessage(Exception ex)
        {
            this.ex = ex; 
        }


        //static as

    }
}
