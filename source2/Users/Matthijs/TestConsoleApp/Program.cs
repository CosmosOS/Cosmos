using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Execution;
using System.IO;

namespace TestConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var xTask = new Cosmos.Build.MSBuild.IL2CPUTask()
            {
                DebugCom = 1,
                DebugMode = "Source",
                EmitDebugSymbols = true,
                OutputFilename = Path.GetFullPath( "output.asm"),
                References = new ITaskItem[]{
                    new ReferenceImpl(typeof(MatthijsPlayground.Kernel).Assembly.Location),
                    new ReferenceImpl(typeof(Cosmos.Core.Plugs.CPUImpl).Assembly.Location),
                    new ReferenceImpl(typeof(Cosmos.Debug.Kernel.Plugs.DebugBreak).Assembly.Location),
                    new ReferenceImpl(typeof(Cosmos.System.Plugs.System.ConsoleImpl).Assembly.Location)
                },
                UseNAsm=true                
            };
            xTask.OnLogError = m => Console.WriteLine("Error: {0}", m);
            xTask.OnLogException = ex => Console.WriteLine("Exception: {0}", ex);
            xTask.OnLogMessage = m => Console.WriteLine("Message: {0}", m);
            xTask.Execute();
        }
    }
}
