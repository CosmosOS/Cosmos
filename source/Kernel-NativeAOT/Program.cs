using System;
using System.Runtime.InteropServices;
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
        Memory.Init(heapBase);

        //0x80000000 is QEMU framebuffer address
        frameBuffer = new FrameBuffer((IntPtr)0x80000000, 1024 * 768 * 4, 1024, 768, FrameBuffer.PixelFormat.R8G8B8);

        frameBuffer.Fill(frameBuffer.MakePixel(0, 0, 0));

        int[] array = new int[] {
            0, 0
        };

        for (int i = 0; i < 2; i++) {
            if (array[i] == 1) {
                frameBuffer.Fill(frameBuffer.MakePixel(255, 255, 255));
            }
            else
            {
                frameBuffer.Fill(frameBuffer.MakePixel(0, 0, 0));
            }
        }

        //DrawChar('X', 50, 50, frameBuffer.MakePixel(255, 255, 255));


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
