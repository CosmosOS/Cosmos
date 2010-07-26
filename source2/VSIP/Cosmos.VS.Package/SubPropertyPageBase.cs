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
	public partial class SubPropertyPageBase : UserControl {
		private CustomPropertyPage subpageOwner;

		public SubPropertyPageBase() {
			InitializeComponent();
		}

		public void SetOwner( CustomPropertyPage owner ) {
			subpageOwner = owner;
		}

		protected Boolean IsDirty {
			get { return subpageOwner.IsDirty; }
			set { subpageOwner.IsDirty = value; }
		}

		protected Boolean IgnoreDirty {
			get { return subpageOwner.IgnoreDirty; }
			set { subpageOwner.IgnoreDirty = value; }
		}

		public void SetConfigProperty(String name, String value) {
            subpageOwner.SetConfigProperty(name, value); 
        }

		public String GetConfigProperty(String name) {
            return subpageOwner.GetConfigProperty(name); 
        }

		public virtual PropertiesBase Properties { 
            get{ return null; } 
        }

		public virtual void FillProperties() {
        }
	}
}
