﻿using System;
using System.Collections.Generic;
using System.Text;
using IL2CPU.API.Attribs;

namespace Cosmos.Core.Processing
{
    public static unsafe class ProcessorScheduler
    {
        public static void Initialize()
        {
            var context = new ProcessContext.Context();
            context.type = ProcessContext.Context_Type.PROCESS;
            context.tid = ProcessContext.m_NextCID++;
            context.name = "Boot";
            context.esp = 0;
            context.stacktop = 0;
            context.state = ProcessContext.Thread_State.ALIVE;
            context.arg = 0;
            context.priority = 0;
            context.age = 0;
            context.parent = 0;
            ProcessContext.m_ContextList = context;
            ProcessContext.m_CurrentContext = context;
        }

        public static void EntryPoint()
        {
            ProcessContext.m_CurrentContext.entry?.Invoke();
            ProcessContext.m_CurrentContext.paramentry?.Invoke(ProcessContext.m_CurrentContext.param);
            ProcessContext.m_CurrentContext.state = ProcessContext.Thread_State.DEAD;
            while (true) { } // remove from thread pool later
        }

        public static void SwitchTask()
        {
            if (ProcessContext.m_CurrentContext != null)
            {
                CPU.DisableInterrupts();

                ProcessContext.Context ctx = ProcessContext.m_ContextList;
                ProcessContext.Context last = ctx;
                while (ctx != null)
                {
                    if (ctx.state == ProcessContext.Thread_State.DEAD)
                    {
                        last.next = ctx.next;
                        break;
                    }
                    last = ctx;
                    ctx = ctx.next;
                }
                ctx = ProcessContext.m_ContextList;
                while (ctx != null)
                {
                    if (ctx.state == ProcessContext.Thread_State.WAITING_SLEEP)
                    {
                        ctx.arg -= 10; //Since Local APIC Frequency = 100Hz remove 10ms per interrupt
                        if (ctx.arg <= 0)
                        {
                            ctx.state = ProcessContext.Thread_State.ALIVE;
                        }
                    }
                    ctx.age++;
                    ctx = ctx.next;
                }
                ProcessContext.m_CurrentContext.esp = INTs.mStackContext;
            tryagain:
                if (ProcessContext.m_CurrentContext.next != null)
                {
                    ProcessContext.m_CurrentContext = ProcessContext.m_CurrentContext.next;
                }
                else
                {
                    ProcessContext.m_CurrentContext = ProcessContext.m_ContextList;
                }
                if (ProcessContext.m_CurrentContext.state != ProcessContext.Thread_State.ALIVE)
                {
                    goto tryagain;
                }
                ProcessContext.m_CurrentContext.age = ProcessContext.m_CurrentContext.priority;
                INTs.mStackContext = ProcessContext.m_CurrentContext.esp;

                CPU.EnableInterrupts();
            }

            LocalAPIC.EndOfInterrupt();
        }
    }
}