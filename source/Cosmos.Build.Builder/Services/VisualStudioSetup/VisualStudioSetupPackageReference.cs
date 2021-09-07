using System;
using System.Windows;
using Microsoft.VisualStudio.Setup.Configuration;

namespace Cosmos.Build.Builder.Services.VisualStudioSetup
{
    internal class VisualStudioSetupPackageReference : ISetupPackageReference
    {
        private ISetupPackageReference _setupPackageReference;

        public VisualStudioSetupPackageReference(ISetupPackageReference setupPackageReference)
        {
            _setupPackageReference = setupPackageReference;
        }

        public string GetId() => RunOnMainThread(_setupPackageReference.GetId);
        public string GetVersion() => RunOnMainThread(_setupPackageReference.GetVersion);
        public string GetChip() => RunOnMainThread(_setupPackageReference.GetChip);
        public string GetLanguage() => RunOnMainThread(_setupPackageReference.GetLanguage);
        public string GetBranch() => RunOnMainThread(_setupPackageReference.GetBranch);
        string ISetupPackageReference.GetType() => RunOnMainThread(_setupPackageReference.GetType);
        public string GetUniqueId() => RunOnMainThread(_setupPackageReference.GetUniqueId);
        public bool GetIsExtension() => RunOnMainThread(_setupPackageReference.GetIsExtension);

        private static T RunOnMainThread<T>(Func<T> function) => Application.Current.Dispatcher.Invoke(function);
    }
}
