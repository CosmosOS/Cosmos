using IL2CPU.API.Attribs;

namespace Cosmos.Core.Processing
{
    public unsafe class Mutex
    {
        public int gate;

        [PlugMethod(PlugRequired = true)]
        public static void MutexLock(int* mtx) { }

        public void Lock()
        {
            while (gate != 0) { }
            gate = 1;
            /*fixed (int* p = &gate)
            {
                MutexLock(p);
            }*/
        }

        public void Unlock()
        {
            gate = 0;
        }
    }
}
