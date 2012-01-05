using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Windows.Controls;

namespace Cosmos.VS.Windows {
  public class DebuggerUC : UserControl {
    protected byte[] mData = new byte[0];

    public void Update(byte[] aData) {
      mData = aData;
      Dispatcher.Invoke(DispatcherPriority.Normal, 
        (Action)delegate() {
          DoUpdate(aData);    
        }
      );
    }

    protected virtual void DoUpdate(byte[] aData) {
    }
  }
}
