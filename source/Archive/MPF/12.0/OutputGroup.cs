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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using MSBuild = Microsoft.Build.Evaluation;
using MSBuildExecution = Microsoft.Build.Execution;

namespace Microsoft.VisualStudio.Project
{
    /// <summary>
    /// Allows projects to group outputs according to usage.
    /// </summary>

    public class OutputGroup : IVsOutputGroup2
    {
        #region fields
        private ProjectConfig projectCfg;
        private ProjectNode project;

        private List<Output> outputs = new List<Output>();
        private Output keyOutput;
        private string name;
        private string targetName;
        #endregion

        #region properties
        /// <summary>
        /// Get the project configuration object associated with this output group
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Cfg")]
        protected ProjectConfig ProjectCfg
        {
            get { return projectCfg; }
        }

        /// <summary>
        /// Get the project object that produces this output group.
        /// </summary>
        protected ProjectNode Project
        {
            get { return project; }
        }

        /// <summary>
        /// Gets the msbuild target name which is assciated to the outputgroup.
        /// ProjectNode defines a static collection of output group names and their associated MsBuild target
        /// </summary>
        protected string TargetName
        {
            get { return targetName; }
        }
        #endregion

        #region ctors

        /// <summary>
        /// Constructor for IVSOutputGroup2 implementation
        /// </summary>
        /// <param name="outputName">Name of the output group. See VS_OUTPUTGROUP_CNAME_Build in vsshell.idl for the list of standard values</param>
        /// <param name="msBuildTargetName">MSBuild target name</param>
        /// <param name="projectManager">Project that produce this output</param>
        /// <param name="configuration">Configuration that produce this output</param>
        public OutputGroup(string outputName, string msBuildTargetName, ProjectNode projectManager, ProjectConfig configuration)
        {
            if(outputName == null)
                throw new ArgumentNullException("outputName");
            if(msBuildTargetName == null)
                throw new ArgumentNullException("outputName");
            if(projectManager == null)
                throw new ArgumentNullException("projectManager");
            if(configuration == null)
                throw new ArgumentNullException("configuration");

            name = outputName;
            targetName = msBuildTargetName;
            project = projectManager;
            projectCfg = configuration;
        }
        #endregion

        #region virtual methods
        protected virtual void Refresh()
        {
            // Let MSBuild know which configuration we are working with
            project.SetConfiguration(projectCfg.ConfigName);

            // Generate dependencies if such a task exist
            const string generateDependencyList = "AllProjectOutputGroups";
            if(project.BuildProject.Targets.ContainsKey(generateDependencyList))
            {
                bool succeeded = false;
                project.BuildTarget(generateDependencyList, out succeeded);
                Debug.Assert(succeeded, "Failed to build target: " + generateDependencyList);
            }

            // Rebuild the content of our list of output
            string outputType = this.targetName + "Output";
            this.outputs.Clear();
            foreach (MSBuildExecution.ProjectItemInstance assembly in project.CurrentConfig.GetItems(outputType))
            {
                Output output = new Output(project, assembly);
                this.outputs.Add(output);

                // See if it is our key output
                if(String.Compare(assembly.GetMetadataValue("IsKeyOutput"), true.ToString(), StringComparison.OrdinalIgnoreCase) == 0)
                    keyOutput = output;
            }

            project.SetCurrentConfiguration();

            // Now that the group is built we have to check if it is invalidated by a property
            // change on the project.
            project.OnProjectPropertyChanged += new EventHandler<ProjectPropertyChangedArgs>(OnProjectPropertyChanged);
        }

        public virtual void InvalidateGroup()
        {
            // Set keyOutput to null so that a refresh will be performed the next time
            // a property getter is called.
            if(null != keyOutput)
            {
                // Once the group is invalidated there is no more reason to listen for events.
                project.OnProjectPropertyChanged -= new EventHandler<ProjectPropertyChangedArgs>(OnProjectPropertyChanged);
            }
            keyOutput = null;
        }
        #endregion

        #region event handlers
        private void OnProjectPropertyChanged(object sender, ProjectPropertyChangedArgs args)
        {
            // In theory here we should decide if we have to invalidate the group according with the kind of property
            // that is changed.
            InvalidateGroup();
        }
        #endregion

        #region IVsOutputGroup2 Members

        public virtual int get_CanonicalName(out string pbstrCanonicalName)
        {
            pbstrCanonicalName = this.name;
            return VSConstants.S_OK;
        }

        public virtual int get_DeployDependencies(uint celt, IVsDeployDependency[] rgpdpd, uint[] pcActual)
        {
            return VSConstants.E_NOTIMPL;
        }

        public virtual int get_Description(out string pbstrDescription)
        {
            pbstrDescription = null;

            string description;
            int hr = this.get_CanonicalName(out description);
            if(ErrorHandler.Succeeded(hr))
                pbstrDescription = this.Project.GetOutputGroupDescription(description);
            return hr;
        }

        public virtual int get_DisplayName(out string pbstrDisplayName)
        {
            pbstrDisplayName = null;

            string displayName;
            int hr = this.get_CanonicalName(out displayName);
            if(ErrorHandler.Succeeded(hr))
                pbstrDisplayName = this.Project.GetOutputGroupDisplayName(displayName);
            return hr;
        }

        public virtual int get_KeyOutput(out string pbstrCanonicalName)
        {
            pbstrCanonicalName = null;
            if(keyOutput == null)
                Refresh();
            if(keyOutput == null)
            {
                pbstrCanonicalName = String.Empty;
                return VSConstants.S_FALSE;
            }
            return keyOutput.get_CanonicalName(out pbstrCanonicalName);
        }

        public virtual int get_KeyOutputObject(out IVsOutput2 ppKeyOutput)
        {
            if(keyOutput == null)
                Refresh();
            ppKeyOutput = keyOutput;
            if(ppKeyOutput == null)
                return VSConstants.S_FALSE;
            return VSConstants.S_OK;
        }

        public virtual int get_Outputs(uint celt, IVsOutput2[] rgpcfg, uint[] pcActual)
        {
            // Ensure that we are refreshed.  This is somewhat of a hack that enables project to
            // project reference scenarios to work.  Normally, output groups are populated as part
            // of build.  However, in the project to project reference case, what ends up happening
            // is that the referencing projects requests the referenced project's output group
            // before a build is done on the referenced project.
            //
            // Furthermore, the project auto toolbox manager requires output groups to be populated
            // on project reopen as well...
            //
            // In the end, this is probably the right thing to do, though -- as it keeps the output
            // groups always up to date.
            Refresh();

            // See if only the caller only wants to know the count
            if(celt == 0 || rgpcfg == null)
            {
                if(pcActual != null && pcActual.Length > 0)
                    pcActual[0] = (uint)outputs.Count;
                return VSConstants.S_OK;
            }

            // Fill the array with our outputs
            uint count = 0;
            foreach(Output output in outputs)
            {
                if(rgpcfg.Length > count && celt > count && output != null)
                {
                    rgpcfg[count] = output;
                    ++count;
                }
            }

            if(pcActual != null && pcActual.Length > 0)
                pcActual[0] = count;

            // If the number asked for does not match the number returned, return S_FALSE
            return (count == celt) ? VSConstants.S_OK : VSConstants.S_FALSE;
        }

        public virtual int get_ProjectCfg(out IVsProjectCfg2 ppIVsProjectCfg2)
        {
            ppIVsProjectCfg2 = (IVsProjectCfg2)this.projectCfg;
            return VSConstants.S_OK;
        }

        public virtual int get_Property(string pszProperty, out object pvar)
        {
            pvar = project.GetProjectProperty(pszProperty);
            return VSConstants.S_OK;
        }

        #endregion
    }
}
