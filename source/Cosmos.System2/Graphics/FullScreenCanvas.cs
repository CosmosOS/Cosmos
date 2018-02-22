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
        
        private static bool SVGAIIExists = SVGAIIDevice.DeviceExists;
        
        private static VideoDriver videoDevice;
        
        // Created null - NullReferenceException when calling GetFullScreenCanvas() with 0 overloads
        private static Canvas MyVideoDriver = null;

        public static Canvas GetFullScreenCanvas(Mode mode)
        {
            Global.mDebugger.SendInternal("GetFullScreenCanvas() with mode " + mode);

            /* Use SVGAII When Exists in PCI */
            if(SVGAIIExists)
                videoDevice = VideoDriver.VMWareSVGAIIDriver;
		
	    // If there's no instance of a video driver created (which there isn't), create it:
            if (MyVideoDriver == null)
            {
    		    // If running on VMWare and using SVGAII, then use that driver:
        		if (videoDevice == VideoDriver.VMWareSVGAIIDriver)
                {
                    // Creates the instance
                    return MyVideoDriver = new SVGAIIScreen(mode);
                }
		        // If not running on VMWare, then use the VESA BIOS Extensions:
		        else if (videoDevice == VideoDriver.VBEDriver)
                {
			        // Creates the instance
			        return MyVideoDriver = new VBEScreen(mode);
	            }
            }

            /* We have already got a VideoDriver instance, simply change its mode */
            MyVideoDriver.Mode = mode;
            return MyVideoDriver;
        }

        public static Canvas GetFullScreenCanvas()
        {
            Global.mDebugger.SendInternal($"GetFullScreenCanvas() with default mode");

            /*            
            WARNING: MyVideoDriver is initiated as null - needs changing!
             We have already got a VideoDriver instance
                - reset its mode to DefaultGraphicMode */
            return GetFullScreenCanvas(MyVideoDriver.DefaultGraphicMode);
        }
    }
}
