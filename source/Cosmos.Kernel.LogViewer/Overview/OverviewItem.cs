using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace Cosmos.Kernel.LogViewer.Overview {
	public class OverviewItem {
		public OverviewItem(Control aDisplayControl, string aDisplayName, string aDescription) {
			DisplayControl = aDisplayControl;
			DisplayName = aDisplayName;
			Description = aDescription;
		}

		public Control DisplayControl {
			get;
			private set;
		}

		public string DisplayName {
			get;
			private set;
		}

		public string Description {
			get;
			private set;
		}
	}
}