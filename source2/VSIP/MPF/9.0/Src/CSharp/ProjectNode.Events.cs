/// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;

namespace Microsoft.VisualStudio.Project
{
	public partial class ProjectNode
	{
		#region fields
		private EventHandler<ProjectPropertyChangedArgs> projectPropertiesListeners;
		#endregion

		#region events
		public event EventHandler<ProjectPropertyChangedArgs> OnProjectPropertyChanged
		{
			add { projectPropertiesListeners += value; }
			remove { projectPropertiesListeners -= value; }
		}
		#endregion

		#region methods
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
		protected void RaiseProjectPropertyChanged(string propertyName, string oldValue, string newValue)
		{
			if(null != projectPropertiesListeners)
			{
				projectPropertiesListeners(this, new ProjectPropertyChangedArgs(propertyName, oldValue, newValue));
			}
		}
		#endregion
	}

}