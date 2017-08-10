namespace Cosmos.Build.Builder
{
    /// <summary>
    /// Build state enum.
    /// </summary>
    public enum BuildState
    {
        CleanupError,
        CompilationError,
        PrerequisiteMissing,
        Running
    }
}