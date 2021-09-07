using System.IO;
using Microsoft.VisualStudio.Setup.Configuration;

namespace Cosmos.Build.Builder.Services
{
    internal class FullMSBuildService : IMSBuildService
    {
        private readonly ISetupInstance2 _visualStudioInstance;

        public FullMSBuildService(ISetupInstance2 visualStudioInstance)
        {
            _visualStudioInstance = visualStudioInstance;
        }

        public string GetMSBuildExePath()
        {
            var msBuildExePath = GetMSBuildExePathForVersion("15.0");

            if (File.Exists(msBuildExePath))
            {
                return msBuildExePath;
            }

            msBuildExePath = GetMSBuildExePathForVersion("Current");

            if (File.Exists(msBuildExePath))
            {
                return msBuildExePath;
            }

            return null;
        }

        private string GetMSBuildExePathForVersion(string version) =>
            Path.Combine(_visualStudioInstance.GetInstallationPath(), "MSBuild", version, "Bin", "MSBuild.exe");
    }
}
