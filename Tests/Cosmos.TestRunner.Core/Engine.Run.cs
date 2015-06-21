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
        private bool mIsELF = true;
        private void ExecuteKernel(string assemblyFileName)
        {
            var xAssemblyFile = Path.Combine(mBaseWorkingDirectory, "Kernel.asm");
            var xObjectFile = Path.Combine(mBaseWorkingDirectory, "Kernel.obj");
            var xTempObjectFile = Path.Combine(mBaseWorkingDirectory, "Kernel.o");
            var xIsoFile = Path.Combine(mBaseWorkingDirectory, "Kernel.iso");

            mLogLevel = 1;
            DoLog(string.Format("Testing '{0}'", assemblyFileName));
            mLogLevel = 2;
            RunIL2CPU(assemblyFileName, xAssemblyFile);
            mLogLevel = 2;
            RunNasm(xAssemblyFile, xObjectFile, mIsELF);
            if (mIsELF)
            {
                File.Move(xObjectFile, xTempObjectFile);

                mLogLevel = 2;
                RunLd(xTempObjectFile, xObjectFile);
            }

            mLogLevel = 2;
            MakeIso(xObjectFile, xIsoFile);
            mLogLevel = 2;
            RunIsoInBochs(xIsoFile);
        }


    }
}
