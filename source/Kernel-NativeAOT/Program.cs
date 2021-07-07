using System;
using System.Runtime.InteropServices;
using Kernel;

public unsafe class Program
{
    public static void Main()
    {
        EntryPoint(IntPtr.Zero, 0);
    }

    public static void EntryPoint(IntPtr MbAddress, long heapBase)
    {
        //0x80000000 is QEMU framebuffer address
        var fb = new FrameBuffer((IntPtr)0x80000000, 1024 * 768 * 4, 1024, 768, FrameBuffer.PixelFormat.R8G8B8);

        fb.Fill(fb.MakePixel(255, 255, 255));

        //Memory.Init(heapBase);

        //Serial.Init(); //COM1
        //Serial.Write('X');
        //serial.Write('H');
        //serial.Write('e');
        //serial.Write('l');
        //serial.Write('l');
        //serial.Write('o');

        //TODO: Parse multiboot2

        while (true)
        {

        }
    }
}
