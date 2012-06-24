using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Cosmos.Build.Common;

namespace Cosmos.VS.Package {
  public partial class DebugPageSub : SubPropertyPageBase {
    public DebugPageSub() {
      InitializeComponent();

    }

    protected DebugProperties mProps = new DebugProperties();
    public override PropertiesBase Properties {
      get { return mProps; }
    }

    public override void FillProperties() {
      base.FillProperties();
      mProps.Reset();

    }

  }
}
