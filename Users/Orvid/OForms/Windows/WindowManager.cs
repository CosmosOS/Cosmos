using System;
using Orvid.Graphics;
using System.Collections.Generic;
using Orvid.Graphics.FontSupport;

namespace OForms.Windows
{
    /// <summary>
    /// The class that represents a WindowManager.
    /// </summary>
    public class WindowManager
    {
        /// <summary>
        /// The height of the taskbar.
        /// </summary>
        internal const int TaskBarHeight = Taskbar.TaskBarHeight;

        //internal static Font WindowFont = FontManager.Instance.LoadFont(1, new System.IO.MemoryStream(EmbeddedFiles.Fonts.Vera10_bdf));
        /// <summary>
        /// The taskbar.
        /// </summary>
        private Taskbar Taskbar;
        /// <summary>
        /// The location of the mouse.
        /// </summary>
        public Vec2 MouseLocation = Vec2.Zero;
        /// <summary>
        /// The X location of the mouse.
        /// </summary>
        public int MouseX
        {
            get { return MouseLocation.X; }
        }
        /// <summary>
        /// The Y location of the mouse.
        /// </summary>
        public int MouseY
        {
            get { return MouseLocation.Y; }
        }
        /// <summary>
        /// Is true when all the windows need to be re-drawn,
        /// in other-words, is true if a window has been moved,
        /// resized, added, or removed.
        /// </summary>
        internal bool NeedToRedrawAll = false;
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
        /// <summary>
        /// The size of the screen.
        /// </summary>
        public Vec2 Size;

        /// <summary>
        /// The default constructor.
        /// </summary>
        public WindowManager(Vec2 size)
        {
            ActiveWindows = new Window[0];
            this.Size = size;
            this.Taskbar = new Taskbar(this);
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
                    if (ActiveWindows[0].CurrentState != WindowState.Minimized)
                    {
                        ActiveWindows[0].Draw(i);
                    }
                }
            }

            Taskbar.Draw(i);
        }

        /// <summary>
        /// Adds a window at the front.
        /// </summary>
        /// <param name="w">The window to add.</param>
        public void AddWindow(Window w)
        {
            InternalAddWindow(w);
            Taskbar.AddWindow(w);
        }

        /// <summary>
        /// Adds the specified window without modifying the taskbar.
        /// </summary>
        /// <param name="w">The window to add.</param>
        private void InternalAddWindow(Window w)
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
                    Taskbar.Modified = true;
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
                    InternalAddWindow(w);
                    NeedToRedrawAll = true;
                    Taskbar.Modified = true;
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
                    Taskbar.RemoveWindow(w);
                    return;
                }
            }
        }


        #region Handle Events

        #region Mouse Click
        /// <summary>
        /// Handles a MouseClick event.
        /// </summary>
        /// <param name="loc">The location of the mouse.</param>
        /// <param name="buttons">The MouseButtons that are pressed.</param>
        /// <param name="i">The image to draw to.</param>
        public void HandleMouseClick(Vec2 loc, MouseButtons buttons, Image i)
        {
            MouseLocation = loc;
            if (Taskbar.Bounds.IsInBounds(loc))
            {
                Taskbar.DoClick(loc, buttons);
            }
            else
            {
                if (Taskbar.WasOverButton)
                {
                    Taskbar.UndrawOverButton(Taskbar.WindowButtonBounds[Taskbar.overButtonIndx], Taskbar.Windows[Taskbar.overButtonIndx]);
                }
                foreach (Window w in ActiveWindows)
                {
                    if (w.Bounds.IsInBounds(loc))
                    {
                        w.DoClick(loc, OForms.MouseButtons.Left);
                        break;
                    }
                }
            }
            this.Draw(i);
        }
        #endregion

        #region Mouse Move
        /// <summary>
        /// Processes a MouseMove event.
        /// </summary>
        /// <param name="loc">The location of the mouse.</param>
        /// <param name="buttons">The buttons of the mouse that are pressed.</param>
        /// <param name="i">The image to draw to.</param>
        public void HandleMouseMove(Vec2 loc, MouseButtons buttons, Image i)
        {
            MouseLocation = loc;
            if (Taskbar.Bounds.IsInBounds(loc))
            {
                Taskbar.DoMouseMove(loc);
            }
            else
            {
                if (Taskbar.WasOverButton)
                {
                    Taskbar.UndrawOverButton(Taskbar.WindowButtonBounds[Taskbar.overButtonIndx], Taskbar.Windows[Taskbar.overButtonIndx]);
                }
                if (ActiveWindows.Length > 0)
                {
                    ActiveWindow.DoMouseMove(loc, buttons);
                }
            }
            this.Draw(i);
        }
        #endregion

        #region Mouse Down
        /// <summary>
        /// Processes a MouseDown event.
        /// </summary>
        /// <param name="loc">The location of the mouse.</param>
        /// <param name="buttons">The MouseButtons that are pressed.</param>
        /// <param name="i">The Image to draw to.</param>
        public void HandleMouseDown(Vec2 loc, MouseButtons buttons, Image i)
        {
            MouseLocation = loc;
            if (ActiveWindows.Length > 0)
            {
                if (ActiveWindow.Bounds.IsInBounds(loc))
                {
                    ActiveWindow.DoMouseDown(loc, buttons);
                    this.Draw(i);
                }
            }
        }
        #endregion

        #region Mouse Up
        /// <summary>
        /// Processes a MouseUp event.
        /// </summary>
        /// <param name="loc">The location of the mouse.</param>
        /// <param name="buttons">The MouseButtons that are still pressed.</param>
        /// <param name="i">The Image to draw to.</param>
        public void HandleMouseUp(Vec2 loc, MouseButtons buttons, Image i)
        {
            MouseLocation = loc;
            if (ActiveWindows.Length > 0)
            {
                ActiveWindow.DoMouseUp(loc, buttons);
                this.Draw(i);
            }
        }
        #endregion

        #endregion


    }
}
