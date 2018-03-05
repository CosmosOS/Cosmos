using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.ProjectSystem;

using VSPropertyPages;

namespace Cosmos.VS.ProjectSystem.VS.PropertyPages
{
    [Guid(PageGuid)]
    internal class OldCosmosPropertyPage : PropertyPageBase
    {
        public const string PageGuid = "8624b37e-183d-416c-a635-99ebc3bcffe6";

        public override string PageName => "Cosmos";

        public override IPropertyPageUI CreatePropertyPageUI() =>
            new OldCosmosPropertyPageControl(
                new OldCosmosPropertyPageViewModel((OldPropertyManager)PropertyManager, ProjectThreadingService));

        public override IPropertyManager CreatePropertyManager(
            IReadOnlyCollection<ConfiguredProject> configuredProjects) => new OldPropertyManager(UnconfiguredProject);
    }
}
