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
    class Thread
    {

        protected int id = 0;
        protected int state = 0;
        protected int priority = 3;
        protected string name = null;

        /// <summary>
        /// Say what cpu and core the thread have been allocation to. Every Theard should be alloction to one core and one cpu only.
        /// 
        /// </summary>
        protected int cpu = 0;
        protected int core = 0;



        /// <summary>
        /// Use for Thread that come from a differnt process or and computer. Allow for the thread to know where to send object back.
        /// </summary>
        protected int returnProcess = 0;
        protected string returnIp = null;
        protected string returnMac = null;
        protected StackContents stack = new StackContents(); //Hold code that going to be load in memory.
 

        
        /// <summary>
        /// Program data and CPU data
        /// </summary>
        /// 
        protected uint esp = 0;
        protected uint ebp = 0;
        protected uint eip = 0;
        protected uint eax = 0;
        protected uint ebx = 0;
        protected uint ecx = 0;
        protected uint edx = 0;
        protected uint esi = 0;
        protected uint edi = 0;

        protected uint endStack = 0; 

        // To Do Load program code in memory;
        public void Load()
        {
        }

        public void P_Load()
        {
        }

        //To Do
        public void Start()
        {
            
        }

        public Thread(int thisId, int thisPriority,uint thisEsp,uint thisEndStack)
        {
            id = thisId;
            esp = thisEsp;
            endStack = thisEndStack;
            priority = thisPriority;

        }

        //Use when program code is not in memory (e.g. on a file).
        public Thread(int thisId, int thisPriority)
        {
            id = thisId;
            priority = thisPriority;

        }

        public Thread(int thisId, int thisPriority, string thisReturnIp, string thisReturnMac, int thisReturnProcess,StackContents thisStack)
        {
            id = thisId;
            returnProcess = thisReturnProcess;
            returnIp = thisReturnIp;
            returnMac = thisReturnMac;
            priority = thisPriority;
            stack = thisStack;
        }

        public void SaveState(uint thisEsp, uint thisEbp, uint thisEip, uint thisEax, uint thisEbx, uint thisEcx, uint thisEdx, uint thisEsi, uint thisEdi)
        {
            esp = thisEsp;
            ebp = thisEbp;
            eip = thisEip;
            eax = thisEax;
            ebx = thisEbx;
            ecx = thisEcx;
            edx = thisEdx;
            esi = thisEsi;
            edi = thisEdi;
        }

        public int ThreadState
        {
            get
            {
                return state;
            }
            set
            {
                state = value;
            }
        }

        public int ThreadId
        {
            get
            {
                return id; 
            }
        }

        public uint ThreadEndStack
        {
            get
            {
                return endStack;
            }
            set
            {
                endStack = value;
            }
        }

        public uint ThreadESP
        {
            get
            {
                return esp;
            }
            set
            {
                esp = value;
            }
        }

        public uint ThreadEBP
        {
            get
            {
                return ebp; 
            }
            set
            {
                ebp = value;
            }
        }

        public uint ThreadEIP
        {
            get
            {
                return eip; 
            }
            set
            {
                eip = value;
            }
        }

        public uint ThreadEAX
        {
            get
            {
                return eax; 
            }
            set
            {
                eax = value; 
            }
        }

        public uint ThreadEBX
        {
            get
            {
                return ebx;
            }
            set
            {
                ebx = value;
            }
        }

        public uint ThreadECX
        {
            get
            {
                return ecx; 
            }
            set
            {
                ecx = value;
            }
        }

        public uint ThreadEDX
        {
            get
            {
                return edx;
            }
            set
            {
                edx = value; 
            }
        }

        public uint ThreadESI
        {
            get
            {
                return esi;
            }
            set
            {
                esi = value;
            }
        }

        public uint ThreadEDI
        {
            get
            {
                return edi;
            }
            set
            {
                edi = value;
            }
        }

        public int ThreadPriority
        {

            get
            {
                return priority;
            }
            set
            {
                priority = value;
            }
        }

        public int ThreadCPU
        {
            get
            {
                return cpu;
            }
            set
            {
                cpu = value;
            }
        }

        public int ThreadCore
        {
            get
            {
                return core;
            }
            set
            {
                core = value;
            }
        }
    }
}
