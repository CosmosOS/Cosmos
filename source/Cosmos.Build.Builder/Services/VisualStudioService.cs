using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Setup.Configuration;

using Cosmos.Build.Builder.Services.VisualStudioSetup;

namespace Cosmos.Build.Builder.Services
{
    internal class VisualStudioService : IVisualStudioService
    {
        private readonly Lazy<IReadOnlyList<ISetupInstance2>> _instances;

        public VisualStudioService()
        {
            _instances = new Lazy<IReadOnlyList<ISetupInstance2>>(GetSetupInstances);
        }

        public IReadOnlyList<ISetupInstance2> GetInstances() => _instances.Value;

        private static List<ISetupInstance2> GetSetupInstances()
        {
            var instances = new List<ISetupInstance2>();

            var setupConfiguration = new SetupConfiguration();
            var instanceEnumerator = setupConfiguration.EnumInstances();

            var setupInstance = new ISetupInstance2[1];
            var count = 0;

            while (true)
            {
                instanceEnumerator.Next(1, setupInstance, out count);

                if (count == 0)
                {
                    break;
                }

                instances.Add(new VisualStudioSetupInstance(setupInstance[0]));
            }

            return instances;
        }
    }
}
