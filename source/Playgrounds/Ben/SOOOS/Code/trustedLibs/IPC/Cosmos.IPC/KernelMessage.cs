using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;



namespace Ben.Kernel.IPC
{
    /// <summary>
    /// Why use a message ? In small projects its anoying but as the system grows benefits are huge.

    /// 
    /// TODO addressing - Address apps and services directly or send to global queue
    /// 
    /// I considered using structs
    /// 
    /// Note with our fast GC's there should not be much difference between the stack and the GC Nursery ( unless a collection takes place) 
    /// and we get inheritance , polymorphism and nice looking code. 
    ///
    /// The BIG issue with structs is boxing . You would need a seperate Send method for each type or you have to box it. 
    /// 
    /// This way is much better .
    /// </summary>
    public abstract class KernelMessage
    {
        static int count = 1; 
        protected int messageId;  // note uniqueness is only guaranteed for Sender + id 
                                    //note short is enough.


        protected KernelMessage()
        {
            messageId = GenerateId();
        }
        /// <summary>
        /// messages must be created by the kernel
        /// </summary>
        protected KernelMessage(int id)
        {
            messageId = id; 
        }


        private int GenerateId()
        {
            return Interlocked.Increment(ref count);
        }


        public abstract void Validate(); //virtual to slow? If so new it

        public bool IsValid()
        {

            try
            {
                Validate();
                return true;
            }
            catch (MessageValidationException)
            { 

            }
            return false; 
        }

        //static as

    }
}
