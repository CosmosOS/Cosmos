using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreLib.Collections;

namespace CoreLib.IPC.IPC.Messaging.AsyncLocklessQueued
{
    public class AsyncLocklessQueuedMessageReader : IIPCMessageReader
    {
        bool IsMultiThreaded = false; 

        AsyncLocklessQueuedMessageWriter writer;
    //    CircularFifoQueue<IIPCMessage> queue;
        SafeCircularFifoQueue<IIPCMessage> queue;
        System.Threading.AutoResetEvent produceEvent;
        System.Threading.AutoResetEvent consumeEvent; 
      //  ITube tube; 


        /// <summary>
        /// Note internal !
        /// </summary>
        internal AsyncLocklessQueuedMessageReader(Tube tube)  //TODO pipe 
        {
            queue = new SafeCircularFifoQueue<IIPCMessage>(100);  //TODO size
            

            produceEvent = new System.Threading.AutoResetEvent(false); 
            consumeEvent = new System.Threading.AutoResetEvent(true); 
            writer = new AsyncLocklessQueuedMessageWriter(queue , tube   , produceEvent , consumeEvent);

        }

        public ProcessModel.STP SenderEndPoint
        {
            get { throw new NotImplementedException(); }
        }

        internal IIPCMessageWriter Writer
        {
            get { return writer; }
        }

        public void ReceiveMessage(out IIPCMessage message)
        {
            throw new NotImplementedException();
        }


        //IIPCMessageWriter IIPCMessageReader.Writer
        //{
        //    get { throw new NotImplementedException(); }
        //}


        public void ReceiveMessage(out IPCMessageHolder message)
        {
            throw new NotImplementedException();
        }
    }
}
