/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Microsoft.VisualStudio.Project
{
	/// <summary>
	/// Defines the config dependent properties exposed through automation
	/// </summary>
	[ComVisible(true)]
	[Guid("21f73a8f-91d7-4085-9d4f-c48ee235ee5b")]
	public interface IProjectConfigProperties
	{
		string OutputPath { get; set; }
	}

	/// <summary>
	/// Implements the configuration dependent properties interface
	/// </summary>
	[CLSCompliant(false), ComVisible(true)]
	[ClassInterface(ClassInterfaceType.None)]
	public class ProjectConfigProperties : IProjectConfigProperties
	{
		#region fields
		private ProjectConfig projectConfig;
		#endregion

		#region ctors
		public ProjectConfigProperties(ProjectConfig projectConfig)
		{
			this.projectConfig = projectConfig;
		}
		#endregion

		#region IProjectConfigProperties Members

		public virtual string OutputPath
		{
			get
			{
				return this.projectConfig.GetConfigurationProperty(BuildPropertyPageTag.OutputPath.ToString(), true);
			}
			set
			{
				this.projectConfig.SetConfigurationProperty(BuildPropertyPageTag.OutputPath.ToString(), value);
			}
		}

		#endregion
	}
}
