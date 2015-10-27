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
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using IServiceProvider = System.IServiceProvider;
using ShellConstants = Microsoft.VisualStudio.Shell.Interop.Constants;

namespace Microsoft.VisualStudio.Project
{
	/// <summary>
	/// helper to make the editor ignore external changes
	/// </summary>
	internal class SuspendFileChanges
	{
		private string documentFileName;

		private bool isSuspending;

		private IServiceProvider site;

		private IVsDocDataFileChangeControl fileChangeControl;

		public SuspendFileChanges(IServiceProvider site, string document)
		{
			this.site = site;
			this.documentFileName = document;
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		public void Suspend()
		{
			if(this.isSuspending)
				return;

			IntPtr docData = IntPtr.Zero;
			try
			{
				IVsRunningDocumentTable rdt = this.site.GetService(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable;

				IVsHierarchy hierarchy;
				uint itemId;
				uint docCookie;
				IVsFileChangeEx fileChange;


				if(rdt == null) return;

				ErrorHandler.ThrowOnFailure(rdt.FindAndLockDocument((uint)_VSRDTFLAGS.RDT_NoLock, this.documentFileName, out hierarchy, out itemId, out docData, out docCookie));

				if((docCookie == (uint)ShellConstants.VSDOCCOOKIE_NIL) || docData == IntPtr.Zero)
					return;

				fileChange = this.site.GetService(typeof(SVsFileChangeEx)) as IVsFileChangeEx;

				if(fileChange != null)
				{
					this.isSuspending = true;
					ErrorHandler.ThrowOnFailure(fileChange.IgnoreFile(0, this.documentFileName, 1));
					if(docData != IntPtr.Zero)
					{
						IVsPersistDocData persistDocData = null;

						// if interface is not supported, return null
						object unknown = Marshal.GetObjectForIUnknown(docData);
						if(unknown is IVsPersistDocData)
						{
							persistDocData = (IVsPersistDocData)unknown;
							if(persistDocData is IVsDocDataFileChangeControl)
							{
								this.fileChangeControl = (IVsDocDataFileChangeControl)persistDocData;
								if(this.fileChangeControl != null)
								{
									ErrorHandler.ThrowOnFailure(this.fileChangeControl.IgnoreFileChanges(1));
								}
							}
						}
					}
				}
			}
			catch(InvalidCastException e)
			{
				Trace.WriteLine("Exception" + e.Message);
			}
			finally
			{
				if(docData != IntPtr.Zero)
				{
					Marshal.Release(docData);
				}
			}
			return;
		}

		public void Resume()
		{
			if(!this.isSuspending)
				return;
			IVsFileChangeEx fileChange;
			fileChange = this.site.GetService(typeof(SVsFileChangeEx)) as IVsFileChangeEx;
			if(fileChange != null)
			{
				this.isSuspending = false;
				ErrorHandler.ThrowOnFailure(fileChange.IgnoreFile(0, this.documentFileName, 0));
				if(this.fileChangeControl != null)
				{
					ErrorHandler.ThrowOnFailure(this.fileChangeControl.IgnoreFileChanges(0));
				}
			}
		}
	}
}
