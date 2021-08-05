using System;
using System.Runtime.InteropServices;
using Internal.Runtime.CompilerServices;
using Kernel;

public unsafe class Program
{
    public static void Main()
    {
        EntryPoint(IntPtr.Zero, 0);
    }

    static FrameBuffer frameBuffer;

    public static void EntryPoint(IntPtr MbAddress, long heapBase)
    {
        Multiboot2.Parse(MbAddress);
        Memory.Init(0xA0000000); //TODO: don't hardcore base heap address

        uint buffersize = (uint)(Multiboot2.TagFramebuffer->Common.Width * Multiboot2.TagFramebuffer->Common.Height * (Multiboot2.TagFramebuffer->Common.Bpp / 8));
        frameBuffer = new FrameBuffer((IntPtr)Multiboot2.TagFramebuffer->Common.Address, buffersize, Multiboot2.TagFramebuffer->Common.Width, Multiboot2.TagFramebuffer->Common.Height, FrameBuffer.PixelFormat.R8G8B8);
        
        frameBuffer.Fill(frameBuffer.MakePixel(0, 0, 255));

        //DrawChar('A', 50, 50, frameBuffer.MakePixel(255, 255, 255));

        //string hello = "Hello world!";
        //fixed (char* pHello = hello)
        //{
        //    systemTable->ConOut->OutputString(systemTable->ConOut, pHello);
        //}


        while (true)
        {

        }
    }

    private static void DrawChar(char letter, int x, int y, uint color)
    {
        int p = (int)letter * 128 - 128;

        for (int l = 0; l < 16; l++) {
            for (int c = 0; c < 8; c++) {
                if (Font.font[p] == 1) {
                    frameBuffer[x + c, y + l] = color;
                }
                p++;
            }
        }
    }
}
