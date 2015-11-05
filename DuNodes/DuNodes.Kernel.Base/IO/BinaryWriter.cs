// Decompiled with JetBrains decompiler
// Type: GruntyOS.IO.BinaryWriter
// Assembly: GLNFSLib, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: B3BB9CCA-702B-49B4-8298-3F149CD7972D
// Assembly location: D:\Users\symura\Desktop\GLNFSLib.dll

using System;

namespace DuNodes.Kernel.Base.IO
{
    public class BinaryWriter
    {
        public ioStream BaseStream;

        public BinaryWriter(ioStream file)
        {
            this.BaseStream = file;
        }

        public void Write(byte data)
        {
            this.BaseStream.Write(data);
        }

        public void Write(char data)
        {
            this.BaseStream.Write((byte)data);
        }

        public void WriteBytes(string str)
        {
            for (int index = 0; index < str.Length; ++index)
                this.Write((byte)str[index]);
        }

        public void Write(int data)
        {
            foreach (byte i in BitConverter.GetBytes(data))
                this.BaseStream.Write(i);
        }

        public void Write(uint data)
        {
            foreach (byte i in BitConverter.GetBytes(data))
                this.BaseStream.Write(i);
        }

        public void Write(short data)
        {
            foreach (byte i in BitConverter.GetBytes(data))
                this.BaseStream.Write(i);
        }

        public void Write(ushort data)
        {
            foreach (byte i in BitConverter.GetBytes(data))
                this.BaseStream.Write(i);
        }

        public void Write(byte[] data)
        {
            foreach (byte i in data)
                this.BaseStream.Write(i);
        }

        public void Write(string data)
        {
            this.BaseStream.Write((byte)data.Length);
            foreach (byte i in data)
                this.BaseStream.Write(i);
        }
    }
}
