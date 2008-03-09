using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware.Network
{    
    public abstract class NetworkDevice : Device
    {

        public abstract MACAddress MACAddress
        {
            get;
        }

        public abstract bool QueueBytes(byte[] buffer, int offset, int length);

        public abstract bool RecieveBytes(byte[] buffer, int offset, int max);

        public abstract int BytesAvailable();

        public abstract bool IsSendBufferFull();
        public abstract bool IsRecieveBufferFull();        
    }
}
