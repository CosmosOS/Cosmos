/// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using EnvDTE;

namespace Microsoft.VisualStudio.Project.Automation
{
	/// <summary>
	/// Represents an automation object for a folder in a project
	/// </summary>
	[SuppressMessage("Microsoft.Interoperability", "CA1405:ComVisibleTypeBaseTypesShouldBeComVisible")]
	[ComVisible(true), CLSCompliant(false)]
	public class OAFolderItem : OAProjectItem<FolderNode>
	{
		#region ctors
		public OAFolderItem(OAProject project, FolderNode node)
			: base(project, node)
		{
		}

		#endregion

		#region overridden methods
		public override ProjectItems Collection
		{
			get
			{
				ProjectItems items = new OAProjectItems(this.Project, this.Node);
				return items;
			}
		}

		public override ProjectItems ProjectItems
		{
			get
			{
				return this.Collection;
			}
		}
		#endregion
	}
}
