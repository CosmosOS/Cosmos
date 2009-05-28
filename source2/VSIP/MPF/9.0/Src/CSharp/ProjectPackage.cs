/// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.Project
{
	/// <summary>
	/// Defines abstract package.
	/// </summary>
	[ComVisible(true)]
	[CLSCompliant(false)]
	public abstract class ProjectPackage : Microsoft.VisualStudio.Shell.Package
	{
		#region fields
		/// <summary>
		/// This is the place to register all the solution listeners.
		/// </summary>
		private List<SolutionListener> solutionListeners = new List<SolutionListener>();

		/// <summary>
		/// Knows about project trust levels for projects
		/// </summary>
		private Dictionary<Guid, ProjectTrustLevel> projectTrustTable = new Dictionary<Guid, ProjectTrustLevel>();

		/// <summary>
		/// Key used for persistence in suo file
		/// </summary>
		private string projectTrustPersistenceKey;
		#endregion

		#region properties
		/// <summary>
		/// Add your listener to this list. They should be added in the overridden Initialize befaore calling the base.
		/// </summary>
		protected internal IList<SolutionListener> SolutionListeners
		{
			get
			{
				return this.solutionListeners;
			}
		}

		/// <summary>
		/// Key used for persistence in suo file.
		/// </summary>
		private string ProjectTrustPersistenceKey
		{
			get
			{
				if(string.IsNullOrEmpty(this.projectTrustPersistenceKey))
				{
					string packageGuid = this.GetType().GUID.ToString("B");
					this.projectTrustPersistenceKey = string.Format(CultureInfo.InvariantCulture, "{0}_projecttrust", packageGuid.Substring(1, 18));
				}
				return this.projectTrustPersistenceKey;
			}
		}
		#endregion

		#region ctor
		protected ProjectPackage()
		{
			this.AddOptionKey(this.ProjectTrustPersistenceKey);
		}
		#endregion

		#region methods
		/// <summary>
		/// Get project trust level for a project instance
		/// </summary>
		/// <param name="projectInstance">the project instance guid</param>
		/// <returns>project trust level</returns>
		/// <exception cref="ArgumentException">project instance guid empty or not known </exception>
		public ProjectTrustLevel GetProjectTrustLevel(Guid projectInstance)
		{
			if(!this.projectTrustTable.ContainsKey(projectInstance))
			{
				return ProjectTrustLevel.Unknown;
			}

			return projectTrustTable[projectInstance]; ;
		}

		/// <summary>
		/// Sets the project trust level for a project.
		/// </summary>
		/// <param name="projectInstance">the project instance guid associated with project where project trus level is set</param>
		/// <param name="projectTrustLevel">the trust level to be assigned to the project</param>
		/// <exception cref="ArgumentException">project instance guid empty</exception>
		public void SetProjectTrustLevel(Guid projectInstance, ProjectTrustLevel projectTrustLevel)
		{
			if(Guid.Empty == projectInstance)
			{
				throw new ArgumentException(SR.GetString(SR.ParameterCannotBeNullOrEmpty, CultureInfo.CurrentUICulture), "projectInstance");
			}

			if(this.projectTrustTable.ContainsKey(projectInstance))
			{
				this.projectTrustTable[projectInstance] = projectTrustLevel;
			}
			else
			{
				this.projectTrustTable.Add(projectInstance, projectTrustLevel);
			}
		}

		/// <summary>
		/// Reads the project trust information. Basically it is invoking the OnLoadOptions method.
		/// This is needed because OnLoadOptions method calls happen later then the project is loaded, and checked for security.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsSolutionPersistence.LoadPackageUserOpts(Microsoft.VisualStudio.Shell.Interop.IVsPersistSolutionOpts,System.String)")]
		internal void ReadProjectTrustInformation()
		{
			this.projectTrustTable.Clear();

			IVsSolutionPersistence persistance = this.GetService(typeof(SVsSolutionPersistence)) as IVsSolutionPersistence;

			if(persistance != null)
			{
				// Read the stream out of the .SUO file.  This will call us back on our ReadUserOptions method.
				persistance.LoadPackageUserOpts(this, this.ProjectTrustPersistenceKey);
			}
		}

		protected override void Initialize()
		{
			base.Initialize();

			// Subscribe to the solution events
			this.solutionListeners.Add(new SolutionListenerForProjectReferenceUpdate(this));
			this.solutionListeners.Add(new SolutionListenerForProjectOpen(this));
			this.solutionListeners.Add(new SolutionListenerForBuildDependencyUpdate(this));
			this.solutionListeners.Add(new SolutionListenerForProjectEvents(this));

			foreach(SolutionListener solutionListener in this.solutionListeners)
			{
				solutionListener.Init();
			}
		}

		protected override void Dispose(bool disposing)
		{
			// Unadvise solution listeners.
			try
			{
				if(disposing)
				{
					foreach(SolutionListener solutionListener in this.solutionListeners)
					{
						solutionListener.Dispose();
					}
				}
			}
			finally
			{

				base.Dispose(disposing);
			}
		}

		/// <summary>
		/// Called by the base package to load solution options.
		/// </summary>
		/// <param name="key">Name of the stream.</param>
		/// <param name="stream">The stream from ehere the pachage should read user specific options.</param>
		protected override void OnLoadOptions(string key, Stream stream)
		{
			// Check if the .suo file is safe, i.e. created on this computer
			// This should really go on the Package.cs
			IVsSolution solution = this.GetService(typeof(SVsSolution)) as IVsSolution;

			if(solution != null)
			{
				object valueAsBool;
				int result = solution.GetProperty((int)__VSPROPID2.VSPROPID_SolutionUserFileCreatedOnThisComputer, out valueAsBool);

				if(ErrorHandler.Failed(result) || !(bool)valueAsBool)
				{
					return;
				}
			}

			if(string.Compare(key, this.ProjectTrustPersistenceKey, StringComparison.OrdinalIgnoreCase) == 0 && stream != null)
			{
				using(BinaryReader reader = new BinaryReader(stream))
				{
					if(reader.BaseStream.Length == 0)
					{
						//No project trust information found
						return;
					}

					int projects = reader.ReadInt32();
					for(int i = 1; i <= projects; i++)
					{
						string projectGuid = reader.ReadString();
						string trustlevel = reader.ReadString();
						this.projectTrustTable.Add(new Guid(projectGuid), (ProjectTrustLevel)Enum.Parse(typeof(ProjectTrustLevel), trustlevel, true));
					}
				}
			}
			else
			{
				base.OnLoadOptions(key, stream);
			}
		}

		/// <summary>
		/// Called by the base package when the solution save the options
		/// </summary>
		/// <param name="key">Name of the stream.</param>
		/// <param name="stream">The stream from ehere the pachage should read user specific options.</param>
		protected override void OnSaveOptions(string key, Stream stream)
		{
			if(string.Compare(key, this.ProjectTrustPersistenceKey, StringComparison.OrdinalIgnoreCase) == 0 && stream != null)
			{
				using(BinaryWriter writer = new BinaryWriter(stream))
				{
					// Write an Int32 for the number of projects in the trust table.
					writer.Write((int)this.projectTrustTable.Count);
					foreach(Guid projectGuid in this.projectTrustTable.Keys)
					{
						writer.Write((string)projectGuid.ToString("B"));
						writer.Write(this.projectTrustTable[projectGuid].ToString());
					}
				}

				this.projectTrustTable.Clear();
			}
			else
			{
				base.OnSaveOptions(key, stream);
			}
		}
		#endregion
	}
}
