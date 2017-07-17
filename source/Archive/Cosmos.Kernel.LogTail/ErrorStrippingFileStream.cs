using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Cosmos.Kernel.LogTail
{
    public class ErrorStrippingFileStream : FileStream
    {
        public ErrorStrippingFileStream(string file)
            : base(file, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite)
        {

        }

        public override int ReadByte()
        {
            int result;
            while ((result = base.ReadByte()) == 0) ;
            return result;
        }

        public override int Read(byte[] array, int offset, int count)
        {
            int i;
            for (i = 0; i < count; i++)
            {
                int b = ReadByte();
                if (b == -1)
                    return i;

                array[offset + i] = (byte) b;
            }
            return i;
        }
    }
}
