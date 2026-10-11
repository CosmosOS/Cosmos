// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Drivers.Pci.Display.IntelGraphics;

/// <summary>
/// The integrated GPU families <see cref="IntelGraphicsDriver"/> knows, one per
/// display engine generation, as Linux's i915 groups the device ids. Each
/// member names the processors whose integrated graphics it covers.
/// </summary>
internal enum IntelGraphicsPlatform
{
    /// <summary>2nd generation Core (Sandy Bridge): HD Graphics 2000 and 3000, display version 6.</summary>
    SandyBridge,

    /// <summary>3rd generation Core (Ivy Bridge): HD Graphics 2500 and 4000, display version 7.</summary>
    IvyBridge,

    /// <summary>4th generation Core (Haswell): HD Graphics 4200 to 5200, display version 7 with DDI.</summary>
    Haswell,

    /// <summary>5th generation Core (Broadwell): HD Graphics 5300 to 6300, display version 8.</summary>
    Broadwell,

    /// <summary>
    /// 6th to 10th generation Core with Gen9 graphics (Skylake, Kaby Lake,
    /// Amber Lake, Coffee Lake, Whiskey Lake, Comet Lake): HD and UHD
    /// Graphics 5xx and 6xx, display version 9, the first with universal planes.
    /// </summary>
    Skylake,

    /// <summary>10th generation Core (Ice Lake): UHD and Iris Plus Graphics, display version 11.</summary>
    IceLake,

    /// <summary>
    /// 11th generation Core (Tiger Lake, Rocket Lake) and the 12th to 14th
    /// generation desktop parts (Alder Lake-S, Raptor Lake-S): UHD and Iris Xe
    /// Graphics, display version 12.
    /// </summary>
    TigerLake,

    /// <summary>
    /// 12th and 13th generation mobile Core (Alder Lake-P, Alder Lake-N,
    /// Raptor Lake-P, Raptor Lake-U): Iris Xe and UHD Graphics, display version 13.
    /// </summary>
    AlderLakeP,
}
