/// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Globalization;
using System.IO;
using Microsoft.VisualStudio.Build.ComInteropWrapper;

namespace Microsoft.VisualStudio.Project
{
	/// <summary>
	/// Does security validation of a user project before loading the project.
	/// </summary>
	public class UserProjectSecurityChecker : ProjectSecurityChecker
	{
		#region fields
		/// <summary>
		/// The project shim for the main project file.
		/// We need this otherwise the msbuild API cannot check the user file.
		/// </summary>
		private ProjectShim mainProjectShim;

		#endregion

		#region ctors
		/// <summary>
		/// Overloaded Constructor 
		/// </summary>
		/// <param name="projectFilePath">path to the project file</param>
		/// <param name="serviceProvider">A service provider.</param>
		public UserProjectSecurityChecker(IServiceProvider serviceProvider, string projectFilePath) :
			base(serviceProvider, projectFilePath)
		{
		}
		#endregion

		#region properties
		/// <summary>
		/// The main projects' shim.
		/// </summary>
		internal protected ProjectShim MainProjectShim
		{
			get
			{
				return this.mainProjectShim;
			}
			internal set
			{
				this.mainProjectShim = value;
			}
		}
		#endregion

		#region overridden method
		/// <summary>
		/// Checks if the user file is safe with imports. If it has then the user file is considered unsafe.
		/// </summary>
		/// <param name="securityErrorMessage">At return describes the reason why the projects is not considered safe.</param>
		/// <returns>true if the user project is safe regarding imports.</returns>
		protected override bool IsProjectSafeWithImports(out string securityErrorMessage)
		{
			securityErrorMessage = String.Empty;

			string[] directImports = this.SecurityCheckHelper.GetDirectlyImportedProjects(this.ProjectShim);

			if(directImports != null && directImports.Length > 0)
			{
				securityErrorMessage = String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.DetailsUserImport, CultureInfo.CurrentUICulture), Path.GetFileName(this.ProjectShim.FullFileName), directImports[0]);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Checks if the project is safe regarding properties.
		/// </summary>
		/// <param name="securityErrorMessage">At return describes the reason why the projects is not considered safe.</param>
		/// <returns>true if the project has only safe properties.</returns>
		protected override bool IsProjectSafeWithProperties(out string securityErrorMessage)
		{
			securityErrorMessage = String.Empty;

			// Now ask the security check heper for the safe properties.
			string reasonForFailure;
			bool isUserFile;
			bool isProjectSafe = this.SecurityCheckHelper.IsProjectSafe(ProjectSecurityChecker.DangerousPropertyProperty,
																		ProjectSecurityChecker.DefaultDangerousProperties,
																		this.mainProjectShim,
																		this.ProjectShim,
																		SecurityCheckPass.Properties,
																		out reasonForFailure,
																		out isUserFile);

			// Main project gets precedence over the user project.
			// Do not report that since this is only for the user file.
			if(!isUserFile)
			{
				return true;
			}

			if(!isProjectSafe)
			{
				securityErrorMessage = this.GetMessageString(reasonForFailure, SR.DetailsProperty);
			}

			return isProjectSafe;
		}

		/// <summary>
		/// Checks if the project is safe regarding targets.
		/// </summary>
		/// <param name="securityErrorMessage">At return describes the reason why the projects is not considered safe.</param>
		/// <returns>true if the project has only safe targets.</returns>
		protected override bool IsProjectSafeWithTargets(out string securityErrorMessage)
		{
			securityErrorMessage = String.Empty;

			// Now ask the security check heper for the safe targets.
			string reasonForFailure;
			bool isUserFile;
			bool isProjectSafe = this.SecurityCheckHelper.IsProjectSafe(ProjectSecurityChecker.DangerousTargetProperty,
																		ProjectSecurityChecker.DefaultDangerousTargets,
																		this.mainProjectShim,
																		this.ProjectShim,
																		SecurityCheckPass.Targets,
																		out reasonForFailure,
																		out isUserFile);

			// Main project gets precedence over the user project.
			// Do not report that since this is only for the user file.
			if(!isUserFile)
			{
				return true;
			}

			if(!isProjectSafe)
			{
				securityErrorMessage = this.GetMessageString(reasonForFailure, SR.DetailsTarget);
			}

			return isProjectSafe;
		}

		/// <summary>
		/// Checks if the project is safe regarding items.
		/// </summary>
		/// <param name="securityErrorMessage">At return describes the reason why the projects is not considered safe.</param>
		/// <returns>true if the project has only safe items.</returns>
		protected override bool IsProjectSafeWithItems(out string securityErrorMessage)
		{
			securityErrorMessage = String.Empty;

			// Now ask the security check heper for the safe items.
			string reasonForFailure;
			bool isUserFile;

			bool isProjectSafe = this.SecurityCheckHelper.IsProjectSafe(ProjectSecurityChecker.DangerousItemsProperty,
																		ProjectSecurityChecker.DefaultDangerousItems,
																		this.mainProjectShim,
																		this.ProjectShim,
																		SecurityCheckPass.Items,
																		out reasonForFailure,
																		out isUserFile);

			// Main project gets precedence over the user project.
			// Do not report that since this is only for the user file.
			if(!isUserFile)
			{
				return true;
			}

			if(!isProjectSafe)
			{
				securityErrorMessage = this.GetMessageString(reasonForFailure, SR.DetailsItem);
			}

			return isProjectSafe;
		}
		#endregion
	}
}
