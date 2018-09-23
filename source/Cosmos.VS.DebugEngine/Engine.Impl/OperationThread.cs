using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Cosmos.VS.DebugEngine.Engine.Impl
{
    public delegate void Operation();

    // This object represents the debugger poll thread to the managed portion of the engine. It allows the engine to perform 
    // operations on the poll thread. This is required because the Win32 debugging API requires thread affinity for several operations.
    public class WorkerThread : IDisposable
    {
        readonly ManualResetEvent m_opSet;
        readonly ManualResetEvent m_opComplete;
        readonly Operation m_quitOperation;

        Operation m_op;
        bool m_fSyncOp;
        Exception m_opException;
        //DebuggedProcess m_debuggedProcess;
        
        public WorkerThread()
        {
            m_opSet = new ManualResetEvent(false);
            m_opComplete = new ManualResetEvent(true);
            m_quitOperation = new Operation(delegate() { });
            
            var thread = new Thread(new ThreadStart(ThreadFunc));
            thread.Start();
        }

        //public void SetDebugProcess(DebuggedProcess debuggedProcess)
        //{
        //    System.Diagnostics.System.Diagnostics.Debug.Assert(m_debuggedProcess == null);
        //    m_debuggedProcess = debuggedProcess;
        //}

        public void RunOperation(Operation op)
        {
            if (op == null)
            {
                throw new ArgumentNullException();
            }

            SetOperationInternal(op, true);
        }

        [SuppressMessage("AsyncUsage.CSharp.Naming", "AvoidAsyncSuffix:Avoid Async suffix", Scope = "member")]
        public void RunOperationAsync(Operation op)
        {
            if (op == null)
            {
                throw new ArgumentNullException();
            }

            SetOperationInternal(op, false);
        }

        public void Close()
        {
            RunOperationAsync(m_quitOperation);
        }

        internal void SetOperationInternal(Operation op, bool fSyncOp)
        {
            while (true)
            {
                m_opComplete.WaitOne();

                if (TrySetOperationInternal(op, fSyncOp))
                {
                    return;
                }
            }
        }

        bool TrySetOperationInternal(Operation op, bool fSyncOp)
        {
            lock(this)
            {
                if (m_op == null)
                {
                    m_op = op;
                    m_fSyncOp = fSyncOp;
                    m_opException = null;
                    m_opComplete.Reset();
                    m_opSet.Set();

                    if (fSyncOp)
                    {
                        m_opComplete.WaitOne();
                        if (m_opException != null)
                        {
                            throw m_opException;
                        }
                    }

                    return true;
                }
                else if (m_op == m_quitOperation)
                {
                    if (op == m_quitOperation)
                    {
                        return true; // we are already closed
                    }
                    else
                    {
                        // Can't try to run something after calling Close
                        throw new InvalidOperationException();
                    }
                }
            }

            return false;
        }          
        
        // Thread routine for the poll loop. It handles calls coming in from the debug engine as well as polling for debug events.
        private void ThreadFunc()
        {
            bool fQuit = false;

            while (!fQuit)
            {
                //if ((m_debuggedProcess != null) && (m_debuggedProcess.IsPumpingDebugEvents))
                {
                    //m_debuggedProcess.WaitForAndDispatchDebugEvent(ResumeEventPumpFlags.ResumeWithExceptionHandled);
                }

                // If the other thread is dispatching a command, execute it now.
                bool fReceivedCommand = m_opSet.WaitOne(new TimeSpan(0, 0, 0, 0, 100), false);

                if (fReceivedCommand)
                {
                    if (m_fSyncOp)
                    {
                        try
                        {
                            m_op();
                        }
                        catch (Exception opException)
                        {
                            m_opException = opException;
                        }
                    }
                    else
                    {
                        m_op();
                    }

                    fQuit = (m_op == m_quitOperation);
                    if (!fQuit)
                    {
                        m_op = null;
                    }

                    m_opComplete.Set();
                    m_opSet.Reset();
                }
            }
        }

        void IDisposable.Dispose()
        {
            Close();
        }
    }
}
