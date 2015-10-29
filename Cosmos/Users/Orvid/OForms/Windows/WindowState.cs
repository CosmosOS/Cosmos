using System;

namespace OForms.Windows
{
    /// <summary>
    /// The State of a window.
    /// </summary>
    public enum WindowState
    {
        /// <summary>
        /// The window is Maximized.
        /// </summary>
        Maximized,
        /// <summary>
        /// The window is Minimized.
        /// </summary>
        Minimized,
        /// <summary>
        /// The window is neither Minimized, nor Maximized.
        /// </summary>
        Normal,
    }
}
