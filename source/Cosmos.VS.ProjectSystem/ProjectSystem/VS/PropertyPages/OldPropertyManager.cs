using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;

using VSPropertyPages;

using Cosmos.Build.Common;

namespace Cosmos.VS.ProjectSystem.VS.PropertyPages
{
    internal class OldPropertyManager : IPropertyManager
    {
        private BuildProperties mBuildProperties;

        public BuildProperties BuildProperties => mBuildProperties;

        public string ProjectPath => mUnconfiguredProject.FullPath;

        private UnconfiguredProject mUnconfiguredProject;
        private IProjectLockService mProjectLockService;

        private bool mIsDirty;

        public event EventHandler<ProjectPropertyChangedEventArgs> PropertyChanged;
        public event EventHandler<ProjectPropertyChangingEventArgs> PropertyChanging;
        public event EventHandler ConfigurationsChanged;

        public OldPropertyManager(UnconfiguredProject unconfiguredProject)
        {
            mBuildProperties = new BuildProperties();

            mBuildProperties.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                PropertyChanged?.Invoke(this, new ProjectPropertyChangedEventArgs(e.PropertyName, e.OldValue, e.NewValue));
            };

            mBuildProperties.PropertyChanging += delegate (object sender, PropertyChangingEventArgs e)
            {
                PropertyChanging?.Invoke(this, new ProjectPropertyChangingEventArgs(e.PropertyName));
            };

            mUnconfiguredProject = unconfiguredProject;
            mProjectLockService = mUnconfiguredProject.ProjectService.Services.ProjectLockService;
        }

        public async Task<string> GetPathPropertyAsync(string propertyName, bool isRelative)
        {
            var property = await GetPropertyAsync(propertyName);

            if (isRelative)
            {
                return mUnconfiguredProject.MakeRelative(property);
            }
            else
            {
                return mUnconfiguredProject.MakeRooted(property);
            }
        }

        public Task<string> GetPropertyAsync(string propertyName) =>
            Task.FromResult(mBuildProperties.GetProperty(propertyName));

        public async Task SetPathPropertyAsync(string propertyName, string value, bool isRelative)
        {
            if (isRelative)
            {
                await SetPropertyAsync(propertyName, mUnconfiguredProject.MakeRelative(value));
            }
            else
            {
                await SetPropertyAsync(propertyName, mUnconfiguredProject.MakeRooted(value));
            }
        }

        public Task SetPropertyAsync(string propertyName, string value)
        {
            PropertyChanging?.Invoke(this, new ProjectPropertyChangingEventArgs(propertyName));

            var oldValue = mBuildProperties.GetProperty(propertyName);
            mBuildProperties.SetProperty(propertyName, value);

            PropertyChanged?.Invoke(this, new ProjectPropertyChangedEventArgs(
                propertyName, oldValue, mBuildProperties.GetProperty(propertyName)));

            mIsDirty = true;

            return Task.CompletedTask;
        }

        public Task<bool> IsDirtyAsync() => Task.FromResult(BuildProperties.IsDirty || mIsDirty);

        public async Task<bool> ApplyAsync()
        {
            using (var projectWriteLock = await mProjectLockService.WriteLockAsync())
            {
                var configuredProject = await mUnconfiguredProject.GetSuggestedConfiguredProjectAsync();

                var project = await projectWriteLock.GetProjectAsync(configuredProject);
                await projectWriteLock.CheckoutAsync(mUnconfiguredProject.FullPath);

                foreach (var property in mBuildProperties.GetProperties())
                {
                    project.SetProperty(property.Key, property.Value);
                }

                project.Save();
            }

            return true;
        }

        public async Task LoadPropertiesAsync()
        {
            using (var projectReadLock = await mProjectLockService.ReadLockAsync())
            {
                var configuredProject = await mUnconfiguredProject.GetSuggestedConfiguredProjectAsync();
                var project = await projectReadLock.GetProjectAsync(configuredProject);

                await mUnconfiguredProject.ProjectService.Services.ThreadingPolicy.SwitchToUIThread();

                foreach (var propertyName in BuildProperties.ProjectIndependentProperties)
                {
                    var propertyValue = project.GetPropertyValue(propertyName);
                    BuildProperties.SetProperty(propertyName, propertyValue);
                }
            }
        }

        public async Task<string> GetProjectPropertyAsync(string propertyName)
        {
            using (var projectReadLock = await mProjectLockService.ReadLockAsync())
            {
                var configuredProject = await mUnconfiguredProject.GetSuggestedConfiguredProjectAsync();
                var project = await projectReadLock.GetProjectAsync(configuredProject);

                return project.GetPropertyValue(propertyName);
            }
        }

        public Task UpdateConfigurationsAsync(IReadOnlyCollection<ConfiguredProject> configuredProjects) => Task.CompletedTask;
    }
}
