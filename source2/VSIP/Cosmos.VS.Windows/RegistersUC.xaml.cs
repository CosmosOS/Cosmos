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
using Cosmos.VS.Debug;
using System.Threading;
using Cosmos.Compiler.Debug;

namespace Cosmos.Cosmos_VS_Windows
{
    public partial class RegistersUC : UserControl
    {
        public RegistersUC()
        {
            InitializeComponent();
            runEAX.Text = "0x--------";
            runAX.Text = "0x----";
            runAH.Text = "0x--";
            runAL.Text = "0x--";
            runEBX.Text = "0x--------";
            runBX.Text = "0x----";
            runBH.Text = "0x--";
            runBL.Text = "0x--";
            runECX.Text = "0x--------";
            runCX.Text = "0x----";
            runCH.Text = "0x--";
            runCL.Text = "0x--";
            runEDX.Text = "0x--------";
            runDX.Text = "0x----";
            runDH.Text = "0x--";
            runDL.Text = "0x--";
            runEBP.Text = "0x--------";
            runESI.Text = "0x--------";
            runEDI.Text = "0x--------";
            runESP.Text = "0x--------";
            runEIP.Text = "0x-------";
            runFlags.Text = "--------";
      }

        protected void UpdateRegisters(byte[] aData, int aOffset, Run a32, Run a16, Run a8Hi, Run a8Lo)
        {
            byte x8Lo = aData[aOffset];
            byte x8Hi = aData[aOffset + 1];
            SetText(a8Lo, "0x" + x8Lo.ToString("X2"));
            SetText(a8Hi, "0x" + x8Hi.ToString("X2"));
            SetText(a16, "0x" + x8Hi.ToString("X2") + x8Lo.ToString("X2"));
            UpdateRegister32(aData, aOffset, a32);
        }

        protected void UpdateRegister32(byte[] aData, int aOffset, Run a32)
        {
            UInt32 x32 = (UInt32)
              (aData[aOffset + 3] << 24 |
              aData[aOffset + 2] << 16 |
              aData[aOffset + 1] << 8 |
              aData[aOffset]);
            SetText(a32, "0x" + x32.ToString("X8"));
        }

        protected void SetText(Run aLabel, string aText)
        {
            string xOldContent = (string)aLabel.Text;
            aLabel.Text = aText;
            aLabel.Foreground = (xOldContent == aText) ? Brushes.Black : Brushes.Red;
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
            UpdateRegisters(aData, 28, runEAX, runAX, runAH, runAL);
            UpdateRegisters(aData, 16, runEBX, runBX, runBH, runBL);
            UpdateRegisters(aData, 24, runECX, runCX, runCH, runCL);
            UpdateRegisters(aData, 20, runEDX, runDX, runDH, runDL);
            UpdateRegister32(aData, 8, runEBP);
            UpdateRegister32(aData, 4, runESI);
            UpdateRegister32(aData, 0, runEDI);
            UpdateRegister32(aData, 32, runESP);
            UpdateRegister32(aData, 36, runEIP);
          //TODO: Flags
        }
    }
}