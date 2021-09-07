using System.Collections.Generic;

namespace Cosmos.TestRunner.Core
{
    internal class TestResult : ITestResult
    {
        private List<IKernelTestResult> mKernelTestResults;
        public IReadOnlyList<IKernelTestResult> KernelTestResults => mKernelTestResults;

        public TestResult()
        {
            mKernelTestResults = new List<IKernelTestResult>();
        }

        public void AddKernelTestResult(IKernelTestResult kernelTestResult)
        {
            mKernelTestResults.Add(kernelTestResult);
        }
    }
}
