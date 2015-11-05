using System;
using Orvid.Graphics;

namespace OForms.Windows
{
    /// <summary>
    /// The class that represents the Taskbar.
    /// </summary>
    internal class Taskbar
    {
        /// <summary>
        /// The internal buffer of the taskbar.
        /// </summary>
        private Image Buffer;
        /// <summary>
        /// All of the windows in the taskbar.
        /// </summary>
        public Window[] Windows;
        /// <summary>
        /// The parent window manager of the taskbar.
        /// </summary>
        private WindowManager Manager;
        /// <summary>
        /// The location of the taskbar on the window manager.
        /// </summary>
        private Vec2 TaskbarLocation;
        /// <summary>
        /// The bounds of the taskbar.
        /// </summary>
        public BoundingBox Bounds;
        /// <summary>
        /// The color to clear the back of the taskbar with.
        /// </summary>
        public Pixel TaskbarClearColor = Colors.Coral;
        /// <summary>
        /// The color to outline the taskbar with.
        /// </summary>
        public Pixel TaskbarOutlineColor = Colors.Crimson;
        /// <summary>
        /// The color to draw an inactive window's back in.
        /// </summary>
        public Pixel WindowInactiveBackColor = Colors.Brown;
        /// <summary>
        /// The color to draw an inactive window's outline in.
        /// </summary>
        public Pixel WindowInactiveLineColor = Colors.Blue;
        /// <summary>
        /// The color to draw the active window's back in.
        /// </summary>
        public Pixel WindowActiveBackColor = Colors.Green;
        /// <summary>
        /// The color to draw the active window's outline in.
        /// </summary>
        public Pixel WindowActiveLineColor = Colors.Black;
        /// <summary>
        /// The color to draw the back of an over active window's back in.
        /// </summary>
        public Pixel WindowActiveOverBackColor = Colors.CadetBlue;
        /// <summary>
        /// The color to draw the back of an over inactive window's back in.
        /// </summary>
        public Pixel WindowInactiveOverBackColor = Colors.Chocolate;
        /// <summary>
        /// The color to draw the text on the taskbar in.
        /// </summary>
        public Pixel TaskbarTextColor = Colors.Black;
        /// <summary>
        /// The default width of a button for a window.
        /// </summary>
        private const int WindowButtonWidth = 160;
        /// <summary>
        /// The margin around a window button.
        /// </summary>
        private const int WindowButtonMargin = 1;
        /// <summary>
        /// The height of the taskbar.
        /// </summary>
        public const int TaskBarHeight = 20;
        /// <summary>
        /// The maximum width for the text on a window button.
        /// </summary>
        public const int MaxTextWidth = WindowButtonWidth - 6;
        /// <summary>
        /// The bounds of all of the window buttons.
        /// </summary>
        internal BoundingBox[] WindowButtonBounds;
        /// <summary>
        /// The index of the button that the mouse is over.
        /// </summary>
        internal int overButtonIndx = 0;
        /// <summary>
        /// True if the over button has been drawn.
        /// </summary>
        private bool DrawnOverButton = false;
        /// <summary>
        /// True if the taskbar needs to be re-drawn.
        /// </summary>
        internal bool Modified = true;
        /// <summary>
        /// True if the mouse was over a window button.
        /// </summary>
        internal bool WasOverButton = false;
        /// <summary>
        /// True if the taskbar has been drawn since it was modified.
        /// </summary>
        private bool Drawn = false;


        /// <summary>
        /// The default constructor
        /// </summary>
        /// <param name="mangr">The parent window manager.</param>
        public Taskbar(WindowManager mangr)
        {
            this.Windows = new Window[0];
            this.Manager = mangr;
            this.Bounds = new BoundingBox(0, mangr.Size.X, mangr.Size.Y, mangr.Size.Y - TaskBarHeight);
            this.Buffer = new Image(mangr.Size.X, WindowManager.TaskBarHeight + 1);
            this.Buffer.Clear(TaskbarClearColor);
            this.WindowButtonBounds = new BoundingBox[0];
            this.TaskbarLocation = new Vec2(0, Manager.Size.Y - TaskBarHeight);
        }

        /// <summary>
        /// Adds the specified window to the taskbar.
        /// </summary>
        /// <param name="w">The window to add.</param>
        public void AddWindow(Window w)
        {
            Window[] tmp = new Window[Windows.Length + 1];
            Array.Copy(Windows, tmp, Windows.Length);
            tmp[tmp.Length - 1] = w;
            Windows = tmp;
            Modified = true;
            Drawn = false;
        }

        /// <summary>
        /// Removes the specified window from the taskbar.
        /// </summary>
        /// <param name="w">The window to remove.</param>
        public void RemoveWindow(Window w)
        {
            uint i;
            for (i = 0; i < Windows.Length; i++)
            {
                if (Windows[i] == w)
                {
                    break;
                }
            }
            Window[] tmp = new Window[Windows.Length - 1];
            Array.Copy(Windows, tmp, i);
            Array.Copy(Windows, i + 1, tmp, i, Windows.Length - i - 1);
            Windows = tmp;
            Modified = true;
            Drawn = false;
        }

        /// <summary>
        /// Redraws the Buffer.
        /// </summary>
        private void RedrawBuffer()
        {
            WindowButtonBounds = new BoundingBox[Windows.Length];
            Buffer.Clear(TaskbarClearColor);
            int loc = 20;
            if (Windows.Length * (WindowButtonWidth + (2 * WindowButtonMargin)) < Manager.Size.X - 20)
            {
                #region No Dynamic Size
                Vec2 tl;
                Vec2 tr;
                Vec2 br;
                Vec2 bl;
                Window w;
                for (uint ind = 0; ind < Windows.Length; ind++)
                {
                    w = Windows[ind];

                    tl = new Vec2(
                        loc + WindowButtonMargin,
                        (TaskBarHeight - (TaskBarHeight - 2 - WindowButtonMargin))
                    );
                    tr = new Vec2(
                        loc + WindowButtonMargin + WindowButtonWidth,
                        (TaskBarHeight - (TaskBarHeight - 2 - WindowButtonMargin))
                    );
                    br = new Vec2(
                        loc + WindowButtonMargin + WindowButtonWidth,
                        (TaskBarHeight - 2 - WindowButtonMargin)
                    );
                    bl = new Vec2(
                        loc + WindowButtonMargin,
                        (TaskBarHeight - 2 - WindowButtonMargin)
                    );

                    WindowButtonBounds[ind] = new BoundingBox(
                        loc + WindowButtonMargin,
                        loc + WindowButtonMargin + WindowButtonWidth,
                        Manager.Size.Y - (TaskBarHeight - (TaskBarHeight - 2 - WindowButtonMargin)),
                        Manager.Size.Y - (TaskBarHeight - 2 - WindowButtonMargin)
                    );

                    if (w.IsActiveWindow && w.CurrentState != WindowState.Minimized)
                    {
                        Buffer.DrawRectangle(tl, br, WindowActiveBackColor);
                        Buffer.DrawLines(new Vec2[] { tl, tr, br, bl, tl }, WindowActiveLineColor);
                    }
                    else
                    {
                        Buffer.DrawRectangle(tl, br, WindowInactiveBackColor);
                        Buffer.DrawLines(new Vec2[] { tl, tr, br, bl, tl }, WindowInactiveLineColor);
                    }

                    DrawWindowName(WindowButtonBounds[ind], w);

                    loc += WindowButtonMargin + WindowButtonMargin + WindowButtonWidth;
                }
                #endregion
            }
            else
            {
                #region Dynamic Size
                uint len = (uint)Manager.Size.X - 20;
                int ButtonWidth = (int)Math.Floor((double)((len / Windows.Length) - 2));
                if (ButtonWidth > 5)
                {
                    Vec2 tl;
                    Vec2 tr;
                    Vec2 br;
                    Vec2 bl;
                    Window w;
                    for (uint ind = 0; ind < Windows.Length; ind++)
                    {
                        w = Windows[ind];

                        tl = new Vec2(
                            loc + WindowButtonMargin,
                            (TaskBarHeight - (TaskBarHeight - 2 - WindowButtonMargin))
                        );
                        tr = new Vec2(
                            loc + WindowButtonMargin + ButtonWidth,
                            (TaskBarHeight - (TaskBarHeight - 2 - WindowButtonMargin))
                        );
                        br = new Vec2(
                            loc + WindowButtonMargin + ButtonWidth,
                            (TaskBarHeight - 2 - WindowButtonMargin)
                        );
                        bl = new Vec2(
                            loc + WindowButtonMargin,
                            (TaskBarHeight - 2 - WindowButtonMargin)
                        );

                        WindowButtonBounds[ind] = new BoundingBox(
                            loc + WindowButtonMargin,
                            loc + WindowButtonMargin + ButtonWidth,
                            Manager.Size.Y - (TaskBarHeight - (TaskBarHeight - 2 - WindowButtonMargin)),
                            Manager.Size.Y - (TaskBarHeight - 2 - WindowButtonMargin)
                        );

                        if (w.IsActiveWindow && w.CurrentState != WindowState.Minimized)
                        {
                            Buffer.DrawRectangle(tl, br, WindowActiveBackColor);
                            Buffer.DrawLines(new Vec2[] { tl, tr, br, bl, tl }, WindowActiveLineColor);
                        }
                        else
                        {
                            Buffer.DrawRectangle(tl, br, WindowInactiveBackColor);
                            Buffer.DrawLines(new Vec2[] { tl, tr, br, bl, tl }, WindowInactiveLineColor);
                        }

                        DrawWindowName(WindowButtonBounds[ind], w);

                        loc += WindowButtonMargin + WindowButtonMargin + ButtonWidth;
                    }
                }
                else
                {
                    Buffer.DrawRectangle(new Vec2(24, 4), new Vec2(Buffer.Width - 10, Buffer.Height - 4), Colors.Cyan);
                }
                #endregion
            }
            Modified = false;
            Drawn = true;
        }

        /// <summary>
        /// Draws the window's name on the taskbar,
        /// using the specified bounds.
        /// </summary>
        /// <param name="bounds">Bounds to use.</param>
        /// <param name="w">Window to draw.</param>
        private void DrawWindowName(BoundingBox bounds, Window w)
        {
            BoundingBox bnds = bounds - TaskbarLocation;
            //throw new Exception();
            //if (WindowManager.WindowFont.GetFontMetrics().StringWidth(w.Name) > bnds.Width - 6)
            //{
            //    // Doesn't fit on the button, need to remove some characters.
            //    string s = w.Name.Substring(0, w.Name.Length - 3) + "...";
            //    while (WindowManager.WindowFont.GetFontMetrics().StringWidth(s) > bnds.Width - 6)
            //    {
            //        if (s.Length == 3)
            //        {
            //            s = "."; // button is to small to have a name drawn.
            //            break;
            //        }
            //        else
            //        {
            //            // It's 4 to make up for the 3 extra characters we add.
            //            s = s.Substring(0, s.Length - 4) + "...";
            //        }
            //    }
            //    //throw new Exception();
            //    Buffer.DrawString(new Vec2(bnds.Left + 2, bnds.Bottom + 2), s, WindowManager.WindowFont, 10, Orvid.Graphics.FontSupport.FontStyle.Normal, TaskbarTextColor);
            //}
            //else // Fits on button.
            //{
            //    Buffer.DrawString(new Vec2(bnds.Left + 2, bnds.Bottom + 2), w.Name, WindowManager.WindowFont, 10, Orvid.Graphics.FontSupport.FontStyle.Normal, TaskbarTextColor);
            //}
        }

        /// <summary>
        /// Draws the taskbar on the specified image.
        /// </summary>
        /// <param name="i">The image to draw on.</param>
        public void Draw(Image i)
        {
            if (Modified)
            {
                RedrawBuffer();
            }
            i.DrawImage(TaskbarLocation, Buffer);
            Drawn = true;
        }

        /// <summary>
        /// Undraws the over WindowButton.
        /// </summary>
        /// <param name="bounds">Bounds of the window to undraw.</param>
        /// <param name="w">The window to undraw.</param>
        internal void UndrawOverButton(BoundingBox bounds, Window w)
        {
            Vec2 tl = new Vec2(bounds.Left, bounds.Bottom) - TaskbarLocation;
            Vec2 tr = new Vec2(bounds.Right, bounds.Bottom) - TaskbarLocation;
            Vec2 br = new Vec2(bounds.Right, bounds.Top) - TaskbarLocation;
            Vec2 bl = new Vec2(bounds.Left, bounds.Top) - TaskbarLocation;

            if (w.IsActiveWindow && w.CurrentState != WindowState.Minimized)
            {
                Buffer.DrawRectangle(tl, br, WindowActiveBackColor);
                Buffer.DrawLines(new Vec2[] { tl, tr, br, bl, tl }, WindowActiveLineColor);
            }
            else
            {
                Buffer.DrawRectangle(tl, br, WindowInactiveBackColor);
                Buffer.DrawLines(new Vec2[] { tl, tr, br, bl, tl }, WindowInactiveLineColor);
            }

            DrawWindowName(bounds, w);

            WasOverButton = false;
        }

        /// <summary>
        /// Draws the over WindowButton.
        /// </summary>
        /// <param name="bounds">The bounds of the window to draw.</param>
        /// <param name="w">The window to draw.</param>
        private void DrawOverButton(BoundingBox bounds, Window w)
        {
            Vec2 tl = new Vec2(bounds.Left, bounds.Bottom) - TaskbarLocation;
            Vec2 tr = new Vec2(bounds.Right, bounds.Bottom) - TaskbarLocation;
            Vec2 br = new Vec2(bounds.Right, bounds.Top) - TaskbarLocation;
            Vec2 bl = new Vec2(bounds.Left, bounds.Top) - TaskbarLocation;

            if (w.IsActiveWindow && w.CurrentState != WindowState.Minimized)
            {
                Buffer.DrawRectangle(tl, br, WindowActiveOverBackColor);
                Buffer.DrawLines(new Vec2[] { tl, tr, br, bl, tl }, WindowActiveLineColor);
            }
            else
            {
                Buffer.DrawRectangle(tl, br, WindowInactiveOverBackColor);
                Buffer.DrawLines(new Vec2[] { tl, tr, br, bl, tl }, WindowInactiveLineColor);
            }

            DrawWindowName(bounds, w);

            WasOverButton = true;
        }

        /// <summary>
        /// Processes the mouse move event.
        /// </summary>
        /// <param name="loc">Location of the mouse.</param>
        internal void DoMouseMove(Vec2 loc)
        {
            if (!Drawn)
            {
                RedrawBuffer();
            }
            if (!WasOverButton)
            {
                for (int i = 0; i < Windows.Length; i++)
                {
                    if (WindowButtonBounds[i].IsInBounds(loc))
                    {
                        DrawOverButton(WindowButtonBounds[i], Windows[i]);
                        overButtonIndx = i;
                        return;
                    }
                }
            }
            else
            {
                if (!WindowButtonBounds[overButtonIndx].IsInBounds(loc) || !DrawnOverButton)
                {
                    UndrawOverButton(WindowButtonBounds[overButtonIndx], Windows[overButtonIndx]);
                    for (int i = 0; i < Windows.Length; i++)
                    {
                        if (WindowButtonBounds[i].IsInBounds(loc))
                        {
                            DrawOverButton(WindowButtonBounds[i], Windows[i]);
                            overButtonIndx = i;
                            return;
                        }
                    }
                    overButtonIndx = 0;
                    DrawnOverButton = true;
                    return;
                }
            }
        }

        /// <summary>
        /// Processes a click event.
        /// </summary>
        /// <param name="loc">The location of the mouse.</param>
        /// <param name="buttons">The buttons that are pressed.</param>
        internal void DoClick(Vec2 loc, MouseButtons buttons)
        {
            if (!Drawn)
            {
                RedrawBuffer();
            }
            if (WasOverButton)
            {
                if (WindowButtonBounds[overButtonIndx].IsInBounds(loc))
                {
                    if (Windows[overButtonIndx].IsActiveWindow)
                    {
                        Manager.MinimizeWindow(Windows[overButtonIndx]);
                    }
                    else if (Windows[overButtonIndx].CurrentState == WindowState.Minimized)
                    {
                        Manager.RestoreWindow(Windows[overButtonIndx]);
                    }
                    else
                    {
                        Manager.BringWindowToFront(Windows[overButtonIndx]);
                    }
                    DrawnOverButton = false;
                    DoMouseMove(loc);
                }
                else
                {
                    for (int i = 0; i < Windows.Length; i++)
                    {
                        if (WindowButtonBounds[i].IsInBounds(loc))
                        {
                            DrawOverButton(WindowButtonBounds[i], Windows[i]);
                            overButtonIndx = i;
                            DrawnOverButton = false;
                            if (Windows[i].IsActiveWindow)
                            {
                                Manager.MinimizeWindow(Windows[i]);
                            }
                            else if (Windows[i].CurrentState == WindowState.Minimized)
                            {
                                Manager.RestoreWindow(Windows[i]);
                            }
                            else
                            {
                                Manager.BringWindowToFront(Windows[i]);
                            }
                            DoMouseMove(loc);
                            return;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < Windows.Length; i++)
                {
                    if (WindowButtonBounds[i].IsInBounds(loc))
                    {
                        DrawOverButton(WindowButtonBounds[i], Windows[i]);
                        overButtonIndx = i;
                        DrawnOverButton = false;
                        if (Windows[i].IsActiveWindow)
                        {
                            Manager.MinimizeWindow(Windows[i]);
                        }
                        else if (Windows[i].CurrentState == WindowState.Minimized)
                        {
                            Manager.RestoreWindow(Windows[i]);
                        }
                        else
                        {
                            Manager.BringWindowToFront(Windows[i]);
                        }
                        DoMouseMove(loc);
                        return;
                    }
                }
            }
        }
    }
}
