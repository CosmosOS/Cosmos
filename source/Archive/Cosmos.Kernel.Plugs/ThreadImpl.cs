using System;
using System.Collections.Generic;
using Cosmos.IL2CPU.Plugs;
using System.Threading;

namespace Cosmos.Kernel.Plugs {
    [Plug(Target=typeof(System.Threading.Thread))]
    public static class ThreadImpl {
        private static bool running = true;

        public static IntPtr InternalGetCurrentThread() {
            return IntPtr.Zero;

        }
        public static void Sleep(int millisecondsTimeout)
        {
            // Cosmos.HAL.Global.Sleep((uint) millisecondsTimeout);
            Kernel.CPU.Halt();
        }
        //public static void Start()
        //{
        //    if (SetMethod == null)
        //        throw new Exception("SetMethod can't equal null");

        //    do
        //    {
        //       SetMethood();
        //    } while (running);
        //}

        public delegate void SetMethod();

        //public static void Stop()
        //{
        //    running = false;
        //}
    }
}