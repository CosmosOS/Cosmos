/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Microsoft.VisualStudio.Project
{
	public class OleServiceProvider : IOleServiceProvider, IDisposable
	{
		#region Public Types
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
		public delegate object ServiceCreatorCallback(Type serviceType);
		#endregion

		#region Private Types
		private class ServiceData : IDisposable
		{
			private Type serviceType;
			private object instance;
			private ServiceCreatorCallback creator;
			private bool shouldDispose;
			public ServiceData(Type serviceType, object instance, ServiceCreatorCallback callback, bool shouldDispose)
			{
				if(null == serviceType)
				{
					throw new ArgumentNullException("serviceType");
				}

				if((null == instance) && (null == callback))
				{
					throw new ArgumentNullException("instance");
				}

				this.serviceType = serviceType;
				this.instance = instance;
				this.creator = callback;
				this.shouldDispose = shouldDispose;
			}

			public object ServiceInstance
			{
				get
				{
					if(null == instance)
					{
						instance = creator(serviceType);
					}
					return instance;
				}
			}

			public Guid Guid
			{
				get { return serviceType.GUID; }
			}

			public void Dispose()
			{
				if((shouldDispose) && (null != instance))
				{
					IDisposable disp = instance as IDisposable;
					if(null != disp)
					{
						disp.Dispose();
					}
					instance = null;
				}
				creator = null;
				GC.SuppressFinalize(this);
			}
		}
		#endregion

		#region fields

		private Dictionary<Guid, ServiceData> services = new Dictionary<Guid, ServiceData>();
		private bool isDisposed;
		/// <summary>
		/// Defines an object that will be a mutex for this object for synchronizing thread calls.
		/// </summary>
		private static volatile object Mutex = new object();
		#endregion

		#region ctors
		public OleServiceProvider()
		{
		}
		#endregion

		#region IOleServiceProvider Members

		public int QueryService(ref Guid guidService, ref Guid riid, out IntPtr ppvObject)
		{
			ppvObject = (IntPtr)0;
			int hr = VSConstants.S_OK;

			ServiceData serviceInstance = null;

			if(services != null && services.ContainsKey(guidService))
			{
				serviceInstance = services[guidService];
			}

			if(serviceInstance == null)
			{
				return VSConstants.E_NOINTERFACE;
			}

			// Now check to see if the user asked for an IID other than
			// IUnknown.  If so, we must do another QI.
			//
			if(riid.Equals(NativeMethods.IID_IUnknown))
			{
				ppvObject = Marshal.GetIUnknownForObject(serviceInstance.ServiceInstance);
			}
			else
			{
				IntPtr pUnk = IntPtr.Zero;
				try
				{
					pUnk = Marshal.GetIUnknownForObject(serviceInstance.ServiceInstance);
					hr = Marshal.QueryInterface(pUnk, ref riid, out ppvObject);
				}
				finally
				{
					if(pUnk != IntPtr.Zero)
					{
						Marshal.Release(pUnk);
					}
				}
			}

			return hr;
		}

		#endregion

		#region Dispose

		/// <summary>
		/// The IDispose interface Dispose method for disposing the object determinastically.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

		/// <summary>
		/// Adds the given service to the service container.
		/// </summary>
		/// <param name="serviceType">The type of the service to add.</param>
		/// <param name="serviceInstance">An instance of the service.</param>
		/// <param name="shouldDisposeServiceInstance">true if the Dipose of the service provider is allowed to dispose the sevice instance.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "The services created here will be disposed in the Dispose method of this type.")]
        public void AddService(Type serviceType, object serviceInstance, bool shouldDisposeServiceInstance)
		{
			// Create the description of this service. Note that we don't do any validation
			// of the parameter here because the constructor of ServiceData will do it for us.
			ServiceData service = new ServiceData(serviceType, serviceInstance, null, shouldDisposeServiceInstance);

			// Now add the service desctription to the dictionary.
			AddService(service);
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification="The services created here will be disposed in the Dispose method of this type.")]
        public void AddService(Type serviceType, ServiceCreatorCallback callback, bool shouldDisposeServiceInstance)
		{
			// Create the description of this service. Note that we don't do any validation
			// of the parameter here because the constructor of ServiceData will do it for us.
			ServiceData service = new ServiceData(serviceType, null, callback, shouldDisposeServiceInstance);

			// Now add the service desctription to the dictionary.
			AddService(service);
		}

		private void AddService(ServiceData data)
		{
			// Make sure that the collection of services is created.
			if(null == services)
			{
				services = new Dictionary<Guid, ServiceData>();
			}

			// Disallow the addition of duplicate services.
			if(services.ContainsKey(data.Guid))
			{
				throw new InvalidOperationException();
			}

			services.Add(data.Guid, data);
		}

		/// <devdoc>
		/// Removes the given service type from the service container.
		/// </devdoc>
		public void RemoveService(Type serviceType)
		{
			if(serviceType == null)
			{
				throw new ArgumentNullException("serviceType");
			}

			if(services.ContainsKey(serviceType.GUID))
			{
				services.Remove(serviceType.GUID);
			}
		}

		#region helper methods
		/// <summary>
		/// The method that does the cleanup.
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing)
		{
			// Everybody can go here.
			if(!this.isDisposed)
			{
				// Synchronize calls to the Dispose simulteniously.
				lock(Mutex)
				{
					if(disposing)
					{
						// Remove all our services
						if(services != null)
						{
							foreach(ServiceData data in services.Values)
							{
								data.Dispose();
							}
							services.Clear();
							services = null;
						}
					}

					this.isDisposed = true;
				}
			}
		}
		#endregion

	}
}
