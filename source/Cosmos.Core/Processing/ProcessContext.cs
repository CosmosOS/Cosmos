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
            PAUSED = 3
        }

        public enum Context_Type
        {
            THREAD = 0,
            PROCESS = 1
        }

        public class Context
        {
            public Context next;
            public Context_Type type;
            public uint tid;
            public string name;
            public uint esp;
            public uint stacktop;
            public System.Threading.ThreadStart entry;
            public System.Threading.ParameterizedThreadStart paramentry;
            public Thread_State state;
            public object param;
            public int arg;
            public uint priority;
            public uint age;
            public uint parent;
        }

        public const uint STACK_SIZE = 4096;
        public static uint m_NextCID;
        public static Context m_CurrentContext;
        public static Context m_ContextList;

        public static Context GetContext(uint tid)
        {
            /*for(int i = 0; i < m_ContextList.Count; i++)
            {
                if(m_ContextList[i].tid == tid)
                {
                    return m_ContextList[i];
                }
            }*/
            Context ctx = m_ContextList;
            while (ctx.next != null)
            {
                if (ctx.tid == tid)
                {
                    return ctx;
                }
                ctx = ctx.next;
            }
            if (ctx.tid == tid)
            {
                return ctx;
            }
            return null;
        }

        public static uint* SetupStack(uint* stack)
        {
            uint origin = (uint)stack;
            *--stack = 0xFFFFFFFF; // trash
            *--stack = 0xFFFFFFFF; // trash
            *--stack = 0xFFFFFFFF; // trash
            *--stack = 0xFFFFFFFF; // trash
            *--stack = 0x10; // ss ?
            *--stack = 0x00000202; // eflags
            *--stack = 0x8; // cs
            *--stack = ObjUtilities.GetEntryPoint(); // eip
            *--stack = 0; // error
            *--stack = 0; // int
            *--stack = 0; // eax
            *--stack = 0; // ebx
            *--stack = 0; // ecx
            *--stack = 0; // offset
            *--stack = 0; // edx
            *--stack = 0; // esi
            *--stack = 0; // edi
            *--stack = origin; //ebp
            *--stack = 0x10; // ds
            *--stack = 0x10; // fs
            *--stack = 0x10; // es
            *--stack = 0x10; // gs
            return stack;
        }

        public static uint StartContext(string name, System.Threading.ThreadStart entry, Context_Type type)
        {
            Context context = new Context();
            context.type = type;
            context.tid = m_NextCID++;
            context.name = name;
            context.stacktop = GCImplementation.AllocNewObject(4096);
            context.esp = (uint)SetupStack((uint*)(context.stacktop + 4000));
            context.state = Thread_State.PAUSED;
            context.entry = entry;
            if (type == Context_Type.PROCESS)
            {
                context.parent = 0;
            }
            else
            {
                context.parent = m_CurrentContext.tid;
            }
            Context ctx = m_ContextList;
            while (ctx.next != null)
            {
                ctx = ctx.next;
            }
            ctx.next = context;
            return context.tid;
        }

        public static uint StartContext(string name, System.Threading.ParameterizedThreadStart entry, Context_Type type, object param)
        {
            Context context = new Context();
            context.type = type;
            context.tid = m_NextCID++;
            context.name = name;
            context.stacktop = GCImplementation.AllocNewObject(4096);
            context.esp = (uint)SetupStack((uint*)(context.stacktop + 4000));
            context.state = Thread_State.ALIVE;
            context.paramentry = entry;
            context.param = param;
            if (type == Context_Type.PROCESS)
            {
                context.parent = 0;
            }
            else
            {
                context.parent = m_CurrentContext.tid;
            }
            Context ctx = m_ContextList;
            while (ctx.next != null)
            {
                ctx = ctx.next;
            }
            ctx.next = context;
            return context.tid;
        }
    }
}
