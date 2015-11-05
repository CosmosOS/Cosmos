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
using ErrorHandler = Microsoft.VisualStudio.ErrorHandler;

namespace Microsoft.VisualStudio.Project.Automation
{
	/// <summary>
	/// Helper class that handle the scope of an automation function.
	/// It should be used inside a "using" directive to define the scope of the
	/// automation function and make sure that the ExitAutomation method is called.
	/// </summary>
	internal class AutomationScope : IDisposable
	{
		private IVsExtensibility3 extensibility;
		private bool inAutomation;
		private static volatile object Mutex;
		private bool isDisposed;

		/// <summary>
		/// Initializes the <see cref="AutomationScope"/> class.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
		static AutomationScope()
		{
			Mutex = new object();
		}

		/// <summary>
		/// Defines the beginning of the scope of an automation function. This constuctor
		/// calls EnterAutomationFunction to signal the Shell that the current function is
		/// changing the status of the automation objects.
		/// </summary>
		public AutomationScope(IServiceProvider provider)
		{
			if(null == provider)
			{
				throw new ArgumentNullException("provider");
			}
			extensibility = provider.GetService(typeof(EnvDTE.IVsExtensibility)) as IVsExtensibility3;
			if(null == extensibility)
			{
				throw new InvalidOperationException();
			}
			ErrorHandler.ThrowOnFailure(extensibility.EnterAutomationFunction());
			inAutomation = true;
		}

		/// <summary>
		/// Ends the scope of the automation function. This function is also called by the
		/// Dispose method.
		/// </summary>
		public void ExitAutomation()
		{
			if(inAutomation)
			{
				ErrorHandler.ThrowOnFailure(extensibility.ExitAutomationFunction());
				inAutomation = false;
			}
		}

		/// <summary>
		/// Gets the IVsExtensibility3 interface used in the automation function.
		/// </summary>
		public IVsExtensibility3 Extensibility
		{
			get { return extensibility; }
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		#region IDisposable Members
		private void Dispose(bool disposing)
		{
			if(!this.isDisposed)
			{
				lock(Mutex)
				{
					if(disposing)
					{
						ExitAutomation();
					}

					this.isDisposed = true;
				}
			}
		}
		#endregion
	}
}
