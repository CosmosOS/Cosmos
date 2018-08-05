namespace Cosmos.TestRunner.Core
{
    public interface IKernelTestResult
    {
        string KernelName { get; }
        RunConfiguration RunConfiguration { get; }

        bool Result { get; }
        string TestLog { get; }
    }
}
