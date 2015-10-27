/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using VSRegistry = Microsoft.VisualStudio.Shell.VSRegistry;

namespace Microsoft.VisualStudio.Project
{
	/// <summary>
	/// Gets registry settings from for a project.
	/// </summary>
	internal class RegisteredProjectType
	{
		private string defaultProjectExtension;

		private string projectTemplatesDir;

		private string wizardTemplatesDir;

		private Guid packageGuid;

		internal const string DefaultProjectExtension = "DefaultProjectExtension";
		internal const string WizardsTemplatesDir = "WizardsTemplatesDir";
		internal const string ProjectTemplatesDir = "ProjectTemplatesDir";
		internal const string Package = "Package";



		internal string DefaultProjectExtensionValue
		{
			get
			{
				return this.defaultProjectExtension;
			}
			set
			{
				this.defaultProjectExtension = value;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		internal string ProjectTemplatesDirValue
		{
			get
			{
				return this.projectTemplatesDir;
			}
			set
			{
				this.projectTemplatesDir = value;
			}
		}

		internal string WizardTemplatesDirValue
		{
			get
			{
				return this.wizardTemplatesDir;
			}
			set
			{
				this.wizardTemplatesDir = value;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		internal Guid PackageGuidValue
		{
			get
			{
				return this.packageGuid;
			}
			set
			{
				this.packageGuid = value;
			}
		}

		/// <summary>
		/// If the project support VsTemplates, returns the path to
		/// the vstemplate file corresponding to the requested template
		/// 
		/// You can pass in a string such as: "Windows\Console Application"
		/// </summary>
		internal string GetVsTemplateFile(string templateFile)
		{
			// First see if this use the vstemplate model
			if(!String.IsNullOrEmpty(DefaultProjectExtensionValue))
			{
				EnvDTE80.DTE2 dte = Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(EnvDTE.DTE)) as EnvDTE80.DTE2;
				if(dte != null)
				{
					EnvDTE80.Solution2 solution = dte.Solution as EnvDTE80.Solution2;
					if(solution != null)
					{
						string fullPath = solution.GetProjectTemplate(templateFile, DefaultProjectExtensionValue);
						// The path returned by GetProjectTemplate can be in the format "path|FrameworkVersion=x.y|Language=xxx"
						// where the framework version and language sections are optional.
						// Here we are interested only in the full path, so we have to remove all the other sections.
						int pipePos = fullPath.IndexOf('|');
						if(0 == pipePos)
						{
							return null;
						}
						if(pipePos > 0)
						{
							fullPath = fullPath.Substring(0, pipePos);
						}
						return fullPath;
					}
				}

			}
			return null;
		}

		internal static RegisteredProjectType CreateRegisteredProjectType(Guid projectTypeGuid)
		{
			RegisteredProjectType registederedProjectType = null;

			using(RegistryKey rootKey = VSRegistry.RegistryRoot(__VsLocalRegistryType.RegType_Configuration))
			{
				if(rootKey == null)
				{
					return null;
				}

				string projectPath = "Projects\\" + projectTypeGuid.ToString("B");
				using(RegistryKey projectKey = rootKey.OpenSubKey(projectPath))
				{
					if(projectKey == null)
					{
						return null;
					}

					registederedProjectType = new RegisteredProjectType();
					registederedProjectType.DefaultProjectExtensionValue = projectKey.GetValue(DefaultProjectExtension) as string;
					registederedProjectType.ProjectTemplatesDirValue = projectKey.GetValue(ProjectTemplatesDir) as string;
					registederedProjectType.WizardTemplatesDirValue = projectKey.GetValue(WizardsTemplatesDir) as string;
					registederedProjectType.PackageGuidValue = new Guid(projectKey.GetValue(Package) as string);
				}
			}

			return registederedProjectType;
		}
	}
}
