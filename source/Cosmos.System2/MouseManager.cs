using System;
using System.Collections.Generic;

using Cosmos.HAL;

namespace Cosmos.System
{
    /// <summary>
    /// The possible states of a mouse.
    /// </summary>
    [Flags]
    public enum MouseState
    {
        /// <summary>
        /// No button is pressed.
        /// </summary>
        None = 0b0000_0000,
        /// <summary>
        /// The left mouse button is pressed.
        /// </summary>
        Left = 0b0000_0001,
        /// <summary>
        /// The right mouse button is pressed.
        /// </summary>
        Right = 0b0000_0010,
        /// <summary>
        /// The middle mouse button is pressed.
        /// </summary>
        Middle = 0b0000_0100,
        /// <summary>
        /// The fourth mouse button is pressed.
        /// </summary>
        FourthButton = 0b0000_1000,
        /// <summary>
        /// The fifth mouse button is pressed.
        /// </summary>
        FifthButton = 0b0001_0000
    }

    public static class MouseManager
    {
        private static List<MouseBase> mMouseList = new List<MouseBase>();

        /// <summary>
        /// The X location of the mouse.
        /// </summary>
        public static uint X;

        /// <summary>
        /// The Y location of the mouse.
        /// </summary>
        public static uint Y;

        /// <summary>
        /// The Point of the mouse.
        /// </summary>
        // public static Point GetPoint() {return new Point((int)MouseManager.X, (int)MouseManager.Y);}

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

                if (X >= mScreenHeight)
                {
                    X = mScreenHeight - 1;
                }
            }
        }

        static MouseManager()
        {
            foreach (var mouse in HAL.Global.GetMouseDevices())
            {
                AddMouse(mouse);
            }
        }

        public static void HandleMouse(int aDeltaX, int aDeltaY, int aMouseState, int aScrollWheel)
        {
            int x = (int)(X + MouseSensitivity * aDeltaX);
            int y = (int)(Y + MouseSensitivity * aDeltaY);
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

        private static void AddMouse(MouseBase aMouse)
        {
            aMouse.OnMouseChanged = HandleMouse;
            mMouseList.Add(aMouse);
        }
    }
}
