/// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using MSBuild = Microsoft.Build.BuildEngine;

namespace Microsoft.VisualStudio.Project
{
	/// <summary>
	/// Creates projects within the solution
	/// </summary>
	[CLSCompliant(false)]
	public abstract class ProjectFactory : Microsoft.VisualStudio.Shell.Flavor.FlavoredProjectFactoryBase
	{
		#region constants
		/// <summary>
		/// The registry key holding the value for the dont show again dialog box.
		/// </summary>
		internal const string DontShowProjectSecurityWarningAgain = "DontShowProjectSecurityWarningAgain";

		/// <summary>
		/// Security warning when opening/creating client project on UNC share.
		/// </summary>
		private const string ProjectFolderFotSecureHelp = "vs.projectfolder_not_secure";
		#endregion

		#region fields
		private Microsoft.VisualStudio.Shell.Package package;
		private System.IServiceProvider site;

		/// <summary>
		/// The msbuild engine that we are going to use.
		/// </summary>
		private MSBuild.Engine buildEngine;

		/// <summary>
		/// The msbuild project for the project file.
		/// </summary>
		private MSBuild.Project buildProject;
		#endregion

		#region properties
		protected Microsoft.VisualStudio.Shell.Package Package
		{
			get
			{
				return this.package;
			}
		}

		protected System.IServiceProvider Site
		{
			get
			{
				return this.site;
			}
		}

		/// <summary>
		/// The msbuild engine that we are going to use.
		/// </summary>
		protected MSBuild.Engine BuildEngine
		{
			get
			{
				return this.buildEngine;
			}
		}

		/// <summary>
		/// The msbuild project for the temporary project file.
		/// </summary>
		protected MSBuild.Project BuildProject
		{
			get
			{
				return this.buildProject;
			}
			set
			{
				this.buildProject = value;
			}
		}
		#endregion

		#region ctor
		protected ProjectFactory(Microsoft.VisualStudio.Shell.Package package)
		{
			this.package = package;
			this.site = package;

			// Please be aware that this methods needs that ServiceProvider is valid, thus the ordering of calls in the ctor matters.
			this.buildEngine = Utilities.InitializeMsBuildEngine(this.buildEngine, this.site);
		}
		#endregion

		#region abstract methods
		protected abstract ProjectNode CreateProject();
		#endregion

		#region overriden methods
		/// <summary>
		/// Rather than directly creating the project, ask VS to initate the process of
		/// creating an aggregated project in case we are flavored. We will be called
		/// on the IVsAggregatableProjectFactory to do the real project creation.
		/// </summary>
		/// <param name="fileName">Project file</param>
		/// <param name="location">Path of the project</param>
		/// <param name="name">Project Name</param>
		/// <param name="flags">Creation flags</param>
		/// <param name="projectGuid">Guid of the project</param>
		/// <param name="project">Project that end up being created by this method</param>
		/// <param name="canceled">Was the project creation canceled</param>
		protected override void CreateProject(string fileName, string location, string name, uint flags, ref Guid projectGuid, out IntPtr project, out int canceled)
		{
			project = IntPtr.Zero;
			canceled = 0;

			// Get the list of GUIDs from the project/template
			string guidsList = this.ProjectTypeGuids(fileName);
			string projectDirectory = String.IsNullOrEmpty(location) ? Path.GetDirectoryName(fileName) : location;
			if(!this.IsProjectLocationSecure(projectDirectory))
			{
				canceled = 1;
				ErrorHandler.ThrowOnFailure(VSConstants.VS_E_WIZARDBACKBUTTONPRESS);
			}

			// Launch the aggregate creation process (we should be called back on our IVsAggregatableProjectFactoryCorrected implementation)
			IVsCreateAggregateProject aggregateProjectFactory = (IVsCreateAggregateProject)this.Site.GetService(typeof(SVsCreateAggregateProject));
			int hr = aggregateProjectFactory.CreateAggregateProject(guidsList, fileName, location, name, flags, ref projectGuid, out project);
			if(hr == VSConstants.E_ABORT)
				canceled = 1;
			ErrorHandler.ThrowOnFailure(hr);

			// This needs to be done after the aggregation is completed (to avoid creating a non-aggregated CCW) and as a result we have to go through the interface
			IProjectEventsProvider eventsProvider = (IProjectEventsProvider)Marshal.GetTypedObjectForIUnknown(project, typeof(IProjectEventsProvider));
			eventsProvider.ProjectEventsProvider = this.GetProjectEventsProvider();

			this.buildProject = null;
		}


		/// <summary>
		/// Instantiate the project class, but do not proceed with the
		/// initialization just yet.
		/// Delegate to CreateProject implemented by the derived class.
		/// </summary>
		protected override object PreCreateForOuter(IntPtr outerProjectIUnknown)
		{
			Debug.Assert(this.buildProject != null, "The build project should have been initialized before calling PreCreateForOuter.");
			GlobalPropertyHandler globalPropertyHandler = new GlobalPropertyHandler(this.buildProject);
			globalPropertyHandler.InitializeGlobalProperties();

			// Please be very carefull what is initialized here on the ProjectNode. Normally this should only instantiate and return a project node.
			// The reason why one should very carefully add state to the project node here is that at this point the aggregation has not yet been created and anything that would cause a CCW for the project to be created would cause the aggregation to fail
			// Our reasoning is that there is no other place where state on the project node can be set that is known by the Factory and has to execute before the Load method.
			ProjectNode node = this.CreateProject();
			Debug.Assert(node != null, "The project failed to be created");
			node.BuildEngine = this.buildEngine;
			node.BuildProject = this.buildProject;
			node.GlobalPropetyHandler = globalPropertyHandler;
			globalPropertyHandler.RegisterConfigurationChangeListener(node, this.site);
			node.Package = this.package as ProjectPackage;
			return node;
		}

		/// <summary>
		/// Retrives the list of project guids from the project file.
		/// If you don't want your project to be flavorable, override
		/// to only return your project factory Guid:
		///      return this.GetType().GUID.ToString("B");
		/// </summary>
		/// <param name="file">Project file to look into to find the Guid list</param>
		/// <returns>List of semi-colon separated GUIDs</returns>
		protected override string ProjectTypeGuids(string file)
		{
			// Load the project so we can extract the list of GUIDs

			this.buildProject = Utilities.ReinitializeMsBuildProject(this.buildEngine, file, this.buildProject);

			// Retrieve the list of GUIDs, if it is not specify, make it our GUID
			string guids = buildProject.GetEvaluatedProperty(ProjectFileConstants.ProjectTypeGuids);
			if(String.IsNullOrEmpty(guids))
				guids = this.GetType().GUID.ToString("B");

			return guids;
		}
		#endregion

		#region helpers
		private IProjectEvents GetProjectEventsProvider()
		{
			ProjectPackage projectPackage = this.package as ProjectPackage;
			Debug.Assert(projectPackage != null, "Package not inherited from framework");
			if(projectPackage != null)
			{
				foreach(SolutionListener listener in projectPackage.SolutionListeners)
				{
					IProjectEvents projectEvents = listener as IProjectEvents;
					if(projectEvents != null)
					{
						return projectEvents;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Checks if the project location is secure. If it is not then it will launch a dialog box prompting the user to acknowledge that the project location is not secure.
		/// </summary>
		/// <param name="location">The location to check</param>
		/// <returns>true if it is secure, false otherwise.</returns>
		private bool IsProjectLocationSecure(string location)
		{
			if(!Utilities.IsProjectLocationSecure(location))
			{
				if(Utilities.IsShellInCommandLineMode(this.Site) || Utilities.IsInAutomationFunction(this.Site))
				{
					return false;
				}

				string errorMessage = String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.ProjectLocationNotTrusted, CultureInfo.CurrentUICulture), Environment.NewLine, Path.GetDirectoryName(location));
				return (DontShowAgainDialog.LaunchDontShowAgainDialog(this.Site, errorMessage, ProjectFolderFotSecureHelp, DontShowAgainDialog.DefaultButton.OK, DontShowProjectSecurityWarningAgain) == DialogResult.OK);
			}

			return true;
		}
		#endregion
	}
}
