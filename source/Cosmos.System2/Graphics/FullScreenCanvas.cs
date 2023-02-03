//#define COSMOSDEBUG
using System;
using Cosmos.Core;
using Cosmos.HAL;
using Cosmos.HAL.Drivers;

namespace Cosmos.System.Graphics
{
    /// <summary>
    /// FullScreenCanvas class. Used to set and get full screen canvas.
    /// </summary>
    public static class FullScreenCanvas
    {
        /// <summary>
        /// Boolean value whether CGS is in use or not
        /// </summary>
        public static bool IsInUse = false;

        /// <summary>
        /// Disables the specified Graphics Driver used and returns to VGA text mode 80x25
        /// </summary>
        public static void Disable()
        {
            if (IsInUse)
            {
                _VideoDriver.Disable();
                VGAScreen.SetTextMode(VGADriver.TextSize.Size80x25);
                IsInUse = false;
            }
        }

        /// <summary>
        /// List of all video drivers (BGA/VBE, VGA, VMware SVGA II)
        /// </summary>
        private enum VideoDriver
        {
            VMWareSVGAIIDriver,
            VBEDriver,
            VGADriver
        }

        /// <summary>
        /// SVGA 2 device.
        /// </summary>
        private static PCIDevice _SVGAIIDevice = PCI.GetDevice(VendorID.VMWare, DeviceID.SVGAIIAdapter);

        /// <summary>
        /// Checks whether the Bochs Graphics Adapter exists (not limited to Bochs)
        /// </summary>
        /// <returns></returns>
        public static bool BGAExists()
        {
            return VBEDriver.ISAModeAvailable();
        }

        /// <summary>
        /// Video driver.
        /// </summary>
        private static Canvas _VideoDriver = null;

        /// <summary>
        /// Get video driver.
        /// </summary>
        /// <returns>Canvas value.</returns>
        /// <exception cref="sys.ArgumentOutOfRangeException">Thrown if default graphics mode is not suppoted.</exception>
        private static Canvas GetVideoDriver()
        {
            if (_SVGAIIDevice != null && PCI.Exists(_SVGAIIDevice))
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
        /// Get video driver.
        /// </summary>
        /// <param name="mode">Mode.</param>
        /// <returns>Canvas value.</returns>
        /// <exception cref="sys.ArgumentOutOfRangeException">Thrown if graphics mode is not suppoted.</exception>
        private static Canvas GetVideoDriver(Mode mode)
        {
            if (_SVGAIIDevice != null && PCI.Exists(_SVGAIIDevice))
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
        /// Get full screen canvas.
        /// Changes current Mode to default.
        /// </summary>
        /// <returns>Canvas value.</returns>
        /// <exception cref="sys.ArgumentOutOfRangeException">Thrown if default graphics mode is not suppoted.</exception>
        public static Canvas GetFullScreenCanvas()
        {
            Global.mDebugger.SendInternal($"GetFullScreenCanvas() with default mode");
            if (_VideoDriver == null)
            {
                Global.mDebugger.SendInternal($"_VideoDriver is null creating new object");
                _VideoDriver = GetVideoDriver();
            }
            else
            {
                Global.mDebugger.SendInternal($"_VideoDriver is NOT null using the old one changing mode to DefaultMode");
                _VideoDriver.Mode = _VideoDriver.DefaultGraphicMode;
            }
            IsInUse = true;
            return _VideoDriver;
        }

        /// <summary>
        /// Get full screen canvas.
        /// Changes the current Mode.
        /// </summary>
        /// <param name="mode">Mode.</param>
        /// <returns>Canvas value.</returns>
        /// <exception cref="sys.ArgumentOutOfRangeException">Thrown if graphics mode is not suppoted.</exception>
        public static Canvas GetFullScreenCanvas(Mode mode)
        {
            Global.mDebugger.SendInternal($"GetFullScreenCanvas() with mode" + mode);

            if (_VideoDriver == null)
            {
                _VideoDriver = GetVideoDriver(mode);
            }
            else
            {
                _VideoDriver.Mode = mode;
            }
            IsInUse = true;
            return _VideoDriver;
        }

        /// <summary>
        /// Trys to get full screen canvas.
        /// </summary>
        /// <param name="mode">Mode.</param>
        /// <returns>true if successfully; otherwise, false.</returns>
        /// <exception cref="sys.ArgumentOutOfRangeException">Thrown if graphics mode is not suppoted.</exception>
        public static bool TryGetFullScreenCanvas(Mode mode, out Canvas canvas)
        {
            Global.mDebugger.SendInternal($"TryGetFullScreenCanvas() with mode" + mode);

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
        /// Gets current full screen canvas
        /// with out setting Mode.
        /// </summary>
        /// <returns>Canvas value.</returns>
        public static Canvas GetCurrentFullScreenCanvas()
        {
            Global.mDebugger.SendInternal($"GetCurrentFullScreenCanvas()");

            return _VideoDriver;
        }

        /// <summary>
        /// Checks is VBE is supported exists
        /// </summary>
        /// <returns></returns>
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
