using System;
using System.IO;

namespace Microsoft.Samples.Debugging.Native
{
    /// <summary>
    /// A very basic PE reader that can extract a few useful pieces of information
    /// </summary>
    public class PEReader
    {
        // PE file
        private FileStream m_peStream;

        // cached information from the PE file
        private int peHeaderOffset = 0;

        public PEReader(FileStream peFileStream)
        {
            m_peStream = peFileStream;
        }

        public int TimeStamp
        {
            get
            {
                return ReadDwordAtFileOffset(PEHeaderOffset + 8);
            }
        }

        public int SizeOfImage
        {
            get
            {
                return ReadDwordAtFileOffset(PEHeaderOffset + 80);
            }
        }

        private int PEHeaderOffset
        {
            get
            {
                if (peHeaderOffset == 0)
                {
                    peHeaderOffset = ReadDwordAtFileOffset(0x3c);
                }
                return peHeaderOffset;
            }
        }

        private int ReadDwordAtFileOffset(int fileOffset)
        {
            byte[] dword = new byte[4];
            ReadBytesAtFileOffset(dword, fileOffset);
            return BitConverter.ToInt32(dword, 0);
        }

        private void ReadBytesAtFileOffset(byte[] bytes, int fileOffset)
        {
            m_peStream.Seek(fileOffset, SeekOrigin.Begin);
            int bytesReadTotal = 0;
            do
            {
                int bytesRead = m_peStream.Read(bytes, bytesReadTotal, bytes.Length - bytesReadTotal);
                bytesReadTotal += bytesRead;
            } while (bytesReadTotal != bytes.Length);
        }
    }
}