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
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Build.Execution;

namespace Microsoft.VisualStudio.Project
{
    class Output : IVsOutput2
    {
        private ProjectNode project;
        private ProjectItemInstance output;

        /// <summary>
        /// Constructor for IVSOutput2 implementation
        /// </summary>
        /// <param name="projectManager">Project that produce this output</param>
        /// <param name="outputAssembly">MSBuild generated item corresponding to the output assembly (by default, these would be of type MainAssembly</param>
        public Output(ProjectNode projectManager, ProjectItemInstance outputAssembly)
        {
            if(projectManager == null)
                throw new ArgumentNullException("projectManager");
            if(outputAssembly == null)
                throw new ArgumentNullException("outputAssembly");

            project = projectManager;
            output = outputAssembly;
        }

        #region IVsOutput2 Members

        public int get_CanonicalName(out string pbstrCanonicalName)
        {
            // Get the output assembly path (including the name)
            pbstrCanonicalName = output.GetMetadataValue(ProjectFileConstants.FinalOutputPath);
            Debug.Assert(!String.IsNullOrEmpty(pbstrCanonicalName), "Output Assembly not defined");

            // Make sure we have a full path
            if(!System.IO.Path.IsPathRooted(pbstrCanonicalName))
            {
                pbstrCanonicalName = new Url(project.BaseURI, pbstrCanonicalName).AbsoluteUrl;
            }
            return VSConstants.S_OK;
        }

        /// <summary>
        /// This path must start with file:/// if it wants other project
        /// to be able to reference the output on disk.
        /// If the output is not on disk, then this requirement does not
        /// apply as other projects probably don't know how to access it.
        /// </summary>
        public virtual int get_DeploySourceURL(out string pbstrDeploySourceURL)
        {
            string path = output.GetMetadataValue(ProjectFileConstants.FinalOutputPath);
            if(string.IsNullOrEmpty(path))
            {
                throw new InvalidOperationException();
            }
            if(path.Length < 9 || String.Compare(path.Substring(0, 8), "file:///", StringComparison.OrdinalIgnoreCase) != 0)
                path = "file:///" + path; // TODO: does not work with '#' char, see e.g. bug 641942
            pbstrDeploySourceURL = path;
            return VSConstants.S_OK;
        }

        public int get_DisplayName(out string pbstrDisplayName)
        {
            return this.get_CanonicalName(out pbstrDisplayName);
        }

        public virtual int get_Property(string szProperty, out object pvar)
        {
            if (string.IsNullOrEmpty(szProperty))
            {
                pvar = null;
                return VSConstants.E_INVALIDARG;
            }

            if (string.Equals(szProperty, "OUTPUTLOC", StringComparison.OrdinalIgnoreCase))
            {
                szProperty = ProjectFileConstants.FinalOutputPath;
            }

            string value = output.GetMetadataValue(szProperty);
            pvar = value;

            // If we don't have a value, we are expected to return unimplemented
            if (string.IsNullOrEmpty(value))
            {
                return VSConstants.E_NOTIMPL;
            }

            // Special hack for COM2REG property: it's a bool rather than a string, and always true, for some reason.
            if (string.Equals(szProperty, "COM2REG", StringComparison.OrdinalIgnoreCase))
            {
                pvar = true;
            }

            return VSConstants.S_OK;
        }

        public int get_RootRelativeURL(out string pbstrRelativePath)
        {
            pbstrRelativePath = String.Empty;
            object variant;
            // get the corresponding property

            if(ErrorHandler.Succeeded(this.get_Property("TargetPath", out variant)))
            {
                string var = variant as String;

                if(var != null)
                {
                    pbstrRelativePath = var;
                }
            }

            return VSConstants.S_OK;
        }

        public virtual int get_Type(out Guid pguidType)
        {
            pguidType = Guid.Empty;
            throw new NotImplementedException();
        }

        #endregion
    }
}
