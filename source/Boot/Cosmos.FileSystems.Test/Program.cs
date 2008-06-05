using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Hardware;
using System.IO;
using Cosmos.FileSystem;

namespace Cosmos.FileSystems.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Stream s = File.Open("hda.img", FileMode.OpenOrCreate);
            s.Seek(1000 * 512 - 1, SeekOrigin.Begin);
            s.WriteByte(0);
            Disk bd = new StreamDisk(s);

            MBR mbr = new MBR(bd);

            mbr.DiskSignature = 0x12345678;
            mbr.Partition[0].Bootable = true;
            mbr.Partition[0].StartLBA = 1;
            mbr.Partition[0].EndLBA = (uint)(bd.BlockCount - 2);
            mbr.Partition[0].PartitionType =0x0c;
            mbr.Save();

        }
    }

    class StreamDisk : Disk
    {
        Stream s;
        public StreamDisk(Stream s)
        {
            this.s = s;
        }
        
        public override uint BlockSize
        {
            get { return 512; }
        }

        public override ulong BlockCount
        {
            get { return (ulong)(s.Length / BlockSize); }
        }

        public override byte[] ReadBlock(ulong aBlock)
        {
            byte[] b = new byte[(uint)BlockSize];
            s.Seek((uint)(aBlock * BlockSize), SeekOrigin.Begin);
            s.Read(b, 0, (int)BlockSize);
            return b;
        }

        public override void WriteBlock(ulong aBlock, byte[] aContents)
        {
            s.Seek((uint)(aBlock * BlockSize), SeekOrigin.Begin);
            s.Write(aContents, 0, (int)BlockSize);
            s.Flush();
        }

        public override string Name
        {
            get { throw new NotImplementedException(); }
        }
    }
}
