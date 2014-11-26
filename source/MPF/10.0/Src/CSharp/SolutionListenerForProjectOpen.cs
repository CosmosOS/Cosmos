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
using IServiceProvider = System.IServiceProvider;

namespace Microsoft.VisualStudio.Project
{

	[CLSCompliant(false)]
	public class SolutionListenerForProjectOpen : SolutionListener
	{
		public SolutionListenerForProjectOpen(IServiceProvider serviceProvider)
			: base(serviceProvider)
		{
		}

		public override int OnAfterOpenProject(IVsHierarchy hierarchy, int added)
		{
			// If this is a new project and our project. We use here that it is only our project that will implemnet the "internal"  IBuildDependencyOnProjectContainer.
			if(added != 0 && hierarchy is IBuildDependencyUpdate)
			{
				IVsUIHierarchy uiHierarchy = hierarchy as IVsUIHierarchy;
				Debug.Assert(uiHierarchy != null, "The ProjectNode should implement IVsUIHierarchy");
				// Expand and select project node
				IVsUIHierarchyWindow uiWindow = UIHierarchyUtilities.GetUIHierarchyWindow(this.ServiceProvider, HierarchyNode.SolutionExplorer);
				if(uiWindow != null)
				{
					__VSHIERARCHYITEMSTATE state;
					uint stateAsInt;
					if(uiWindow.GetItemState(uiHierarchy, VSConstants.VSITEMID_ROOT, (uint)__VSHIERARCHYITEMSTATE.HIS_Expanded, out stateAsInt) == VSConstants.S_OK)
					{
						state = (__VSHIERARCHYITEMSTATE)stateAsInt;
						if(state != __VSHIERARCHYITEMSTATE.HIS_Expanded)
						{
							int hr;
							hr = uiWindow.ExpandItem(uiHierarchy, VSConstants.VSITEMID_ROOT, EXPANDFLAGS.EXPF_ExpandParentsToShowItem);
							if(ErrorHandler.Failed(hr))
								Trace.WriteLine("Failed to expand project node");
							hr = uiWindow.ExpandItem(uiHierarchy, VSConstants.VSITEMID_ROOT, EXPANDFLAGS.EXPF_SelectItem);
							if(ErrorHandler.Failed(hr))
								Trace.WriteLine("Failed to select project node");

							return hr;
						}
					}
				}
			}
			return VSConstants.S_OK;
		}
	}
}
