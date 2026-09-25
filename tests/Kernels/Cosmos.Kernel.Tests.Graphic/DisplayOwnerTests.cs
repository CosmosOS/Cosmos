// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Pci;
using Cosmos.TestRunner.Framework;

namespace Cosmos.Kernel.Tests.Graphic;

/// <summary>
/// Pins who owns each display function on the PCI bus. Enumeration reserves
/// the function behind the boot framebuffer as "gop" so a driver bound later
/// cannot reprogram the console's BAR, and a built-in display driver replaces
/// that reservation with its own name when it takes the function. IDs and
/// owner names are spelled out, not read from the kernel's enums or
/// <c>PciOwner</c>, so a wrong value is caught instead of compared against
/// itself.
/// </summary>
public static class DisplayOwnerTests
{
    /// <summary>Skip reason on cells without a virtio GPU (bare, vmware-svga).</summary>
    public const string SkipNoVirtioGpu = "virtio-gpu not attached, needs the virtio-gpu profile";

    private const string GopOwner = "gop";
    private const string VmwareSvgaOwner = "vmware-svga";
    private const string VirtioGpuOwner = "virtio-gpu";

    /// <summary>PCI base class of display controllers.</summary>
    private const byte DisplayClassCode = 0x03;

    /// <summary>Display subclass of a VGA-compatible adapter (std VGA, SVGA II): the one the x64 firmware boots on.</summary>
    private const byte VgaCompatibleSubclass = 0x00;

    /// <summary>PCI vendor id of VMware.</summary>
    private const ushort VMwareVendorId = 0x15AD;

    /// <summary>PCI vendor id of virtio devices (Red Hat).</summary>
    private const ushort VirtioVendorId = 0x1AF4;

    /// <summary>Modern virtio device id of a GPU (0x1040 + device type 16); virtio-gpu-pci has no transitional id.</summary>
    private const ushort VirtioGpuDeviceId = 0x1050;

    private static PciDevice? s_vga;
    private static PciDevice? s_virtioGpu;

    /// <summary>True when a virtio GPU was enumerated on the PCI bus.</summary>
    public static bool VirtioGpuPresent => s_virtioGpu is not null;

    /// <summary>Locates the VGA-compatible adapter and the virtio GPU. Called once from BeforeRun.</summary>
    public static void Discover()
    {
        PciDevice[]? devices = PciManager.Devices;
        if (devices is null)
        {
            return;
        }

        for (uint i = 0; i < PciManager.Count; i++)
        {
            PciDevice device = devices[i];
            if (s_vga is null && device.ClassCode == DisplayClassCode && device.Subclass == VgaCompatibleSubclass)
            {
                s_vga = device;
            }

            if (s_virtioGpu is null && device.VendorId == VirtioVendorId && device.DeviceId == VirtioGpuDeviceId)
            {
                s_virtioGpu = device;
            }
        }
    }

    /// <summary>
    /// On x64 every cell boots on a VGA-compatible adapter, and that adapter
    /// must be the function enumeration reserved. The std VGA of the bare and
    /// virtio-gpu cells has no driver and stays reserved; the SVGA II adapter
    /// of the vmware-svga cell is taken by the console's full-screen canvas
    /// during boot. arm64 boots on ramfb, whose framebuffer is in RAM, so no
    /// function may be reserved there, not even the virtio GPU that sits on
    /// the bus in the virtio-gpu cell.
    /// </summary>
    /// <remarks>
    /// The reservation is read from <c>PciManager.BootDisplay</c>, not from
    /// the owners: by the time this runs the virtio scan and the canvas have
    /// replaced gop on every function they drive, so a wrong reservation on
    /// the virtio GPU would be gone from its owner. The gop count then
    /// catches a second function pinned alongside the right one.
    /// </remarks>
    public static void TestBootDisplayOwner()
    {
        PciDevice? bootDisplay = PciManager.BootDisplay;
        int reserved = CountOwnedBy(GopOwner);
        PciDevice? vga = s_vga;
        if (vga is null)
        {
            Assert.Null(bootDisplay, "no function should be reserved when the framebuffer is not in a PCI BAR");
            Assert.Equal(0, reserved, "no function should be owned by gop when none was reserved");
            return;
        }

        Assert.True(bootDisplay == vga, "the VGA-compatible adapter holding the boot framebuffer should be the function reserved");
        bool takenByDriver = vga.VendorId == VMwareVendorId;
        string expected = takenByDriver ? VmwareSvgaOwner : GopOwner;
        Assert.True(vga.Owner == expected, "the boot display should be reserved as gop, or owned by vmware-svga once the canvas took it");
        Assert.Equal(takenByDriver ? 0 : 1, reserved, "only the boot display should be reserved as gop");
    }

    /// <summary>
    /// The virtio PCI scan claims the GPU for its driver; being a display
    /// function does not make it the boot display.
    /// </summary>
    public static void TestVirtioGpuOwner()
    {
        Assert.True(s_virtioGpu?.Owner == VirtioGpuOwner, "the virtio GPU's PCI function should be owned by virtio-gpu");
        Assert.False(PciManager.BootDisplay == s_virtioGpu, "the virtio GPU should not have been reserved as the boot display");
    }

    /// <summary>Number of enumerated functions whose owner is <paramref name="owner"/>.</summary>
    private static int CountOwnedBy(string owner)
    {
        PciDevice[]? devices = PciManager.Devices;
        if (devices is null)
        {
            return 0;
        }

        int count = 0;
        for (uint i = 0; i < PciManager.Count; i++)
        {
            if (devices[i].Owner == owner)
            {
                count++;
            }
        }

        return count;
    }
}
