using System.Collections.Generic;
using System.IO;

using Cosmos.Build.Builder.Services;

namespace Cosmos.Build.Builder.BuildTasks
{
    internal class PublishTask : MSBuildTargetBuildTaskBase
    {
        private const string PublishTargetName = "Publish";

        public override string Name => $"Publish - {Path.GetFileName(ProjectFilePath)}";

        public override string ProjectFilePath { get; }

        public override IEnumerable<string> Targets { get { yield return PublishTargetName; } }

        protected override IReadOnlyDictionary<string, string> Properties => _properties;

        private readonly Dictionary<string, string> _properties;

        public PublishTask(
            IMSBuildService msBuildService,
            string projectFilePath,
            string publishOutputPath)
            : base(msBuildService)
        {
            ProjectFilePath = projectFilePath;

            _properties = new Dictionary<string, string>()
            {
                ["PublishDir"] = publishOutputPath
            };
        }
    }
}
