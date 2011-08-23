using System;
using Orvid.Graphics;
using System.Collections.Generic;

namespace OForms.Windows
{
    public class WindowManager
    {
        internal const int TaskBarHeight = 15;

        /// <summary>
        /// An array containing all of the active windows 
        /// in the current window manager instance.
        /// </summary>
        public Window[] ActiveWindows;
        /// <summary>
        /// The currently active window. Beware,
        /// there is no array bounds check.
        /// </summary>
        public Window ActiveWindow
        {
            get
            {
                return ActiveWindows[0];
            }
            set
            {
                BringWindowToFront(value);
            }
        }

        public Vec2 Size;

        /// <summary>
        /// The default constructor.
        /// </summary>
        public WindowManager(Vec2 size)
        {
            ActiveWindows = new Window[0];
            this.Size = size;
        }

        /// <summary>
        /// Draws all the windows on the specified image.
        /// </summary>
        /// <param name="i">The image to draw the windows on.</param>
        public void Draw(Image i)
        {
            if (NeedToRedrawAll)
            {
                i.Clear(Colors.White);
                if (ActiveWindows.Length > 0 && ActiveWindows[0].CurrentState == WindowState.Maximized)
                {
                    ActiveWindows[0].Draw(i);
                }
                else
                {
                    for (int ind = ActiveWindows.Length - 1; ind >= 0; ind--)
                    {
                        if (ActiveWindows[ind].CurrentState != WindowState.Minimized)
                        {
                            ActiveWindows[ind].Draw(i);
                        }
                    }
                }
                NeedToRedrawAll = false;
            }
            else
            {
                if (ActiveWindows.Length > 0)
                {
                    ActiveWindows[0].Draw(i);
                }
            }
        }

        /// <summary>
        /// Is true when all the windows need to be re-drawn,
        /// in other-words, is true if a window has been moved,
        /// resized, added, or removed.
        /// </summary>
        internal bool NeedToRedrawAll = false;

        /// <summary>
        /// Adds a window at the front.
        /// </summary>
        /// <param name="w">The window to add.</param>
        public void AddWindow(Window w)
        {
            w.Parent = this;
            w.IsActiveWindow = true;
            if (ActiveWindows.Length > 0)
            {
                ActiveWindows[0].IsActiveWindow = false;
            }
            Window[] tmp = new Window[ActiveWindows.Length + 1];
            Array.Copy(ActiveWindows, 0, tmp, 1, ActiveWindows.Length);
            tmp[0] = w;
            ActiveWindows = tmp;
            NeedToRedrawAll = true;
        }

        /// <summary>
        /// Maximize the specified window.
        /// </summary>
        /// <param name="w">The window to maximize.</param>
        public void MaximizeWindow(Window w)
        {
            w.CurrentState = WindowState.Maximized;
        }

        /// <summary>
        /// Restore a window to the Normal state.
        /// </summary>
        /// <param name="w">The window to restore.</param>
        public void RestoreWindow(Window w)
        {
            w.CurrentState = WindowState.Normal;
        }

        /// <summary>
        /// Minimizes the specified window.
        /// </summary>
        /// <param name="w">The window to minimize.</param>
        public void MinimizeWindow(Window w)
        {
            if (w.IsActiveWindow)
            {
                this.SendWindowToBack(w);
            }
            w.CurrentState = WindowState.Minimized;
        }

        /// <summary>
        /// Sends the specified window to the back.
        /// </summary>
        /// <param name="w">The window to send to the back.</param>
        public void SendWindowToBack(Window w)
        {
            if (w.IsActiveWindow)
            {
                w.IsActiveWindow = false;
            }
            for (int i = 0; i < ActiveWindows.Length; i++)
            {
                if (ActiveWindows[i] == w)
                {
                    RemoveWindow(i);
                    NeedToRedrawAll = true;

                    Window[] winds = new Window[ActiveWindows.Length + 1];
                    Array.Copy(ActiveWindows, winds, ActiveWindows.Length);
                    winds[winds.Length - 1] = w;
                    ActiveWindows = winds;
                    ActiveWindows[0].IsActiveWindow = true;
                    return;
                }
            }
            throw new Exception("Unable to find the specified window.");
        }

        /// <summary>
        /// Bring the specified window to the front.
        /// </summary>
        /// <param name="w">The window to move to the front.</param>
        public void BringWindowToFront(Window w)
        {
            if (ActiveWindows.Length > 0)
            {
                ActiveWindows[0].IsActiveWindow = false;
            }
            w.IsActiveWindow = true;
            for (int i = 0; i < ActiveWindows.Length; i++)
            {
                if (ActiveWindows[i] == w)
                {
                    RemoveWindow(i);
                    AddWindow(w);
                    NeedToRedrawAll = true;
                    return;
                }
            }
            throw new Exception("Specified Window not found!");
        }

        /// <summary>
        /// Removes the window at the specified index.
        /// </summary>
        /// <param name="indx">The index of the window to remove.</param>
        private void RemoveWindow(int indx)
        {
            Window[] tmp = new Window[ActiveWindows.Length - 1];
            Array.Copy(ActiveWindows, tmp, indx);
            Array.Copy(ActiveWindows, indx + 1, tmp, indx, ActiveWindows.Length - indx - 1);
            ActiveWindows = tmp;
            if (ActiveWindows.Length > 0)
            {
                ActiveWindows[0].IsActiveWindow = true;
            }
            NeedToRedrawAll = true;
        }

        /// <summary>
        /// Closes the specified window.
        /// </summary>
        /// <param name="w">The window to close.</param>
        public void CloseWindow(Window w)
        {
            for (int i = 0; i < ActiveWindows.Length; i++)
            {
                if (ActiveWindows[i] == w)
                {
                    RemoveWindow(i);
                    NeedToRedrawAll = true;
                    w.DoClose();
                    return;
                }
            }
        }


    }
}
