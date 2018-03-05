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
using System.Reflection;
using System.Globalization;
using System.Resources;
using System.Text;
using System.Threading;
using System.ComponentModel;
using System.Security.Permissions;

namespace Microsoft.VisualStudio.Project
{
    [AttributeUsage(AttributeTargets.All)]
    internal sealed class SRDescriptionAttribute : DescriptionAttribute
    {
        private bool replaced;

        public SRDescriptionAttribute(string description)
            : base(description)
        {
        }

        public override string Description
        {
            get
            {
                if(!replaced)
                {
                    replaced = true;
                    DescriptionValue = SR.GetString(base.Description, CultureInfo.CurrentUICulture);
                }
                return base.Description;
            }
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    internal sealed class SRCategoryAttribute : CategoryAttribute
    {

        public SRCategoryAttribute(string category)
            : base(category)
        {
        }

        protected override string GetLocalizedString(string value)
        {
            return SR.GetString(value, CultureInfo.CurrentUICulture);
        }
    }
    internal sealed class SR
    {
        internal const string AddReferenceDialogTitle = "AddReferenceDialogTitle";
        internal const string AddToNullProjectError = "AddToNullProjectError";
        internal const string Advanced = "Advanced";
        internal const string AssemblyReferenceAlreadyExists = "AssemblyReferenceAlreadyExists";
        internal const string AttributeLoad = "AttributeLoad";
        internal const string BuildAction = "BuildAction";
        internal const string BuildActionDescription = "BuildActionDescription";
        internal const string BuildCaption = "BuildCaption";
        internal const string BuildVerbosity = "BuildVerbosity";
        internal const string BuildVerbosityDescription = "BuildVerbosityDescription";
        internal const string BuildEventError = "BuildEventError";
        internal const string CancelQueryEdit = "CancelQueryEdit";
        internal const string CannotAddFileThatIsOpenInEditor = "CannotAddFileThatIsOpenInEditor";
        internal const string CanNotSaveFileNotOpeneInEditor = "CanNotSaveFileNotOpeneInEditor";
        internal const string cli1 = "cli1";
        internal const string Compile = "Compile";
        internal const string ConfirmExtensionChange = "ConfirmExtensionChange";
        internal const string Content = "Content";
        internal const string CopyToLocal = "CopyToLocal";
        internal const string CopyToLocalDescription = "CopyToLocalDescription";
        internal const string EmbedInteropTypes = "EmbedInteropTypes";
        internal const string EmbedInteropTypesDescription = "EmbedInteropTypesDescription";
        internal const string CustomTool = "CustomTool";
        internal const string CustomToolDescription = "CustomToolDescription";
        internal const string CustomToolNamespace = "CustomToolNamespace";
        internal const string CustomToolNamespaceDescription = "CustomToolNamespaceDescription";
        internal const string DetailsImport = "DetailsImport";
        internal const string DetailsUserImport = "DetailsUserImport";
        internal const string DetailsItem = "DetailsItem";
        internal const string DetailsItemLocation = "DetailsItemLocation";
        internal const string DetailsProperty = "DetailsProperty";
        internal const string DetailsTarget = "DetailsTarget";
        internal const string DetailsUsingTask = "DetailsUsingTask";
        internal const string Detailed = "Detailed";
        internal const string Diagnostic = "Diagnostic";
        internal const string DirectoryExistError = "DirectoryExistError";
        internal const string EditorViewError = "EditorViewError";
        internal const string EmbeddedResource = "EmbeddedResource";
        internal const string Error = "Error";
        internal const string ErrorInvalidFileName = "ErrorInvalidFileName";
        internal const string ErrorInvalidProjectName = "ErrorInvalidProjectName";
        internal const string ErrorReferenceCouldNotBeAdded = "ErrorReferenceCouldNotBeAdded";
        internal const string ErrorMsBuildRegistration = "ErrorMsBuildRegistration";
        internal const string ErrorSaving = "ErrorSaving";
        internal const string Exe = "Exe";
        internal const string ExpectedObjectOfType = "ExpectedObjectOfType";
        internal const string FailedToGetService = "FailedToGetService";
        internal const string FailedToRetrieveProperties = "FailedToRetrieveProperties";
        internal const string FileNameCannotContainALeadingPeriod = "FileNameCannotContainALeadingPeriod";
        internal const string FileCannotBeRenamedToAnExistingFile = "FileCannotBeRenamedToAnExistingFile";
        internal const string FileAlreadyExistsAndCannotBeRenamed = "FileAlreadyExistsAndCannotBeRenamed";
        internal const string FileAlreadyExists = "FileAlreadyExists";
        internal const string FileAlreadyExistsCaption = "FileAlreadyExistsCaption";
        internal const string FileAlreadyInProject = "FileAlreadyInProject";
        internal const string FileAlreadyInProjectCaption = "FileAlreadyInProjectCaption";
        internal const string FileCopyError = "FileCopyError";
        internal const string FileName = "FileName";
        internal const string FileNameDescription = "FileNameDescription";
        internal const string FileOrFolderAlreadyExists = "FileOrFolderAlreadyExists";
        internal const string FileOrFolderCannotBeFound = "FileOrFolderCannotBeFound";
        internal const string FileProperties = "FileProperties";
        internal const string FolderName = "FolderName";
        internal const string FolderNameDescription = "FolderNameDescription";
        internal const string FolderProperties = "FolderProperties";
        internal const string FullPath = "FullPath";
        internal const string FullPathDescription = "FullPathDescription";
        internal const string ItemDoesNotExistInProjectDirectory = "ItemDoesNotExistInProjectDirectory";
        internal const string InvalidAutomationObject = "InvalidAutomationObject";
        internal const string InvalidLoggerType = "InvalidLoggerType";
        internal const string InvalidParameter = "InvalidParameter";
        internal const string Library = "Library";
        internal const string LinkedItemsAreNotSupported = "LinkedItemsAreNotSupported";
        internal const string Minimal = "Minimal";
        internal const string Misc = "Misc";
        internal const string None = "None";
        internal const string Normal = "Normal";
        internal const string NestedProjectFailedToReload = "NestedProjectFailedToReload";
        internal const string OutputPath = "OutputPath";
        internal const string OutputPathDescription = "OutputPathDescription";
        internal const string PasteFailed = "PasteFailed";
        internal const string ParameterMustBeAValidGuid = "ParameterMustBeAValidGuid";
        internal const string ParameterMustBeAValidItemId = "ParameterMustBeAValidItemId";
        internal const string ParameterCannotBeNullOrEmpty = "ParameterCannotBeNullOrEmpty";
        internal const string PathTooLong = "PathTooLong";
        internal const string ProjectContainsCircularReferences = "ProjectContainsCircularReferences";
        internal const string Program = "Program";
        internal const string Project = "Project";
        internal const string ProjectFile = "ProjectFile";
        internal const string ProjectFileDescription = "ProjectFileDescription";
        internal const string ProjectFolder = "ProjectFolder";
        internal const string ProjectFolderDescription = "ProjectFolderDescription";
        internal const string ProjectProperties = "ProjectProperties";
        internal const string Quiet = "Quiet";
        internal const string QueryReloadNestedProject = "QueryReloadNestedProject";
        internal const string ReferenceAlreadyExists = "ReferenceAlreadyExists";
        internal const string ReferencesNodeName = "ReferencesNodeName";
        internal const string ReferenceProperties = "ReferenceProperties";
        internal const string RefName = "RefName";
        internal const string RefNameDescription = "RefNameDescription";
        internal const string RenameFolder = "RenameFolder";
        internal const string RTL = "RTL";
        internal const string SaveCaption = "SaveCaption";
        internal const string SaveModifiedDocuments = "SaveModifiedDocuments";
        internal const string SaveOfProjectFileOutsideCurrentDirectory = "SaveOfProjectFileOutsideCurrentDirectory";
        internal const string StandardEditorViewError = "StandardEditorViewError";
        internal const string Settings = "Settings";
        internal const string URL = "URL";
        internal const string UseOfDeletedItemError = "UseOfDeletedItemError";
        internal const string Warning = "Warning";
        internal const string WinExe = "WinExe";
        internal const string CannotLoadUnknownTargetFrameworkProject = "CannotLoadUnknownTargetFrameworkProject";
        internal const string ReloadPromptOnTargetFxChanged = "ReloadPromptOnTargetFxChanged";
        internal const string ReloadPromptOnTargetFxChangedCaption = "ReloadPromptOnTargetFxChangedCaption";

        static SR loader;
        ResourceManager resources;

        private static Object s_InternalSyncObject;
        private static Object InternalSyncObject
        {
            get
            {
                if(s_InternalSyncObject == null)
                {
                    Object o = new Object();
                    Interlocked.CompareExchange(ref s_InternalSyncObject, o, null);
                }
                return s_InternalSyncObject;
            }
        }

        internal SR()
        {
            resources = new System.Resources.ResourceManager("Microsoft.VisualStudio.Project", this.GetType().Assembly);
        }

        private static SR GetLoader()
        {
            if(loader == null)
            {
                lock(InternalSyncObject)
                {
                    if(loader == null)
                    {
                        loader = new SR();
                    }
                }
            }

            return loader;
        }

        private static CultureInfo Culture
        {
            get { return null/*use ResourceManager default, CultureInfo.CurrentUICulture*/; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static ResourceManager Resources
        {
            get
            {
                return GetLoader().resources;
            }
        }

        public static string GetString(string name, params object[] args)
        {
            SR sys = GetLoader();
            if(sys == null)
                return null;
            string res = sys.resources.GetString(name, SR.Culture);

            if(args != null && args.Length > 0)
            {
                return String.Format(CultureInfo.CurrentCulture, res, args);
            }
            else
            {
                return res;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static string GetString(string name)
        {
            SR sys = GetLoader();
            if(sys == null)
                return null;
            return sys.resources.GetString(name, SR.Culture);
        }

        public static string GetString(string name, CultureInfo culture)
        {
            SR sys = GetLoader();
            if(sys == null)
                return null;
            return sys.resources.GetString(name, culture);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static object GetObject(string name)
        {
            SR sys = GetLoader();
            if(sys == null)
                return null;
            return sys.resources.GetObject(name, SR.Culture);
        }
    }
}
