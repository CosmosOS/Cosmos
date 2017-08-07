using System;
using System.Collections.Generic;
using System.IO;

namespace Cosmos.TestRunner.Core
{
    partial class Engine
    {
        public int AllowedSecondsInKernel = 30;
        public List<RunTargetEnum> RunTargets = new List<RunTargetEnum>();

        private bool ExecuteKernel(string assemblyFileName, RunConfiguration configuration)
        {
            var xResult = true;
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
                    RunTask("ExtractMapFromElfFile", () => RunExtractMapFromElfFile(mBaseWorkingDirectory, xObjectFile));
                }

                string xHarddiskPath;
                if (configuration.RunTarget == RunTargetEnum.HyperV)
                {
                    xHarddiskPath = Path.Combine(mBaseWorkingDirectory, "Harddisk.vhdx");
                    var xOriginalHarddiskPath = Path.Combine(GetCosmosUserkitFolder(), "Build", "HyperV", "Filesystem.vhdx");
                    File.Copy(xOriginalHarddiskPath, xHarddiskPath);
                }
                else
                {
                    xHarddiskPath = Path.Combine(mBaseWorkingDirectory, "Harddisk.vmdk");
                    var xOriginalHarddiskPath = Path.Combine(GetCosmosUserkitFolder(), "Build", "VMware", "Workstation", "Filesystem.vmdk");
                    File.Copy(xOriginalHarddiskPath, xHarddiskPath);
                }

                RunTask("MakeISO", () => MakeIso(xObjectFile, xIsoFile));
                switch (configuration.RunTarget)
                {
                    case RunTargetEnum.Bochs:
                        RunTask("RunISO", () => RunIsoInBochs(xIsoFile, xHarddiskPath));
                        break;
                    case RunTargetEnum.VMware:
                        RunTask("RunISO", () => RunIsoInVMware(xIsoFile, xHarddiskPath));
                        break;
                    case RunTargetEnum.HyperV:
                        RunTask("RunISO", () => RunIsoInHyperV(xIsoFile, xHarddiskPath));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("RunTarget " + configuration.RunTarget + " not implemented!");
                }
            }
            catch (Exception e)
            {
                if (!mKernelResultSet)
                {
                    OutputHandler.SetKernelTestResult(false, e.ToString());
                    mKernelResult = false;
                }
                if (e is TaskFailedException)
                {
                    return mKernelResult;
                }
                OutputHandler.UnhandledException(e);
            }
            finally
            {
                OutputHandler.ExecuteKernelEnd(assemblyFileName);

            }
            xResult = mKernelResult;
            return xResult;
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
