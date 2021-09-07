using System;
using System.Windows;
using Microsoft.VisualStudio.Setup.Configuration;

namespace Cosmos.Build.Builder.Services.VisualStudioSetup
{
    internal class VisualStudioSetupFailedPackageReference : VisualStudioSetupPackageReference, ISetupFailedPackageReference3
    {
        private ISetupFailedPackageReference3 _setupFailedPackageReference;

        public VisualStudioSetupFailedPackageReference(ISetupFailedPackageReference3 setupFailedPackageReference)
            : base(setupFailedPackageReference)
        {
            _setupFailedPackageReference = setupFailedPackageReference;
        }

        string ISetupFailedPackageReference3.GetType() => RunOnMainThread(_setupFailedPackageReference.GetType);

        public string GetLogFilePath() => RunOnMainThread(_setupFailedPackageReference.GetLogFilePath);
        public string GetDescription() => RunOnMainThread(_setupFailedPackageReference.GetDescription);
        public string GetSignature() => RunOnMainThread(_setupFailedPackageReference.GetSignature);
        public string[] GetDetails() => RunOnMainThread(_setupFailedPackageReference.GetDetails);

        public ISetupPackageReference[] GetAffectedPackages()
        {
            var packages = RunOnMainThread(_setupFailedPackageReference.GetAffectedPackages);

            for (int i = 0; i < packages.Length; i++)
            {
                packages[i] = new VisualStudioSetupPackageReference(packages[i]);
            }

            return packages;
        }

        public string GetAction() => RunOnMainThread(_setupFailedPackageReference.GetAction);
        public string GetReturnCode() => RunOnMainThread(_setupFailedPackageReference.GetReturnCode);
        string ISetupFailedPackageReference2.GetType() => RunOnMainThread(_setupFailedPackageReference.GetType);
        string ISetupFailedPackageReference.GetType() => RunOnMainThread(_setupFailedPackageReference.GetType);

        private static T RunOnMainThread<T>(Func<T> function) => Application.Current.Dispatcher.Invoke(function);
    }
}
