using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Build.MSBuild;
using Cosmos.Debug.Common;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace IL2CPU
{
  internal class Program
  {

    public const string CosmosRoot = "";//@"e:\OpenSource\Cosmos";
    private const string KernelFile = CosmosRoot + "";// @"\Users\Sentinel209\SentinelKernel\bin\Debug\SentinelKernel.dll";
    private const string OutputFile = CosmosRoot + "";//@"\Users\Sentinel209\SentinelKernel\bin\Debug\SentinelKernelBoot.asm";
    private static  Dictionary<string, string> CmdOptions = new Dictionary<string, string>();
    private static List<ITaskItem>  References = new List<ITaskItem>();

    private static void Main(string[] args)
    {


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
          
          References.Add(new TaskItemImpl(s.Replace(s1[0] + ":","")));
        }

      }
      File.WriteAllText("C:\\Users\\Emile\\Desktop\\dump.txt",tmp);
      try
      {
        var xTask = new IL2CPUTask();
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


        xTask.OnLogError = (m) => Console.WriteLine("Error: {0}", m);
        xTask.OnLogWarning = (m) => Console.WriteLine("Warning: {0}", m);
        xTask.OnLogMessage = (m) =>
        {
          Console.WriteLine("Message: {0}", m);
        };
        xTask.OnLogException = (m) => Console.WriteLine("Exception: {0}", m.ToString());

        if (xTask.Execute())
        {
          Console.WriteLine("Executed OK");
        }
        else
        {
          Console.WriteLine("Errorred");
        }


      }
      catch (Exception E)
      {
       // Console.Out.Flush();
      // File.WriteAllText("./ErrorDump.txt",E.ToString()  + " " + E.Source);

        return;
      }

      //Console.Out.Flush();
     // File.WriteAllText("./ErrorDump.txt", Console.Out.ToString());
    //  Console.ReadKey();
    }

    private static ITaskItem[] GetReferences()
    {
      return new ITaskItem[]
      {
        new TaskItemImpl(KernelFile),
        new TaskItemImpl(CosmosRoot + @"\source\Cosmos.Core.Plugs\bin\x86\Debug\Cosmos.Core.Plugs.dll"),
        new TaskItemImpl(CosmosRoot + @"\source\Cosmos.Debug.Kernel.Plugs\bin\x86\Debug\Cosmos.Debug.Kernel.Plugs.dll"),
        new TaskItemImpl(CosmosRoot + @"\source\Cosmos.HAL\bin\x86\Debug\Cosmos.HAL.dll"),
        new TaskItemImpl(CosmosRoot + @"\source\Cosmos.System.Plugs\bin\x86\Debug\Cosmos.System.Plugs.dll"),
        new TaskItemImpl(CosmosRoot + @"\Users\Sentinel209\SentinelSystemLib\bin\Debug\SentinelSystemLib.dll"),
      };
    }

    private class TaskItemImpl : ITaskItem
    {
      private string path;

      public TaskItemImpl(string path)
      {
        this.path = path;
      }

      public System.Collections.IDictionary CloneCustomMetadata()
      {
        throw new NotImplementedException();
      }

      public void CopyMetadataTo(ITaskItem destinationItem)
      {
        throw new NotImplementedException();
      }

      public string GetMetadata(string metadataName)
      {
        if (metadataName == "FullPath")
        {
          return path;
        }
        throw new NotImplementedException();
      }

      public string ItemSpec
      {
        get { throw new NotImplementedException(); }
        set { throw new NotImplementedException(); }
      }

      public int MetadataCount
      {
        get { return MetadataNames.Count; }
      }

      public System.Collections.ICollection MetadataNames
      {
        get { return new String[] {"FullPath"}; }
      }

      public void RemoveMetadata(string metadataName)
      {
        throw new NotImplementedException();
      }

      public void SetMetadata(string metadataName, string metadataValue)
      {
        throw new NotImplementedException();
      }
    }
  }
}
