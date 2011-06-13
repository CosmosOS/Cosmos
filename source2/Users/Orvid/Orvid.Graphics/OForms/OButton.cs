using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orvid.Graphics;

namespace OForms
{
    #region OButton
    /// <summary>
    /// This class is a button.
    /// </summary>
    public class OButton : OControl
    {
        /// <summary>
        /// The bounding box for this control.
        /// </summary>
        public BoundingBox Bounds;
        private static Vec2 DefaultSize = new Vec2(75, 20);
        //public event Mouse MouseClick = new Mouse(None);
        //private static void None(object o, MouseArgs e) { }

        //public OButton()
        //    : base()
        //{
        //    MouseClick += new Mouse(CheckMouseClick);
        //}

        //private void CheckMouseClick(object o, MouseArgs e)
        //{
        //    if (Bounds.IsInBounds(new Vec2(e.X, e.Y)))
        //    {
        //        MouseClick.Invoke(o, e);
        //    }
        //}

        #region Draw
        /// <summary>
        /// Draws the button.
        /// </summary>
        public override void Draw(Image i)
        {
            float xScale = Size.X / DefaultSize.X;
            float yScale = Size.Y / DefaultSize.Y;

            // Center fill.
            //(0,4), (75, 4), (75, 17), (0,17)
            i.DrawRectangle(
                new Vec2(Location.X, (Location.Y - (Int32)(yScale * 4))),
                new Vec2(Location.X + (Int32)(xScale * 75), (Location.Y - (Int32)(yScale * 4))),
                new Vec2(Location.X + (Int32)(xScale * 75), (Location.Y - (Int32)(yScale * 17))),
                new Vec2(Location.X, (Location.Y - (Int32)(yScale * 17))),
                Color);
            // Top left corner.
            // (5, 4), 4, 4
            i.DrawElipse(
                new Vec2(Location.X + (Int32)(xScale * 5), (Location.Y - (Int32)(yScale * 4))),
                (Int32)(yScale * 4),
                (Int32)(xScale * 4),
                Color);
            // Top right corner.
            // (71, 4), 4, 4
            i.DrawElipse(
                new Vec2(Location.X + (Int32)(xScale * 71), Location.Y - (Int32)(yScale * 4)),
                (Int32)(yScale * 4),
                (Int32)(xScale * 4),
                Color);
            // Bottom left corner
            // (5, 16), 4, 4
            i.DrawElipse(
                new Vec2(Location.X + (Int32)(xScale * 5), (Location.Y - (Int32)(yScale * 16))),
                (Int32)(yScale * 4),
                (Int32)(xScale * 4),
                Color);
            // Bottom right corner
            // (71, 16), 4, 4
            i.DrawElipse(
                new Vec2(Location.X + (Int32)(xScale * 71), (Location.Y - (Int32)(yScale * 16))),
                (Int32)(yScale * 4),
                (Int32)(xScale * 4),
                Color);
            // The rest of the fill
            //(5,0), (71, 0), (71, 20), (5,20)
            i.DrawRectangle(
                new Vec2(Location.X + (Int32)(xScale * 5), Location.Y),
                new Vec2(Location.X + (Int32)(xScale * 71), Location.Y),
                new Vec2(Location.X + (Int32)(xScale * 71), (Location.Y - (Int32)(yScale * 20))),
                new Vec2(Location.X + (Int32)(xScale * 5), (Location.Y - (Int32)(yScale * 20))),
                Color);

            // Set the bounding box.
            Bounds = new BoundingBox(
                Location.X,
                Location.X + (Int32)(xScale * 75),
                Location.Y,
                Location.Y - (Int32)(yScale * 20)
            );
        }
        #endregion
    }
    #endregion

    #region Mouse Event Delegates
    ///// <summary>
    ///// The enum that describes the various types of actions 
    ///// that a mouse event can send.
    ///// </summary>
    //public enum MouseButton
    //{
    //    /// <summary>
    //    /// This type means the mouse was released.
    //    /// </summary>
    //    MouseUp,
    //    /// <summary>
    //    /// This type means the left mouse button was,
    //    /// and still is, pressed down.
    //    /// </summary>
    //    MouseDown,
    //    /// <summary>
    //    /// This means that the left mouse button was clicked.
    //    /// </summary>
    //    MouseClick,
    //    /// <summary>
    //    /// This means the right mouse button was,
    //    /// and still is, pressed down.
    //    /// </summary>
    //    MouseRightDown,
    //    /// <summary>
    //    /// This means that the right mouse button was clicked.
    //    /// </summary>
    //    MouseRightClick,
    //    /// <summary>
    //    /// This means that the middle mouse button was,
    //    /// and still is, pressed down.
    //    /// </summary>
    //    MouseMiddleDown,
    //    /// <summary>
    //    /// This means that the middle mouse button was clicked.
    //    /// </summary>
    //    MouseMiddleClick,
    //    /// <summary>
    //    /// This means the mouse was moved.
    //    /// </summary>
    //    MouseMove
    //}

    ///// <summary>
    ///// The class that describes a mouse event.
    ///// </summary>
    //public class MouseArgs
    //{
    //    /// <summary>
    //    /// The X position of the mouse.
    //    /// </summary>
    //    public int X;
    //    /// <summary>
    //    /// The Y position of the mouse.
    //    /// </summary>
    //    public int Y;
    //    /// <summary>
    //    /// The type of event that occurred.
    //    /// </summary>
    //    public MouseButton Type;
    //}

    //public delegate void Mouse(object o, MouseArgs e);
    #endregion

}
