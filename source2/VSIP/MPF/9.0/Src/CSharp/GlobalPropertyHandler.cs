/// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using IServiceProvider = System.IServiceProvider;
using MSBuild = Microsoft.Build.BuildEngine;
using VSRegistry = Microsoft.VisualStudio.Shell.VSRegistry;

namespace Microsoft.VisualStudio.Project
{
	/// <summary>
	/// This class defines and sets the so called global properties that are needed to be provided
	/// before a project builds.
	/// </summary>
	internal class GlobalPropertyHandler : IDisposable
	{
		#region constants
		/// <summary>
		/// The registry relative path entry for finding the fxcop installdir
		/// </summary>
		private const string FxCopRegistryRelativePathEntry = "Setup\\EDev";

		/// <summary>
		/// The registry installation Directory key name.
		/// </summary>
		private const string FxCopRegistryInstallDirKeyName = "FxCopDir";

		/// <summary>
		/// This is the constant that will be set as the value of the VSIDEResolvedNonMSBuildProjectOutputs global property.
		/// </summary>
		private const string VSIDEResolvedNonMSBuildProjectOutputsValue = "<VSIDEResolvedNonMSBuildProjectOutputs></VSIDEResolvedNonMSBuildProjectOutputs>";


		#endregion

		#region fields
		/// <summary>
		/// Raised when the active project configuration for a project in the solution has changed. 
		/// </summary>
		internal event EventHandler<ActiveConfigurationChangedEventArgs> ActiveConfigurationChanged;

		/// <summary>
		/// Defines the global properties of the associated build project.
		/// </summary>
		private MSBuild.BuildPropertyGroup globalProjectProperties;

		/// <summary>
		/// Defines the global properties of the associated build engine.
		/// </summary>
		private MSBuild.BuildPropertyGroup globalEngineProperties;

		/// <summary>
		/// Flag determining if the object has been disposed.
		/// </summary>
		private bool isDisposed;

		/// <summary>
		/// Defines an object that will be a mutex for this object for synchronizing thread calls.
		/// </summary>
		private static volatile object Mutex = new object();

		/// <summary>
		/// Defines the configuration change listener.
		/// </summary>
		private UpdateConfigPropertiesListener configurationChangeListener;
		#endregion

		#region constructors
		/// <summary>
		/// Overloaded constructor.
		/// </summary>
		/// <param name="project">An instance of a build project</param>
		/// <exception cref="ArgumentNullException">Is thrown if the passed Project is null.</exception>
		internal GlobalPropertyHandler(MSBuild.Project project)
		{
			Debug.Assert(project != null, "The project parameter passed cannot be null");

			this.globalProjectProperties = project.GlobalProperties;

			Debug.Assert(project.ParentEngine != null, "The parent engine has not been initialized");

			this.globalEngineProperties = project.ParentEngine.GlobalProperties;
		}
		#endregion

		#region IDisposable Members

		/// <summary>
		/// The IDispose interface Dispose method for disposing the object determinastically.
		/// </summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

		#region methods
		/// <summary>
		/// Initializes MSBuild project properties. This method is called before the first project re-evaluation happens in order to set the global properties.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
		internal virtual void InitializeGlobalProperties()
		{

			// Set the BuildingInsideVisualStudio property to true.
			this.SetGlobalProperty(GlobalProperty.BuildingInsideVisualStudio.ToString(), "true");

			// Set the ResolvedNonMSBuildProjectOutputs property to empty.  This is so that it has some deterministic value, even
			// if it's empty.  This is important because of the way global properties are merged together when one
			// project is calling the <MSBuild> task on another project.  
			this.SetGlobalProperty(GlobalProperty.VSIDEResolvedNonMSBuildProjectOutputs.ToString(), VSIDEResolvedNonMSBuildProjectOutputsValue);

			// Set the RunCodeAnalysisOverride property to false.  This is so that it has some deterministic value.
			// This is important because of the way global properties are merged together when one
			// project is calling the <MSBuild> task on another project. 
			this.SetGlobalProperty(GlobalProperty.RunCodeAnalysisOnce.ToString(), "false");

			// Set Configuration=Debug.  This is a perf optimization, not strictly required for correct functionality.
			// Since most people keep most of their projects with Active Configuration = "Debug" during development,
			// setting this up front makes it faster to load the project.  This way, we don't have to change the
			// value of Configuration down the road, forcing MSBuild to have to re-evaluate the project.
			this.SetGlobalProperty(GlobalProperty.Configuration.ToString(), ProjectConfig.Debug);

			// Set Platform=AnyCPU.  This is a perf optimization, not strictly required for correct functionality.
			// Since most people keep most of their projects with Active Platform = "AnyCPU" during development,
			// setting this up front makes it faster to load the project.  This way, we don't have to change the
			// value of Platform down the road, forcing MSBuild to have to re-evaluate the project.
			this.SetGlobalProperty(GlobalProperty.Platform.ToString(), ProjectConfig.AnyCPU);

			// Set the solution related msbuild global properties.
			this.SetSolutionProperties();

			// Set the VS location global property.
			this.SetGlobalProperty(GlobalProperty.DevEnvDir.ToString(), GetEnvironmentDirectoryLocation());

			// Set the fxcop location global property.
			this.SetGlobalProperty(GlobalProperty.FxCopDir.ToString(), GetFxCopDirectoryLocation());
		}

		/// <summary>
		/// Initializes the internal configuration change listener.
		/// </summary>
		/// <param name="hierarchy">The associated service hierarchy.</param>
		/// <param name="serviceProvider">The associated service provider.</param>
		internal void RegisterConfigurationChangeListener(IVsHierarchy hierarchy, IServiceProvider serviceProvider)
		{
			Debug.Assert(hierarchy != null, "The passed hierarchy cannot be null");
			Debug.Assert(serviceProvider != null, "The passed service provider cannot be null");
			Debug.Assert(this.configurationChangeListener == null, "The configuration change listener has already been initialized");
			this.configurationChangeListener = new UpdateConfigPropertiesListener(this, serviceProvider);
		}

		/// <summary>
		/// The method that does the cleanup.
		/// </summary>
		/// <param name="disposing">true if called from IDispose.Dispose; false if called from Finalizer.</param>
		protected virtual void Dispose(bool disposing)
		{
			// Everybody can go here.
			if(!this.isDisposed)
			{
				// Synchronize calls to the Dispose simultaniously.
				lock(Mutex)
				{
					if(disposing)
					{
						this.configurationChangeListener.Dispose();
					}

					this.isDisposed = true;
				}
			}
		}

		/// <summary>
		/// Called when the active project configuration for a project in the solution has changed. 
		/// </summary>
		/// <param name="hierarchy">The project whose configuration has changed.</param>
		private void RaiseActiveConfigurationChanged(IVsHierarchy hierarchy)
		{
			// Save event in temporary variable to avoid race condition.
			EventHandler<ActiveConfigurationChangedEventArgs> tempEvent = this.ActiveConfigurationChanged;
			if(tempEvent != null)
			{
				tempEvent(this, new ActiveConfigurationChangedEventArgs(hierarchy));
			}
		}

		/// <summary>
		/// Sets the solution related global properties (SolutionName, SolutionFileName, SolutionPath, SolutionDir, SolutionExt).
		/// </summary>
		private void SetSolutionProperties()
		{
			IVsSolution solution = Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(IVsSolution)) as IVsSolution;
			Debug.Assert(solution != null, "Could not retrieve the solution service from the global service provider");

			string solutionDirectory, solutionFile, userOptionsFile;

			// We do not want to throw. If we cannot set the solution related constants we set them to empty string.
			ErrorHandler.ThrowOnFailure(solution.GetSolutionInfo(out solutionDirectory, out solutionFile, out userOptionsFile));

			if(solutionDirectory == null)
			{
				solutionDirectory = String.Empty;
			}

			this.SetGlobalProperty(GlobalProperty.SolutionDir.ToString(), solutionDirectory);

			if(solutionFile == null)
			{
				solutionFile = String.Empty;
			}

			this.SetGlobalProperty(GlobalProperty.SolutionPath.ToString(), solutionFile);

			string solutionFileName = (solutionFile.Length == 0) ? String.Empty : Path.GetFileName(solutionFile);
			this.SetGlobalProperty(GlobalProperty.SolutionFileName.ToString(), solutionFileName);

			string solutionName = (solutionFile.Length == 0) ? String.Empty : Path.GetFileNameWithoutExtension(solutionFile);
			this.SetGlobalProperty(GlobalProperty.SolutionName.ToString(), solutionName);

			string solutionExtension = String.Empty;
			if(solutionFile.Length > 0 && Path.HasExtension(solutionFile))
			{
				solutionExtension = Path.GetExtension(solutionFile);
			}

			this.SetGlobalProperty(GlobalProperty.SolutionExt.ToString(), solutionExtension);
		}

		/// <summary>
		/// Retrieves the Devenv installation directory.
		/// </summary>
		private static string GetEnvironmentDirectoryLocation()
		{
			IVsShell shell = Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(IVsShell)) as IVsShell;
			Debug.Assert(shell != null, "Could not retrieve the IVsShell service from the global service provider");

			object installDirAsObject;

			// We do not want to throw. If we cannot set the solution related constants we set them to empty string.
			ErrorHandler.ThrowOnFailure(shell.GetProperty((int)__VSSPROPID.VSSPROPID_InstallDirectory, out installDirAsObject));

			string installDir = ((string)installDirAsObject);

			if(String.IsNullOrEmpty(installDir))
			{
				return String.Empty;
			}

			// Ensure that we have traimnling backslash as this is done for the langproj macros too.
			if(installDir[installDir.Length - 1] != Path.DirectorySeparatorChar)
			{
				installDir += Path.DirectorySeparatorChar;
			}

			return installDir;
		}


		/// <summary>
		/// Retrieves the fxcop dierctory location
		/// </summary>
		private static string GetFxCopDirectoryLocation()
		{
			using(RegistryKey root = VSRegistry.RegistryRoot(__VsLocalRegistryType.RegType_Configuration))
			{
				if(null == root)
				{
					return String.Empty;
				}

				using(RegistryKey key = root.OpenSubKey(FxCopRegistryRelativePathEntry))
				{
					if(key != null)
					{
						string fxcopInstallDir = key.GetValue(FxCopRegistryInstallDirKeyName) as string;

						return (fxcopInstallDir == null) ? String.Empty : fxcopInstallDir;
					}
				}
			}

			return String.Empty;
		}

		/// <summary>
		/// Sets a global property on the associated build project and build engine.
		/// </summary>
		/// <param name="propertyName">The name of teh property to set.</param>
		/// <param name="propertyValue">Teh value of teh property.</param>
		private void SetGlobalProperty(string propertyName, string propertyValue)
		{
			this.globalProjectProperties.SetProperty(propertyName, propertyValue, true);

			// Set the same global property on the parent Engine object.  The Project
			// object, when it was created, got a clone of the global properties from
			// the engine.  So changing it in the Project doesn't impact the Engine.
			// However, we do need the Engine to have this new global property setting
			// as well, because with project-to-project references, any child projects
			// are going to get their initial global properties from the Engine when
			// they are created.
			this.globalEngineProperties.SetProperty(propertyName, propertyValue, true);
		}
		#endregion

		#region nested types
		/// <summary>
		/// Defines a class that will listen to configuration changes and will update platform and configuration name changes accordingly.
		/// </summary>
		private class UpdateConfigPropertiesListener : UpdateSolutionEventsListener
		{
			#region fields

			/// <summary>
			/// Defines the containing object.
			/// </summary>
			private GlobalPropertyHandler globalPropertyHandler;
			#endregion

			#region constructors
			/// <summary>
			/// Overloaded constructor.
			/// </summary>
			/// <param name="globalProperties"></param>
			/// <param name="associatedHierachy">The associated hierrachy.</param>
			/// <param name="serviceProvider">The associated service provider</param>
			internal UpdateConfigPropertiesListener(GlobalPropertyHandler globalPropertyHandler, IServiceProvider serviceProvider)
				: base(serviceProvider)
			{
				this.globalPropertyHandler = globalPropertyHandler;
			}
			#endregion

			#region methods
			/// <summary>
			/// Called when the active project configuration for a project in the solution has changed. 
			/// </summary>
			/// <param name="hierarchy">The project whose configuration has changed.</param>
			/// <returns>If the method succeeds, it returns S_OK. If it fails, it returns an error code.</returns>
			public override int OnActiveProjectCfgChange(IVsHierarchy hierarchy)
			{
				this.globalPropertyHandler.RaiseActiveConfigurationChanged(hierarchy);
				return VSConstants.S_OK;
			}
			#endregion
		}
		#endregion
	}
}
