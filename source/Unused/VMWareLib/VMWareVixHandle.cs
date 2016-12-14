using System;
using System.Collections.Generic;
using System.Text;
using Interop.VixCOM;

namespace Vestris.VMWareLib
{
    /// <summary>
    /// A wrapper for a VixCOM handle. 
    /// </summary>
    /// <typeparam name="T">Type of VixCOM handle.</typeparam>
    /// <remarks>
    /// Most VixCOM objects returned from VixCOM API functions implement IVixHandle.
    /// </remarks>
    public class VMWareVixHandle<T> : IDisposable
    {
        /// <summary>
        /// Raw VixCOM handle of implemented type.
        /// </summary>
        protected T _handle = default(T);

        /// <summary>
        /// Pointer to the IVixHandle interface.
        /// </summary>
        protected IVixHandle _vixhandle
        {
            get
            {
                return _handle as IVixHandle;
            }
        }

        /// <summary>
        /// Pointer to the IVixHandle2 interface.
        /// </summary>
        /// <remarks>
        /// This type was introduced in VixCOM 1.7.0 and will return null with older versions of VixCOM.
        /// </remarks>
        protected IVixHandle2 _vixhandle2
        {
            get
            {
                return _handle as IVixHandle2;
            }
        }

        /// <summary>
        /// A constructor for a null Vix handle.
        /// </summary>
        public VMWareVixHandle()
        {

        }

        /// <summary>
        /// A constructor for an existing Vix handle.
        /// </summary>
        /// <param name="handle">handle value</param>
        public VMWareVixHandle(T handle)
        {
            _handle = handle;
        }

        /// <summary>
        /// Get an array of properties.
        /// </summary>
        /// <param name="properties">properties to fetch</param>
        /// <returns>An array of property values.</returns>
        public object[] GetProperties(object[] properties)
        {
            object result = null;
            VMWareInterop.Check(_vixhandle.GetProperties(properties, ref result));
            return (object[])result;
        }

        /// <summary>
        /// Return the value of a single property.
        /// </summary>
        /// <param name="propertyId">property id</param>
        /// <typeparam name="R">property value type</typeparam>
        /// <returns>The value of a single property of type R.</returns>
        public R GetProperty<R>(int propertyId)
        {
            object[] properties = { propertyId };
            return (R)GetProperties(properties)[0];
        }

        /// <summary>
        /// Close the handle.
        /// </summary>
        public void Close()
        {
            if (_vixhandle2 != null)
            {
                _vixhandle2.Close();
                _handle = default(T);
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Close the handle with VixCOM 1.7.0 or newer.
        /// </summary>
        public virtual void Dispose()
        {
            Close();            
        }

        #endregion
    }
}
