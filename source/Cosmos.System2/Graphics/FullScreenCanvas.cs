//#define COSMOSDEBUG
using System;
using Cosmos.Core;
using Cosmos.HAL;
using Cosmos.HAL.Drivers;

namespace Cosmos.System.Graphics
{
    /// <summary>
    /// Provides functionality to fetch canvases that write directly to the
    /// underlying display device.
    /// </summary>
    public static class FullScreenCanvas
    {
        /// <summary>
        /// Whether the CGS (Cosmos Graphics Subsystem) is currently in use.
        /// </summary>
        public static bool IsInUse { get; private set; } = false;

        /// <summary>
        /// Disables the specified graphics driver used, and returns to VGA text mode 80x25.
        /// </summary>
        public static void Disable()
        {
            if (IsInUse)
            {
                videoDriver.Disable();
                VGAScreen.SetTextMode(VGADriver.TextSize.Size80x25);
                IsInUse = false;
            }
        }

        private enum VideoDriver
        {
            VMWareSVGAIIDriver,
            VBEDriver,
            VGADriver
        }

        static Canvas videoDriver = null;
        static readonly PCIDevice svgaIIDevice = PCI.GetDevice(VendorID.VMWare, DeviceID.SVGAIIAdapter);

        /// <summary>
        /// Checks whether the Bochs Graphics Adapter exists.
        /// </summary>
        public static bool BGAExists()
        {
            return VBEDriver.ISAModeAvailable();
        }

        /// <summary>
        /// Gets a <see cref="Canvas"/> instance, using an implementation based on
        /// the currently used video driver.
        /// </summary>
        private static Canvas GetVideoDriver()
        {
            if (svgaIIDevice != null && PCI.Exists(svgaIIDevice))
            {
                return new SVGAIICanvas();
            }
            else if (VBEAvailable())
            {
                return new VBECanvas();
            }
            else
            {
                return new VGACanvas();
            }
        }

        /// <summary>
        /// Gets a <see cref="Canvas"/> instance, using an implementation based on
        /// the currently used video driver, constructing the canvas with the given
        /// <paramref name="mode"/>.
        /// </summary>
        private static Canvas GetVideoDriver(Mode mode)
        {
            if (svgaIIDevice != null && PCI.Exists(svgaIIDevice))
            {
                return new SVGAIICanvas(mode);
            }
            else if (VBEAvailable())
            {
                return new VBECanvas(mode);
            }
            else
            {
                return new VGACanvas(mode);
            }
        }

        /// <summary>
        /// Gets a screen display canvas, and changes the display mode to the default.
        /// </summary>
        public static Canvas GetFullScreenCanvas()
        {
            if (videoDriver == null)
            {
                videoDriver = GetVideoDriver();
            }
            else
            {
                videoDriver.Mode = videoDriver.DefaultGraphicsMode;
            }

            IsInUse = true;
            return videoDriver;
        }

        /// <summary>
        /// Gets a screen display canvas, and changes the display mode to the given <paramref name="mode"/>.
        /// </summary>
        public static Canvas GetFullScreenCanvas(Mode mode)
        {
            if (videoDriver == null)
            {
                videoDriver = GetVideoDriver(mode);
            }
            else
            {
                videoDriver.Mode = mode;
            }

            IsInUse = true;
            return videoDriver;
        }

        /// <summary>
        /// Attempts to get a screen display canvas, and changes the display mode to the default.
        /// </summary>
        /// <returns><see langword="true"/> if the operation was successful; otherwise, <see langword="false"/>.</returns>
        public static bool TryGetFullScreenCanvas(Mode mode, out Canvas canvas)
        {
            try
            {
                canvas = GetFullScreenCanvas(mode);
                IsInUse = true;
                return true;
            }
            catch
            {
            }

            canvas = null;
            return false;
        }

        /// <summary>
        /// Gets the currently used screen display canvas.
        /// </summary>
        public static Canvas GetCurrentFullScreenCanvas()
        {
            return videoDriver;
        }

        private static bool VBEAvailable()
        {
            if (BGAExists())
            {
                return true;
            }
            else if (PCI.Exists(VendorID.VirtualBox, DeviceID.VBVGA))
            {
                return true;
            }
            else if (PCI.Exists(VendorID.Bochs, DeviceID.BGA))
            {
                return true;
            }
            else if (VBE.IsAvailable())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
