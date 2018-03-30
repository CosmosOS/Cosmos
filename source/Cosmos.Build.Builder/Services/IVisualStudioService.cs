using System.Collections.Generic;
using Microsoft.VisualStudio.Setup.Configuration;

namespace Cosmos.Build.Builder.Services
{
    internal interface IVisualStudioService
    {
        IReadOnlyList<ISetupInstance2> GetInstances();
    }
}
