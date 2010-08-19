/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.ComponentModel;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using ErrorHandler = Microsoft.VisualStudio.ErrorHandler;

namespace Microsoft.VisualStudio.Project
{
	/// <summary>
	/// This class is used to enable launching the project properties
	/// editor from the Properties Browser.
	/// </summary>
	[CLSCompliant(false)]
	public class PropertiesEditorLauncher : ComponentEditor
	{
		private ServiceProvider serviceProvider;

		#region ctor
		public PropertiesEditorLauncher(ServiceProvider serviceProvider)
		{
			if(serviceProvider == null)
				throw new ArgumentNullException("serviceProvider");

			this.serviceProvider = serviceProvider;
		}
		#endregion
		#region overridden methods
		/// <summary>
		/// Launch the Project Properties Editor (properties pages)
		/// </summary>
		/// <returns>If we succeeded or not</returns>
		public override bool EditComponent(ITypeDescriptorContext context, object component)
		{
			if(component is ProjectNodeProperties)
			{
				IVsPropertyPageFrame propertyPageFrame = (IVsPropertyPageFrame)serviceProvider.GetService((typeof(SVsPropertyPageFrame)));

				int hr = propertyPageFrame.ShowFrame(Guid.Empty);
				if(ErrorHandler.Succeeded(hr))
					return true;
				else
					ErrorHandler.ThrowOnFailure(propertyPageFrame.ReportError(hr));
			}

			return false;
		}
		#endregion

	}
}
