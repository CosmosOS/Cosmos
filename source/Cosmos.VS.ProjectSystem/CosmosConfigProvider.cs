using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Project;

namespace Cosmos.VS.ProjectSystem
{
    public class CosmosConfigProvider : ConfigProvider
    {

        public CosmosConfigProvider(CosmosProjectNode manager) : base(manager)
        {
        }

        protected override ProjectConfig CreateProjectConfiguration(string configName)
        {
            return new CosmosProjectConfig(ProjectMgr, configName);
        }
    }
}