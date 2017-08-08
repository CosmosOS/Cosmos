using System;
using Orvid.Graphics;

namespace OForms
{
    /// <summary>
    /// A delegate that represents a mouse event.
    /// </summary>
    /// <param name="loc">The location of the mouse.</param>
    /// <param name="buttons">The MouseButtons that are pressed.</param>
    public delegate void MouseEvent(Vec2 loc, MouseButtons buttons);

    /// <summary>
    /// A delegate that represents an event 
    /// relating to the disposing of a control.
    /// </summary>
    public delegate void DisposingEvent();

}
