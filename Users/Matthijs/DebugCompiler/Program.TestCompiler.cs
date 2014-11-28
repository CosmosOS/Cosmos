using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading;
using Cosmos.Build.MSBuild;
using Microsoft.Build.Framework;
using System.Diagnostics;
using System.Data.SQLite;
using Cosmos.Debug.Common;


namespace DebugCompiler
{
    internal class Program
    {
        //public const string CosmosRoot = @"e:\Cosmos";
        public const string CosmosRoot = @"e:\OpenSOurce\Cosmos";
        //public const string CosmosRoot = @"C:\Users\Huge\Documents\Visual Studio 2010\Projects\IL2CPU";

        private const string KernelFile = CosmosRoot + @"\Users\Kudzu\Breakpoints\bin\Debug\Playground.Kudzu.BreakpointsKernel.dll";
        private const string OutputFile = CosmosRoot + @"\Users\Kudzu\Breakpoints\bin\Debug\Kudzu.Breakpoints.asm";
        //private const string KernelFile = CosmosRoot + @"\source2\Users\Matthijs\Playground\bin\Debug\Playground.dll";
        //private const string OutputFile = CosmosRoot + @"\source2\Users\Matthijs\Playground\bin\Debug\Playground.asm";

        private static void Main(string[] args)
        {
            Console.SetOut(new StreamWriter("out", false));

            var xSW = Stopwatch.StartNew();
            try
            {
                var xTask = new IL2CPUTask();
                xTask.DebugEnabled = true;
                xTask.StackCorruptionDetectionEnabled = false;
                xTask.DebugMode = "Source";
                xTask.TraceAssemblies = "User";
                xTask.DebugCom = 1;
                xTask.UseNAsm = true;
                xTask.OutputFilename = OutputFile;
                xTask.EnableLogging = true;
                xTask.EmitDebugSymbols = true;
                xTask.IgnoreDebugStubAttribute = false;
                xTask.References = GetReferences();
                xTask.OnLogError = (m) => Console.WriteLine("Error: {0}", m);
                xTask.OnLogWarning = (m) => Console.WriteLine("Warning: {0}", m);
                xTask.OnLogMessage = (m) => Console.WriteLine("Message: {0}", m);
                xTask.OnLogException = (m) =>
                {
                    Console.WriteLine("Exception: {0}", m.ToString());
                    //Console.ReadLine();
                };
                if (xTask.Execute())
                {
                    Console.WriteLine("Executed OK");
                }
                else
                {
                    Console.WriteLine("Errorred");
                }
                xSW.Stop();

            }
            catch (Exception E)
            {
                Console.Out.Flush();
                Console.WriteLine(E.ToString());
                //Console.ReadLine();
                return;
            }
            Console.WriteLine("Run took {0}", xSW.Elapsed);
            Console.WriteLine("Generated {0} Guids", DebugInfo.mLastGuid);
            Console.Out.Flush();
        }

        //static void Main()
        //{
        //    var di = new DebugInfo(Path.Combine(CosmosRoot, @"source2\Demos\Guess\bin\Debug\Guess.cdb"));
        //    di.LoadLookups();
        //    var addr = (uint)1;
        //    var q = new SQLinq<Label>()
        //      .Where(i => i.Address <= addr)
        //      .OrderByDescending(i => i.Address)
        //      ;
        //    Console.WriteLine(q.ToSQL().ToQuery());
        //    var xLabels = di.Connection.Query<Label>(q).Select(i => i.Name).ToArray();
        //    var g = Guid.Empty;
        //    di.GetMethod(1);
        //}

        private static void SQLiteLog_Log(object sender, LogEventArgs e)
        {
            Console.WriteLine("SQL: {0}", e.Message);
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
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            public int MetadataCount
            {
                get
                {
                    return MetadataNames.Count;
                }
            }

            public System.Collections.ICollection MetadataNames
            {
                get
                {
                    return new String[] { "FullPath" };
                }
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