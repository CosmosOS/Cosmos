//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
using System;
using System.Reflection;
using System.Text;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Collections;
using System.Diagnostics;

using Microsoft.Samples.Debugging.CorDebug;
using Microsoft.Samples.Debugging.CorMetadata.NativeApi;
using Microsoft.Samples.Debugging.CorDebug.NativeApi;
using System.Collections.Generic;

namespace Microsoft.Samples.Debugging.CorMetadata
{
    public sealed class CorMetadataImport
    {
        public CorMetadataImport(CorModule managedModule)
        {
            // With a dump, m_importer may be null; metadata is not assured.
            m_importer = managedModule.GetMetaDataInterface<IMetadataImport>();
        }

        public CorMetadataImport(object metadataImport)
        {
            // With a dump, m_importer may be null; metadata is not assured.
            m_importer = (IMetadataImport)metadataImport;
        }

        // methods
        public MetadataMethodInfo GetMethodInfo(MetadataToken token)
        {
            return new MetadataMethodInfo(m_importer, token.Value);
        }
        public MethodInfo GetMethodInfo(int methodToken)
        {
            return new MetadataMethodInfo(m_importer, methodToken);
        }

        public Type GetType(int typeToken)
        {
            return new MetadataType(m_importer, typeToken);
        }


        // Get number of generic parameters on a given type.
        // Eg, for 'Foo<t, u>', returns 2.
        // for 'Foo', returns 0.
        public int CountGenericParams(int typeToken)
        {
            // This may fail in pre V2.0 debuggees.
            // Guid IID_IMetadataImport2 = new Guid("FCE5EFA0-8BBA-4f8e-A036-8F2022B08466");
            if (!(m_importer is IMetadataImport2))
                return 0; // this means we're pre v2.0 debuggees.

            IMetadataImport2 importer2 = (m_importer as IMetadataImport2);

            int dummy;
            uint dummy2;
            IntPtr hEnum = IntPtr.Zero;
            int count;
            importer2.EnumGenericParams(ref hEnum, typeToken, out dummy, 1, out dummy2);
            try
            {
                m_importer.CountEnum(hEnum, out count);
            }
            finally
            {
                if (hEnum != IntPtr.Zero)
                    importer2.CloseEnum(hEnum);
            }
            return count;
        }

        // Returns filename of scope, if available.
        // Returns null if filename is not available.
        public string GetScopeFilename()
        {
            int size;

            try
            {
                Guid mvid;
                m_importer.GetScopeProps(null, 0, out size, out mvid);
                StringBuilder sb = new StringBuilder(size);
                m_importer.GetScopeProps(sb, sb.Capacity, out size, out mvid);
                sb.Length = size - 1;   // size returned by GetScopeProps includes null terminator
                return sb.ToString();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get a user string for the mdString token
        /// </summary>
        /// <param name="token">an mdString token</param>
        /// <returns>string for the token via IMetaDataImport::GetUserString</returns>
        public string GetUserString(int token)
        {
            int size;
            m_importer.GetUserString(token, null, 0, out size);
            StringBuilder sb = new StringBuilder(size);
            m_importer.GetUserString(token, sb, sb.Capacity, out size);
            sb.Length = size;
            return sb.ToString();
        }

        public const int TokenNotFound = -1;
        public const int TokenGlobalNamespace = 0;

        // returns a type token from name
        // when the function fails, we return token TokenNotFound value.
        public int GetTypeTokenFromName(string name)
        {
            int token = CorMetadataImport.TokenNotFound;
            if (name.Length == 0)
            {
                // this is special global type (we'll return token 0)
                token = CorMetadataImport.TokenGlobalNamespace;
            }
            else
            {
                try
                {
                    m_importer.FindTypeDefByName(name, 0, out token);
                }
                catch (COMException e)
                {
                    token = CorMetadataImport.TokenNotFound;
                    if ((HResult)e.ErrorCode == HResult.CLDB_E_RECORD_NOTFOUND)
                    {
                        // The '.' separates namespaces, and the '+' separates nested classes.
                        int i = name.LastIndexOfAny(new char[] { '.', '+' });
                        if (i > 0)
                        {
                            int parentToken = GetTypeTokenFromName(name.Substring(0, i));
                            if (parentToken != CorMetadataImport.TokenNotFound)
                            {
                                try
                                {
                                    m_importer.FindTypeDefByName(name.Substring(i + 1), parentToken, out token);
                                }
                                catch (COMException e2)
                                {
                                    token = CorMetadataImport.TokenNotFound;
                                    if ((HResult)e2.ErrorCode != HResult.CLDB_E_RECORD_NOTFOUND)
                                        throw;
                                }
                            }
                        }
                    }
                    else
                        throw;
                }
            }
            return token;
        }

        /// <summary>
        /// Given a type ref token, get the name. 
        /// </summary>
        /// <param name="token">mdTypeRef token</param>
        /// <returns>name of typeref</returns>
        /// <remarsks>A typeref also has a resolution scope (mdModuleRef or mdAssemblyRef). The
        /// name can then be used in that resolution scope to get a typedef. </remarsks>
        public string GetTypeNameFromRef(int token)
        {
            int resScope; // ignore
            int size; // length in characters

            m_importer.GetTypeRefProps(token, out resScope, null, 0, out size);
            StringBuilder sb = new StringBuilder(size);
            m_importer.GetTypeRefProps(token, out resScope, sb, sb.Capacity, out size);
            return sb.ToString();
        }

        public string GetTypeNameFromDef(int token, out int extendsToken)
        {
            int size;
            TypeAttributes pdwTypeDefFlags;
            m_importer.GetTypeDefProps(token, null, 0, out size,
                                       out pdwTypeDefFlags, out extendsToken);
            StringBuilder sb = new StringBuilder(size);
            m_importer.GetTypeDefProps(token, sb, sb.Capacity, out size,
                                       out pdwTypeDefFlags, out extendsToken);
            return sb.ToString();
        }


        public string GetMemberRefName(int token)
        {
            if (!m_importer.IsValidToken((uint)token))
                throw new ArgumentException();

            uint size;
            int classToken;
            IntPtr ppvSigBlob;
            int pbSig;

            m_importer.GetMemberRefProps((uint)token,
                                         out classToken,
                                         null,
                                         0,
                                         out size,
                                         out ppvSigBlob,
                                         out pbSig
                                         );

            StringBuilder member = new StringBuilder((int)size);
            m_importer.GetMemberRefProps((uint)token,
                                         out classToken,
                                         member,
                                         member.Capacity,
                                         out size,
                                         out ppvSigBlob,
                                         out pbSig
                                         );

            string className = null;
            switch (TokenUtils.TypeFromToken(classToken))
            {
                default:
                    Debug.Assert(false);
                    break;
                case CorTokenType.mdtTypeRef:
                    className = GetTypeNameFromRef(classToken);
                    break;
                case CorTokenType.mdtTypeDef:
                    {
                        int parentToken;
                        className = GetTypeNameFromDef(classToken, out parentToken);
                        break;
                    }
                case CorTokenType.mdtTypeSpec:
                    {
                        className = string.Format("NOTIMPL<TypeSpec({0})>", classToken);
                        break;
                    }
            }
            return className + "." + member.ToString();
        }


        /// <summary>
        /// Enumerate all the types in the module
        /// </summary>
        public IEnumerable DefinedTypes
        {
            get
            {
                return new TypeDefEnum(this);
            }
        }

        // helper to get number of rows in MethodDef table.
        int GetTokenRows(MetadataTokenType tableIndex)
        {
            IMetadataTables table = (IMetadataTables)this.m_importer;
            uint tbl = (uint)tableIndex >> 24;
            uint cbRow;
            uint cRows;
            uint cColumns;
            uint iKey;
            string name;
            table.GetTableInfo(tbl, out cbRow, out cRows, out cColumns, out iKey, out name);
            return (int)cRows;
        }

        /// <summary>
        /// Enumerate all methodDef tokens in the importer.
        /// </summary>        
        /// <seealso cref="CorMetadataImport.EnumerateAllMethods"/>
        public IEnumerable<MetadataToken> EnumerateAllMethodTokens()
        {
            int maxRows = GetTokenRows(MetadataTokenType.MethodDef);
            for (int i = 1; i <= maxRows; i++)
            {
                yield return new MetadataToken(MetadataTokenType.MethodDef, i);
            }
        }

        /// <summary>
        /// Convenience method to enumerate all of the methods in the module.
        /// </summary>
        /// <returns>An enumeration of all methods in the module</returns>
        /// <seealso cref="CorMetadataImport.EnumerateAllMethodTokens"/>
        public IEnumerable<MetadataMethodInfo> EnumerateAllMethods()
        {
            // This is easier than enumerating all types and then enumerating methods in the types.
            // And it catches corner cases (like globals).
            // And it can be significantly faster. 
            int maxRows = GetTokenRows(MetadataTokenType.MethodDef);
            for (int i = 1; i <= maxRows; i++)
            {
                yield return GetMethodInfo(new MetadataToken(MetadataTokenType.MethodDef, i));
            }
        }

        public object RawCOMObject
        {
            get
            {
                return m_importer;
            }
        }

        // properties


        //////////////////////////////////////////////////////////////////////////////////
        //
        // CorMetadataImport variables
        //
        //////////////////////////////////////////////////////////////////////////////////

        internal IMetadataImport m_importer;
    }

    //////////////////////////////////////////////////////////////////////////////////
    //
    // MetadataMethodInfo
    //
    //////////////////////////////////////////////////////////////////////////////////

    public sealed class MetadataMethodInfo : MethodInfo
    {
        internal MetadataMethodInfo(IMetadataImport importer, int methodToken)
        {
            if (!importer.IsValidToken((uint)methodToken))
                throw new ArgumentException();

            m_importer = importer;
            m_methodToken = methodToken;

            int size;
            uint pdwAttr;
            IntPtr ppvSigBlob;
            uint pulCodeRVA, pdwImplFlags;
            uint pcbSigBlob;

            m_importer.GetMethodProps((uint)methodToken,
                                      out m_classToken,
                                      null,
                                      0,
                                      out size,
                                      out pdwAttr,
                                      out ppvSigBlob,
                                      out pcbSigBlob,
                                      out pulCodeRVA,
                                      out pdwImplFlags);

            StringBuilder szMethodName = new StringBuilder(size);
            m_importer.GetMethodProps((uint)methodToken,
                                    out m_classToken,
                                    szMethodName,
                                    szMethodName.Capacity,
                                    out size,
                                    out pdwAttr,
                                    out ppvSigBlob,
                                    out pcbSigBlob,
                                    out pulCodeRVA,
                                    out pdwImplFlags);

            m_name = szMethodName.ToString();
            m_methodAttributes = (MethodAttributes)pdwAttr;
        }

        public override Type ReturnType
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override ICustomAttributeProvider ReturnTypeCustomAttributes
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override Type ReflectedType
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Type that the method is declared in.
        /// </summary>
        public override Type DeclaringType
        {
            get
            {
                if (TokenUtils.IsNullToken(m_classToken))
                    return null;                            // this is method outside of class

                return new MetadataType(m_importer, m_classToken);
            }
        }

        /// <summary>
        /// Name of the Method, not including the class name.
        /// </summary>
        public override string Name
        {
            get
            {
                return m_name;
            }
        }

        public override MethodAttributes Attributes
        {
            get
            {
                return m_methodAttributes;
            }
        }

        public override RuntimeMethodHandle MethodHandle
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override MethodInfo GetBaseDefinition()
        {
            throw new NotImplementedException();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            throw new NotImplementedException();
        }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override System.Reflection.MethodImplAttributes GetMethodImplementationFlags()
        {
            throw new NotImplementedException();
        }

        public override System.Reflection.ParameterInfo[] GetParameters()
        {
            ArrayList al = new ArrayList();
            IntPtr hEnum = new IntPtr();
            try
            {
                while (true)
                {
                    uint count;
                    int paramToken;
                    m_importer.EnumParams(ref hEnum,
                                          m_methodToken, out paramToken, 1, out count);
                    if (count != 1)
                        break;
                    al.Add(new MetadataParameterInfo(m_importer, paramToken,
                                                     this, DeclaringType));
                }
            }
            finally
            {
                m_importer.CloseEnum(hEnum);
            }
            return (ParameterInfo[])al.ToArray(typeof(ParameterInfo));
        }

        /// <summary>
        /// Metadata token for this method. Be sure to use this in the proper scope.
        /// </summary>
        public override int MetadataToken
        {
            get
            {
                return m_methodToken;
            }
        }

        public string[] GetGenericArgumentNames()
        {
            return MetadataHelperFunctions.GetGenericArgumentNames(m_importer, m_methodToken);
        }


        /// <summary>
        /// Does the function have the custom attribute on it.
        /// </summary>
        /// <param name="fullname">full name (including namespace and class, and "Attribute" suffix) of type to check</param>
        /// <returns>true if this method has the custom attribute, else false</returns>
        public bool HasCustomAttribute(string fullName)
        {
            // Metadata hands back blob for attribute. We only care if the attribute is present,
            // so we ignore any additional data. We would need some fancy metadata parsing to
            // decode the data.  If the attribute doesn't have any parameters, then these won't
            // get set anyways.
            IntPtr ppData;
            uint pcbData;

            // Returns S_OK (0) if attribute is found, else S_FALSE if attribute is not found.
            int val = m_importer.GetCustomAttributeByName(m_methodToken, fullName, out ppData, out pcbData);
            return (val == 0);

        }

        private IMetadataImport m_importer;
        private string m_name;
        private int m_classToken;
        private int m_methodToken;
        private MethodAttributes m_methodAttributes;
    }

    public enum MetadataTokenType
    {
        Module = 0x00000000,
        TypeRef = 0x01000000,
        TypeDef = 0x02000000,
        FieldDef = 0x04000000,
        MethodDef = 0x06000000,
        ParamDef = 0x08000000,
        InterfaceImpl = 0x09000000,
        MemberRef = 0x0a000000,
        CustomAttribute = 0x0c000000,
        Permission = 0x0e000000,
        Signature = 0x11000000,
        Event = 0x14000000,
        Property = 0x17000000,
        ModuleRef = 0x1a000000,
        TypeSpec = 0x1b000000,
        Assembly = 0x20000000,
        AssemblyRef = 0x23000000,
        File = 0x26000000,
        ExportedType = 0x27000000,
        ManifestResource = 0x28000000,
        GenericPar = 0x2a000000,
        MethodSpec = 0x2b000000,
        String = 0x70000000,
        Name = 0x71000000,
        BaseType = 0x72000000,
        Invalid = 0x7FFFFFFF,
    }

    public enum CorCallingConvention
    {
        Default = 0x0,

        VarArg = 0x5,
        Field = 0x6,
        LocalSig = 0x7,
        Property = 0x8,
        Unmanaged = 0x9,
        GenericInst = 0xa,  // generic method instantiation
        NativeVarArg = 0xb,  // used ONLY for 64bit vararg PInvoke calls

        // The high bits of the calling convention convey additional info
        Mask = 0x0f,  // Calling convention is bottom 4 bits
        HasThis = 0x20,  // Top bit indicates a 'this' parameter
        ExplicitThis = 0x40,  // This parameter is explicitly in the signature
        Generic = 0x10,  // Generic method sig with explicit number of type arguments (precedes ordinary parameter count)
    };



    public struct MetadataToken
    {
        public MetadataToken(int value)
        {
            this.value = value;
        }
        public MetadataToken(MetadataTokenType mdType, int rid)
        {
            this.value = (int)((uint)mdType + (uint)rid);
            Debug.Assert(this.Type == mdType);
            Debug.Assert(this.Index == rid);
        }

        /// <summary>
        /// Get the raw value of the token
        /// </summary>
        public int Value
        {
            get
            {
                return value;
            }
        }

        /// <summary>
        /// Get the type of the token (top 2 nibbles), such as mdTypeDef, etc.
        /// </summary>
        public MetadataTokenType Type
        {
            get
            {
                return (MetadataTokenType)(value & 0xFF000000);
            }
        }

        /// <summary>
        /// Get the index (RID, bottom 6 nibbles) of the token. 
        /// </summary>
        public int Index
        {
            get
            {
                return value & 0x00FFFFFF;
            }
        }

        public static implicit operator int(MetadataToken token) { return token.value; }
        public static bool operator ==(MetadataToken v1, MetadataToken v2) { return (v1.value == v2.value); }
        public static bool operator !=(MetadataToken v1, MetadataToken v2) { return !(v1 == v2); }

        public static bool IsTokenOfType(int token, params MetadataTokenType[] types)
        {
            for (int i = 0; i < types.Length; i++)
            {
                if ((int)(token & 0xFF000000) == (int)types[i])
                    return true;
            }

            return false;
        }

        public bool IsOfType(params MetadataTokenType[] types)
        {
            return IsTokenOfType(Value, types);
        }

        public override bool Equals(object other)
        {
            if (other is MetadataToken)
            {
                MetadataToken oToken = (MetadataToken)other;
                return (value == oToken.value);
            }
            return false;
        }

        public override int GetHashCode() { return value.GetHashCode(); }

        private int value;

        public override string ToString()
        {
            // Tokens generally appears as 8-digit hex.
            return String.Format("{0,8:x}", value);
        }
    }

    static class MetadataHelperFunctions
    {
        private static uint TokenFromRid(uint rid, uint tktype) { return (rid) | (tktype); }

        // The below have been translated manually from the inline C++ helpers in cor.h            
        internal static uint CorSigUncompressBigData(
            ref IntPtr pData)             // [IN,OUT] compressed data 
        {
            unsafe
            {
                byte* pBytes = (byte*)pData;
                uint res;

                // 1 byte data is handled in CorSigUncompressData   
                //  Debug.Assert(*pBytes & 0x80);    

                // Medium.  
                if ((*pBytes & 0xC0) == 0x80)  // 10?? ????  
                {
                    res = (uint)((*pBytes++ & 0x3f) << 8);
                    res |= *pBytes++;
                }
                else // 110? ???? 
                {
                    res = (uint)(*pBytes++ & 0x1f) << 24;
                    res |= (uint)(*pBytes++) << 16;
                    res |= (uint)(*pBytes++) << 8;
                    res |= (uint)(*pBytes++);
                }

                pData = (IntPtr)pBytes;
                return res;
            }
        }

        internal static uint CorSigUncompressData(
            ref IntPtr pData)             // [IN,OUT] compressed data 
        {
            unsafe
            {
                byte* pBytes = (byte*)pData;

                // Handle smallest data inline. 
                if ((*pBytes & 0x80) == 0x00)        // 0??? ????    
                {
                    uint retval = (uint)(*pBytes++);
                    pData = (IntPtr)pBytes;
                    return retval;
                }
                return CorSigUncompressBigData(ref pData);
            }
        }

        // Function translated directly from cor.h but never tested; included here in case someone wants to use it in future
        /*        internal static uint CorSigUncompressData(      // return number of bytes of that compressed data occupied in pData 
                    IntPtr pData,              // [IN] compressed data 
                    out uint pDataOut)              // [OUT] the expanded *pData    
                {   
                    unsafe
                    {
                        uint       cb = 0xffffffff;    
                        byte *pBytes = (byte*)(pData); 
                        pDataOut = 0;

                        // Smallest.    
                        if ((*pBytes & 0x80) == 0x00)       // 0??? ????    
                        {   
                            pDataOut = *pBytes;    
                            cb = 1; 
                        }   
                        // Medium.  
                        else if ((*pBytes & 0xC0) == 0x80)  // 10?? ????    
                        {   
                            pDataOut = (uint)(((*pBytes & 0x3f) << 8 | *(pBytes+1)));  
                            cb = 2; 
                        }   
                        else if ((*pBytes & 0xE0) == 0xC0)      // 110? ????    
                        {   
                            pDataOut = (uint)(((*pBytes & 0x1f) << 24 | *(pBytes+1) << 16 | *(pBytes+2) << 8 | *(pBytes+3)));  
                            cb = 4; 
                        }   
                        return cb;  
                    }
                }*/

        static uint[] g_tkCorEncodeToken = { (uint)MetadataTokenType.TypeDef, (uint)MetadataTokenType.TypeRef, (uint)MetadataTokenType.TypeSpec, (uint)MetadataTokenType.BaseType };

        // uncompress a token
        internal static uint CorSigUncompressToken(   // return the token.    
            ref IntPtr pData)             // [IN,OUT] compressed data 
        {
            uint tk;
            uint tkType;

            tk = CorSigUncompressData(ref pData);
            tkType = g_tkCorEncodeToken[tk & 0x3];
            tk = TokenFromRid(tk >> 2, tkType);
            return tk;
        }


        // Function translated directly from cor.h but never tested; included here in case someone wants to use it in future
        /*        internal static uint CorSigUncompressToken(     // return number of bytes of that compressed data occupied in pData 
                    IntPtr pData,              // [IN] compressed data 
                    out uint     pToken)                // [OUT] the expanded *pData    
                {
                    uint       cb; 
                    uint     tk; 
                    uint     tkType; 

                    cb = CorSigUncompressData(pData, out tk); 
                    tkType = g_tkCorEncodeToken[tk & 0x3];  
                    tk = TokenFromRid(tk >> 2, tkType); 
                    pToken = tk;   
                    return cb;  
                }*/

        internal static CorCallingConvention CorSigUncompressCallingConv(
            ref IntPtr pData)             // [IN,OUT] compressed data 
        {
            unsafe
            {
                byte* pBytes = (byte*)pData;
                CorCallingConvention retval = (CorCallingConvention)(*pBytes++);
                pData = (IntPtr)pBytes;
                return retval;
            }
        }

        // Function translated directly from cor.h but never tested; included here in case someone wants to use it in future
        /*        private enum SignMasks : uint {
                    ONEBYTE  = 0xffffffc0,        // Mask the same size as the missing bits.  
                    TWOBYTE  = 0xffffe000,        // Mask the same size as the missing bits.  
                    FOURBYTE = 0xf0000000,        // Mask the same size as the missing bits.  
                };

                // uncompress a signed integer
                internal static uint CorSigUncompressSignedInt( // return number of bytes of that compressed data occupied in pData
                    IntPtr pData,              // [IN] compressed data 
                    out int         pInt)                  // [OUT] the expanded *pInt 
                {
                    uint       cb; 
                    uint       ulSigned;   
                    uint       iData;  

                    cb = CorSigUncompressData(pData, out iData);
                    pInt = 0;
                    if (cb == 0xffffffff) return cb;
                    ulSigned = iData & 0x1; 
                    iData = iData >> 1; 
                    if (ulSigned != 0)   
                    {   
                        if (cb == 1)    
                        {   
                            iData |= (uint)SignMasks.ONEBYTE; 
                        }   
                        else if (cb == 2)   
                        {   
                            iData |= (uint)SignMasks.TWOBYTE; 
                        }   
                        else    
                        {   
                            iData |= (uint)SignMasks.FOURBYTE;    
                        }   
                    }   
                    pInt = (int)iData;  
                    return cb;  
                }*/


        // uncompress encoded element type
        internal static CorElementType CorSigUncompressElementType(//Element type
            ref IntPtr pData)             // [IN,OUT] compressed data 
        {
            unsafe
            {
                byte* pBytes = (byte*)pData;

                CorElementType retval = (CorElementType)(*pBytes++);
                pData = (IntPtr)pBytes;
                return retval;
            }
        }

        // Function translated directly from cor.h but never tested; included here in case someone wants to use it in future
        /*        internal static uint CorSigUncompressElementType(// return number of bytes of that compressed data occupied in pData
                    IntPtr pData,              // [IN] compressed data 
                    out CorElementType pElementType)       // [OUT] the expanded *pData    
                {  
                    unsafe
                    {
                        byte *pBytes = (byte*)pData;
                        pElementType = (CorElementType)(*pBytes & 0x7f);    
                        return 1;   
                    }
                }*/

        static internal string[] GetGenericArgumentNames(IMetadataImport importer,
                                                int typeOrMethodToken)
        {
            IMetadataImport2 importer2 = (importer as IMetadataImport2);
            if (importer2 == null)
                return new string[0]; // this means we're pre v2.0 debuggees.

            Debug.Assert(importer2 != null);

            string[] genargs = null;

            IntPtr hEnum = IntPtr.Zero;
            try
            {
                int i = 0;
                do
                {
                    uint nOut;
                    int genTypeToken;
                    importer2.EnumGenericParams(ref hEnum, typeOrMethodToken,
                                                out genTypeToken, 1, out nOut);
                    if (genargs == null)
                    {
                        int count;
                        importer.CountEnum(hEnum, out count);
                        genargs = new string[count];
                    }
                    if (nOut == 0)
                        break;

                    Debug.Assert(nOut == 1);
                    if (nOut == 1)
                    {
                        uint genIndex;
                        int genFlags, ptkOwner, ptkKind;
                        ulong genArgNameSize;

                        importer2.GetGenericParamProps(genTypeToken,
                                                       out genIndex,
                                                       out genFlags,
                                                       out ptkOwner,
                                                       out ptkKind,
                                                       null,
                                                       0,
                                                       out genArgNameSize);
                        StringBuilder genArgName = new StringBuilder((int)genArgNameSize);
                        importer2.GetGenericParamProps(genTypeToken,
                                                       out genIndex,
                                                       out genFlags,
                                                       out ptkOwner,
                                                       out ptkKind,
                                                       genArgName,
                                                       (ulong)genArgName.Capacity,
                                                       out genArgNameSize);

                        genargs[i] = genArgName.ToString();
                    }
                    ++i;
                } while (i < genargs.Length);
            }
            finally
            {
                if (hEnum != IntPtr.Zero)
                    importer2.CloseEnum(hEnum);
            }
            return genargs;
        }
    }

} // namspace Microsoft.Debugger.MetadataWrapper
