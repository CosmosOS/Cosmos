using st = System.Threading;

namespace Cosmos.System
{
    public class Thread
    {
        public uint ThreadID;
        private Cosmos.Core.Processing.ProcessContext.Context Data;

        public Thread(st.ThreadStart start)
        {
            ThreadID = Cosmos.HAL.Global.SpawnThread(start);
            ThreadFinalSetup();
        }

        public Thread(st.ParameterizedThreadStart start, object param)
        {
            ThreadID = Cosmos.HAL.Global.SpawnThread(start, param);
            ThreadFinalSetup();
        }

        private void ThreadFinalSetup()
        {
            Data = Cosmos.Core.Processing.ProcessContext.GetContext(ThreadID);
            Data.state = Core.Processing.ProcessContext.Thread_State.PAUSED;
        }

        public void Start()
        {
            Data.state = Core.Processing.ProcessContext.Thread_State.ALIVE;
        }

        public void Stop()
        {
            Data.state = Core.Processing.ProcessContext.Thread_State.PAUSED;
        }

        public static void Sleep(int ms)
        {
            Cosmos.Core.Processing.ProcessContext.m_CurrentContext.arg = ms;
            Cosmos.Core.Processing.ProcessContext.m_CurrentContext.state = Core.Processing.ProcessContext.Thread_State.WAITING_SLEEP;
            while (Cosmos.Core.Processing.ProcessContext.m_CurrentContext.state == Core.Processing.ProcessContext.Thread_State.WAITING_SLEEP) { }
        }
    }
}
