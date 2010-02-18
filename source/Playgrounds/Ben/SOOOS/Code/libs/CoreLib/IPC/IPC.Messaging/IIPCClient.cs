using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreLib.IPC.IPC.Messaging
{

    /// <summary>
    ///  <endpoint name="httpEndpoint" 
    ///  address="http://localhost:8080/echo/svc"
    ///  binding="basicHttpBinding"
    ///  contract="ServiceLibrary.IEchoService"/>

    /// </summary>
    public interface IIPCClient
    {
        void SendMessage(ref IIPCMessage message);

        Uri ServerAddress { get; } 
        // compiler will generate from interface to avoid boxing 
         //eg 
        //void SendMemoryRequestMessage(ref MemoryRequestMessage message); 

    }
}
