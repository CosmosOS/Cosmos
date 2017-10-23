using System;
using Orvid.Graphics;
using OForms.Controls;
using System.Collections.Generic;

namespace OForms.Windows
{
    /// <summary>
    /// The class that represents a single window.
    /// </summary>
    public class Window : IDisposable
    {
        /// <summary>
        /// The name of the window.
        /// </summary>
        public string Name;
        /// <summary>
        /// The parent WindowManager of this window.
        /// </summary>
        public WindowManager Parent;
        /// <summary>
        /// The location of the window. Use this internally only
        /// if you are reading the location. If you are changing 
        /// the location, use the public Location property instead, 
        /// so that the bounds, get properly reset.
        /// </summary>
        private Vec2 iLocation;
        /// <summary>
        /// The location of the window.
        /// </summary>
        public Vec2 Location
        {
            get
            {
                return iLocation;
            }
            set
            {
                Parent.NeedToRedrawAll = true;
                iLocation = value;
                ComputeBounds();
                // Doesn't change the physical size,
                // so buffers don't get reset.
            }
        }
        /// <summary>
        /// The minimum allowable window size in the X direction.
        /// </summary>
        private const int MinXWindowSize = 50;
        /// <summary>
        /// The minimum allowable window size in the Y direction.
        /// </summary>
        private const int MinYWindowSize = 40;
        /// <summary>
        /// The size of the window. Use this internally only
        /// if you are reading the size. If you are changing 
        /// the size, use the public Size property instead, 
        /// so that the bounds, and buffers get properly reset.
        /// </summary>
        private Vec2 iSize;
        /// <summary>
        /// The size of the window.
        /// </summary>
        public Vec2 Size
        {
            get
            {
                return iSize;
            }
            set
            {
                Parent.NeedToRedrawAll = true;
                if (value.X < MinXWindowSize)
                {
                    value.X = MinXWindowSize;
                }
                if (value.Y < MinYWindowSize)
                {
                    value.Y = MinYWindowSize;
                }
                iSize = value;
                ComputeBounds();
                ResetBuffers();
            }
        }
        /// <summary>
        /// The Bounds of this window.
        /// </summary>
        public BoundingBox Bounds;
        /// <summary>
        /// The BoundingBox for the window's contents.
        /// </summary>
        private BoundingBox ContentBounds;
        /// <summary>
        /// The BoundingBox for the header of the window.
        /// </summary>
        private BoundingBox HeaderBounds;
        /// <summary>
        /// The BoundingBox for the Close button.
        /// </summary>
        private BoundingBox CloseButtonBounds;
        /// <summary>
        /// The BoundingBox for the Maximize/Restore button.
        /// </summary>
        private BoundingBox MaxButtonBounds;
        /// <summary>
        /// The BoundingBox for the Minimize button.
        /// </summary>
        private BoundingBox MinButtonBounds;
        /// <summary>
        /// The height of the header. (in pixels)
        /// </summary>
        private const int HeaderHeight = 15;
        /// <summary>
        /// The thickness of the window border. (in pixels)
        /// </summary>
        private const int WindowBorderSize = 1;
        /// <summary>
        /// The overall buffer for the window.
        /// </summary>
        private Image WindowBuffer;
        /// <summary>
        /// The buffer for the content of the window.
        /// </summary>
        private Image ContentBuffer;
        /// <summary>
        /// The buffer for the header of the window.
        /// </summary>
        private Image HeaderBuffer;
        /// <summary>
        /// True if this is the currently active window.
        /// </summary>
        private bool isActiveWindow = false;  
        /// <summary>
        /// True if this is the currently active window.
        /// </summary>
        public bool IsActiveWindow
        {
            get
            {
                return isActiveWindow;
            }
            set
            {
                if (value != isActiveWindow)
                {
                    if (value)
                    {
                        FadingIn = true;
                    }
                    isActiveWindow = value;
                }
            }
        }
        /// <summary>
        /// True if the window is in the process of being dragged.
        /// </summary>
        private bool IsDragging = false;
        /// <summary>
        /// True if the window should fade in when selected.
        /// </summary>
        public bool ShouldFadeIn = true;
        /// <summary>
        /// True if we are fading in.
        /// </summary>
        private bool FadingIn = false;
        /// <summary>
        /// The current state of the window. 
        /// Don't use this field, use CurrentState
        /// instead.
        /// </summary>
        private WindowState iCurrentState = WindowState.Normal;
        /// <summary>
        /// The current WindowState of the window.
        /// </summary>
        public WindowState CurrentState
        {
            get
            {
                return iCurrentState;
            }
            set
            {
                if (iCurrentState != value)
                {
                    if (iCurrentState == WindowState.Maximized)
                    {
                        if (value == WindowState.Minimized)
                        {
                            WasMaximized = true;
                            iCurrentState = value;
                        }
                        else // It means window state is normal.
                        {
                            WasMaximized = false;
                            this.Location = PrevLoc;
                            this.Size = PrevSize;
                            iCurrentState = value;
                        }
                    }
                    // Should only be set to normal from the minimized state.
                    else if (iCurrentState == WindowState.Minimized)
                    {
                        if (WasMaximized)
                        {
                            iCurrentState = WindowState.Maximized;
                        }
                        else
                        {
                            iCurrentState = value;
                        }
                    }
                    else if (iCurrentState == WindowState.Normal)
                    {
                        if (value == WindowState.Maximized)
                        {
                            PrevLoc = iLocation;
                            this.Location = Vec2.Zero;
                            PrevSize = iSize;
                            this.Size = new Vec2(Parent.Size.X, Parent.Size.Y - WindowManager.TaskBarHeight);
                            iCurrentState = WindowState.Maximized;
                        }
                        else
                        {
                            iCurrentState = value;
                        }
                    }
                    else
                    {
                        throw new Exception("Unknown WindowState!");
                    }
                    ResetBuffers();
                    ComputeBounds();
                }
            }
        }
        /// <summary>
        /// Location of the window before it was maximized.
        /// </summary>
        private Vec2 PrevLoc;
        /// <summary>
        /// Size of the window before it was maximized.
        /// </summary>
        private Vec2 PrevSize;
        /// <summary>
        /// True if the window was Maximized when the window got Minimized.
        /// </summary>
        private bool WasMaximized = false;
        /// <summary>
        /// True if we're currently resizing.
        /// </summary>
        private bool IsResizing = false;
        /// <summary>
        /// The size of the window when resizing started.
        /// </summary>
        private Vec2 InitSizeOnResizeStart;
        /// <summary>
        /// The location of the mouse when resizing started.
        /// </summary>
        private Vec2 InitResizeLocation;
        /// <summary>
        /// The location of the window when window dragging started.
        /// </summary>
        private Vec2 InitWindowLocOnDragStart;
        /// <summary>
        /// The location of the mouse when window dragging started.
        /// </summary>
        private Vec2 InitDraggingLocation;
        /// <summary>
        /// True if the last mouse move event had the mouse in
        /// the window header.
        /// </summary>
        private bool WasInHeader = false;
        /// <summary>
        /// True if the last mouse move event had the mouse
        /// over the Close button.
        /// </summary>
        private bool WasOverClose = false;
        /// <summary>
        /// True if the last mouse move event had the mouse
        /// over the Maximize/Restore button.
        /// </summary>
        private bool WasOverMax = false;
        /// <summary>
        /// True if the last mouse move event had the mouse
        /// over the Minimize button.
        /// </summary>
        private bool WasOverMin = false;
        /// <summary>
        /// The color to clear the ContentBuffer with when the window is inactive.
        /// </summary>
        public Pixel ClearInactiveColor = Colors.Brown;
        /// <summary>
        /// The color to clear the ContentBuffer with when the window is active.
        /// </summary>
        public Pixel ClearColor = Colors.Brown;
        /// <summary>
        /// True if the header has been drawn.
        /// </summary>
        private bool DrawnHeader = false;
        /// <summary>
        /// The controls in the window.
        /// </summary>
        public List<Control> Controls = new List<Control>();


        #region Colors
        /// <summary>
        /// The current background color of the Close button.
        /// </summary>
        private Pixel CurCloseButtonColor = Colors.Red;
        /// <summary>
        /// The current background color of the Maximize/Restore button.
        /// </summary>
        private Pixel CurMaxButtonColor = Colors.Red;
        /// <summary>
        /// The current background color of the Minimize button.
        /// </summary>
        private Pixel CurMinButtonColor = Colors.Red;

        #region Default Colors
        /// <summary>
        /// The default background color of the Close button.
        /// </summary>
        private static Pixel DefaultCloseButtonColor = Colors.Red;
        /// <summary>
        /// The default background color of the Maximize/Restore button.
        /// </summary>
        private static Pixel DefaultMaxButtonColor = Colors.Red;
        /// <summary>
        /// The default background color of the Minimize button.
        /// </summary>
        private static Pixel DefaultMinButtonColor = Colors.Red;
        #endregion

        #endregion


        /// <summary>
        /// The default constructor for a window.
        /// </summary>
        /// <param name="loc">The initial location of the window.</param>
        /// <param name="size">The size of the window.</param>
        /// <param name="name">The name of the window.</param>
        public Window(Vec2 loc, Vec2 size, string name)
        {
            iLocation = loc;
            iSize = size;
            Name = name;
            ResetBuffers();
            ComputeBounds();
        }

        /// <summary>
        /// Gets a string representing this window.
        /// </summary>
        /// <returns>The window as a string.</returns>
        public override string ToString()
        {
            return this.Name;
        }

        /// <summary>
        /// Closes this window.
        /// </summary>
        public void Close()
        {
            Parent.CloseWindow(this);
        }

        /// <summary>
        /// Does the actual closing of this window.
        /// </summary>
        internal void DoClose()
        {
            foreach (Control c in Controls)
            {
                c.DoClosing();
            }
            this.Dispose();
        }

        /// <summary>
        /// Disposes of all of the resources used by this window.
        /// </summary>
        public void Dispose()
        {
            this.Bounds = null;
            this.CloseButtonBounds = null;
            this.ContentBounds = null;
            this.ContentBuffer = null;
            foreach (Control c in Controls)
            {
                c.DoBeforeDispose();
                c.Dispose();
                c.DoAfterDispose();
            }
            this.Controls = null;
            this.HeaderBounds = null;
            this.HeaderBuffer = null;
            this.MaxButtonBounds = null;
            this.MinButtonBounds = null;
            this.Parent = null;
            this.WindowBuffer = null;
            this.Name = null;
        }

        /// <summary>
        /// Resets all of the buffers, and redraws the HeaderBuffer.
        /// </summary>
        private void ResetBuffers()
        {
            WindowBuffer = new Image(iSize);
            WindowBuffer.Clear(Colors.BurlyWood);
            DrawnHeader = false;

            RedrawHeader(); // Draw the header.

            ContentBuffer = new Image(new Vec2(iSize.X - WindowBorderSize - WindowBorderSize, iSize.Y - HeaderHeight));
        }

        /// <summary>
        /// Redraws the header.
        /// </summary>
        private void RedrawHeader()
        {
            HeaderBuffer = new Image(new Vec2(iSize.X, HeaderHeight));
            HeaderBuffer.Clear(Colors.BlueViolet);
            DrawnHeader = false;

            //HeaderBuffer.DrawString(new Vec2(3, 3), Name, WindowManager.WindowFont, 10, Orvid.Graphics.FontSupport.FontStyle.Normal, Colors.Black);

            RedrawCloseButton();
            RedrawMaxRestButton();
            RedrawMinButton();
        }

        #region Draw Close Button
        /// <summary>
        /// Re-Draws the Close button on the HeaderBuffer.
        /// </summary>
        private void RedrawCloseButton()
        {
            Vec2 tl = new Vec2(iSize.X - HeaderHeight + 2, WindowBorderSize + 1);
            Vec2 tr = new Vec2(iSize.X - WindowBorderSize - 2, WindowBorderSize + 1);
            Vec2 br = new Vec2(iSize.X - WindowBorderSize - 2, HeaderHeight - 2);
            Vec2 bl = new Vec2(iSize.X - HeaderHeight + 2, HeaderHeight - 2);
            HeaderBuffer.DrawRectangle(tl, br, CurCloseButtonColor);
            HeaderBuffer.DrawLines(new Vec2[] { tl, tr, br, bl, tl }, Colors.Green);
            HeaderBuffer.DrawLine(new Vec2(iSize.X - WindowBorderSize - 4, WindowBorderSize + 3), new Vec2(iSize.X - HeaderHeight + 4, HeaderHeight - 4), Colors.Green);
            HeaderBuffer.DrawLine(new Vec2(iSize.X - HeaderHeight + 4, WindowBorderSize + 3), new Vec2(iSize.X - WindowBorderSize - 4, HeaderHeight - 4), Colors.Green);
            DrawnHeader = false;
        }
        #endregion

        #region Draw Maximize/Restore Button
        /// <summary>
        /// Re-Draws the Maximize/Restore button on the HeaderBuffer.
        /// </summary>
        private void RedrawMaxRestButton()
        {
            Vec2 tl = new Vec2(iSize.X - (HeaderHeight + HeaderHeight - 2), WindowBorderSize + 1);
            Vec2 tr = new Vec2(iSize.X - (HeaderHeight + WindowBorderSize + 2), WindowBorderSize + 1);
            Vec2 br = new Vec2(iSize.X - (HeaderHeight + WindowBorderSize + 2), HeaderHeight - 2);
            Vec2 bl = new Vec2(iSize.X - (HeaderHeight + HeaderHeight - 2), HeaderHeight - 2);
            if (CurrentState == WindowState.Maximized) // Draw Restore Button.
            {
                #region Draw Restore Button
                HeaderBuffer.DrawRectangle(tl, br, CurMaxButtonColor);
                HeaderBuffer.DrawLines(new Vec2[] { tl, tr, br, bl, tl }, Colors.Green);
                HeaderBuffer.DrawLines(new Vec2[] {
                    new Vec2(tl.X + 4, tl.Y + 2),
                    new Vec2(tr.X - 2, tr.Y + 2),
                    new Vec2(tr.X - 2, tr.Y + 3),
                    new Vec2(tl.X + 4, tl.Y + 3),
                    new Vec2(tl.X + 4, tl.Y + 5),
                    new Vec2(tl.X + 2, tl.Y + 5),
                    new Vec2(tl.X + 2, bl.Y - 2),
                    new Vec2(tr.X - 4, bl.Y - 2),
                    new Vec2(tr.X - 4, tl.Y + 5),
                    new Vec2(tl.X + 4, tl.Y + 5),
                    new Vec2(tl.X + 4, tl.Y + 6),
                    new Vec2(tl.X + 2, tl.Y + 6),
                    new Vec2(tr.X - 4, tl.Y + 6),
                    new Vec2(tr.X - 4, tl.Y + 7),
                    new Vec2(tr.X - 2, tl.Y + 7),
                    new Vec2(tr.X - 2, tl.Y + 2),
                }, Colors.Green);
                #endregion
            }
            else // Draw Maximize Button.
            {
                #region Draw Maximize Button
                HeaderBuffer.DrawRectangle(tl, br, CurMaxButtonColor);
                HeaderBuffer.DrawLines(new Vec2[] { tl, tr, br, bl, tl }, Colors.Green);
                HeaderBuffer.DrawLines(new Vec2[] {
                    new Vec2(tl.X + 2, tl.Y + 2),
                    new Vec2(tr.X - 2, tr.Y + 2),
                    new Vec2(br.X - 2, br.Y - 2),
                    new Vec2(bl.X + 2, bl.Y - 2),
                    new Vec2(tl.X + 2, tl.Y + 2),
                }, Colors.Green);
                HeaderBuffer.DrawLine(new Vec2(tl.X + 2, tl.Y + 3), new Vec2(tr.X - 2, tr.Y + 3), Colors.Green);
                #endregion
            }
            DrawnHeader = false;
        }
        #endregion

        #region Redraw Minimize Button
        /// <summary>
        /// Re-Draws the Minimize button on the HeaderBuffer.
        /// </summary>
        private void RedrawMinButton()
        {
            Vec2 tl = new Vec2(iSize.X - (HeaderHeight + HeaderHeight + HeaderHeight - 2), WindowBorderSize + 1);
            Vec2 tr = new Vec2(iSize.X - (HeaderHeight + HeaderHeight + WindowBorderSize + 2), WindowBorderSize + 1);
            Vec2 br = new Vec2(iSize.X - (HeaderHeight + HeaderHeight + WindowBorderSize + 2), HeaderHeight - 2);
            Vec2 bl = new Vec2(iSize.X - (HeaderHeight + HeaderHeight + HeaderHeight - 2), HeaderHeight - 2);
            HeaderBuffer.DrawRectangle(tl, br, CurMinButtonColor);
            HeaderBuffer.DrawLines(new Vec2[] { tl, tr, br, bl, tl }, Colors.Green);
            HeaderBuffer.DrawLine(new Vec2(br.X - 3, br.Y - 2), new Vec2(bl.X + 3, bl.Y - 2), Colors.Green);
            HeaderBuffer.DrawLine(new Vec2(br.X - 3, br.Y - 3), new Vec2(bl.X + 3, bl.Y - 3), Colors.Green);
            DrawnHeader = false;
        }
        #endregion

        /// <summary>
        /// Re-Computes all of the bounding boxes.
        /// </summary>
        private void ComputeBounds()
        {
            this.Bounds = new BoundingBox(
                iLocation.X,
                iLocation.X + iSize.X,
                iLocation.Y + iSize.Y,
                iLocation.Y
            );
            this.HeaderBounds = new BoundingBox(
                iLocation.X,
                iLocation.X + iSize.X,
                iLocation.Y + HeaderHeight,
                iLocation.Y
            );
            this.ContentBounds = new BoundingBox(
                iLocation.X + WindowBorderSize,
                iLocation.X + (iSize.X - WindowBorderSize - WindowBorderSize),
                iLocation.Y + (iSize.Y - HeaderHeight),
                iLocation.Y + HeaderHeight
            );

            this.CloseButtonBounds = new BoundingBox(
                iLocation.X + (iSize.X - HeaderHeight + 2),
                iLocation.X + (iSize.X - WindowBorderSize - 2),
                iLocation.Y + (HeaderHeight - 2),
                iLocation.Y + (WindowBorderSize + 1)
            );
            this.MaxButtonBounds = new BoundingBox(
                iLocation.X + (iSize.X - (HeaderHeight + HeaderHeight - 2)),
                iLocation.X + (iSize.X - (HeaderHeight + WindowBorderSize + 2)),
                iLocation.Y + (HeaderHeight - 2),
                iLocation.Y + (WindowBorderSize + 1)
            );
            this.MinButtonBounds = new BoundingBox(
                iLocation.X + (iSize.X - (HeaderHeight + HeaderHeight + HeaderHeight - 2)),
                iLocation.X + (iSize.X - (HeaderHeight + HeaderHeight + WindowBorderSize + 2)),
                iLocation.Y + (HeaderHeight - 2),
                iLocation.Y + (WindowBorderSize + 1)
            );
        }

        /// <summary>
        /// Draws this window on the specified image.
        /// </summary>
        /// <param name="i">The image to draw the window to.</param>
        internal void Draw(Image i)
        {
            if (!DrawnHeader)
            {
                WindowBuffer.DrawImage(Vec2.Zero, HeaderBuffer);
                WindowBuffer.DrawLine(Vec2.Zero, new Vec2(iSize.X - WindowBorderSize, 0), Colors.Black);
                WindowBuffer.DrawLine(new Vec2(iSize.X - WindowBorderSize, 0), iSize - WindowBorderSize, Colors.Black);
                WindowBuffer.DrawLine(new Vec2(0, iSize.Y - WindowBorderSize), Vec2.Zero, Colors.Black);
                DrawnHeader = true;
            }

            if (IsActiveWindow)
            {
                if (FadingIn)
                {
                    ContentBuffer.Clear(new Pixel(ClearColor.R, ClearColor.G, ClearColor.B, 128));
                    FadingIn = false;
                }
                else
                {
                    ContentBuffer.Clear(ClearColor);
                }
            }
            else
            {
                ContentBuffer.Clear(ClearInactiveColor);
            }
            foreach (Control c in Controls)
            {
                c.Draw(ContentBuffer);
            }
            WindowBuffer.DrawImage(new Vec2(WindowBorderSize, HeaderHeight), ContentBuffer);
            WindowBuffer.DrawLine(iSize - WindowBorderSize, new Vec2(0, iSize.Y - WindowBorderSize), Colors.Black);
            WindowBuffer.DrawLine(new Vec2(WindowBorderSize, HeaderHeight), new Vec2(iSize.X - WindowBorderSize - 1, HeaderHeight), new Pixel(214, 211, 206, 255));

            i.DrawImage(iLocation, WindowBuffer);
        }


        #region Do Events
        /// <summary>
        /// Processes a MouseClick event.
        /// </summary>
        /// <param name="loc">The location of the mouse.</param>
        /// <param name="button">The buttons that are pressed.</param>
        internal void DoClick(Vec2 loc, MouseButtons button)
        {
            if (IsActiveWindow)
            {
                if (ContentBounds.IsInBounds(loc))
                {
                    Vec2 RelativeLoc;
                    foreach (Control c in Controls)
                    {
                        RelativeLoc = new Vec2(loc.X - Location.X - WindowBorderSize, loc.Y - Location.Y - HeaderHeight);
                        if (c.Bounds.IsInBounds(RelativeLoc))
                        {
                            c.DoClick(RelativeLoc, button);
                        }
                    }
                }
                else
                {
                    if (HeaderBounds.IsInBounds(loc))
                    {
                        if (CloseButtonBounds.IsInBounds(loc))
                        {
                            this.Close();
                        }
                        else if (MaxButtonBounds.IsInBounds(loc))
                        {
                            if (CurrentState == WindowState.Maximized)
                            {
                                Parent.RestoreWindow(this);
                            }
                            else
                            {
                                Parent.MaximizeWindow(this);
                            }
                        }
                        else if (MinButtonBounds.IsInBounds(loc))
                        {
                            Parent.MinimizeWindow(this);
                        }
                    }
                    else // Window border was clicked, and we don't care about it.
                    {
                        //throw new Exception("Unknown part of the window clicked!");
                    }
                }
            }
            else
            {
                Parent.BringWindowToFront(this);
            }
        }

        /// <summary>
        /// Processes a MouseUp event.
        /// </summary>
        /// <param name="loc">The location of the mouse.</param>
        /// <param name="button">The buttons that are still pressed down.</param>
        internal void DoMouseUp(Vec2 loc, MouseButtons button)
        {
            if (IsDragging)
            {
                IsDragging = false;
                InitDraggingLocation = null;
                InitWindowLocOnDragStart = null;
            }
            else if (IsResizing)
            {
                IsResizing = false;
                InitResizeLocation = null;
                InitSizeOnResizeStart = null;
            }
            else
            {
                Vec2 RelativeLoc;
                foreach (Control c in Controls)
                {
                    if (c.IsMouseDown)
                    {
                        RelativeLoc = new Vec2(loc.X - Location.X - WindowBorderSize, loc.Y - Location.Y - HeaderHeight);
                        c.IsMouseDown = false;
                        c.DoMouseUp(RelativeLoc, button);
                    }
                }
            }
        }

        /// <summary>
        /// Processes a MouseDown event.
        /// </summary>
        /// <param name="loc">The location of the mouse.</param>
        /// <param name="button">The buttons that are down.</param>
        internal void DoMouseDown(Vec2 loc, MouseButtons button)
        {
            if (ContentBounds.IsInBounds(loc))
            {
                Vec2 RelativeLoc;
                foreach (Control c in Controls)
                {
                    RelativeLoc = new Vec2(loc.X - Location.X - WindowBorderSize, loc.Y - Location.Y - HeaderHeight);
                    if (c.Bounds.IsInBounds(RelativeLoc))
                    {
                        c.IsMouseDown = true;
                        c.DoMouseDown(RelativeLoc, button);
                    }
                }
            }
            else
            {
                if (HeaderBounds.IsInBounds(loc))
                {
                    IsDragging = true;
                    InitDraggingLocation = loc;
                    InitWindowLocOnDragStart = this.iLocation;
                }
                else // The border of the window was pressed, begin resize.
                {
                    IsResizing = true;
                    InitSizeOnResizeStart = this.iSize;
                    InitResizeLocation = loc;
                }
            }
        }



        /// <summary>
        /// Checks if we were over buttons,
        /// and reset their colors if needed.
        /// </summary>
        private void CheckOldButtons()
        {
            if (WasOverClose)
            {
                CurCloseButtonColor = DefaultCloseButtonColor;
                WasOverClose = false;
                RedrawCloseButton();
            }
            else if (WasOverMax)
            {
                CurMaxButtonColor = DefaultMaxButtonColor;
                WasOverMax = false;
                RedrawMaxRestButton();
            }
            else if (WasOverMin)
            {
                CurMinButtonColor = DefaultMinButtonColor;
                WasOverMin = false;
                RedrawMinButton();
            }
        }

        /// <summary>
        /// Processes a MouseMove event.
        /// </summary>
        /// <param name="newLoc">The new location of the mouse.</param>
        /// <param name="button">The buttons on the mouse that are pressed.</param>
        internal void DoMouseMove(Vec2 newLoc, MouseButtons button)
        {
            if (IsDragging)
            {
                Vec2 Transform = newLoc - InitDraggingLocation;
                this.Location = InitWindowLocOnDragStart + Transform;
            }
            else if (IsResizing)
            {
                Vec2 Transform = newLoc - InitResizeLocation;
                this.Size = InitSizeOnResizeStart + Transform;
            }
            else
            {
                if (WasInHeader)
                {
                    if (HeaderBounds.IsInBounds(newLoc))
                    {
                        if (CloseButtonBounds.IsInBounds(newLoc))
                        {
                            if (!WasOverClose)
                            {
                                CheckOldButtons();
                                CurCloseButtonColor = Colors.Brown;
                                WasOverClose = true;
                                RedrawCloseButton();
                                return;
                            }
                            // Otherwise we've already done whats 
                            // needed for being over the close button.
                            return;
                        }
                        else if (MaxButtonBounds.IsInBounds(newLoc))
                        {
                            if (!WasOverMax)
                            {
                                CheckOldButtons();
                                CurMaxButtonColor = Colors.Brown;
                                WasOverMax = true;
                                RedrawMaxRestButton();
                                return;
                            }
                            // Otherwise we've already done whats 
                            // needed for being over the maximize/restore button.
                            return;
                        }
                        else if (MinButtonBounds.IsInBounds(newLoc))
                        {
                            if (!WasOverMin)
                            {
                                CheckOldButtons();
                                CurMinButtonColor = Colors.Brown;
                                WasOverMin = true;
                                RedrawMinButton();
                                return;
                            }
                            // Otherwise we've already done whats 
                            // needed for being over the minimize button.
                            return;
                        }
                        else
                        {
                            CheckOldButtons();
                        }
                    }
                    else // It's not in the header anymore.
                    {
                        CheckOldButtons();
                        WasInHeader = false;
                    }
                }
                if (ContentBounds.IsInBounds(newLoc))
                {
                    Vec2 RelativeLoc;
                    foreach (Control c in Controls)
                    {
                        RelativeLoc = new Vec2(newLoc.X - Location.X - WindowBorderSize, newLoc.Y - Location.Y - HeaderHeight);
                        if (!c.IsIn)
                        {
                            if (c.Bounds.IsInBounds(RelativeLoc))
                            {
                                c.IsIn = true;
                                c.DoMouseEnter(RelativeLoc, button);
                            }
                        }
                        else
                        {
                            if (!c.Bounds.IsInBounds(RelativeLoc))
                            {
                                c.IsIn = false;
                                c.DoMouseLeave(RelativeLoc, button);
                            }
                        }
                    }
                }
                else if (HeaderBounds.IsInBounds(newLoc))
                {
                    WasInHeader = true;
                    if (CloseButtonBounds.IsInBounds(newLoc))
                    {
                        CurCloseButtonColor = Colors.Brown;
                        WasOverClose = true;
                        RedrawCloseButton();
                    }
                    else if (MaxButtonBounds.IsInBounds(newLoc))
                    {
                        CurMaxButtonColor = Colors.Brown;
                        WasOverMax = true;
                        RedrawMaxRestButton();
                    }
                    else if (MinButtonBounds.IsInBounds(newLoc))
                    {
                        CurMinButtonColor = Colors.Brown;
                        WasOverMin = true;
                        RedrawMinButton();
                    }
                }
                else // the mouse was in the window border.
                {

                }
            }
        }
        #endregion

        public static bool operator ==(Window w, Window w2)
        {
            if (w.Name == w2.Name && w.Size == w2.Size && w.Location == w2.Location)
            {
                return true;
            }
            return false;
        }

        public static bool operator !=(Window w, Window w2)
        {
            return ((w == w2) == false);
        }

        public override bool Equals(object obj)
        {
            return (this == (Window)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
