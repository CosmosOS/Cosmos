using System;
using Cosmos.Compiler.Builder;
using System.Drawing;
using Cosmos.Hardware;
using Cosmos.Kernel;

namespace SteveKernel
{
    class Program
    {
        #region Cosmos Builder logic
        // Most users wont touch this. This will call the Cosmos Build tool
        [STAThread]
        static void Main(string[] args)
        {
            BuildUI.Run();
        }
        #endregion

        public unsafe static void Init()
        {
            var xBoot = new Cosmos.Sys.Boot();
            xBoot.Execute();

            MouseDemo();

        }

        private static void MouseDemo()
        {
            VGAScreen.SetMode320x200x8();

            VGAScreen.SetPaletteEntry(0, 0, 0, 0);
            VGAScreen.SetPaletteEntry(1, 63, 0, 0);
            VGAScreen.SetPaletteEntry(2, 63, 63, 63);

            VGAScreen.Clear(0);

            uint x = (uint)Mouse.X;
            uint y = (uint)Mouse.Y;
            uint oc = 0;

            while (true)
            {
                uint mx = (uint)Mouse.X;
                uint my = (uint)Mouse.Y;

                if (mx != x || my != y)
                {
                    if (Mouse.Buttons == Mouse.MouseState.Left)
                        VGAScreen.SetPixel320x200x8(x, y, 1);
                    else if (Mouse.Buttons == Mouse.MouseState.Right)
                        VGAScreen.SetPixel320x200x8(x, y, 0);
                    else
                        VGAScreen.SetPixel320x200x8(x, y, oc);

                    x = mx;
                    y = my;
                    oc = VGAScreen.GetPixel320x200x8(x, y);

                    VGAScreen.SetPixel320x200x8(x, y, 2);
                }
            }
        }
    }
}