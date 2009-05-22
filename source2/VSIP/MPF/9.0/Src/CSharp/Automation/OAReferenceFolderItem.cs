/// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Microsoft.VisualStudio.Project.Automation
{
	/// <summary>
	/// Contains OAReferenceItem objects 
	/// </summary>
	[SuppressMessage("Microsoft.Interoperability", "CA1405:ComVisibleTypeBaseTypesShouldBeComVisible")]
	[ComVisible(true), CLSCompliant(false)]
	public class OAReferenceFolderItem : OAProjectItem<ReferenceContainerNode>
	{
		#region ctors
		public OAReferenceFolderItem(OAProject project, ReferenceContainerNode node)
			: base(project, node)
		{
		}

		#endregion

		#region overridden methods
		/// <summary>
		/// Returns the project items collection of all the references defined for this project.
		/// </summary>
		public override EnvDTE.ProjectItems ProjectItems
		{
			get
			{
				return new OANavigableProjectItems(this.Project, this.GetListOfProjectItems(), this.Node);
			}
		}


		#endregion

		#region Helper methods
		private List<EnvDTE.ProjectItem> GetListOfProjectItems()
		{
			List<EnvDTE.ProjectItem> list = new List<EnvDTE.ProjectItem>();
			for(HierarchyNode child = this.Node.FirstChild; child != null; child = child.NextSibling)
			{
				ReferenceNode node = child as ReferenceNode;

				if(node != null)
				{
					list.Add(new OAReferenceItem(this.Project, node));
				}
			}

			return list;
		}
		#endregion
	}
}
