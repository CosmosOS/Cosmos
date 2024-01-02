using System.Collections.Generic;
using Cosmos.HAL;
using System;

namespace Cosmos.System
{
    /// <summary>
    /// Manages the mouse.
    /// </summary>
    public static class MouseManager
    {
        #region Fields
        private static List<MouseBase> mouseList = new();

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
        public static float MouseSensitivity;

        private static uint screenWidth;
        private static uint screenHeight;

        // These values are used as flags for the Delta X and Y properties, explained more there.
        private static bool hasReadDeltaX;
        private static bool hasReadDeltaY;

        // Temporary 'cache' delta values.
        private static int deltaX;
        private static int deltaY;

        private static int scrollDelta;

        /// <summary>
        /// The X location of the mouse.
        /// </summary>
        public static uint X;

        /// <summary>
        /// The Y location of the mouse.
        /// </summary>
        public static uint Y;

        #endregion

        static MouseManager()
        {
            MouseSensitivity = 1f;

            foreach (var mouse in HAL.Global.GetMouseDevices())
            {
                AddMouse(mouse);
            }
        }

        #region Properties

        /// <summary>
        /// The width of the mouse screen area (i.e. max value of X).
        /// </summary>
        public static uint ScreenWidth
        {
            get => screenWidth;
            set
            {
                screenWidth = value;

                if (X >= screenWidth)
                {
                    X = screenWidth - 1;
                }
            }
        }

        /// <summary>
        /// The screen height (i.e. max value of Y).
        /// </summary>
        public static uint ScreenHeight
        {
            get => screenHeight;
            set
            {
                screenHeight = value;

                if (Y >= screenHeight)
                {
                    Y = screenHeight - 1;
                }
            }
        }

        /// <summary>
        /// The 'delta' mouse movement for X.
        /// </summary>
        public static int DeltaX
        {
            get
            {
                // If the delta has been read already, return 0.
                // This is a workaround for the PS/2 mouse not updating it's delta values when movement has stopped.
                if (hasReadDeltaX)
                {
                    return 0;
                }

                hasReadDeltaX = true;
                return deltaX;
            }
            internal set
            {
                hasReadDeltaX = false;
                deltaX = value;
            }
        }

        /// <summary>
        /// The 'delta' mouse movement for Y.
        /// </summary>
        public static int DeltaY
        {
            get
            {
                // If the delta has been read already, return 0.
                // This is a workaround for the PS/2 mouse not updating it's delta values when movement has stopped.
                if (hasReadDeltaY)
                {
                    return 0;
                }

                hasReadDeltaY = true;
                return deltaY;
            }
            internal set
            {
                hasReadDeltaY = false;
                deltaY = value;
            }
        }

        /// <summary>
        /// The 'delta' for the mouse scroll wheel. Needs to be manually reset.
        /// </summary>
        public static int ScrollDelta {
            get {
                return scrollDelta;
            }
            internal set => scrollDelta = value;
        }

        public static bool ScrollWheelPresent => mouseList.Exists(x => (x is PS2Mouse xPs2 && xPs2.HasScrollWheel));

        #endregion

        #region Methods

        /// <summary>
        /// Handles mouse input.
        /// </summary>
        /// <param name="deltaX">Mouse location change on X axis.</param>
        /// <param name="deltaY">Mouse location change on Y axis.</param>
        /// <param name="mouseState">Mouse pressed button state</param>
        /// <param name="scrollWheel">Unused in this implementation.</param>
        public static void HandleMouse(int deltaX, int deltaY, int mouseState, int scrollWheel)
        {
            // Mouse should be disabled if nothing has been set.
            if (ScreenHeight == 0 || ScreenWidth == 0)
			{
                return;
			}

            // Assign new delta values.
            DeltaX = deltaX;
            DeltaY = deltaY;

            ScrollDelta += scrollWheel;

            X = (uint)Math.Clamp(X + (MouseSensitivity * deltaX), 0, ScreenWidth - 1);
            Y = (uint)Math.Clamp(Y + (MouseSensitivity * deltaY), 0, ScreenHeight - 1);
            LastMouseState = MouseState;
            MouseState = (MouseState)mouseState;
        }

        /// <summary>
        /// Reset the scroll delta to 0.
        /// </summary>
        public static void ResetScrollDelta() {
            ScrollDelta = 0;
        }

        /// <summary>
        /// Add mouse to the mouse list.
        /// </summary>
        /// <param name="aMouse">A mouse to add.</param>
        private static void AddMouse(MouseBase aMouse)
        {
            aMouse.OnMouseChanged = HandleMouse;
            mouseList.Add(aMouse);
        }

        #endregion
    }
}
