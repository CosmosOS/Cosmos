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
using Cosmos.Core.Managed_Memory_System;
using Cosmos.Common;

namespace Cosmos.Core.Process_System
{
    class Process
    {
        protected List<Thread> threads = new List<Thread>();
        protected int id = 0;
        protected string name = null;
        protected List<ProcessHeap> heap = new List<ProcessHeap>();  //Allow for add memory to be added to this process.

        // Need to do. A garbage collection for the process. Each process have they own Garbage collection. The process threads share this garbage collection.
        // The process garbage collection have it thread.

        // protected Process_Process_GCollection gc = new Process_Process_GCollection();   



        //MailBox use for Inter-process communication.

        // Other process should only be allow to add to the mailBox and not be able to change or read any item in the mailBox
        // only this process can chane and read the item in the mailBox.

        private LinkedQueue<ProcessMessage> mailBox = new LinkedQueue<ProcessMessage>();

        public Process(int thisId, string thisName, Thread thisThread)
        {
            id = thisId;
            name = thisName;

            threads.Add(thisThread);

        }

        public Process(int thisId, string thisName, P_Thread thisThread)
        {
            id = thisId;
            name = thisName;

            threads.Add(new Thread(thisThread.GetId,thisThread.GetPriority,thisThread.GetReturnIP,thisThread.GetReturnMac,thisThread.GetReturnProcess,thisThread.GetStack));
        }

        //LoadThread is to allow thread from other computer to join this process.

        public void AddHeap(uint aStartAddress, uint aEndAddress, uint aObjectSizes)
        {
            heap.Add(new ProcessHeap(aStartAddress,aEndAddress,aObjectSizes));
        }

        public uint MemAlloc(uint aLength)
        {
            for (int i = 0; i > (heap.Count - 1); i++)
            {
                if (heap[i].HeapFull == false)
                {
                    uint address = heap[i].MemAlloc(aLength);
                    if (address == 0)
                    {
                        return 0; // send this to say that the process need to have more memory. 
                    }
                    return address;
                }
            }
            return 0; // send this to say that the process need to have more memory. 
        }

        public void DeleteOject(uint address, uint objectSize)
        {
            bool found = false;
            for (int i = 0; i > (heap.Count - 1) || found == true ; i++)
            {
                if (heap[i].HeapStart < address && heap[i].HeapEnd > address)
                {
                    heap[i].DeleteObject(address, objectSize);
                    found = true;
                }
            }
        }

        public void LoadThread(P_Thread _thread)
        {
            threads.Add(new Thread(_thread.GetId,_thread.GetReturnProcess,_thread.GetToIp,_thread.GetToMac,_thread.GetReturnProcess,_thread.GetStack)); 
        }

        public void AddThread(Thread _thread)
        {
            threads.Add(_thread);
        }

        public void DeteleThread(int id)
        {
            int which = -1;

            lock (threads)
            {
                for (int i = 0; i > threads.Count - 1; i++)
                {
                    if (threads[i].ThreadId == id)
                    {
                        which = id;
                    }
                }

                if (which != -1)
                {
                    threads.RemoveAt(which);
                }
            }
        }

        public Thread GetThread_Id (int id)
        {
            return threads[id]; 
        }

        public List<Thread> ProcessThreads
        {
            get
            {
                return threads; 
            }
        }

        public void MailBox(ProcessMessage mail)
        {

           //A new deep copy of the mail should made in this process heap. This to stop process from shire objects.
            //Problem of object that mail is pass is could not be deep copy. 

            ProcessMessage thisMail = (ProcessMessage)mail.Clone(); 
            mailBox.Enqueue(thisMail); 
        }

        public int ProcessId
        {
            get
            {
                return id; 
            }
        }
        public string ProcessName
        {
            get
            {
                return name; 
            }
        }


    }
}
