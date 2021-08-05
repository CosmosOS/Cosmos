using System.Runtime.InteropServices;

namespace EfiSharp
{
    //TODO Replace with EFI_HANDLE? Are they the same?
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct EFIKeyNotifyHandle
    {
        private readonly void* Handle;
    }
}