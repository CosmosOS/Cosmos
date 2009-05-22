using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Cosmos.VS.Package {
  [Guid("FA935644-BA67-465d-BB88-12997EFA4C21")]
  public class PropPageTest : SettingsPage {
    public PropPageTest() {
      Name = "Test";
    }

    protected string mTest;
    [SRCategoryAttribute("Category")]
    [DisplayName("Property Name")]
    [SRDescriptionAttribute("Description")]
    public string Test {
      get { return mTest; }
      set {
        mTest = value; 
        this.IsDirty = true;
      }
    }

    protected override void BindProperties() {
    }

    protected override int ApplyChanges() {
      SetConfigProperty("Test", "Hello");
      return VSConstants.S_OK;
    }
  }
}
