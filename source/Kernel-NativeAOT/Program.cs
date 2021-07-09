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
    static EFI_SYSTEM_TABLE* systemTable;

    public static void EntryPoint(IntPtr MbAddress, long heapBase)
    {
        Multiboot2.Parse(MbAddress);

        systemTable = (EFI_SYSTEM_TABLE*)Multiboot2.TagEFI64->Address;

        string hello = "Hello world!";
        fixed (char* pHello = hello)
        {
            systemTable->ConOut->OutputString(systemTable->ConOut, pHello);
        }

        //0x80000000 is QEMU framebuffer address
        //frameBuffer = new FrameBuffer((IntPtr)Multiboot2.TagFramebuffer->Common.Address, 1024 * 768 * 4, 1024, 768, FrameBuffer.PixelFormat.R8G8B8);

        //frameBuffer.Fill(frameBuffer.MakePixel(255, 255, 255));

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
