using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Build.Common {
	public class DebugProperties : PropertiesBase {
        public TraceAssemblies TraceAssemblies {
            get { return GetProperty("TraceAssemblies", TraceAssemblies.User); }
            set { SetProperty("TraceAssemblies", value); }
        }

        public DebugMode DebugMode {
            get { return GetProperty("DebugMode", DebugMode.None); }
            set { SetProperty("DebugMode", value); }
        }

        public Boolean EnableGDB {
            get { return GetProperty("EnableGDB", false); }
            set { SetProperty("EnableGDB", value); }
        }

    }
}
