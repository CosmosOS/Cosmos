using System;
using System.Collections.Generic;
using System.Text;

namespace OForms.Windows
{
    /// <summary>
    /// This class describes the default window manager
    /// for the OForms Framework.
    /// </summary>
    public class OWindowManager
    {
        /// <summary>
        /// The list of windows that are minimized.
        /// </summary>
        public LinkedList<OWindow> MinimizedWindows = new LinkedList<OWindow>();
        /// <summary>
        /// The list of windows that are maximized.
        /// </summary>
        public LinkedList<OWindow> MaximizedWindows = new LinkedList<OWindow>();
        /// <summary>
        /// The list of windows that are open, but not maximized;
        /// </summary>
        public LinkedList<OWindow> OpenWindows = new LinkedList<OWindow>();
        /// <summary>
        /// The currently selected window.
        /// </summary>
        public OWindow ActiveWindow;

        /// <summary>
        /// Minimize the specified window.
        /// </summary>
        /// <param name="wndo">The window to minimize.</param>
        public void MinimizeWindow(OWindow wndo)
        {
            if (wndo.WindowState == OWindowState.Active || wndo.WindowState == OWindowState.Open)
            {
                OpenWindows.Remove(wndo);
            }
            else
            {
                MaximizedWindows.Remove(wndo);
            }
            wndo.WindowState = OWindowState.Minimized;
            MinimizedWindows.AddFirst(wndo);
            ActiveWindow = GetNextWindow();
        }

        private OWindow GetNextWindow()
        {
            return null;
        }

        public void MaximizeWindow(OWindow wndo)
        {

        }
    }
}
