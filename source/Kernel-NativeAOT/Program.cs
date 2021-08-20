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

        frameBuffer.Fill(frameBuffer.MakePixel(255, 255, 0));

        while (true)
        {

        }
    }
}
