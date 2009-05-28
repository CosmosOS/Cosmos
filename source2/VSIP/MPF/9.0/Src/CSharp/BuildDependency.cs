/// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.Project
{
	public class BuildDependency : IVsBuildDependency
	{
		Guid referencedProjectGuid = Guid.Empty;
		ProjectNode projectMgr = null;

		[CLSCompliant(false)]
		public BuildDependency(ProjectNode projectMgr, Guid projectReference)
		{
			this.referencedProjectGuid = projectReference;
			this.projectMgr = projectMgr;
		}

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
			unknownProject = null;

			unknownProject = this.GetReferencedHierarchy();

			// If we cannot find the referenced hierarchy return S_FALSE.
			return (unknownProject == null) ? VSConstants.S_FALSE : VSConstants.S_OK;
		}

		#endregion

		#region helper methods
		private IVsHierarchy GetReferencedHierarchy()
		{
			IVsHierarchy hierarchy = null;

			if(this.referencedProjectGuid == Guid.Empty || this.projectMgr == null || this.projectMgr.IsClosed)
			{
				return hierarchy;
			}

			return VsShellUtilities.GetHierarchy(this.projectMgr.Site, this.referencedProjectGuid);

		}

		#endregion

	}
}
