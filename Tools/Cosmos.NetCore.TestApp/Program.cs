using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyModel;

namespace Cosmos.NetCore.TestApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //var xType = Type.GetType("System.Object");

            //var ctx = DependencyContext.Default;

            //Console.WriteLine("--- Native libraries ---");
            //foreach (var r in ctx.GetDefaultNativeAssets())
            //{
            //    Console.WriteLine($"    {r}");


            //}
            //Console.WriteLine();


            //Console.ReadKey();

            var xArgs = new string[]
            {
                "DebugEnabled:False",
                "StackCorruptionDetectionEnabled:False",
                "StackCorruptionDetectionLevel:",
                "DebugMode:Source",
                "TraceAssemblies:Cosmos",
                "DebugCom:1",
                "UseNAsm:True",
                @"OutputFilename:path\to\GuessKernel.asm",
                "EnableLogging:True",
                "EmitDebugSymbols:True",
                "IgnoreDebugStubAttribute:False",
                @"References:path\to\GuessKernel.dll",
                @"References:path\to\Cosmos.Core.Plugs.dll",
                @"References:path\to\Cosmos.Debug.Kernel.Plugs.dll",
                @"References:path\to\Cosmos.System.Plugs.dll"
            };

            global::IL2CPU.Program.Run(xArgs, LogMessage, LogError);

            Console.ReadLine();
        }

        public static void LogMessage(string aMessage)
        {
            Console.WriteLine("Message: " + aMessage);
        }

        public static void LogError(string aError)
        {
            Console.WriteLine("Error: " + aError);
        }
    }
}
