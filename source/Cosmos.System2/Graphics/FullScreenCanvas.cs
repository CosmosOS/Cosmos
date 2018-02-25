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

        public static bool SVGAIIExist()
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
        private static VideoDriver videoDevice;

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
            // If MyVideoDriver is null (hasn't checked if VMWare SVGA exists),
            // Do necessary check and set gfx mode as specified (mode)
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
/*
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
/*

// If MyVideoDriver is null (hasn't checked if VMWare SVGA exists),
// Do necessary check and set to default gfx mode
if (MyVideoDriver == null)
{
if (DoesSVGAIIExist())
{
    // Set videoDevice to SVGA, initialize MyVideoDriver as an SVGA display using specified mode
    // MyVideoDriver.Mode = mode; isn't exactly needed, just done in case it doesn't set.
    // returns MyVideoDriver as the Canvas
    videoDevice = VideoDriver.VMWareSVGAIIDriver;
    MyVideoDriver = new SVGAIIScreen(SVGAIIScreen.defaultGraphicsMode);
    MyVideoDriver.Mode = SVGAIIScreen.defaultGraphicsMode;
    return MyVideoDriver;
}
else
{
    // Does the same as above, this time using VESA BIOS Extensions (supported by loads of graphics cards)
    videoDevice = VideoDriver.VBEDriver;
    MyVideoDriver = new VBEScreen(VBEScreen.defaultGraphicsMode);
    MyVideoDriver.Mode = VBEScreen.defaultGraphicsMode;
    return MyVideoDriver;
}
}
// If it's not null, simply change graphics mode
else
{
MyVideoDriver.Mode = MyVideoDriver.DefaultGraphicMode;
return MyVideoDriver;
}
*/


