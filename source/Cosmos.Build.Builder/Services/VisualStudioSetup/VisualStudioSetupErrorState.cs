using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualStudio.Setup.Configuration;

namespace Cosmos.Build.Builder.Services.VisualStudioSetup
{
    class VisualStudioSetupErrorState : ISetupErrorState3
    {
        private ISetupErrorState3 _setupErrorState;

        public VisualStudioSetupErrorState(ISetupErrorState3 setupErrorState)
        {
            _setupErrorState = setupErrorState;
        }

        public ISetupFailedPackageReference[] GetFailedPackages()
        {
            var packages = RunOnMainThread(_setupErrorState.GetFailedPackages);

            for (int i = 0; i < packages.Length; i++)
            {
                packages[i] = new VisualStudioSetupFailedPackageReference((ISetupFailedPackageReference3)packages[i]);
            }

            return packages;
        }

        public ISetupPackageReference[] GetSkippedPackages()
        {
            var packages = RunOnMainThread(_setupErrorState.GetSkippedPackages);

            for (int i = 0; i < packages.Length; i++)
            {
                packages[i] = new VisualStudioSetupPackageReference(packages[i]);
            }

            return packages;
        }

        public string GetErrorLogFilePath() => RunOnMainThread(_setupErrorState.GetErrorLogFilePath);
        public string GetLogFilePath() => RunOnMainThread(_setupErrorState.GetLogFilePath);
        public ISetupErrorInfo GetRuntimeError() => new VisualStudioSetupErrorInfo(RunOnMainThread(_setupErrorState.GetRuntimeError));

        private static T RunOnMainThread<T>(Func<T> function) => Application.Current.Dispatcher.Invoke(function);
    }
}
