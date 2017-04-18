using System.Reflection;
using Microsoft.VisualStudio.Project;

namespace Cosmos.VS.ProjectSystem
{
    public class CosmosConfigProvider : ConfigProvider
    {
        public CosmosConfigProvider(CosmosProjectNode manager) : base(manager)
        {
            Logger.TraceMethod(MethodBase.GetCurrentMethod());
        }

        protected override ProjectConfig CreateProjectConfiguration(string configName)
        {
            Logger.TraceMethod(MethodBase.GetCurrentMethod());
            return new CosmosProjectConfig(ProjectMgr, configName);
        }
    }
}