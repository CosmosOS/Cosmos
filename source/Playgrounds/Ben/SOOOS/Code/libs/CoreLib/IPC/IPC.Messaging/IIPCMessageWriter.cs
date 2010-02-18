using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreLib.IPC
{



    //Note Pipes can be one way using reply mechanism! 
    //Purpose is to 

    //Pipe
    //    InPipe
    //        MessageReader
    //            (MessageWriter
    //)

    //    OutPipe
    //        EndPointInterface ( Only part exposed to user) 
    //        MessageWriter


    //IPCManager Wires things up.


    //Access via Capability ( ie reference to the Pipe) ?

    //Or send Message to serviceManager and get a Pipe back ?



    //Send A Message 


    //Use Client and Service ( ala WCF) 


    ////Cache Results 
    //eg 
    //static IMMPIpe memoryManagerClient;
    //static ISMPIpe serviceManagerClient;

    ////static IPIpe memoryManagerPipe;


    //1.
    //memoryManagerClient.SendMemoryRequestMessage(ref mmRequestMessage);

    //2.
    //SendMemoryRequestMessage calls
    // internal outpipe.


   


    // Creator Creates the Channel then exposes it to the ServiceManager


    /// <summary>
    /// Note we dont know who we are writing to.
    /// 
    /// This can be a bit quicker by using a struct since that avoids pushing this on the stack 
    /// however being able to change IPC algorthms will have much higher pay offs initially.
    /// </summary>
  //  [IPCStructCall]
    public interface IIPCMessageWriter
    {

        //with no references
        void WriteStruct(ref IIPCMessage  message);

        //struct or object can contain references
        // This will use a Helper to copy to teh destination GC
        void WriteImmutable(ref IIPCMessage message); // size not needed overload if it is , int size);

        //TODO how do we do this 
      //  IIPCMessageWriter Reader { get; }  

         Tube Tube { get; }

        /// <summary>
         /// The compiler will generate an overload for  WriteStruct for each valid type that way we avoid the boxing. ( Based on IPCStructCall being present) 
         /// and the valid types . It then generates direct instead of boxed calls. 
        /// </summary>
         ICollection<Type> ValidTypes { get; } // set in constructor 



    }
}
