namespace Cosmos.Build.Builder
{
    internal interface ILogger
    {
        void NewSection(string name);
        void LogMessage(string text);
        void SetError();
    }
}
