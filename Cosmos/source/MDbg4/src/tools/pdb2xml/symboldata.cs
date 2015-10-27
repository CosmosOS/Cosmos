using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

// 
// This file contains type definitions for storing interesting information from a symbol file (corresponding
// to the ISymbolReader APIs).
// 
// These types intended to be written/read using XmlSerializer, which restricts us to using public
// read/write fields and default constructors.  These types are all dumb containers.
// 
// These data structures capture all the information available from diasymreader.dll v2.0.50727 (Whidbey RTM),
// except for:
//   - Differentiation of multiple documents with the same URL
//      Diasymreader allows multiple documents to be created with the same URL, and this has an affect on semantics
//      of the various reader APIs.  However, there doesn't appear to be any good way to match a document and so
//      differentiate between two documents that have the same URL (one heuristic may be to see what FindClosestLine(0)
//      returns for each).
//   - All Symbol custom attributes:
//      There are no APIs to enumerate these (you must know the name to look for).  Pdb2Xml supports reading only
//      attribute names that are known to be used (eg. by C#), but it will write any name given.
//  Several additional APIs (globals, parameters, fields, namespace definitions) are known to not be
//  implemented, and probably never will be.
// 

namespace Pdb2Xml
{
    // TODO: can I have the Xml serializer auto-convert hex numbers?  Eg. XmlElement(datatype="hexBinary")
    
    /// <summary>
    /// Represents all the data we want to read out of an ISymbolReader
    /// </summary>
    public class SymbolData
    {
        /// <summary>
        /// Path of the assembly for which these symbols correspond.  
        /// For informational purposes only - not written to the symbol store
        /// </summary>
        [XmlAttribute]
        public string assembly;

        /// <summary>
        /// MethodDef token (in hex) of the user entrypoint method, or null if none (eg. a DLL)
        /// </summary>
        public string entryPointToken;

        [XmlArray]
        [XmlArrayItem(ElementName= "document")]
        public List<Document> sourceFiles;

        [XmlElement(ElementName = "method")]
        public List<Method> methods;
    }

    /// <summary>
    /// Data from an ISymbolDocument
    /// </summary>
    public class Document
    {
        [XmlAttribute]
        public int id;

        [XmlAttribute]
        public string url;

        [XmlAttribute]
        public Guid language;

        [XmlAttribute]
        public Guid languageVendor;

        [XmlAttribute]
        public Guid documentType;
    }

    /// <summary>
    /// Data from an ISymbolMethod
    /// </summary>
    public class Method
    {
        /// <summary>
        /// The type and name of this method.
        /// For informational purposes only - not written to the symbol store
        /// </summary>
        [XmlAttribute]
        public string name;

        /// <summary>
        /// MethodDef token (in hex) of the method
        /// </summary>
        [XmlAttribute]
        public string token;

        /// <summary>
        /// Signature token (in hex) of the local variable signature for this method (or null if no locals)
        /// This can't be read from the symbol store directly (it's read by reflection), but can be used when writing locals.
        /// </summary>
        [XmlAttribute]
        public string localSigMetadataToken;

        /// <summary>
        /// This is set to true by the reader when it can't read the method body from the method because the CLR
        /// claims it is invalid.  There is at least one known C# compiler bug that can cause this.  The
        /// reader will not be able to provide a localSigMetadataToken in this case, and so the writer may not
        /// be able to write out the locals.
        /// </summary>
        [System.ComponentModel.DefaultValue(false)]
        [XmlAttribute]
        public bool hasInvalidMethodBody;

        [XmlArray]
        [XmlArrayItem(ElementName="seqPoint")]
        public List<SequencePoint> sequencePoints;

        public Scope rootScope;

        [XmlArray]
        [XmlArrayItem(ElementName = "attribute")]
        public List<SymAttribute> symAttributes;

        /// <summary>
        /// Optionally, C# custom debug information
        /// </summary>
        [XmlElement(ElementName="csharpCustomDebugInfo")]
        public CSharpCDI csharpCDI;
    }

    /// <summary>
    /// Data from ISymbolMethod.GetSequencePoints
    /// </summary>
    public class SequencePoint
    {
        [XmlAttribute]
        public int ilOffset;

        [XmlAttribute]
        public int sourceId;

        /// <summary>
        /// If true, this sequence point corresponds to a hidden (0xfeefee) sequence point.
        /// This property isn't directly read or written from the symbols, but inferred by the line number value.
        /// </summary>
        [XmlAttribute]
        [System.ComponentModel.DefaultValue(false)]
        public bool hidden;

        [XmlAttribute]
        public int startRow;

        [XmlAttribute]
        public int startColumn;

        [XmlAttribute]
        public int endRow;

        [XmlAttribute]
        public int endColumn;
    }

    /// <summary>
    /// Data from an ISymbolScope
    /// </summary>
    public class Scope
    {
        /// <summary>
        /// True if this scope is created implicity by the symbol library and so should not explicitly
        /// written out.
        /// </summary>
        [XmlAttribute(AttributeName="implicit")]
        [System.ComponentModel.DefaultValue(false)]
        public bool isImplicit;

        [XmlAttribute]
        public int startOffset;

        [XmlAttribute]
        public int endOffset;

        [XmlElement(ElementName = "local")]
        public List<Variable> locals;

        [XmlElement(ElementName = "constant")]
        public List<Constant> constants;

        [XmlElement(ElementName="scope")]
        public List<Scope> scopes;

        [XmlElement(ElementName = "usingNamespace")]
        public List<Namespace> usedNamespaces;

        /// <summary>
        /// True if this scope didn't actually show up while reading a PDB, but we know it must have
        /// been written there.  See SymbolDataReader.WorkAroundDiasymreaderScopeBug for details.
        /// </summary>
        [XmlAttribute]
        [System.ComponentModel.DefaultValue(false)]
        public bool isReconstructedDueToDiasymreaderBug;
    }

    /// <summary>
    /// Data from an ISymbolVariable
    /// </summary>
    public class Variable
    {
        [XmlAttribute]
        public string name;

        [XmlAttribute]
        public int ilIndex;

        [XmlAttribute]
        public int attributes;

        [XmlAttribute]
        public string signature;
    }

    /// <summary>
    /// Data from an ISymbolConstant
    /// </summary>
    public class Constant
    {
        [XmlAttribute]
        public string name;

        [XmlAttribute]
        public string value;

        [XmlAttribute]
        public string signature;
    }

    /// <summary>
    /// Data from an ISymbolNamespace
    /// Note that most uses of namespaces in the symbol APIs aren't actually implemented.
    /// Namespaces are just a name (no children or variables) and are used only in scopes to 
    /// say which namespaces are being "used".
    /// </summary>
    public class Namespace
    {
        [XmlAttribute]
        public string name;
    }

    /// <summary>
    /// Data returned by ISymUnmanagedReader::GetSymAttribute
    /// </summary>
    public class SymAttribute
    {
        [XmlAttribute]
        public string name;

        [XmlAttribute]
        public string value;   // hex bytes
    }

    //
    // The following are used to represent C# custom debug information
    // Theese structures are undocumented implementation details of the C#
    // compiler and debugger interaction.
    //

    public class CSharpCDI
    {
        [XmlAttribute]
        public int version;

        [XmlArrayItem(Type = typeof(CDIUsing), ElementName="using"),
         XmlArrayItem(Type = typeof(CDIForward), ElementName="usingForward"),
         XmlArrayItem(Type = typeof(CDIForwardModule), ElementName="usingForwardToModule"),
         XmlArrayItem(Type = typeof(CDIIteratorLocals), ElementName="iteratorLocals"),
         XmlArrayItem(Type = typeof(CDIForwardIterator), ElementName="forwardIterator"),
         XmlArrayItem(Type = typeof(CDIUnknown), ElementName="unknown")]
        public CDIItem[] entries;
    }

    public abstract class CDIItem
    {
        [XmlAttribute]
        public int version;
    }

    public class CDIUsing : CDIItem
    {
        /// <summary>
        /// This appears to be used as follows:
        /// There is one entry for each level of declaration nesting for the containing method.
        /// The value indicates the number of using-namespace declarations (from the PDB scope using
        /// data) that should be imported, and applied to the indicated nesting level.
        /// Presumably this impacts what symbols are available when doing expression evaluation in the
        /// context of a method.
        /// </summary>
        [XmlElement]
        public int[] countOfUsing;
    }

    public class CDIForward : CDIItem
    {
        /// <summary>
        /// This data indicates that CDIUsing information should be imported from the specified
        /// method.  Presumably this is an optimizaton to avoid duplicating all the using namespace
        /// names in each method.
        /// </summary>
        [XmlAttribute]
        public string tokenToForwardTo;
    }

    public class CDIForwardModule : CDIItem
    {
        /// <summary>
        /// This is similar to tokenToForwardTo, but appears to be treated slightly differently somehow.
        /// </summary>
        [XmlAttribute]
        public string tokenOfModuleInfo;
    }

    public class CDIIteratorLocalBucket
    {
        [XmlAttribute]
        public int ilOffsetStart;

        [XmlAttribute]
        public int ilOffsetEnd;
    }

    public class CDIIteratorLocals : CDIItem
    {
        /// <summary>
        /// This causes local variables in iterator methods (which are actually implemented 
        /// as fields on the generated class) to be visible in the debugger.
        /// </summary>
        [XmlElement(ElementName="bucket")]
        public CDIIteratorLocalBucket[] buckets;
    }

    public class CDIForwardIterator : CDIItem
    {
        /// <summary>
        /// This indicates that the current method is an iterator method and is implemented
        /// by the specified generated class.  This causes callstacks to display the name of 
        /// this method when they're actually inside a method on the generated iterator class.
        /// </summary>
        [XmlAttribute]
        public string iteratorClassName;
    }

    public class CDIUnknown : CDIItem
    {
        [XmlAttribute]
        public int kind;

        [XmlAttribute]
        public string bytes;
    }
}   
