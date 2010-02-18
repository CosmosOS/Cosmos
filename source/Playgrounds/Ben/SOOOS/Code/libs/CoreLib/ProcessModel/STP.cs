

using CoreLib.Diagnostics;
using CoreLib.IPC;
using CoreLib.Capabilities;
namespace CoreLib.ProcessModel
{

    /// <summary>
    /// NOte this is the user lib version  , the underlying schedular object is wrapped.
    /// 
    /// STP holds 1 thread !
    /// 
    /// Note statics are valid only within an STP , if you share multiple STPs ovcer the same COllectorDomain statics will be duplicated
    /// 
    /// Id is the same as the thread.
    /// </summary>
    public class STP : ICapability // 
    {

        ///// <summary>
        /////  Holds current message for some IPC implemetations 
        ///// </summary>
        //internal IPCMessageHolder CurrentMessage { get; set; }

      

        string name = string.Empty;

   
      
        Thread thread = Thread.None;

        internal IIPCSTPExtention IPCSTPExtention { get; set; }


        static public STP Current 
        { 
            get
            {
                throw new SystemException("TODO");
            }
 
        }

        internal STP()
        {

        }

        public Thread Thread
        {
            get { return thread; }

        }

        public string Name
        {
            get { return name; }

        }

        public IRevokeCapability GetRevokeCapability()
        {
            throw new System.NotImplementedException();
        }

        public bool IsNull
        {
            get { throw new System.NotImplementedException(); }
        }
    }
}
