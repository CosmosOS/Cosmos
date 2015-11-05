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
using System.Text;
using Microsoft.VisualStudio;
using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;
using VsCommands = Microsoft.VisualStudio.VSConstants.VSStd97CmdID;
using VsCommands2K = Microsoft.VisualStudio.VSConstants.VSStd2KCmdID;

namespace Microsoft.VisualStudio.Project
{
	/// <summary>
	/// Defines the logic for all dependent file nodes (solution explorer icon, commands etc.)
	/// </summary>
	[CLSCompliant(false)]
	[ComVisible(true)]
	public class DependentFileNode : FileNode
	{
		#region fields
		/// <summary>
		/// Defines if the node has a name relation to its parent node
		/// e.g. Form1.ext and Form1.resx are name related (until first occurence of extention separator)
		/// </summary>
		#endregion

		#region Properties
		public override int ImageIndex
		{
			get { return (this.CanShowDefaultIcon() ? (int)ProjectNode.ImageName.DependentFile : (int)ProjectNode.ImageName.MissingFile); }
		}
		#endregion

		#region ctor
		/// <summary>
		/// Constructor for the DependentFileNode
		/// </summary>
		/// <param name="root">Root of the hierarchy</param>
		/// <param name="e">Associated project element</param>
		public DependentFileNode(ProjectNode root, ProjectElement element)
			: base(root, element)
		{
			this.HasParentNodeNameRelation = false;
		}


		#endregion

		#region overridden methods
		/// <summary>
		/// Disable rename
		/// </summary>
		/// <param name="label">new label</param>
		/// <returns>E_NOTIMPLE in order to tell the call that we do not support rename</returns>
		public override string GetEditLabel()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets a handle to the icon that should be set for this node
		/// </summary>
		/// <param name="open">Whether the folder is open, ignored here.</param>
		/// <returns>Handle to icon for the node</returns>
		public override object GetIconHandle(bool open)
		{
			return this.ProjectMgr.ImageHandler.GetIconHandle(this.ImageIndex);
		}

		/// <summary>
		/// Disable certain commands for dependent file nodes 
		/// </summary>
		protected override int QueryStatusOnNode(Guid cmdGroup, uint cmd, IntPtr pCmdText, ref QueryStatusResult result)
		{
			if(cmdGroup == VsMenus.guidStandardCommandSet97)
			{
				switch((VsCommands)cmd)
				{
					case VsCommands.Copy:
					case VsCommands.Paste:
					case VsCommands.Cut:
					case VsCommands.Rename:
						result |= QueryStatusResult.NOTSUPPORTED;
						return VSConstants.S_OK;

					case VsCommands.ViewCode:
					case VsCommands.Open:
					case VsCommands.OpenWith:
						result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
						return VSConstants.S_OK;
				}
			}
			else if(cmdGroup == VsMenus.guidStandardCommandSet2K)
			{
				if((VsCommands2K)cmd == VsCommands2K.EXCLUDEFROMPROJECT)
				{
					result |= QueryStatusResult.NOTSUPPORTED;
					return VSConstants.S_OK;
				}
			}
			else
			{
				return (int)OleConstants.OLECMDERR_E_UNKNOWNGROUP;
			}
			return base.QueryStatusOnNode(cmdGroup, cmd, pCmdText, ref result);
		}

		/// <summary>
		/// DependentFileNodes node cannot be dragged.
		/// </summary>
		/// <returns>null</returns>
		protected internal override StringBuilder PrepareSelectedNodesForClipBoard()
		{
			return null;
		}

		protected override NodeProperties CreatePropertiesObject()
		{
			return new DependentFileNodeProperties(this);
		}

		/// <summary>
		/// Redraws the state icon if the node is not excluded from source control.
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Scc")]
		protected internal override void UpdateSccStateIcons()
		{
			if(!this.ExcludeNodeFromScc)
			{
				this.Parent.ReDraw(UIHierarchyElement.SccState);
			}
		}
		#endregion

	}
}
