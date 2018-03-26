using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Core.Processing
{
    public static unsafe class ProcessContext
    {
        public enum Thread_State
        {
            ALIVE = 0,
            DEAD = 1,
            WAITING_SLEEP = 2,
            WAITING_SEMAPHORE = 3,
            PAUSED = 4
        }

        public enum Context_Type
        {
            THREAD = 0,
            PROCESS = 1
        }

        public class Context
        {
            public Context_Type type;
            public uint tid;
            public string name;
            public uint esp;
            public uint stacktop;
            public uint eip;
            public uint cr3;
            public Thread_State state;
            public Thread_State old_state;
            public uint arg;
	        public uint priority;
            public uint age;
            public uint parent;
        }

        public const uint STACK_SIZE = 4096;
        public static uint m_NextCID;
        public static int m_CurrentContext;
        public static List<Context> m_ContextList = new List<Context>(256);

        public static Context GetContext()
        {
            return m_ContextList[m_CurrentContext];
        }

        public static Context GetContext(int tid)
        {
            for(int i = 0; i < m_ContextList.Count; i++)
            {
                if(m_ContextList[i].tid == tid)
                {
                    return m_ContextList[i];
                }
            }
            return null;
        }

        public static uint StartContext(string name, System.Threading.ThreadStart entry, Context_Type type, params object[] args)
        {
            uint address = ObjUtilities.GetPointer(entry);
            Context context = new Context();
            context.type = type;
            context.tid = m_NextCID++;
            context.name = name;
            uint[] tmp = new uint[STACK_SIZE / 4];
            fixed (uint* p = tmp)
            {
                context.esp = (uint)p;
            }
            context.stacktop = context.esp;
            uint* stack = (uint*)(context.esp + 4000);
            *--stack = 0xFFFFFFFF; // trash
            *--stack = 0xFFFFFFFF; // trash
            *--stack = 0xFFFFFFFF; // trash
            *--stack = 0xFFFFFFFF; // trash
            /*for(int i = 0; i < 512 / 4; i++)
            {
                *--stack = 0; // MMX
            }*/ // nope, just not going to bother today
            for(int i = args.Length - 1; i >= 0; i++) // will push arguments when we patch in the object utils
            {
                *--stack = 0; ObjUtilities.GetPointer(args[i]);
            }
            *--stack = 0x10; // ss ?
            *--stack = 0x00000202; // eflags
            *--stack = 0x8; // cs
            *--stack = address; // eip
            *--stack = 0; // error
            *--stack = 0; // int
            *--stack = 0; // eax
            *--stack = 0; // ebx
            *--stack = 0; // ecx
            *--stack = *stack; // offset
            *--stack = 0; // edx
            *--stack = 0; // esi
            *--stack = 0; // edi
            *--stack = context.esp + 4000; //ebp
            context.esp = (uint)stack;
            context.eip = address;
            context.state = Thread_State.ALIVE;
            if (type == Context_Type.PROCESS)
            {
                context.parent = 0;
            }
            else
            {
                Context parent = GetContext();
                context.parent = parent.tid;
            }
            m_ContextList.Add(context);
            return context.tid;
        }
    }
}
