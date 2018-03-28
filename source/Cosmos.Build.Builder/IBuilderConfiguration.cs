namespace Cosmos.Build.Builder
{
    internal interface IBuilderConfiguration
    {
        bool NoVsLaunch { get; }
        bool UserKit { get; }
        bool StayOpen { get; }
        bool NoClean { get; }
        string VsPath { get; }
    }
}
