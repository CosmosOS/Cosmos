using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orvid.Graphics;

namespace OForms
{
    /// <summary>
    /// The base class of all controls in the OForms Framework.
    /// </summary>
    public abstract class OControl
    {
        /// <summary>
        /// This is the unique id of the control.
        /// </summary>
        public int CID;
        /// <summary>
        /// The size of the control.
        /// </summary>
        public Vec2 Size;
        /// <summary>
        /// The location of the control.
        /// Measured from the top left corner.
        /// </summary>
        public Vec2 Location;
        /// <summary>
        /// The parent of the OControl.
        /// Null if this control is the window itself.
        /// </summary>
        public OControl Parent;
        /// <summary>
        /// The color of the OControl.
        /// </summary>
        public uint Color;
        /// <summary>
        /// Draws the control.
        /// </summary>
        public abstract void Draw(Image i);
    }
}
