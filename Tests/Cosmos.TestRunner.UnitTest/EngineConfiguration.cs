using System;
using System.Collections.Generic;

using Cosmos.Build.Common;
using Cosmos.TestRunner.Core;
using Cosmos.TestRunner.Full;

namespace Cosmos.TestRunner.UnitTest
{
    internal class EngineConfiguration : DefaultEngineConfiguration
    {
        private Type mKernelType;

        public EngineConfiguration(Type aKernelType)
        {
            mKernelType = aKernelType;
        }

        public override int AllowedSecondsInKernel => 1200;

        public override IEnumerable<RunTargetEnum> RunTargets
        {
            get
            {
                yield return RunTargetEnum.Bochs;
            }
        }

        public override StackCorruptionDetectionLevel StackCorruptionDetectionLevel => StackCorruptionDetectionLevel.MethodFooters;

        public override IEnumerable<string> KernelAssembliesToRun
        {
            get
            {
                yield return mKernelType.Assembly.Location;
            }
        }
    }
}
