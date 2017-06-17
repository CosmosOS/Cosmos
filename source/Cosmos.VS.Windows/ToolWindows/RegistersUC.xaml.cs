using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
using Cosmos.Debug.Common;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Cosmos.VS.Windows
{
    [Guid("CE2A2D0F-0F1B-4A1F-A9AC-5A5F2A5E2C25")]
    public class RegistersTW : ToolWindowPane2
    {
        public RegistersTW()
        {
            Caption = "Cosmos x86 Registers";
            BitmapResourceID = 301;
            BitmapIndex = 1;

            mUserControl = new RegistersUC();
            Content = mUserControl;
        }
    }

    public partial class RegistersUC : DebuggerUC
    {
        public RegistersUC()
        {
            InitializeComponent();
        }

        protected UInt32 mCurrEBP = 0x0;
        public UInt32 CurrentEBP
        {
            get
            {
                return mCurrEBP;
            }
        }

        protected void UpdateRegisters(byte[] aData, int aOffset, DataBytesUC a32, DataBytesUC a16, DataBytesUC a8Hi, DataBytesUC a8Lo)
        {
            a8Lo.Value = aData[aOffset];
            a8Hi.Value = aData[aOffset + 1];
            a16.Value = a8Hi.Value << 8 | a8Lo.Value;
            UpdateRegister32(aData, aOffset, a32);
        }

        protected void UpdateRegister32(byte[] aData, int aOffset, DataBytesUC a32)
        {
            UInt32 x32 = (UInt32)
              (aData[aOffset + 3] << 24 |
              aData[aOffset + 2] << 16 |
              aData[aOffset + 1] << 8 |
              aData[aOffset]);
            a32.Value = x32;

            if (a32 == dataEBP)
            {
                mCurrEBP = x32;
            }
        }

        protected override void DoUpdate(string aTag)
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
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal,
                (Action)delegate()
                {
                    if (mData == null)
                    {
                        dataEAX.Value = null;
                        dataAX.Value = null;
                        dataAH.Value = null;
                        dataAL.Value = null;

                        dataEBX.Value = null;
                        dataBX.Value = null;
                        dataBH.Value = null;
                        dataBL.Value = null;

                        dataECX.Value = null;
                        dataCX.Value = null;
                        dataCH.Value = null;
                        dataCL.Value = null;

                        dataEDX.Value = null;
                        dataDX.Value = null;
                        dataDH.Value = null;
                        dataDL.Value = null;

                        dataEBP.Value = null;
                        mCurrEBP = 0x0;

                        dataESI.Value = null;
                        dataEDI.Value = null;
                        dataESP.Value = null;
                        dataEIP.Value = null;
                    }
                    else
                    {
                        try
                        {
                            UpdateRegisters(mData, 28, dataEAX, dataAX, dataAH, dataAL);
                            UpdateRegisters(mData, 16, dataEBX, dataBX, dataBH, dataBL);
                            UpdateRegisters(mData, 24, dataECX, dataCX, dataCH, dataCL);
                            UpdateRegisters(mData, 20, dataEDX, dataDX, dataDH, dataDL);
                            UpdateRegister32(mData, 8, dataEBP);
                            UpdateRegister32(mData, 4, dataESI);
                            UpdateRegister32(mData, 0, dataEDI);
                            UpdateRegister32(mData, 32, dataESP);
                            UpdateRegister32(mData, 36, dataEIP);
                        }
                        catch
                        {
                        }
                    }
                }
            );
            //TODO: Flags
        }
    }
}