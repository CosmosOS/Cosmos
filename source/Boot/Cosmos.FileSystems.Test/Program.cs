using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Hardware;
using System.IO;
using Cosmos.FileSystem;
using Cosmos.FileSystem.FAT32;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace Cosmos.FileSystems.Test
{
    class Program
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError=true)]
internal static extern SafeFileHandle CreateFile(string lpFileName, int
dwDesiredAccess, int dwShareMode,
IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint
dwFlagsAndAttributes,
SafeFileHandle hTemplateFile);

        internal const int GENERIC_READ = unchecked((int)0x80000000);
        internal const int GENERIC_WRITE= unchecked((int)0x40000000);
        internal const int OPEN_EXISTING = 3;
internal const int FILE_ATTRIBUTE_NORMAL = 0x80;



        static void Main(string[] args)
        {
        
#if USEFILE
            Stream s = File.Open("hda.img", FileMode.OpenOrCreate);
            s.Seek(1000 * 512 - 1, SeekOrigin.Begin);
            s.WriteByte(0);
#else
SafeFileHandle h = null;
h = CreateFile("\\\\.\\PhysicalDrive1",GENERIC_READ | GENERIC_WRITE, 0, IntPtr.Zero, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, new SafeFileHandle(IntPtr.Zero, true));
if (h.IsInvalid)
{
    Console.WriteLine("wnope");
    Console.ReadLine();
}
    Stream s = new FileStream(h, FileAccess.ReadWrite);
    // Read from stream



#endif

            Disk bd = new StreamDisk(s,1500*1024*1024);
            byte[] blank = new byte[512];
            bd.WriteBlock(0, blank);

            MBR mbr = new MBR(bd);

            mbr.DiskSignature = 0x12345678;
            //mbr.Partition[0].Bootable = true;
            mbr.Partition[0].StartLBA = 10;
            mbr.Partition[0].LengthLBA = (uint)(bd.BlockCount - 20);
            mbr.Partition[0].PartitionType =0x0b;
            mbr.Save();

            Partition p = mbr.Partition[0].GetPartitionDevice();

            FAT32 fat32 = new FAT32(p);
            fat32.Format("hello!", 512);
            s.Flush();

        }
    }

    class StreamDisk : Disk
    {
        Stream s;
        long length;
        public StreamDisk(Stream s) : this(s, s.Length)
        {

        }
        public StreamDisk(Stream s, long length)
        {
            this.s=s;
            this.length=length;
        }
        
        public override uint BlockSize
        {
            get { return 512; }
        }

        public override ulong BlockCount
        {
            get { return (ulong)(length / BlockSize); }
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
