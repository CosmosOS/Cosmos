using System;

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
}
