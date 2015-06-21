using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.TestRunner.Core
{
    public partial class Engine
    {
        private List<string> mKernelsToRun = new List<string>();
        public void AddKernel(string assemblyFile)
        {
            if (!File.Exists(assemblyFile))
            {
                throw new FileNotFoundException("Kernel file not found!", assemblyFile);
            }
            mKernelsToRun.Add(assemblyFile);
        }

        private string mBaseWorkingDirectory;

        public OutputHandlerBase OutputHandler;

        public void Execute()
        {
            if (OutputHandler == null)
            {
                throw new InvalidOperationException("No OutputHandler set!");
            }

            mBaseWorkingDirectory = Path.Combine(Path.GetDirectoryName(typeof(Engine).Assembly.Location), "WorkingDirectory");
            if (Directory.Exists(mBaseWorkingDirectory))
            {
                Directory.Delete(mBaseWorkingDirectory, true);
            }
            Directory.CreateDirectory(mBaseWorkingDirectory);

            OutputHandler.ExecutionStart();
            try
            {
                // todo: test with multiple configurations (for example, ELF and BIN format, with or without stack corruption detection, etc)
                foreach (var xAssemblyFile in mKernelsToRun)
                {
                    ExecuteKernel(xAssemblyFile);
                }
            }
            catch (Exception E)
            {
                OutputHandler.UnhandledException(E);
            }
            finally
            {
                OutputHandler.ExecutionEnd();
            }

            // todo: now report summary
            //DoLog("NotImplemented, summary?");
        }
    }
}
