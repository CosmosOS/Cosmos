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
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.Project
{
    internal enum tagDVASPECT
    {
        DVASPECT_CONTENT = 1,
        DVASPECT_THUMBNAIL = 2,
        DVASPECT_ICON = 4,
        DVASPECT_DOCPRINT = 8
    }

    internal enum tagTYMED
    {
        TYMED_HGLOBAL = 1,
        TYMED_FILE = 2,
        TYMED_ISTREAM = 4,
        TYMED_ISTORAGE = 8,
        TYMED_GDI = 16,
        TYMED_MFPICT = 32,
        TYMED_ENHMF = 64,
        TYMED_NULL = 0
    }

    internal sealed class DataCacheEntry : IDisposable
    {
        #region fields
        /// <summary>
        /// Defines an object that will be a mutex for this object for synchronizing thread calls.
        /// </summary>
        private static volatile object Mutex = new object();

        private FORMATETC format;

        private long data;

        private DATADIR dataDir;

        private bool isDisposed;
        #endregion

        #region properties
        internal FORMATETC Format
        {
            get
            {
                return this.format;
            }
        }

        internal long Data
        {
            get
            {
                return this.data;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal DATADIR DataDir
        {
            get
            {
                return this.dataDir;
            }
        }

        #endregion

        /// <summary>
        /// The IntPtr is data allocated that should be removed. It is allocated by the ProcessSelectionData method.
        /// </summary>
        internal DataCacheEntry(FORMATETC fmt, IntPtr data, DATADIR dir)
        {
            this.format = fmt;
            this.data = (long)data;
            this.dataDir = dir;
        }

        #region Dispose
        ~DataCacheEntry()
        {
            Dispose(false);
        }

        /// <summary>
        /// The IDispose interface Dispose method for disposing the object determinastically.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// The method that does the cleanup.
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose(bool disposing)
        {
            // Everybody can go here.
            if(!this.isDisposed)
            {
                // Synchronize calls to the Dispose simulteniously.
                lock(Mutex)
                {
                    if(disposing && this.data != 0)
                    {
                        Marshal.FreeHGlobal((IntPtr)this.data);
                        this.data = 0;
                    }

                    this.isDisposed = true;
                }
            }
        }
        #endregion
    }

    /// <summary>
    /// Unfortunately System.Windows.Forms.IDataObject and
    /// Microsoft.VisualStudio.OLE.Interop.IDataObject are different...
    /// </summary>
    internal sealed class DataObject : IDataObject
    {
        #region fields
        internal const int DATA_S_SAMEFORMATETC = 0x00040130;
        EventSinkCollection map;
        ArrayList entries;
        #endregion

        internal DataObject()
        {
            this.map = new EventSinkCollection();
            this.entries = new ArrayList();
        }

        internal void SetData(FORMATETC format, IntPtr data)
        {
            this.entries.Add(new DataCacheEntry(format, data, DATADIR.DATADIR_SET));
        }

        #region IDataObject methods
        int IDataObject.DAdvise(FORMATETC[] e, uint adv, IAdviseSink sink, out uint cookie)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            STATDATA sdata = new STATDATA();

            sdata.ADVF = adv;
            sdata.FORMATETC = e[0];
            sdata.pAdvSink = sink;
            cookie = this.map.Add(sdata);
            sdata.dwConnection = cookie;
            return 0;
        }

        void IDataObject.DUnadvise(uint cookie)
        {
            this.map.RemoveAt(cookie);
        }

        int IDataObject.EnumDAdvise(out IEnumSTATDATA e)
        {
            e = new EnumSTATDATA((IEnumerable)this.map);
            return 0; //??
        }

        int IDataObject.EnumFormatEtc(uint direction, out IEnumFORMATETC penum)
        {
            penum = new EnumFORMATETC((DATADIR)direction, (IEnumerable)this.entries);
            return 0;
        }

        int IDataObject.GetCanonicalFormatEtc(FORMATETC[] format, FORMATETC[] fmt)
        {
            throw new System.Runtime.InteropServices.COMException("", DATA_S_SAMEFORMATETC);
        }

        void IDataObject.GetData(FORMATETC[] fmt, STGMEDIUM[] m)
        {
            STGMEDIUM retMedium = new STGMEDIUM();

            if(fmt == null || fmt.Length < 1)
                return;

            foreach(DataCacheEntry e in this.entries)
            {
                if(e.Format.cfFormat == fmt[0].cfFormat /*|| fmt[0].cfFormat == InternalNativeMethods.CF_HDROP*/)
                {
                    retMedium.tymed = e.Format.tymed;

                    // Caller must delete the memory.
                    retMedium.unionmember = DragDropHelper.CopyHGlobal(new IntPtr(e.Data));
                    break;
                }
            }

            if(m != null && m.Length > 0)
                m[0] = retMedium;
        }

        void IDataObject.GetDataHere(FORMATETC[] fmt, STGMEDIUM[] m)
        {
        }

        int IDataObject.QueryGetData(FORMATETC[] fmt)
        {
            if(fmt == null || fmt.Length < 1)
                return VSConstants.S_FALSE;

            foreach(DataCacheEntry e in this.entries)
            {
                if(e.Format.cfFormat == fmt[0].cfFormat /*|| fmt[0].cfFormat == InternalNativeMethods.CF_HDROP*/)
                    return VSConstants.S_OK;
            }

            return VSConstants.S_FALSE;
        }

        void IDataObject.SetData(FORMATETC[] fmt, STGMEDIUM[] m, int fRelease)
        {
        }
        #endregion
    }

    [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
    internal static class DragDropHelper
    {
#pragma warning disable 414
        internal static readonly ushort CF_VSREFPROJECTITEMS;
        internal static readonly ushort CF_VSSTGPROJECTITEMS;
        internal static readonly ushort CF_VSPROJECTCLIPDESCRIPTOR;
#pragma warning restore 414

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static DragDropHelper()
        {
            CF_VSREFPROJECTITEMS = UnsafeNativeMethods.RegisterClipboardFormat("CF_VSREFPROJECTITEMS");
            CF_VSSTGPROJECTITEMS = UnsafeNativeMethods.RegisterClipboardFormat("CF_VSSTGPROJECTITEMS");
            CF_VSPROJECTCLIPDESCRIPTOR = UnsafeNativeMethods.RegisterClipboardFormat("CF_PROJECTCLIPBOARDDESCRIPTOR");
        }


        public static FORMATETC CreateFormatEtc(ushort iFormat)
        {
            FORMATETC fmt = new FORMATETC();
            fmt.cfFormat = iFormat;
            fmt.ptd = IntPtr.Zero;
            fmt.dwAspect = (uint)DVASPECT.DVASPECT_CONTENT;
            fmt.lindex = -1;
            fmt.tymed = (uint)TYMED.TYMED_HGLOBAL;
            return fmt;
        }

        public static int QueryGetData(Microsoft.VisualStudio.OLE.Interop.IDataObject pDataObject, ref FORMATETC fmtetc)
        {
            int returnValue = VSConstants.E_FAIL;
            FORMATETC[] af = new FORMATETC[1];
            af[0] = fmtetc;
            try
            {
                int result = ErrorHandler.ThrowOnFailure(pDataObject.QueryGetData(af));
                if(result == VSConstants.S_OK)
                {
                    fmtetc = af[0];
                    returnValue = VSConstants.S_OK;
                }
            }
            catch(COMException e)
            {
                Trace.WriteLine("COMException : " + e.Message);
                returnValue = e.ErrorCode;
            }

            return returnValue;
        }

        public static STGMEDIUM GetData(Microsoft.VisualStudio.OLE.Interop.IDataObject pDataObject, ref FORMATETC fmtetc)
        {
            FORMATETC[] af = new FORMATETC[1];
            af[0] = fmtetc;
            STGMEDIUM[] sm = new STGMEDIUM[1];
            pDataObject.GetData(af, sm);
            fmtetc = af[0];
            return sm[0];
        }

        /// <summary>
        /// Retrives data from a VS format.
        /// </summary>
        public static List<string> GetDroppedFiles(ushort format, Microsoft.VisualStudio.OLE.Interop.IDataObject dataObject, out DropDataType ddt)
        {
            ddt = DropDataType.None;
            List<string> droppedFiles = new List<string>();

            // try HDROP
            FORMATETC fmtetc = CreateFormatEtc(format);

            if(QueryGetData(dataObject, ref fmtetc) == VSConstants.S_OK)
            {
                STGMEDIUM stgmedium = DragDropHelper.GetData(dataObject, ref fmtetc);
                if(stgmedium.tymed == (uint)TYMED.TYMED_HGLOBAL)
                {
                    // We are releasing the cloned hglobal here.
                    IntPtr dropInfoHandle = stgmedium.unionmember;
                    if(dropInfoHandle != IntPtr.Zero)
                    {
                        ddt = DropDataType.Shell;
                        try
                        {
                            uint numFiles = UnsafeNativeMethods.DragQueryFile(dropInfoHandle, 0xFFFFFFFF, null, 0);

                            // We are a directory based project thus a projref string is placed on the clipboard.
                            // We assign the maximum length of a projref string.
                            // The format of a projref is : <Proj Guid>|<project rel path>|<file path>
                            uint lenght = (uint)Guid.Empty.ToString().Length + 2 * NativeMethods.MAX_PATH + 2;
                            char[] moniker = new char[lenght + 1];
                            for(uint fileIndex = 0; fileIndex < numFiles; fileIndex++)
                            {
                                uint queryFileLength = UnsafeNativeMethods.DragQueryFile(dropInfoHandle, fileIndex, moniker, lenght);
                                string filename = new String(moniker, 0, (int)queryFileLength);
                                droppedFiles.Add(filename);
                            }
                        }
                        finally
                        {
                            Marshal.FreeHGlobal(dropInfoHandle);
                        }
                    }
                }
            }

            return droppedFiles;
        }



        public static string GetSourceProjectPath(Microsoft.VisualStudio.OLE.Interop.IDataObject dataObject)
        {
            string projectPath = null;
            FORMATETC fmtetc = CreateFormatEtc(CF_VSPROJECTCLIPDESCRIPTOR);

            if(QueryGetData(dataObject, ref fmtetc) == VSConstants.S_OK)
            {
                STGMEDIUM stgmedium = DragDropHelper.GetData(dataObject, ref fmtetc);
                if(stgmedium.tymed == (uint)TYMED.TYMED_HGLOBAL)
                {
                    // We are releasing the cloned hglobal here.
                    IntPtr dropInfoHandle = stgmedium.unionmember;
                    if(dropInfoHandle != IntPtr.Zero)
                    {
                        try
                        {
                            string path = GetData(dropInfoHandle);

                            // Clone the path that we can release our memory.
                            if(!String.IsNullOrEmpty(path))
                            {
                                projectPath = String.Copy(path);
                            }
                        }
                        finally
                        {
                            Marshal.FreeHGlobal(dropInfoHandle);
                        }
                    }
                }
            }

            return projectPath;
        }

        /// <summary>
        /// Returns the data packed after the DROPFILES structure.
        /// </summary>
        /// <param name="dropHandle"></param>
        /// <returns></returns>
        internal static string GetData(IntPtr dropHandle)
        {
            IntPtr data = UnsafeNativeMethods.GlobalLock(dropHandle);
            try
            {
                _DROPFILES df = (_DROPFILES)Marshal.PtrToStructure(data, typeof(_DROPFILES));
                if(df.fWide != 0)
                {
                    IntPtr pdata = new IntPtr((long)data + df.pFiles);
                    return Marshal.PtrToStringUni(pdata);
                }
            }
            finally
            {
                if(data != null)
                {
                    UnsafeNativeMethods.GlobalUnLock(data);
                }
            }

            return null;
        }

        internal static IntPtr CopyHGlobal(IntPtr data)
        {
            IntPtr src = UnsafeNativeMethods.GlobalLock(data);
            int size = UnsafeNativeMethods.GlobalSize(data);
            IntPtr ptr = Marshal.AllocHGlobal(size);
            IntPtr buffer = UnsafeNativeMethods.GlobalLock(ptr);

            try
            {
                for(int i = 0; i < size; i++)
                {
                    byte val = Marshal.ReadByte(new IntPtr((long)src + i));

                    Marshal.WriteByte(new IntPtr((long)buffer + i), val);
                }
            }
            finally
            {
                if(buffer != IntPtr.Zero)
                {
                    UnsafeNativeMethods.GlobalUnLock(buffer);
                }

                if(src != IntPtr.Zero)
                {
                    UnsafeNativeMethods.GlobalUnLock(src);
                }
            }
            return ptr;
        }

        internal static void CopyStringToHGlobal(string s, IntPtr data, int bufferSize)
        {
            Int16 nullTerminator = 0;
            int dwSize = Marshal.SizeOf(nullTerminator);

            if((s.Length + 1) * Marshal.SizeOf(s[0]) > bufferSize)
                throw new System.IO.InternalBufferOverflowException();
            // IntPtr memory already locked...
            for(int i = 0, len = s.Length; i < len; i++)
            {
                Marshal.WriteInt16(data, i * dwSize, s[i]);
            }
            // NULL terminate it
            Marshal.WriteInt16(new IntPtr((long)data + (s.Length * dwSize)), nullTerminator);
        }

    } // end of dragdrophelper

    internal class EnumSTATDATA : IEnumSTATDATA
    {
        IEnumerable i;

        IEnumerator e;

        public EnumSTATDATA(IEnumerable i)
        {
            this.i = i;
            this.e = i.GetEnumerator();
        }

        void IEnumSTATDATA.Clone(out IEnumSTATDATA clone)
        {
            clone = new EnumSTATDATA(i);
        }

        int IEnumSTATDATA.Next(uint celt, STATDATA[] d, out uint fetched)
        {
            uint rc = 0;
            //uint size = (fetched != null) ? fetched[0] : 0;
            for(uint i = 0; i < celt; i++)
            {
                if(e.MoveNext())
                {
                    STATDATA sdata = (STATDATA)e.Current;

                    rc++;
                    if(d != null && d.Length > i)
                    {
                        d[i] = sdata;
                    }
                }
            }

            fetched = rc;
            return 0;
        }

        int IEnumSTATDATA.Reset()
        {
            e.Reset();
            return 0;
        }

        int IEnumSTATDATA.Skip(uint celt)
        {
            for(uint i = 0; i < celt; i++)
            {
                e.MoveNext();
            }

            return 0;
        }
    }

    internal class EnumFORMATETC : IEnumFORMATETC
    {
        IEnumerable cache; // of DataCacheEntrys.

        DATADIR dir;

        IEnumerator e;

        public EnumFORMATETC(DATADIR dir, IEnumerable cache)
        {
            this.cache = cache;
            this.dir = dir;
            e = cache.GetEnumerator();
        }

        void IEnumFORMATETC.Clone(out IEnumFORMATETC clone)
        {
            clone = new EnumFORMATETC(dir, cache);
        }

        int IEnumFORMATETC.Next(uint celt, FORMATETC[] d, uint[] fetched)
        {
            uint rc = 0;
            //uint size = (fetched != null) ? fetched[0] : 0;
            for(uint i = 0; i < celt; i++)
            {
                if(e.MoveNext())
                {
                    DataCacheEntry entry = (DataCacheEntry)e.Current;

                    rc++;
                    if(d != null && d.Length > i)
                    {
                        d[i] = entry.Format;
                    }
                }
                else
                {
                    return VSConstants.S_FALSE;
                }
            }

            if(fetched != null && fetched.Length > 0)
                fetched[0] = rc;
            return VSConstants.S_OK;
        }

        int IEnumFORMATETC.Reset()
        {
            e.Reset();
            return 0;
        }

        int IEnumFORMATETC.Skip(uint celt)
        {
            for(uint i = 0; i < celt; i++)
            {
                e.MoveNext();
            }

            return 0;
        }
    }
}
