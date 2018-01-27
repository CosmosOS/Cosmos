using Microsoft.VisualStudio.ProjectSystem;

using VSPropertyPages;

using Cosmos.Build.Common;

namespace Cosmos.VS.ProjectSystem.VS.PropertyPages
{
    internal class CosmosPropertyPageViewModel : PropertyPageViewModel
    {
        private OldPropertyManager mPropertyManager;
        private IProjectThreadingService mProjectThreadingService;

        public BuildProperties BuildProperties => mPropertyManager.BuildProperties;

        public CosmosPropertyPageViewModel(
            OldPropertyManager propertyManager,
            IProjectThreadingService projectThreadingService)
            : base(propertyManager, projectThreadingService)
        {
            mPropertyManager = propertyManager;
            mProjectThreadingService = projectThreadingService;
        }

        public string ProjectPath => mPropertyManager.ProjectPath;

        public void LoadProjectProperties()
        {
            mProjectThreadingService.ExecuteSynchronously(() => mPropertyManager.LoadPropertiesAsync());
        }

        public void LoadProfile()
        {
            BuildProperties.Profile = mProjectThreadingService.ExecuteSynchronously(
                () => mPropertyManager.GetProjectPropertyAsync(BuildPropertyNames.ProfileString));
        }

        public string GetProjectProperty(string propertyName)
        {
            return mProjectThreadingService.ExecuteSynchronously(() => mPropertyManager.GetProjectPropertyAsync(propertyName));
        }
    }
}
