using System.Collections.Generic;
using Cosmos.HAL;

namespace Cosmos.System
{
    /// <summary>
    /// Mouse manager class.
    /// </summary>
    public static class MouseManager
    {
        /// <summary>
        /// Mouse manager constructor.
        /// </summary>
        static MouseManager()
        {
            foreach (var mouse in HAL.Global.GetMouseDevices())
            {
                AddMouse(mouse);
            }
        }

        #region Properties

        /// <summary>
        /// The 'delta' mouse movement for X.
        /// </summary>
        public static int DeltaX { get; internal set; }

        /// <summary>
        /// The 'delta' mouse movement for Y.
        /// </summary>
        public static int DeltaY { get; internal set; }

        /// <summary>
        /// The screen width (i.e. max value of X).
        /// </summary>
        public static uint ScreenWidth
        {
            get => mScreenWidth;
            set
            {
                mScreenWidth = value;

                if (X >= mScreenWidth)
                {
                    X = mScreenWidth - 1;
                }
            }
        }

        /// <summary>
        /// The screen height (i.e. max value of Y).
        /// </summary>
        public static uint ScreenHeight
        {
            get => mScreenHeight;
            set
            {
                mScreenHeight = value;

                if (Y >= mScreenHeight)
                {
                    Y = mScreenHeight - 1;
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Mouse handler.
        /// </summary>
        /// <param name="aDeltaX">Mouse location change on X axis.</param>
        /// <param name="aDeltaY">Mouse location change on Y axis.</param>
        /// <param name="aMouseState">Mouse pressed button state</param>
        /// <param name="aScrollWheel">unused</param>
        public static void HandleMouse(int aDeltaX, int aDeltaY, int aMouseState, int aScrollWheel)
        {
            // Assign new dleta values.
            DeltaX = aDeltaX;
            DeltaY = aDeltaY;

            int x = (int)(X + MouseSensitivity * aDeltaX);
            int y = (int)(Y + MouseSensitivity * aDeltaY);
            LastMouseState = MouseState;
            MouseState = (MouseState)aMouseState;

            if (x <= 0)
            {
                X = 0;
            }
            else if (x >= ScreenWidth)
            {
                X = ScreenWidth - 1;
            }
            else
            {
                X = (uint)x;
            }

            if (y <= 0)
            {
                Y = 0;
            }
            else if (y >= ScreenHeight)
            {
                Y = ScreenHeight - 1;
            }
            else
            {
                Y = (uint)y;
            }
        }

        /// <summary>
        /// Add mouse to the mouse list.
        /// </summary>
        /// <param name="aMouse">A mouse to add.</param>
        private static void AddMouse(MouseBase aMouse)
        {
            aMouse.OnMouseChanged = HandleMouse;
            mMouseList.Add(aMouse);
        }

        #endregion

        #region Fields

        private static List<MouseBase> mMouseList = new();

        /// <summary>
        /// The state the mouse was in the last frame.
        /// </summary>
        public static MouseState LastMouseState;

        /// <summary>
        /// The state the mouse is currently in.
        /// </summary>
        public static MouseState MouseState;

        /// <summary>
        /// The sensitivity of the mouse, 1.0f is the default.
        /// </summary>
        public static float MouseSensitivity = 1.0f;

        private static uint mScreenWidth;
        private static uint mScreenHeight;

        /// <summary>
        /// The X location of the mouse.
        /// </summary>
        public static uint X;

        /// <summary>
        /// The Y location of the mouse.
        /// </summary>
        public static uint Y;

        #endregion
    }
}
