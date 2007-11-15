using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Cosmos.Kernel.LogViewer.Overview {
	public class OverviewItemCollection: ObservableCollection<OverviewItem> {
		public OverviewItemCollection() {
			Add(new OverviewItem(new MessagesDisplay(), "General Message", "Shows all errors, warnings and informational messages written out by the kernel"));
			Add(new OverviewItem(new Multiboot_MMapDisplay(), "MultiBoot MMap info", "Shows the memorymap information, available from the MultiBoot Specification boot loader"));
			Add(new OverviewItem(new HeapUsageDisplay(), "Heap Usage", "Shows the usage of the heap. Right now, we don't have a Garbage Collector yet, so you need to have a large amount of memory."));
		}
	}
}