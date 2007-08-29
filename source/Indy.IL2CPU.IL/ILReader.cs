using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.IL
{
	public class ILReader {
		private byte[] mILContents;
		private int mILIndex = 0;
		public ILReader(byte[] aContents)
		{
			mILContents = aContents;
		}

		public byte ReadByte()
		{
			byte result;
			if(!TryReadByte(out result))
			{
				throw new Exception("Couldn't read byte!");
			}
			return result;
		}

		public bool TryReadByte(out byte result)
		{
			if (mILIndex == mILContents.Length)
			{
				result = 0;
				return false;
			}
			result = mILContents[mILIndex];
			mILIndex++;
			return true;
		}
	}
}