/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.Project
{
	/// <summary>
	/// Used for adding a build dependency to nested project (not a real project reference)
	/// </summary>
	public class NestedProjectBuildDependency : IVsBuildDependency
	{
		IVsHierarchy dependentHierarchy = null;

		#region ctors
		[CLSCompliant(false)]
		public NestedProjectBuildDependency(IVsHierarchy dependentHierarchy)
		{
			this.dependentHierarchy = dependentHierarchy;
		}
		#endregion

		#region IVsBuildDependency methods
		public int get_CanonicalName(out string canonicalName)
		{
			canonicalName = null;
			return VSConstants.S_OK;
		}

		public int get_Type(out System.Guid guidType)
		{
			// All our dependencies are build projects
			guidType = VSConstants.GUID_VS_DEPTYPE_BUILD_PROJECT;

			return VSConstants.S_OK;
		}

        public int get_Description(out string description)
		{
			description = null;
			return VSConstants.S_OK;
		}

		[CLSCompliant(false)]
		public int get_HelpContext(out uint helpContext)
		{
			helpContext = 0;
			return VSConstants.E_NOTIMPL;
		}

		public int get_HelpFile(out string helpFile)
		{
			helpFile = null;
			return VSConstants.E_NOTIMPL;
		}

		public int get_MustUpdateBefore(out int mustUpdateBefore)
		{
			// Must always update dependencies
			mustUpdateBefore = 1;

			return VSConstants.S_OK;
		}

		public int get_ReferredProject(out object unknownProject)
		{
			unknownProject = this.dependentHierarchy;

			return (unknownProject == null) ? VSConstants.E_FAIL : VSConstants.S_OK;
		}
		#endregion

	}
}
