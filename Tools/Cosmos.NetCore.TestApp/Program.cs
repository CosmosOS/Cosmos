using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;
using Cosmos.Debug.Symbols;
using Microsoft.DiaSymReader;
using Microsoft.Extensions.DependencyModel;
using AssemblyFile = System.Reflection.Metadata.AssemblyFile;

namespace Cosmos.NetCore.TestApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string xKernelPath = Path.GetFullPath(@"..\..\Demos\Guess\bin\Debug\netstandard1.6");

            var xArgs = new string[]
            {
                "DebugEnabled:False",
                "StackCorruptionDetectionEnabled:False",
                "StackCorruptionDetectionLevel:",
                "DebugMode:Source",
                "TraceAssemblies:Cosmos",
                "DebugCom:1",
                "UseNAsm:True",
                @"OutputFilename:" + Path.Combine(xKernelPath, "GuessKernel.asm"),
                "EnableLogging:True",
                "EmitDebugSymbols:True",
                "IgnoreDebugStubAttribute:False",
                @"References:" + Path.Combine(xKernelPath, "Guess.dll"),
                @"References:" + Path.Combine(xKernelPath, "Cosmos.Core.Plugs.dll"),
                @"References:" + Path.Combine(xKernelPath, "Cosmos.Core.Plugs.Asm.dll"),
                @"References:" + Path.Combine(xKernelPath, "Cosmos.Debug.Kernel.Plugs.Asm.dll"),
                @"References:" + Path.Combine(xKernelPath, "Cosmos.System.Plugs.dll")
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
