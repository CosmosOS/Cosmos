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
using VSLangProj;

namespace Microsoft.VisualStudio.Project.Automation
{
    /// <summary>
    /// Represents the automation equivalent of ReferenceNode
    /// </summary>
    /// <typeparam name="RefType"></typeparam>
    [SuppressMessage("Microsoft.Naming", "CA1715:IdentifiersShouldHaveCorrectPrefix", MessageId = "T")]
    [ComVisible(true)]
    public abstract class OAReferenceBase<RefType> : Reference
        where RefType : ReferenceNode
    {
        #region fields
        private RefType referenceNode;
        #endregion

        #region ctors
        protected OAReferenceBase(RefType referenceNode)
        {
            this.referenceNode = referenceNode;
        }
        #endregion

        #region properties
        protected RefType BaseReferenceNode
        {
            get { return referenceNode; }
        }
        #endregion

        #region Reference Members
        public virtual int BuildNumber
        {
            get { return 0; }
        }

        public virtual References Collection
        {
            get
            {
                return BaseReferenceNode.Parent.Object as References;
            }
        }

        public virtual EnvDTE.Project ContainingProject
        {
            get
            {
                return BaseReferenceNode.ProjectMgr.GetAutomationObject() as EnvDTE.Project;
            }
        }

        public virtual bool CopyLocal
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public virtual string Culture
        {
            get { throw new NotImplementedException(); }
        }

        public virtual EnvDTE.DTE DTE
        {
            get
            {
                return BaseReferenceNode.ProjectMgr.Site.GetService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
            }
        }

        public virtual string Description
        {
            get
            {
                return this.Name;
            }
        }

        public virtual string ExtenderCATID
        {
            get { throw new NotImplementedException(); }
        }

        public virtual object ExtenderNames
        {
            get { throw new NotImplementedException(); }
        }

        public virtual string Identity
        {
            get { throw new NotImplementedException(); }
        }

        public virtual int MajorVersion
        {
            get { return 0; }
        }

        public virtual int MinorVersion
        {
            get { return 0; }
        }

        public virtual string Name
        {
            get { throw new NotImplementedException(); }
        }

        public virtual string Path
        {
            get
            {
                return BaseReferenceNode.Url;
            }
        }

        public virtual string PublicKeyToken
        {
            get { throw new NotImplementedException(); }
        }

        public virtual void Remove()
        {
            BaseReferenceNode.Remove(false);
        }

        public virtual int RevisionNumber
        {
            get { return 0; }
        }

        public virtual EnvDTE.Project SourceProject
        {
            get { return null; }
        }

        public virtual bool StrongName
        {
            get { return false; }
        }

        public virtual prjReferenceType Type
        {
            get { throw new NotImplementedException(); }
        }

        public virtual string Version
        {
            get { return new Version().ToString(); }
        }

        public virtual object get_Extender(string ExtenderName)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
