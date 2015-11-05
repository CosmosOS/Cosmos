/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Microsoft.VisualStudio.Project.Automation
{
	/// <summary>
	/// Represents the automation object equivalent to a ReferenceNode object
	/// </summary>
	[SuppressMessage("Microsoft.Interoperability", "CA1405:ComVisibleTypeBaseTypesShouldBeComVisible")]
	[ComVisible(true), CLSCompliant(false)]
	public class OAReferenceItem : OAProjectItem<ReferenceNode>
	{
		#region ctors
		public OAReferenceItem(OAProject project, ReferenceNode node)
			: base(project, node)
		{
		}

		#endregion

		#region overridden methods
		/// <summary>
		/// Not implemented. If called throws invalid operation exception.
		/// </summary>	
		public override void Delete()
		{
			throw new InvalidOperationException();
		}


		/// <summary>
		/// Not implemented. If called throws invalid operation exception.
		/// </summary>
		/// <param name="viewKind"> A Constants. vsViewKind indicating the type of view to use.</param>
		/// <returns></returns>
		public override EnvDTE.Window Open(string viewKind)
		{
			throw new InvalidOperationException();
		}

		/// <summary>
		/// Gets or sets the name of the object.
		/// </summary>
		public override string Name
		{
			get
			{
				return base.Name;
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Gets the ProjectItems collection containing the ProjectItem object supporting this property.
		/// </summary>
		public override EnvDTE.ProjectItems Collection
		{
			get
			{
				// Get the parent node (ReferenceContainerNode)
				ReferenceContainerNode parentNode = this.Node.Parent as ReferenceContainerNode;
				Debug.Assert(parentNode != null, "Failed to get the parent node");

				// Get the ProjectItems object for the parent node
				if(parentNode != null)
				{
					// The root node for the project
					return ((OAReferenceFolderItem)parentNode.GetAutomationObject()).ProjectItems;
				}

				return null;
			}
		}
		#endregion
	}
}
