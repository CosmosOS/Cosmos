using System;
using Orvid.Graphics;
using Forms = System.Windows.Forms;

namespace TestBed
{

    internal delegate void ObjectClick(object sender, Forms.MouseEventArgs e);
    internal delegate void DrawMethod();

    internal class ObjectEvents
    {
        public BoundingBox Bounds;
        public ObjectClick MouseClick;
        public ObjectClick MouseEnter;
        public ObjectClick MouseLeave;
        public ObjectClick MouseDown;
        public ObjectClick MouseUp;
        public DrawMethod Draw;
        public bool IsIn;
        public bool IsMouseDown;
        public ObjectEvents(ObjectClick onClick, ObjectClick onEnter, ObjectClick onLeave, ObjectClick onMouseDown, ObjectClick onMouseUp, DrawMethod drawMethod, BoundingBox b)
        {
            this.MouseClick = onClick;
            this.MouseEnter = onEnter;
            this.MouseLeave = onLeave;
            this.MouseDown = onMouseDown;
            this.MouseUp = onMouseUp;
            this.Draw = drawMethod;
            this.IsIn = false;
            this.IsMouseDown = false;
            this.Bounds = b;
        }
    }
}
