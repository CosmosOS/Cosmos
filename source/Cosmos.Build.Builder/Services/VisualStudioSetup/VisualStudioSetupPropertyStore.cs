using System;
using System.Windows;
using Microsoft.VisualStudio.Setup.Configuration;

namespace Cosmos.Build.Builder.Services.VisualStudioSetup
{
    internal class VisualStudioSetupPropertyStore : ISetupPropertyStore
    {
        private ISetupPropertyStore _setupPropertyStore;

        public VisualStudioSetupPropertyStore(ISetupPropertyStore setupPropertyStore)
        {
            _setupPropertyStore = setupPropertyStore;
        }

        public string[] GetNames() => RunOnMainThread(_setupPropertyStore.GetNames);
        public object GetValue(string pwszName) => RunOnMainThread(() => _setupPropertyStore.GetValue(pwszName));

        private static T RunOnMainThread<T>(Func<T> function) => Application.Current.Dispatcher.Invoke(function);
    }
}
