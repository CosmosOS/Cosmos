using System;
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

        public void Execute()
        {
            mBaseWorkingDirectory = Path.Combine(Path.GetDirectoryName(typeof(Engine).Assembly.Location), "WorkingDirectory");
            if (Directory.Exists(mBaseWorkingDirectory))
            {
                Directory.Delete(mBaseWorkingDirectory, true);
            }
            Directory.CreateDirectory(mBaseWorkingDirectory);

            DoLog("Start executing");
            // todo: test with multiple configurations (for example, ELF and BIN format, with or without stack corruption detection, etc)

            foreach (var xAssemblyFile in mKernelsToRun)
            {
                ExecuteKernel(xAssemblyFile);
            }

            // todo: now report summary
            throw new NotImplementedException();
        }

        private int mLogLevel = 0;
        private void DoLog(string message)
        {
            Console.Write(new String(' ', mLogLevel * 2));
            Console.WriteLine(message);
        }
    }
}
