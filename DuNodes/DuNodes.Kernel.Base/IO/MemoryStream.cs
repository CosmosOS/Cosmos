// Decompiled with JetBrains decompiler
// Type: GruntyOS.IO.MemoryStream
// Assembly: GLNFSLib, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: B3BB9CCA-702B-49B4-8298-3F149CD7972D
// Assembly location: D:\Users\symura\Desktop\GLNFSLib.dll

using System;

namespace DuNodes.Kernel.Base.IO
{
    public class MemoryStream : ioStream
    {
        private unsafe byte* pointer = (byte*)null;

        public unsafe MemoryStream(int size)
        {
            this.pointer = (byte*)null;
            this.init(size);
        }

        public unsafe MemoryStream(byte[] dat)
        {
            this.pointer = (byte*)null;
            this.Data = dat;
        }

        public unsafe MemoryStream(byte* ptr)
        {
            this.pointer = ptr;
        }

        public override void Close()
        {
        }

        public override unsafe void Write(byte i)
        {
            if ((IntPtr)this.pointer == IntPtr.Zero)
            {
                base.Write(i);
            }
            else
            {
                this.pointer[this.Position] = i;
                ++this.Position;
            }
        }

        public override unsafe byte Read()
        {
            if ((IntPtr)this.pointer == IntPtr.Zero)
                return base.Read();
            ++this.Position;
            return this.pointer[this.Position - 1];
        }
    }
}
