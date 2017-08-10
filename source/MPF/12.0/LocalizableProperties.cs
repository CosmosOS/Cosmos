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
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Microsoft.VisualStudio.Project
{
    /// <summary>
    /// Enables a managed object to expose properties and attributes for COM objects.
    /// </summary>
    [ComVisible(true)]
    public class LocalizableProperties : ICustomTypeDescriptor
    {
        #region ICustomTypeDescriptor
        public virtual AttributeCollection GetAttributes()
        {
            AttributeCollection col = TypeDescriptor.GetAttributes(this, true);
            return col;
        }

        public virtual EventDescriptor GetDefaultEvent()
        {
            EventDescriptor ed = TypeDescriptor.GetDefaultEvent(this, true);
            return ed;
        }

        public virtual PropertyDescriptor GetDefaultProperty()
        {
            PropertyDescriptor pd = TypeDescriptor.GetDefaultProperty(this, true);
            return pd;
        }

        public virtual object GetEditor(Type editorBaseType)
        {
            object o = TypeDescriptor.GetEditor(this, editorBaseType, true);
            return o;
        }

        public virtual EventDescriptorCollection GetEvents()
        {
            EventDescriptorCollection edc = TypeDescriptor.GetEvents(this, true);
            return edc;
        }

        public virtual EventDescriptorCollection GetEvents(System.Attribute[] attributes)
        {
            EventDescriptorCollection edc = TypeDescriptor.GetEvents(this, attributes, true);
            return edc;
        }

        public virtual object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        public virtual PropertyDescriptorCollection GetProperties()
        {
            PropertyDescriptorCollection pcol = GetProperties(null);
            return pcol;
        }

        public virtual PropertyDescriptorCollection GetProperties(System.Attribute[] attributes)
        {
            ArrayList newList = new ArrayList();
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(this, attributes, true);

            for(int i = 0; i < props.Count; i++)
                newList.Add(CreateDesignPropertyDescriptor(props[i]));

            return new PropertyDescriptorCollection((PropertyDescriptor[])newList.ToArray(typeof(PropertyDescriptor))); ;
        }

        public virtual DesignPropertyDescriptor CreateDesignPropertyDescriptor(PropertyDescriptor propertyDescriptor)
        {
            return new DesignPropertyDescriptor(propertyDescriptor);
        }

        public virtual string GetComponentName()
        {
            string name = TypeDescriptor.GetComponentName(this, true);
            return name;
        }

        public virtual TypeConverter GetConverter()
        {
            TypeConverter tc = TypeDescriptor.GetConverter(this, true);
            return tc;
        }

        public virtual string GetClassName()
        {
            return this.GetType().FullName;
        }

        #endregion ICustomTypeDescriptor
    }
}
