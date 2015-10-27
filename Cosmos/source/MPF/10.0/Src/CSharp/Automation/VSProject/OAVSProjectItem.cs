/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using EnvDTE;
using VSLangProj;

namespace Microsoft.VisualStudio.Project.Automation
{
	/// <summary>
	/// Represents a language-specific project item
	/// </summary>
	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "OAVS")]
	[ComVisible(true), CLSCompliant(false)]
	public class OAVSProjectItem : VSProjectItem
	{
		#region fields
		private FileNode fileNode;
		#endregion

		#region ctors
		public OAVSProjectItem(FileNode fileNode)
		{
			this.FileNode = fileNode;
		}
		#endregion

		#region VSProjectItem Members

		public virtual EnvDTE.Project ContainingProject
		{
			get { return fileNode.ProjectMgr.GetAutomationObject() as EnvDTE.Project; }
		}

		public virtual ProjectItem ProjectItem
		{
			get { return fileNode.GetAutomationObject() as ProjectItem; }
		}

		public virtual DTE DTE
		{
			get { return (DTE)this.fileNode.ProjectMgr.Site.GetService(typeof(DTE)); }
		}

		public virtual void RunCustomTool()
		{
			this.FileNode.RunGenerator();
		}

		#endregion

		#region public properties
		/// <summary>
		/// File Node property
		/// </summary>
		public FileNode FileNode
		{
			get
			{
				return fileNode;
			}
			set
			{
				fileNode = value;
			}
		}
		#endregion

	}
}
