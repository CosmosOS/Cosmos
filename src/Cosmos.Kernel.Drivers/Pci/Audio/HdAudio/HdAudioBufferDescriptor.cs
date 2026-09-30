// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers.Pci.Audio.HdAudio;

/// <summary>
/// One entry of a stream's Buffer Descriptor List: where a run of frames
/// sits in memory, how long it is, and whether finishing it raises an
/// interrupt. The list is read by the controller's DMA engine, so the
/// layout is the hardware's, not the compiler's.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct HdAudioBufferDescriptor
{
    /// <summary>Interrupt on completion: the engine raises the stream's completion bit when it finishes this entry.</summary>
    internal const uint InterruptOnCompletion = 1u << 0;

    /// <summary>Physical address of the entry's first byte.</summary>
    internal ulong Address;

    /// <summary>Length of the entry in bytes.</summary>
    internal uint Length;

    /// <summary>Entry flags; only <see cref="InterruptOnCompletion"/> is defined.</summary>
    internal uint Flags;
}
