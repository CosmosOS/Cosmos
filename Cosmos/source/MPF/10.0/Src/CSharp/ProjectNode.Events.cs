/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

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