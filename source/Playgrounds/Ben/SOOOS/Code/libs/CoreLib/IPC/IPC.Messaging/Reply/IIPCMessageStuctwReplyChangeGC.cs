using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreLib.GC;
using Util;

namespace CoreLib.IPC
{

    /// <summary>
    /// TODO provide a non event driven one.. via a set callback
    /// 
    /// event driven probably too slow and a bit dangerous
    /// </summary>
    public struct IPCMessageStuctwReplyChangeGC : IIPCMessageStuctwReply
    {
        //Trusted 
        private ICollector captureGC;
        private event EventHandler<EventArgs<IIPCMessage>> replyMessageEvent; // sender hooks 




        //IPCMessageStuctwReplyChangeGC()
        //{

        //}

        //trusted ..?
        // NEEDS To be typed to the right value boxing is bad ! 
        //shown here as an example
        public void Reply(ref IIPCMessageStuct msg)
        {
            // set collector domain .
            using (captureGC.GetContext())
            {
                // do stuff here .. 
                OnReply(new EventArgs<IIPCMessage>( msg) );  // boxed !
            }


    
        
        }


        private void OnReply(EventArgs<IIPCMessage>  msg)
        {
            // hello
            if (replyMessageEvent != null)
                replyMessageEvent(null, msg); 
        }

        public ulong messageId
        {
            get { throw new NotImplementedException(); }
        }

        //WARNING events and structs are dangerous
        public event EventHandler<EventArgs<IIPCMessage>> ReplyMessageEvent
        {
            add
            {
                replyMessageEvent += value;
            }
            remove
            {
                replyMessageEvent -= value; 
            }
        }
    }
}
