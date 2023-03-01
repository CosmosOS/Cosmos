using System.Collections.Generic;
using Cosmos.HAL;
using System;

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
            MouseSensitivity = 1f;

            foreach (var mouse in HAL.Global.GetMouseDevices())
            {
                AddMouse(mouse);
            }
        }

        #region Properties

        /// <summary>
        /// The screen width (i.e. max value of X).
        /// </summary>
        public static uint ScreenWidth
        {
            get => _ScreenWidth;
            set
            {
                _ScreenWidth = value;

                if (X >= _ScreenWidth)
                {
                    X = _ScreenWidth - 1;
                }
            }
        }

        /// <summary>
        /// The screen height (i.e. max value of Y).
        /// </summary>
        public static uint ScreenHeight
        {
            get => _ScreenHeight;
            set
            {
                _ScreenHeight = value;

                if (Y >= _ScreenHeight)
                {
                    Y = _ScreenHeight - 1;
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
                if (_HasReadDeltaX)
                {
                    return 0;
                }

                _HasReadDeltaX = true;
                return _DeltaX;
            }
            internal set
            {
                _HasReadDeltaX = false;
                _DeltaX = value;
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
                if (_HasReadDeltaY)
                {
                    return 0;
                }

                _HasReadDeltaY = true;
                return _DeltaY;
            }
            internal set
            {
                _HasReadDeltaY = false;
                _DeltaY = value;
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
            // Mouse should be disabled if nothing has been set.
            if (ScreenHeight == 0 || ScreenWidth == 0)
			{
                return;
			}

            // Assign new dleta values.
            DeltaX = aDeltaX;
            DeltaY = aDeltaY;

            X = (uint)Math.Clamp(X + (MouseSensitivity * aDeltaX), 0, ScreenWidth - 1);
            Y = (uint)Math.Clamp(Y + (MouseSensitivity * aDeltaY), 0, ScreenHeight - 1);
            LastMouseState = MouseState;
            MouseState = (MouseState)aMouseState;
        }

        /// <summary>
        /// Add mouse to the mouse list.
        /// </summary>
        /// <param name="aMouse">A mouse to add.</param>
        private static void AddMouse(MouseBase aMouse)
        {
            aMouse.OnMouseChanged = HandleMouse;
            _MouseList.Add(aMouse);
        }

        #endregion

        #region Fields

        private static List<MouseBase> _MouseList = new();

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

        private static uint _ScreenWidth;
        private static uint _ScreenHeight;

        // These values are used as flags for the Delta X and Y properties, explained more there.
        private static bool _HasReadDeltaX;
        private static bool _HasReadDeltaY;

        // Temporary 'cache' delta values.
        private static int _DeltaX;
        private static int _DeltaY;

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
