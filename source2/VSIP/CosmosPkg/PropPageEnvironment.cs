using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Cosmos.VS.Package {
  [Guid(Guids.guidPropPageEnvironmentString)] 
  public partial class PropPageEnvironment : CustomPropertyPage {
    public PropPageEnvironment() {
      InitializeComponent();
    }
  }
}
