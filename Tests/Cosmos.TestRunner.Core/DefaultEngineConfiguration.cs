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

            engine.AllowedSecondsInKernel = 30;

            // Currently, there's still a bug somewhere for testing vmware.
            engine.RunTargets.Remove(RunTargetEnum.VMware);

            engine.AddKernel(typeof(Cosmos.Compiler.Tests.SimpleWriteLine.Kernel.Kernel).Assembly.Location);
            engine.AddKernel(typeof(SimpleStructsAndArraysTest.Kernel).Assembly.Location);
            engine.AddKernel(typeof(VGACompilerCrash.Kernel).Assembly.Location);

            // known bugs, therefor disabled for now:
        }
    }
}
