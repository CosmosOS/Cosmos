using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Core;
using Cosmos.HAL;
using Cosmos.HAL.Drivers;

namespace Cosmos.System.Graphics
{
    public enum VideoDriver
    {
        VMWareSVGAIIDriver,
        VBEDriver,
        VGADriver
    }

    public static class FullScreenCanvas
    {
        public static bool InUse = false;

        // svga device
        private static PCIDevice svga = PCI.GetDevice(VendorID.VMWare, DeviceID.SVGAIIAdapter);

        // driver canvas
        private static Canvas canvas;

        // get driver
        private static Canvas GetDriverCanvas()
        {
            if (svga != null && PCI.Exists(svga)) { return new SVGAIICanvas(); }
            if (VBEAvailable()) { return new VBECanvas(); }
            return new VGACanvas();
        }

        // get driver with mode
        private static Canvas GetDriverCanvas(Mode mode)
        {
            if (svga != null && PCI.Exists(svga)) { return new SVGAIICanvas(mode); }
            if (VBEAvailable()) { return new VBECanvas(mode); }
            return new VGACanvas(mode);
        }

        // get full screen canvas
        public static Canvas GetFullScreenCanvas()
        {
            if (canvas == null) { canvas = GetDriverCanvas(); }
            else { canvas.Mode = canvas.DefaultGraphicMode; }
            return canvas;
        }

        // get full screen canvas with size
        public static Canvas GetFullScreenCanvas(Mode mode)
        {
            if (canvas == null) { canvas = GetDriverCanvas(mode); }
            else { canvas.Mode = mode; }
            return canvas;
        }

        // disable canvas
        public static void Disable()
        {
            if (InUse)
            {
                canvas.Disable();
                VGATextMode.SetTextMode(VGADriver.TextSize.Size80x25);
            }
        }

        // check for bochs graphics adapter
        public static bool BGAExists()
        {
            return VBEDriver.ISAModeAvailable();
        }

        // check if vesa is supported
        private static bool VBEAvailable()
        {
            if (BGAExists()) { return true; }
            if (PCI.Exists(VendorID.VirtualBox, DeviceID.VBVGA)) { return true; }
            else if (PCI.Exists(VendorID.Bochs, DeviceID.BGA)) { return true; }
            else if (VBE.IsAvailable()) { return true; }
            return false;
        }
    }
}
