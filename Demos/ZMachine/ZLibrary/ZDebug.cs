using System.Diagnostics;

namespace ZLibrary
{
    public static class ZDebug
    {
        public static bool Enable = false;

#if COSMOSDEBUG
        private static Cosmos.Debug.Kernel.Debugger Debugger = new Cosmos.Debug.Kernel.Debugger("", "");
#else
        //private static StreamWriter writer = new StreamWriter("log.txt");
#endif

        public static void Output(string s)
        {
            if (Enable)
            {
#if COSMOSDEBUG
                Debugger.Send(s);
#else
                //writer.WriteLine(s);
                //Debug.WriteLine(s);
#endif
            }
        }
    }
}
