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
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;
using VsCommands2K = Microsoft.VisualStudio.VSConstants.VSStd2KCmdID;

namespace Microsoft.VisualStudio.Project
{
	[CLSCompliant(false), ComVisible(true)]
	public abstract class ReferenceNode : HierarchyNode
	{
		protected delegate void CannotAddReferenceErrorMessage();

		#region ctors
		/// <summary>
		/// constructor for the ReferenceNode
		/// </summary>
		protected ReferenceNode(ProjectNode root, ProjectElement element)
			: base(root, element)
		{
			this.ExcludeNodeFromScc = true;
		}

		/// <summary>
		/// constructor for the ReferenceNode
		/// </summary>
		protected ReferenceNode(ProjectNode root)
			: base(root)
		{
			this.ExcludeNodeFromScc = true;
		}

		#endregion

		#region overridden properties
		public override int MenuCommandId
		{
			get { return VsMenus.IDM_VS_CTXT_REFERENCE; }
		}

		public override Guid ItemTypeGuid
		{
			get { return Guid.Empty; }
		}

		public override string Url
		{
			get
			{
				return String.Empty;
			}
		}

		public override string Caption
		{
			get
			{
				return String.Empty;
			}
		}
		#endregion

		#region overridden methods
		protected override NodeProperties CreatePropertiesObject()
		{
			return new ReferenceNodeProperties(this);
		}

		/// <summary>
		/// Get an instance of the automation object for ReferenceNode
		/// </summary>
		/// <returns>An instance of Automation.OAReferenceItem type if succeeded</returns>
		public override object GetAutomationObject()
		{
			if(this.ProjectMgr == null || this.ProjectMgr.IsClosed)
			{
				return null;
			}

			return new Automation.OAReferenceItem(this.ProjectMgr.GetAutomationObject() as Automation.OAProject, this);
		}

		/// <summary>
		/// Disable inline editing of Caption of a ReferendeNode
		/// </summary>
		/// <returns>null</returns>
		public override string GetEditLabel()
		{
			return null;
		}


		public override object GetIconHandle(bool open)
		{
			int offset = (this.CanShowDefaultIcon() ? (int)ProjectNode.ImageName.Reference : (int)ProjectNode.ImageName.DanglingReference);
			return this.ProjectMgr.ImageHandler.GetIconHandle(offset);
		}

		/// <summary>
		/// This method is called by the interface method GetMkDocument to specify the item moniker.
		/// </summary>
		/// <returns>The moniker for this item</returns>
		public override string GetMkDocument()
		{
			return this.Url;
		}

		/// <summary>
		/// Not supported.
		/// </summary>
		protected override int ExcludeFromProject()
		{
			return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
		}

		/// <summary>
		/// References node cannot be dragged.
		/// </summary>
		/// <returns>A stringbuilder.</returns>
		protected internal override StringBuilder PrepareSelectedNodesForClipBoard()
		{
			return null;
		}

		protected override int QueryStatusOnNode(Guid cmdGroup, uint cmd, IntPtr pCmdText, ref QueryStatusResult result)
		{
			if(cmdGroup == VsMenus.guidStandardCommandSet2K)
			{
				if((VsCommands2K)cmd == VsCommands2K.QUICKOBJECTSEARCH)
				{
					result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
					return VSConstants.S_OK;
				}
			}
			else
			{
				return (int)OleConstants.OLECMDERR_E_UNKNOWNGROUP;
			}
			return base.QueryStatusOnNode(cmdGroup, cmd, pCmdText, ref result);
		}

		protected override int ExecCommandOnNode(Guid cmdGroup, uint cmd, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
		{
			if(cmdGroup == VsMenus.guidStandardCommandSet2K)
			{
				if((VsCommands2K)cmd == VsCommands2K.QUICKOBJECTSEARCH)
				{
					return this.ShowObjectBrowser();
				}
			}

			return base.ExecCommandOnNode(cmdGroup, cmd, nCmdexecopt, pvaIn, pvaOut);
		}

		#endregion

		#region  methods


		/// <summary>
		/// Links a reference node to the project and hierarchy.
		/// </summary>
		public virtual void AddReference()
		{
			ReferenceContainerNode referencesFolder = this.ProjectMgr.FindChild(ReferenceContainerNode.ReferencesNodeVirtualName) as ReferenceContainerNode;
			Debug.Assert(referencesFolder != null, "Could not find the References node");

			CannotAddReferenceErrorMessage referenceErrorMessageHandler = null;

			if(!this.CanAddReference(out referenceErrorMessageHandler))
			{
				if(referenceErrorMessageHandler != null)
				{
					referenceErrorMessageHandler.DynamicInvoke(new object[] { });
				}
				return;
			}

			// Link the node to the project file.
			this.BindReferenceData();

			// At this point force the item to be refreshed
			this.ItemNode.RefreshProperties();

			referencesFolder.AddChild(this);

			return;
		}

		/// <summary>
		/// Refreshes a reference by re-resolving it and redrawing the icon.
		/// </summary>
		internal virtual void RefreshReference()
		{
			this.ResolveReference();
			this.ReDraw(UIHierarchyElement.Icon);
		}

		/// <summary>
		/// Resolves references.
		/// </summary>
		protected virtual void ResolveReference()
		{

		}

		/// <summary>
		/// Validates that a reference can be added.
		/// </summary>
		/// <param name="errorHandler">A CannotAddReferenceErrorMessage delegate to show the error message.</param>
		/// <returns>true if the reference can be added.</returns>
		protected virtual bool CanAddReference(out CannotAddReferenceErrorMessage errorHandler)
		{
			// When this method is called this refererence has not yet been added to the hierarchy, only instantiated.
			errorHandler = null;
			if(this.IsAlreadyAdded())
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Checks if a reference is already added. The method parses all references and compares the Url.
		/// </summary>
		/// <returns>true if the assembly has already been added.</returns>
		protected bool IsAlreadyAdded()
		{
			ReferenceNode existingReference;
			return IsAlreadyAdded(out existingReference);
		}

		/// <summary>
		/// Checks if a reference is already added. The method parses all references and compares the Url.
		/// </summary>
		/// <param name="existingEquivalentNode">The existing reference, if one is found.</param>
		/// <returns>true if the assembly has already been added.</returns>
		protected internal virtual bool IsAlreadyAdded(out ReferenceNode existingEquivalentNode)
		{
			ReferenceContainerNode referencesFolder = this.ProjectMgr.FindChild(ReferenceContainerNode.ReferencesNodeVirtualName) as ReferenceContainerNode;
			Debug.Assert(referencesFolder != null, "Could not find the References node");

			for(HierarchyNode n = referencesFolder.FirstChild; n != null; n = n.NextSibling)
			{
				ReferenceNode referenceNode = n as ReferenceNode;
				if(null != referenceNode)
				{
					// We check if the Url of the assemblies is the same.
					if(NativeMethods.IsSamePath(referenceNode.Url, this.Url))
					{
						existingEquivalentNode = referenceNode;
						return true;
					}
				}
			}

			existingEquivalentNode = null;
			return false;
		}


		/// <summary>
		/// Shows the Object Browser
		/// </summary>
		/// <returns></returns>
		protected virtual int ShowObjectBrowser()
		{
			if(String.IsNullOrEmpty(this.Url) || !File.Exists(this.Url))
			{
				return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
			}

			// Request unmanaged code permission in order to be able to creaet the unmanaged memory representing the guid.
			new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();

			Guid guid = VSConstants.guidCOMPLUSLibrary;
			IntPtr ptr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(guid.ToByteArray().Length);

			System.Runtime.InteropServices.Marshal.StructureToPtr(guid, ptr, false);
			int returnValue = VSConstants.S_OK;
			try
			{
				VSOBJECTINFO[] objInfo = new VSOBJECTINFO[1];

				objInfo[0].pguidLib = ptr;
				objInfo[0].pszLibName = this.Url;

				IVsObjBrowser objBrowser = this.ProjectMgr.Site.GetService(typeof(SVsObjBrowser)) as IVsObjBrowser;

				ErrorHandler.ThrowOnFailure(objBrowser.NavigateTo(objInfo, 0));
			}
			catch(COMException e)
			{
				Trace.WriteLine("Exception" + e.ErrorCode);
				returnValue = e.ErrorCode;
			}
			finally
			{
				if(ptr != IntPtr.Zero)
				{
					System.Runtime.InteropServices.Marshal.FreeCoTaskMem(ptr);
				}
			}

			return returnValue;
		}

		protected override bool CanDeleteItem(__VSDELETEITEMOPERATION deleteOperation)
		{
			if(deleteOperation == __VSDELETEITEMOPERATION.DELITEMOP_RemoveFromProject)
			{
				return true;
			}
			return false;
		}

		protected abstract void BindReferenceData();

		#endregion
	}
}
