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

namespace Microsoft.VisualStudio.Project
{
    /// <summary>
    /// Used by a project to query the environment for permission to add, remove, or rename a file or directory in a solution
    /// </summary>
    internal class TrackDocumentsHelper
    {
        #region fields
        private ProjectNode projectMgr;
        #endregion

        #region properties

        #endregion

        #region ctors
        internal TrackDocumentsHelper(ProjectNode project)
        {
            this.projectMgr = project;
        }
        #endregion

        #region helper methods
        /// <summary>
        /// Gets the IVsTrackProjectDocuments2 object by asking the service provider for it.
        /// </summary>
        /// <returns>the IVsTrackProjectDocuments2 object</returns>
        private IVsTrackProjectDocuments2 GetIVsTrackProjectDocuments2()
        {
            Debug.Assert(this.projectMgr != null && !this.projectMgr.IsClosed && this.projectMgr.Site != null);

            IVsTrackProjectDocuments2 documentTracker = this.projectMgr.Site.GetService(typeof(SVsTrackProjectDocuments)) as IVsTrackProjectDocuments2;
            if(documentTracker == null)
            {
                throw new InvalidOperationException();
            }

            return documentTracker;
        }

        /// <summary>
        /// Asks the environment for permission to add files.
        /// </summary>
        /// <param name="files">The files to add.</param>
        /// <param name="flags">The VSQUERYADDFILEFLAGS flags associated to the files added</param>
        /// <returns>true if the file can be added, false if not.</returns>
        internal bool CanAddItems(string[] files, VSQUERYADDFILEFLAGS[] flags)
        {
            // If we are silent then we assume that the file can be added, since we do not want to trigger this event.
            if((this.projectMgr.EventTriggeringFlag & ProjectNode.EventTriggering.DoNotTriggerTrackerEvents) != 0)
            {
                return true;
            }

            if(files == null || files.Length == 0)
            {
                return false;
            }

            int len = files.Length;
            VSQUERYADDFILERESULTS[] summary = new VSQUERYADDFILERESULTS[1];
            ErrorHandler.ThrowOnFailure(this.GetIVsTrackProjectDocuments2().OnQueryAddFiles(this.projectMgr.InteropSafeIVsProject3, len, files, flags, summary, null));
            if(summary[0] == VSQUERYADDFILERESULTS.VSQUERYADDFILERESULTS_AddNotOK)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Notify the environment about a file just added
        /// </summary>
        internal void OnItemAdded(string file, VSADDFILEFLAGS flag)
        {
            if((this.projectMgr.EventTriggeringFlag & ProjectNode.EventTriggering.DoNotTriggerTrackerEvents) == 0)
            {
                ErrorHandler.ThrowOnFailure(this.GetIVsTrackProjectDocuments2().OnAfterAddFilesEx(this.projectMgr.InteropSafeIVsProject3, 1, new string[1] { file }, new VSADDFILEFLAGS[1] { flag }));
            }
        }

        /// <summary>
        ///  Asks the environment for permission to remove files.
        /// </summary>
        /// <param name="files">an array of files to remove</param>
        /// <param name="flags">The VSQUERYREMOVEFILEFLAGS associated to the files to be removed.</param>
        /// <returns>true if the files can be removed, false if not.</returns>
        internal bool CanRemoveItems(string[] files, VSQUERYREMOVEFILEFLAGS[] flags)
        {
            // If we are silent then we assume that the file can be removed, since we do not want to trigger this event.
            if((this.projectMgr.EventTriggeringFlag & ProjectNode.EventTriggering.DoNotTriggerTrackerEvents) != 0)
            {
                return true;
            }

            if(files == null || files.Length == 0)
            {
                return false;
            }
            int length = files.Length;

            VSQUERYREMOVEFILERESULTS[] summary = new VSQUERYREMOVEFILERESULTS[1];

            ErrorHandler.ThrowOnFailure(this.GetIVsTrackProjectDocuments2().OnQueryRemoveFiles(this.projectMgr.InteropSafeIVsProject3, length, files, flags, summary, null));
            if(summary[0] == VSQUERYREMOVEFILERESULTS.VSQUERYREMOVEFILERESULTS_RemoveNotOK)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Notify the environment about a file just removed
        /// </summary>
        internal void OnItemRemoved(string file, VSREMOVEFILEFLAGS flag)
        {
            if((this.projectMgr.EventTriggeringFlag & ProjectNode.EventTriggering.DoNotTriggerTrackerEvents) == 0)
            {
                ErrorHandler.ThrowOnFailure(this.GetIVsTrackProjectDocuments2().OnAfterRemoveFiles(this.projectMgr.InteropSafeIVsProject3, 1, new string[1] { file }, new VSREMOVEFILEFLAGS[1] { flag }));
            }
        }

        /// <summary>
        ///  Asks the environment for permission to rename files.
        /// </summary>
        /// <param name="oldFileName">Path to the file to be renamed.</param>
        /// <param name="newFileName">Path to the new file.</param>
        /// <param name="flag">The VSRENAMEFILEFLAGS associated with the file to be renamed.</param>
        /// <returns>true if the file can be renamed. Otherwise false.</returns>
        internal bool CanRenameItem(string oldFileName, string newFileName, VSRENAMEFILEFLAGS flag)
        {
            // If we are silent then we assume that the file can be renamed, since we do not want to trigger this event.
            if((this.projectMgr.EventTriggeringFlag & ProjectNode.EventTriggering.DoNotTriggerTrackerEvents) != 0)
            {
                return true;
            }

            int iCanContinue = 0;
            ErrorHandler.ThrowOnFailure(this.GetIVsTrackProjectDocuments2().OnQueryRenameFile(this.projectMgr.InteropSafeIVsProject3, oldFileName, newFileName, flag, out iCanContinue));
            return (iCanContinue != 0);
        }

        /// <summary>
        /// Get's called to tell the env that a file was renamed
        /// </summary>
        /// 
        internal void OnItemRenamed(string strOldName, string strNewName, VSRENAMEFILEFLAGS flag)
        {
            if((this.projectMgr.EventTriggeringFlag & ProjectNode.EventTriggering.DoNotTriggerTrackerEvents) == 0)
            {
                ErrorHandler.ThrowOnFailure(this.GetIVsTrackProjectDocuments2().OnAfterRenameFile(this.projectMgr.InteropSafeIVsProject3, strOldName, strNewName, flag));
            }
        }
        #endregion
    }
}

