using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using Cosmos.Debug.Symbols.Metadata;
using Cosmos.Debug.Symbols.Pdb;

namespace Cosmos.Debug.Symbols
{
    public class DebugSymbolReader
    {
        private static string mCurrentFile;
        private static DebugSymbolReader mCurrentDebugSymbolReader;

        private readonly PEReader mPEReader;
        private readonly MetadataReader mMetadataReader;
        private readonly PdbSymbolReader mSymbolReader;
        private static readonly Dictionary<string, List<ILLocalVariable>> xLocalVariableInfosCache = new Dictionary<string, List<ILLocalVariable>>();

        private DebugSymbolReader(string aFilePath)
        {
            mPEReader = new PEReader(File.OpenRead(aFilePath), PEStreamOptions.PrefetchEntireImage);
            mSymbolReader = OpenAssociatedSymbolFile(aFilePath, mPEReader);
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

        private PdbSymbolReader OpenAssociatedSymbolFile(string peFilePath, PEReader peReader)
        {
            // Assume that the .pdb file is next to the binary
            var pdbFilename = Path.ChangeExtension(peFilePath, ".pdb");
            string searchPath = "";

            if (!File.Exists(pdbFilename))
            {
                pdbFilename = null;

                // If the file doesn't exist, try the path specified in the CodeView section of the image
                foreach (DebugDirectoryEntry debugEntry in peReader.ReadDebugDirectory())
                {
                    if (debugEntry.Type != DebugDirectoryEntryType.CodeView)
                    {
                        continue;
                    }

                    string candidateFileName = peReader.ReadCodeViewDebugDirectoryData(debugEntry).Path;
                    if (Path.IsPathRooted(candidateFileName) && File.Exists(candidateFileName))
                    {
                        pdbFilename = candidateFileName;
                        searchPath = Path.GetDirectoryName(pdbFilename);
                        break;
                    }
                }

                if (pdbFilename == null)
                {
                    return null;
                }
            }

            // Try to open the symbol file as portable pdb first
            PdbSymbolReader reader = PortablePdbSymbolReader.TryOpen(pdbFilename, MetadataHelper.GetMetadataStringDecoder());
            if (reader == null)
            {
                // Fallback to the diasymreader for non-portable pdbs
                reader = UnmanagedPdbSymbolReader.TryOpenSymbolReaderForMetadataFile(peFilePath, searchPath);
            }

            return reader;
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
            catch (Exception ex)
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

        public static List<ILLocalVariable> GetLocalVariableInfos(MethodBase aMethodBase)
        {
            string xMenthodId = $"{aMethodBase.MetadataToken}_{aMethodBase.DeclaringType?.FullName}_{aMethodBase.Name}";

            if (xLocalVariableInfosCache.ContainsKey(xMenthodId))
            {
                return xLocalVariableInfosCache[xMenthodId];
            }
            var xLocalVariables = new List<ILLocalVariable>();

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

            var xReader = GetReader(xLocation).mMetadataReader;
            List<ILLocalVariable> xLocalVariablesFromPdb = null;
            try
            {
                xLocalVariablesFromPdb = mCurrentDebugSymbolReader.mSymbolReader.GetLocalVariableNamesForMethod(aMethodBase.MetadataToken).ToList();
            }
            catch (Exception)
            {
            }

            var xTypes = ResolveLocalsFromSignature(xReader, aMethodBase, xGenericTypeParameters, xGenericMethodParameters).ToList();
            for (int i = 0; i < xTypes.Count; i++)
            {
                int xSlot = i;
                string xName = "Local" + i;
                bool xCompilerGenerated = true;
                ILLocalVariable xLocal = xLocalVariablesFromPdb?.FirstOrDefault(x => x.Slot == i);
                if (xLocal != null)
                {
                    xName = xLocal.Name;
                    xSlot = xLocal.Slot;
                    xCompilerGenerated = xLocal.CompilerGenerated;
                }
                xLocalVariables.Add(new ILLocalVariable(xSlot, xName, xCompilerGenerated, xTypes[i]));
            }

            xLocalVariableInfosCache.Add(xMenthodId, xLocalVariables);
            return xLocalVariables;
        }

        private static List<Type> ResolveLocalsFromSignature(MetadataReader aReader, MethodBase aMethodBase, Type[] aGenericTypeParameters, Type[] aGenericMethodParameters)
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
                    return MetadataHelper.GetTypeFromReference(xReader, aModule,
                        (TypeReferenceHandle)aRegion.CatchType, 0);
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

        public static bool TryGetStaticFieldValue(Module aModule, int aMetadataToken, ref byte[] aBuffer)
        {
            var xAssemblyPath = aModule.Assembly.Location;
            var xMetadataReader = GetReader(xAssemblyPath).mMetadataReader;
            var xPEReader = GetReader(xAssemblyPath).mPEReader;

            var xHandle = (FieldDefinitionHandle)MetadataTokens.Handle(aMetadataToken);

            if (!xHandle.IsNil)
            {
                var xFieldDefinition = xMetadataReader.GetFieldDefinition(xHandle);
                var xRVA = xFieldDefinition.GetRelativeVirtualAddress();

                if (xFieldDefinition.Attributes.HasFlag(FieldAttributes.HasFieldRVA))
                {
                    var xBytes = xPEReader.GetSectionData(xRVA).GetContent();

                    for (int i = 0; i < aBuffer.Length; i++)
                    {
                        aBuffer[i] = xBytes[i];
                    }
                }
            }

            return false;
        }
    }
}
