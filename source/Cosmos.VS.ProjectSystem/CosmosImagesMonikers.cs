using System;
using Microsoft.VisualStudio.Imaging.Interop;

namespace Cosmos.VS.ProjectSystem
{
    internal static class CosmosImagesMonikers
    {
        private static readonly Guid ManifestGuid = new Guid("{4862ad45-3b62-4112-8dcd-cf2df64ff519}");

        private const int ProjectRootIconId = 2;

        public static ImageMoniker ProjectRootIcon => new ImageMoniker { Guid = ManifestGuid, Id = ProjectRootIconId };
    }
}
