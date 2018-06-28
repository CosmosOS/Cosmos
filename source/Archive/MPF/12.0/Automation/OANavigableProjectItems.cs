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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Microsoft.VisualStudio.Project.Automation
{
    /// <summary>
    /// This can navigate a collection object only (partial implementation of ProjectItems interface)
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    [ComVisible(true)]
    public class OANavigableProjectItems : EnvDTE.ProjectItems
    {
        #region fields
        private OAProject project;
        private IList<EnvDTE.ProjectItem> items;
        private HierarchyNode nodeWithItems;
        #endregion

        #region properties
        /// <summary>
        /// Defines an internal list of project items
        /// </summary>
        internal IList<EnvDTE.ProjectItem> Items
        {
            get
            {
                return this.items;
            }
        }

        /// <summary>
        /// Defines a relationship to the associated project.
        /// </summary>
        internal OAProject Project
        {
            get
            {
                return this.project;
            }
        }

        /// <summary>
        /// Defines the node that contains the items
        /// </summary>
        internal HierarchyNode NodeWithItems
        {
            get
            {
                return this.nodeWithItems;
            }
        }
        #endregion

        #region ctor
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="project">The associated project.</param>
        /// <param name="nodeWithItems">The node that defines the items.</param>
        public OANavigableProjectItems(OAProject project, HierarchyNode nodeWithItems)
        {
            this.project = project;
            this.nodeWithItems = nodeWithItems;
            this.items = this.GetListOfProjectItems();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="project">The associated project.</param>
        /// <param name="items">A list of items that will make up the items defined by this object.</param>
        /// <param name="nodeWithItems">The node that defines the items.</param>
        public OANavigableProjectItems(OAProject project, IList<EnvDTE.ProjectItem> items, HierarchyNode nodeWithItems)
        {
            this.items = items;
            this.project = project;
            this.nodeWithItems = nodeWithItems;
        }
        #endregion

        #region EnvDTE.ProjectItems

        /// <summary>
        /// Gets a value indicating the number of objects in the collection.
        /// </summary>
        public virtual int Count
        {
            get
            {
                return items.Count;
            }
        }

        /// <summary>
        /// Gets the immediate parent object of a ProjectItems collection.
        /// </summary>
        public virtual object Parent
        {
            get
            {
                return this.nodeWithItems.GetAutomationObject();
            }
        }

        /// <summary>
        /// Gets an enumeration indicating the type of object.
        /// </summary>
        public virtual string Kind
        {
            get
            {
                // TODO:  Add OAProjectItems.Kind getter implementation
                return null;
            }
        }

        /// <summary>
        /// Gets the top-level extensibility object.
        /// </summary>
        public virtual EnvDTE.DTE DTE
        {
            get
            {
                return (EnvDTE.DTE)this.project.DTE;
            }
        }

        /// <summary>
        /// Gets the project hosting the project item or items.
        /// </summary>
        public virtual EnvDTE.Project ContainingProject
        {
            get
            {
                return this.project;
            }
        }

        /// <summary>
        /// Adds one or more ProjectItem objects from a directory to the ProjectItems collection. 
        /// </summary>
        /// <param name="directory">The directory from which to add the project item.</param>
        /// <returns>A ProjectItem object.</returns>
        public virtual EnvDTE.ProjectItem AddFromDirectory(string directory)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a new project item from an existing item template file and adds it to the project. 
        /// </summary>
        /// <param name="fileName">The full path and file name of the template project file.</param>
        /// <param name="name">The file name to use for the new project item.</param>
        /// <returns>A ProjectItem object. </returns>
        public virtual EnvDTE.ProjectItem AddFromTemplate(string fileName, string name)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a new folder in Solution Explorer. 
        /// </summary>
        /// <param name="name">The name of the folder node in Solution Explorer.</param>
        /// <param name="kind">The type of folder to add. The available values are based on vsProjectItemsKindConstants and vsProjectItemKindConstants</param>
        /// <returns>A ProjectItem object.</returns>
        public virtual EnvDTE.ProjectItem AddFolder(string name, string kind)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Copies a source file and adds it to the project. 
        /// </summary>
        /// <param name="filePath">The path and file name of the project item to be added.</param>
        /// <returns>A ProjectItem object. </returns>
        public virtual EnvDTE.ProjectItem AddFromFileCopy(string filePath)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds a project item from a file that is installed in a project directory structure. 
        /// </summary>
        /// <param name="fileName">The file name of the item to add as a project item. </param>
        /// <returns>A ProjectItem object. </returns>
        public virtual EnvDTE.ProjectItem AddFromFile(string fileName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get Project Item from index
        /// </summary>
        /// <param name="index">Either index by number (1-based) or by name can be used to get the item</param>
        /// <returns>Project Item. null is return if invalid index is specified</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        public virtual EnvDTE.ProjectItem Item(object index)
        {
            if(index is int)
            {
                int realIndex = (int)index - 1;
                if(realIndex >= 0 && realIndex < this.items.Count)
                {
                    return (EnvDTE.ProjectItem)items[realIndex];
                }
                return null;
            }
            else if(index is string)
            {
                string name = (string)index;
                foreach(EnvDTE.ProjectItem item in items)
                {
                    if(String.Compare(item.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Returns an enumeration for items in a collection. 
        /// </summary>
        /// <returns>An IEnumerator for this object.</returns>
        public virtual IEnumerator GetEnumerator()
        {
            if(this.items == null)
            {
                yield return null;
            }

            int count = items.Count;
            for(int i = 0; i < count; i++)
            {
                yield return items[i];
            }
        }

        #endregion

        #region virtual methods
        /// <summary>
        /// Retrives a list of items associated with the current node.
        /// </summary>
        /// <returns>A List of project items</returns>
        protected IList<EnvDTE.ProjectItem> GetListOfProjectItems()
        {
            return UIThread.DoOnUIThread(delegate()
            {
                List<EnvDTE.ProjectItem> list = new List<EnvDTE.ProjectItem>();
                for (HierarchyNode child = this.NodeWithItems.FirstChild; child != null; child = child.NextSibling)
                {
                    EnvDTE.ProjectItem item = child.GetAutomationObject() as EnvDTE.ProjectItem;
                    if (null != item)
                    {
                        list.Add(item);
                    }
                }

                return list;
            });
        }
        #endregion
    }
}
