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
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using ShellConstants = Microsoft.VisualStudio.Shell.Interop.Constants;

namespace Microsoft.VisualStudio.Project
{

	[CLSCompliant(false)]
	public abstract class SelectionListener : IVsSelectionEvents, IDisposable
	{
		#region fields
		private uint eventsCookie;
		private IVsMonitorSelection monSel;
		private ServiceProvider serviceProvider;
		private bool isDisposed;
		/// <summary>
		/// Defines an object that will be a mutex for this object for synchronizing thread calls.
		/// </summary>
		private static volatile object Mutex = new object();
		#endregion

		#region ctors
		protected SelectionListener(ServiceProvider serviceProviderParameter)
		{
            if (serviceProviderParameter == null)
            {
                throw new ArgumentNullException("serviceProviderParameter");
            }

            this.serviceProvider = serviceProviderParameter;
            this.monSel = this.serviceProvider.GetService(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;

			if(this.monSel == null)
			{
				throw new InvalidOperationException();
			}
		}
		#endregion

		#region properties
		protected uint EventsCookie
		{
			get
			{
				return this.eventsCookie;
			}
		}

		protected IVsMonitorSelection SelectionMonitor
		{
			get
			{
				return this.monSel;
			}
		}

		protected ServiceProvider ServiceProvider
		{
			get
			{
				return this.serviceProvider;
			}
		}
		#endregion

		#region IVsSelectionEvents Members

		public virtual int OnCmdUIContextChanged(uint dwCmdUICookie, int fActive)
		{
			return VSConstants.E_NOTIMPL;
		}

		public virtual int OnElementValueChanged(uint elementid, object varValueOld, object varValueNew)
		{
			return VSConstants.E_NOTIMPL;
		}

		public virtual int OnSelectionChanged(IVsHierarchy pHierOld, uint itemidOld, IVsMultiItemSelect pMISOld, ISelectionContainer pSCOld, IVsHierarchy pHierNew, uint itemidNew, IVsMultiItemSelect pMISNew, ISelectionContainer pSCNew)
		{
			return VSConstants.E_NOTIMPL;
		}

		#endregion

		#region IDisposable Members
		/// <summary>
		/// The IDispose interface Dispose method for disposing the object determinastically.
		/// </summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion

		#region methods
		public void Init()
		{
			if(this.SelectionMonitor != null)
			{
				ErrorHandler.ThrowOnFailure(this.SelectionMonitor.AdviseSelectionEvents(this, out this.eventsCookie));
			}
		}

		/// <summary>
		/// The method that does the cleanup.
		/// </summary>
		/// <param name="disposing"></param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsMonitorSelection.UnadviseSelectionEvents(System.UInt32)")]
		protected virtual void Dispose(bool disposing)
		{
			// Everybody can go here.
			if(!this.isDisposed)
			{
				// Synchronize calls to the Dispose simulteniously.
				lock(Mutex)
				{
					if(disposing && this.eventsCookie != (uint)ShellConstants.VSCOOKIE_NIL && this.SelectionMonitor != null)
					{
						this.SelectionMonitor.UnadviseSelectionEvents((uint)this.eventsCookie);
						this.eventsCookie = (uint)ShellConstants.VSCOOKIE_NIL;
					}

					this.isDisposed = true;
				}
			}
		}
		#endregion

	}
}
