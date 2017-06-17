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
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using ShellConstants = Microsoft.VisualStudio.Shell.Interop.Constants;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.VisualStudio.Project
{


    public abstract class ProjectDocumentsListener : IVsTrackProjectDocumentsEvents2, IDisposable
    {
        #region fields
        private uint eventsCookie;
        private IVsTrackProjectDocuments2 projectDocTracker;
        private ServiceProvider serviceProvider;
        private bool isDisposed;
        /// <summary>
        /// Defines an object that will be a mutex for this object for synchronizing thread calls.
        /// </summary>
        private static volatile object Mutex = new object();
        #endregion

        #region ctors
        protected ProjectDocumentsListener(ServiceProvider serviceProviderParameter)
        {
            if (serviceProviderParameter == null)
            {
                throw new ArgumentNullException("serviceProviderParameter");
            }

            this.serviceProvider = serviceProviderParameter;
            this.projectDocTracker = this.serviceProvider.GetService(typeof(SVsTrackProjectDocuments)) as IVsTrackProjectDocuments2;

            Debug.Assert(this.projectDocTracker != null, "Could not get the IVsTrackProjectDocuments2 object from the services exposed by this project");

            if(this.projectDocTracker == null)
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

        protected IVsTrackProjectDocuments2 ProjectDocumentTracker2
        {
            get
            {
                return this.projectDocTracker;
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

        #region IVsTrackProjectDocumentsEvents2 Members

        public virtual int OnAfterAddDirectoriesEx(int cProjects, int cDirectories, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSADDDIRECTORYFLAGS[] rgFlags)
        {
            return VSConstants.E_NOTIMPL;
        }

        public virtual int OnAfterAddFilesEx(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSADDFILEFLAGS[] rgFlags)
        {
            return VSConstants.E_NOTIMPL;
        }

        public virtual int OnAfterRemoveDirectories(int cProjects, int cDirectories, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSREMOVEDIRECTORYFLAGS[] rgFlags)
        {
            return VSConstants.E_NOTIMPL;
        }

        public virtual int OnAfterRemoveFiles(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSREMOVEFILEFLAGS[] rgFlags)
        {
            return VSConstants.E_NOTIMPL;
        }

        public virtual int OnAfterRenameDirectories(int cProjects, int cDirs, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgszMkOldNames, string[] rgszMkNewNames, VSRENAMEDIRECTORYFLAGS[] rgFlags)
        {
            return VSConstants.E_NOTIMPL;
        }

        public virtual int OnAfterRenameFiles(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgszMkOldNames, string[] rgszMkNewNames, VSRENAMEFILEFLAGS[] rgFlags)
        {
            return VSConstants.E_NOTIMPL;
        }

        public virtual int OnAfterSccStatusChanged(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, uint[] rgdwSccStatus)
        {
            return VSConstants.E_NOTIMPL;
        }

        public virtual int OnQueryAddDirectories(IVsProject pProject, int cDirectories, string[] rgpszMkDocuments, VSQUERYADDDIRECTORYFLAGS[] rgFlags, VSQUERYADDDIRECTORYRESULTS[] pSummaryResult, VSQUERYADDDIRECTORYRESULTS[] rgResults)
        {
            return VSConstants.E_NOTIMPL;
        }

        public virtual int OnQueryAddFiles(IVsProject pProject, int cFiles, string[] rgpszMkDocuments, VSQUERYADDFILEFLAGS[] rgFlags, VSQUERYADDFILERESULTS[] pSummaryResult, VSQUERYADDFILERESULTS[] rgResults)
        {
            return VSConstants.E_NOTIMPL;
        }

        public virtual int OnQueryRemoveDirectories(IVsProject pProject, int cDirectories, string[] rgpszMkDocuments, VSQUERYREMOVEDIRECTORYFLAGS[] rgFlags, VSQUERYREMOVEDIRECTORYRESULTS[] pSummaryResult, VSQUERYREMOVEDIRECTORYRESULTS[] rgResults)
        {
            return VSConstants.E_NOTIMPL;
        }

        public virtual int OnQueryRemoveFiles(IVsProject pProject, int cFiles, string[] rgpszMkDocuments, VSQUERYREMOVEFILEFLAGS[] rgFlags, VSQUERYREMOVEFILERESULTS[] pSummaryResult, VSQUERYREMOVEFILERESULTS[] rgResults)
        {
            return VSConstants.E_NOTIMPL;
        }

        public virtual int OnQueryRenameDirectories(IVsProject pProject, int cDirs, string[] rgszMkOldNames, string[] rgszMkNewNames, VSQUERYRENAMEDIRECTORYFLAGS[] rgFlags, VSQUERYRENAMEDIRECTORYRESULTS[] pSummaryResult, VSQUERYRENAMEDIRECTORYRESULTS[] rgResults)
        {
            return VSConstants.E_NOTIMPL;
        }

        public virtual int OnQueryRenameFiles(IVsProject pProject, int cFiles, string[] rgszMkOldNames, string[] rgszMkNewNames, VSQUERYRENAMEFILEFLAGS[] rgFlags, VSQUERYRENAMEFILERESULTS[] pSummaryResult, VSQUERYRENAMEFILERESULTS[] rgResults)
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
            if(this.ProjectDocumentTracker2 != null)
            {
                ErrorHandler.ThrowOnFailure(this.ProjectDocumentTracker2.AdviseTrackProjectDocumentsEvents(this, out this.eventsCookie));
            }
        }

        /// <summary>
        /// The method that does the cleanup.
        /// </summary>
        /// <param name="disposing"></param>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsTrackProjectDocuments2.UnadviseTrackProjectDocumentsEvents(System.UInt32)")]
        protected virtual void Dispose(bool disposing)
        {
            // Everybody can go here.
            if(!this.isDisposed)
            {
                // Synchronize calls to the Dispose simulteniously.
                lock(Mutex)
                {
                    if(disposing && this.eventsCookie != (uint)ShellConstants.VSCOOKIE_NIL && this.ProjectDocumentTracker2 != null)
                    {
                        this.ProjectDocumentTracker2.UnadviseTrackProjectDocumentsEvents((uint)this.eventsCookie);
                        this.eventsCookie = (uint)ShellConstants.VSCOOKIE_NIL;
                    }

                    this.isDisposed = true;
                }
            }
        }
        #endregion
    }
}
