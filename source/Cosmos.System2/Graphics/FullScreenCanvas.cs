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

        public static Canvas GetFullScreenCanvas(bool buffered = false)
        {
            Global.mDebugger.SendInternal($"GetFullScreenCanvas() with default mode");
            if (MyVideoDriver == null)
            {
                Global.mDebugger.SendInternal($"MyVideoDriver is null creating new object");
                if (buffered)
                    return MyVideoDriver = new BasicBufferScreen(GetVideoDriver());
                else
                    return MyVideoDriver = GetVideoDriver();
            }
            else
            {
                Global.mDebugger.SendInternal($"MyVideoDriver is NOT null using the old one changing mode to DefaulMode");
                MyVideoDriver.Mode = MyVideoDriver.DefaultGraphicMode;
                return MyVideoDriver;
            }
        }

        public static Canvas GetFullScreenCanvas(Mode mode, bool buffered = false)
        {
            Global.mDebugger.SendInternal($"GetFullScreenCanvas() with mode" + mode);

            if (MyVideoDriver == null)
            {
                if (buffered)
                    return MyVideoDriver = new BasicBufferScreen(GetVideoDriver(mode));
                else
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
