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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using IServiceProvider = System.IServiceProvider;
using VSConstants = Microsoft.VisualStudio.VSConstants;

namespace Microsoft.VisualStudio.Project.Automation
{
    /// <summary>
    /// Represents an automation object for a file in a project
    /// </summary>
    [SuppressMessage("Microsoft.Interoperability", "CA1405:ComVisibleTypeBaseTypesShouldBeComVisible")]
    [ComVisible(true)]
    public class OAFileItem : OAProjectItem<FileNode>
    {
        #region ctors
        public OAFileItem(OAProject project, FileNode node)
            : base(project, node)
        {
        }

        #endregion

        #region overridden methods
        /// <summary>
        /// Returns the dirty state of the document.
        /// </summary>
        /// <exception cref="InvalidOperationException">Is thrown if the project is closed or it the service provider attached to the project is invalid.</exception>
        /// <exception cref="ComException">Is thrown if the dirty state cannot be retrived.</exception>
        public override bool IsDirty
        {
            get
            {
                return UIThread.DoOnUIThread(delegate()
                {
                    if (this.Node == null || this.Node.ProjectMgr == null || this.Node.ProjectMgr.IsClosed || this.Node.ProjectMgr.Site == null)
                    {
                        throw new InvalidOperationException();
                    }
                    bool isDirty = false;

                    using (AutomationScope scope = new AutomationScope(this.Node.ProjectMgr.Site))
                    {
                        DocumentManager manager = this.Node.GetDocumentManager();

                        if (manager == null)
                        {
                            throw new InvalidOperationException();
                        }

                        bool isOpen, isOpenedByUs;
                        uint docCookie;
                        IVsPersistDocData persistDocData;
                        manager.GetDocInfo(out isOpen, out isDirty, out isOpenedByUs, out docCookie, out persistDocData);
                    }

                    return isDirty;
                });
            }

        }

        /// <summary>
        /// Gets the Document associated with the item, if one exists.
        /// </summary>
        public override EnvDTE.Document Document
        {
            get
            {
                return UIThread.DoOnUIThread(delegate()
                {
                    if (this.Node == null || this.Node.ProjectMgr == null || this.Node.ProjectMgr.IsClosed || this.Node.ProjectMgr.Site == null)
                    {
                        throw new InvalidOperationException();
                    }

                    EnvDTE.Document document = null;

                    using (AutomationScope scope = new AutomationScope(this.Node.ProjectMgr.Site))
                    {
                        IVsUIHierarchy hier;
                        uint itemid;

                        IVsWindowFrame windowFrame;

                        VsShellUtilities.IsDocumentOpen(this.Node.ProjectMgr.Site, this.Node.Url, VSConstants.LOGVIEWID_Any, out hier, out itemid, out windowFrame);

                        if (windowFrame != null)
                        {
                            object var;
                            ErrorHandler.ThrowOnFailure(windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocCookie, out var));
                            object documentAsObject;
                            ErrorHandler.ThrowOnFailure(scope.Extensibility.GetDocumentFromDocCookie((int)var, out documentAsObject));
                            if (documentAsObject == null)
                            {
                                throw new InvalidOperationException();
                            }
                            else
                            {
                                document = (Document)documentAsObject;
                            }
                        }

                    }

                    return document;
                });
            }
        }


        /// <summary>
        /// Opens the file item in the specified view.
        /// </summary>
        /// <param name="ViewKind">Specifies the view kind in which to open the item (file)</param>
        /// <returns>Window object</returns>
        public override EnvDTE.Window Open(string viewKind)
        {
            return UIThread.DoOnUIThread(delegate()
            {
                if (this.Node == null || this.Node.ProjectMgr == null || this.Node.ProjectMgr.IsClosed || this.Node.ProjectMgr.Site == null)
                {
                    throw new InvalidOperationException();
                }

                IVsWindowFrame windowFrame = null;
                IntPtr docData = IntPtr.Zero;

                using (AutomationScope scope = new AutomationScope(this.Node.ProjectMgr.Site))
                {
                    try
                    {
                        // Validate input params
                        Guid logicalViewGuid = VSConstants.LOGVIEWID_Primary;
                        try
                        {
                            if (!(String.IsNullOrEmpty(viewKind)))
                            {
                                logicalViewGuid = new Guid(viewKind);
                            }
                        }
                        catch (FormatException)
                        {
                            // Not a valid guid
                            throw new ArgumentException(SR.GetString(SR.ParameterMustBeAValidGuid, CultureInfo.CurrentUICulture), "viewKind");
                        }

                        uint itemid;
                        IVsHierarchy ivsHierarchy;
                        uint docCookie;
                        IVsRunningDocumentTable rdt = this.Node.ProjectMgr.Site.GetService(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable;
                        Debug.Assert(rdt != null, " Could not get running document table from the services exposed by this project");
                        if (rdt == null)
                        {
                            throw new InvalidOperationException();
                        }

                        ErrorHandler.ThrowOnFailure(rdt.FindAndLockDocument((uint)_VSRDTFLAGS.RDT_NoLock, this.Node.Url, out ivsHierarchy, out itemid, out docData, out docCookie));

                        // Open the file using the IVsProject3 interface
                        ErrorHandler.ThrowOnFailure(this.Node.ProjectMgr.OpenItem(this.Node.ID, ref logicalViewGuid, docData, out windowFrame));

                    }
                    finally
                    {
                        if (docData != IntPtr.Zero)
                        {
                            Marshal.Release(docData);
                        }
                    }
                }

                // Get the automation object and return it
                return ((windowFrame != null) ? VsShellUtilities.GetWindowObject(windowFrame) : null);
            });
        }

        /// <summary>
        /// Saves the project item.
        /// </summary>
        /// <param name="fileName">The name with which to save the project or project item.</param>
        /// <exception cref="InvalidOperationException">Is thrown if the save operation failes.</exception>
        /// <exception cref="ArgumentNullException">Is thrown if fileName is null.</exception>
        public override void Save(string fileName)
        {
            this.DoSave(false, fileName);
        }

        /// <summary>
        /// Saves the project item.
        /// </summary>
        /// <param name="fileName">The file name with which to save the solution, project, or project item. If the file exists, it is overwritten</param>
        /// <returns>true if the rename was successful. False if Save as failes</returns>
        public override bool SaveAs(string fileName)
        {
            try
            {
                this.DoSave(true, fileName);
            }
            catch(InvalidOperationException)
            {
                return false;
            }
            catch(COMException)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Gets a value indicating whether the project item is open in a particular view type. 
        /// </summary>
        /// <param name="viewKind">A Constants.vsViewKind* indicating the type of view to check./param>
        /// <returns>A Boolean value indicating true if the project is open in the given view type; false if not. </returns>
        public override bool get_IsOpen(string viewKind)
        {
            return UIThread.DoOnUIThread(delegate()
            {
                if (this.Node == null || this.Node.ProjectMgr == null || this.Node.ProjectMgr.IsClosed || this.Node.ProjectMgr.Site == null)
                {
                    throw new InvalidOperationException();
                }

                // Validate input params
                Guid logicalViewGuid = VSConstants.LOGVIEWID_Primary;
                try
                {
                    if (!(String.IsNullOrEmpty(viewKind)))
                    {
                        logicalViewGuid = new Guid(viewKind);
                    }
                }
                catch (FormatException)
                {
                    // Not a valid guid
                    throw new ArgumentException(SR.GetString(SR.ParameterMustBeAValidGuid, CultureInfo.CurrentUICulture), "viewKind");
                }

                bool isOpen = false;

                using (AutomationScope scope = new AutomationScope(this.Node.ProjectMgr.Site))
                {
                    IVsUIHierarchy hier;
                    uint itemid;

                    IVsWindowFrame windowFrame;

                    isOpen = VsShellUtilities.IsDocumentOpen(this.Node.ProjectMgr.Site, this.Node.Url, logicalViewGuid, out hier, out itemid, out windowFrame);

                }

                return isOpen;
            });
        }

        /// <summary>
        /// Gets the ProjectItems for the object.
        /// </summary>
        public override ProjectItems ProjectItems
        {
            get
            {
                return UIThread.DoOnUIThread(delegate()
                {
                    if (this.Project.Project.CanFileNodesHaveChilds)
                        return new OAProjectItems(this.Project, this.Node);
                    else
                        return base.ProjectItems;
                });
            }
        }


        #endregion

        #region helpers
        /// <summary>
        /// Saves or Save As the  file
        /// </summary>
        /// <param name="isCalledFromSaveAs">Flag determining which Save method called , the SaveAs or the Save.</param>
        /// <param name="fileName">The name of the project file.</param>        
        private void DoSave(bool isCalledFromSaveAs, string fileName)
        {
            UIThread.DoOnUIThread(delegate()
            {
                if (fileName == null)
                {
                    throw new ArgumentNullException("fileName");
                }

                if (this.Node == null || this.Node.ProjectMgr == null || this.Node.ProjectMgr.IsClosed || this.Node.ProjectMgr.Site == null)
                {
                    throw new InvalidOperationException();
                }

                using (AutomationScope scope = new AutomationScope(this.Node.ProjectMgr.Site))
                {
                    IntPtr docData = IntPtr.Zero;

                    try
                    {
                        IVsRunningDocumentTable rdt = this.Node.ProjectMgr.Site.GetService(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable;
                        Debug.Assert(rdt != null, " Could not get running document table from the services exposed by this project");
                        if (rdt == null)
                        {
                            throw new InvalidOperationException();
                        }

                        // First we see if someone else has opened the requested view of the file.
                        uint itemid;
                        IVsHierarchy ivsHierarchy;
                        uint docCookie;
                        int canceled;
                        string url = this.Node.Url;

                        ErrorHandler.ThrowOnFailure(rdt.FindAndLockDocument((uint)_VSRDTFLAGS.RDT_NoLock, url, out ivsHierarchy, out itemid, out docData, out docCookie));

                        // If an empty file name is passed in for Save then make the file name the project name.
                        if (!isCalledFromSaveAs && fileName.Length == 0)
                        {
                            ErrorHandler.ThrowOnFailure(this.Node.ProjectMgr.SaveItem(VSSAVEFLAGS.VSSAVE_SilentSave, url, this.Node.ID, docData, out canceled));
                        }
                        else
                        {
                            Utilities.ValidateFileName(this.Node.ProjectMgr.Site, fileName);

                            // Compute the fullpath from the directory of the existing Url.
                            string fullPath = fileName;
                            if (!Path.IsPathRooted(fileName))
                            {
                                string directory = Path.GetDirectoryName(url);
                                fullPath = Path.Combine(directory, fileName);
                            }

                            if (!isCalledFromSaveAs)
                            {
                                if (!NativeMethods.IsSamePath(this.Node.Url, fullPath))
                                {
                                    throw new InvalidOperationException();
                                }

                                ErrorHandler.ThrowOnFailure(this.Node.ProjectMgr.SaveItem(VSSAVEFLAGS.VSSAVE_SilentSave, fullPath, this.Node.ID, docData, out canceled));
                            }
                            else
                            {
                                ErrorHandler.ThrowOnFailure(this.Node.ProjectMgr.SaveItem(VSSAVEFLAGS.VSSAVE_SilentSave, fullPath, this.Node.ID, docData, out canceled));
                            }
                        }

                        if (canceled == 1)
                        {
                            throw new InvalidOperationException();
                        }
                    }
                    catch (COMException e)
                    {
                        throw new InvalidOperationException(e.Message);
                    }
                    finally
                    {
                        if (docData != IntPtr.Zero)
                        {
                            Marshal.Release(docData);
                        }
                    }
                }
            });
        }
        #endregion

    }
}
