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
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.Project
{

    public class EnumDependencies : IVsEnumDependencies
    {
        private List<IVsDependency> dependencyList = new List<IVsDependency>();

        private uint nextIndex;

        public EnumDependencies(IList<IVsDependency> dependencyList)
        {
            if (dependencyList == null)
            {
                throw new ArgumentNullException("dependencyList");
            }

            foreach(IVsDependency dependency in dependencyList)
            {
                this.dependencyList.Add(dependency);
            }
        }

        public EnumDependencies(IList<IVsBuildDependency> dependencyList)
        {
            if (dependencyList == null)
            {
                throw new ArgumentNullException("dependencyList");
            }

            foreach(IVsBuildDependency dependency in dependencyList)
            {
                this.dependencyList.Add(dependency);
            }
        }

        public int Clone(out IVsEnumDependencies enumDependencies)
        {
            enumDependencies = new EnumDependencies(this.dependencyList);
            ErrorHandler.ThrowOnFailure(enumDependencies.Skip(this.nextIndex));
            return VSConstants.S_OK;
        }

        public int Next(uint elements, IVsDependency[] dependencies, out uint elementsFetched)
        {
            elementsFetched = 0;
            if (dependencies == null)
            {
                throw new ArgumentNullException("dependencies");
            }
            
            uint fetched = 0;
            int count = this.dependencyList.Count;

            while(this.nextIndex < count && elements > 0 && fetched < count)
            {
                dependencies[fetched] = this.dependencyList[(int)this.nextIndex];
                this.nextIndex++;
                fetched++;
                elements--;

            }

            elementsFetched = fetched;

            // Did we get 'em all?
            return (elements == 0 ? VSConstants.S_OK : VSConstants.S_FALSE);
        }

        public int Reset()
        {
            this.nextIndex = 0;
            return VSConstants.S_OK;
        }

        public int Skip(uint elements)
        {
            this.nextIndex += elements;
            uint count = (uint)this.dependencyList.Count;

            if(this.nextIndex > count)
            {
                this.nextIndex = count;
                return VSConstants.S_FALSE;
            }

            return VSConstants.S_OK;
        }
    }
}
