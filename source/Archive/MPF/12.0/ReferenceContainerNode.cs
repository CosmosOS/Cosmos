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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using MSBuild = Microsoft.Build.Evaluation;
using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;
using VsCommands = Microsoft.VisualStudio.VSConstants.VSStd97CmdID;
using VsCommands2K = Microsoft.VisualStudio.VSConstants.VSStd2KCmdID;

namespace Microsoft.VisualStudio.Project
{

    public class ReferenceContainerNode : HierarchyNode, IReferenceContainer
    {
        #region fields
        internal const string ReferencesNodeVirtualName = "References";
        #endregion

        #region ctor
        public ReferenceContainerNode(ProjectNode root)
            : base(root)
        {
            this.VirtualNodeName = ReferencesNodeVirtualName;
            this.ExcludeNodeFromScc = true;
        }
        #endregion

        #region Properties
        private static string[] supportedReferenceTypes = new string[] {
            ProjectFileConstants.ProjectReference,
            ProjectFileConstants.Reference,
            ProjectFileConstants.COMReference
        };
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        protected virtual string[] SupportedReferenceTypes
        {
            get { return supportedReferenceTypes; }
        }
        #endregion

        #region overridden properties
        public override int SortPriority
        {
            get
            {
                return DefaultSortOrderNode.ReferenceContainerNode;
            }
        }

        public override int MenuCommandId
        {
            get { return VsMenus.IDM_VS_CTXT_REFERENCEROOT; }
        }


        public override Guid ItemTypeGuid
        {
            get { return VSConstants.GUID_ItemType_VirtualFolder; }
        }


        public override string Url
        {
            get { return this.VirtualNodeName; }
        }

        public override string Caption
        {
            get
            {
                return SR.GetString(SR.ReferencesNodeName, CultureInfo.CurrentUICulture);
            }
        }


        private Automation.OAReferences references;
        internal override object Object
        {
            get
            {
                if(null == references)
                {
                    references = new Automation.OAReferences(this);
                }
                return references;
            }
        }

        #endregion

        #region overridden methods
        /// <summary>
        /// Returns an instance of the automation object for ReferenceContainerNode
        /// </summary>
        /// <returns>An intance of the Automation.OAReferenceFolderItem type if succeeeded</returns>
        public override object GetAutomationObject()
        {
            if(this.ProjectMgr == null || this.ProjectMgr.IsClosed)
            {
                return null;
            }

            return new Automation.OAReferenceFolderItem(this.ProjectMgr.GetAutomationObject() as Automation.OAProject, this);
        }

        /// <summary>
        /// Disable inline editing of Caption of a ReferendeContainerNode
        /// </summary>
        /// <returns>null</returns>
        public override string GetEditLabel()
        {
            return null;
        }


        public override object GetIconHandle(bool open)
        {
            return this.ProjectMgr.ImageHandler.GetIconHandle(open ? (int)ProjectNode.ImageName.OpenReferenceFolder : (int)ProjectNode.ImageName.ReferenceFolder);
        }


        /// <summary>
        /// References node cannot be dragged.
        /// </summary>
        /// <returns>A stringbuilder.</returns>
        protected internal override StringBuilder PrepareSelectedNodesForClipBoard()
        {
            return null;
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        protected override int ExcludeFromProject()
        {
            return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
        }

        protected override int QueryStatusOnNode(Guid cmdGroup, uint cmd, IntPtr pCmdText, ref QueryStatusResult result)
        {
            if(cmdGroup == VsMenus.guidStandardCommandSet97)
            {
                switch((VsCommands)cmd)
                {
                    case VsCommands.AddNewItem:
                    case VsCommands.AddExistingItem:
                        result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                        return VSConstants.S_OK;
                }
            }
            else if(cmdGroup == VsMenus.guidStandardCommandSet2K)
            {
                if((VsCommands2K)cmd == VsCommands2K.ADDREFERENCE)
                {
                    result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                    return VSConstants.S_OK;
                }
            }
            else
            {
                return (int)OleConstants.OLECMDERR_E_UNKNOWNGROUP;
            }
            return base.QueryStatusOnNode(cmdGroup, cmd, pCmdText, ref result);
        }

        protected override int ExecCommandOnNode(Guid cmdGroup, uint cmd, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if(cmdGroup == VsMenus.guidStandardCommandSet2K)
            {
                switch((VsCommands2K)cmd)
                {
                    case VsCommands2K.ADDREFERENCE:
                        return this.ProjectMgr.AddProjectReference();
                    case VsCommands2K.ADDWEBREFERENCE:
                        return this.ProjectMgr.AddWebReference();
                }
            }

            return base.ExecCommandOnNode(cmdGroup, cmd, nCmdexecopt, pvaIn, pvaOut);
        }

        protected override bool CanDeleteItem(__VSDELETEITEMOPERATION deleteOperation)
        {
            return false;
        }

        /// <summary>
        /// Defines whether this node is valid node for painting the refererences icon.
        /// </summary>
        /// <returns></returns>
        protected override bool CanShowDefaultIcon()
        {
            if(!String.IsNullOrEmpty(this.VirtualNodeName))
            {
                return true;
            }
            return false;
        }

        #endregion

        #region IReferenceContainer
        public IList<ReferenceNode> EnumReferences()
        {
            List<ReferenceNode> refs = new List<ReferenceNode>();
            for(HierarchyNode node = this.FirstChild; node != null; node = node.NextSibling)
            {
                ReferenceNode refNode = node as ReferenceNode;
                if(refNode != null)
                {
                    refs.Add(refNode);
                }
            }

            return refs;
        }
        /// <summary>
        /// Adds references to this container from a MSBuild project.
        /// </summary>
        public void LoadReferencesFromBuildProject(MSBuild.Project buildProject)
        {
            foreach(string referenceType in SupportedReferenceTypes)
            {
                IEnumerable<MSBuild.ProjectItem> referencesGroup = this.ProjectMgr.BuildProject.GetItems(referenceType);

                bool isAssemblyReference = referenceType == ProjectFileConstants.Reference;
                // If the project was loaded for browsing we should still create the nodes but as not resolved.
                if(isAssemblyReference && this.ProjectMgr.Build(MsBuildTarget.ResolveAssemblyReferences) != MSBuildResult.Successful)
                {
                    continue;
                }

                foreach (MSBuild.ProjectItem item in referencesGroup)
                {
                    ProjectElement element = new ProjectElement(this.ProjectMgr, item, false);

                    ReferenceNode node = CreateReferenceNode(referenceType, element);

                    if(node != null)
                    {
                        // Make sure that we do not want to add the item twice to the ui hierarchy
                        // We are using here the UI representation of the Node namely the Caption to find that out, in order to
                        // avoid different representation problems.
                        // Example :<Reference Include="EnvDTE80, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" />
                        //		  <Reference Include="EnvDTE80" />
                        bool found = false;
                        for(HierarchyNode n = this.FirstChild; n != null && !found; n = n.NextSibling)
                        {
                            if(String.Compare(n.Caption, node.Caption, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                found = true;
                            }
                        }

                        if(!found)
                        {
                            this.AddChild(node);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds a reference to this container using the selector data structure to identify it.
        /// </summary>
        /// <param name="selectorData">data describing selected component</param>
        /// <returns>Reference in case of a valid reference node has been created. Otherwise null</returns>
        public ReferenceNode AddReferenceFromSelectorData(VSCOMPONENTSELECTORDATA selectorData, string wrapperTool = null)
        {
            //Make sure we can edit the project file
            if(!this.ProjectMgr.QueryEditProjectFile(false))
            {
                throw Marshal.GetExceptionForHR(VSConstants.OLE_E_PROMPTSAVECANCELLED);
            }

            //Create the reference node
            ReferenceNode node = null;
            try
            {
                node = CreateReferenceNode(selectorData, wrapperTool);
            }
            catch(ArgumentException)
            {
                // Some selector data was not valid. 
            }

            // Does such a reference already exist in the project?
            ReferenceNode existingNode;
            if (node.IsAlreadyAdded(out existingNode))
            {
                return existingNode;
            }

            //Add the reference node to the project if we have a valid reference node
            if(node != null)
            {
                // This call will find if the reference is in the project and, in this case
                // will not add it again, so the parent node will not be set.
                node.AddReference();
                if(null == node.Parent)
                {
                    // The reference was not added, so we can not return this item because it
                    // is not inside the project.
                    return null;
                }
            }

            return node;
        }
        #endregion

        #region virtual methods
        protected virtual ReferenceNode CreateReferenceNode(string referenceType, ProjectElement element)
        {
            ReferenceNode node = null;
            if(referenceType == ProjectFileConstants.COMReference)
            {
                node = this.CreateComReferenceNode(element);
            }
            else if(referenceType == ProjectFileConstants.Reference)
            {
                node = this.CreateAssemblyReferenceNode(element);
            }
            else if(referenceType == ProjectFileConstants.ProjectReference)
            {
                node = this.CreateProjectReferenceNode(element);
            }

            return node;
        }

        protected virtual ReferenceNode CreateReferenceNode(VSCOMPONENTSELECTORDATA selectorData, string wrapperTool = null)
        {
            ReferenceNode node = null;
            switch(selectorData.type)
            {
                case VSCOMPONENTTYPE.VSCOMPONENTTYPE_Project:
                    node = this.CreateProjectReferenceNode(selectorData);
                    break;
                case VSCOMPONENTTYPE.VSCOMPONENTTYPE_File:
                // This is the case for managed assembly
                case VSCOMPONENTTYPE.VSCOMPONENTTYPE_ComPlus:
                    node = this.CreateFileComponent(selectorData, wrapperTool);
                    break;
                case VSCOMPONENTTYPE.VSCOMPONENTTYPE_Com2:
                    node = this.CreateComReferenceNode(selectorData, wrapperTool);
                    break;
            }

            return node;
        }
        #endregion

        #region Helper functions to add references
        /// <summary>
        /// Creates a project reference node given an existing project element.
        /// </summary>
        protected virtual ProjectReferenceNode CreateProjectReferenceNode(ProjectElement element)
        {
            return new ProjectReferenceNode(this.ProjectMgr, element);
        }
        /// <summary>
        /// Create a Project to Project reference given a VSCOMPONENTSELECTORDATA structure
        /// </summary>
        protected virtual ProjectReferenceNode CreateProjectReferenceNode(VSCOMPONENTSELECTORDATA selectorData)
        {
            return new ProjectReferenceNode(this.ProjectMgr, selectorData.bstrTitle, selectorData.bstrFile, selectorData.bstrProjRef);
        }

        /// <summary>
        /// Creates an assemby or com reference node given a selector data.
        /// </summary>
        protected virtual ReferenceNode CreateFileComponent(VSCOMPONENTSELECTORDATA selectorData, string wrapperTool = null)
        {
            if(null == selectorData.bstrFile)
            {
                throw new ArgumentNullException("selectorData");
            }

            // We have a path to a file, it could be anything
            // First see if it is a managed assembly
            bool tryToCreateAnAssemblyReference = true;
            if(File.Exists(selectorData.bstrFile))
            {
                try
                {
                    // We should not load the assembly in the current appdomain.
                    // If we do not do it like that and we load the assembly in the current appdomain then the assembly cannot be unloaded again. 
                    // The following problems might arose in that case.
                    // 1. Assume that a user is extending the MPF and  his project is creating a managed assembly dll.
                    // 2. The user opens VS and creates a project and builds it.
                    // 3. Then the user opens VS creates another project and adds a reference to the previously built assembly. This will load the assembly in the appdomain had we been using Assembly.ReflectionOnlyLoadFrom.
                    // 4. Then he goes back to the first project modifies it an builds it. A build error is issued that the assembly is used.

                    // GetAssemblyName is assured not to load the assembly.
                    tryToCreateAnAssemblyReference = (AssemblyName.GetAssemblyName(selectorData.bstrFile) != null);
                }
                catch(BadImageFormatException)
                {
                    // We have found the file and it is not a .NET assembly; no need to try to
                    // load it again.
                    tryToCreateAnAssemblyReference = false;
                }
                catch(FileLoadException)
                {
                    // We must still try to load from here because this exception is thrown if we want 
                    // to add the same assembly refererence from different locations.
                    tryToCreateAnAssemblyReference = true;
                }
            }

            ReferenceNode node = null;

            if(tryToCreateAnAssemblyReference)
            {
                // This might be a candidate for an assembly reference node. Try to load it.
                // CreateAssemblyReferenceNode will suppress BadImageFormatException if the node cannot be created.
                node = this.CreateAssemblyReferenceNode(selectorData.bstrFile);
            }

            // If no node has been created try to create a com reference node.
            if(node == null)
            {
                if(!File.Exists(selectorData.bstrFile))
                {
                    return null;
                }
                node = this.CreateComReferenceNode(selectorData, wrapperTool);
            }

            return node;
        }

        /// <summary>
        /// Creates an assembly refernce node from a project element.
        /// </summary>
        protected virtual AssemblyReferenceNode CreateAssemblyReferenceNode(ProjectElement element)
        {
            AssemblyReferenceNode node = null;
            try
            {
                node = new AssemblyReferenceNode(this.ProjectMgr, element);
            }
            catch(ArgumentNullException e)
            {
                Trace.WriteLine("Exception : " + e.Message);
            }
            catch(FileNotFoundException e)
            {
                Trace.WriteLine("Exception : " + e.Message);
            }
            catch(BadImageFormatException e)
            {
                Trace.WriteLine("Exception : " + e.Message);
            }
            catch(FileLoadException e)
            {
                Trace.WriteLine("Exception : " + e.Message);
            }
            catch(System.Security.SecurityException e)
            {
                Trace.WriteLine("Exception : " + e.Message);
            }

            return node;
        }
        /// <summary>
        /// Creates an assembly reference node from a file path.
        /// </summary>
        protected virtual AssemblyReferenceNode CreateAssemblyReferenceNode(string fileName)
        {
            AssemblyReferenceNode node = null;
            try
            {
                node = new AssemblyReferenceNode(this.ProjectMgr, fileName);
            }
            catch(ArgumentNullException e)
            {
                Trace.WriteLine("Exception : " + e.Message);
            }
            catch(FileNotFoundException e)
            {
                Trace.WriteLine("Exception : " + e.Message);
            }
            catch(BadImageFormatException e)
            {
                Trace.WriteLine("Exception : " + e.Message);
            }
            catch(FileLoadException e)
            {
                Trace.WriteLine("Exception : " + e.Message);
            }
            catch(System.Security.SecurityException e)
            {
                Trace.WriteLine("Exception : " + e.Message);
            }

            return node;
        }

        /// <summary>
        /// Creates a com reference node from the project element.
        /// </summary>
        protected virtual ComReferenceNode CreateComReferenceNode(ProjectElement reference)
        {
            return new ComReferenceNode(this.ProjectMgr, reference);
        }
        /// <summary>
        /// Creates a com reference node from a selector data.
        /// </summary>
        protected virtual ComReferenceNode CreateComReferenceNode(Microsoft.VisualStudio.Shell.Interop.VSCOMPONENTSELECTORDATA selectorData, string wrapperTool = null)
        {
            ComReferenceNode node = new ComReferenceNode(this.ProjectMgr, selectorData);
            return node;
        }
        #endregion

    }
}
