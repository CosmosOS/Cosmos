using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreLib.IPC
{
    /// <summary>
    /// Only used where performance is critical , this message can save a fixed memory address or trigger an even eg set a Wait on an Autoreset Event
    /// 
    ///TODO See if we dont need  , the nice part here is we dont need to "burdon" IPC with the abbility to have to wait for a specific message
    ///and the message can be sent direct to the original sender. The message can save an original fixed address either fixed or marked with the GCFIxedAttribute ( which is private but copied)  , 
    ///store the result and then call Set on a wait handle .  A copy of trusted Reply classes will be provided  which can be wrapped to do  other things. 
    ///
    /// 
    ///
    /// Another way would be instead of waiting for a specific message maybe to continue reading messages but dont process them , but this would require more complicated thread/schedular interactions. 
    /// when we received the message we are waiting for activate the event. eg when we receive the correct message we have to reprocess the saved messaged by the message pump. 
    /// </summary>
    public interface IIPCMessageWithReply : IIPCMessage  //We do not support structs only immutables
    {

 
        ///// <summary>
        ///// eg 
        ///// 
        ///// See sample for Memory Management
        ///// 
        ///// 
        ///// </summary>
        ///// <param name="msg"></param>
        //void Reply(ref IIPCMessageStuct msg); 


        /// <summary>
        /// except from 
        /// </summary>
        //public void Reply(IIPCMessage msg); // will be boxed unles we get compiler to generate methods. BUT Boxing cannot work since the box will be a ref to a different GC !.
    }
}
