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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using MSBuild = Microsoft.Build.Evaluation;

namespace Microsoft.VisualStudio.Project
{

    /// <summary>
    /// This interface defines the rules for handling build dependency on a project container.
    /// </summary>
    /// <remarks>Normally this should be an internal interface but since it shouldbe available for the aggregator it must be made public.</remarks>
    [ComVisible(true)]

    public interface IBuildDependencyOnProjectContainer
    {
        /// <summary>
        /// Defines whether the nested projects should be build with the parent project.
        /// </summary>
        bool BuildNestedProjectsOnBuild
        {
            get;
            set;
        }

        /// <summary>
        /// Enumerates the nested hierachies present that will participate in the build dependency update.
        /// </summary>
        /// <returns>A list of hierrachies.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Hierachies")]
        IVsHierarchy[] EnumNestedHierachiesForBuildDependency();
    }

    /// <summary>
    /// Interface for manipulating build dependency
    /// </summary>
    /// <remarks>Normally this should be an internal interface but since it shouldbe available for the aggregator it must be made public.</remarks>
    [ComVisible(true)]

    public interface IBuildDependencyUpdate
    {
        /// <summary>
        /// Defines a container for storing BuildDependencies
        /// </summary>

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        IVsBuildDependency[] BuildDependencies
        {
            get;
        }

        /// <summary>
        /// Adds a BuildDependency to the container
        /// </summary>
        /// <param name="dependency">The dependency to add</param>
        void AddBuildDependency(IVsBuildDependency dependency);

        /// <summary>
        /// Removes the builddependency from teh container.
        /// </summary>
        /// <param name="dependency">The dependency to add</param>
        void RemoveBuildDependency(IVsBuildDependency dependency);

    }

    /// <summary>
    /// Provides access to the reference data container.
    /// </summary>
    /// <remarks>Normally this should be an internal interface but since it should be available for
    /// the aggregator it must be made public.</remarks>
    [ComVisible(true)]
    public interface IReferenceContainerProvider
    {
        IReferenceContainer GetReferenceContainer();
    }

    /// <summary>
    /// Defines a container for manipulating references
    /// </summary>
    /// <remarks>Normally this should be an internal interface but since it should be available for
    /// the aggregator it must be made public.</remarks>
    [ComVisible(true)]
    public interface IReferenceContainer
    {
        IList<ReferenceNode> EnumReferences();
        ReferenceNode AddReferenceFromSelectorData(VSCOMPONENTSELECTORDATA selectorData, string wrapperTool = null);
        void LoadReferencesFromBuildProject(MSBuild.Project buildProject);
    }

    /// <summary>
    /// Defines the events that are internally defined for communication with other subsytems.
    /// </summary>
    [ComVisible(true)]
    public interface IProjectEvents
    {
        /// <summary>
        /// Event raised just after the project file opened.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1713:EventsShouldNotHaveBeforeOrAfterPrefix")]
        event EventHandler<AfterProjectFileOpenedEventArgs> AfterProjectFileOpened;

        /// <summary>
        /// Event raised before the project file closed.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1713:EventsShouldNotHaveBeforeOrAfterPrefix")]
        event EventHandler<BeforeProjectFileClosedEventArgs> BeforeProjectFileClosed;
    }

    /// <summary>
    /// Defines the interface that will specify ehethrr the object is a project events listener.
    /// </summary>
    [ComVisible(true)]
    public interface IProjectEventsListener
    {

        /// <summary>
        /// Is the object a project events listener.
        /// </summary>
        /// <returns></returns>
        bool IsProjectEventsListener
        { get; set; }

    }

    /// <summary>
    /// Enable getting and setting the project events provider
    /// </summary>
    [ComVisible(true)]
    public interface IProjectEventsProvider
    {
        /// <summary>
        /// Defines the provider for the project events
        /// </summary>
        IProjectEvents ProjectEventsProvider
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Defines support for single file generator
    /// </summary>
    public interface ISingleFileGenerator
    {
        ///<summary>
        /// Runs the generator on the item represented by the document moniker.
        /// </summary>
        /// <param name="document"></param>
        void RunGenerator(string document);
    }
}
