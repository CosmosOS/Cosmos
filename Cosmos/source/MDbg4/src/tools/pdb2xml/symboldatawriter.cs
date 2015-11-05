using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

// For symbol store
using System.Diagnostics.SymbolStore;
using Microsoft.Samples.Debugging.CorSymbolStore;
using System.Runtime.InteropServices;
using System.IO;

namespace Pdb2Xml
{
    /// <summary>
    /// Class to write out a PDB corresponding to a SymbolData object
    /// </summary>
    public class SymbolDataWriter
    {
        ISymbolWriter2 m_writer;
        SymbolFormat m_symFormat;

        // A map of fileRef id to document writers
        Dictionary<int, ISymbolDocumentWriter> m_docWriters;

        string m_outputAssembly;

        public SymbolDataWriter(string asmPath, SymbolFormat symFormat)
        {
            m_outputAssembly = asmPath;
            m_symFormat = symFormat;
        }

        public void WritePdb(SymbolData symData)
        {
            m_docWriters = new Dictionary<int, ISymbolDocumentWriter>();

            ImageDebugDirectory debugDirectory;
            byte[] debugInfo = null;
            // Rather than use the emitter here, we are just careful enough to emit pdb metadata that
            // matches what is already in the assembly image.
            object emitter = null;

            // We must be careful to close the writer before updating the debug headers.  The writer has an
            // open file handle on the assembly we want to update.
            m_writer = SymbolAccess.GetWriterForFile(m_symFormat, m_outputAssembly, ref emitter);

            // We don't actually need the emitter in managed code at all, so release the CLR reference on it
            Marshal.FinalReleaseComObject(emitter);

            try
            {
                WriteEntryPoint(symData.entryPointToken);
                WriteFiles(symData.sourceFiles);
                WriteMethods(symData.methods);
                debugInfo = m_writer.GetDebugInfo(out debugDirectory);
            }
            finally
            {
                m_writer.Close();
                ((IDisposable)m_writer).Dispose();
                m_writer = null;
                m_docWriters.Clear();
            }

            UpdatePEDebugHeaders(debugInfo);
        }

        private void WriteEntryPoint(string entryPointToken)
        {
            if (entryPointToken != null)
            {
                m_writer.SetUserEntryPoint(Util.AsSymToken(entryPointToken));
            }
        }

        private void WriteFiles(List<Document> documents)
        {
            foreach (Document doc in documents)
            {
                ISymbolDocumentWriter docwriter = m_writer.DefineDocument(
                    doc.url,
                    doc.language,
                    doc.languageVendor,
                    doc.documentType);
                m_docWriters.Add(doc.id, docwriter);
            }
        }

        private void WriteMethods(List<Method> methods)
        {
            foreach (Method m in methods)
            {
                WriteMethod(m);
            }
        }

        private void WriteMethod(Method m)
        {
            SymbolToken methodToken = Util.AsSymToken(m.token);
            m_writer.OpenMethod(methodToken);

            SymbolToken? localSigToken = null;
            if (m.localSigMetadataToken != null)
            {
                localSigToken = new SymbolToken(Util.ToInt32(m.localSigMetadataToken, 16));
            }
            WriteSequencePoints(m.sequencePoints);
            WriteScopesAndLocals(m.rootScope, localSigToken, m);
            WriteSymAttributes(m.symAttributes, methodToken);

            if (m.csharpCDI != null)
                WriteCSharpCDI(m.csharpCDI, methodToken);

            m_writer.CloseMethod();
        }

        private void WriteScopesAndLocals(Scope scope, SymbolToken? localSigToken, Method method)
        {
            // If this scope is marked implicit then we won't explicitly write it out
            // This is usually used for the root scope
            if (!scope.isImplicit)
                m_writer.OpenScope(scope.startOffset);

            foreach (Variable localVar in scope.locals)
            {
                // Note that local variables can have a start/end offset within their enclosing scope.
                // Infortunately ISymUnmanagedVariable.GetStartOffset() returns E_NOTIMPL, so we
                // can't tell whether this was used or not.  Passing 0 says to use the start/end for
                // the entire scope (which is probably usually the case anyway).

                // Diasymreader.dll always stores local signatures by token, and if we use the V1
                // API will attempt to emit a new token to the metadata (which we don't support here).
                // IldbSymbols.dll on the other hand always stores signatures inline, and doesn't support
                // the V2 API.  RefEmit uses the V1 API, so it's fine to use it here for ILDB too.
                if (m_symFormat == SymbolFormat.PDB)
                {
                    if (localSigToken.HasValue)
                    {
                        m_writer.DefineLocalVariable(
                            localVar.name,
                            localVar.attributes,
                            localSigToken.Value,
                            (int)SymAddressKind.ILOffset, localVar.ilIndex, 0, 0,
                            0, 0); // start/end offsets - get from current scope
                    }
                    else
                    {
                        // We want to handle this case as gracefully since there is a bug in the C# compiler that
                        // can prevent us from getting the local var sig token.
                        // Note that we don't want to just use the V1 API with PDBs because we're not saving the 
                        // metadata, and so the call to ISymUnmanagedVariable::GetSignature will just fail because
                        // the token will be invalid.
                        if (!method.hasInvalidMethodBody)
                            throw new FormatException("Missing localVarsigToken in a method without hasInvalidMethodBody set");
                    }
                }
                else
                {
                    byte[] sig = Util.ToByteArray(localVar.signature);
                    m_writer.DefineLocalVariable(
                        localVar.name, 
                        (FieldAttributes)localVar.attributes, 
                        sig, 
                        SymAddressKind.ILOffset, localVar.ilIndex, 0, 0, 
                        0, 0);   // start/end offsets - get from current scope
                }
            }

            foreach (Constant constant in scope.constants)
            {
                m_writer.DefineConstant(constant.name, constant.value, Util.ToByteArray(constant.signature));
            }

            // Now recursively write out any child scopes
            foreach (Scope childScope in scope.scopes)
            {
                WriteScopesAndLocals(childScope, localSigToken, method);
            }

            foreach (Namespace ns in scope.usedNamespaces)
            {
                m_writer.UsingNamespace(ns.name);
            }

            if (!scope.isImplicit)
                m_writer.CloseScope(scope.endOffset);
        }

        private void WriteSequencePoints(List<SequencePoint> spData)
        {
            // Construct a mapping from file ID to sequence point data
            Dictionary<int, SequencePointCollection> dataByFile = new Dictionary<int, SequencePointCollection>();
            foreach (int fileRef in m_docWriters.Keys)
            {
                dataByFile.Add(fileRef, new SequencePointCollection());
            }

            // Read all the sequence points and group them by fileID
            // Note that we don't want to assume that all entries for one file will come together, so we can't just
            // write them out as we go.
            foreach (SequencePoint sp in spData)
            {
                dataByFile[sp.sourceId].Add(sp.ilOffset, sp.startRow, sp.startColumn, sp.endRow, sp.endColumn);
            }

            // Now write them all out one document at a time
            foreach (KeyValuePair<int, ISymbolDocumentWriter> docEntry in m_docWriters)
            {
                SequencePointCollection seq = dataByFile[docEntry.Key];
                if (seq.ILOffsets.Count > 0)
                {
                    m_writer.DefineSequencePoints(
                        docEntry.Value,
                        seq.ILOffsets.ToArray(),
                        seq.StartRows.ToArray(),
                        seq.StartColumns.ToArray(),
                        seq.EndRows.ToArray(),
                        seq.EndColumns.ToArray());
                }
            }
        }


        /// <summary>
        /// Write out the symbol attributes for this method.
        /// Note that symbol attributes can only be associated to a method, and only when 
        /// that method is open
        /// </summary>
        /// <param name="attrs"></param>
        /// <param name="methodToken"></param>
        private void WriteSymAttributes(List<SymAttribute> attrs, SymbolToken methodToken)
        {
            foreach (SymAttribute attr in attrs)
            {
                m_writer.SetSymAttribute(methodToken, attr.name, Util.ToByteArray(attr.value));
            }
        }

        private void WriteCSharpCDI(CSharpCDI cdi, SymbolToken methodToken)
        {
            byte[] val = CDIWriter.WriteCDI(cdi);
            m_writer.SetSymAttribute(methodToken, "MD2", val);
        }

        /* 
         * This function (UpdatePEDebugHeaders) represents a bit of a hack.
         * 
         * When a debugger attempts to find debug information (pdb) 
         * for an image file (dll or exe), it uses a data-blob in the
         * image file to decide if the debug info "matches" this version.
         * 
         * Modifying the pdb without updating the image file would cause
         * a debugger to not load this new pdb due to mismatched information.
         * 
         * In most situations when somebody emits debug info, they are also 
         * emitting metadata and an assembly, so they can just include the
         * results of ISymUnmanagedWriter.GetDebugInfo in that new image file.
         * 
         * The intent of this sample is to demonstrate managed symbol reading
         * and writing without too much extra stuff.  For this reason, I didn't
         * include a full implementation of PEFile manipulation because it's
         * not really the point of this sample.  The code that does the PE File
         * manipulation was thrown together to get a specific job done and not
         * to be a good demonstration of how a compiler should deal with this.
         * Managed data structures to mirror the native format would be a better
         * approach if you needed more functionality out of the PEFile class.
         */
        private void UpdatePEDebugHeaders(byte[] debugInfo)
        {
            if (File.Exists(m_outputAssembly))
            {
                PEFile file = new PEFile(m_outputAssembly);
                file.UpdateHeader(debugInfo);
            }
            else
            {
                Console.WriteLine("Warning: Assembly couldn't be found to update.");
                Console.WriteLine("New pdb won't \"match\" any assembly and will thus not be useful to a debugger.");
            }
        }
        
        // For convience with the underlying diasymreader APIs, sequence points are represented 
        // as 6 integer lists (as opposed to the more natural choice of a list of structures 
        // containing 6 integers).  All lists must be the same length.
        class SequencePointCollection
        {
            public void Add(int ilOffset, int startRow, int startColumn, int endRow, int endColumn)
            {
                ILOffsets.Add(ilOffset);
                StartRows.Add(startRow);
                StartColumns.Add(startColumn);
                EndRows.Add(endRow);
                EndColumns.Add(endColumn);
            }

            public List<int> ILOffsets = new List<int>();
            public List<int> StartRows = new List<int>();
            public List<int> StartColumns = new List<int>();
            public List<int> EndRows = new List<int>();
            public List<int> EndColumns = new List<int>();
        }

        // Note that this is designed to write verison 4 of all records, but
        // we'll write version numbers other than 4 if requested (for testing purposes).
        class CDIWriter : BinaryWriter
        {
            public static byte[] WriteCDI(CSharpCDI cdi)
            {
                CDIWriter writer = new CDIWriter(cdi);

                writer.Write((byte)cdi.version);
                writer.Write((byte)cdi.entries.Length);
                writer.WritePadding(4);

                foreach(CDIItem i in cdi.entries)
                    writer.WriteItem(i);

                return writer.GetBytes();
            }

            private CDIWriter(CSharpCDI cdi)
                : base(new MemoryStream(), Encoding.Unicode)
            {
            }

            private byte[] GetBytes()
            {
                Flush();
                MemoryStream ms = (MemoryStream)OutStream;
                return ms.ToArray();
            }

            private void WritePadding(int alignment)
            {
                while (BaseStream.Position % alignment != 0)
                    Write((byte)0);
            }

            private void WriteItem(CDIItem item)
            {
                long startPos = BaseStream.Position;
                uint length = 2 * sizeof(int);
                Write((byte)item.version);

                if (item is CDIUsing)
                {
                    CDIUsing cdiUsing = (CDIUsing)item;
                    Write((byte)CDIKind.UsingInfo);
                    WritePadding(4);
                    length = AlignUp(8 + ((uint)cdiUsing.countOfUsing.Length + 1) * sizeof(ushort), 4);
                    Write(length);
                    Write(checked((ushort)cdiUsing.countOfUsing.Length));
                    foreach (ushort u in cdiUsing.countOfUsing)
                        Write(u);
                    WritePadding(4);
                } 
                else if(item is CDIForward)
                {
                    CDIForward cdiFwd = (CDIForward)item;
                    Write((byte)CDIKind.ForwardInfo);
                    WritePadding(4);
                    length = 12;
                    Write(length);
                    Write(Util.ToInt32(cdiFwd.tokenToForwardTo, 16));
                } 
                else if(item is CDIForwardModule)
                {
                    CDIForwardModule cdiFwdMod = (CDIForwardModule)item;
                    Write((byte)CDIKind.ForwardToModuleInfo);
                    WritePadding(4);
                    length = 12;
                    Write(length);
                    Write(Util.ToInt32(cdiFwdMod.tokenOfModuleInfo, 16));
                } 
                else if(item is CDIIteratorLocals)
                {
                    CDIIteratorLocals cdiIterLocals = (CDIIteratorLocals)item;
                    Write((byte)CDIKind.IteratorLocals);
                    WritePadding(4);
                    length = (uint)(12 + cdiIterLocals.buckets.Length * 2 * sizeof(uint));
                    Write(length);
                    Write(cdiIterLocals.buckets.Length);
                    foreach (CDIIteratorLocalBucket b in cdiIterLocals.buckets)
                    {
                        Write(b.ilOffsetStart);
                        Write(b.ilOffsetEnd);
                    }
                }
                else if(item is CDIForwardIterator)
                {
                    CDIForwardIterator cdiIter = (CDIForwardIterator)item;
                    Write((byte)CDIKind.ForwardIterator);
                    WritePadding(4);
                    length = AlignUp(8 + 2 * ((uint)cdiIter.iteratorClassName.Length + 1), 4);
                    Write(length);
                    WriteString(cdiIter.iteratorClassName);
                    WritePadding(4);
                }
                else if (item is CDIUnknown)
                {
                    CDIUnknown cdiUnknown = (CDIUnknown)item;
                    Write((byte)cdiUnknown.kind);
                    WritePadding(4);
                    byte[] bytes = Util.ToByteArray(cdiUnknown.bytes);
                    length = 8 + (uint)bytes.Length;
                    Write(length);
                    Write(bytes);
                }
                else
                {
                    Debug.Assert(false, "Unexpected CDIItem type");
                }

                Debug.Assert(BaseStream.Position == startPos + length, "Didn't write the length we promised");
            }

            private uint AlignUp(uint len, uint alignment)
            {
                uint newLen = len;
                if (newLen % alignment != 0)
                    newLen += alignment - (len % alignment);
                return newLen;
            }

            private void WriteString(string str)
            {
                Write(str.ToCharArray());
                Write((char)0);
            }

            private enum CDIKind : byte
            {
                UsingInfo = 0,
                ForwardInfo = 1,
                ForwardToModuleInfo = 2,
                IteratorLocals = 3,
                ForwardIterator = 4,
            }
        }
    }
}
