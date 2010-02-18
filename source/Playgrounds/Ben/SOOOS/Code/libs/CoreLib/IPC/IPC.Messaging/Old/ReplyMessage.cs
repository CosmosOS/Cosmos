using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Ben.Kernel.IPC
{

    /// </summary>
    public abstract class ReplyMessage : KernelMessage
    {
        private ushort replyToMessageId;



        /// <summary>
        /// messages must be created by the kernel
        /// </summary>
        public ReplyMessage(ushort replyToMessageId )
            : base()
        {
            this.replyToMessageId = replyToMessageId;
        }


        //static as

        public uint ReplyToMessageId
        {
            get { return replyToMessageId; }
        }

    }
}
