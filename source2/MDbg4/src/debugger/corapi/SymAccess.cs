//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------

/*
 * This file contains a class that distributes Symbol Reader and Writer interfaces.
 */

namespace Microsoft.Samples.Debugging.CorSymbolStore
{
    using System.Diagnostics;
    using System.Diagnostics.SymbolStore;
    using System;
    using System.Runtime.InteropServices;

    // Definition of the different underlying symbol formats that are supported
    public enum SymbolFormat
    {
        PDB,        // normal PDB format - requires diasymreader.dll
        ILDB        // ILDB format - requires ildbsymbols.dll
    }

    /*
     * This class includes methods for getting top-level access to symbol objects for reading and writing to PDB files
     */
    public static class SymbolAccess
    {
        // Guids for imported metadata interfaces.
        private static Guid dispenserClassID = new Guid(0xe5cb7a31, 0x7512, 0x11d2, 0x89, 0xce, 0x00, 0x80, 0xc7, 0x92, 0xe5, 0xd8);    // CLSID_CorMetaDataDispenser
        private static Guid dispenserIID = new Guid(0x809c652e, 0x7396, 0x11d2, 0x97, 0x71, 0x00, 0xa0, 0xc9, 0xb4, 0xd5, 0x0c);        // IID_IMetaDataDispenser
        private static Guid importerIID = new Guid(0x7dac8207, 0xd3ae, 0x4c75, 0x9b, 0x67, 0x92, 0x80, 0x1a, 0x49, 0x7d, 0x44);         // IID_IMetaDataImport
        private static Guid emitterIID = new Guid(0xba3fee4c, 0xecb9, 0x4e41, 0x83, 0xb7, 0x18, 0x3f, 0xa4, 0x1c, 0xd8, 0x59);     // IID_IMetaDataEmit

        const int OPEN_READ = 0;
        const int OPEN_WRITE = 1;

        internal static class NativeMethods
        {
            [DllImport("ole32.dll")]
            internal static extern int CoCreateInstance([In] ref Guid rclsid,
                                                       [In, MarshalAs(UnmanagedType.IUnknown)] Object pUnkOuter,
                                                       [In] uint dwClsContext,
                                                       [In] ref Guid riid,
                                                       [Out, MarshalAs(UnmanagedType.Interface)] out Object ppv);
        }

        // This function will either use a given IMetadataEmitter for the ISymbolWriter2
        // or it will make a new one based on the path that you provide.
        // In either case, you need to make sure that the metadata in the image file matches
        // the metadata in the symbols.
        public static ISymbolWriter2 GetWriterForFile(string pathModule, ref object emitter)
        {
            return GetWriterForFile(SymbolFormat.PDB, pathModule, ref emitter);
        }

        // Writer access with custom-specified format
        public static ISymbolWriter2 GetWriterForFile(SymbolFormat format, string pathModule, ref object emitter)
        {
            SymbolWriter writer;
            if (format == SymbolFormat.PDB)
                writer = new SymbolWriter();
            else if (format == SymbolFormat.ILDB)
                writer = new IldbSymbolWriter();
            else
                throw new ArgumentException("Invalid format", "format");

            return InitializeWriterForFile(writer, pathModule, ref emitter);
        }

        // Initialization routine that allows a customized writer object to be supplied
        private static ISymbolWriter2 InitializeWriterForFile(SymbolWriter writer, string pathModule, ref object emitter)
        {
            // If no emitter is provided, make one and return it.
            if (emitter == null)
            {
                // First get a dispenser
                object objDispenser;
                NativeMethods.CoCreateInstance(ref dispenserClassID, null, 1, ref dispenserIID, out objDispenser);
                Debug.Assert(objDispenser != null, "Dispenser is null.");

                // Now get an emitter
                IMetaDataDispenserPrivate dispenser = (IMetaDataDispenserPrivate)objDispenser;
                dispenser.OpenScope(pathModule, OPEN_WRITE, ref emitterIID, out emitter);
            }
            Debug.Assert(emitter != null, "Emitter is null.");

            // An emitter was provided, just use that one.
            writer.Initialize(emitter, pathModule, false);
            return writer;
        }

        /*
         * If you want a SymbolReader for a given exe, just use this function.
         */
        public static ISymbolReader GetReaderForFile(string pathModule)
        {
            return GetReaderForFile(pathModule, null);
        }
        /*
         * If you know the name of the exe and a searchPath where the file may exist, use this one.
         */
        public static ISymbolReader GetReaderForFile(string pathModule, string searchPath)
        {
            return GetReaderForFile(SymbolFormat.PDB, pathModule, searchPath);
        }
        /*
         * Implementation which allows customization of the SymbolBinder to use.
         * searchPath is a simicolon-delimited list of paths on which to search for pathModule.
         * If searchPath is null, pathModule must be a full path to the assembly.
         */
         public static ISymbolReader GetReaderForFile(SymbolFormat symFormat, string pathModule, string searchPath)
        {
            // First create the Metadata dispenser.
            // Create the appropriate symbol binder
            SymbolBinder binder;
            if (symFormat == SymbolFormat.PDB)
                binder = new SymbolBinder();
            else if (symFormat == SymbolFormat.ILDB)
                binder = new IldbSymbolBinder();
            else
                throw new ArgumentException("Invalid format", "symFormat");

            // Create the Metadata dispenser.
            object objDispenser;
            NativeMethods.CoCreateInstance(ref dispenserClassID, null, 1, ref dispenserIID, out objDispenser);

            // Now open an Importer on the given filename. We'll end up passing this importer straight
            // through to the Binder.
            object objImporter;
            IMetaDataDispenserPrivate dispenser = (IMetaDataDispenserPrivate)objDispenser;
            dispenser.OpenScope(pathModule, OPEN_READ, ref importerIID, out objImporter);

            IntPtr importerPtr = IntPtr.Zero;
            ISymbolReader reader;
            try
            {
                // This will manually AddRef the underlying object, so we need to be very careful to Release it.
                importerPtr = Marshal.GetComInterfaceForObject(objImporter, typeof(IMetadataImportPrivateComVisible));

                reader = binder.GetReader(importerPtr, pathModule, searchPath);
            }
            finally
            {
                if (importerPtr != IntPtr.Zero)
                {
                    Marshal.Release(importerPtr);
                }
            }
            return reader;
        }
    }

    // We can use reflection-only load context to use reflection to query for metadata information rather
    // than painfully import the com-classic metadata interfaces.
    [Guid("809c652e-7396-11d2-9771-00a0c9b4d50c"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComVisible(true)]
    internal interface IMetaDataDispenserPrivate
    {
        // We need to be able to call OpenScope, which is the 2nd vtable slot.
        // Thus we need this one placeholder here to occupy the first slot..
        void DefineScope_Placeholder();

        //STDMETHOD(OpenScope)(                 // Return code.
        //  LPCWSTR     szScope,                // [in] The scope to open.
        //  DWORD       dwOpenFlags,            // [in] Open mode flags.
        //  REFIID      riid,                   // [in] The interface desired.
        //  IUnknown    **ppIUnk) PURE;         // [out] Return interface on success.
        void OpenScope([In, MarshalAs(UnmanagedType.LPWStr)] String szScope, [In] Int32 dwOpenFlags, [In] ref Guid riid, [Out, MarshalAs(UnmanagedType.IUnknown)] out Object punk);

        // There are more methods in this interface, but we don't need them.
    }
    // Since we're just blindly passing this interface through managed code to the Symbinder, we don't care about actually
    // importing the specific methods.
    // This needs to be public so that we can call Marshal.GetComInterfaceForObject() on it to get the
    // underlying metadata pointer.
    // That doesn't mean that you should actually use it though because the interface is basically empty.
    [Guid("7DAC8207-D3AE-4c75-9B67-92801A497D44"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComVisible(true)]
    public interface IMetadataImportPrivateComVisible
    {
        // Just need a single placeholder method so that it doesn't complain about an empty interface.
        void Placeholder();
    }
}
