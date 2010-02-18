using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreLib.ProcessModel;
using System.Threading;

namespace CoreLib.IPC.IPC.Messaging.Minix
{

    public enum IPCState { UNKNOWN, RECEIVING, SENDING, BUSY } 

    internal class MinixIPCExtention : IIPCSTPExtention
    {
        STP STP; 
        IPCMessageHolder message;

        internal IPCState State { get; set; }
        internal IPCMessageHolder Message
        {
            get { return message; }
            set { message = value; }
        } 
        internal System.Collections.Generic.Queue<IPCMessageHolder> ReceiveQueue { get; set; }
        AutoResetEvent waitEvent;

        internal MinixIPCExtention(STP source, AutoResetEvent waitEvent)
        {
            this.waitEvent = waitEvent;
            ReceiveQueue = new System.Collections.Generic.Queue<IPCMessageHolder>(); 
        }

        internal MinixIPCExtention(STP stp)
        {
            this.STP = stp; 

        }

        // use Waitevent as its cleaner
        //TODO schedule destination

           // in case they are waiting
        //        waitEvent.Set();

        internal void ScheduleDestination()
        {
            // schedule     
            waitEvent.Set();
        }
    }
}
