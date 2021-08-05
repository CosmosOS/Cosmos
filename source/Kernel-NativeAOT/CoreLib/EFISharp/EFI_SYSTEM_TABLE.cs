using System.Runtime.InteropServices;

namespace EfiSharp
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct EFI_SYSTEM_TABLE
    {
        internal readonly EFI_TABLE_HEADER Hdr;
        private readonly char* FirmwareVendor;
        private readonly uint FirmwareRevision;
        internal readonly EFI_HANDLE ConsoleInHandle;
        //EFI_SIMPLE_TEXT_INPUT_PROTOCOL
        private readonly void* _pad1;
        private readonly EFI_HANDLE ConsoleOutHandle;
        internal readonly EFI_SIMPLE_TEXT_OUTPUT_PROTOCOL* ConOut;
        private readonly void* _pad2;
        private readonly void* _pad3;
        private readonly void* _pad4;
        public readonly EFI_BOOT_SERVICES* BootServices;
    }
}
