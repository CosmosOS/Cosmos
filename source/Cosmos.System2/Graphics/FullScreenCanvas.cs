//#define COSMOSDEBUG
using Cosmos.HAL;

namespace Cosmos.System.Graphics
{
    public static class FullScreenCanvas
    {
        private static PCIDevice SVGAIIDevice = PCI.GetDevice(VendorID.VMWare, DeviceID.SVGAIIAdapter);

        public static bool SVGAIIExist()
        {
            if (SVGAIIDevice == null)
            {
                return false;
            }

            return SVGAIIDevice.DeviceExists;
        }

        private static Canvas MyVideoDriver = null;

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
