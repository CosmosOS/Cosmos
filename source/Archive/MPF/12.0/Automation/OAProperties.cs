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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Microsoft.VisualStudio.Project.Automation
{
    /// <summary>
    /// Contains all of the properties of a given object that are contained in a generic collection of properties.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]

    public class OAProperties : EnvDTE.Properties
    {
        #region fields
        private NodeProperties target;
        private Dictionary<string, EnvDTE.Property> properties = new Dictionary<string, EnvDTE.Property>();
        #endregion

        #region properties
        /// <summary>
        /// Defines the NodeProperties object that contains the defines the properties.
        /// </summary>
        public NodeProperties Target
        {
            get
            {
                return this.target;
            }
        }

        /// <summary>
        /// The hierarchy node for the object which properties this item represent
        /// </summary>
        public HierarchyNode Node
        {
            get
            {
                return this.Target.Node;
            }
        }

        /// <summary>
        /// Defines a dictionary of the properties contained.
        /// </summary>
        public Dictionary<string, EnvDTE.Property> Properties
        {
            get
            {
                return this.properties;
            }
        }
        #endregion

        #region ctor
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public OAProperties(NodeProperties target)
        {
            System.Diagnostics.Debug.Assert(target != null);

            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            this.target = target;
            this.AddPropertiesFromType(target.GetType());
        }
        #endregion

        #region EnvDTE.Properties
        /// <summary>
        /// Microsoft Internal Use Only.
        /// </summary>
        public virtual object Application
        {
            get { return null; }
        }

        /// <summary>
        /// Gets a value indicating the number of objects in the collection.
        /// </summary>
        public int Count
        {
            get { return properties.Count; }
        }

        /// <summary>
        /// Gets the top-level extensibility object.
        /// </summary>
        public virtual EnvDTE.DTE DTE
        {
            get
            {
                return UIThread.DoOnUIThread(delegate()
                {
                    if (this.target == null || this.target.Node == null || this.target.Node.ProjectMgr == null || this.target.Node.ProjectMgr.IsClosed ||
                        this.target.Node.ProjectMgr.Site == null)
                    {
                        throw new InvalidOperationException();
                    }
                    return this.target.Node.ProjectMgr.Site.GetService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
                });
            }
        }

        /// <summary>
        /// Gets an enumeration for items in a collection. 
        /// </summary>
        /// <returns>An enumerator. </returns>
        public IEnumerator GetEnumerator()
        {
            if(this.properties == null)
            {
                yield return null;
            }

            if(this.properties.Count == 0)
            {
                yield return new OANullProperty(this);
            }

            IEnumerator enumerator = this.properties.Values.GetEnumerator();

            while(enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }

        /// <summary>
        /// Returns an indexed member of a Properties collection. 
        /// </summary>
        /// <param name="index">The index at which to return a mamber.</param>
        /// <returns>A Property object.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        public virtual EnvDTE.Property Item(object index)
        {
            if(index is string)
            {
                string indexAsString = (string)index;
                if(this.properties.ContainsKey(indexAsString))
                {
                    return (EnvDTE.Property)this.properties[indexAsString];
                }
            }
            else if(index is int)
            {
                int realIndex = (int)index - 1;
                if(realIndex >= 0 && realIndex < this.properties.Count)
                {
                    IEnumerator enumerator = this.properties.Values.GetEnumerator();

                    int i = 0;
                    while(enumerator.MoveNext())
                    {
                        if(i++ == realIndex)
                        {
                            return (EnvDTE.Property)enumerator.Current;
                        }
                    }
                }
            }

            return new OANullProperty(null);
        }
        /// <summary>
        /// Gets the immediate parent object of a Properties collection.
        /// </summary>
        public virtual object Parent
        {
            get { return null; }
        }
        #endregion

        #region methods
        /// <summary>
        /// Add properties to the collection of properties filtering only those properties which are com-visible and AutomationBrowsable
        /// </summary>
        /// <param name="targetType">The type of NodeProperties the we should filter on</param>
        protected void AddPropertiesFromType(Type targetType)
        {
            Debug.Assert(targetType != null);

            if (targetType == null)
            {
                throw new ArgumentNullException("targetType");
            }

            // If the type is not COM visible, we do not expose any of the properties
            if(!IsComVisible(targetType))
                return;

            // Add all properties being ComVisible and AutomationVisible 
            PropertyInfo[] propertyInfos = targetType.GetProperties();
            foreach(PropertyInfo propertyInfo in propertyInfos)
            {
                if(!IsInMap(propertyInfo) && IsComVisible(propertyInfo) && IsAutomationVisible(propertyInfo))
                {
                    AddProperty(propertyInfo);
                }
            }
        }
        #endregion

        #region virtual methods
        /// <summary>
        /// Creates a new OAProperty object and adds it to the current list of properties
        /// </summary>
        /// <param name="propertyInfo">The property to be associated with an OAProperty object</param>
        protected virtual void AddProperty(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                throw new ArgumentNullException("propertyInfo");
            }

            this.properties.Add(propertyInfo.Name, new OAProperty(this, propertyInfo));
        }
        #endregion

        #region helper methods

        private bool IsInMap(PropertyInfo propertyInfo)
        {
            return this.properties.ContainsKey(propertyInfo.Name);
        }

        private static bool IsAutomationVisible(PropertyInfo propertyInfo)
        {
            object[] customAttributesOnProperty = propertyInfo.GetCustomAttributes(typeof(AutomationBrowsableAttribute), true);

            foreach(AutomationBrowsableAttribute attr in customAttributesOnProperty)
            {
                if(!attr.Browsable)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsComVisible(Type targetType)
        {
            object[] customAttributesOnProperty = targetType.GetCustomAttributes(typeof(ComVisibleAttribute), true);

            foreach(ComVisibleAttribute attr in customAttributesOnProperty)
            {
                if(!attr.Value)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsComVisible(PropertyInfo propertyInfo)
        {
            object[] customAttributesOnProperty = propertyInfo.GetCustomAttributes(typeof(ComVisibleAttribute), true);

            foreach(ComVisibleAttribute attr in customAttributesOnProperty)
            {
                if(!attr.Value)
                {
                    return false;
                }
            }
            return true;
        }
        #endregion
    }
}
