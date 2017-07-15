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
        static private Canvas mVideoDriver = null;

        public static Canvas GetFullScreenCanvas(Mode mode)
        {
            Global.mDebugger.SendInternal("GetFullScreenCanvas() with mode " + mode);

            if (mVideoDriver == null)
                return mVideoDriver = new VBEScreen(mode);

            /* We have already got a VideoDriver istance simple change its mode */
            mVideoDriver.Mode = mode;
            return mVideoDriver;
        }

        public static Canvas GetFullScreenCanvas()
        {
            Global.mDebugger.SendInternal($"GetFullScreenCanvas() with default mode");

            if (mVideoDriver == null)
            {
                mVideoDriver = new VBEScreen();
            }

            /* We have already got a VideoDriver instance simple reset its mode to DefaultGraphicMode */
            //mVideoDriver.Mode = mVideoDriver.DefaultGraphicMode;

            MouseManager.ScreenWidth = (uint)mVideoDriver.Mode.Columns;
            MouseManager.ScreenHeight = (uint)mVideoDriver.Mode.Rows;

            return mVideoDriver;
        }
    }
}
