using System.IO;
using Microsoft.VisualStudio.Setup.Configuration;

namespace Cosmos.Build.Builder.Services
{
    internal class FullMSBuildService : IMSBuildService
    {
        private ISetupInstance2 _visualStudioInstance;

        public FullMSBuildService(ISetupInstance2 visualStudioInstance)
        {
            _visualStudioInstance = visualStudioInstance;
        }

        public string GetMSBuildExePath()
        {
            var msBuildExePath = Path.Combine(
                _visualStudioInstance.GetInstallationPath(), "MSBuild", "15.0", "Bin", "MSBuild.exe");

            if (File.Exists(msBuildExePath))
            {
                return msBuildExePath;
            }

            return null;
        }
    }
}
