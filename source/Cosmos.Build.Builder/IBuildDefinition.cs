using System.Collections.Generic;

namespace Cosmos.Build.Builder
{
    internal interface IBuildDefinition
    {
        IEnumerable<IDependency> GetDependencies();
        IEnumerable<IBuildTask> GetBuildTasks();
    }
}
