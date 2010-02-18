using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ben.Kernel.IPC;


namespace Ben.Services.MM
{
    // different style message  with only the creater being able to change data
    public sealed class AllocationResponse : ReplyMessage
    {
        private UIntPtr address;

        public UIntPtr Address
        {
            get { return address; }

        }


        public AllocationResponse(UIntPtr Address ,  ushort replyId ) : base (replyId  )
        {
            address = Address; 
        }
        public override void Validate()
        {

        }
    }
}
