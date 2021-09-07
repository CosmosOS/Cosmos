using System;
using System.Runtime.InteropServices.ComTypes;
using System.Windows;
using Microsoft.VisualStudio.Setup.Configuration;

namespace Cosmos.Build.Builder.Services.VisualStudioSetup
{
    internal class VisualStudioSetupInstance : ISetupInstance2
    {
        private ISetupInstance2 _setupInstance;

        public VisualStudioSetupInstance(ISetupInstance2 setupInstance)
        {
            _setupInstance = setupInstance;
        }

        public string GetInstanceId() => RunOnMainThread(_setupInstance.GetInstanceId);
        public FILETIME GetInstallDate() => RunOnMainThread(_setupInstance.GetInstallDate);
        public string GetInstallationName() => RunOnMainThread(_setupInstance.GetInstallationName);
        public string GetInstallationPath() => RunOnMainThread(_setupInstance.GetInstallationPath);
        public string GetInstallationVersion() => RunOnMainThread(_setupInstance.GetInstallationVersion);
        public string GetDisplayName(int lcid = 0) => RunOnMainThread(() => _setupInstance.GetDisplayName(lcid));
        public string GetDescription(int lcid = 0) => RunOnMainThread(() => _setupInstance.GetDescription(lcid));
        public string ResolvePath(string pwszRelativePath = null) => RunOnMainThread(() => _setupInstance.ResolvePath(pwszRelativePath));
        public InstanceState GetState() => RunOnMainThread(_setupInstance.GetState);

        public ISetupPackageReference[] GetPackages()
        {
            var packages = RunOnMainThread(_setupInstance.GetPackages);

            for (int i = 0; i < packages.Length; i++)
            {
                packages[i] = new VisualStudioSetupPackageReference(packages[i]);
            }

            return packages;
        }

        public ISetupPackageReference GetProduct() => new VisualStudioSetupPackageReference(RunOnMainThread(_setupInstance.GetProduct));
        public string GetProductPath() => RunOnMainThread(_setupInstance.GetProductPath);
        public ISetupErrorState GetErrors() => new VisualStudioSetupErrorState((ISetupErrorState3)RunOnMainThread(_setupInstance.GetErrors));
        public bool IsLaunchable() => RunOnMainThread(_setupInstance.IsLaunchable);
        public bool IsComplete() => RunOnMainThread(_setupInstance.IsComplete);
        public ISetupPropertyStore GetProperties() => new VisualStudioSetupPropertyStore(RunOnMainThread(_setupInstance.GetProperties));
        public string GetEnginePath() => RunOnMainThread(_setupInstance.GetEnginePath);

        private static T RunOnMainThread<T>(Func<T> function) => Application.Current.Dispatcher.Invoke(function);
    }
}
