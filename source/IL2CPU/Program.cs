using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Build.Common;
using Cosmos.Debug.Common;
using Cosmos.IL2CPU;

namespace IL2CPU
{
  internal class Program
  {

    public const string CosmosRoot = "";//@"e:\OpenSource\Cosmos";
    private const string KernelFile = CosmosRoot + "";// @"\Users\Sentinel209\SentinelKernel\bin\Debug\SentinelKernel.dll";
    private const string OutputFile = CosmosRoot + "";//@"\Users\Sentinel209\SentinelKernel\bin\Debug\SentinelKernelBoot.asm";
    private static  Dictionary<string, string> CmdOptions = new Dictionary<string, string>();
    private static List<string> References = new List<string>();

    private static int Main(string[] args)
    {
      try {
        var tmp = "";
        foreach (var s in args)
        {

          tmp += s;
          string[] s1 = s.Split(':');
          string argID = s1[0].ToLower();
          if (argID != "References".ToLower())
          {
            CmdOptions.Add(argID, s.Replace(s1[0] + ":",""));
          }
          else
          {

            References.Add(s.Replace(s1[0] + ":",""));
          }

        }
        //File.WriteAllText("C:\\Users\\Emile\\Desktop\\dump.txt",tmp);

        var xTask = new CompilerEngine();
        xTask.DebugEnabled = Convert.ToBoolean(CmdOptions["DebugEnabled".ToLower()]);
        Console.WriteLine("Loaded : DebugEnabled");
        xTask.StackCorruptionDetectionEnabled = Convert.ToBoolean(CmdOptions["StackCorruptionDetectionEnabled".ToLower()]);
        Console.WriteLine("Loaded : StackCorruptionDetectionEnabled");
        xTask.DebugMode = CmdOptions["DebugMode".ToLower()];
        Console.WriteLine("Loaded : DebugMode");
        xTask.TraceAssemblies = null;
        Console.WriteLine("Loaded : TraceAssemblies");
        xTask.DebugCom = Convert.ToByte(CmdOptions["DebugCom".ToLower()]);
        Console.WriteLine("Loaded : DebugCom");
        xTask.UseNAsm = Convert.ToBoolean(CmdOptions["UseNAsm".ToLower()]);
        Console.WriteLine("Loaded : UseNAsm");
        xTask.OutputFilename = CmdOptions["OutputFilename".ToLower()]; ;
        Console.WriteLine("Loaded : OutputFilename");
        xTask.EnableLogging = Convert.ToBoolean(CmdOptions["EnableLogging".ToLower()]); ;
        Console.WriteLine("Loaded : EnableLogging");
        xTask.EmitDebugSymbols = Convert.ToBoolean(CmdOptions["EmitDebugSymbols".ToLower()]); ;
        Console.WriteLine("Loaded : EmitDebugSymbols");
        xTask.IgnoreDebugStubAttribute = Convert.ToBoolean(CmdOptions["IgnoreDebugStubAttribute".ToLower()]);
        Console.WriteLine("Loaded : IgnoreDebugStubAttribute");
        xTask.References = References.ToArray();
        Console.WriteLine("Loaded : References");

        xTask.OnLogError = (m) => Console.Error.WriteLine("Error: {0}", m);
        xTask.OnLogWarning = (m) => Console.WriteLine("Warning: {0}", m);
        xTask.OnLogMessage = (m) =>
        {
          Console.WriteLine("Message: {0}", m);
        };
        xTask.OnLogException = (m) => Console.Error.WriteLine("Exception: {0}", m.ToString());
        xTask.AssemblerLog = "Cosmos.Assembler.log";
        if (xTask.Execute())
        {
          Console.WriteLine("Executed OK");
//          File.WriteAllText(@"e:\compiler.log", "OK");
          return 0;
        }
        else
        {
          Console.WriteLine("Errorred");
//          File.WriteAllText(@"e:\compiler.log", "Errored");
          return 2;
        }
      }
      catch (Exception E)
      {
       // Console.Out.Flush();
      // File.WriteAllText("./ErrorDump.txt",E.ToString()  + " " + E.Source);
        Console.WriteLine("Error occurred: " + E.ToString());
//        File.WriteAllText(@"e:\compiler.log", "Exception: " + E.ToString());
        return 1;
      }
    }
  }
}
