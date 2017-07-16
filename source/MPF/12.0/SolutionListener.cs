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
using System.Diagnostics;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using IServiceProvider = System.IServiceProvider;
using ShellConstants = Microsoft.VisualStudio.Shell.Interop.Constants;

namespace Microsoft.VisualStudio.Project
{


    public abstract class SolutionListener : IVsSolutionEvents3, IVsSolutionEvents4, IDisposable
    {

        #region fields
        private uint eventsCookie;
        private IVsSolution solution;
        private IServiceProvider serviceProvider;
        private bool isDisposed;
        /// <summary>
        /// Defines an object that will be a mutex for this object for synchronizing thread calls.
        /// </summary>
        private static volatile object Mutex = new object();
        #endregion

        #region ctors
        protected SolutionListener(IServiceProvider serviceProviderParameter)
        {
            if (serviceProviderParameter == null)
            {
                throw new ArgumentNullException("serviceProviderParameter");
            }

            this.serviceProvider = serviceProviderParameter;
            this.solution = this.serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;

            Debug.Assert(this.solution != null, "Could not get the IVsSolution object from the services exposed by this project");

            if(this.solution == null)
            {
                throw new InvalidOperationException();
            }

            this.InteropSafeIVsSolutionEvents = Utilities.GetOuterAs<IVsSolutionEvents>(this);
        }
        #endregion

        #region properties

        public IVsSolutionEvents InteropSafeIVsSolutionEvents
        {
            get;
            protected set;
        }

        protected uint EventsCookie
        {
            get
            {
                return this.eventsCookie;
            }
        }

        protected IVsSolution Solution
        {
            get
            {
                return this.solution;
            }
        }

        protected IServiceProvider ServiceProvider
        {
            get
            {
                return this.serviceProvider;
            }
        }
        #endregion

        #region IVsSolutionEvents3, IVsSolutionEvents2, IVsSolutionEvents methods
        public virtual int OnAfterCloseSolution(object reserved)
        {
            return VSConstants.E_NOTIMPL;
        }

        public virtual int OnAfterClosingChildren(IVsHierarchy hierarchy)
        {
            return VSConstants.E_NOTIMPL;
        }

        public virtual int OnAfterLoadProject(IVsHierarchy stubHierarchy, IVsHierarchy realHierarchy)
        {
            return VSConstants.E_NOTIMPL;
        }

        public virtual int OnAfterMergeSolution(object pUnkReserved)
        {
            return VSConstants.E_NOTIMPL;
        }

        public virtual int OnAfterOpenProject(IVsHierarchy hierarchy, int added)
        {
            return VSConstants.E_NOTIMPL;
        }

        public virtual int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            return VSConstants.E_NOTIMPL;
        }

        public virtual int OnAfterOpeningChildren(IVsHierarchy hierarchy)
        {
            return VSConstants.E_NOTIMPL;
        }

        public virtual int OnBeforeCloseProject(IVsHierarchy hierarchy, int removed)
        {
            return VSConstants.E_NOTIMPL;
        }

        public virtual int OnBeforeCloseSolution(object pUnkReserved)
        {
            return VSConstants.E_NOTIMPL;
        }

        public virtual int OnBeforeClosingChildren(IVsHierarchy hierarchy)
        {
            return VSConstants.E_NOTIMPL;
        }

        public virtual int OnBeforeOpeningChildren(IVsHierarchy hierarchy)
        {
            return VSConstants.E_NOTIMPL;
        }

        public virtual int OnBeforeUnloadProject(IVsHierarchy realHierarchy, IVsHierarchy rtubHierarchy)
        {
            return VSConstants.E_NOTIMPL;
        }

        public virtual int OnQueryCloseProject(IVsHierarchy hierarchy, int removing, ref int cancel)
        {
            return VSConstants.E_NOTIMPL;
        }

        public virtual int OnQueryCloseSolution(object pUnkReserved, ref int cancel)
        {
            return VSConstants.E_NOTIMPL;
        }

        public virtual int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int cancel)
        {
            return VSConstants.E_NOTIMPL;
        }
        #endregion

        #region IVsSolutionEvents4 methods
        public virtual int OnAfterAsynchOpenProject(IVsHierarchy hierarchy, int added)
        {
            return VSConstants.E_NOTIMPL;
        }

        public virtual int OnAfterChangeProjectParent(IVsHierarchy hierarchy)
        {
            return VSConstants.E_NOTIMPL;
        }

        public virtual int OnAfterRenameProject(IVsHierarchy hierarchy)
        {
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// Fired before a project is moved from one parent to another in the solution explorer
        /// </summary>
        public virtual int OnQueryChangeProjectParent(IVsHierarchy hierarchy, IVsHierarchy newParentHier, ref int cancel)
        {
            return VSConstants.E_NOTIMPL;
        }
        #endregion

        #region Dispose

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
            if(this.solution != null)
            {
                ErrorHandler.ThrowOnFailure(this.solution.AdviseSolutionEvents(this.InteropSafeIVsSolutionEvents, out this.eventsCookie));
            }
        }

        /// <summary>
        /// The method that does the cleanup.
        /// </summary>
        /// <param name="disposing"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsSolution.UnadviseSolutionEvents(System.UInt32)")]
        protected virtual void Dispose(bool disposing)
        {
            // Everybody can go here.
            if(!this.isDisposed)
            {
                // Synchronize calls to the Dispose simulteniously.
                lock(Mutex)
                {
                    if(disposing && this.eventsCookie != (uint)ShellConstants.VSCOOKIE_NIL && this.solution != null)
                    {
                        this.solution.UnadviseSolutionEvents((uint)this.eventsCookie);
                        this.eventsCookie = (uint)ShellConstants.VSCOOKIE_NIL;
                    }

                    this.isDisposed = true;
                }
            }
        }
        #endregion
    }
}
