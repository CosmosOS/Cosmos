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
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Microsoft.VisualStudio.Project
{
	class ProjectDesignerDocumentManager : DocumentManager
	{
		#region ctors
		public ProjectDesignerDocumentManager(ProjectNode node)
			: base(node)
		{
		}
		#endregion

		#region overriden methods

		public override int Open(ref Guid logicalView, IntPtr docDataExisting, out IVsWindowFrame windowFrame, WindowFrameShowAction windowFrameAction)
		{
			Guid editorGuid = VSConstants.GUID_ProjectDesignerEditor;
			return this.OpenWithSpecific(0, ref editorGuid, String.Empty, ref logicalView, docDataExisting, out windowFrame, windowFrameAction);
		}

		public override int OpenWithSpecific(uint editorFlags, ref Guid editorType, string physicalView, ref Guid logicalView, IntPtr docDataExisting, out IVsWindowFrame frame, WindowFrameShowAction windowFrameAction)
		{
			frame = null;
			Debug.Assert(editorType == VSConstants.GUID_ProjectDesignerEditor, "Cannot open project designer with guid " + editorType.ToString());


			if(this.Node == null || this.Node.ProjectMgr == null || this.Node.ProjectMgr.IsClosed)
			{
				return VSConstants.E_FAIL;
			}

			IVsUIShellOpenDocument uiShellOpenDocument = this.Node.ProjectMgr.Site.GetService(typeof(SVsUIShellOpenDocument)) as IVsUIShellOpenDocument;
			IOleServiceProvider serviceProvider = this.Node.ProjectMgr.Site.GetService(typeof(IOleServiceProvider)) as IOleServiceProvider;

			if(serviceProvider != null && uiShellOpenDocument != null)
			{
				string fullPath = this.GetFullPathForDocument();
				string caption = this.GetOwnerCaption();

				IVsUIHierarchy parentHierarchy = this.Node.ProjectMgr.GetProperty((int)__VSHPROPID.VSHPROPID_ParentHierarchy) as IVsUIHierarchy;

                int parentHierarchyItemId = (int)this.Node.ProjectMgr.GetProperty((int)__VSHPROPID.VSHPROPID_ParentHierarchyItemid);

                ErrorHandler.ThrowOnFailure(uiShellOpenDocument.OpenSpecificEditor(editorFlags, fullPath, ref editorType, physicalView, ref logicalView, caption, parentHierarchy, (uint)parentHierarchyItemId, docDataExisting, serviceProvider, out frame));

				if(frame != null)
				{
					if(windowFrameAction == WindowFrameShowAction.Show)
					{
						ErrorHandler.ThrowOnFailure(frame.Show());
					}
				}
			}

			return VSConstants.S_OK;
		}
		#endregion

	}
}
