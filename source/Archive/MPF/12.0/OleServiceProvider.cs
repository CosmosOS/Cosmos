/********************************************************************************************

Copyright (c) Microsoft Corporation 
All rights reserved. 

Microsoft Public License: 

This license governs use of the accompanying software. If you use the software, you 
accept this license. If you do not accept the license, do not use the software. 

1. Definitions 
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the 
same meaning here as under U.S. copyright law. 
A "contribution" is the original software, or any additions or changes to the software. 
A "contributor" is any person that distributes its contribution under this license. 
"Licensed patents" are a contributor's patent claims that read directly on its contribution. 

2. Grant of Rights 
(A) Copyright Grant- Subject to the terms of this license, including the license conditions 
and limitations in section 3, each contributor grants you a non-exclusive, worldwide, 
royalty-free copyright license to reproduce its contribution, prepare derivative works of 
its contribution, and distribute its contribution or any derivative works that you create. 
(B) Patent Grant- Subject to the terms of this license, including the license conditions 
and limitations in section 3, each contributor grants you a non-exclusive, worldwide, 
royalty-free license under its licensed patents to make, have made, use, sell, offer for 
sale, import, and/or otherwise dispose of its contribution in the software or derivative 
works of the contribution in the software. 

3. Conditions and Limitations 
(A) No Trademark License- This license does not grant you rights to use any contributors' 
name, logo, or trademarks. 
(B) If you bring a patent claim against any contributor over patents that you claim are 
infringed by the software, your patent license from such contributor to the software ends 
automatically. 
(C) If you distribute any portion of the software, you must retain all copyright, patent, 
trademark, and attribution notices that are present in the software. 
(D) If you distribute any portion of the software in source code form, you may do so only 
under this license by including a complete copy of this license with your distribution. 
If you distribute any portion of the software in compiled or object code form, you may only 
do so under a license that complies with this license. 
(E) The software is licensed "as-is." You bear the risk of using it. The contributors give 
no express warranties, guarantees or conditions. You may have additional consumer rights 
under your local laws which this license cannot change. To the extent permitted under your 
local laws, the contributors exclude the implied warranties of merchantability, fitness for 
a particular purpose and non-infringement.

********************************************************************************************/

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
