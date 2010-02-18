using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreLib.IPC.IPC.Messaging.SyncNonQueued
{
    public class SyncNonQueuedMessageReader : IIPCMessageReader
    {

        SyncNonQueuedMessageWriter writer;
      //  ITube tube; 
      
        System.Threading.EventWaitHandle waitHandle;

      

       // Queue<IIPCMessage> queue;  

        /// <summary>
        /// Note internal !
        /// </summary>
        internal SyncNonQueuedMessageReader(Tube tube)  //TODO pipe 
        {
           // queue = new Queue<IIPCMessage>(); 
            writer = new SyncNonQueuedMessageWriter(tube , this);
            waitHandle = new System.Threading.AutoResetEvent(false); 

        }

        public ProcessModel.STP SenderEndPoint
        {
            get { throw new NotImplementedException(); }
        }


        internal IIPCMessageWriter Writer
        {
            get { return writer; }
        }

        // out here is brain dead...since it will be overwritten by next message
        public void ReceiveMessage(out IIPCMessage message)
        {
            message = ReceiveMessage();
        }

        public IIPCMessage ReceiveMessage()
        {
            // DOesnt work since the message ID  will be set to zero. 
            //  IIPCMessage message;
            //writer.GetMessage( out message ); //copies from remote GC
            //message.messageId = 0;

            var message = writer.Message; //copies from remote GC
            message.messageId = 0;
            waitHandle.Set();
            return message;

        }


        //IIPCMessageWriter IIPCMessageReader.Writer
        //{
        //    get { throw new NotImplementedException(); }
        //}


        public void ReceiveMessage(out IPCMessageHolder message)
        {
            message = new IPCMessageHolder(writer.Tube, ReceiveMessage()); 
        }

        public System.Threading.EventWaitHandle WaitHandle
        {
            get { return waitHandle; }

        }

      
    }
}
