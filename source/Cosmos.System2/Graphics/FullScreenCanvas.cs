//#define COSMOSDEBUG
using Cosmos.System.Graphics;
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
                IsInUse = true;
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
                return new SVGAIIScreen();
            }
            else if (BGAExists() == true)
            {
                return new VBEScreen();
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
                return new SVGAIIScreen(mode);
            }
            else if (BGAExists() == true)
            {
                return new VBEScreen(mode);
            }
            else
            {
                return new VGACanvas(mode);
            }
        }

        public static Canvas GetFullScreenCanvas()
        {
            if (MyVideoDriver == null)
            {
                return MyVideoDriver = GetVideoDriver();
            }
            else
            {
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
