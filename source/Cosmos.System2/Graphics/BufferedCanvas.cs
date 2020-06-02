//#define COSMOSDEBUG
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Cosmos.System.Graphics
{
    public class BufferedCanvas : Canvas
    {

        private Canvas Backend;
        private Color[] Buffer;
        private bool[] changed;

        public BufferedCanvas(Mode mode, Color? color = null)
        {
            Color bufferColor = color ?? Color.Black;
                Backend = FullScreenCanvas.GetFullScreenCanvas(mode);

                Buffer = new Color[Backend.Mode.Columns * Backend.Mode.Rows];
                changed = new bool[Backend.Mode.Columns / 10 * Backend.Mode.Rows / 10];
                Clear(bufferColor);
                for (int i = 0; i < changed.Length; i++)
                {
                    changed[i] = false;
                }
                Global.mDebugger.Send("DEBUG Rows: " + Backend.Mode.Rows + " Columns: " + Backend.Mode.Columns + " Color: " + Buffer.ToString());
        }
        public BufferedCanvas(Color? color = null)
        {
            Color bufferColor = color ?? Color.Black;

                Backend = FullScreenCanvas.GetFullScreenCanvas();
                Buffer = new Color[Backend.Mode.Columns * Backend.Mode.Rows];
                changed = new bool[Backend.Mode.Columns / 10 * Backend.Mode.Rows / 10];
                Clear(bufferColor);
                for (int i = 0; i < changed.Length; i++)
                {
                    changed[i] = false;
                }
        }


        public override List<Mode> AvailableModes => Backend.AvailableModes;

        public override Mode DefaultGraphicMode => Backend.DefaultGraphicMode;

        public override Mode Mode
        {
            get => Backend.Mode;
            set
            {
                Backend.Mode = value;
                Buffer = new Color[Backend.Mode.Rows * Backend.Mode.Columns];
                changed = new bool[Backend.Mode.Columns / 10 * Backend.Mode.Rows / 10];
                for (int i = 0; i < changed.Length; i++)
                {
                    changed[i] = false;
                }
                Global.mDebugger.Send("DEBUG Rows: " + Backend.Mode.Rows + " Columns: " + Backend.Mode.Columns);
                Backend.Clear();
            }
        }
        public override void DrawArray(Color[] colors, int x, int y, int width, int height)
        {
            for (int _y = 0; _y < height; _y++)
            {
                for (int _x = 0; _x < width; _x++)
                {

                    DrawPoint(new Pen(colors[_y * width + _x + x]), _x+x, _y+y);

                }
            }

        }
        public override void DrawPoint(Pen pen, int x, int y)
        {
            Buffer[(y * Backend.Mode.Columns) + x] = pen.Color;
            changed[(y / 10 * Backend.Mode.Columns / 10) + x / 10] = true;
        }


        public override void DrawPoint(Pen pen, float x, float y)
        {
            DrawPoint(pen, (int)x, (int)y);

        }

        public override Color GetPointColor(int x, int y)
        {
            return Buffer[(y * Backend.Mode.Columns) + x];

        }
        public void Clear(Color? color = null)
        {
                Color DefaultColor = color ?? Color.Black;

                for (int i = 0; i < Buffer.Length; i++)
                {
                    Buffer[i] = DefaultColor;
                }

                for (int i = 0; i < changed.Length; i++)
                {
                    changed[i] = true;
                }

            }
        }

        public void Clear(Color? color = null, bool direct = false)
        {


                Color DefaultColor = color ?? Color.Black;

                for (int i = 0; i < Buffer.Length; i++)
                {
                    Buffer[i] = DefaultColor;
                }
                if (direct)
                {
                    Backend.Clear(DefaultColor);
                }
                else
                {
                    for (int i = 0; i < changed.Length; i++)
                    {
                        changed[i] = true;
                    }
                }

            }
        }
        public override void Disable()
        {
            Backend.Disable();
        }
        public void Render()
        {
            for (int by = 0; by < Backend.Mode.Rows / 10; by++)
            {
                for (int bx = 0; bx < Backend.Mode.Columns / 10; bx++)
                {
                    if (changed[by * Backend.Mode.Columns / 10 + bx])
                    {
                        changed[by * Backend.Mode.Columns / 10 + bx] = false;
                        Global.mDebugger.Send($"Checking {by * Backend.Mode.Columns / 10 + bx} {bx * 10} {by * 10}");
                        for (int y = 0; y < 10; y++)
                        {
                            for (int x = 0; x < 10; x++)
                            {
                                if (GetPointColor(bx * 10 + x, by * 10 + y).ToArgb() != Backend.GetPointColor(bx * 10 + x, by * 10 + y).ToArgb())
                                {
                                    Backend.DrawPoint(new Pen(Buffer[((by * 10 + y) * Backend.Mode.Columns) + (bx * 10 + x)]), bx * 10 + x, by * 10 + y);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
