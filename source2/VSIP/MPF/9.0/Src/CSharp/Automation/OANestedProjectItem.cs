/// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.Project.Automation
{
	[SuppressMessage("Microsoft.Interoperability", "CA1405:ComVisibleTypeBaseTypesShouldBeComVisible")]
	[ComVisible(true), CLSCompliant(false)]
	public class OANestedProjectItem : OAProjectItem<NestedProjectNode>
	{
		#region fields
		EnvDTE.Project nestedProject;
		#endregion

		#region ctors
		public OANestedProjectItem(OAProject project, NestedProjectNode node)
			: base(project, node)
		{
			object nestedproject = null;
			if(ErrorHandler.Succeeded(node.NestedHierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out nestedproject)))
			{
				this.nestedProject = nestedproject as EnvDTE.Project;
			}
		}

		#endregion

		#region overridden methods
		/// <summary>
		/// Returns the collection of project items defined in the nested project
		/// </summary>
		public override EnvDTE.ProjectItems ProjectItems
		{
			get
			{
				if(this.nestedProject != null)
				{
					return this.nestedProject.ProjectItems;
				}
				return null;
			}
		}

		/// <summary>
		/// Returns the nested project.
		/// </summary>
		public override EnvDTE.Project SubProject
		{
			get
			{
				return this.nestedProject;
			}
		}
		#endregion
	}
}
