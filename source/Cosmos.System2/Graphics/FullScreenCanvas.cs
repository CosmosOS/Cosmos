//#define COSMOSDEBUG
using Cosmos.HAL;
using sys = System;

namespace Cosmos.System.Graphics
{
    /// <summary>
    /// FullScreenCanvas class. Used to set and get full screen canvas.
    /// </summary>
    public static class FullScreenCanvas
    {
        /// <summary>
        /// SVGA 2 device.
        /// </summary>
        private static PCIDevice SVGAIIDevice = PCI.GetDevice(VendorID.VMWare, DeviceID.SVGAIIAdapter);

        /// <summary>
        /// Check if SVGA 2 Exists.
        /// </summary>
        /// <returns>bool value.</returns>
        public static bool SVGAIIExist()
        {
            if (SVGAIIDevice == null)
            {
                return false;
            }

            return SVGAIIDevice.DeviceExists;
        }

        /// <summary>
        /// Video driver.
        /// </summary>
        private static Canvas MyVideoDriver = null;

        /// <summary>
        /// Get video driver.
        /// </summary>
        /// <returns>Canvas value.</returns>
        /// <exception cref="sys.ArgumentOutOfRangeException">Thrown if default graphics mode is not suppoted.</exception>
        private static Canvas GetVideoDriver()
        {
            if (SVGAIIExist())
            {
                return new SVGAIIScreen();
            }
            else
            {
                return new VBEScreen();
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
            if (SVGAIIExist())
            {
                return new SVGAIIScreen(mode);
            }
            else
            {
                return new VBEScreen(mode);
            }
        }

        /// <summary>
        /// Get full screen canvas.
        /// </summary>
        /// <returns>Canvas value.</returns>
        /// <exception cref="sys.ArgumentOutOfRangeException">Thrown if default graphics mode is not suppoted.</exception>
        public static Canvas GetFullScreenCanvas()
        {
            Global.mDebugger.SendInternal($"GetFullScreenCanvas() with default mode");
            if (MyVideoDriver == null)
            {
                Global.mDebugger.SendInternal($"MyVideoDriver is null creating new object");
                return MyVideoDriver = GetVideoDriver();
            }
            else
            {
                Global.mDebugger.SendInternal($"MyVideoDriver is NOT null using the old one changing mode to DefaulMode");
                MyVideoDriver.Mode = MyVideoDriver.DefaultGraphicMode;
                return MyVideoDriver;
            }
        }

        /// <summary>
        /// Get full screen canvas.
        /// </summary>
        /// <param name="mode">Mode.</param>
        /// <returns>Canvas value.</returns>
        /// <exception cref="sys.ArgumentOutOfRangeException">Thrown if graphics mode is not suppoted.</exception>
        public static Canvas GetFullScreenCanvas(Mode mode)
        {
            Global.mDebugger.SendInternal($"GetFullScreenCanvas() with mode" + mode);

            if (MyVideoDriver == null)
            {
                return MyVideoDriver = GetVideoDriver(mode);
            }
            else
            {
                MyVideoDriver.Mode = mode;
                return MyVideoDriver;
            }
        }
    }
}
