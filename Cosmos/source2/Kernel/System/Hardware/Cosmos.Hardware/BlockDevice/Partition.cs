﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware.BlockDevice
{
	public class Partition : BlockDevice
	{
		BlockDevice mHost;
		UInt64 mStartingSector;

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