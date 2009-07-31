using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace Cosmos.Kernel.IPC
{
    /// <summary>
    /// Why use a message ? In small projects its anoying but as the system grows benefits are huge.

    /// 
    /// TODO addressing - Address apps and services directly or send to global queue
    /// 
    /// I considered using structs but you really want to use queues etc and a ref pass is prob more 
    /// efficient once you have processes queueing .
    /// 

    /// </summary>
    public abstract class KernelMessage
    {
  


        /// <summary>
        /// messages must be created by the kernel
        /// </summary>
        public KernelMessage()
        {
            throw new InvalidOperationException("Cant use default on Kernel Messages"); 
        }

       

      

        
        /// <summary>
        ///  this will be inlined
        /// </summary>
        public void Send()
        {

        }

        //static as

    }
}
