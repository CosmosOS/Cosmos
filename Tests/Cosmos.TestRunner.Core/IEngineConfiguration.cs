using System;
using System.Collections.Generic;

using Cosmos.Build.Common;

namespace Cosmos.TestRunner.Core
{
    public interface IEngineConfiguration
    {
        int AllowedSecondsInKernel { get; }
        IEnumerable<RunTargetEnum> RunTargets { get; }
        bool RunWithGDB { get; }
        bool StartBochsDebugGUI { get; }

        bool DebugIL2CPU { get; }
        string KernelPkg { get; }
        TraceAssemblies TraceAssembliesLevel { get; }
        bool EnableStackCorruptionChecks { get; }
        StackCorruptionDetectionLevel StackCorruptionChecksLevel { get; }

        IEnumerable<Type> KernelTypesToRun { get; }
    }
}
