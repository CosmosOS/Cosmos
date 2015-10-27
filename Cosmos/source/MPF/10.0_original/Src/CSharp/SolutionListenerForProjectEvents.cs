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
using IServiceProvider = System.IServiceProvider;

namespace Microsoft.VisualStudio.Project
{
	/// <summary>
	/// This class triggers the project events for "our" hierrachies.
	/// </summary>
	internal class SolutionListenerForProjectEvents : SolutionListener, IProjectEvents
	{
		#region events
		/// Event raised just after the project file opened.
		/// </summary>
		public event EventHandler<AfterProjectFileOpenedEventArgs> AfterProjectFileOpened;

		/// <summary>
		/// Event raised before the project file closed.
		/// </summary>
		public event EventHandler<BeforeProjectFileClosedEventArgs> BeforeProjectFileClosed;
		#endregion

		#region ctor
		internal SolutionListenerForProjectEvents(IServiceProvider serviceProvider)
			: base(serviceProvider)
		{
		}
		#endregion

		#region overridden methods
		public override int OnAfterOpenProject(IVsHierarchy hierarchy, int added)
		{
			IProjectEventsListener projectEventListener = hierarchy as IProjectEventsListener;
			if(projectEventListener != null && projectEventListener.IsProjectEventsListener)
			{
				this.RaiseAfterProjectFileOpened((added != 0) ? true : false);
			}

			return VSConstants.S_OK;
		}

		public override int OnBeforeCloseProject(IVsHierarchy hierarchy, int removed)
		{
			IProjectEventsListener projectEvents = hierarchy as IProjectEventsListener;
			if(projectEvents != null && projectEvents.IsProjectEventsListener)
			{
				this.RaiseBeforeProjectFileClosed((removed != 0) ? true : false);
			}

			return VSConstants.S_OK;
		}
		#endregion

		#region helpers
		/// <summary>
		/// Raises after project file opened event.
		/// </summary>
		/// <param name="added">True if the project is added to the solution after the solution is opened. false if the project is added to the solution while the solution is being opened.</param>
		private void RaiseAfterProjectFileOpened(bool added)
		{
			// Save event in temporary variable to avoid race condition.
			EventHandler<AfterProjectFileOpenedEventArgs> tempEvent = this.AfterProjectFileOpened;
			if(tempEvent != null)
			{
				tempEvent(this, new AfterProjectFileOpenedEventArgs(added));
			}
		}




		/// <summary>
		/// Raises the before  project file closed event.
		/// </summary>
		/// <param name="added">true if the project was removed from the solution before the solution was closed. false if the project was removed from the solution while the solution was being closed.</param>
		private void RaiseBeforeProjectFileClosed(bool removed)
		{
			// Save event in temporary variable to avoid race condition.
			EventHandler<BeforeProjectFileClosedEventArgs> tempEvent = this.BeforeProjectFileClosed;
			if(tempEvent != null)
			{
				tempEvent(this, new BeforeProjectFileClosedEventArgs(removed));
			}
		}
	}
		#endregion
}
