namespace Cosmos.Build.Builder
{
    internal interface IBuilderConfiguration
    {
        bool NoVsLaunch { get; }
        bool UserKit { get; }
        bool BuildExtensions { get; }
    }
}
