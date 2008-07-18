using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Hardware;
using System.IO;

namespace MohammedTest
{
    public class BlockDeviceStream : BlockDevice, IDisposable
    {
        public const int MAXBLOCKSIZE = 512;
        private Stream str;

        public BlockDeviceStream(Stream str)
        {
            this.str = str;
        }

        public override uint BlockSize
        {
            get
            {
                return MAXBLOCKSIZE;
            }
        }

        public override ulong BlockCount
        {
            get
            { 
                return (ulong)str.Length; 
            }
        }


        public override string Name
        {
            get
            {
                if(str is FileStream)
                    return (str as FileStream).Name;
                else
                return string.Empty;
            }
        }

        public override void ReadBlock(ulong aBlock, byte[] aBuffer)
        {
            str.Read(aBuffer, (int)aBlock, MAXBLOCKSIZE);
        }

        public override void WriteBlock(ulong aBlock, byte[] aContents)
        {
            str.Write(aContents,(int)aBlock,MAXBLOCKSIZE);
        }

        public void Close()
        {
            if(str != null)
              str.Close();
        }

        #region IDisposable Members

        public void Dispose()
        {
            if(str != null)
              str.Dispose();
        }

        #endregion
    }
}
