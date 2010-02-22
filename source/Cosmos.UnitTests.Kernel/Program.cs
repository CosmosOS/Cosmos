using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Cosmos.Compiler.Builder;
using Cosmos.Build.Common;

using Cosmos.Hardware;
using Cosmos.Kernel;

using Cosmos.UnitTests;

namespace Cosmos.UnitTests.Kernel
{
    class Program
    {
        #region Cosmos Builder logic
        // Most users wont touch this. This will call the Cosmos Build tool and launch QEMU
        [STAThread]
        static void Main(string[] args)
        {
            var xBuilder = new Builder();

            Console.WriteLine("BuildPath = '{0}'", xBuilder.BuildPath);
            xBuilder.TargetAssembly = typeof(Cosmos.UnitTests.Kernel.Program).Assembly;
            var xEvent = new AutoResetEvent(false);
            xBuilder.BuildCompleted += delegate { xEvent.Set(); };
            xBuilder.LogMessage += delegate(LogSeverityEnum aSeverity, string aMessage)
            {
              Console.WriteLine("Log: {0} - {1}", aSeverity, aMessage);
            };

            var options = Cosmos.Compiler.Builder.BuildOptions.Load();

            options.DebugMode = DebugMode.None;
            options.DebugPortId = 0;
            options.UseGDB = false;

            options.CompileIL = true; 
            options.UseInternalAssembler = false; // force externel assemble and link
                        
            options.Target = "ISO"; 

            xBuilder.BeginCompile(options);

            //  xBuilder.BeginCompile(options);
            xEvent.WaitOne();

            new MakeISOStep(options).Execute();

            //From v0.9.1 Qemu requires forward slashes in path
            String xBuildPath = xBuilder.BuildPath.Replace('\\', '/');
            AutoResetEvent xTestEvent = new AutoResetEvent(false);

            var xProcess = Cosmos.Compiler.Builder.Global.Call(xBuilder.ToolsPath + @"qemu\qemu.exe",
                " -L ."
                // CD ROM image
                + " -cdrom \"" + xBuilder.BuildPath.Replace('\\', '/') + "Cosmos.iso\""
                // Boot CD ROM
                + " -boot d"               
                , xBuilder.ToolsPath + @"qemu", false, true);

            System.Threading.Thread.Sleep(500);  //give it time to launch

        }
        #endregion

        // Main entry point of the kernel
        public static void Init()
        {
            var xBoot = new Cosmos.Sys.Boot();
            xBoot.Execute();

            //Console.WriteLine("Edit Program.cs to create your own Operating System.");

            RunTests();

            Console.WriteLine("Press any key to shutdown...");            
            Console.Read();

            Cosmos.Sys.Deboot.ShutDown();

        }

        public static void RunTests()
        {

            int tTotal = 0;
            int tPass = 0;
            string xMessage;

            var xVar = new Cosmos.UnitTests.SimpleAddFunction();
            tPass += xVar.Test();
            tTotal += 1;
            xVar = null;

            xMessage = string.Concat(tPass, " test(s) passed, ");
            xMessage = string.Concat(xMessage, tTotal);
            xMessage = string.Concat(xMessage, " test(s) run.");

            Console.WriteLine(xMessage);
            Console.WriteLine();
            
        }
    }
}
