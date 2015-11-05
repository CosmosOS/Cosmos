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
