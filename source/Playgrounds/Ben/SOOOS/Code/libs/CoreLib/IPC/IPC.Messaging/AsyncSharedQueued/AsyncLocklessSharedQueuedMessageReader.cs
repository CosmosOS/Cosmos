using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreLib.Collections;
using System.Threading;

namespace CoreLib.IPC.IPC.Messaging.AsyncSharedQueued
{
    // need to comnpare shared queue ( ie more lock contention and more copying ) vs multiple queueus ( ie more CPU checking queues , more memory usage , less copying )  
    // note a shared lock queue doesnt need volotiles so will use less memory ( doesnt need to pad out cache lines) 
    public class AsyncSharedQueuedMessageReader : IIPCMessageReader ,IDisposable
    {
    //    IIPCMessageWriter writer;
        
        //As the lock is so quickly released and there is little contention this will be quite quick. 
        ExpandableSpinQueue<IPCMessageHolder> queue;
        AutoResetEvent waitEvent = new AutoResetEvent(false); //TODO write our own


        
   

        Tube tube; 


        /// <summary>
        /// Note internal !
        /// </summary>
        internal AsyncSharedQueuedMessageReader(Tube commTube, ExpandableSpinQueue<IPCMessageHolder> sharedQueue) 
        {
            queue = sharedQueue;
            tube = commTube;           
        }

        public ProcessModel.STP SenderEndPoint
        {
            get { return tube.Source; }
        }

        

       


        //dont use

        /// <summary>
        /// Messaage must be finished with before removing the next.           
        /// </summary>
        /// <param name="message"></param>
        public void ReceiveMessage(out IIPCMessage message)
        {
            IPCMessageHolder messageHolder ; 
            ReceiveMessage(out messageHolder);

            message = messageHolder.Message;
        }


        public void ReceiveMessage(out IPCMessageHolder message)
        {

            // lets add some waiting..
            while (queue.Dequeue(out message) == false)
            {
                waitEvent.WaitOne(200); 
            }
        }


        internal ExpandableSpinQueue<IPCMessageHolder> Queue
        {
            get { return queue; }

        }



        internal AutoResetEvent WaitEvent
        {
            get { return waitEvent; }
        }





        public void Dispose()
        {
            waitEvent.Close(); 
            //TODO queue
        }
    }
}
