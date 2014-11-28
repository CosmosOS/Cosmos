//  Add you name here when you change to add to this class
//
//  Class made by: Craig Lee Mark Adams
//  
//  


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Common;

namespace Cosmos.Core.Process_System
{

    // A lot need to done in this class. One par Core. it tell when thread can run on the core.
    // This shoud be a priority pre-emptive round-robin Hybrid. 


    class CoreProcessScheduler
    {
        protected LinkedQueue<Thread> realTime = new LinkedQueue<Thread>();     //Should avoid setting thread as readtime.
        protected LinkedQueue<Thread> level1Thread = new LinkedQueue<Thread>();
        protected LinkedQueue<Thread> level2Thread = new LinkedQueue<Thread>();
        protected LinkedQueue<Thread> level3Thread = new LinkedQueue<Thread>();
        protected LinkedQueue<Thread> level4Thread = new LinkedQueue<Thread>();
        protected LinkedQueue<Thread> level5Thread = new LinkedQueue<Thread>();

        protected List<Thread> runningThread = new List<Thread>();

        protected uint coreId;
        protected uint processId;
        protected int amountThreadHardware = 1;
        protected int amountThreads = 0; 



        protected int level1Loop = 0;
        protected int level2Loop = 0;
        protected int level3Loop = 0;
        protected int level4Loop = 0;
        protected int level5Loop = 0;

        //Keep Thread that are sleep or wait for IO 

        protected LinkedQueue<Thread> sleepThread = new LinkedQueue<Thread>();
        protected LinkedQueue<Thread> waitThread = new LinkedQueue<Thread>(); 

        public CoreProcessScheduler (uint thisCoreId, uint thisProcessId, int thisAmountThreadHardware)
        {
            coreId = thisCoreId;
            processId = thisProcessId;
            amountThreadHardware = thisAmountThreadHardware;
        }

        public void AddThread(Thread thread)
        {
            if (thread.ThreadPriority == -1)
            {
                realTime.Enqueue(thread);
            }
            else if (thread.ThreadPriority == 0)
            {
                level1Thread.Enqueue(thread);
            }
            else if (thread.ThreadPriority == 1)
            {
                level2Thread.Enqueue(thread); 
            }
            else if (thread.ThreadPriority == 2)
            {
                level3Thread.Enqueue(thread); 
            }
            else if (thread.ThreadPriority == 3)
            {
                level4Thread.Enqueue(thread);
            }
            else
            {
                level5Thread.Enqueue(thread);
            }
            amountThreads++; 
        }

        private void SeleteThread()
        {
            for (int i = 1; i > amountThreadHardware; i++)
            {
                if (realTime.Count != 0)
                {
                    runningThread.Add(realTime.Dequeue());
                }
                else if (level1Loop != 2 && level1Thread.Count != 0)
                {
                    level1Loop++;
                    runningThread.Add(level1Thread.Dequeue());
                }
                else if (level2Loop != 2 && level2Thread.Count != 0)
                {
                    level2Loop++;
                    level1Loop = 0;
                    runningThread.Add(level2Thread.Dequeue());
                }
                else if (level3Loop != 2 && level3Thread.Count != 0)
                {
                    level3Loop++;
                    level2Loop = 0;
                    runningThread.Add(level3Thread.Dequeue());
                }
                else if (level4Loop != 2 && level4Thread.Count != 0)
                {
                    level4Loop++;
                    level3Loop = 0;
                    runningThread.Add(level4Thread.Dequeue());
                }
                else if (level5Loop != 2 && level5Thread.Count != 0)
                {
                    level5Loop++;
                    level4Loop = 0;
                    runningThread.Add(level5Thread.Dequeue());
                }
                else
                {
                    level5Loop = 0;
                    SeleteThread();
                }
            }
            Run();
        }

        //To Do: ChangeThread and save thread states and cell stack. 
        //Also need to setup Interrupt to cell this at a given interval.
        private void ChangeThread()
        {
            SeleteThread();
        }

        //To Do: use to start the threads
        private void Run()
        {
            
        }

        //To Do: use to put the thread sleep
        private void Sleep()
        {
        }
        
        //To Do: use to put the thread in the with queue
        private void Wait()
        {
        }

        // To Do: when a thread get a meassage ("e.g. IO") and the thread in the waiting queue or Sleep Thread.
        private void Wait_UP_Thread()
        {
        }

        // To Do: Remove a Thread
        private void DeleteThread(int threadId, int threadPriority)
        {
        }

        public uint GetCoreId
        {
            get
            {
                return coreId;
            }
        }

        public uint GetProcessId
        {
            get
            {
                return processId; 
            }
        }

        public int GetAmountThreads_Hardware
        {
            get
            {
                return amountThreads;
            }
        }

        public int GetAmountThreads
        {
            get
            {
                return amountThreads;
            }
        }

    }
}
