using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreLib.IPC
{
    //used for structs keep thse small
    public interface IIPCMessage
    {
        //TODO set should be via method or something.. 
        ulong messageId { get; set; } // normally & 

        //There is a trusted method on the channel that builds it,


        // procedure is service installer gets capability any IPC requires the user to share the capability  eg print server , file server etc 

     //   bool IsEmpty();
    }
}
