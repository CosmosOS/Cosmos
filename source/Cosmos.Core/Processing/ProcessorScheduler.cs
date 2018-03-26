using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Core.Processing
{
    public static unsafe class ProcessorScheduler
    {
        public static void test()
        {
            while (true)
            {
                for (int i = 0; i < 1000; i++)
                {

                }
                Console.WriteLine("Thread 2");
            }
        }

        public static void Initialize()
        {
            var context = new ProcessContext.Context();
            context.type = ProcessContext.Context_Type.PROCESS;
            context.tid = ProcessContext.m_NextCID++;
            context.name = "Boot";
            context.esp = 0;
            context.stacktop = 0;
            context.eip = 0;
            context.cr3 = 0;
            context.state = ProcessContext.Thread_State.ALIVE;
            context.old_state = ProcessContext.Thread_State.ALIVE;
            context.arg = 0;
            context.priority = 0;
            context.age = 0;
            context.parent = 0;
            ProcessContext.m_ContextList[0] = context;
            //ProcessContext.StartContext("Test", test, ProcessContext.Context_Type.PROCESS);
        }

        public static void SwitchTask()
        {
            /*if (ProcessContext.m_ContextList[0] != null)
            {
                ProcessContext.m_ContextList[ProcessContext.m_CurrentContext].esp = INTs.mStackContext;
                ProcessContext.m_CurrentContext++;
                ProcessContext.m_CurrentContext %= (ProcessContext.m_ContextList.Count - 1);
                INTs.mStackContext = ProcessContext.m_ContextList[ProcessContext.m_CurrentContext].esp;
            }*/
        }
    }
}
