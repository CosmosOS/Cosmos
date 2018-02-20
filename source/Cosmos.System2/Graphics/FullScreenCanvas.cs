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

        private static Canvas MyVideoDriver = null;

        public static Canvas GetFullScreenCanvas(Mode mode)
        {
            Global.mDebugger.SendInternal("GetFullScreenCanvas() with mode " + mode);

            /* Use SVGAII When Exists in PCI */
            if(SVGAIIExists)
                videoDevice = VideoDriver.VMWareSVGAIIDriver;
		
	    // If there's no instance of a video driver created (which there isn't), create it:
            if (MyVideoDriver == null) {
		
		// If running on VMWare and using SVGAII, then use that driver:
		if (videoDevice == VideoDriver.VMWareSVGAIIDriver)
			// Creates the instance
			return MyVideoDriver = new SVGAIIScreen(mode);
		// If not running on VMWare, then use the VESA BIOS Extensions:
		else if (videoDevice == VideoDriver.VBEDriver)
			// Creates the instance
			return MyVideoDriver = new VBAScreen(mode);
	    }
            

            /* We have already got a VideoDriver istance simple change its mode */
            MyVideoDriver.Mode = mode;
            return MyVideoDriver;
        }

        public static Canvas GetFullScreenCanvas()
        {
            Global.mDebugger.SendInternal($"GetFullScreenCanvas() with default mode");
 
            /* We have already got a VideoDriver istance simple reset its mode to DefaultGraphicMode */
            return GetFullScreenCanvas(MyVideoDriver.DefaultGraphicMode);
        }
    }
}
