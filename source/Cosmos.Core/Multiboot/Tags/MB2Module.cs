using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Core.Multiboot.Tags
{
    /// <summary>
    /// Multiboot2 Module tag
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 44)]
    public unsafe readonly struct MB2Module
    {
        /// <summary>
        /// Information about the tag.
        /// </summary>
        [FieldOffset(0)]
        public readonly MB2Tag Info;

        /// <summary>
        /// Starting address of the module.
        /// </summary>
        [FieldOffset(8)]
        public readonly uint ModuleStartAddress;

        /// <summary>
        /// Ending address of the module.
        /// </summary>
        [FieldOffset(16)]
        public readonly uint ModuleEndAddress;

        /// <summary>
        /// Zero-end string of the command line.
        /// </summary>
        [FieldOffset(24)]
        public readonly char* CommandLine;
    }
}
