/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using Microsoft.VisualStudio.Shell.Interop;
using VSLangProj;
using Microsoft.VisualStudio;

namespace Microsoft.VisualStudio.Project.Automation
{
	public class OABuildManager : ConnectionPointContainer,
									IEventSource<_dispBuildManagerEvents>,
									BuildManager,
									BuildManagerEvents
	{
		private ProjectNode projectManager;

		public OABuildManager(ProjectNode project)
		{
			projectManager = project;
			AddEventSource<_dispBuildManagerEvents>(this as IEventSource<_dispBuildManagerEvents>);
		}


		#region BuildManager Members

		public virtual string BuildDesignTimeOutput(string bstrOutputMoniker)
		{
			throw new NotImplementedException();
		}

		public virtual EnvDTE.Project ContainingProject
		{
			get { return projectManager.GetAutomationObject() as EnvDTE.Project; }
		}

		public virtual EnvDTE.DTE DTE
		{
			get { return projectManager.Site.GetService(typeof(EnvDTE.DTE)) as EnvDTE.DTE; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public virtual object DesignTimeOutputMonikers
		{
			get { throw new NotImplementedException(); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public virtual object Parent
		{
			get { throw new NotImplementedException(); }
		}

		#endregion

		#region _dispBuildManagerEvents_Event Members

		public event _dispBuildManagerEvents_DesignTimeOutputDeletedEventHandler DesignTimeOutputDeleted;

		public event _dispBuildManagerEvents_DesignTimeOutputDirtyEventHandler DesignTimeOutputDirty;

		#endregion

		#region IEventSource<_dispBuildManagerEvents> Members

		void IEventSource<_dispBuildManagerEvents>.OnSinkAdded(_dispBuildManagerEvents sink)
		{
			DesignTimeOutputDeleted += new _dispBuildManagerEvents_DesignTimeOutputDeletedEventHandler(sink.DesignTimeOutputDeleted);
			DesignTimeOutputDirty += new _dispBuildManagerEvents_DesignTimeOutputDirtyEventHandler(sink.DesignTimeOutputDirty);
		}

		void IEventSource<_dispBuildManagerEvents>.OnSinkRemoved(_dispBuildManagerEvents sink)
		{
			DesignTimeOutputDeleted -= new _dispBuildManagerEvents_DesignTimeOutputDeletedEventHandler(sink.DesignTimeOutputDeleted);
			DesignTimeOutputDirty -= new _dispBuildManagerEvents_DesignTimeOutputDirtyEventHandler(sink.DesignTimeOutputDirty);
		}

		#endregion

        protected virtual void OnDesignTimeOutputDeleted(string outputMoniker)
        {
            var handlers = this.DesignTimeOutputDeleted;
            if (handlers != null)
            {
                handlers(outputMoniker);
            }
        }

        protected virtual void OnDesignTimeOutputDirty(string outputMoniker)
        {
            var handlers = this.DesignTimeOutputDirty;
            if (handlers != null)
            {
                handlers(outputMoniker);
            }
        }
	}
}
