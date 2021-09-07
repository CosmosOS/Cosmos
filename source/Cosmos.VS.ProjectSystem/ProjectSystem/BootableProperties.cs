using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;

namespace Cosmos.VS.ProjectSystem
{
    [Export(typeof(IBootableProperties))]
    [AppliesTo(ProjectCapability.Bootable)]
    internal class BootableProperties : IBootableProperties
    {
        private ProjectProperties _projectProperties;

        [ImportingConstructor]
        public BootableProperties(ProjectProperties projectProperties)
        {
            _projectProperties = projectProperties;
        }

        public async Task<string> GetBinFileAsync()
        {
            var bootableProperties = await _projectProperties.GetBootableConfigurationPropertiesAsync();
            return await bootableProperties.BinFile.GetEvaluatedValueAtEndAsync();
        }

        public async Task<string> GetIsoFileAsync()
        {
            var bootableProperties = await _projectProperties.GetBootableConfigurationPropertiesAsync();
            return await bootableProperties.IsoFile.GetEvaluatedValueAtEndAsync();
        }
    }
}
