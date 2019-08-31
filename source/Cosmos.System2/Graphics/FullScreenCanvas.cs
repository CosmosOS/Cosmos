//#define COSMOSDEBUG
using Cosmos.HAL;

namespace Cosmos.System.Graphics
{
    public static class FullScreenCanvas
    {
        public static bool IsInUse = false;

        public static void Disable()
        {
            if (IsInUse == false) { }
            else if (IsInUse == true)
            {
                MyVideoDriver.Disable();
                VGAScreen.SetTextMode(VGAScreen.TextSize.Size80x25);
            }
        }

        private enum VideoDriver
        {
            VMWareSVGAIIDriver,
            VBEDriver,
            VGADriver
        }

        private static HAL.Drivers.VBEDriver VBEDriver;

        private static PCIDevice SVGAIIDevice = PCI.GetDevice(VendorID.VMWare, DeviceID.SVGAIIAdapter);

        public static bool BGAExists()
        {
            if (VBEDriver.Available() == false)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private static VideoDriver _VideoDevice;

        private static Canvas MyVideoDriver = null;

        private static Canvas GetVideoDriver()
        {
            if (PCI.Exists(SVGAIIDevice))
            {
                return new SVGAIICanvas();
            }
            else if (BGAExists() == true)
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
            if (PCI.Exists(SVGAIIDevice) == true)
            {
                return new SVGAIICanvas(mode);
            }
            else if (BGAExists() == true)
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
