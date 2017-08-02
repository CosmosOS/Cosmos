using System;
using System.Reflection;

using Cosmos.Build.Common;

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

            // Sets the time before an error is registered. For example if set to 60 then if a kernel runs for more than 60 seconds then
            // that kernel will be marked as a failure and terminated
            engine.AllowedSecondsInKernel = 6000;

            // If you want to test only specific platforms, add them to the list, like next line. By default, all platforms are run.
            engine.RunTargets.Add(RunTargetEnum.Bochs);
            //engine.RunTargets.Add(RunTargetEnum.VMware);
            //engine.RunTargets.Add(RunTargetEnum.HyperV);

            // If you're working on the compiler (or other lower parts), you can choose to run the compiler in process
            // one thing to keep in mind though, is that this only works with 1 kernel at a time!
            engine.DebugIL2CPU = false;
            engine.TraceAssembliesLevel = TraceAssemblies.User;
            //engine.EnableStackCorruptionChecks = false;
            engine.KernelPkg = "X86G3";

            engine.EnableStackCorruptionChecks = true;
            engine.StackCorruptionChecksLevel = StackCorruptionDetectionLevel.AllInstructions;

            //engine.RunWithGDB = true;
            //engine.StartBochsDebugGui = true;

            // Select kernels to be tested by adding them to the engine
            foreach (var xType in TestKernelSets.GetKernelTypesToRun())
            {
                engine.AddKernel(xType.GetTypeInfo().Assembly.Location);
            }

            //engine.AddKernel(typeof(VGACompilerCrash.Kernel).Assembly.Location);
            //engine.AddKernel(typeof(Cosmos.Compiler.Tests.Bcl.Kernel).Assembly.Location);
            //engine.AddKernel(typeof(Cosmos.Compiler.Tests.SingleEchoTest.Kernel).Assembly.Location);
            //engine.AddKernel(typeof(Cosmos.Compiler.Tests.SimpleWriteLine.Kernel.Kernel).Assembly.Location);
            //engine.AddKernel(typeof(SimpleStructsAndArraysTest.Kernel).Assembly.Location);
            //engine.AddKernel(typeof(Cosmos.Compiler.Tests.Exceptions.Kernel).Assembly.Location);
            //engine.AddKernel(typeof(Cosmos.Compiler.Tests.MethodTests.Kernel).Assembly.Location);
            //engine.AddKernel(typeof(Cosmos.Kernel.Tests.Fat.Kernel).Assembly.Location);

            // Known bugs, therefore disabled for now:
            //engine.AddKernel(typeof(BoxingTests.Kernel).Assembly.Location);
            //engine.AddKernel(typeof(Cosmos.Compiler.Tests.MultidimensionalArrays.Kernel).Assembly.Location);

            // Experimental stuff:

            // end of known bugs

            // double check: this check is in the engine, but lets put it here as well
            if (engine.DebugIL2CPU)
            {
                if (engine.KernelsToRun.Count > 1 || engine.RunTargets.Count == 0 || engine.RunTargets.Count > 1)
                {
                    throw new InvalidOperationException("Can only run 1 kernel if IL2CPU is ran in-process!");
                }
            }
        }
    }
}
