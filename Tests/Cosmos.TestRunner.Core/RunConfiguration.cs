namespace Cosmos.TestRunner.Core
{
    public class RunConfiguration
    {
        public bool IsELF { get; }
        public RunTargetEnum RunTarget { get; }

        public RunConfiguration(bool isElf, RunTargetEnum runTarget)
        {
            IsELF = isElf;
            RunTarget = runTarget;
        }
    }
}
