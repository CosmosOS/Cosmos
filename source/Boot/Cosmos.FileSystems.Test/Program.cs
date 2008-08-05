//#define USEFILE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Hardware;
using System.IO;
using Cosmos.Sys.FileSystem;
using Cosmos.Sys.FileSystem.FAT32;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Diagnostics;

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
    Debug.Assert(   false, "WARNING! About to access physical disk!! WARNING!");

    SafeFileHandle h = null;
    h = CreateFile("\\\\.\\PhysicalDrive1", GENERIC_READ | GENERIC_WRITE, 0, IntPtr.Zero, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, new SafeFileHandle(IntPtr.Zero, true));
    if (h.IsInvalid)
    {
        Console.WriteLine("wnope");
        Console.ReadLine();
    }
    Stream s = new FileStream(h, FileAccess.ReadWrite);
 
#endif

    Disk bd = new StreamDisk(s, 1500 * 1024 * 1024);
    
    byte[] blank = new byte[512];
    bd.WriteBlock(0, blank);

    MBR mbr = new MBR(bd);
    mbr.DiskSignature = 0x10101010;
    mbr.Partition[0].StartLBA = 0x1;
    mbr.Partition[0].LengthLBA = 0x100000;
    mbr.Partition[0].PartitionType = 0x0c;
    mbr.Partition[1].StartLBA = 0x100001;
    mbr.Partition[1].LengthLBA = 0x100000;
    mbr.Partition[1].PartitionType = 0x0c;
    //mbr.Save();

    Partition p = mbr.Partition[0].GetPartitionDevice();

    FAT32 fat32 = new FAT32(p);
    Stream fs = new FATStream(fat32, fat32.FileAllocationTable, 2);

    byte[] b = new byte[16];
    int bytes;
    while ((bytes = fs.Read(b, 0, 16)) != 0)
    {
        for (int i = 0; i < bytes; i++)
            Console.Write(" 0x" + b[i].ToString("x"));
        Console.WriteLine();
    }

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

        public override void ReadBlock(ulong aBlock, byte[] aBuffer)
        {
            s.Seek((uint)(aBlock * BlockSize), SeekOrigin.Begin);
            s.Read(aBuffer, 0, (int)BlockSize);
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
