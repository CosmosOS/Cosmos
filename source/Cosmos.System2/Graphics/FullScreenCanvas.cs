#define COSMOSDEBUG
using Cosmos.HAL;
using Cosmos.HAL.Drivers;

namespace Cosmos.System.Graphics
{
    public static class FullScreenCanvas
    {
        public static bool IsInUse = false;

        public static void Disable()
        {
            if (IsInUse)
            {
                _VideoDriver.Disable();
                VGAScreen.SetTextMode(VGADriver.TextSize.Size80x25);
            }
        }

        private enum VideoDriver
        {
            VMWareSVGAIIDriver,
            VBEDriver,
            VGADriver
        }

        private static PCIDevice _SVGAIIDevice = PCI.GetDevice(VendorID.VMWare, DeviceID.SVGAIIAdapter);

        public static bool BGAExists()
        {
            return VBEDriver.Available();
        }

        private static Canvas _VideoDriver = null;

        private static Canvas GetVideoDriver()
        {
            if (_SVGAIIDevice != null && PCI.Exists(_SVGAIIDevice))
            {
                return new SVGAIICanvas();
            }
            else if (BGAExists())
            {
                return new VBECanvas();
            }
            else
            {
                return new VGACanvas();
            }
        }

        private static Canvas GetVideoDriver(Mode mode)
        {
            if (_SVGAIIDevice != null && PCI.Exists(_SVGAIIDevice))
            {
                return new SVGAIICanvas(mode);
            }
            else if (BGAExists())
            {
                return new VBECanvas(mode);
            }
            else
            {
                return new VGACanvas(mode);
            }
        }

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
            return _VideoDriver;
        }

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
                return _VideoDriver;
        }
    }
}
