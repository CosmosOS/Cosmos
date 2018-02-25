//#define COSMOSDEBUG
using Cosmos.System.Graphics;
using Cosmos.HAL;

namespace Cosmos.System.Graphics
{
    public static class FullScreenCanvas
    {
        private enum VideoDriver
        {
            VMWareSVGAIIDriver,
            //VGADriver,
            VBEDriver
        }

        private static PCIDevice SVGAIIDevice = PCI.GetDevice(VendorID.VMWare, DeviceID.SVGAIIAdapter);

        public static bool DoesSVGAIIExist()
        {
            if (SVGAIIDevice != null)
            {
                if (SVGAIIDevice.DeviceExists == true)
                {
                    return true;
                }
                return false;
            }
            else
            {
                return false;
            }

        }
        //public static bool SVGAIIExists = SVGAIIDevice != null;

        private static VideoDriver videoDevice;

        // Created null - NullReferenceException when calling GetFullScreenCanvas() with 0 overloads
        private static Canvas MyVideoDriver = null;

        public static Canvas GetFullScreenCanvas()
        {
            // If MyVideoDriver is null (hasn't checked if VMWare SVGA exists),
            // Do necessary check and set to default gfx mode
            if (MyVideoDriver == null)
            {
                return GetFullScreenCanvas(MyVideoDriver.DefaultGraphicMode);
            }
            // If it's not null, simply change graphics mode */
            else
            {
                MyVideoDriver.Mode = MyVideoDriver.DefaultGraphicMode;
                return MyVideoDriver;
            }
            // Old check left for reference
            /*
                if (videoDevice == VideoDriver.VMWareSVGAIIDriver)
                {

                }
                else
                {

                }
            */
        }
        public static Canvas GetFullScreenCanvas(Mode mode)
        {
            Global.mDebugger.SendInternal($"GetFullScreenCanvas() with mode" + mode);
            // If MyVideoDriver is null (hasn't checked if VMWare SVGA exists),
            // Do necessary check and set gfx mode as specified (mode)
            if (MyVideoDriver == null)
            {
                if (DoesSVGAIIExist())
                {
                    // Set videoDevice to SVGA, initialize MyVideoDriver as an SVGA display using specified mode
                    // MyVideoDriver.Mode = mode; isn't exactly needed, just done in case it doesn't set.
                    // returns MyVideoDriver as the Canvas
                    videoDevice = VideoDriver.VMWareSVGAIIDriver;
                    MyVideoDriver = new SVGAIIScreen(mode);
                    MyVideoDriver.Mode = mode;
                    return MyVideoDriver;
                }
                else
                {
                    // Does the same as above, this time using VESA BIOS Extensions (supported by loads of graphics cards)
                    videoDevice = VideoDriver.VBEDriver;
                    MyVideoDriver = new VBEScreen(mode);
                    MyVideoDriver.Mode = mode;
                    return MyVideoDriver;
                }
            }
            else
            {
                // If MyVideoDriver has been initialized before (Graphics mode has previously been set)
                // Change the graphics mode to the mode specified
                MyVideoDriver.Mode = mode;
                return MyVideoDriver;
            }
        }
    }
}
