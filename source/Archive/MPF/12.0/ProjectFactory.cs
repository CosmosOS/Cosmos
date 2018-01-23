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
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using MSBuild = Microsoft.Build.Evaluation;
using MSBuildExecution = Microsoft.Build.Execution;

namespace Microsoft.VisualStudio.Project
{
    /// <summary>
    /// Creates projects within the solution
    /// </summary>

    public abstract class ProjectFactory : Microsoft.VisualStudio.Shell.Flavor.FlavoredProjectFactoryBase //, IVsAsynchronousProjectCreate
    {
        #region fields
        private Microsoft.VisualStudio.Shell.Package package;
        private System.IServiceProvider site;
        private static readonly Lazy<IVsTaskSchedulerService> taskSchedulerService = new Lazy<IVsTaskSchedulerService>(() => Package.GetGlobalService(typeof(SVsTaskSchedulerService)) as IVsTaskSchedulerService);

        /// <summary>
        /// The msbuild engine that we are going to use.
        /// </summary>
        private MSBuild.ProjectCollection buildEngine;

        /// <summary>
        /// The msbuild project for the project file.
        /// </summary>
        private MSBuild.Project buildProject;
        #endregion

        #region properties
        protected Microsoft.VisualStudio.Shell.Package Package
        {
            get
            {
                return this.package;
            }
        }

        protected System.IServiceProvider Site
        {
            get
            {
                return this.site;
            }
        }

        /// <summary>
        /// The msbuild engine that we are going to use.
        /// </summary>
        protected MSBuild.ProjectCollection BuildEngine
        {
            get
            {
                return this.buildEngine;
            }
        }

        /// <summary>
        /// The msbuild project for the temporary project file.
        /// </summary>
        protected MSBuild.Project BuildProject
        {
            get
            {
                return this.buildProject;
            }
            set
            {
                this.buildProject = value;
            }
        }
        #endregion

        #region ctor
        protected ProjectFactory(Microsoft.VisualStudio.Shell.Package package)
        {
            this.package = package;
            this.site = package;

            // Please be aware that this methods needs that ServiceProvider is valid, thus the ordering of calls in the ctor matters.
            this.buildEngine = Utilities.InitializeMsBuildEngine(this.buildEngine, this.site);
        }
        #endregion

        #region methods

        public virtual bool CanCreateProjectAsynchronously(ref Guid rguidProjectID, string filename, uint flags)
        {
            return true;
        }

        public void OnBeforeCreateProjectAsync(ref Guid rguidProjectID, string filename, string location, string pszName, uint flags)
        {
        }

        public virtual IVsTask CreateProjectAsync(ref Guid rguidProjectID, string filename, string location, string pszName, uint flags)
        {
            Guid iid = typeof(IVsHierarchy).GUID;
            return VsTaskLibraryHelper.CreateAndStartTask(taskSchedulerService.Value, VsTaskRunContext.UIThreadBackgroundPriority, VsTaskLibraryHelper.CreateTaskBody(() =>
                {
                    IntPtr project;
                    int cancelled;
                    CreateProject(filename, location, pszName, flags, ref iid, out project, out cancelled);
                    if (cancelled != 0)
                    {
                        throw new OperationCanceledException();
                    }

                    return Marshal.GetObjectForIUnknown(project);
                }));
        }

        #endregion

        #region abstract methods
        protected abstract ProjectNode CreateProject();
        #endregion

        #region overriden methods
        /// <summary>
        /// Rather than directly creating the project, ask VS to initate the process of
        /// creating an aggregated project in case we are flavored. We will be called
        /// on the IVsAggregatableProjectFactory to do the real project creation.
        /// </summary>
        /// <param name="fileName">Project file</param>
        /// <param name="location">Path of the project</param>
        /// <param name="name">Project Name</param>
        /// <param name="flags">Creation flags</param>
        /// <param name="projectGuid">Guid of the project</param>
        /// <param name="project">Project that end up being created by this method</param>
        /// <param name="canceled">Was the project creation canceled</param>
        protected override void CreateProject(string fileName, string location, string name, uint flags, ref Guid projectGuid, out IntPtr project, out int canceled)
        {
            project = IntPtr.Zero;
            canceled = 0;

            // Get the list of GUIDs from the project/template
            string guidsList = this.ProjectTypeGuids(fileName);

            // Launch the aggregate creation process (we should be called back on our IVsAggregatableProjectFactoryCorrected implementation)
            IVsCreateAggregateProject aggregateProjectFactory = (IVsCreateAggregateProject)this.Site.GetService(typeof(SVsCreateAggregateProject));
            int hr = aggregateProjectFactory.CreateAggregateProject(guidsList, fileName, location, name, flags, ref projectGuid, out project);
            if(hr == VSConstants.E_ABORT)
                canceled = 1;
            ErrorHandler.ThrowOnFailure(hr);

            // This needs to be done after the aggregation is completed (to avoid creating a non-aggregated CCW) and as a result we have to go through the interface
            IProjectEventsProvider eventsProvider = (IProjectEventsProvider)Marshal.GetTypedObjectForIUnknown(project, typeof(IProjectEventsProvider));
            eventsProvider.ProjectEventsProvider = this.GetProjectEventsProvider();

            this.buildProject = null;
        }


        /// <summary>
        /// Instantiate the project class, but do not proceed with the
        /// initialization just yet.
        /// Delegate to CreateProject implemented by the derived class.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification="The global property handles is instantiated here and used in the project node that will Dispose it")]
        protected override object PreCreateForOuter(IntPtr outerProjectIUnknown)
        {
            Debug.Assert(this.buildProject != null, "The build project should have been initialized before calling PreCreateForOuter.");
            
            // Please be very carefull what is initialized here on the ProjectNode. Normally this should only instantiate and return a project node.
            // The reason why one should very carefully add state to the project node here is that at this point the aggregation has not yet been created and anything that would cause a CCW for the project to be created would cause the aggregation to fail
            // Our reasoning is that there is no other place where state on the project node can be set that is known by the Factory and has to execute before the Load method.
            ProjectNode node = this.CreateProject();
            Debug.Assert(node != null, "The project failed to be created");
            node.BuildEngine = this.buildEngine;
            node.BuildProject = this.buildProject;
            node.Package = this.package as ProjectPackage;
            return node;
        }

        /// <summary>
        /// Retrives the list of project guids from the project file.
        /// If you don't want your project to be flavorable, override
        /// to only return your project factory Guid:
        ///      return this.GetType().GUID.ToString("B");
        /// </summary>
        /// <param name="file">Project file to look into to find the Guid list</param>
        /// <returns>List of semi-colon separated GUIDs</returns>
        protected override string ProjectTypeGuids(string file)
        {
            // Load the project so we can extract the list of GUIDs
           
            this.buildProject = Utilities.ReinitializeMsBuildProject(this.buildEngine, file, this.buildProject);

            // Retrieve the list of GUIDs, if it is not specify, make it our GUID
            string guids = buildProject.GetPropertyValue(ProjectFileConstants.ProjectTypeGuids);
            if(String.IsNullOrEmpty(guids))
                guids = this.GetType().GUID.ToString("B");

            return guids;
        }
        #endregion

        #region helpers
        private IProjectEvents GetProjectEventsProvider()
        {
            ProjectPackage projectPackage = this.package as ProjectPackage;
            Debug.Assert(projectPackage != null, "Package not inherited from framework");
            if(projectPackage != null)
            {
                foreach(SolutionListener listener in projectPackage.SolutionListeners)
                {
                    IProjectEvents projectEvents = listener as IProjectEvents;
                    if(projectEvents != null)
                    {
                        return projectEvents;
                    }
                }
            }

            return null;
        }

        #endregion
    }
}
