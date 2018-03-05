using System;
using System.Collections.Generic;
using System.IO;

namespace Cosmos.TestRunner.Core
{
    partial class Engine
    {
        protected int AllowedSecondsInKernel => mConfiguration.AllowedSecondsInKernel;
        protected IEnumerable<RunTargetEnum> RunTargets => mConfiguration.RunTargets;

        private bool ExecuteKernel(string assemblyFileName, string workingDirectory, RunConfiguration configuration)
        {
            var xResult = true;
            OutputHandler.ExecuteKernelStart(assemblyFileName);
            try
            {
                var xAssemblyFile = Path.Combine(workingDirectory, "Kernel.asm");
                var xObjectFile = Path.Combine(workingDirectory, "Kernel.obj");
                var xTempObjectFile = Path.Combine(workingDirectory, "Kernel.o");
                var xIsoFile = Path.Combine(workingDirectory, "Kernel.iso");

                if (KernelPkg == "X86")
                {
                    RunTask("TheRingMaster", () => RunTheRingMaster(assemblyFileName));
                }
                RunTask("IL2CPU", () => RunIL2CPU(assemblyFileName, xAssemblyFile));
                RunTask("Nasm", () => RunNasm(xAssemblyFile, xObjectFile, configuration.IsELF));
                if (configuration.IsELF)
                {
                    File.Move(xObjectFile, xTempObjectFile);

                    RunTask("Ld", () => RunLd(xTempObjectFile, xObjectFile));
                    RunTask("ExtractMapFromElfFile", () => RunExtractMapFromElfFile(workingDirectory, xObjectFile));
                }

                string xHarddiskPath;
                if (configuration.RunTarget == RunTargetEnum.HyperV)
                {
                    xHarddiskPath = Path.Combine(workingDirectory, "Harddisk.vhdx");
                    var xOriginalHarddiskPath = Path.Combine(GetCosmosUserkitFolder(), "Build", "HyperV", "Filesystem.vhdx");
                    File.Copy(xOriginalHarddiskPath, xHarddiskPath);
                }
                else
                {
                    xHarddiskPath = Path.Combine(workingDirectory, "Harddisk.vmdk");
                    var xOriginalHarddiskPath = Path.Combine(GetCosmosUserkitFolder(), "Build", "VMware", "Workstation", "Filesystem.vmdk");
                    File.Copy(xOriginalHarddiskPath, xHarddiskPath);
                }

                RunTask("MakeISO", () => MakeIso(xObjectFile, xIsoFile));
                switch (configuration.RunTarget)
                {
                    case RunTargetEnum.Bochs:
                        RunTask("RunISO", () => RunIsoInBochs(xIsoFile, xHarddiskPath, workingDirectory));
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
