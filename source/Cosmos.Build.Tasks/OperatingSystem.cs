using System.Runtime.InteropServices;

namespace Cosmos.Build.Tasks
{
    public static class OperatingSystem
    {
        public static bool IsWindows() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }
}
