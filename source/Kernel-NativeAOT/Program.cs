using System;
using System.Runtime.InteropServices;
using Kernel;

public unsafe class Program
{
    public static FrameBuffer frameBuffer;

    public static void Main()
    {
        EntryPoint(IntPtr.Zero, IntPtr.Zero);
    }

    public static void EntryPoint(IntPtr MbAddress, IntPtr heapBase)
    {
        Multiboot2.Parse(MbAddress);
        Memory.Init((long)heapBase);

        frameBuffer = new FrameBuffer((IntPtr)0x80000000, 1024 * 768 * 4, 1024, 768, FrameBuffer.PixelFormat.R8G8B8);

        

        int[] test = new int[]
        {
            0, 1, 0
        };

        

        if (test[1] == 1)
        {
            frameBuffer.Fill(frameBuffer.MakePixel(255, 0, 0));
        }
        else
        {
            frameBuffer.Fill(frameBuffer.MakePixel(0, 255, 0));
        }

        //DrawChar('A', 50, 50, frameBuffer.MakePixel(255, 255, 255));

        while (true)
        {

        }
    }

    private static void DrawChar(char letter, int x, int y, uint color)
    {
        int p = (int)letter * 128 - 128;

        for (int l = 0; l < 16; l++)
        {
            for (int c = 0; c < 8; c++)
            {
                if (Font.font[p] == 1)
                {
                    frameBuffer[x + c, y + l] = color;
                }
                p++;
            }
        }
    }
}
