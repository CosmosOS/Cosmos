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
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Microsoft.VisualStudio.Project
{
    /// <summary>
    /// This class handles opening, saving of file items in the hierarchy.
    /// </summary>

    public class FileDocumentManager : DocumentManager
    {
        #region ctors

        public FileDocumentManager(FileNode node)
            : base(node)
        {
        }
        #endregion

        #region overriden methods

        /// <summary>
        /// Open a file using the standard editor
        /// </summary>
        /// <param name="logicalView">In MultiView case determines view to be activated by IVsMultiViewDocumentView. For a list of logical view GUIDS, see constants starting with LOGVIEWID_ defined in NativeMethods class</param>
        /// <param name="docDataExisting">IntPtr to the IUnknown interface of the existing document data object</param>
        /// <param name="windowFrame">A reference to the window frame that is mapped to the file</param>
        /// <param name="windowFrameAction">Determine the UI action on the document window</param>
        /// <returns>If the method succeeds, it returns S_OK. If it fails, it returns an error code.</returns>
        public override int Open(ref Guid logicalView, IntPtr docDataExisting, out IVsWindowFrame windowFrame, WindowFrameShowAction windowFrameAction)
        {
            bool newFile = false;
            bool openWith = false;
            return this.Open(newFile, openWith, ref logicalView, docDataExisting, out windowFrame, windowFrameAction);
        }

        /// <summary>
        /// Open a file with a specific editor
        /// </summary>
        /// <param name="editorFlags">Specifies actions to take when opening a specific editor. Possible editor flags are defined in the enumeration Microsoft.VisualStudio.Shell.Interop.__VSOSPEFLAGS</param>
        /// <param name="editorType">Unique identifier of the editor type</param>
        /// <param name="physicalView">Name of the physical view. If null, the environment calls MapLogicalView on the editor factory to determine the physical view that corresponds to the logical view. In this case, null does not specify the primary view, but rather indicates that you do not know which view corresponds to the logical view</param>
        /// <param name="logicalView">In MultiView case determines view to be activated by IVsMultiViewDocumentView. For a list of logical view GUIDS, see constants starting with LOGVIEWID_ defined in NativeMethods class</param>
        /// <param name="docDataExisting">IntPtr to the IUnknown interface of the existing document data object</param>
        /// <param name="windowFrame">A reference to the window frame that is mapped to the file</param>
        /// <param name="windowFrameAction">Determine the UI action on the document window</param>
        /// <returns>If the method succeeds, it returns S_OK. If it fails, it returns an error code.</returns>
        public override int OpenWithSpecific(uint editorFlags, ref Guid editorType, string physicalView, ref Guid logicalView, IntPtr docDataExisting, out IVsWindowFrame windowFrame, WindowFrameShowAction windowFrameAction)
        {
            windowFrame = null;
            bool newFile = false;
            bool openWith = false;
            return this.Open(newFile, openWith, editorFlags, ref editorType, physicalView, ref logicalView, docDataExisting, out windowFrame, windowFrameAction);
        }

        #endregion

        #region public methods
        /// <summary>
        /// Open a file in a document window with a std editor
        /// </summary>
        /// <param name="newFile">Open the file as a new file</param>
        /// <param name="openWith">Use a dialog box to determine which editor to use</param>
        /// <param name="windowFrameAction">Determine the UI action on the document window</param>
        /// <returns>If the method succeeds, it returns S_OK. If it fails, it returns an error code.</returns>
        public int Open(bool newFile, bool openWith, WindowFrameShowAction windowFrameAction)
        {
            Guid logicalView = Guid.Empty;
            IVsWindowFrame windowFrame = null;
            return this.Open(newFile, openWith, logicalView, out windowFrame, windowFrameAction);
        }

        /// <summary>
        /// Open a file in a document window with a std editor
        /// </summary>
        /// <param name="newFile">Open the file as a new file</param>
        /// <param name="openWith">Use a dialog box to determine which editor to use</param>
        /// <param name="logicalView">In MultiView case determines view to be activated by IVsMultiViewDocumentView. For a list of logical view GUIDS, see constants starting with LOGVIEWID_ defined in NativeMethods class</param>
        /// <param name="frame">A reference to the window frame that is mapped to the file</param>
        /// <param name="windowFrameAction">Determine the UI action on the document window</param>
        /// <returns>If the method succeeds, it returns S_OK. If it fails, it returns an error code.</returns>
        public int Open(bool newFile, bool openWith, Guid logicalView, out IVsWindowFrame frame, WindowFrameShowAction windowFrameAction)
        {
            frame = null;
            IVsRunningDocumentTable rdt = this.Node.ProjectMgr.Site.GetService(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable;
            Debug.Assert(rdt != null, " Could not get running document table from the services exposed by this project");
            if(rdt == null)
            {
                return VSConstants.E_FAIL;
            }

            // First we see if someone else has opened the requested view of the file.
            _VSRDTFLAGS flags = _VSRDTFLAGS.RDT_NoLock;
            uint itemid;
            IntPtr docData = IntPtr.Zero;
            IVsHierarchy ivsHierarchy;
            uint docCookie;
            string path = this.GetFullPathForDocument();
            int returnValue = VSConstants.S_OK;

            try
            {
                ErrorHandler.ThrowOnFailure(rdt.FindAndLockDocument((uint)flags, path, out ivsHierarchy, out itemid, out docData, out docCookie));
                ErrorHandler.ThrowOnFailure(this.Open(newFile, openWith, ref logicalView, docData, out frame, windowFrameAction));
            }
            catch(COMException e)
            {
                Trace.WriteLine("Exception :" + e.Message);
                returnValue = e.ErrorCode;
            }
            finally
            {
                if(docData != IntPtr.Zero)
                {
                    Marshal.Release(docData);
                }
            }

            return returnValue;
        }

        #endregion

        #region virtual methods
        /// <summary>
        /// Open a file in a document window
        /// </summary>
        /// <param name="newFile">Open the file as a new file</param>
        /// <param name="openWith">Use a dialog box to determine which editor to use</param>
        /// <param name="logicalView">In MultiView case determines view to be activated by IVsMultiViewDocumentView. For a list of logical view GUIDS, see constants starting with LOGVIEWID_ defined in NativeMethods class</param>
        /// <param name="docDataExisting">IntPtr to the IUnknown interface of the existing document data object</param>
        /// <param name="windowFrame">A reference to the window frame that is mapped to the file</param>
        /// <param name="windowFrameAction">Determine the UI action on the document window</param>
        /// <returns>If the method succeeds, it returns S_OK. If it fails, it returns an error code.</returns>
        public virtual int Open(bool newFile, bool openWith, ref Guid logicalView, IntPtr docDataExisting, out IVsWindowFrame windowFrame, WindowFrameShowAction windowFrameAction)
        {
            windowFrame = null;
            Guid editorType = Guid.Empty;
            return this.Open(newFile, openWith, 0, ref editorType, null, ref logicalView, docDataExisting, out windowFrame, windowFrameAction);
        }

        #endregion

        #region helper methods

        private int Open(bool newFile, bool openWith, uint editorFlags, ref Guid editorType, string physicalView, ref Guid logicalView, IntPtr docDataExisting, out IVsWindowFrame windowFrame, WindowFrameShowAction windowFrameAction)
        {
            windowFrame = null;
            if(this.Node == null || this.Node.ProjectMgr == null || this.Node.ProjectMgr.IsClosed)
            {
                return VSConstants.E_FAIL;
            }

            Debug.Assert(this.Node != null, "No node has been initialized for the document manager");
            Debug.Assert(this.Node.ProjectMgr != null, "No project manager has been initialized for the document manager");
            Debug.Assert(this.Node is FileNode, "Node is not FileNode object");

            int returnValue = VSConstants.S_OK;
            string caption = this.GetOwnerCaption();
            string fullPath = this.GetFullPathForDocument();

            // Make sure that the file is on disk before we open the editor and display message if not found
            if(!((FileNode)this.Node).IsFileOnDisk(true))
            {
                // Inform clients that we have an invalid item (wrong icon)
                this.Node.OnInvalidateItems(this.Node.Parent);

                // Bail since we are not able to open the item
                // Do not return an error code otherwise an internal error message is shown. The scenario for this operation
                // normally is already a reaction to a dialog box telling that the item has been removed.
                return VSConstants.S_FALSE;
            }

            IVsUIShellOpenDocument uiShellOpenDocument = this.Node.ProjectMgr.Site.GetService(typeof(SVsUIShellOpenDocument)) as IVsUIShellOpenDocument;
            IOleServiceProvider serviceProvider = this.Node.ProjectMgr.Site.GetService(typeof(IOleServiceProvider)) as IOleServiceProvider;

            try
            {
                this.Node.ProjectMgr.OnOpenItem(fullPath);
                int result = VSConstants.E_FAIL;

                if(openWith)
                {
                    result = uiShellOpenDocument.OpenStandardEditor((uint)__VSOSEFLAGS.OSE_UseOpenWithDialog, fullPath, ref logicalView, caption, this.Node.ProjectMgr.InteropSafeIVsUIHierarchy, this.Node.ID, docDataExisting, serviceProvider, out windowFrame);
                }
                else
                {
                    __VSOSEFLAGS openFlags = 0;
                    if(newFile)
                    {
                        openFlags |= __VSOSEFLAGS.OSE_OpenAsNewFile;
                    }

                    //NOTE: we MUST pass the IVsProject in pVsUIHierarchy and the itemid
                    // of the node being opened, otherwise the debugger doesn't work.
                    if(editorType != Guid.Empty)
                    {
                        result = uiShellOpenDocument.OpenSpecificEditor(editorFlags, fullPath, ref editorType, physicalView, ref logicalView, caption, this.Node.ProjectMgr.InteropSafeIVsUIHierarchy, this.Node.ID, docDataExisting, serviceProvider, out windowFrame);
                    }
                    else
                    {
                        openFlags |= __VSOSEFLAGS.OSE_ChooseBestStdEditor;
                        result = uiShellOpenDocument.OpenStandardEditor((uint)openFlags, fullPath, ref logicalView, caption, this.Node.ProjectMgr.InteropSafeIVsUIHierarchy, this.Node.ID, docDataExisting, serviceProvider, out windowFrame);
                    }
                }

                if(result != VSConstants.S_OK && result != VSConstants.S_FALSE && result != VSConstants.OLE_E_PROMPTSAVECANCELLED)
                {
                    ErrorHandler.ThrowOnFailure(result);
                }

                if(windowFrame != null)
                {
                    object var;

                    if(newFile)
                    {
                        ErrorHandler.ThrowOnFailure(windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocData, out var));
                        IVsPersistDocData persistDocData = (IVsPersistDocData)var;
                        ErrorHandler.ThrowOnFailure(persistDocData.SetUntitledDocPath(fullPath));
                    }

                    var = null;
                    ErrorHandler.ThrowOnFailure(windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocCookie, out var));
                    this.Node.DocCookie = (uint)(int)var;

                    if(windowFrameAction == WindowFrameShowAction.Show)
                    {
                        ErrorHandler.ThrowOnFailure(windowFrame.Show());
                    }
                    else if(windowFrameAction == WindowFrameShowAction.ShowNoActivate)
                    {
                        ErrorHandler.ThrowOnFailure(windowFrame.ShowNoActivate());
                    }
                    else if(windowFrameAction == WindowFrameShowAction.Hide)
                    {
                        ErrorHandler.ThrowOnFailure(windowFrame.Hide());
                    }
                }
            }
            catch(COMException e)
            {
                Trace.WriteLine("Exception e:" + e.Message);
                returnValue = e.ErrorCode;
                CloseWindowFrame(ref windowFrame);
            }

            return returnValue;
        }


        #endregion
    }
}
