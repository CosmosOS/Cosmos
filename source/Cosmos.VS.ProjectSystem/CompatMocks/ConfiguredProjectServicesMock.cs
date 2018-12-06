using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Composition;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Properties;

namespace Cosmos.VS.ProjectSystem.CompatMocks
{
    [Export]
    internal sealed class ConfiguredProjectServicesMock
    {
        [ImportingConstructor]
        public ConfiguredProjectServicesMock(
            IAdditionalRuleDefinitionsService additionalRuleDefinitions,
            IPropertyPagesCatalogProvider propertyPagesCatalog)
        {
            AdditionalRuleDefinitions = additionalRuleDefinitions;
            PropertyPagesCatalog = propertyPagesCatalog;
        }

        public IAdditionalRuleDefinitionsService AdditionalRuleDefinitions { get; }

        public IPropertyPagesCatalogProvider PropertyPagesCatalog { get; }
    }
}
