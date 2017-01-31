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
    internal static class MetadataHelper
    {
        private static readonly Dictionary<string, MetadataReaderProvider> mMetadataCache;

        static MetadataHelper()
        {
            mMetadataCache = new Dictionary<string, MetadataReaderProvider>();
        }

        public static MetadataReader TryGetReader(string aAssemblyPath)
        {
            MetadataReaderProvider provider;
            if (mMetadataCache.TryGetValue(aAssemblyPath, out provider))
            {
                return provider.GetMetadataReader();
            }

            provider = TryOpenReaderFromAssemblyFile(aAssemblyPath);

            if (provider == null)
            {
                return null;
            }

            mMetadataCache.Add(aAssemblyPath, provider);

            // The reader has already been open, so this doesn't throw:
            return provider.GetMetadataReader();
        }

        public static Type GetTypeFromReference(MetadataReader reader, Module aModule, TypeReferenceHandle handle, byte rawTypeKind)
        {
            int xToken = MetadataTokens.GetToken(handle);
            return aModule.ResolveType(xToken, null, null);
            TypeReference xReference = reader.GetTypeReference(handle);
            Handle scope = xReference.ResolutionScope;

            string xName = xReference.Namespace.IsNil
                ? reader.GetString(xReference.Name)
                : reader.GetString(xReference.Namespace) + "." + reader.GetString(xReference.Name);

            var xType = Type.GetType(xName);
            if (xType != null)
            {
                return xType;
            }

            try
            {
                xType = aModule.ResolveType(MetadataTokens.GetToken(handle), null, null);
                return xType;
            }
            catch
            {
                switch (scope.Kind)
                {
                    case HandleKind.ModuleReference:
                        string xModule = "[.module  " + reader.GetString(reader.GetModuleReference((ModuleReferenceHandle)scope).Name) + "]" + xName;
                        return null;
                    case HandleKind.AssemblyReference:
                        var assemblyReferenceHandle = (AssemblyReferenceHandle)scope;
                        var assemblyReference = reader.GetAssemblyReference(assemblyReferenceHandle);
                        string xAssembly = "[" + reader.GetString(assemblyReference.Name) + "]" + xName;
                        return null;
                    case HandleKind.TypeReference:
                        return GetTypeFromReference(reader, aModule, (TypeReferenceHandle)scope, 0);
                    default:
                        // rare cases:  ModuleDefinition means search within defs of current module (used by WinMDs for projections)
                        //              nil means search exported types of same module (haven't seen this in practice). For the test
                        //              purposes here, it's sufficient to format both like defs.
                        return null;
                }
            }
        }

        private static PEReader TryGetPEReader(string aAssemblyPath)
        {
            var peStream = TryOpenFile(aAssemblyPath);
            if (peStream != null)
            {
                return new PEReader(peStream);
            }

            return null;
        }

        private static MetadataReaderProvider TryOpenReaderFromAssemblyFile(string aAssemblyPath)
        {
            using (var peReader = TryGetPEReader(aAssemblyPath))
            {
                if (peReader == null)
                {
                    return null;
                }

                string pdbPath;
                MetadataReaderProvider provider;
                if (peReader.TryOpenAssociatedPortablePdb(aAssemblyPath, TryOpenFile, out provider, out pdbPath))
                {
                    return provider;
                }
            }

            return null;
        }

        private static Stream TryOpenFile(string aPath)
        {
            if (!File.Exists(aPath))
            {
                return null;
            }

            try
            {
                return File.OpenRead(aPath);
            }
            catch
            {
                return null;
            }
        }
    }

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

        public static DebugInfo.SequencePoint[] GetSequencePoints(string aAssemblyPath, int aMetadataToken)
        {
            var xSequencePoints = new List<DebugInfo.SequencePoint>();
            try
            {
                var xReader = MetadataHelper.TryGetReader(aAssemblyPath);
                if (xReader == null)
                {
                    return xSequencePoints.ToArray();
                }

                var xMethodDebugInfoHandle = MetadataTokens.MethodDebugInformationHandle(aMetadataToken);
                if (!xMethodDebugInfoHandle.IsNil)
                {
                    var xDebugInfo = xReader.GetMethodDebugInformation(xMethodDebugInfoHandle);
                    var xDebugInfoSequencePoints = xDebugInfo.GetSequencePoints();
                    foreach (var xSequencePoint in xDebugInfoSequencePoints)
                    {
                        string xDocumentName = string.Empty;
                        if (!xSequencePoint.Document.IsNil)
                        {
                            var xDocument = xReader.GetDocument(xSequencePoint.Document);
                            if (!xDocument.Name.IsNil)
                            {
                                xDocumentName = xReader.GetString(xDocument.Name);
                            }
                        }

                        xSequencePoints.Add(new DebugInfo.SequencePoint
                        {
                            Document = xDocumentName,
                            ColStart = xSequencePoint.StartColumn,
                            ColEnd = xSequencePoint.EndColumn,
                            LineStart = xSequencePoint.StartLine,
                            LineEnd = xSequencePoint.EndLine,
                            Offset = xSequencePoint.Offset
                        });
                    }

                }
            }
            catch(Exception ex)
            {

            }

            return xSequencePoints.ToArray();
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
            string xLocation = aMethodBase.Module.Assembly.Location;
            var xGenericMethodParameters = new Type[0];
            var xGenericTypeParameters = new Type[0];
            if (aMethodBase.IsGenericMethod)
            {
                xGenericMethodParameters = aMethodBase.GetGenericArguments();
            }
            if (aMethodBase.DeclaringType.GetTypeInfo().IsGenericType)
            {
                xGenericTypeParameters = aMethodBase.DeclaringType.GetTypeInfo().GetGenericArguments();
            }

            // TODO: Read from pdb.
            //var xReader = MetadataHelper.TryGetReader(xLocation);
            //if (xReader != null)
            //{
            //    xLocalVariables = ResolveLocalsFromPdb(xReader, aMethodBase, xGenericTypeParameters, xGenericMethodParameters).ToList();
            //}
            //else
            //{
            var xReader = GetReader(xLocation).mMetadataReader;
            xLocalVariables = ResolveLocalsFromSignature(xReader, aMethodBase, xGenericTypeParameters, xGenericMethodParameters).ToList();
            //}
#else
            var xLocals = aMethodBase.GetMethodBody().LocalVariables;
            foreach (var xLocal in xLocals)
            {
                xLocalVariables.Add(xLocal.LocalType);
            }
#endif
            return xLocalVariables;
        }

        private static IList<Type> ResolveLocalsFromPdb(MetadataReader aReader, MethodBase aMethodBase, Type[] aGenericTypeParameters, Type[] aGenericMethodParameters)
        {
            var xLocalVariables = new List<Type>();

            // TODO

            return xLocalVariables;
        }

        private static IList<Type> ResolveLocalsFromSignature(MetadataReader aReader, MethodBase aMethodBase, Type[] aGenericTypeParameters, Type[] aGenericMethodParameters)
        {
            var xLocalVariables = new List<Type>();
            var xMethodBody = GetMethodBodyBlock(aMethodBase.Module, aMethodBase.MetadataToken);
            if (xMethodBody != null && !xMethodBody.LocalSignature.IsNil)
            {
                var xSig = aReader.GetStandaloneSignature(xMethodBody.LocalSignature);
                var xLocals = xSig.DecodeLocalSignature(new LocalTypeProvider(aMethodBase.Module), new LocalTypeGenericContext(aGenericTypeParameters.ToImmutableArray(), aGenericMethodParameters.ToImmutableArray()));
                foreach (var xLocal in xLocals)
                {
                    xLocalVariables.Add(xLocal);
                }
            }
            return xLocalVariables;
        }

        public static Type GetCatchType(Module aModule, ExceptionRegion aRegion)
        {
            string xLocation = aModule.Assembly.Location;
            var xReader = MetadataHelper.TryGetReader(xLocation);
            switch (aRegion.CatchType.Kind)
            {
                case HandleKind.TypeReference:
                    return MetadataHelper.GetTypeFromReference(xReader, aModule, (TypeReferenceHandle) aRegion.CatchType, 0);
                    break;
                case HandleKind.TypeDefinition:
                    break;
                case HandleKind.FieldDefinition:
                    break;
                case HandleKind.MethodDefinition:
                    break;
                case HandleKind.Parameter:
                    break;
                case HandleKind.InterfaceImplementation:
                    break;
                case HandleKind.MemberReference:
                    break;
                case HandleKind.Constant:
                    break;
                case HandleKind.CustomAttribute:
                    break;
                case HandleKind.DeclarativeSecurityAttribute:
                    break;
                case HandleKind.StandaloneSignature:
                    break;
                case HandleKind.EventDefinition:
                    break;
                case HandleKind.PropertyDefinition:
                    break;
                case HandleKind.MethodImplementation:
                    break;
                case HandleKind.ModuleReference:
                    break;
                case HandleKind.TypeSpecification:
                    break;
                case HandleKind.AssemblyDefinition:
                    break;
                case HandleKind.AssemblyFile:
                    break;
                case HandleKind.AssemblyReference:
                    break;
                case HandleKind.ExportedType:
                    break;
                case HandleKind.GenericParameter:
                    break;
                case HandleKind.MethodSpecification:
                    break;
                case HandleKind.GenericParameterConstraint:
                    break;
                case HandleKind.MethodDebugInformation:
                    break;
                case HandleKind.CustomDebugInformation:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            throw new NotImplementedException();
        }
    }
}
