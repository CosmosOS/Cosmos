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

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.VisualStudio.Project
{
    /// <summary>
    /// Defines the constant strings for various msbuild targets
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Ms")]
    public static class MsBuildTarget
    {
        public const string ResolveProjectReferences = "ResolveProjectReferences";
        public const string ResolveAssemblyReferences = "ResolveAssemblyReferences";
        public const string ResolveComReferences = "ResolveComReferences";
        public const string Build = "Build";
        public const string Rebuild = "ReBuild";
        public const string Clean = "Clean";
    }

    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Ms")]
    public static class MsBuildGeneratedItemType
    {
        public const string ReferenceCopyLocalPaths = "ReferenceCopyLocalPaths";
        public const string ComReferenceWrappers = "ComReferenceWrappers";
    }

    /// <summary>
    /// Defines the constant strings used with project files.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "COM")]
    public static class ProjectFileConstants
    {
        public const string Include = "Include";
        public const string Name = "Name";
        public const string HintPath = "HintPath";
        public const string AssemblyName = "AssemblyName";
        public const string FinalOutputPath = "FinalOutputPath";
        public const string Project = "Project";
        public const string LinkedIntoProjectAt = "LinkedIntoProjectAt";
        public const string TypeGuid = "TypeGuid";
        public const string InstanceGuid = "InstanceGuid";
        public const string Private = "Private";
        public const string EmbedInteropTypes = "EmbedInteropTypes";
        public const string ProjectReference = "ProjectReference";
        public const string Reference = "Reference";
        public const string WebReference = "WebReference";
        public const string WebReferenceFolder = "WebReferenceFolder";
        public const string Folder = "Folder";
        public const string Content = "Content";
        public const string EmbeddedResource = "EmbeddedResource";
        public const string RootNamespace = "RootNamespace";
        public const string OutputType = "OutputType";
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "SubType")]
        public const string SubType = "SubType";
        public const string DependentUpon = "DependentUpon";
        public const string Compile = "Compile";
        public const string ReferencePath = "ReferencePath";
        public const string ResolvedProjectReferencePaths = "ResolvedProjectReferencePaths";
        public const string Configuration = "Configuration";
        public const string Platform = "Platform";
        public const string AvailablePlatforms = "AvailablePlatforms";
        public const string BuildVerbosity = "BuildVerbosity";
        public const string Template = "Template";
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "SubProject")]
        public const string SubProject = "SubProject";
        public const string BuildAction = "BuildAction";
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "COM")]
        public const string COMReference = "COMReference";
        public const string Guid = "Guid";
        public const string VersionMajor = "VersionMajor";
        public const string VersionMinor = "VersionMinor";
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Lcid")]
        public const string Lcid = "Lcid";
        public const string Isolated = "Isolated";
        public const string WrapperTool = "WrapperTool";
        public const string BuildingInsideVisualStudio = "BuildingInsideVisualStudio";
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Scc")]
        public const string SccProjectName = "SccProjectName";
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Scc")]
        public const string SccLocalPath = "SccLocalPath";
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Scc")]
        public const string SccAuxPath = "SccAuxPath";
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Scc")]
        public const string SccProvider = "SccProvider";
        public const string ProjectGuid = "ProjectGuid";
        public const string ProjectTypeGuids = "ProjectTypeGuids";
        public const string Generator = "Generator";
        public const string CustomToolNamespace = "CustomToolNamespace";
        public const string FlavorProperties = "FlavorProperties";
        public const string VisualStudio = "VisualStudio";
        public const string User = "User";
        public const string ApplicationDefinition = "ApplicationDefinition";
        public const string Link = "Link";
        public const string Page = "Page";
        public const string Resource = "Resource";
        public const string None = "None";
    }

    public static class ProjectFileAttributeValue
    {
        public const string Code = "Code";
        public const string Form = "Form";
        public const string Component = "Component";
        public const string Designer = "Designer";
        public const string UserControl = "UserControl";
    }

    internal static class ProjectFileValues
    {
        internal const string AnyCPU = "AnyCPU";
    }

    public enum WrapperToolAttributeValue
    {
        Primary,
        TlbImp
    }

    /// <summary>
    /// A set of constants that specify the default sort order for different types of hierarchy nodes.
    /// </summary>
    public static class DefaultSortOrderNode
    {
        public const int HierarchyNode = 1000;
        public const int FolderNode = 500;
        public const int NestedProjectNode = 200;
        public const int ReferenceContainerNode = 300;
    }

}
