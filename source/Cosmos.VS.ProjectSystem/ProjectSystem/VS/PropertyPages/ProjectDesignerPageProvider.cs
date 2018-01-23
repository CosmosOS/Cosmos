using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.VS.Properties;

using VSPropertyPages;

namespace Cosmos.VS.ProjectSystem.VS.PropertyPages
{
    [Export(typeof(IVsProjectDesignerPageProvider))]
    [AppliesTo(ProjectCapability.CosmosAndAppDesigner)]
    internal class ProjectDesignerPageProvider : IVsProjectDesignerPageProvider
    {
        private static readonly IPageMetadata CosmosPage =
            new PropertyPageMetadata("Cosmos", CosmosPropertyPage.PageGuid, Int32.MinValue, false);

        public Task<IReadOnlyCollection<IPageMetadata>> GetPagesAsync()
        {
            return Task.FromResult<IReadOnlyCollection<IPageMetadata>>(
                ImmutableArray.Create(CosmosPage));
        }
    }
}
