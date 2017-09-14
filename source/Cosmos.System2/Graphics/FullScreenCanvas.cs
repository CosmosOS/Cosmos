//#define COSMOSDEBUG
using Cosmos.System.Graphics;
using Cosmos.HAL;

namespace Cosmos.System.Graphics
{
    public static class FullScreenCanvas
    {
        public enum VideoDriver
        {
            VMWareSVGAIIDriver,
            //VGADriver,
            VBEDriver
        }

        private static Canvas MyVideoDriver;

        public static Canvas GetFullScreenCanvas(Mode mode, VideoDriver videoDriver)
        {
            Global.mDebugger.SendInternal("GetFullScreenCanvas() with mode " + mode);

            if (videoDriver == VideoDriver.VMWareSVGAIIDriver)
                return MyVideoDriver = new SVGAIIScreen(mode);
            else if (videoDriver == VideoDriver.VBEDriver)
                return MyVideoDriver = new VBEScreen(mode);

            /* We have already got a VideoDriver istance simple change its mode */
            MyVideoDriver.Mode = mode;
            return MyVideoDriver;
        }

        public static Canvas GetFullScreenCanvas(VideoDriver videoDriver)
        {
            Global.mDebugger.SendInternal($"GetFullScreenCanvas() with default mode");

            if (videoDriver == VideoDriver.VMWareSVGAIIDriver)
                return MyVideoDriver = new SVGAIIScreen();
            else if (videoDriver == VideoDriver.VBEDriver)
                return MyVideoDriver = new VBEScreen();

            /* We have already got a VideoDriver istance simple reset its mode to DefaultGraphicMode */
            MyVideoDriver.Mode = MyVideoDriver.DefaultGraphicMode;
            return MyVideoDriver;
        }
    }
}
