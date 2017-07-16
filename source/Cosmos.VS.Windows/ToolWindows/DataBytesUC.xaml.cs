﻿using System;
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

namespace Cosmos.VS.Windows {
  public partial class DataBytesUC : UserControl {
    public DataBytesUC() {
      InitializeComponent();
    }

    protected void UpdateDisplay() {
      if (mValue.HasValue) {
        tblkData.Text = mPrefix + mValue.Value.ToString("X" + mDigits);
      } else {
        tblkData.Text = mPrefix + (new string('-', mDigits));
      }
      tblkData.Foreground = mPrevValue == mValue ? Brushes.Black : Brushes.Red;
    }

    protected UInt32? mPrevValue;
    protected UInt32? mValue;
    public UInt32? Value {
      get { return mValue; }

      set { 
        mValue = value;
        UpdateDisplay();
        mPrevValue = mValue;
      }
    }

    protected string mPrefix = "0x";
    public string Prefix {
      get { return mPrefix; }
      set { 
        mPrefix = value;
        UpdateDisplay();
      }
    }

    protected int mDigits = 8;
    public int Digits {
      get { return mDigits; }
      set {
        mDigits = value;
        UpdateDisplay();
      }
    }
  }

}
