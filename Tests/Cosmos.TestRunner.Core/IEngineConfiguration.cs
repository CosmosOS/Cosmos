using System;
using System.Collections.Generic;

using Cosmos.Build.Common;

namespace Cosmos.TestRunner.Core
{
    public interface IEngineConfiguration
    {
        /// <summary>
        /// Sets the time before an error is registered. For example if set to 60 then if a kernel runs for more than
        /// 60 seconds then that kernel will be marked as a failure and terminated.
        /// </summary>
        int AllowedSecondsInKernel { get; }
        /// <summary>
        /// An enumerable of platforms to test.
        /// </summary>
        IEnumerable<RunTargetEnum> RunTargets { get; }
        bool RunWithGDB { get; }
        bool StartBochsDebugGUI { get; }

        /// <summary>
        /// If you're working on the compiler (or other lower parts), you can choose to run the compiler in process
        /// one thing to keep in mind though, is that this only works with 1 kernel at a time!
        /// </summary>
        bool DebugIL2CPU { get; }
        string KernelPkg { get; }
        TraceAssemblies TraceAssembliesLevel { get; }
        bool EnableStackCorruptionChecks { get; }
        StackCorruptionDetectionLevel StackCorruptionDetectionLevel { get; }

        /// <summary>
        /// An enumerable of kernel assemblies which will be run.
        /// </summary>
        IEnumerable<string> KernelAssembliesToRun { get; }
    }
}
