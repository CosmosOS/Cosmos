//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Cosmos.Build.MSBuild;

//namespace DebugCompiler
//{
//    internal class Program
//    {
//        public static void Main()
//        {
//            var task = new ExtractMapFromElfFile
//            {
//                CosmosBuildDir = @"c:\Data\Sources\Cosmos\Build",
//                DebugInfoFile = @"c:\Data\Sources\Cosmos\source2\Users\Kudzu\Breakpoints\bin\Debug\Kudzu.Breakpoints.cdb",
//                InputFile = @"c:\Data\Sources\Cosmos\source2\Users\Kudzu\Breakpoints\bin\Debug\Kudzu.Breakpoints.bin",
//                WorkingDir = @"c:\Data\Sources\Cosmos\source2\Users\Kudzu\Breakpoints\bin\Debug\"
//            };
//            var sw = Stopwatch.StartNew();
//            task.UseConsoleForLog = true;
//            task.Execute();
//            sw.Stop();
//            Console.WriteLine("Took {0}", sw.Elapsed);
//        }
//    }
//}