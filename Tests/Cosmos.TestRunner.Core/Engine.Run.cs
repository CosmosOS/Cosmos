using System;
using System.IO;
using Cosmos.Build.Common;
using Cosmos.Build.MSBuild;
using Cosmos.Core.Plugs;
using Cosmos.Debug.Kernel;
using Cosmos.Debug.Kernel.Plugs;
using Cosmos.System.Plugs.System;
using IL2CPU;
using Microsoft.Win32;

namespace Cosmos.TestRunner.Core
{
    partial class Engine
    {
        private void ExecuteKernel(string assemblyFileName, RunConfiguration configuration)
        {
            OutputHandler.ExecuteKernelStart(assemblyFileName);
            try
            {

                var xAssemblyFile = Path.Combine(mBaseWorkingDirectory, "Kernel.asm");
                var xObjectFile = Path.Combine(mBaseWorkingDirectory, "Kernel.obj");
                var xTempObjectFile = Path.Combine(mBaseWorkingDirectory, "Kernel.o");
                var xIsoFile = Path.Combine(mBaseWorkingDirectory, "Kernel.iso");

                RunTask("IL2CPU", () => RunIL2CPU(assemblyFileName, xAssemblyFile));
                RunTask("Nasm", () => RunNasm(xAssemblyFile, xObjectFile, configuration.IsELF));
                if (configuration.IsELF)
                {
                    File.Move(xObjectFile, xTempObjectFile);

                    RunTask("Ld", () => RunLd(xTempObjectFile, xObjectFile));
                }

                RunTask("MakeISO", () => MakeIso(xObjectFile, xIsoFile));
                RunTask("RunISOInBochs", () => RunIsoInBochs(xIsoFile));
            }
            catch (Exception e)
            {
                if (!mKernelResultSet)
                {
                    OutputHandler.SetKernelTestResult(false, e.ToString());
                }
                if (e is TaskFailedException)
                {
                    return;
                }
                OutputHandler.UnhandledException(e);
            }
            finally
            {
                OutputHandler.ExecuteKernelEnd(assemblyFileName);
            }
        }


        private void RunTask(string taskName, Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            OutputHandler.TaskStart(taskName);
            try
            {
                action();
            }
            catch (Exception e)
            {
                OutputHandler.UnhandledException(e);
                throw new TaskFailedException();
            }
            finally
            {
                OutputHandler.TaskEnd(taskName);
            }
        }
    }
}
