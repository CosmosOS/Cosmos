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

namespace Cosmos.Cosmos_VS_Windows {
  public partial class DataBytesUC : UserControl {
    public DataBytesUC() {
      InitializeComponent();
    }

    protected UInt32? mPrevValue;
    protected UInt32? mValue;
    public UInt32? Value {
      set { 
        mValue = value;
        if (mValue.HasValue) {
          tblkData.Text = "0x" + mValue.Value.ToString("X8");
        } else {
          tblkData.Text = "0x--------";
        }
        tblkData.Foreground = mPrevValue == mValue ? Brushes.Black : Brushes.Red;
        mPrevValue = mValue;
      }
      get { return mValue; }
    }
  }
}
