//using System;
//using System.Collections.Generic;
//using System.Data.SQLite;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Cosmos.Debug.Common;

//namespace DebugCompiler
//{
//    public static class Program
//    {
//        public static void Main()
//        {
//            var addr = 0x02085690u;
//            Log("Starting");
//            using (var dbgInfo = new DebugInfo(@"c:\Data\Sources\Cosmos\source2\Users\Kudzu\Breakpoints\bin\Debug\Kudzu.Breakpoints.cdb"))
//            {
//                Log("Database loaded");
//                //02085690 l       .text	00000000 SystemVoidKudzuBreakpointsKernelBreakpointsOSTestATA.IL_0321 
//                for (int i = 0; i < 5; i++)
//                {
//                    //var meth = dbgInfo.GetMethod(addr);
//                    //Log("Method: {0}, {1}, {2}", meth.ID, meth.MethodToken, meth.LabelCall);
//                    //var m = dbgInfo.GetMethodLabels(addr);
//                    //Log("Labels retrieved. ({0}): {1}", i, m.Length);
//                    var s = dbgInfo.GetSourceInfos(addr);
//                    Log("Retrieved ({0}): {1} (Old", i, s.Count);
//                }
//            }
//            Log("Database closed");
//        }

//        private static void Log(string message, params object[] args)
//        {
//            Console.Write("{0}: ", DateTime.Now.ToString("HH:mm:ss.ffffff"));
//            Console.WriteLine(message, args);
//        }
//    }
//}