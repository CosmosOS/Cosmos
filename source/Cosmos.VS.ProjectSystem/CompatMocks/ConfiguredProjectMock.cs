using System.ComponentModel.Composition;
using Microsoft.VisualStudio.ProjectSystem;

namespace Cosmos.VS.ProjectSystem.CompatMocks
{
    [Export]
    internal sealed class ConfiguredProjectMock
    {
        [ImportingConstructor]
        public ConfiguredProjectMock(ConfiguredProjectServicesMock configuredProjectServicesMock)
        {
            Services = configuredProjectServicesMock;
        }
        
        public ConfiguredProjectServicesMock Services { get; }
    }
}
