using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;

namespace MatthijsPlayground
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back.");
        }

        protected override void Run()
        {
            //Console.Write("Input: ");
            //var input = Console.ReadLine();
            //Console.ForegroundColor = ConsoleColor.Green;
            //Console.Write("Text typed: ");
            //Console.WriteLine(input);
            //Console.ForegroundColor = ConsoleColor.White;
            var xVGA = new Cosmos.Hardware.VGAScreen();
            xVGA.SetMode320x200x8();
            xVGA.SetPaletteEntry(0, 63, 0, 0);
            //if (xVGA.Colors == 256)
            //{
            //    for (byte i = 0; i < 64; i++)
            //    {
            //        xVGA.SetPaletteEntry(i, i, 0, 0);
            //        xVGA.SetPaletteEntry(i + 64, 63, i, 0);
            //        xVGA.SetPaletteEntry(i + 128, 63, 63, i);
            //        xVGA.SetPaletteEntry(i + 192, (byte)(63 - i), (byte)(63 - i), (byte)(63 - i));

            //    }
            //}
            //else
            //{
            //    for (byte i = 0; i < xVGA.Colors; i++)
            //    {
            //        byte ii = (byte)((int)i * 64 / xVGA.Colors);
            //        xVGA.SetPaletteEntry(i, ii, ii, ii);
            //    }
            //}

            for (uint x = 0; x < xVGA.PixelWidth; x++)
            {
                for (uint y = 0; y < xVGA.PixelHeight; y++)
                {
                    xVGA.SetPixel320x200x8(x, y, 0);
                }
            }
            var xDbg = new Cosmos.Debug.Kernel.Debugger("USer", "User");
            xDbg.Send("After VGATest");
            while (true)
                ;
        }
    }
}
