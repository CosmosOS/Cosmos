using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;

namespace Cosmos.Debug.Symbols
{
    public class DebugSymbolReader
    {
        private static string mCurrentFile;
        private static DebugSymbolReader mCurrentDebugSymbolReader;

        private readonly PEReader mPEReader;
        private readonly MetadataReader mMetadataReader;

        private DebugSymbolReader(string aFilePath)
        {
            mPEReader = new PEReader(File.OpenRead(aFilePath), PEStreamOptions.PrefetchEntireImage);
            mMetadataReader = mPEReader.GetMetadataReader();
        }

        internal static DebugSymbolReader GetReader(string aFilePath)
        {
            if (File.Exists(aFilePath))
            {
                if (mCurrentDebugSymbolReader != null && mCurrentFile == aFilePath)
                {
                    return mCurrentDebugSymbolReader;
                }

                mCurrentDebugSymbolReader = new DebugSymbolReader(aFilePath);

                if (mCurrentDebugSymbolReader.mPEReader.HasMetadata)
                {
                    mCurrentFile = aFilePath;

                    return mCurrentDebugSymbolReader;
                }
            }

            return null;
        }

        private string ResolveEntity(EntityHandle aEntityHandle)
        {
            switch (aEntityHandle.Kind)
            {
                case HandleKind.AssemblyReference:
                    var xAssemblyRef = mMetadataReader.GetAssemblyReference((AssemblyReferenceHandle)aEntityHandle);
                    return ResolveAssemblyReference(xAssemblyRef);
                case HandleKind.FieldDefinition:
                    var xFieldDef = mMetadataReader.GetFieldDefinition((FieldDefinitionHandle)aEntityHandle);
                    return ResolveFieldDefinition(xFieldDef);
                case HandleKind.MethodDefinition:
                    var xMethodDef = mMetadataReader.GetMethodDefinition((MethodDefinitionHandle)aEntityHandle);
                    return ResolveMethodDefinition(xMethodDef);
                case HandleKind.MemberReference:
                    var xMemberRef = mMetadataReader.GetMemberReference((MemberReferenceHandle)aEntityHandle);
                    return ResolveMemberReference(xMemberRef);
                case HandleKind.TypeReference:
                    var xTypeRef = mMetadataReader.GetTypeReference((TypeReferenceHandle)aEntityHandle);
                    return ResolveTypeReference(xTypeRef);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static string ResolveString(Module aModule, int aMetadataToken)
        {
            var xHandle = MetadataTokens.Handle(aMetadataToken);
            if (!xHandle.IsNil)
            {
                string xLocation = aModule.Assembly.Location;
                var xReader = GetReader(xLocation);
                var xOffset = xReader.mMetadataReader.GetHeapOffset(xHandle);
                var xStringHandle = MetadataTokens.UserStringHandle(xOffset);

                if (!xStringHandle.IsNil)
                {
                    return xReader.mMetadataReader.GetUserString(xStringHandle);
                }
            }

            return null;
        }

        private string ResolveAssemblyReference(AssemblyReference aMemberRef)
        {
            string xFullTypeName = string.Empty;
            if (!aMemberRef.Name.IsNil)
            {
                xFullTypeName = mMetadataReader.GetString(aMemberRef.Name);
            }
            return xFullTypeName;
        }

        private string ResolveMemberReference(MemberReference aMemberRef)
        {
            string xFullTypeName = string.Empty;
            if (!aMemberRef.Parent.IsNil)
            {
                xFullTypeName = ResolveEntity(aMemberRef.Parent);
            }
            return xFullTypeName;
        }

        private string ResolveTypeReference(TypeReference aTypeRef)
        {
            string xFullTypeName = string.Empty;
            if (!aTypeRef.ResolutionScope.IsNil)
            {
                xFullTypeName = ResolveEntity(aTypeRef.ResolutionScope);
            }
            if (!aTypeRef.Name.IsNil)
            {
                xFullTypeName = xFullTypeName + "." + mMetadataReader.GetString(aTypeRef.Name);
            }
            return xFullTypeName;
        }

        private string ResolveFieldDefinition(FieldDefinition aFieldDef)
        {
            string xFullTypeName = string.Empty;
            var xTypeDefHandle = aFieldDef.GetDeclaringType();
            if (!xTypeDefHandle.IsNil)
            {
                var xTypeDef = mMetadataReader.GetTypeDefinition(xTypeDefHandle);
                xFullTypeName = ResolveTypeDefinition(xTypeDef);
            }
            return xFullTypeName;
        }

        private string ResolveMethodDefinition(MethodDefinition aMethodDef)
        {
            string xFullTypeName = string.Empty;
            var xTypeDefHandle = aMethodDef.GetDeclaringType();
            if (!xTypeDefHandle.IsNil)
            {
                var xTypeDef = mMetadataReader.GetTypeDefinition(xTypeDefHandle);
                xFullTypeName = ResolveTypeDefinition(xTypeDef);
            }
            return xFullTypeName;
        }

        private string ResolveTypeDefinition(TypeDefinition aTypeDef)
        {
            string xFullTypeName = string.Empty;
            var xNSDefHandle = aTypeDef.NamespaceDefinition;
            if (!xNSDefHandle.IsNil)
            {
                var xNSDef = mMetadataReader.GetNamespaceDefinition(xNSDefHandle);
                xFullTypeName = ResolveNamespaceDefinition(xNSDef);
            }
            xFullTypeName = xFullTypeName + "." + mMetadataReader.GetString(aTypeDef.Name);
            xFullTypeName = xFullTypeName.Trim('.');
            return xFullTypeName;
        }

        private string ResolveNamespaceDefinition(NamespaceDefinition aNamespaceDef)
        {
            string xName = string.Empty;
            if (!aNamespaceDef.Parent.IsNil)
            {
                var xParent = mMetadataReader.GetNamespaceDefinition(aNamespaceDef.Parent);
                xName = ResolveNamespaceDefinition(xParent);
            }
            xName = xName + "." + mMetadataReader.GetString(aNamespaceDef.Name);
            return xName;
        }

        public MethodDebugInformation GetMethodDebugInformation(int aMetadataToken)
        {
            var xHandle = MetadataTokens.MethodDebugInformationHandle(aMetadataToken);

            if (!xHandle.IsNil)
            {
                return mMetadataReader.GetMethodDebugInformation(xHandle);
            }

            return new MethodDebugInformation();
        }

        public string GetDocumentPath(DocumentHandle aHandle)
        {
            var xDocument = mMetadataReader.GetDocument(aHandle);

            if (!xDocument.Name.IsNil)
            {
                return mMetadataReader.GetString(xDocument.Name);
            }

            return "";
        }

        public static MethodBodyBlock GetMethodBodyBlock(Module aModule, int aMetadataToken)
        {
            var xMethodDefHandle = MetadataTokens.MethodDefinitionHandle(aMetadataToken);
            if (!xMethodDefHandle.IsNil)
            {
                string xLocation = aModule.Assembly.Location;
                var xReader = GetReader(xLocation);
                var xMethodDefinition = xReader.mMetadataReader.GetMethodDefinition(xMethodDefHandle);
                if (xMethodDefinition.RelativeVirtualAddress > 0)
                {
                    int xRelativeVirtualAddress = xMethodDefinition.RelativeVirtualAddress;
                    return xReader.mPEReader.GetMethodBody(xRelativeVirtualAddress);
                }
            }
            return null;
        }

        public static IList<Type> GetLocalVariableInfos(MethodBase aMethodBase)
        {
            var xLocalVariables = new List<Type>();
#if NETSTANDARD1_6
            var xGenericArgs = new Type[0];
            if (aMethodBase.IsGenericMethod)
            {
                xGenericArgs = aMethodBase.GetGenericArguments();
            }
            var xMethodBody = GetMethodBodyBlock(aMethodBase.Module, aMethodBase.MetadataToken);
            if (!xMethodBody.LocalSignature.IsNil)
            {
                string xLocation = aMethodBase.Module.Assembly.Location;
                var xReader = GetReader(xLocation);
                var xSig = xReader.mMetadataReader.GetStandaloneSignature(xMethodBody.LocalSignature);
                var xLocals = xSig.DecodeLocalSignature(new LocalTypeProvider(aMethodBase.Module), new LocalTypeGenericContext(new ImmutableArray<Type>(), xGenericArgs.ToImmutableArray()));
                foreach (var xLocal in xLocals)
                {
                    xLocalVariables.Add(xLocal);
                }
            }
#else
            var xLocals = aMethodBase.GetMethodBody().LocalVariables;
            foreach (var xLocal in xLocals)
            {
                xLocalVariables.Add(xLocal.LocalType);
            }
#endif
            return xLocalVariables;
        }
    }
}
