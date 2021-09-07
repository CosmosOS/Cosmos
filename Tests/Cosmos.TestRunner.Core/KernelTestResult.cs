namespace Cosmos.TestRunner.Core
{
    internal class KernelTestResult : IKernelTestResult
    {
        public string KernelName { get; }
        public RunConfiguration RunConfiguration { get; }

        public bool Result { get; set; }

        public string TestLog { get; set; }

        public KernelTestResult(
            string aKernelName,
            RunConfiguration aRunConfiguration)
        {
            KernelName = aKernelName;
            RunConfiguration = aRunConfiguration;
        }
    }
}
