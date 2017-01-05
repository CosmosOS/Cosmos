using System.IO;
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
            mPEReader = new PEReader(File.Open(aFilePath, FileMode.Open));
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
            var xHandle = (MethodDefinitionHandle)MetadataTokens.Handle(aMethodToken);

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
                return mMetadataReader.GetBlobReader(xDocument.Name).ReadSerializedString();
            }

            return "";
        }

        public SequencePointCollection GetSequencePoints(int aMethodToken)
        {
            var xDebugInformation = GetMethodDebugInformation(aMethodToken);

            return xDebugInformation.GetSequencePoints();
        }
    }
}
