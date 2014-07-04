using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Build.MSBuild;
using Microsoft.Build.Framework;
using System.Diagnostics;
using System.Data.SQLite;
using Cosmos.Debug.Common;
using System.IO;
using Dapper;
using SQLinq;
using DapperExtensions;
using SQLinq.Dapper;
using SQLinq;

namespace DebugCompiler
{
    class Program
    {
        //public const string CosmosRoot = @"e:\Cosmos";
        public const string CosmosRoot = @"c:\data\sources\cosmos";
        //public const string CosmosRoot = @"C:\Users\Huge\Documents\Visual Studio 2010\Projects\IL2CPU";

        static void Main(string[] args)
        {
            var xTimespans = new List<TimeSpan>();
            #region Bench
            for (int i = 0; i < 1; i++)
            {
                var xSW = Stopwatch.StartNew();
                try
                {
                    var xTask = new IL2CPUTask();
                    xTask.DebugEnabled = true;
                    xTask.DebugMode = "Source";
                    xTask.TraceAssemblies = "User";
                    xTask.DebugCom = 1;
                    xTask.UseNAsm = true;
                    xTask.OutputFilename = CosmosRoot + @"\source2\Users\Kudzu\Breakpoints\bin\Debug\Kudzu.Breakpoints.asm";
                    xTask.EnableLogging = true;
                    xTask.EnableLogging = false;
                    xTask.EmitDebugSymbols = true;
                    xTask.IgnoreDebugStubAttribute = false;
                    xTask.References = GetReferences();
                    xTask.OnLogError = (m) => Console.WriteLine("Error: {0}", m);
                    xTask.OnLogWarning = (m) => Console.WriteLine("Warning: {0}", m);
                    xTask.OnLogMessage = (m) => Console.WriteLine("Message: {0}", m);
                    xTask.OnLogException = (m) =>
                    {
                        Console.WriteLine("Exception: {0}", m.ToString());
                        Console.ReadLine();
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
                    xTimespans.Add(xSW.Elapsed);

                }
                catch (Exception E)
                {
                    Console.WriteLine(E.ToString());
                    Console.ReadLine();
                    return;
                }
            }
            #endregion Bench
            for (int i = 0; i < xTimespans.Count; i++)
            {
                Console.WriteLine("Run {0} took {1}", i + 1, xTimespans[i].ToString());
            }
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

        static void SQLiteLog_Log(object sender, LogEventArgs e)
        {
            Console.WriteLine("SQL: {0}", e.Message);
        }

        private static ITaskItem[] GetReferences()
        {
            return new ITaskItem[]{
                new TaskItemImpl(CosmosRoot + @"\source2\Users\Kudzu\Breakpoints\bin\Debug\Playground.Kudzu.BreakpointsKernel.dll"),
                new TaskItemImpl(CosmosRoot + @"\source2\Kernel\System\Hardware\Core\Cosmos.Core.Plugs\bin\x86\Debug\Cosmos.Core.Plugs.dll"),
                new TaskItemImpl(CosmosRoot + @"\source2\Kernel\Debug\Cosmos.Debug.Kernel.Plugs\bin\x86\Debug\Cosmos.Debug.Kernel.Plugs.dll"),
                new TaskItemImpl(CosmosRoot + @"\source2\Kernel\System\Hardware\Cosmos.Hardware\bin\x86\Debug\Cosmos.Hardware.dll"),
                new TaskItemImpl(CosmosRoot + @"\source2\Kernel\System\Cosmos.System.Plugs.System\bin\x86\Debug\Cosmos.System.Plugs.System.dll"),
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