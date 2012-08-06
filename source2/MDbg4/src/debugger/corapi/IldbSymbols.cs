using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Microsoft.Samples.Debugging.CorSymbolStore 
{
    /// <summary>
    /// A specialization of SymbolBinder to using ildbsymbols.dll (ILDB symbol format)
    /// Note that ildbsymbols.dll must be available in the current directory or on the path.
    /// </summary>
    internal class IldbSymbolBinder : SymbolBinder
    {
        public IldbSymbolBinder() : base(GetIldbBinderObject())
        {
        }

        /// <summary>
        /// Get the CorSymBinder object from ildbsymbols.dll
        /// </summary>
        /// <returns></returns>
        private static ISymUnmanagedBinder GetIldbBinderObject()
        {
            return (ISymUnmanagedBinder)IldbNativeMethods.IldbCoCreateInstance(CLSID_CorSymBinder, typeof(ISymUnmanagedBinder).GUID);
        }

    }

    /// <summary>
    /// A specialization of SymbolWriter to using ildbsymbols.dll (ILDB symbol format)
    /// </summary>
    internal class IldbSymbolWriter : SymbolWriter
    {
        public IldbSymbolWriter()
            : base(GetIldbWriterObject())
        {
        }

        /// <summary>
        /// Get the CorSymWriter object from ildbsymbols.dll
        /// </summary>
        /// <returns></returns>
        private static ISymUnmanagedWriter GetIldbWriterObject()
        {
            return (ISymUnmanagedWriter)IldbNativeMethods.IldbCoCreateInstance(CLSID_CorSymWriter, typeof(ISymUnmanagedWriter).GUID);
        }

    }

    internal class IldbNativeMethods
    {
        [ComImport]
        [Guid("00000001-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IClassFactory
        {
            [return:MarshalAs(UnmanagedType.Interface)]
            object CreateInstance(
                [MarshalAs(UnmanagedType.IUnknown)] object punkOuter,
                [In, MarshalAs(UnmanagedType.LPStruct)] Guid iid);

            void LockServer(bool fLock);
        }

        public static object IldbCoCreateInstance(System.Guid clsid, System.Guid iid)
        {
            IClassFactory fac = (IClassFactory)DllGetClassObject(clsid, typeof(IClassFactory).GUID);
            return fac.CreateInstance(null, iid);
        }

        /// <summary>
        /// PInvoke signature for the GetClassObject function
        /// </summary>
        /// <param name="clsid"></param>
        /// <param name="iid"></param>
        /// <returns></returns>
        [DllImport("ildbsymbols.dll", PreserveSig = false)]
        [return: MarshalAs(UnmanagedType.Interface)]
        public static extern object DllGetClassObject(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid clsid, 
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid iid);
    }
}
