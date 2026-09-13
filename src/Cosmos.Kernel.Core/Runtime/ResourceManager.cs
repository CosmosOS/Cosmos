using System.Runtime.InteropServices;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Utilities;
using Internal.NativeFormat;
using Internal.Runtime;

namespace Cosmos.Kernel.Core.Runtime;

internal static class ResourceManager
{
    public static unsafe UnmanagedMemoryStream? GetResourceStream(string resourceName)
    {
        if (TryGetResouceBlob(resourceName, out byte* pBlob, out int length))
        {
            return new(pBlob, length);
        }
        return null;
    }

    public static unsafe ReadOnlySpan<byte> GetResourceAsSpan(string resourceName)
    {
        if (TryGetResouceBlob(resourceName, out byte* pBlob, out int length))
        {
            return new(pBlob, length);
        }

        return [];
    }

    private static unsafe bool TryGetResouceBlob(string resourceName, out byte* pBlob, out int length)
    {
        uint blobLength = 0;

        if (Resources.TryGetValue(resourceName, out ResourceInfo resourceInfo))
        {
            TypeManagerHandle handle = ManagedModule.s_modules[resourceInfo.ModuleIndex];
            fixed (byte** ppBlob = &pBlob)
            {
                if (!ModuleHelpers.RhFindBlob(&handle, (uint)ReflectionMapBlob.BlobIdResourceData, ppBlob, &blobLength))
                {
                    throw new BadImageFormatException();
                }
            }

            pBlob += resourceInfo.Index;
            length = resourceInfo.Length;
            return true;
        }

        pBlob = (byte*)IntPtr.Zero;
        length = 0;
        return false;
    }

    //private static SimpleDictionary<string, ResourceInfo>? s_resourceInfos;
    private static SimpleDictionary<string, ResourceInfo> Resources => GetResources();

    private static SimpleDictionary<string, ResourceInfo> GetResources()
    {
        SimpleDictionary<string, ResourceInfo> s_resourceInfos = new();

        for (int i = 0; i < ManagedModule.s_moduleCount; i++)
        {
            TypeManagerHandle module = ManagedModule.s_modules[i];
            if (!TryGetNativeReaderForBlob(module, ReflectionMapBlob.BlobIdResourceIndex, out NativeReader reader))
            {
                throw new BadImageFormatException();
            }

            NativeParser indexParser = new(reader, 0);
            NativeHashtable indexHashTable = new(indexParser);

            NativeHashtable.AllEntriesEnumerator entryEnumerator = indexHashTable.EnumerateAllEntries();

            NativeParser entryParser;
            while (!(entryParser = entryEnumerator.GetNext()).IsNull)
            {
                //TODO: Replace this for the commented blocks once UTF8 Encoding works
                entryParser.Offset = DecodeString(entryParser.Reader, entryParser.Offset, out _ /* Assembly Name */);
                //string assemblyName = entryParser.GetString();
                entryParser.Offset = DecodeString(entryParser.Reader, entryParser.Offset, out string resourceName);
                //string resourceName = entryParser.GetString();
                int resourceOffset = (int)entryParser.GetUnsigned();
                int resourceLength = (int)entryParser.GetUnsigned();
                ResourceInfo resourceInfo = new(resourceName, resourceOffset, resourceLength, i);
                s_resourceInfos[resourceName] = resourceInfo;
                Serial.WriteString($"Found resource: {resourceName}\n");
            }
        }

        return s_resourceInfos;
    }

    private static unsafe bool TryGetNativeReaderForBlob(TypeManagerHandle module, ReflectionMapBlob blob, out NativeReader reader)
    {
        byte* pBlob;
        uint cbBlob;

        if (ModuleHelpers.RhFindBlob(&module, (uint)blob, &pBlob, &cbBlob))
        {
            reader = new NativeReader(pBlob, cbBlob);
            return true;
        }

        reader = default!;
        return false;
    }

    internal static unsafe uint DecodeString(NativeReader aThis, uint offset, out string value)
    {
        offset = aThis.DecodeUnsigned(offset, out uint numBytes);

        if (numBytes == 0)
        {
            value = string.Empty;
            return offset;
        }

        uint endOffset = offset + numBytes;
        if (endOffset < numBytes || endOffset > aThis.Size)
        {
            throw new BadImageFormatException();
        }

        value = Utf8Decode(new((byte*)aThis.OffsetToAddress(offset), (int)numBytes));

        return endOffset;
    }

    internal static unsafe string Utf8Decode(ReadOnlySpan<byte> bytes)
    {
        // WORKAROUND: Utf8.ToUtf16() has a NativeAOT ARM64 codegen bug with infinite loop
        // Use simple ASCII-only conversion instead (resource names are ASCII-only)
        Span<char> buffer = stackalloc char[bytes.Length];

        fixed (byte* pBytes = bytes)
        fixed (char* pChars = buffer)
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                byte b = pBytes[i];
                if (b > 127)
                {
                    // Non-ASCII - should not happen for resource names
                    return string.Empty;
                }
                pChars[i] = (char)b;
            }
        }

        return new string(buffer);
    }

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct ResourceInfo(string Name, int Index, int Length, int ModuleIndex)
    {
        public readonly string Name = Name;
        public readonly int Index = Index;
        public readonly int Length = Length;
        public readonly int ModuleIndex = ModuleIndex;
    }
}
