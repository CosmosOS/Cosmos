using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Threading;
using Cosmos.Debug.Consts;

namespace Cosmos.Cosmos_VS_Windows
{
    public partial class RegistersUC : UserControl
    {
        public static byte[] mData;

        public RegistersUC() {
            InitializeComponent();
        }

        protected void UpdateRegisters(byte[] aData, int aOffset, DataBytesUC a32, DataBytesUC a16, DataBytesUC a8Hi, DataBytesUC a8Lo)
        {
            a8Lo.Value = aData[aOffset];
            a8Hi.Value = aData[aOffset + 1];
            a16.Value = a8Hi.Value << 8 | a8Lo.Value;
            UpdateRegister32(aData, aOffset, a32);
        }

        protected void UpdateRegister32(byte[] aData, int aOffset, DataBytesUC a32) {
          UInt32 x32 = (UInt32)
            (aData[aOffset + 3] << 24 |
            aData[aOffset + 2] << 16 |
            aData[aOffset + 1] << 8 |
            aData[aOffset]);
          a32.Value = x32;
        }

        public void Update(byte[] aData)
        {
            //Push All
            //  Temp = (ESP);
            //  Push(EAX); 28
            //  Push(ECX); 24
            //  Push(EDX); 20
            //  Push(EBX); 16
            //  Push(Temp); 12 // Have to get from another source, ESP is already in DS when we pushall
            //  Push(EBP); 8
            //  Push(ESI); 4
            //  Push(EDI); 0
            // We get them from bottom up, so we receive them in reverse order as shown above. That is 0-3 is EDI.
            //
            // Additional ones sent manually (at end, not from stack)
            // ESP 32
            // EIP 36
            //
            UpdateRegisters(aData, 28, dataEAX, dataAX, dataAH, dataAL);
            UpdateRegisters(aData, 16, dataEBX, dataBX, dataBH, dataBL);
            UpdateRegisters(aData, 24, dataECX, dataCX, dataCH, dataCL);
            UpdateRegisters(aData, 20, dataEDX, dataDX, dataDH, dataDL);
            UpdateRegister32(aData, 8, dataEBP);
            UpdateRegister32(aData, 4, dataESI);
            UpdateRegister32(aData, 0, dataEDI);
            UpdateRegister32(aData, 32, dataESP);
            UpdateRegister32(aData, 36, dataEIP);
          //TODO: Flags
        }
    }
}