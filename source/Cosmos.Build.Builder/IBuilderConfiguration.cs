namespace Cosmos.Build.Builder
{
    internal interface IBuilderConfiguration
    {
        bool NoVsLaunch { get; }
        bool UserKit { get; }
        bool ResetHive { get; }
        bool StayOpen { get; }
        bool NoClean { get; }
        bool VsExpHive { get; }
        string VsPath { get; }
    }
}
