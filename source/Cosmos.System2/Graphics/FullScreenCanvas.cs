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
        
        private static PCIDevice SVGAIIDevice = PCI.GetDevice(0x15AD, 0x0405);
        
        private static bool SVGAIIExists = SVGAIIDevice.DeviceExists;
        
        private static VideoDriver videoDevice;

        private static Canvas MyVideoDriver;

        public static Canvas GetFullScreenCanvas(Mode mode)
        {
            Global.mDebugger.SendInternal("GetFullScreenCanvas() with mode " + mode);

            /* Use SVGAII When Exists in PCI */
            if(SVGAIIExists)
                videoDevice = VideoDriver.VMWareSVGAIIDriver;
            
            if (videoDevice == VideoDriver.VMWareSVGAIIDriver)
                return MyVideoDriver = new SVGAIIScreen(mode);
            else if (videoDevice == VideoDriver.VBEDriver)
                return MyVideoDriver = new VBEScreen(mode);

            /* We have already got a VideoDriver istance simple change its mode */
            MyVideoDriver.Mode = mode;
            return MyVideoDriver;
        }

        public static Canvas GetFullScreenCanvas()
        {
            Global.mDebugger.SendInternal($"GetFullScreenCanvas() with default mode");
            
            /* Use SVGAII When Exists in PCI */
            if(SVGAIIExists)
                videoDevice = VideoDriver.VMWareSVGAIIDriver;
            
            if (videoDevice == VideoDriver.VMWareSVGAIIDriver)
                return MyVideoDriver = new SVGAIIScreen();
            else if (videoDevice == VideoDriver.VBEDriver)
                return MyVideoDriver = new VBEScreen();

            /* We have already got a VideoDriver istance simple reset its mode to DefaultGraphicMode */
            MyVideoDriver.Mode = MyVideoDriver.DefaultGraphicMode;
            return MyVideoDriver;
        }
    }
}
