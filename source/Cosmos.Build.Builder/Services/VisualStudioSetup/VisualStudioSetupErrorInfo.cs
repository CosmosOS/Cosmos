using System;
using System.Windows;
using Microsoft.VisualStudio.Setup.Configuration;

namespace Cosmos.Build.Builder.Services.VisualStudioSetup
{
    internal class VisualStudioSetupErrorInfo : ISetupErrorInfo
    {
        private ISetupErrorInfo _setupErrorInfo;

        public VisualStudioSetupErrorInfo(ISetupErrorInfo setupErrorInfo)
        {
            _setupErrorInfo = setupErrorInfo;
        }

        public int GetErrorHResult() => RunOnMainThread(_setupErrorInfo.GetErrorHResult);
        public string GetErrorClassName() => RunOnMainThread(_setupErrorInfo.GetErrorClassName);
        public string GetErrorMessage() => RunOnMainThread(_setupErrorInfo.GetErrorMessage);

        private static T RunOnMainThread<T>(Func<T> function) => Application.Current.Dispatcher.Invoke(function);
    }
}
