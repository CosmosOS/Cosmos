using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Cosmos.Sys.FileSystem.FAT32
{
    public class FATStream : Stream
    {
        private FAT fs;
        private FileAllocationTable fat;
        private uint startcluster;

        long ClusterSize;

        public FATStream(FAT fs, FileAllocationTable fat, UInt32 startcluster)
        {
            this.fs = fs;
            this.fat = fat;
            this.startcluster = startcluster;

            ClusterSize = fat.ClusterSize;

            Restart(0);
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        private long position;
        public override long Position
        {
            get
            {
                return position;
            }
            set
            {
                GotoPosition(value);
            }
        }
        
        private long CurrentClusterStart;
        private byte[] CurrentCluster;
        private long CurrentClusterEnd;
        private uint CurrentClusterNumber;


        private void GotoPosition(long value)
        {
            if (value < CurrentClusterStart)
            {
                Restart(value);
            }
            else if ( value >= CurrentClusterEnd)
            {
                if (value < 2 * CurrentClusterEnd - CurrentClusterStart)
                {
                    GetNextCluster();
                }
                else
                {
                    Restart(value);
                }
            }
            position = value;
        }

        private void GetNextCluster()
        {
            CurrentClusterStart += ClusterSize;
            CurrentClusterEnd = CurrentClusterStart + ClusterSize;
            CurrentClusterNumber = fat.GetNextCluster(CurrentClusterNumber);
            if (CurrentClusterNumber == (fat as FileAllocationTableFAT32).ClusterEOL)
                throw new Exception("end of file");
            CurrentCluster = fs.ReadCluster(CurrentClusterNumber);
        }

        private void Restart(long value)
        {
            CurrentClusterStart = 0;

            CurrentClusterNumber = startcluster;
            while (value >= CurrentClusterStart + ClusterSize) // TODO: check eof!
            {
                if (CurrentClusterNumber == (fat as FileAllocationTableFAT32).ClusterEOL)
                    throw new Exception("end of file");

                CurrentClusterStart += ClusterSize;
                CurrentClusterNumber = fat.GetNextCluster(CurrentClusterNumber);
            }

            CurrentClusterEnd = CurrentClusterStart + ClusterSize;
            CurrentCluster = fs.ReadCluster(CurrentClusterNumber);
        }

        public override int Read(byte[] aBuffer, int aOffset, int aCount)
		{
			return Read(aBuffer , (long)aOffset , (long)aCount);
		}
		
        public int Read(byte[] buffer, long offset, long count)
        {
            int read=0;
            while (count > 0)
            {
                long left = CurrentClusterEnd - Position;
                if (left > count)
                    left = count;
                Array.Copy(CurrentCluster, position - CurrentClusterStart,
                    buffer, offset, left);
                Position += left;
                offset += left;
                count -= left;
                read+=left;
            }
            return read;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.Begin)
            {
                Position = offset;
            }
            else if (origin == SeekOrigin.Current)
            {
                Position += offset;
            }
            else
            {
                Position = Length + offset;
            }
            return position;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] aBuffer, int aOffset, int aCount)
        {
            Write(aBuffer , (long)aOffset , (long)aCount);
        }

        public void Write(byte[] buffer, long offset, long count)
        {
            throw new NotImplementedException();
        }
    }
}