using System.Collections.Generic;
using System.IO;

using Cosmos.Build.Builder.Services;

namespace Cosmos.Build.Builder.BuildTasks
{
    internal class RestoreTask : MSBuildTargetBuildTaskBase
    {
        private const string RestoreTaskName = "Restore";

        public override string Name => $"Restore - {Path.GetFileName(ProjectFilePath)}";

        public override string ProjectFilePath { get; }

        public override IEnumerable<string> Targets { get { yield return RestoreTaskName; } }

        protected override IReadOnlyDictionary<string, string> Properties => null;

        public RestoreTask(
            IMSBuildService msBuildService,
            string projectFilePath)
            : base(msBuildService)
        {
            ProjectFilePath = projectFilePath;
        }
    }
}
