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

namespace Cosmos.VS.Debug {
  public partial class RegistersUC : UserControl {
    public RegistersUC() {
      InitializeComponent();
    }

    protected void UpdateRegisters(byte[] aData, int aOffset, Label a32, Label a16, Label a8Hi, Label a8Lo) {
      byte x8Lo = aData[aOffset + 3];
      byte x8Hi = aData[aOffset + 2];
      a8Lo.Content = x8Lo.ToString("X2");
      a8Hi.Content = x8Hi.ToString("X2");
      a16.Content = x8Hi.ToString("X2") + x8Lo.ToString("X2");
      UpdateRegister32(aData, aOffset, a32);
    }

    protected void UpdateRegister32(byte[] aData, int aOffset, Label a32) {
      UInt32 x32 = (UInt32)
        (aData[aOffset + 0] << 24 |
        aData[aOffset + 1] << 16 |
        aData[aOffset + 2] << 8 |
        aData[aOffset + 3]);
      a32.Content = x32.ToString("X8");
    }

    public void Update(byte[] aData) {
      //Temp = (ESP);
      //Push(EAX); 28
      //Push(ECX); 24
      //Push(EDX); 20
      //Push(EBX); 16
      //Push(Temp); 12 // TODO - Have to get from another source, ESP is already in DS when we pushall
      //Push(EBP); 8
      //Push(ESI); 4
      //Push(EDI); 0
      // We get them from bottom up, so we receive them in reverse order as shown above. That is 0-3 is EDI.
      UpdateRegisters(aData, 28, lablEAX, lablAX, lablAH, lablAL);
      UpdateRegisters(aData, 16, lablEBX, lablBX, lablBH, lablBL);
      UpdateRegisters(aData, 24, lablECX, lablCX, lablCH, lablCL);
      UpdateRegisters(aData, 20, lablEDX, lablDX, lablDH, lablDL);
      //TODO: EIP
      //TODO: Flags
      //TODO: ESP
      UpdateRegister32(aData, 8, lablEBP);
      UpdateRegister32(aData, 4, lablESI);
      UpdateRegister32(aData, 0, lablEDI);
    }

  }
}
