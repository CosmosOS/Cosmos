using System;
using Microsoft.VisualStudio.Imaging.Interop;

namespace Cosmos.VS.ProjectSystem
{
    internal static class CosmosImagesMonikers
    {
        private static readonly Guid ManifestGuid = new Guid("9feaa5d5-9298-4c74-915e-2d0ab67992f9");

        private const int ProjectRootIconId = 0;

        public static ImageMoniker ProjectRootIcon => new ImageMoniker { Guid = ManifestGuid, Id = ProjectRootIconId };
    }
}
