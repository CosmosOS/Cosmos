using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Debug.GDB
{
	public class AsmLine
	{
		public readonly string OrignalLine;
		private uint mAddress;
		private string mAddressLine;
		private string mLabel;
		private string mFirstToken;

		public AsmLine(string line)
		{
			OrignalLine = line;
			mAddressLine = OrignalLine;
			Parse();
		}

		private void Parse()
		{
			string startTrim = OrignalLine.TrimStart(' ', '\t');
			if(startTrim.Length == 0)
				return;

			char firstChar = startTrim[0];
			int firstCharAsNumber = (int) firstChar;
			bool possibleLabel = false;
			if (firstCharAsNumber >= 'A' && firstCharAsNumber <= 'Z'
				|| firstCharAsNumber >= 'a' && firstCharAsNumber <= 'z'
				|| firstChar == '_')
			{
				possibleLabel = true;
			}
				bool needToFoundDoublePoint = false;
				int lengthOfLabel = -1;
				for (int i = 1; i < startTrim.Length; i++)
				{
					if (needToFoundDoublePoint == false)
					{
						if (startTrim[i] == ':' && possibleLabel)
						{
							mLabel = startTrim.Substring(0, i);
							break;
						}
						if (startTrim[i] == ' ' || startTrim[i] == '\t')
						{
							mFirstToken = startTrim.Substring(0, i);
							if(possibleLabel == false)
								break;
							needToFoundDoublePoint = true;
							lengthOfLabel = i;
						}
						continue;
					}
					else
					{
						if (startTrim[i] == ':' && possibleLabel)
						{
							mLabel = startTrim.Substring(0, lengthOfLabel);
							break;
						}
						if (startTrim[i] == ' ' || startTrim[i] == '\t')
							continue;
						// other characters found, so no label
						break;
					}
				}
		}

		public uint Address
		{
			get
			{
				return mAddress;
			}
			set
			{
				if (mAddress != value)
				{
					mAddress = value;
					mAddressLine = string.Format("{0:X08}: {1}", value, OrignalLine);
				}
			}
		}

		public string LineWithAddress
		{
			get
			{
				return mAddressLine;
			}
		}

		public bool IsLabel
		{
			get
			{
				return mLabel != null;
			}
		}

		public string Label
		{
			get
			{
				return mLabel == null ? string.Empty : mLabel;
			}
		}

		public string FirstToken
		{
			get
			{
				return mFirstToken;
			}
		}

		public string GDBLine { get; set; }

		public override string ToString()
		{
			return mAddressLine;
		}
	}
}