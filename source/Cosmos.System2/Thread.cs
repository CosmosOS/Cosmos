using static Cosmos.Core.Processing.ProcessContext;
using st = System.Threading;

namespace Cosmos.System
{
    public class Thread
    {
        public uint ThreadID;
        private Context Data;

        public Thread(st.ThreadStart start)
        {
            ThreadID = HAL.Global.SpawnThread(start);
            ThreadFinalSetup();
        }

        public Thread(st.ParameterizedThreadStart start, object param)
        {
            ThreadID = HAL.Global.SpawnThread(start, param);
            ThreadFinalSetup();
        }

        private void ThreadFinalSetup()
        {
            Data = GetContext(ThreadID);
            Data.state = Thread_State.PAUSED;
        }

        public void Start()
        {
            Data.state = Thread_State.ALIVE;
        }

        public void Stop()
        {
            Data.state = Thread_State.PAUSED;
        }

        public static void Sleep(int ms)
        {
            m_CurrentContext.arg = ms;
            m_CurrentContext.state = Thread_State.WAITING_SLEEP;
            while (m_CurrentContext.state == Thread_State.WAITING_SLEEP) { }
        }
    }
}
