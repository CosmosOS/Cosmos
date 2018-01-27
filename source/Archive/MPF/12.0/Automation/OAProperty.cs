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
using System.Runtime.InteropServices;

namespace Microsoft.VisualStudio.Project.Automation
{

    public class OAProperty : EnvDTE.Property
    {
        #region fields
        private OAProperties parent;
        private PropertyInfo pi;
        #endregion

        #region ctors

        public OAProperty(OAProperties parent, PropertyInfo pi)
        {
            this.parent = parent;
            this.pi = pi;
        }
        #endregion

        #region EnvDTE.Property
        /// <summary>
        /// Microsoft Internal Use Only.
        /// </summary>
        public object Application
        {
            get { return null; }
        }

        /// <summary>
        /// Gets the Collection containing the Property object supporting this property.
        /// </summary>
        public EnvDTE.Properties Collection
        {
            get
            {
                //todo: EnvDTE.Property.Collection
                return this.parent;
            }
        }

        /// <summary>
        /// Gets the top-level extensibility object.
        /// </summary>
        public EnvDTE.DTE DTE
        {
            get
            {
                return this.parent.DTE;
            }
        }

        /// <summary>
        /// Returns one element of a list. 
        /// </summary>
        /// <param name="index1">The index of the item to display.</param>
        /// <param name="index2">The index of the item to display. Reserved for future use.</param>
        /// <param name="index3">The index of the item to display. Reserved for future use.</param>
        /// <param name="index4">The index of the item to display. Reserved for future use.</param>
        /// <returns>The value of a property</returns>
        public object get_IndexedValue(object index1, object index2, object index3, object index4)
        {
            ParameterInfo[] par = pi.GetIndexParameters();
            int len = Math.Min(par.Length, 4);
            if(len == 0) return this.Value;
            object[] index = new object[len];
            Array.Copy(new object[4] { index1, index2, index3, index4 }, index, len);
            return this.pi.GetValue(this.parent.Target, index);
        }

        /// <summary>
        /// Setter function to set properties values. 
        /// </summary>
        /// <param name="value"></param>
        public void let_Value(object value)
        {
            this.Value = value;
        }

        /// <summary>
        /// Gets the name of the object.
        /// </summary>
        public string Name
        {
            get
            {
                return pi.Name;
            }
        }

        /// <summary>
        /// Gets the number of indices required to access the value.
        /// </summary>
        public short NumIndices
        {
            get { return (short)pi.GetIndexParameters().Length; }
        }

        /// <summary>
        /// Sets or gets the object supporting the Property object.
        /// </summary>
        public object Object
        {
            get
            {
                return this.parent.Target;
            }
            set
            {
            }
        }

        /// <summary>
        /// Microsoft Internal Use Only.
        /// </summary>
        public EnvDTE.Properties Parent
        {
            get { return this.parent; }
        }

        /// <summary>
        /// Sets the value of the property at the specified index.
        /// </summary>
        /// <param name="index1">The index of the item to set.</param>
        /// <param name="index2">Reserved for future use.</param>
        /// <param name="index3">Reserved for future use.</param>
        /// <param name="index4">Reserved for future use.</param>
        /// <param name="value">The value to set.</param>
        public void set_IndexedValue(object index1, object index2, object index3, object index4, object value)
        {
            ParameterInfo[] par = pi.GetIndexParameters();
            int len = Math.Min(par.Length, 4);
            if(len == 0)
            {
                this.Value = value;
            }
            else
            {
                object[] index = new object[len];
                Array.Copy(new object[4] { index1, index2, index3, index4 }, index, len);

                using(AutomationScope scope = new AutomationScope(this.parent.Target.Node.ProjectMgr.Site))
                {
                    this.pi.SetValue(this.parent.Target, value, index);
                }
            }

        }

        /// <summary>
        /// Gets or sets the value of the property returned by the Property object.
        /// </summary>
        public object Value
        {
            get { return pi.GetValue(this.parent.Target, null); }
            set
            {
                using(AutomationScope scope = new AutomationScope(this.parent.Target.Node.ProjectMgr.Site))
                {
                    this.pi.SetValue(this.parent.Target, value, null);
                }
            }
        }
        #endregion
    }
}
