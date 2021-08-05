using System;

namespace EfiSharp
{
    [Flags]
    public enum EFI_OPEN_PROTOCOL : uint
    {
        BY_HANDLE_PROTOCOL = 0x00000001,
        GET_PROTOCOL = 0x00000002,
        TEST_PROTOCOL = 0x00000004,
        BY_CHILD_CONTROLLER = 0x00000008,
        BY_DRIVER = 0x00000010,
        EXCLUSIVE = 0x00000020
    }
}
