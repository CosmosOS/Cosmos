using System;
using System.Collections.Generic;

using Cosmos.Build.Common;

namespace Cosmos.TestRunner.Core
{
    public class DefaultEngineConfiguration : IEngineConfiguration
    {
        public virtual int AllowedSecondsInKernel => 6000;

        public virtual IEnumerable<RunTargetEnum> RunTargets
        {
            get
            {
                yield return RunTargetEnum.Bochs;
                //yield return RunTargetEnum.VMware;
                //yield return RunTargetEnum.HyperV;
            }
        }

        public virtual bool RunWithGDB => false;
        public virtual bool StartBochsDebugGUI => false;

        public virtual bool DebugIL2CPU => false;
        public virtual string KernelPkg => String.Empty;
        public virtual TraceAssemblies TraceAssembliesLevel => TraceAssemblies.User;
        public virtual bool EnableStackCorruptionChecks => true;
        public virtual StackCorruptionDetectionLevel StackCorruptionDetectionLevel => StackCorruptionDetectionLevel.AllInstructions;

        public virtual IEnumerable<Type> KernelTypesToRun => TestKernelSets.GetStableKernelTypes();
    }
}
