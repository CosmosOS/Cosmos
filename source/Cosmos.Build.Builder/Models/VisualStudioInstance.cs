using Microsoft.VisualStudio.Setup.Configuration;

namespace Cosmos.Build.Builder.Models
{
    internal class VisualStudioInstance
    {
        public ISetupInstance2 SetupInstance { get; }

        public string ID => SetupInstance.GetInstanceId();
        public string Name => SetupInstance.GetDisplayName();
        public string InstallationPath => SetupInstance.GetInstallationPath();

        public VisualStudioInstance(ISetupInstance2 setupInstance)
        {
            SetupInstance = setupInstance;
        }
    }
}
