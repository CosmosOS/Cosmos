using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Core;
using Cosmos.HAL;
using Cosmos.HAL.Drivers;

namespace Cosmos.System.Graphics
{
    /// <summary>
    /// Available video driver modes
    /// <list>
    /// <item>VMWareSVGAII - Compatible with VMWare and QEMU(using vga -vmware)</item>
    /// <item>VBE/VESA - Compatible with VirtualBox, QEMU, and BOCHS</item>
    /// <item>VGA - Compatible with VMWare, QEMU, VirtualBox, BOCHS, and bare-metal</item>
    /// </list>
    /// </summary>
    public enum VideoDriver
    {
        VMWareSVGAII,
        VBE,
        VGA,
    }

    /// <summary>
    /// Manage video driver and canvas
    /// </summary>
    public static class FullScreenCanvas
    {
        /// <summary>
        /// Checks if canvas actively being used
        /// </summary>
        public static bool InUse = false;

        // svga pci device
        private static PCIDevice svgaDevice = PCI.GetDevice(VendorID.VMWare, DeviceID.SVGAIIAdapter);

        // canvas
        private static Canvas canvas;

        /// <summary>
        /// Disable fullscreen canvas
        /// </summary>
        public static void Disable()
        {
            if (InUse)
            {
                canvas.Disable();
                VGAScreen.SetTextMode(VGADriver.TextSize.Size80x25);
            }
        }

        // get canvas
        private static Canvas GetCanvas()
        {
            if (svgaDevice != null && PCI.Exists(svgaDevice)) { return new SVGAIICanvas(); }
            if (IsVBECompatible()) { return new VBECanvas(); }
            return new VGACanvas();
        }

        // get canvas with mode
        private static Canvas GetCanvas(Mode mode)
        {
            if (svgaDevice != null && PCI.Exists(svgaDevice)) { return new SVGAIICanvas(mode); }
            if (IsVBECompatible()) { return new VBECanvas(mode); }
            return new VGACanvas(mode);
        }

        // get canvas with driver and mode
        private static Canvas GetCanvas(VideoDriver driver, Mode mode)
        {
            // vmware
            if (driver == VideoDriver.VMWareSVGAII)
            {
                if (svgaDevice != null && PCI.Exists(svgaDevice)) { return new SVGAIICanvas(mode); }
                else { Global.mDebugger.SendInternal("No VMWareSVGAII device found"); throw new Exception("No VMWareSVGAII device found"); }
            }
            // vbe
            else if (driver == VideoDriver.VBE)
            {
                if (IsVBECompatible()) { return new VBECanvas(mode); }
                else { Global.mDebugger.SendInternal("No VMWareSVGAII device found"); throw new Exception("No VMWareSVGAII device found"); }
            }
            // vga
            else if (driver == VideoDriver.VGA) { return new VGACanvas(mode); }
            // return auto-selected otherwise
            return GetCanvas(mode);
        }

        /// <summary>
        /// Automatically select video driver and set to default mode
        /// </summary>
        /// <returns></returns>
        public static Canvas GetFullScreenCanvas()
        {
            Global.mDebugger.SendInternal("Get fullscreen canvas with default mode");
            if (canvas == null) { canvas = GetCanvas(); }
            else { canvas.Mode = canvas.DefaultGraphicMode; }
            return canvas;
        }

        /// <summary>
        /// Automatically select video driver and set to specified mode
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static Canvas GetFullScreenCanvas(Mode mode)
        {
            Global.mDebugger.SendInternal($"Get fullscreen canvas with mode {mode}");
            if (canvas == null) { canvas = GetCanvas(mode); }
            else { canvas.Mode = mode; }
            return canvas;
        }

        /// <summary>
        /// Manually select video driver and set to specified mode
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static Canvas GetFullScreenCanvas(VideoDriver driver, Mode mode)
        {
            Global.mDebugger.SendInternal($"Get fullscreen canvas with driver {driver} and mode {mode}");
            canvas = GetCanvas(driver, mode);
            return canvas;
        }

        /// <summary>
        /// Check if system is compatible with VBE/VESA
        /// </summary>
        /// <returns></returns>
        public static bool IsVBECompatible()
        {
            if (IsBGACompatible()) { return true; }
            if (PCI.Exists(VendorID.VirtualBox, DeviceID.VBVGA)) { return true; }
            if (PCI.Exists(VendorID.Bochs, DeviceID.BGA)) { return true; }
            if (VBE.IsAvailable()) { return true; }
            return false;
        }

        /// <summary>
        /// Check if system is compatible with BGA(Bochs Graphics Adapter)
        /// </summary>
        /// <returns></returns>
        public static bool IsBGACompatible() { return VBEDriver.ISAModeAvailable(); }
    }
}
