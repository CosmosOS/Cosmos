//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
using System;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Diagnostics.SymbolStore;
using Microsoft.Samples.Debugging.CorSymbolStore;
using SymbolStore = Microsoft.Samples.Debugging.CorSymbolStore;

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.Samples.Debugging.CorDebug;
using Microsoft.Samples.Debugging.CorMetadata;




namespace Microsoft.Samples.Debugging.MdbgEngine
{

    /// <summary>
    /// MDbgModuleCollection class.  Allows for grouping of Modules.
    /// </summary>
    public sealed class MDbgModuleCollection : MarshalByRefObject, IEnumerable, IDisposable
    {
        IEnumerator IEnumerable.GetEnumerator()
        {
            MDbgModule[] ret = new MDbgModule[m_items.Count];
            m_items.Values.CopyTo(ret, 0);
            Array.Sort(ret);
            return ret.GetEnumerator();
        }

        /// <summary>
        /// Releases all resources used by the MDbgModuleCollection.
        /// </summary>
        /// <remarks>
        ///     This method effectively calls Dispose() on all modules in the collection.
        /// </remarks>
        public void Dispose()
        {
            foreach (MDbgModule module in m_items.Values)
            {
                module.Dispose();
            }
            Clear();
        }

        /// <summary>
        /// How many modules are in the collection.
        /// </summary>
        /// <value>Module Count.</value>
        public int Count
        {
            get
            {
                return m_items.Count;
            }
        }

        /// <summary>
        /// Looks up a CorFunction.
        /// </summary>
        /// <param name="managedFunction">Which CorFunction to lookup.</param>
        /// <returns>The coresponding MDbgFunction.</returns>
        public MDbgFunction LookupFunction(CorFunction managedFunction)
        {
            if (managedFunction != null)
            {
                return this.Lookup(managedFunction.Module).GetFunction(managedFunction);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Looks up a CorModule.
        /// </summary>
        /// <param name="managedModule">Which CorModule to lookup.</param>
        /// <returns>The coresponding MDbgModule.</returns>
        public MDbgModule Lookup(CorModule managedModule)
        {
            return (MDbgModule)m_items[managedModule];
        }

        /// <summary>
        /// Looks up a Module Name.
        /// </summary>
        /// <param name="moduleName">Which Module Name to lookup.</param>
        /// <returns>The coresponding MDbgModule with the given name.</returns>
        public MDbgModule Lookup(string moduleName)
        {
            MDbgModule matchedModule = null;
            foreach (MDbgModule module in m_items.Values)
            {
                if (module.MatchesModuleName(moduleName))
                {
                    if (matchedModule == null)
                        matchedModule = module;
                    else
                        throw new MDbgAmbiguousModuleNameException();
                }
            }
            return matchedModule;
        }

        internal MDbgModuleCollection(MDbgProcess process)
        {
            Debug.Assert(process != null);
            m_process = process;
        }

        internal void Clear()
        {
            m_items.Clear();
        }

        internal MDbgModule Register(CorModule managedModule)
        {
            MDbgModule mdbgModule;
            if (m_items.ContainsKey(managedModule))
            {
                mdbgModule = (MDbgModule)m_items[managedModule];
                return mdbgModule;
            }

            mdbgModule = new MDbgModule(m_process, managedModule, m_freeModuleNumber++);
            m_items.Add(managedModule, mdbgModule);
            return mdbgModule;
        }

        internal void Unregister(CorModule managedModule)
        {
            Debug.Assert(m_items.ContainsKey(managedModule));
            m_items.Remove(managedModule);
        }

        private Hashtable m_items = new Hashtable();
        private MDbgProcess m_process;
        private int m_freeModuleNumber = 0;
    }

    /// <summary>
    /// The MDbgModule class.
    /// </summary>
    public sealed class MDbgModule : MarshalByRefObject, IComparable, IDisposable
    {

        /// <summary>
        /// Gets the MDbgProcess that has loaded the module.
        /// </summary>
        /// <value>The MDbgProcess.</value>
        public MDbgProcess Process
        {
            get
            {
                return m_process;
            }
        }

        /// <summary>
        /// Releases all resources used by the MDbgModule.
        /// </summary>
        public void Dispose()
        {
            // Our funtion list may hold onto unmanaged SymbolMethod objects, so dispose that too.
            m_functions.Dispose();
            m_functions = null;

            // Release unmanaged resources
            if (m_symReader != null)
            {
                // Disposing the symbol reader will release the file lock on the PDB (even when other
                // reader COM objects have yet to be released by the garbage collector).
                ((IDisposable)m_symReader).Dispose();
                m_symReader = null;
            }
            m_module = null;
            m_importer = null;
        }

        /// <summary>
        /// Gets the CorModule encapsulated in the MDbgModule.
        /// </summary>
        /// <value>The CorModule.</value>
        public CorModule CorModule
        {
            get
            {
                return m_module;
            }
        }

        // lazy loading of Importer
        /// <summary>
        /// Gets the metadata importer for the Module.
        /// </summary>
        /// <value>The CorMetadataImport.</value>
        /// <remarks> The metadata provides rich compile-time information such
        /// as all the functions and types in the module. </remarks>
        public CorMetadataImport Importer
        {
            get
            {
                if (m_importer == null)
                {
                    m_importer = new CorMetadataImport(m_module);
                    Debug.Assert(m_importer != null);
                }
                return m_importer;
            }
        }

        /// <summary>
        /// Gets the number for the Module.
        /// </summary>
        /// <value>The number.</value>
        public int Number
        {
            get
            {
                return m_number;
            }
        }

        /// <summary>
        /// Gets the SymReader for the Module. 
        /// This will attempt to load symbols if not already loaded.
        /// </summary>
        /// <value>The SymReader.</value>
        public ISymbolReader SymReader
        {
            get
            {
                // Try and initialize the symbol reader if we haven't already.  
                if (!m_isSymReaderInitialized)
                {
                    // Mark the symbol reader as initialized so that we won't try to initialize again
                    // (even if we fail here) until an interesting event has occurred (eg. LoadClass / 
                    // UpdateModuleSymbolsif) or a user explicitly requests a symbol reload.
                    m_isSymReaderInitialized = true;

                    if (CorModule.IsInMemory && !CorModule.IsDynamic)
                    {
                        // Module is in-memory and non-dynamic (i.e. loaded from byte-array).
                        // We could use the below case to eagerly get the reader, but symbols won't 
                        // be available until the UpdateModuleSymbols callback is dispatched anyway, 
                        // so we'll just continue to rely on that to avoid the extra cost of 
                        // transferring the symbol data twice in that case.
                        // This is just a perf optimization and could be removed in the future if we 
                        // want to support scenarios / versions in which UpdateModuleSymbols doesn't 
                        // get dispatched
                        return null;
                    }

                    // Try and eagerly create the symbol reader (if in-memory symbols exist and the API
                    // to get them eaglery is supported by this CLR).
                    object symObj = this.CorModule.CreateReaderForInMemorySymbols();
                    if (symObj != null)
                    {
                        // The module supported ICDModule3 and has in-memory symbols available.
                        // Therefore we can update our symbol reader eagerly.
                        System.Diagnostics.SymbolStore.ISymbolReader symReader =
                            Microsoft.Samples.Debugging.CorSymbolStore.SymbolBinder.GetReaderFromCOM(symObj);

                        // Now we have a new symbol reader object, follow the same code path as used for
                        // UpdateModulesymbols
                        this.UpdateSymbols(symReader);
                    }
                    else if (CorModule.IsInMemory || CorModule.IsDynamic)
                    {
                        // Couldn't create a symbol reader eagerly and the module is in-memory, so we
                        // don't want to try and load symbols from disk.  Dynamic modules are always updating
                        // so even if we found a copy on disk it might be stale.  The "Name" for in-memory modules 
                        // won't necessarily correspond to any file name, so there's not much point in
                        // trying to look for symbols based on it (although conceptually it would be nice to
                        // support an option to find symbols on disk for in-memory modules).
                        // If this module has symbols, we'll get it from the UpdateModuleSymbols callback.
                    }
                    else // The module was loaded from disk...
                    {
                        // Shouldn't be any harm in Mdbg scenarios to looking everywhere possible for matching PDBs
                        // Note that symsrv.dll must be available on the path in order for symbol server access to work.
                        SymSearchPolicies symPolicy =
                            SymSearchPolicies.AllowOriginalPathAccess |
                            SymSearchPolicies.AllowReferencePathAccess |
                            SymSearchPolicies.AllowRegistryAccess |
                            SymSearchPolicies.AllowSymbolServerAccess;

                        string sympath = m_process.SymbolPath;
                        if (sympath == null)
                            sympath = m_process.m_engine.Options.SymbolPath;
                        string moduleName = m_module.Name;
                        Debug.Assert(moduleName.Length > 0);
                        // If we can load a file from the same path then we will assume that we got the same
                        // module as the one in memory (But we could still be cross machine loading a completely
                        // different file that happens to have the same path locally as the debuggee's
                        // module does remotely). If we can't find the file then we will assume that the module
                        // is layed out in memory just as LoadLibrary would place it and read the data from there.

                        try
                        {
                            if (File.Exists(moduleName))
                            {
                                m_symReader = SymBinder.GetReaderForFile(Importer.RawCOMObject, moduleName, sympath, symPolicy);
                            }
                            else
                            {
                                m_symReader = SymBinder.GetReaderForFile(Importer.RawCOMObject,
                                    moduleName, sympath, symPolicy, new ModuleRVAReader(CorModule));
                            }
                        }
                        catch (COMException e)
                        {
                            if ((e.ErrorCode == (int)HResult.E_PDB_CORRUPT) ||
                                (e.ErrorCode == (int)HResult.E_PARTIAL_COPY)) // Only part of a ReadProcessMemory or WriteProcessMemory request was completed.
                            {
                                // Ignore it.
                                // The first may happen for mismatched pdbs
                                // The second may happen for a dump
                            }
                            else
                                throw;
                        }
                    }
                }
                return m_symReader;
            }
        }

        /// <summary>
        /// Get the filename of the symbols for this module.
        /// This will attempt to load symbols if not already loaded.
        /// </summary>
        /// <return>
        /// If successful, returns the full path and filename for the symbols loaded for this 
        /// module. If no symbols are loaded or if symbols are loaded but it can't determine the filename, 
        /// returns null. </return>
        public string SymbolFilename
        {
            get
            {
                // Try to get exact PDB name.
                try
                {
                    SymbolStore.ISymbolReader2 s2 = (SymReader as SymbolStore.ISymbolReader2);
                    if (s2 != null)
                    {
                        string stPdbName = s2.GetSymbolStoreFileName();
                        return stPdbName;
                    }
                }
                catch
                {
                    // We've already set it to a default, so ignore the rest.   
                }

                return null;
            }
        }

        /// <summary>
        /// Checks if the module name string matches this module.
        /// This will attempt to load symbols if not already loaded.
        /// </summary>
        /// <param name="moduleName">Name of the module in one of supported formats.</param>
        /// <return>
        /// True if string matches this module name.
        /// </return>
        public bool MatchesModuleName(string moduleName)
        {

            // module names can be provide in following forms:
            // :1                       -- identifes module by logical number
            // mscorlib                 -- identifes module by base module name without path
            // mscorlib.dll             -- identifes module by name without path
            // c:\path\mscorlib.dll     -- identifes module by full load name
            //
            // futher modules identified by names can be more specified into which appdomian the
            // modules have been loaded. The syntax is as following:
            // moduleName#appdomainNumber.
            // E.g. mscorlib in first appdomain can be identified as:
            // mscrolib#0 or mscorlib.dll#0.
            // 

            // handle special :number syntax.
            if (moduleName.StartsWith(":") && moduleName.Length > 1)
            {
                UInt32 logicalNumber;
                if (!UInt32.TryParse(moduleName.Substring(1), out logicalNumber))
                    return false;
                return logicalNumber == this.Number;
            }

            // maybe the module has been specified with a full path
            bool fullNameProvided;
            if (moduleName.IndexOfAny(new char[]{ System.IO.Path.DirectorySeparatorChar,
                                               System.IO.Path.AltDirectorySeparatorChar }) == -1)
                fullNameProvided = false;
            else
                fullNameProvided = true;

            // 
            // There is a problem with the # suffix. The # is a legal character for a module
            // name. This means that a module can be named "a#0.dll".
            // What does the string a#0 should refer to? Module a in appdomain 0 or a module name
            // a#0 in any appdomain?
            //
            // There are 3 possible solutions:
            // a) we'll pick antoher char that is illegal in file name: e.g. ?<>|:/\
            //    This is best solution, however it is not portable because we don't about
            //    any universal char that is illegal on all platforms.
            //    Moreover this would also be a breaking change.
            // 
            // b) We'll try to treat "a#0" first as complete filename and then
            //    as name "a" in appdomain 0. This solution however brings problems
            //    in cases you have following modules loaded:
            //    a
            //    a#0
            //    How do you refer to the fist module in appdomain 0? a#0 will match also
            //    module a#0 loaded to any appdomain.
            //
            // c) We'll do nothing and assume that modules loaded don't end with #{number}. If they will
            //    the user cannot refer to them by name but only by syntax :1.
            //    There are other cases where this may be necessary (eg. a dynamic module with a space
            //    in it's name or in-memory modules with no name).
            //
            // Currently implementation goes with c).
            // 
            bool appDomainNumberProvided = false;
            UInt32 appDomainNumber = 0; // just prevent compiler warnings 

            int i = moduleName.LastIndexOf('#');
            if (i != -1)
            {
                // contains at least one #.
                appDomainNumberProvided = UInt32.TryParse(moduleName.Substring(i + 1), out appDomainNumber);
                if (appDomainNumberProvided)
                    moduleName = moduleName.Substring(0, i);
            }

            if (this.CorModule.IsInMemory && !this.CorModule.IsDynamic)
            {
                //  in-memory modules need to be referenced only by : syntax
                // because CorModule.Name for those modules is always: "<unknown>".
                // Ideally ICorDebugModule.Name should be returning the metadata name, and
                // we'd have another method for an optional file name.
                return false;
            }

            bool isMatch;
            if (fullNameProvided)
            {
                isMatch = String.Compare(this.CorModule.Name, moduleName,
                                          true, CultureInfo.InvariantCulture) == 0;
            }
            else
            {
                bool checkOnlyFullName;
                if (moduleName.EndsWith("."))
                {
                    checkOnlyFullName = true;
                    moduleName = moduleName.Substring(0, moduleName.Length - 1);
                }
                else
                    checkOnlyFullName = false;

                isMatch = String.Compare(System.IO.Path.GetFileName(this.CorModule.Name), moduleName,
                                          true, CultureInfo.InvariantCulture) == 0;

                // Dot at the end of module name explicitely says that we have specified an extension.
                // e.g. "a." will match only module named: "a." not "a..dll" or "a.dll".
                // On the other hand "a.dll" will match both modules "a.dll" and "a.dll.dll".

                if (!isMatch && !checkOnlyFullName)
                {
                    isMatch = String.Compare(System.IO.Path.GetFileNameWithoutExtension(this.CorModule.Name), moduleName,
                                              true, CultureInfo.InvariantCulture) == 0;
                }
            }

            if (isMatch && appDomainNumberProvided)
            {
                // we'll check if the appdomain matches as well.
                isMatch =
                    m_process.AppDomains.Lookup(this.CorModule.Assembly.AppDomain).Number == appDomainNumber;
            }

            return isMatch;
        }

        /// <summary>
        /// Reloads the symbols for the module.
        /// </summary>
        /// <param name="force">Forces reloading of symbols that have already been successfully loaded.</param>
        public void ReloadSymbols(bool force)
        {
            if (m_isSymReaderInitialized == false)
                return;

            if (m_isSymReaderInitialized && (m_symReader != null) && !force)
                return; // we don't want to reload symbols that has been sucessfully loaded

            if (EditsCounter > 0)
                throw new MDbgException("Cannot reload symbols for edited module " + CorModule.Name);

            // MdbgFunctions cache symbol information. This doesn't reset that cached info.
            m_isSymReaderInitialized = false;
            m_symReader = null;

            // clear the cache of functions. This is necessary since the cache contains also
            // information from symbol files. Reloding the files might cause the information
            // in the cache to become stale.
            m_functions.Clear();
        }

        /// <summary>
        /// Updates the symbols for the module given the raw PDB symbol stream.
        /// </summary>
        /// <param name="symbolStream">New IStream to use for symbol reading.
        /// If this is null, unloads the symbols for this module.</param>
        /// <returns></returns>
        public bool UpdateSymbols(IStream symbolStream)
        {
            if (symbolStream == null)
            {
                // Leave m_isSymReaderInitialized so that we don't automatically reload.
                // MDbgFunction objects cache symbol information. This won't reset that cache.
                m_isSymReaderInitialized = true;
                m_symReader = null;
                return true;
            }

            // Create a new symbol reader for this stream
            ISymbolReader newSymReader = SymBinder.GetReaderFromStream(Importer.RawCOMObject, symbolStream);
            if (newSymReader == null)
                return false;

            UpdateSymbols(newSymReader);
            return true;
        }

        /// <summary>
        /// Updates the symbols for a module given a new reader object
        /// </summary>
        /// <param name="newSymReader">The new symbol reader object</param>
        private void UpdateSymbols(ISymbolReader newSymReader)
        {
            // replace symbol reader with the updated one.
            m_symReader = newSymReader;
            m_isSymReaderInitialized = true;

            // Reset the cache of MDbgFunction objects since their symbol information is now
            // possibly out-of-date.
            // Note that in practice UpdateModuleSymbols is really only used to add symbols 
            // for newly emitted types.  This means we could probably get away with something
            // lighter weight, like looping through the MDbgFunction objects, finding ones
            // with missing symbol info, and resetting their m_isInitialized to false.  But this
            // would leave multiple symbol readers alive, taking up memory etc.  It's better
            // to throw away all references to the old symbol reader and recreate everything
            // for the new reader.
            m_functions.Clear();
        }

        /// <summary>
        /// Apply an edit.  (Edit and Continue feature)
        /// </summary>
        /// <param name="deltaMetadataFile">File containing the Metadata delta.</param>
        /// <param name="deltaILFile">File containing the IL delta.</param>
        /// <param name="deltaPdbFile">File containing the PDB delta.</param>
        /// <param name="editSourceFile">The edited source file. WARNING - this param may be removed in next release.</param>
        public void ApplyEdit(string deltaMetadataFile,
                               string deltaILFile,
                               string deltaPdbFile,
                               string editSourceFile
                               )
        {
            if (SymReader == null && deltaPdbFile != null)
                throw new MDbgException("Cannot update symbols on module without loaded symbols.");

            // read arguments from files
            byte[] deltaMeta;
            using (FileStream dmetaFile = File.OpenRead(deltaMetadataFile))
            {
                deltaMeta = new byte[dmetaFile.Length];
                dmetaFile.Read(deltaMeta, 0, deltaMeta.Length);
            }

            byte[] deltaIL;
            using (FileStream dilFile = File.OpenRead(deltaILFile))
            {
                deltaIL = new byte[dilFile.Length];
                dilFile.Read(deltaIL, 0, deltaIL.Length);
            }

            CorModule.ApplyChanges(deltaMeta, deltaIL);

            if (deltaPdbFile != null)
            {
                // apply dpdb to the symbol store.
                ISymbolReader sr = SymReader;
                (sr as SymbolStore.ISymbolReader2).UpdateSymbolStore(deltaPdbFile, null);
            }

            // save file name into of the edit
            if (m_editsSources == null)
            {
                Debug.Assert(EditsCounter == 0); // we don't have any edits
                m_editsSources = new ArrayList();
            }

            m_editsSources.Add(editSourceFile);
            m_editsCounter++;
        }

        /// <summary>
        /// Gets the MDbgFunction for a given CorFunction.
        /// </summary>
        /// <param name="managedFunction">The CorFunction to lookup.</param>
        /// <returns>The coresponding MDbgFunction.</returns>
        public MDbgFunction GetFunction(CorFunction managedFunction)
        {
            return m_functions.Get(managedFunction);
        }

        /// <summary>
        /// Gets the MDbgFunction for a given Function Token.
        /// </summary>
        /// <param name="functionToken">The Function Token to lookup.</param>
        /// <returns>The coresponding MDbgFunction.</returns>
        public MDbgFunction GetFunction(int functionToken)
        {
            CorFunction f = m_module.GetFunctionFromToken(functionToken);
            Debug.Assert(f != null);
            return GetFunction(f);
        }

        /// <summary>
        /// Gets the number of edits performed on a module.
        /// </summary>
        /// <value>The number of edits.</value>
        public int EditsCounter
        {
            get
            {
                return m_editsCounter;
            }
        }

        /// <summary>
        /// Gets the name of edit file that was to used to do specified edits.
        /// </summary>
        /// <param name="editNumber">Which edit to lookup.</param>
        /// <returns>The source file used to perform the given edit number.</returns>
        public string GetEditsSourceFile(int editNumber)
        {
            Debug.Assert(editNumber <= m_editsCounter);
            Debug.Assert(editNumber > 0);
            if (!(editNumber <= m_editsCounter ||
                  editNumber > 0))
                throw new ArgumentException();

            if (m_editsSources == null)
                return null;
            return (string)m_editsSources[editNumber - 1];
        }

        internal MDbgModule(MDbgProcess process, CorModule managedModule, int number)
        {
            Debug.Assert(process != null && managedModule != null);
            m_process = process;
            m_module = managedModule;
            m_functions = new MDbgFunctionMgr(this);
            m_number = number;
        }

        int IComparable.CompareTo(object obj)
        {
            return this.Number - (obj as MDbgModule).Number;
        }

        // lazy initialization of SymBiner (on Class level)
        private static ISymbolBinder2 SymBinder
        {
            get
            {
                if (g_symBinder == null)
                {
                    g_symBinder = new SymbolStore.SymbolBinder();
                    Debug.Assert(g_symBinder != null);
                }
                return g_symBinder;
            }
        }

        private CorModule m_module;
        private MDbgProcess m_process;
        private int m_number;

        private CorMetadataImport m_importer;
        private ISymbolReader m_symReader;
        private bool m_isSymReaderInitialized = false;

        private int m_editsCounter = 0;

        private ArrayList m_editsSources;

        private MDbgFunctionMgr m_functions;

        private static ISymbolBinder2 g_symBinder;
    }

}
