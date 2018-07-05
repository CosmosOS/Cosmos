using System.Collections.Generic;
using System.IO;

using Cosmos.Build.Builder.Services;

namespace Cosmos.Build.Builder.BuildTasks
{
    internal class PackTask : MSBuildTargetBuildTaskBase
    {
        private const string PackTargetName = "Pack";

        public override string Name => $"Pack - {Path.GetFileName(ProjectFilePath)}";

        public override string ProjectFilePath { get; }

        public override IEnumerable<string> Targets { get { yield return PackTargetName; } }

        protected override IReadOnlyDictionary<string, string> Properties => _properties;

        private Dictionary<string, string> _properties;

        public PackTask(
            IMSBuildService msBuildService,
            string projectFilePath,
            string packageOutputPath)
            : base(msBuildService)
        {
            ProjectFilePath = projectFilePath;

            _properties = new Dictionary<string, string>();
            _properties.Add("PackageOutputPath", packageOutputPath);
        }
    }
}
