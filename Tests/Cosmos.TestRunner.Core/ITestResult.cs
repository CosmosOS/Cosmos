using System.Collections.Generic;

namespace Cosmos.TestRunner.Core
{
    public interface ITestResult
    {
        IReadOnlyList<IKernelTestResult> KernelTestResults { get; }
    }
}
