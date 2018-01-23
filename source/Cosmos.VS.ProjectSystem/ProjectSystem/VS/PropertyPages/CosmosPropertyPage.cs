using System.Runtime.InteropServices;
using Microsoft.VisualStudio.ProjectSystem;

using VSPropertyPages;

namespace Cosmos.VS.ProjectSystem.VS.PropertyPages
{
    [Guid(PageGuid)]
    internal class CosmosPropertyPage : PropertyPage
    {
        public const string PageGuid = "8624b37e-183d-416c-a635-99ebc3bcffe6";

        public override string PageName => "Cosmos";

        public override IPropertyPageUI CreatePropertyPageUI() => new CosmosPropertyPageControl();

        public override PropertyPageViewModel CreatePropertyPageViewModel(
            UnconfiguredProject unconfiguredProject,
            IProjectThreadingService projectThreadingService)
        {
            return new CosmosPropertyPageViewModel(
                new OldPropertyManager(unconfiguredProject),
                projectThreadingService);
        }
    }
}
