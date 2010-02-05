using System;
using System.IO;
using Cosmos.Compiler.Builder;
using Cosmos.Hardware;
using Cosmos.Sys;
using Cosmos.Debug;

namespace VGA_Mouse_Cursor
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

        // Main entry point of the kernel
        public static void Init()
        {
            var xBoot = new Cosmos.Sys.Boot();
            xBoot.Execute();
            Mouse.Initialize();
            VGAScreen.SetMode320x200x8();
            VGAScreen.SetPaletteEntry(0, 0, 0, 0);
            VGAScreen.SetPaletteEntry(1, 63, 0, 0);
            VGAScreen.SetPaletteEntry(2, 63, 63, 63);
            VGAScreen.Clear(0); //clean everything up
            
            uint mx = (uint)Mouse.X;
            uint my = (uint)Mouse.Y;
            VGAScreen.SetPixel320x200x8(mx, my, 2); //Start drawing mouse
            VGAScreen.SetPixel320x200x8(mx, my + 1, 2);
            VGAScreen.SetPixel320x200x8(mx, my + 2, 2);
            VGAScreen.SetPixel320x200x8(mx + 1, my, 2);
            VGAScreen.SetPixel320x200x8(mx + 2, my, 2);
            VGAScreen.SetPixel320x200x8(mx + 1, my + 1, 2);
            VGAScreen.SetPixel320x200x8(mx + 2, my + 2, 2);
            VGAScreen.SetPixel320x200x8(mx + 3, my + 3, 2);
            while (true)
            {
                
            }

        }
    }
}
