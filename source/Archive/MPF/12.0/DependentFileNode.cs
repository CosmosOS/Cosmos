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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio;
using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;
using VsCommands = Microsoft.VisualStudio.VSConstants.VSStd97CmdID;
using VsCommands2K = Microsoft.VisualStudio.VSConstants.VSStd2KCmdID;

namespace Microsoft.VisualStudio.Project
{
    /// <summary>
    /// Defines the logic for all dependent file nodes (solution explorer icon, commands etc.)
    /// </summary>

    [ComVisible(true)]
    public class DependentFileNode : FileNode
    {
        #region fields
        /// <summary>
        /// Defines if the node has a name relation to its parent node
        /// e.g. Form1.ext and Form1.resx are name related (until first occurence of extention separator)
        /// </summary>
        #endregion

        #region Properties
        public override int ImageIndex
        {
            get { return (this.CanShowDefaultIcon() ? (int)ProjectNode.ImageName.DependentFile : (int)ProjectNode.ImageName.MissingFile); }
        }
        #endregion

        #region ctor
        /// <summary>
        /// Constructor for the DependentFileNode
        /// </summary>
        /// <param name="root">Root of the hierarchy</param>
        /// <param name="e">Associated project element</param>
        public DependentFileNode(ProjectNode root, ProjectElement element)
            : base(root, element)
        {
            this.HasParentNodeNameRelation = false;
        }


        #endregion

        #region overridden methods
        /// <summary>
        /// Disable rename
        /// </summary>
        /// <param name="label">new label</param>
        /// <returns>E_NOTIMPLE in order to tell the call that we do not support rename</returns>
        public override string GetEditLabel()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a handle to the icon that should be set for this node
        /// </summary>
        /// <param name="open">Whether the folder is open, ignored here.</param>
        /// <returns>Handle to icon for the node</returns>
        public override object GetIconHandle(bool open)
        {
            return this.ProjectMgr.ImageHandler.GetIconHandle(this.ImageIndex);
        }

        /// <summary>
        /// Disable certain commands for dependent file nodes 
        /// </summary>
        protected override int QueryStatusOnNode(Guid cmdGroup, uint cmd, IntPtr pCmdText, ref QueryStatusResult result)
        {
            if(cmdGroup == VsMenus.guidStandardCommandSet97)
            {
                switch((VsCommands)cmd)
                {
                    case VsCommands.Copy:
                    case VsCommands.Paste:
                    case VsCommands.Cut:
                    case VsCommands.Rename:
                        result |= QueryStatusResult.NOTSUPPORTED;
                        return VSConstants.S_OK;

                    case VsCommands.ViewCode:
                    case VsCommands.Open:
                    case VsCommands.OpenWith:
                        result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                        return VSConstants.S_OK;
                }
            }
            else if(cmdGroup == VsMenus.guidStandardCommandSet2K)
            {
                if((VsCommands2K)cmd == VsCommands2K.EXCLUDEFROMPROJECT)
                {
                    result |= QueryStatusResult.NOTSUPPORTED;
                    return VSConstants.S_OK;
                }
            }
            else
            {
                return (int)OleConstants.OLECMDERR_E_UNKNOWNGROUP;
            }
            return base.QueryStatusOnNode(cmdGroup, cmd, pCmdText, ref result);
        }

        /// <summary>
        /// DependentFileNodes node cannot be dragged.
        /// </summary>
        /// <returns>null</returns>
        protected internal override StringBuilder PrepareSelectedNodesForClipBoard()
        {
            return null;
        }

        protected override NodeProperties CreatePropertiesObject()
        {
            return new DependentFileNodeProperties(this);
        }

        /// <summary>
        /// Redraws the state icon if the node is not excluded from source control.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Scc")]
        protected internal override void UpdateSccStateIcons()
        {
            if(!this.ExcludeNodeFromScc)
            {
                this.Parent.ReDraw(UIHierarchyElement.SccState);
            }
        }
        #endregion

    }
}
