using System;
using Orvid.Graphics;
using System.Collections.Generic;

namespace OForms.Controls
{
    /// <summary>
    /// An abstract class that represents a Control and it's Children.
    /// </summary>
    public abstract class Control : IDisposable
    {
        #region Events
        /// <summary>
        /// The event that occurs when the control is clicked.
        /// </summary>
        public event MouseEvent Click;
        /// <summary>
        /// The event that occurs when a mouse 
        /// enters the bounds of the control.
        /// </summary>
        public event MouseEvent MouseEnter;
        /// <summary>
        /// The event that occurs when a mouse
        /// leaves the bounds of the control.
        /// </summary>
        public event MouseEvent MouseLeave;
        /// <summary>
        /// The event that occurs when a mouse 
        /// button is pressed down over the control.
        /// </summary>
        public event MouseEvent MouseDown;
        /// <summary>
        /// The event that occurs when a mouse 
        /// button is released, but only if 
        /// the MouseDown event was handled 
        /// by this control.
        /// </summary>
        public event MouseEvent MouseUp;
        /// <summary>
        /// The event that occurs just before this
        /// control is disposed of.
        /// </summary>
        public event DisposingEvent BeforeDispose;
        /// <summary>
        /// The event that occurs directly after 
        /// this control is disposed of.
        /// </summary>
        public event DisposingEvent AfterDispose;
        /// <summary>
        /// This event occurs directly before the 
        /// window containing this control is closed.
        /// </summary>
        public event DisposingEvent Closing;

        /// <summary>
        /// The dummy MouseEvent method.
        /// </summary>
        /// <param name="loc">The location of the mouse.</param>
        /// <param name="button">The current state of the mouse buttons.</param>
        private void DummyMouseEvent(Vec2 loc, MouseButtons button) { }
        /// <summary>
        /// The dummy DisposingEvent method.
        /// </summary>
        private void DummyDisposingEvent() { }
        /// <summary>
        /// Invokes the Click event.
        /// </summary>
        /// <param name="v">Location of the mouse.</param>
        /// <param name="b">The current state of the mouse buttons.</param>
        internal void DoClick(Vec2 v, MouseButtons b) { Click.Invoke(v, b); }
        /// <summary>
        /// Invokes the MouseEnter event.
        /// </summary>
        /// <param name="v">Location of the mouse.</param>
        /// <param name="b">The current state of the mouse buttons.</param>
        internal void DoMouseEnter(Vec2 v, MouseButtons b) { MouseEnter.Invoke(v, b); }
        /// <summary>
        /// Invokes the MouseLeave event.
        /// </summary>
        /// <param name="v">Location of the mouse.</param>
        /// <param name="b">The current state of the mouse buttons.</param>
        internal void DoMouseLeave(Vec2 v, MouseButtons b) { MouseLeave.Invoke(v, b); }
        /// <summary>
        /// Invokes the MouseDown event.
        /// </summary>
        /// <param name="v">Location of the mouse.</param>
        /// <param name="b">The current state of the mouse buttons.</param>
        internal void DoMouseDown(Vec2 v, MouseButtons b) { MouseDown.Invoke(v, b); }
        /// <summary>
        /// Invokes the MouseUp event.
        /// </summary>
        /// <param name="v">Location of the mouse.</param>
        /// <param name="b">The current state of the mouse buttons.</param>
        internal void DoMouseUp(Vec2 v, MouseButtons b) { MouseUp.Invoke(v, b); }
        /// <summary>
        /// Invokes the BeforeDispose event.
        /// </summary>
        internal void DoBeforeDispose() { BeforeDispose.Invoke(); }
        /// <summary>
        /// Invokes the AfterDispose event.
        /// </summary>
        internal void DoAfterDispose() { AfterDispose.Invoke(); }
        /// <summary>
        /// Invokes the Closing event.
        /// </summary>
        internal void DoClosing() { Closing.Invoke(); }
        #endregion


        public object Parent;

        private Vec2 iSize;
        /// <summary>
        /// The size of the control.
        /// </summary>
        public Vec2 Size
        {
            get
            {
                return iSize;
            }
            set
            {
                iSize = value;
            }
        }

        private int iX;
        /// <summary>
        /// The X location of this control in it's parent container.
        /// </summary>
        public int X
        {
            get
            {
                return iX;
            }
            set
            {
                iX = value;
            }
        }
        
        private int iY;
        /// <summary>
        /// The Y location of this control in it's parent container.
        /// </summary>
        public int Y
        {
            get
            {
                return iY;
            }
            set
            {
                iY = value;
            }
        }
        /// <summary>
        /// The graphics buffer for this control.
        /// </summary>
        protected Image Buffer;

        private BoundingBox iBounds;
        /// <summary>
        /// The Bounds for this control.
        /// </summary>
        public BoundingBox Bounds
        {
            get
            {
                return iBounds;
            }
            set
            {
                iBounds = value;
            }
        }

        private bool iIsIn = false;
        /// <summary>
        /// True if the mouse is currently in the control.
        /// </summary>
        internal bool IsIn
        {
            get
            {
                return iIsIn;
            }
            set
            {
                iIsIn = value;
            }
        }

        private bool iIsMouseDown = false;
        /// <summary>
        /// True if the mouse pressed down on this control.
        /// </summary>
        internal bool IsMouseDown
        {
            get
            {
                return iIsMouseDown;
            }
            set
            {
                iIsMouseDown = value;
            }
        }

        /// <summary>
        /// The child controls of this control.
        /// </summary>
        public List<Control> Children = new List<Control>();

        /// <summary>
        /// The default constructor.
        /// </summary>
        /// <param name="loc">The location of the control in it's parent container.</param>
        /// <param name="size">The size of the control.</param>
        public Control(Vec2 loc, Vec2 size)
        {
            this.X = loc.X;
            this.Y = loc.Y;
            this.Size = size;
            this.Closing = new DisposingEvent(DummyDisposingEvent);
            this.BeforeDispose = new DisposingEvent(DummyDisposingEvent);
            this.AfterDispose = new DisposingEvent(DummyDisposingEvent);
            this.Click = new MouseEvent(DummyMouseEvent);
            this.MouseEnter = new MouseEvent(DummyMouseEvent);
            this.MouseLeave = new MouseEvent(DummyMouseEvent);
            this.MouseDown = new MouseEvent(DummyMouseEvent);
            this.MouseUp = new MouseEvent(DummyMouseEvent);
            this.Bounds = new BoundingBox(this.X, this.X + Size.X, this.Y + Size.Y, this.Y);
        }

        /// <summary>
        /// Draws the control on the specified image.
        /// </summary>
        /// <param name="i">The image to draw this control on.</param>
        public abstract void Draw(Image i);

        /// <summary>
        /// Disposes of the resources of this control, and all child controls.
        /// </summary>
        public virtual void Dispose()
        {
            this.Bounds = null;
            this.Buffer = null;
            foreach (Control c in Children)
            {
                c.DoBeforeDispose();
                c.Dispose();
                c.DoAfterDispose();
            }
            this.Children = null;
        }
    }
}
