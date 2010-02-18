using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreLib.ProcessModel;

namespace CoreLib.IPC
{

    //1.
    //memoryManagerClient.SendMemoryRequestMessage(ref mmRequestMessage);

    //2.
    //SendMemoryRequestMessage calls
    // internal outpipe.


    //calls 
    //outpipe.BuddyPipe.MessageWriter.WriteStruct(&mmRequestMessage , sizeof( mmRequestMessage) ); 

    //// options are WriteStruct 
    //// WriteImmutable
    //Object



    /// <summary>
    /// Note we have removed the abbility to wait on multiple messages etc , this may be implemented in some readers but with a custom reply i see no reason why this is needed. 
    /// 
    /// eg for a Synch style send and wait for message , create a cutom message with a reply which has the information
    /// 
    /// </summary>


    [TrustedInterface]
    internal interface IIPCMessageReader
    {
        STP SenderEndPoint {get; } // while we have an end point i dont want to make Channels sockets or pipes yet , need to see what works best

       
        
        // pass these by reference 
        // problem with  void ReceiveMessage(out IIPCMessage message); is who stores and releases the message ?
        // it would require the Reader to keep a List and release when not needed with all this effort you may as well use a heap structure. 
        void ReceiveMessage(out IIPCMessage message);

        void ReceiveMessage(out IPCMessageHolder message);
 

    }
}
