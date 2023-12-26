using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Core.Multiboot.Tags
{
    /// <summary>
    /// Tag AcpiOld
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 28)]
    public unsafe readonly struct AcpiOld
    {
        [FieldOffset(0)]
        public readonly uint Type;
        [FieldOffset(4)]
        public readonly uint Size;
        [FieldOffset(8)]
        public readonly ulong Signature;
        [FieldOffset(16)]
        public readonly byte Checksum;
        [FieldOffset(17)]
        public readonly uint OEMID;
        [FieldOffset(23)]
        public readonly byte Revision;
        [FieldOffset(24)]
        public readonly uint RsdtAddress;
    }
}
