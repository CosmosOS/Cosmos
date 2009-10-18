using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using Cosmos.IL2CPU.Plugs;
/*
namespace SteveKernel
{
    [Plug(TargetName = "System.Drawing.SafeNativeMethods.Gdip")]
    public sealed unsafe class GDIpImpl
    {
        private class InternalBitmapFormat
        {
           public IntPtr handle;
           public int width;
           public int height;
           public int stride;
           public int format;

           public byte[] scan0;
        }

        private static List<InternalBitmapFormat> mybitmaps = new List<InternalBitmapFormat>();

        internal static int
        GdipCreateBitmapFromScan0(
        int width, int height, int stride, int format,
        HandleRef scan0, out IntPtr bitmap)
        {
            InternalBitmapFormat ibf = new InternalBitmapFormat()
            {
                format = format,
                height = height,
                stride = stride,
                width = width
            };

            mybitmaps.Add(ibf);

            int size = stride * height;

            ibf.scan0 = new byte[size];

            if (scan0.Handle != IntPtr.Zero)
            {
                unsafe
                {
                    byte* x =(byte*)(scan0.Handle.ToPointer());
                    for (int i = 0; i < size; i++)
                    {
                        ibf.scan0[i] = *x;
                        x++;
                    }
                }
            }
            
            bitmap = ibf.handle = new IntPtr(mybitmaps.Count);
            
            return 0;
        }

    }
}
*/