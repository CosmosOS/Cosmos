using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;

namespace Cosmos.Debug.Symbols
{
    public class DebugSymbolReader
    {
        private static string mCurrentFile;
        private static DebugSymbolReader mCurrentDebugSymbolReader;

        private PEReader mPEReader;
        private MetadataReader mMetadataReader;

        private DebugSymbolReader(string aFilePath)
        {
            mPEReader = new PEReader(File.OpenRead(aFilePath));
            mMetadataReader = mPEReader.GetMetadataReader();
        }

        public static DebugSymbolReader GetReader(string aFilePath)
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

        public MethodDebugInformation GetMethodDebugInformation(int aMethodToken)
        {
            var xHandle = (MethodDebugInformationHandle)MetadataTokens.Handle(aMethodToken);

            if (!xHandle.IsNil)
            {
                return mMetadataReader.GetMethodDebugInformation(xHandle);
            }

            return new MethodDebugInformation();
        }

        public MethodDefinition GetMethodDefinition(int aMethodToken)
        {
            var xHandle = (MethodDefinitionHandle)MetadataTokens.Handle(aMethodToken);

            if (!xHandle.IsNil)
            {
                return mMetadataReader.GetMethodDefinition(xHandle);
            }

            return new MethodDefinition();
        }

        public MethodImplementation GetMethodImplementation(int aMethodToken)
        {
            var xHandle = (MethodImplementationHandle)MetadataTokens.Handle(aMethodToken);

            if (!xHandle.IsNil)
            {
                return mMetadataReader.GetMethodImplementation(xHandle);
            }

            return new MethodImplementation();
        }

        public MethodSpecification GetMethodSpecification(int aMethodToken)
        {
            var xHandle = (MethodSpecificationHandle)MetadataTokens.Handle(aMethodToken);

            if (!xHandle.IsNil)
            {
                return mMetadataReader.GetMethodSpecification(xHandle);
            }

            return new MethodSpecification();
        }

        public ModuleDefinition GetModuleDefintition()
        {
            return mMetadataReader.GetModuleDefinition();
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

        public MethodBodyBlock GetMethodBodyBlock(int aMethodToken)
        {
            var xMethodDefinition = GetMethodDefinition(aMethodToken);
            var xRelativeVirtualAddress = xMethodDefinition.RelativeVirtualAddress;

            return mPEReader.GetMethodBody(xRelativeVirtualAddress);
        }

        public SequencePointCollection GetSequencePoints(int aMethodToken)
        {
            var xDebugInformation = GetMethodDebugInformation(aMethodToken);

            return xDebugInformation.GetSequencePoints();
        }

        public IList<LocalVariable> GetLocalVariables(int aMethodToken)
        {
            var xLocalVariables = new List<LocalVariable>();
            var xMethodDefinitionHandle = (MethodDefinitionHandle)MetadataTokens.Handle(aMethodToken);

            foreach (var xLocalScopeHandle in mMetadataReader.GetLocalScopes(xMethodDefinitionHandle))
            {
                var xLocalScope = mMetadataReader.GetLocalScope(xLocalScopeHandle);

                foreach (var xLocalVariableHandle in xLocalScope.GetLocalVariables())
                {
                    xLocalVariables.Add(mMetadataReader.GetLocalVariable(xLocalVariableHandle));
                }
            }

            return xLocalVariables;
        }

        public string GetLocalVariableName(int aMethodToken, int aIndex)
        {
            var xLocalVariables = GetLocalVariables(aMethodToken);

            return mMetadataReader.GetString(xLocalVariables[aIndex].Name);
        }

        public IList<LocalVariableInfo> GetLocalVariablesInfo(MethodBodyBlock aMethodBodyBlock)
        {
            throw new Exception("NetCore Fix Me");
        }

        public string GetString(int aMetadataToken)
        {
            var xHandle = MetadataTokens.Handle(aMetadataToken);

            if (!xHandle.IsNil)
            {
                var xOffset = mMetadataReader.GetHeapOffset(xHandle);
                var xStringHandle = MetadataTokens.UserStringHandle(xOffset);

                if (!xStringHandle.IsNil)
                {
                    return mMetadataReader.GetUserString(xStringHandle);
                }
            }

            return null;
        }
    }
}
