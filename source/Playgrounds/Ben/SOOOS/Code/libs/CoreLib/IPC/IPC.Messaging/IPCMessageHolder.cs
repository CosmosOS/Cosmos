using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreLib.Capabilities;

namespace CoreLib.IPC
{
    public struct IPCMessageHolder
    {
        IIPCMessage message;

      
        Tube channel;

      

        public ICapability Sender 
        {
            get
            {
                return Channel.Source; 
            }
        } // get from Pipe.


        internal IPCMessageHolder(Tube tube, ref IIPCMessage msg)
        {
           
            message = msg; 
            channel = tube;

        }


        //copy in the params
        internal IPCMessageHolder(Tube tube, IIPCMessage msg)
        {

            message = msg;
            channel = tube;

        }  

        public Tube Channel
        {
            get { return channel; }
    
        }  public IIPCMessage Message
        {
            get { return message; }
               }

        //optional prob sequence enough ? 
    //    public DateTime Received { get; set; } //TODO set to ticks when ! 
    //    public uint Sequence { get; set; }

    }
}
