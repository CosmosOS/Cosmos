////////////////////////////////////////
//  Add you name here when you change to add to this class
//
//  Class made by: Craig Lee Mark Adams
//  
//  


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Core.Process_System
{
    class ProcessMessage : ICloneable
    {

        private int fromProcessId;
        private string fromIP;
        private string fromMAC;
        private object messageObject;
        private int toPocessId;
        private string toIP;
        private string toMAC;
        private string objectType;

        public ProcessMessage(int thisFromProcessId, int thisToPocessId, string thisFromIP, string thisFromMAC, string thisToIP, string thisToMAC, object thisMessageObject, string thisObjectType) 
        {
            fromProcessId = thisFromProcessId;
            messageObject = thisMessageObject;
            fromIP = thisFromIP;
            fromMAC = thisFromMAC;
            toPocessId = thisToPocessId;
            toIP = thisToIP;
            toMAC = thisToMAC;
            objectType = thisObjectType;
        }

        #region ICloneable Members

        public object Clone()
        {

            //Do not work as the messageObject is of unknow object type is been pass and object type can not do deep copy. 
            //Need to be done show that the new process does not have a reference  to Memory in the other process.  

            // thisMessageObject = messageObject.Clone(); 

            ///

            ProcessMessage thisProcessMessage = new ProcessMessage(fromProcessId, toPocessId, fromIP, fromMAC, toIP, toMAC, messageObject, objectType);
             return thisProcessMessage;
            throw new NotImplementedException();
        }

        #endregion
}
}
