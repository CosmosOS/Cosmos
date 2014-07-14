//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Cosmos.Debug.Common;

//namespace DebugCompiler
//{
//    public class Program
//    {
//        static void Main()
//        {
//            using (var dbgInfo = new DebugInfo(@"c:\Data\Sources\Cosmos\source2\Users\Kudzu\Breakpoints\bin\Debug\Kudzu.Breakpoints.cdb"))
//            {
//                var infos = dbgInfo.GetSourceInfos(0x02070D19);
//                Console.WriteLine("Done. Retrieved {0} items", infos.Count);
//            }
//        }
//    }
//}