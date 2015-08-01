﻿using System;
﻿using System.Linq;

namespace Cosmos.TestRunner.Core
{
    public static class DefaultEngineConfiguration
    {
        public static void Apply(Engine engine)
        {
            if (engine == null)
            {
                throw new ArgumentNullException("engine");
            }

            engine.AllowedSecondsInKernel = 120;

            // If you want to test only specific platforms, add them to the list, like next line. By default, all platforms are ran.
            engine.RunTargets.Add(RunTargetEnum.VMware);

            // if you're working on the compiler (or other lower parts), you can choose to run the compiler in process
            // 1 thing to keep in mind though, is that this only works with 1 kernel at a time!
            engine.RunIL2CPUInProcess = true;

            engine.AddKernel(typeof(Cosmos.Compiler.Tests.SimpleWriteLine.Kernel.Kernel).Assembly.Location);
            engine.AddKernel(typeof(SimpleStructsAndArraysTest.Kernel).Assembly.Location);
            //engine.AddKernel(typeof(VGACompilerCrash.Kernel).Assembly.Location);
            // engine.AddKernel(typeof(BoxingTests.Kernel).Assembly.Location);
            // known bugs, therefor disabled for now:

            // end of known bugs

            // double check: this check is in the engine, but lets put it here as well
            if (engine.RunIL2CPUInProcess)
            {
                if (engine.KernelsToRun.Count() > 1 || engine.RunTargets.Count == 0 || engine.RunTargets.Count > 1)
                {
                    throw new InvalidOperationException("Can only run 1 kernel if IL2CPU is ran in-process!");
                }
            }
        }
    }
}
