//#define COSMOSDEBUG
using Cosmos.System.Graphics;

namespace Cosmos.System.Graphics
{
    public static class FullScreenCanvas
    {
        /*
         * For now we hardcode that the VideoDriver is always VBE when we have more that a driver supported we need to find
         * what to use when we do the 'new' (inside GetFullScreenCanvas() static methods). MyVideoDriver should be
         * of type Canvas
         */
        static private Canvas MyVideoDriver = null;

        public static Canvas GetFullScreenCanvas(Mode mode)
        {
            Global.mDebugger.SendInternal("GetFullScreenCanvas() with mode " + mode);

            if (MyVideoDriver == null)
                return MyVideoDriver = new VBEScreen(mode);

            /* We have already got a VideoDriver istance simple change its mode */
            MyVideoDriver.Mode = mode;
            return MyVideoDriver;
        }

        public static Canvas GetFullScreenCanvas()
        {
            Global.mDebugger.SendInternal($"GetFullScreenCanvas() with default mode");
            if (MyVideoDriver == null)
                return new VBEScreen();

            /* We have already got a VideoDriver istance simple reset its mode to DefaultGraphicMode */
            MyVideoDriver.Mode = MyVideoDriver.DefaultGraphicMode;
            return MyVideoDriver;
        }
    }
}
