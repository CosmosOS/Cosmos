using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.TestRunner.Core
{
    public partial class Engine
    {
        // configuration: in process eases debugging, but means certain errors (like stack overflow) kill the test runner.
        public const bool RunIL2CPUInProcess = false;

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

            OutputHandler.ExecutionStart();
            try
            {
                foreach (var xConfig in GetRunConfigurations())
                {
                    OutputHandler.RunConfigurationStart(xConfig);
                    try
                    {
                        foreach (var xAssemblyFile in mKernelsToRun)
                        {
                            mBaseWorkingDirectory = Path.Combine(Path.GetDirectoryName(typeof(Engine).Assembly.Location), "WorkingDirectory");
                            if (Directory.Exists(mBaseWorkingDirectory))
                            {
                                Directory.Delete(mBaseWorkingDirectory, true);
                            }
                            Directory.CreateDirectory(mBaseWorkingDirectory);

                            ExecuteKernel(xAssemblyFile, xConfig);
                        }
                    }
                    catch (Exception e)
                    {
                        OutputHandler.UnhandledException(e);
                    }
                    finally
                    {
                        OutputHandler.RunConfigurationEnd(xConfig);
                    }
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

        private IEnumerable<RunConfiguration> GetRunConfigurations()
        {
            yield return new RunConfiguration {IsELF = true};
            //yield return new RunConfiguration {IsELF = false};
        }
    }
}
