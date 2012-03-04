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
using Cosmos.Assembler;



namespace Cosmos.Core.Process_System
{
    class P_Thread
    {

        protected int id = 0;
        protected int returnProcess = 0;
        protected string returnIp = null;
        protected string returnMac = null;
        protected int localRange = 0;
        protected int priority = 3;
        protected StackContents stack = new StackContents(); //Hold code that going to be load in memory.
        protected int toProcess = -1;
        protected string toIp = null;
        protected string toMac = null;

        public P_Thread(int thisReturnProcess, int thisId, int thisLocalRange, int thisPriority, StackContents stack)
        {
            id = thisId;
            returnProcess = thisReturnProcess;
            localRange = thisLocalRange;
            priority = thisPriority;
        }
        public P_Thread(int thisReturnProcess, int thisId, int thisLocalRange, int thisPriority, string thisToIp, string thisToMac, int thisToProcess, StackContents stack)
        {
            id = thisId;
            returnProcess = thisReturnProcess;
            localRange = thisLocalRange;
            priority = thisPriority;
            toIp = thisToIp;
            toMac = thisToMac;
            toProcess = thisToProcess;
        }

        public int GetId
        {
            get
            {
                return id; 
            }
        }
        public int GetReturnProcess
        {
            get
            {
                return returnProcess; 
            }
        }
        public int GetLocalRange
        {
            get
            {
                return localRange; 
            }
        }
        public int GetPriority
        {
            get
            {
                return priority; 
            }
        }
        public StackContents GetStack
        {
            get
            {
                return stack;
            }
        }
        public string GetReturnIP
        {
            get
            {
                return returnIp;
            }
        }
        public string GetReturnMac
        {
            get
            {
                return returnMac;
            }
        }
        public string GetToIp
        {
            get
            {
                return toIp;
            }
        }
        public string GetToMac
        {
            get
            {
                return toMac;
            }
        }
        public int GetToProcess
        {
            get
            {
                return toProcess;
            }
        }

    }
}
