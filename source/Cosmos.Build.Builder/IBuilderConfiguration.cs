namespace Cosmos.Build.Builder
{
    internal interface IBuilderConfiguration
    {
        bool NoVsLaunch { get; }
        bool UserKit { get; }
        string VsPath { get; }
    }
}
