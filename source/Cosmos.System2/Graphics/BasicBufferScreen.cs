using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Cosmos.System.Graphics
{
    public class BasicBufferScreen : Canvas
    {

        private Canvas Backend;

        private Color[] Buffer;

        public BasicBufferScreen(Canvas backend)
        {
            Backend = backend;
            Buffer = new Color[Backend.Mode.Columns * Backend.Mode.Rows];
        }

        public override List<Mode> AvailableModes => Backend.AvailableModes;

        public override Mode DefaultGraphicMode => Backend.DefaultGraphicMode;

        public override Mode Mode { get => Backend.Mode; set => throw new NotImplementedException(); }

        public override void DrawArray(Color[] colors, int x, int y, int width, int height)
        {
            Backend.DrawArray(colors, x, y, width, height);
        }

        public override void DrawPoint(Pen pen, int x, int y)
        {
            Buffer[(y * Backend.Mode.Rows) + x] = pen.Color;
        }

        public override void DrawPoint(Pen pen, float x, float y)
        {
            DrawPoint(pen, (int)x, (int)y);
        }

        public override Color GetPointColor(int x, int y)
        {
            return Buffer[(y * Backend.Mode.Rows) + x];
        }

        public void Render()
        {
            for (int y = 0; y < Backend.Mode.Rows; y++)
            {
                for (int x = 0; x < Backend.Mode.Columns; x++)
                {
                    if (GetPointColor(x, y) != Buffer[(y * Backend.Mode.Rows) + x])
                        Backend.DrawPoint(new Pen(Buffer[(y * Backend.Mode.Rows) + x]), x, y);
                }
            }
        }
    }
}
