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
    /// <summary>
    /// Defines an abstract class implementing IVsUpdateSolutionEvents interfaces.
    /// </summary>

    public abstract class UpdateSolutionEventsListener : IVsUpdateSolutionEvents3, IVsUpdateSolutionEvents2, IDisposable
    {
        #region fields
        /// <summary>
        /// The cookie associated to the the events based IVsUpdateSolutionEvents2.
        /// </summary>
        private uint solutionEvents2Cookie;

        /// <summary>
        /// The cookie associated to the theIVsUpdateSolutionEvents3 events.
        /// </summary>
        private uint solutionEvents3Cookie;

        /// <summary>
        /// The IVsSolutionBuildManager2 object controlling the update solution events.
        /// </summary>
        private IVsSolutionBuildManager2 solutionBuildManager;


        /// <summary>
        /// The associated service provider.
        /// </summary>
        private IServiceProvider serviceProvider;

        /// <summary>
        /// Flag determining if the object has been disposed.
        /// </summary>
        private bool isDisposed;

        /// <summary>
        /// Defines an object that will be a mutex for this object for synchronizing thread calls.
        /// </summary>
        private static volatile object Mutex = new object();
        #endregion

        #region ctors
        /// <summary>
        /// Overloaded constructor.
        /// </summary>
        /// <param name="serviceProvider">A service provider.</param>
        protected UpdateSolutionEventsListener(IServiceProvider serviceProvider)
        {
            if(serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }

            this.serviceProvider = serviceProvider;

            this.solutionBuildManager = this.serviceProvider.GetService(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager2;

            if(this.solutionBuildManager == null)
            {
                throw new InvalidOperationException();
            }

            ErrorHandler.ThrowOnFailure(this.solutionBuildManager.AdviseUpdateSolutionEvents(this, out this.solutionEvents2Cookie));

            Debug.Assert(this.solutionBuildManager is IVsSolutionBuildManager3, "The solution build manager object implementing IVsSolutionBuildManager2 does not implement IVsSolutionBuildManager3");
            ErrorHandler.ThrowOnFailure(this.SolutionBuildManager3.AdviseUpdateSolutionEvents3(this, out this.solutionEvents3Cookie));
        }
        #endregion

        #region properties

        /// <summary>
        /// The associated service provider.
        /// </summary>
        protected IServiceProvider ServiceProvider
        {
            get
            {
                return this.serviceProvider;
            }
        }

        /// <summary>
        /// The solution build manager object controlling the solution events.
        /// </summary>
        protected IVsSolutionBuildManager2 SolutionBuildManager2
        {
            get
            {
                return this.solutionBuildManager;
            }
        }

        /// <summary>
        /// The solution build manager object controlling the solution events.
        /// </summary>
        protected IVsSolutionBuildManager3 SolutionBuildManager3
        {
            get
            {
                return (IVsSolutionBuildManager3)this.solutionBuildManager;
            }

        }
        #endregion

        #region IVsUpdateSolutionEvents3 Members

        /// <summary>
        /// Fired after the active solution config is changed (pOldActiveSlnCfg can be NULL).
        /// </summary>
        /// <param name="oldActiveSlnCfg">Old configuration.</param>
        /// <param name="newActiveSlnCfg">New configuration.</param>
        /// <returns>If the method succeeds, it returns S_OK. If it fails, it returns an error code.</returns>
        public virtual int OnAfterActiveSolutionCfgChange(IVsCfg oldActiveSlnCfg, IVsCfg newActiveSlnCfg)
        {
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// Fired before the active solution config is changed (pOldActiveSlnCfg can be NULL
        /// </summary>
        /// <param name="oldActiveSlnCfg">Old configuration.</param>
        /// <param name="newActiveSlnCfg">New configuration.</param>
        /// <returns>If the method succeeds, it returns S_OK. If it fails, it returns an error code.</returns>
        public virtual int OnBeforeActiveSolutionCfgChange(IVsCfg oldActiveSlnCfg, IVsCfg newActiveSlnCfg)
        {
            return VSConstants.E_NOTIMPL;
        }

        #endregion

        #region IVsUpdateSolutionEvents2 Members

        /// <summary>
        /// Called when the active project configuration for a project in the solution has changed. 
        /// </summary>
        /// <param name="hierarchy">The project whose configuration has changed.</param>
        /// <returns>If the method succeeds, it returns S_OK. If it fails, it returns an error code.</returns>
        public virtual int OnActiveProjectCfgChange(IVsHierarchy hierarchy)
        {
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// Called right before a project configuration begins to build. 
        /// </summary>
        /// <param name="hierarchy">The project that is to be build.</param>
        /// <param name="configProject">A configuration project object.</param>
        /// <param name="configSolution">A configuration solution object.</param>
        /// <param name="action">The action taken.</param>
        /// <param name="cancel">A flag indicating cancel.</param>
        /// <returns>If the method succeeds, it returns S_OK. If it fails, it returns an error code.</returns>
        /// <remarks>The values for the action are defined in the enum _SLNUPDACTION env\msenv\core\slnupd2.h</remarks>
        public int UpdateProjectCfg_Begin(IVsHierarchy hierarchy, IVsCfg configProject, IVsCfg configSolution, uint action, ref int cancel)
        {
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// Called right after a project configuration is finished building. 
        /// </summary>
        /// <param name="hierarchy">The project that has finished building.</param>
        /// <param name="configProject">A configuration project object.</param>
        /// <param name="configSolution">A configuration solution object.</param>
        /// <param name="action">The action taken.</param>
        /// <param name="success">Flag indicating success.</param>
        /// <param name="cancel">Flag indicating cancel.</param>
        /// <returns>If the method succeeds, it returns S_OK. If it fails, it returns an error code.</returns>
        /// <remarks>The values for the action are defined in the enum _SLNUPDACTION env\msenv\core\slnupd2.h</remarks>
        public virtual int UpdateProjectCfg_Done(IVsHierarchy hierarchy, IVsCfg configProject, IVsCfg configSolution, uint action, int success, int cancel)
        {
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// Called before any build actions have begun. This is the last chance to cancel the build before any building begins. 
        /// </summary>
        /// <param name="cancelUpdate">Flag indicating cancel update.</param>
        /// <returns>If the method succeeds, it returns S_OK. If it fails, it returns an error code.</returns>
        public virtual int UpdateSolution_Begin(ref int cancelUpdate)
        {
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// Called when a build is being cancelled. 
        /// </summary>
        /// <returns>If the method succeeds, it returns S_OK. If it fails, it returns an error code.</returns>
        public virtual int UpdateSolution_Cancel()
        {
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// Called when a build is completed. 
        /// </summary>
        /// <param name="succeeded">true if no update actions failed.</param>
        /// <param name="modified">true if any update action succeeded.</param>
        /// <param name="cancelCommand">true if update actions were canceled.</param>
        /// <returns>If the method succeeds, it returns S_OK. If it fails, it returns an error code.</returns>
        public virtual int UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
        {
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// Called before the first project configuration is about to be built. 
        /// </summary>
        /// <param name="cancelUpdate">A flag indicating cancel update.</param>
        /// <returns>If the method succeeds, it returns S_OK. If it fails, it returns an error code.</returns>
        public virtual int UpdateSolution_StartUpdate(ref int cancelUpdate)
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
        /// <summary>
        /// The method that does the cleanup.
        /// </summary>
        /// <param name="disposing">true if called from IDispose.Dispose; false if called from Finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            // Everybody can go here.
            if(!this.isDisposed)
            {
                // Synchronize calls to the Dispose simultaniously.
                lock(Mutex)
                {
                    if(this.solutionEvents2Cookie != (uint)ShellConstants.VSCOOKIE_NIL)
                    {
                        ErrorHandler.ThrowOnFailure(this.solutionBuildManager.UnadviseUpdateSolutionEvents(this.solutionEvents2Cookie));
                        this.solutionEvents2Cookie = (uint)ShellConstants.VSCOOKIE_NIL;
                    }

                    if(this.solutionEvents3Cookie != (uint)ShellConstants.VSCOOKIE_NIL)
                    {
                        ErrorHandler.ThrowOnFailure(this.SolutionBuildManager3.UnadviseUpdateSolutionEvents3(this.solutionEvents3Cookie));
                        this.solutionEvents3Cookie = (uint)ShellConstants.VSCOOKIE_NIL;
                    }

                    this.isDisposed = true;
                }
            }
        }
        #endregion
    }
}
