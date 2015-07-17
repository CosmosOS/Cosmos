namespace Cosmos.TestRunner
{
    public enum TestChannelCommandEnum: byte
    {
        TestCompleted = 0,
        TestFailed = 1,
        AssertionSucceeded = 2,
    }
}
