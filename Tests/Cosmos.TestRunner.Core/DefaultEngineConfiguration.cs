﻿using System;

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

            engine.AllowedSecondsInKernel = 3000;

            // Currently, there's still a bug somewhere for testing vmware.
            //engine.RunTargets.Remove(RunTargetEnum.Bochs);

            // if you're working on the compiler (or other lower parts), you can choose to run the compiler in process
            // 1 thing to keep in mind though, is that this only works with 1 kernel at a time!
            engine.RunIL2CPUInProcess = false;

            engine.AddKernel(typeof(Cosmos.Compiler.Tests.SimpleWriteLine.Kernel.Kernel).Assembly.Location);
            engine.AddKernel(typeof(SimpleStructsAndArraysTest.Kernel).Assembly.Location);
            engine.AddKernel(typeof(VGACompilerCrash.Kernel).Assembly.Location);

            // known bugs, therefor disabled for now:
        }
    }
}
