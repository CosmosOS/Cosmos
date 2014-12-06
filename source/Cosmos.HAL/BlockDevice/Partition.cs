﻿using System;

namespace Cosmos.HAL.BlockDevice
{
    public class Partition : BlockDevice
    {
        private BlockDevice mHost;
        private UInt64 mStartingSector;

        public Partition(BlockDevice aHost, UInt64 aStartingSector, UInt64 aSectorCount)
        {
            mHost = aHost;
            mStartingSector = aStartingSector;
            mBlockCount = aSectorCount;
            mBlockSize = aHost.BlockSize;
        }

        public override void ReadBlock(UInt64 aBlockNo, UInt64 aBlockCount, byte[] aData)
        {
            UInt64 xHostBlockNo = mStartingSector + aBlockNo;
            CheckBlockNo(xHostBlockNo, aBlockCount);
            mHost.ReadBlock(xHostBlockNo, aBlockCount, aData);
        }

        public override void WriteBlock(UInt64 aBlockNo, UInt64 aBlockCount, byte[] aData)
        {
            UInt64 xHostBlockNo = mStartingSector + aBlockNo;
            CheckBlockNo(xHostBlockNo, aBlockCount);
            mHost.WriteBlock(xHostBlockNo, aBlockCount, aData);
        }
    }
}