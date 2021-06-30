using System;
using System.Runtime.InteropServices;

namespace Loader
{
    [StructLayout(LayoutKind.Sequential)]
    public struct EFI_HANDLE
    {
        private IntPtr _handle;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe readonly struct EFI_SIMPLE_TEXT_OUTPUT_PROTOCOL
    {
        private readonly IntPtr _pad;

        public readonly delegate* unmanaged<void*, char*, void*> OutputString;
    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly struct EFI_TABLE_HEADER
    {
        public readonly ulong Signature;
        public readonly uint Revision;
        public readonly uint HeaderSize;
        public readonly uint Crc32;
        public readonly uint Reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    unsafe public readonly struct EFI_SYSTEM_TABLE
    {
        public readonly EFI_TABLE_HEADER Hdr;
        public readonly char* FirmwareVendor;
        public readonly uint FirmwareRevision;
        public readonly EFI_HANDLE ConsoleInHandle;
        public readonly void* ConIn;
        public readonly EFI_HANDLE ConsoleOutHandle;
        public readonly EFI_SIMPLE_TEXT_OUTPUT_PROTOCOL* ConOut;
    }

    public unsafe class Program
    {
        static void Main() { }

        public static EFI_SYSTEM_TABLE* efitable;

        [System.Runtime.RuntimeExport("EfiMain")]
        public static long EfiMain(IntPtr imageHandle, EFI_SYSTEM_TABLE* systemTable)
        {
            efitable = systemTable;

            Write("Welcome to Cosmos NativeAOT!");

            return 0;
        }

        public static void Write(string str)
        {
            fixed (char* pHello = str)
            {
                efitable->ConOut->OutputString(efitable->ConOut, pHello);
            }
        }
    }
}
