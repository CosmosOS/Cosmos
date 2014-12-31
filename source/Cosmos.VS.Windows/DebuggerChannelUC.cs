using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Cosmos.VS.Windows
{
    public class DebuggerChannelUC: UserControl
    {
        protected Cosmos_VS_WindowsPackage mPackage;
        public Cosmos_VS_WindowsPackage Package
        {
            get
            {
                return mPackage;
            }
            set
            {
                mPackage = value;
            }
        }

        protected virtual void HandleChannelMessage(byte aChannel, byte aCommand, byte[] aData)
        {

        }

        public virtual void HandleChannelMessage(ushort aChannelAndCommand, byte[] aData)
        {
            HandleChannelMessage((byte)(aChannelAndCommand >> 8), (byte)(aChannelAndCommand & 0xFF), aData);
        }
    }
}
