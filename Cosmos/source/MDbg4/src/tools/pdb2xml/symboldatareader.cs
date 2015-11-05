using System;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;

// For symbol store
using System.Diagnostics.SymbolStore;
using Microsoft.Samples.Debugging.CorSymbolStore;
using System.Text;
using System.IO;

namespace Pdb2Xml
{
    /// <summary>
    /// Class to read a symbol (PDB) file into a SymbolData object
    /// </summary>
    public class SymbolDataReader
    {
        /// <summary>
        /// Initialize the Pdb to Xml converter. Actual conversion happens in ReadPdbAndWriteToXml.
        /// Passing a filename also makes it easy for us to use reflection to get some information 
        /// (such as enumeration)
        /// </summary>
        public SymbolDataReader(string assemblyPath, SymbolFormat symFormat, bool expandAttributes)
        {
            m_assemblyPath = assemblyPath;
            m_symFormat = symFormat;
            m_expandAttributes = expandAttributes;
        }

        string m_assemblyPath;

        SymbolFormat m_symFormat;

        // Expand known attributes when possible
        bool m_expandAttributes;

        // Keep assembly so we can query metadata on it.
        System.Reflection.Assembly m_assembly;

        // Maps files to ids. 
        Dictionary<string, int> m_fileMapping;

        /// <summary>
        /// Load the PDB given the parameters at the ctor and spew it out to the XmlWriter specified
        /// at the ctor.
        /// </summary>
        public SymbolData ReadSymbols()
        {
            // Actually load the files
            ISymbolReader reader = SymbolAccess.GetReaderForFile(m_symFormat, m_assemblyPath, null);
            if (reader == null)
            {
                Console.WriteLine("Error: No matching PDB could be found for the specified assembly.");
                return null;
            }

            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += new ResolveEventHandler(CurrentDomain_ReflectionOnlyAssemblyResolve);
            m_assembly = System.Reflection.Assembly.ReflectionOnlyLoadFrom(m_assemblyPath);

            m_fileMapping = new Dictionary<string, int>();

            SymbolData symbolData = new SymbolData();

            // Record what input file these symbols are for.
            symbolData.assembly = m_assemblyPath;

            symbolData.entryPointToken = ReadEntryPoint(reader);
            symbolData.sourceFiles = ReadDocList(reader);
            symbolData.methods = ReadAllMethods(reader);

            return symbolData;
        }

        // In order to call GetTypes(), we need to manually resolve any assembly references.
        // For example, if a type derives from a type in another module, we need to resolve that module.
        Assembly CurrentDomain_ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {
            // args.Name is the assembly name, not the filename.
            // Naive implementation that just assumes the assembly is in the working directory.
            // This does not have any knowledge about the initial assembly we were trying to load.
            Assembly a = System.Reflection.Assembly.ReflectionOnlyLoad(args.Name);

            return a;
        }

        // Dump all of the methods in the given ISymbolReader to the SymbolData provided
        private List<Method> ReadAllMethods(ISymbolReader reader)
        {
            List<Method> methods = new List<Method>();

            // Use reflection to enumerate all methods            
            foreach (MethodBase methodReflection in GetAllMethods(m_assembly))
            {
                int token = methodReflection.MetadataToken;
                ISymbolMethod methodSymbol = reader.GetMethod(new SymbolToken(token));
                if (methodSymbol != null)
                {
                    Method methodData = new Method();
                    methodData.token = Util.AsToken(token);
                    methodData.name = methodReflection.DeclaringType.FullName + "::" + methodReflection.Name;
                    
                    // This localSigMetadataToken information actually comes from the metadata in the assembly because the symbol reading API does not provide it.
                    try
                    {
                        MethodBody body = methodReflection.GetMethodBody();
                        int lSMT = body.LocalSignatureMetadataToken;
                        if (lSMT != 0)
                        {
                            methodData.localSigMetadataToken = Util.AsToken(lSMT);
                        }
                    }
                    catch (System.Security.VerificationException)
                    {
                        // Work around a CLR or C# compiler bug with Void& types in signatures
                        // <strip>See DevDiv Bugs 146662</strip>
                        methodData.hasInvalidMethodBody = true;
                    }

                    methodData.sequencePoints = ReadSequencePoints(methodSymbol);
                    methodData.rootScope = ReadScope(methodSymbol.RootScope);

                    // Read symbol attributes, except on ILDB where it isn't supported
                    if (m_symFormat != SymbolFormat.ILDB)
                    {
                        if (m_expandAttributes)
                            methodData.csharpCDI = ReadCSharpCDI(reader, methodSymbol);
                        methodData.symAttributes = ReadSymAttributes(reader, methodSymbol, methodData.csharpCDI != null);
                    }

                    if (m_symFormat == SymbolFormat.PDB)
                        WorkAroundDiasymreaderScopeBug(methodData.rootScope);

                    methods.Add(methodData);
                }
            }

            return methods;
        }

        /// <summary>
        /// Iterator method to enumerate all the methods and constructors defined in an assembly
        /// </summary>
        /// <param name="asm">The assembly to inspect</param>
        /// <returns>Each method/constructor defined in the specified assembly</returns>
        private static IEnumerable<MethodBase> GetAllMethods(Assembly asm)
        {
            // A set storing all methodDef tokens we've seen
            // Ideally we'd just be able to walk the MethodDef table - we want to return exactly once for
            // each element in this table.  But reflection doesn't give us a way to get a MethodBase
            // from a token, so instead we'll use reflection facilities and then validate that we've
            // found them all.
            SortedDictionary<int, bool> tokenSeenSet = new SortedDictionary<int, bool>();
            
            BindingFlags allDeclared = 
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Static | 
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly;
            foreach (Module mod in asm.GetModules(false))
            {
                // First return all module-global methods
                foreach (MethodInfo meth in mod.GetMethods(allDeclared))
                {
                    Debug.Assert(meth.MetadataToken >> 24 == 0x6, "Got a method token which wasn't a MethodDef");
                    Debug.Assert(!tokenSeenSet.ContainsKey(meth.MetadataToken), "Saw the same method token more than once");
                    tokenSeenSet.Add(meth.MetadataToken, true);
                    yield return meth;
                }

                // Now loop through each type.  Note that this includes nested types.
                foreach(Type type in mod.GetTypes())
                {
                    // Return each constructor
                    foreach (ConstructorInfo con in type.GetConstructors(allDeclared))
                    {
                        Debug.Assert(con.MetadataToken >> 24 == 0x6, "Got a method token which wasn't a MethodDef");
                        Debug.Assert(!tokenSeenSet.ContainsKey(con.MetadataToken), "Saw the same method token more than once");
                        tokenSeenSet.Add(con.MetadataToken, true);
                        yield return con;
                    }

                    // Return each method.
                    // Note that this includes methods that are special in C# like property getters
                    foreach (MethodInfo meth in type.GetMethods(allDeclared))
                    {
                        Debug.Assert(meth.MetadataToken >> 24 == 0x6, "Got a method token which wasn't a MethodDef");
                        Debug.Assert(!tokenSeenSet.ContainsKey(meth.MetadataToken), "Saw the same method token more than once");
                        tokenSeenSet.Add(meth.MetadataToken, true);
                        yield return meth;
                    }
                }

                // Now sanity-check that we haven't missed any methods by looking for gaps in the token list
                // Unfortunately we have no easy way to check that there aren't any extra methods at the end.
                // Tokens are scoped to a module, so we track usage on a per-module basis 
                int lastTok = 0x06000000;
                foreach (int tok in tokenSeenSet.Keys)
                {
                    Debug.Assert(tok > lastTok, "token list is supposed to be sorted");
                    Debug.Assert(tok == lastTok + 1, "missed some method tokens",
                        String.Format("Missed 0x{0:x} to 0x{0:x}", lastTok + 1, tok));
                    lastTok = tok;
                }
                tokenSeenSet.Clear();
            }
        }

        // Write the sequence points for the given method
        // Sequence points are the map between IL offsets and source lines.
        // A single method could span multiple files (use C#'s #line directive to see for yourself).        
        private List<SequencePoint> ReadSequencePoints(ISymbolMethod method)
        {
            int count = method.SequencePointCount;

            // Get the sequence points from the symbol store. 
            // We could cache these arrays and reuse them.
            int[] offsets = new int[count];
            ISymbolDocument[] docs = new ISymbolDocument[count];
            int[] startColumn = new int[count];
            int[] endColumn = new int[count];
            int[] startRow = new int[count];
            int[] endRow = new int[count];
            method.GetSequencePoints(offsets, docs, startRow, startColumn, endRow, endColumn);

            // Store them into the list
            List<SequencePoint> sequencePoints = new List<SequencePoint>(count);
            for (int i = 0; i < count; i++)
            {
                SequencePoint sp = new SequencePoint();
                sp.ilOffset = offsets[i];

                // If it's a special 0xFeeFee sequence point (eg, "hidden"), 
                // place an attribute on it to make it very easy for tools to recognize.
                // See http://blogs.msdn.com/jmstall/archive/2005/06/19/FeeFee_SequencePoints.aspx
                if (startRow[i] == 0xFeeFee)
                {
                    sp.hidden = true;
                }

                sp.sourceId = this.m_fileMapping[docs[i].URL];
                sp.startRow = startRow[i];
                sp.startColumn = startColumn[i];
                sp.endRow = endRow[i];
                sp.endColumn = endColumn[i];

                sequencePoints.Add(sp);
            }

            return sequencePoints;
        }

        private Scope ReadScope(ISymbolScope scope)
        {
            Scope scopeData = new Scope();

            // If this is the root scope, then it was created implicitly and should not be explicitly
            // opened by a writer
            if (scope.Parent == null)
            {
                scopeData.isImplicit = true;
            }
            scopeData.startOffset = scope.StartOffset;
            scopeData.endOffset = scope.EndOffset;

            // Read the locals, constants and namespaces in this scope (may be empty)
            scopeData.locals = ReadLocals(scope);
            scopeData.constants = ReadConstants(scope);
            scopeData.usedNamespaces = ReadUsedNamespaces(scope);

            // Read the child scopes recursively
            scopeData.scopes = new List<Scope>();
            foreach (ISymbolScope child in scope.GetChildren())
            {
                Scope childData = ReadScope(child);
                scopeData.scopes.Add(childData);
            }

            return scopeData;
        }

        // Write the local variables in the given scope.
        // Scopes match an IL range, and also have child scopes.
        private List<Variable> ReadLocals(ISymbolScope scope)
        {
            List<Variable> locals = new List<Variable>();
            foreach (ISymbolVariable l in scope.GetLocals())
            {
                Variable localData = ReadVariable(l);
                locals.Add(localData);
            }
            return locals;
        }

        private List<Variable> ReadVariables(IEnumerable<ISymbolVariable> symVars)
        {
            List<Variable> varList = new List<Variable>();
            foreach (ISymbolVariable symVar in symVars)
            {
                Variable varData = ReadVariable(symVar);
                varList.Add(varData);
            }
            return varList;
        }

        public Variable ReadVariable(ISymbolVariable symVar)
        {
            Variable varData = new Variable();
            varData.name = symVar.Name;

            // Each local maps to a unique "IL Index" or "slot" number.
            // This index is what you pass to ICorDebugILFrame::GetLocalVariable() to get
            // a specific local variable. 
            Debug.Assert(symVar.AddressKind == SymAddressKind.ILOffset);
            varData.ilIndex = symVar.AddressField1;

            varData.attributes = (int)symVar.Attributes;

            Byte[] b_signature = symVar.GetSignature();
            varData.signature = Util.ToHexString(b_signature);

            return varData;
        }

        private List<Constant> ReadConstants(ISymbolScope scope)
        {
            // Read the constants.
            // IldbSymbols.dll doesn't support ISymUnmanagedScope2 so nothing we can do for ILDB to read
            // constants (even though it does support writing them).  But RefEmit doesn't ever emit constants
            // anyway, so there should be no need for this.
            if (m_symFormat == SymbolFormat.ILDB)
            {
                return null;
            }

            // Note - ISymbolConstants are written to the xml, but cannot be easily round-tripped.
            // The SigTokens cannot be easily retrieved from either the pdb or the assembly metadata
            List<Constant> constants = new List<Constant>();
            ISymbolScope2 scope2 = (ISymbolScope2)scope;
            foreach (ISymbolConstant c in scope2.GetConstants())
            {
                Constant constData = new Constant();
                constData.name = c.GetName();
                constData.value = c.GetValue().ToString();
                constData.signature = Util.ToHexString(c.GetSignature());
                constants.Add(constData);
            }
            return constants;
        }


        private List<Namespace> ReadUsedNamespaces(ISymbolScope scope)
        {
            List<Namespace> namespaces = new List<Namespace>();

            foreach (ISymbolNamespace symNs in scope.GetNamespaces())
            {
                Namespace n = new Namespace();
                n.name = symNs.Name;
                namespaces.Add(n);
            }
            return namespaces;
        }

        // Write all docs, and add to the m_fileMapping list.
        // Other references to docs will then just refer to this list.
        private List<Document> ReadDocList(ISymbolReader reader)
        {
            List<Document> docs = new List<Document>();

            int id = 0;
            foreach (ISymbolDocument doc in reader.GetDocuments())
            {
                Document docData = new Document();
                string url = doc.URL;

                // Symbol store may give out duplicate documents. We'll fold them here
                if (m_fileMapping.ContainsKey(url))
                {
                    continue;
                }
                id++;
                m_fileMapping.Add(url, id);

                docData.id = id;
                docData.url = url;
                docData.language = doc.Language;
                docData.languageVendor = doc.LanguageVendor;
                docData.documentType = doc.DocumentType;
                docs.Add(docData);
            }
            return docs;
        }

        // Write out a reference to the entry point method (if one exists)
        private string ReadEntryPoint(ISymbolReader reader)
        {
            // If there is no entry point token (such as in a dll), this will throw.
            int tokenVal = reader.UserEntryPoint.GetToken();
            if (tokenVal == 0)
            {
                // If the Symbol APIs fail when looking for an entry point token, there is no entry point
                // (eg. probably a dll)
                return null;
            }

            return Util.AsToken(tokenVal);
        }

        /// <summary>
        /// ISymUnmanagedReader provides no mechanism to enumerate custom attributes.  So in order
        /// to support reading them, we need to probe for possible names.
        /// In more complex scenarios I could imagine computing values dynamically here, possibly based
        /// on metadata for the method in question.
        /// </summary>
        /// <returns>A sequence of attribute names to try reading for each method</returns>
        private IEnumerable<string> AttributeNamesToSearch()
        {
            // Used by the C# compiler to store method debug information
            yield return k_cSharpCDIAttrName;     
        }

        private const string k_cSharpCDIAttrName = "MD2";

        private List<SymAttribute> ReadSymAttributes(ISymbolReader reader, ISymbolMethod method, bool haveCSharpCDI)
        {
            List<SymAttribute> attrs = new List<SymAttribute>();
            foreach (string name in AttributeNamesToSearch())
            {
                // If this attirubte represents C# CDI data, and we were able to expand it, then indicate that
                // the attribute here is redundant (as a raw view) and shouldn't be included again.
                if (name == k_cSharpCDIAttrName && haveCSharpCDI)
                    continue;

                // Note that despite being defined on ISymbolReader instead of ISymbolMethod, custom
                // attributes are permitted only on method metadata tokens.
                byte[] attrVal = reader.GetSymAttribute(method.Token, name);
                if (attrVal != null)
                {
                    SymAttribute attr = new SymAttribute();
                    attr.name = name;
                    attr.value = Util.ToHexString(attrVal);
                    attrs.Add(attr);
                }
            }
            return attrs;
        }

        /// <summary>
        /// Work-around for a bug in diasymreader.  
        /// </summary>
        /// <param name="rootScope"></param>
        /// <remarks>
        /// There should always be at least one explicit scope written, since this is how the writer determines
        /// the end offset for the root scope (and hence the maximum offset permitted by any sequence point -
        /// other will be silently removed). But if a scope has no locals or child scopes in it, diasymreader will 
        /// omit it, and we won't see it in the reader.  So if we read a root scope with nothing inside of it,
        /// we'll reconstruct the child scope we know must have originally been written there.
        /// </remarks>
        private void WorkAroundDiasymreaderScopeBug(Scope rootScope)
        {
            if ((rootScope.scopes.Count > 0) ||
                (rootScope.locals.Count > 0) ||
                (rootScope.usedNamespaces.Count > 0) ||
                (rootScope.constants.Count > 0))
            {
                // There is something inside this scope - there shouldn't be any problem with diasymreader
                // removing it.
                return;
            }

            // We've got a root scope with nothing in it.  Reconstruct the explicit scope we know must have
            // been written here.
            Scope child = new Scope();
            child.startOffset = rootScope.startOffset;
            child.endOffset = rootScope.endOffset;
            child.isReconstructedDueToDiasymreaderBug = true;
            rootScope.scopes.Add(child);
        }

        private CSharpCDI ReadCSharpCDI(ISymbolReader reader, ISymbolMethod methodSymbol)
        {
            // See if the C# CDI attribute exists
            byte[] attrVal = reader.GetSymAttribute(methodSymbol.Token, k_cSharpCDIAttrName);
            if (attrVal == null)
                return null;        // No such attribute

            try
            {
                return CDIReader.ParseCDI(attrVal);
            }
            catch (System.FormatException e)
            {
                Console.WriteLine("WARNING: Error parsing CSharp CDI for method {0}: {1}", Util.AsToken(methodSymbol.Token.GetToken()), e.Message);
                return null;
            }
        }

        private class CDIReader : BinaryReader
        {
            public static CSharpCDI ParseCDI(byte[] val)
            {
                CDIReader r = new CDIReader(val);

                CSharpCDI cdi = new CSharpCDI();

                try
                {
                    // Read the CDIGlobal header
                    byte version = r.ReadByte();
                    if (version != CDIVERSION)
                        throw new FormatException("Got unexpected CDIGlobal version: " + version);
                    cdi.version = version;

                    byte count = r.ReadByte();
                    r.SkipPadding(4);
                    cdi.entries = new CDIItem[count];

                    // Read each CDI record
                    for (int i = 0; i < count; i++)
                    {
                        cdi.entries[i] = r.ReadItem();
                    }
                }
                catch(EndOfStreamException e)
                {
                    throw new FormatException("Unexpected end of CDI stream", e);
                }
                return cdi;
            }

            private CDIReader(byte[] val) : base(new MemoryStream(val), Encoding.Unicode)
            {
            }

            private void SkipPadding(int alignment)
            {
                // Note that the padding written by the C# compiler isn't always zeroed, 
                // sometimes it's left uninitialized.  So technically we are loosing
                // some information here, but it's useless information.
                while (Index % alignment != 0)
                    ReadByte();
            }

            // Read a null-terminated Unicode string
            public override string ReadString()
            {
                StringBuilder sb = new StringBuilder();
                while (true)
                {
                    char c = ReadChar();
                    if (c == 0)
                        break;
                    sb.Append(c);
                }
                return sb.ToString();
            }

            const byte CDIVERSION = 4;
            private enum CDIKind : byte
            {
                UsingInfo = 0,
                ForwardInfo = 1,
                ForwardToModuleInfo = 2,
                IteratorLocals = 3,
                ForwardIterator = 4,
            }

            public int Index
            {
                get 
                { 
                    return (int)BaseStream.Position; 
                }
            }

            public CDIItem ReadItem()
            {
                long nextIndex = Index;

                // Read the CDIBaseInfo header
                byte version = ReadByte();
                if (version != CDIVERSION)
                    throw new FormatException("Got unexpected CDIBaseInfo version at " + Index);
                byte kind = ReadByte();
                SkipPadding(4);
                uint size = ReadUInt32();
                nextIndex += size;

                CDIItem item = null;

                // Read kind-specific data
                switch ((CDIKind)kind)
                {
                    case CDIKind.UsingInfo:
                        ushort cUsings = ReadUInt16();
                        CDIUsing cdiUsing = new CDIUsing();
                        cdiUsing.countOfUsing = new int[cUsings];
                        for (ushort j = 0; j < cUsings; j++)
                            cdiUsing.countOfUsing[j] = ReadUInt16();
                        SkipPadding(4);
                        item = cdiUsing;
                        break;

                    case CDIKind.ForwardInfo:
                        uint tokenToForwardTo = ReadUInt32();
                        CDIForward fwd = new CDIForward();
                        fwd.tokenToForwardTo = Util.AsToken((int)tokenToForwardTo);
                        item = fwd;
                        break;

                    case CDIKind.ForwardToModuleInfo:
                        uint token = ReadUInt32();
                        CDIForwardModule modFwd = new CDIForwardModule();
                        modFwd.tokenOfModuleInfo = Util.AsToken((int)token);
                        item = modFwd;
                        break;

                    case CDIKind.IteratorLocals:
                        int cBuckets = ReadInt32();
                        if (cBuckets < 0)
                            throw new FormatException("Unexpected negative iteratorLocal bucket count at " + BaseStream.Position);
                        CDIIteratorLocals iterLocals = new CDIIteratorLocals();
                        iterLocals.buckets = new CDIIteratorLocalBucket[cBuckets];
                        for (int j = 0; j < cBuckets; j++)
                        {
                            CDIIteratorLocalBucket b = new CDIIteratorLocalBucket();
                            b.ilOffsetStart = ReadInt32();
                            b.ilOffsetEnd = ReadInt32();
                            iterLocals.buckets[j] = b;
                        }
                        item = iterLocals;
                        break;

                    case CDIKind.ForwardIterator:
                        CDIForwardIterator fwdIter = new CDIForwardIterator();
                        fwdIter.iteratorClassName = ReadString();
                        SkipPadding(4);
                        item = fwdIter;
                        break;

                    default:
                        CDIUnknown u = new CDIUnknown();
                        u.kind = kind;
                        byte[] bytes = ReadBytes(checked((int)(nextIndex - Index)));
                        u.bytes = Util.ToHexString(bytes);
                        item = u;
                        break;
                }

                if (Index != nextIndex)
                {
                    throw new FormatException(String.Format("Expected CDI item to end at index {0}, but ended at {1} instead", nextIndex, Index));
                }
                item.version = version;
                return item;
            }
        }

    }
}
