using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Internal.Metadata.NativeFormat;
using Internal.NativeFormat;
using Internal.Runtime;
using Internal.StackTraceMetadata;

namespace Cosmos.Kernel.Core.Runtime
{
    internal static class StackTraceMetadata
    {
        /// <summary>
        /// Indicates whether stack trace metadata support is enabled.
        /// </summary>
        [FeatureSwitchDefinition("Cosmos.Kernel.Core.Runtime.StackTraceMetadata.IsSupported")]
        public static bool IsSupported => AppContext.TryGetSwitch("Cosmos.Kernel.Core.Runtime.StackTraceMetadata.IsSupported", out bool isEnabled) && isEnabled;
        // Cache of per-module resolvers to avoid repeatedly constructing them
        private static PerModuleMethodNameResolverCache? s_resolverCache;

        /// <summary>
        /// Get reference to Exception._stackTraceString field
        /// </summary>
        /// <param name="exception"> The exception instance </param>
        /// <returns></returns>
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_stackTraceString")]
        public static extern ref string? GetStackTraceString(Exception exception);

        [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "GetMethodNameFromStartAddressIfAvailable")]
        private static extern string GetMethodNameFromStartAddressIfAvailable([UnsafeAccessorType("Internal.StackTraceMetadata.StackTraceMetadata, System.Private.StackTraceMetadata")] object obj, IntPtr methodStartAddress, out bool isStackTraceHidden);

        /// <summary>
        /// Try to get method name from its starting address
        /// </summary>
        /// <param name="methodStart">Method starting address</param>
        /// <param name="methodName">Resolved method name</param>
        /// <returns>True if method name was resolved and is not hidden from stack traces</returns>
        /// <remarks>
        /// This method uses stack trace metadata embedded in the binary modules to resolve method names.
        /// If no metadata is available, or if the method is marked as hidden, this method returns false.
        /// </remarks>
        public static bool TryGetMethodNameFromStartAddress(nuint methodStart, out string methodName)
        {
            methodName = GetMethodNameFromStartAddressIfAvailable(methodStart, out bool isHidden);
            return !string.IsNullOrEmpty(methodName) && !isHidden;
        }

        /// <summary>
        /// Get method name from its starting address, if available.
        /// </summary>
        /// <param name="methodStart">Method starting address</param>
        /// <param name="isStackTraceHidden">Output flag indicating if the method is marked as hidden from stack traces</param>
        /// <returns>Resolved method name, or empty string if not available</returns>
        /// <remarks>
        /// This method uses stack trace metadata embedded in the binary modules to resolve method names.
        /// If no metadata is available, this method returns an empty string.
        /// </remarks>
        public static unsafe string GetMethodNameFromStartAddressIfAvailable(nuint methodStart, out bool isStackTraceHidden)
        {
            s_resolverCache ??= new PerModuleMethodNameResolverCache();

            int rva = (int)(methodStart - (nuint)ModuleHelpers.OsModule);
            foreach (var module in ManagedModule.Modules)
            {
                if (module.AsTypeManager()->OsHandle == ModuleHelpers.OsModule)
                {
                    var resolver = s_resolverCache.GetOrCreate(module);

                    if (resolver.TryGetStackTraceData(rva, out var stackTraceData))
                    {
                        // Validate handles before calling FormatMethodName to avoid BadImageFormatException
                        // during exception handling (which would cause recursive exception detection)
                        if (resolver.Reader == null ||
                            stackTraceData.OwningType.IsNil ||
                            stackTraceData.Name.IsNil ||
                            stackTraceData.Signature.IsNil)
                        {
                            isStackTraceHidden = false;
                            return string.Empty;
                        }

                        isStackTraceHidden = stackTraceData.IsHidden;
                        return MethodNameFormatter.FormatMethodName(resolver.Reader, stackTraceData.OwningType, stackTraceData.Name, stackTraceData.Signature, stackTraceData.GenericArguments);
                    }
                }
            }

            isStackTraceHidden = false;
            return string.Empty;
        }

        /// <summary>
        /// Method name resolver for a single binary module
        /// </summary>
        private sealed class PerModuleMethodNameResolver
        {

            /// <summary>
            /// Dictionary mapping method RVA's to tokens within the metadata blob.
            /// </summary>
            private readonly StackTraceData[] _stacktraceDatas;

            /// <summary>
            /// Metadata reader for the stack trace metadata.
            /// </summary>
            public readonly MetadataReader? Reader;

            /// <summary>
            /// Publicly exposed module address property.
            /// </summary>
            public IntPtr ModuleAddress { get; }

            /// <summary>
            /// Construct the per-module resolver by looking up the necessary blobs.
            /// </summary>
            public unsafe PerModuleMethodNameResolver(TypeManagerHandle handle)
            {
                ModuleAddress = handle.AsTypeManager()->OsHandle;

                byte* metadataBlob;
                uint metadataBlobSize;

                byte* rvaToTokenMapBlob;
                uint rvaToTokenMapBlobSize;

                if (ModuleHelpers.RhFindBlob(&handle, (uint)ReflectionMapBlob.EmbeddedMetadata, &metadataBlob, &metadataBlobSize) &&
                    ModuleHelpers.RhFindBlob(&handle, (uint)ReflectionMapBlob.BlobIdStackTraceMethodRvaToTokenMapping, &rvaToTokenMapBlob, &rvaToTokenMapBlobSize))
                {
                    Reader = new MetadataReader(new IntPtr(metadataBlob), (int)metadataBlobSize);

                    int entryCount = *(int*)rvaToTokenMapBlob;
                    _stacktraceDatas = new StackTraceData[entryCount];

                    PopulateRvaToTokenMap(handle, rvaToTokenMapBlob + sizeof(int), rvaToTokenMapBlobSize - sizeof(int));
                }
                else
                {
                    // No stack trace metadata available
                    _stacktraceDatas = Array.Empty<StackTraceData>();
                    Reader = null;
                }
            }

            /// <summary>
            /// Construct the dictionary mapping method RVAs to stack trace metadata tokens
            /// within a single binary module.
            /// </summary>
            /// <param name="handle">Module to use to construct the mapping</param>
            /// <param name="pMap">List of RVA - token pairs</param>
            /// <param name="length">Length of the blob</param>
            private unsafe void PopulateRvaToTokenMap(TypeManagerHandle handle, byte* pMap, uint length)
            {
                Handle currentOwningType = default;
                MethodSignatureHandle currentSignature = default;
                ConstantStringValueHandle currentName = default;
                ConstantStringArrayHandle currentMethodInst = default;

                int current = 0;
                byte* pCurrent = pMap;
                while (pCurrent < pMap + length)
                {
                    byte command = *pCurrent++;

                    if ((command & StackTraceDataCommand.UpdateOwningType) != 0)
                    {
                        currentOwningType = Handle.FromIntToken((int)NativePrimitiveDecoder.ReadUInt32(ref pCurrent));
                    }

                    if ((command & StackTraceDataCommand.UpdateName) != 0)
                    {
                        currentName = new Handle(HandleType.ConstantStringValue, (int)NativePrimitiveDecoder.DecodeUnsigned(ref pCurrent)).ToConstantStringValueHandle(Reader!);
                    }

                    if ((command & StackTraceDataCommand.UpdateSignature) != 0)
                    {
                        currentSignature = new Handle(HandleType.MethodSignature, (int)NativePrimitiveDecoder.DecodeUnsigned(ref pCurrent)).ToMethodSignatureHandle(Reader!);
                        currentMethodInst = default;
                    }

                    if ((command & StackTraceDataCommand.UpdateGenericSignature) != 0)
                    {
                        currentSignature = new Handle(HandleType.MethodSignature, (int)NativePrimitiveDecoder.DecodeUnsigned(ref pCurrent)).ToMethodSignatureHandle(Reader!);
                        currentMethodInst = new Handle(HandleType.ConstantStringArray, (int)NativePrimitiveDecoder.DecodeUnsigned(ref pCurrent)).ToConstantStringArrayHandle(Reader!);
                    }

                    void* pMethod = ReadRelPtr32(pCurrent);
                    pCurrent += sizeof(int);

                    int methodRva = (int)((nint)pMethod - handle.AsTypeManager()->OsHandle);

                    _stacktraceDatas[current++] = new StackTraceData
                    {
                        Rva = methodRva,
                        IsHidden = (command & StackTraceDataCommand.IsStackTraceHidden) != 0,
                        OwningType = currentOwningType,
                        Name = currentName,
                        Signature = currentSignature,
                        GenericArguments = currentMethodInst,
                    };

                    static void* ReadRelPtr32(byte* address)
                        => address + *(int*)address;
                }

                // Use simple insertion sort instead of Array.Sort
                // Array.Sort uses IntroSort which is recursive and causes page faults in kernel
                InsertionSort(_stacktraceDatas, current);
            }

            /// <summary>
            /// Simple insertion sort - iterative, no allocations, safe for kernel use
            /// </summary>
            private static void InsertionSort(StackTraceData[] array, int count)
            {
                for (int i = 1; i < count; i++)
                {
                    var key = array[i];
                    int j = i - 1;

                    while (j >= 0 && array[j].Rva > key.Rva)
                    {
                        array[j + 1] = array[j];
                        j--;
                    }
                    array[j + 1] = key;
                }
            }

            /// <summary>
            /// Try to resolve method name based on its address using the stack trace metadata
            /// </summary>
            public bool TryGetStackTraceData(int rva, out StackTraceData data)
            {
                if (_stacktraceDatas == null)
                {
                    // No stack trace metadata for this module
                    data = default;
                    return false;
                }

                int index = Array.BinarySearch(_stacktraceDatas, new StackTraceData() { Rva = rva });
                if (index < 0)
                {
                    // Method RVA not found in the map
                    data = default;
                    return false;
                }

                data = _stacktraceDatas[index];
                return true;
            }

            public struct StackTraceData : IComparable<StackTraceData>
            {
                private const int IsHiddenFlag = 0x2;

                private readonly int _rvaAndIsHiddenBit;

                public int Rva
                {
                    get => _rvaAndIsHiddenBit & ~IsHiddenFlag;
                    init
                    {
                        _rvaAndIsHiddenBit = value | (_rvaAndIsHiddenBit & IsHiddenFlag);
                    }
                }
                public bool IsHidden
                {
                    get => (_rvaAndIsHiddenBit & IsHiddenFlag) != 0;
                    init
                    {
                        if (value)
                        {
                            _rvaAndIsHiddenBit |= IsHiddenFlag;
                        }
                    }
                }
                public Handle OwningType { get; init; }
                public ConstantStringValueHandle Name { get; init; }
                public MethodSignatureHandle Signature { get; init; }
                public ConstantStringArrayHandle GenericArguments { get; init; }

                public int CompareTo(StackTraceData other) => Rva.CompareTo(other.Rva);
            }
        }

        /// <summary>
        /// Simple cache of PerModuleMethodNameResolver instances keyed by module address.
        /// </summary>
        private sealed class PerModuleMethodNameResolverCache
        {
            private readonly IntPtr[] _keys;
            private readonly PerModuleMethodNameResolver[] _resolvers;

            public PerModuleMethodNameResolverCache()
            {
                int count = ManagedModule.ModuleCount;

                _keys = new IntPtr[count];
                _resolvers = new PerModuleMethodNameResolver[count];
            }

            public unsafe PerModuleMethodNameResolver GetOrCreate(TypeManagerHandle handle)
            {
                nint key = handle.AsTypeManager()->OsHandle;

                // Fast path: find existing
                for (int i = 0; i < _keys.Length; i++)
                {
                    if (_keys[i] == key && key != IntPtr.Zero)
                    {
                        return _resolvers[i];
                    }
                }

                // Try to claim an empty slot
                for (int i = 0; i < _keys.Length; i++)
                {
                    if (_keys[i] == IntPtr.Zero)
                    {
                        _keys[i] = key;
                        var resolver = new PerModuleMethodNameResolver(handle);
                        _resolvers[i] = resolver;
                        return resolver;
                    }
                }

                throw new InvalidOperationException("PerModuleMethodNameResolverCache is full");
            }
        }
    }

    internal static class StackTraceDataCommand
    {
        public const byte UpdateOwningType = 0x01;
        public const byte UpdateName = 0x02;
        public const byte UpdateSignature = 0x04;
        public const byte UpdateGenericSignature = 0x08; // Just a shortcut - sig metadata has the info
        public const byte IsStackTraceHidden = 0x10;
    }
}
