// Decompiled with JetBrains decompiler
// Type: GruntyOS.IO.BinaryReader
// Assembly: GLNFSLib, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: B3BB9CCA-702B-49B4-8298-3F149CD7972D
// Assembly location: D:\Users\symura\Desktop\GLNFSLib.dll

using System;

namespace DuNodes.Kernel.Base.IO
{
    public class BinaryReader
    {
        public ioStream BaseStream;

        public BinaryReader(ioStream stream)
        {
            stream.Position = 0;
            this.BaseStream = stream;
        }

        public byte ReadByte()
        {
            ++this.BaseStream.Position;
            return this.BaseStream.Data[this.BaseStream.Position - 1];
        }

        public int ReadInt32()
        {
            int num = BitConverter.ToInt32(this.BaseStream.Data, this.BaseStream.Position);
            this.BaseStream.Position += 4;
            return num;
        }

        public uint ReadUInt32()
        {
            uint num = BitConverter.ToUInt32(this.BaseStream.Data, this.BaseStream.Position);
            this.BaseStream.Position += 4;
            return num;
        }

        public string ReadAllText()
        {
            string str = "";
            while (this.BaseStream.Position < this.BaseStream.Data.Length)
                str += ((char)this.BaseStream.Read()).ToString();
            return str;
        }

        public void Close()
        {
            this.BaseStream.Close();
        }

        public string ReadString()
        {
            byte num = this.BaseStream.Read();
            string str = "";
            for (int index = 0; index < (int)num; ++index)
                str += ((char)this.BaseStream.Read()).ToString();
            return str;
        }
    }
}
