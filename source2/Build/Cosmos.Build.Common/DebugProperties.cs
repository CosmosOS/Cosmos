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
            get { return GetProperty("DebugMode", DebugMode.Source); }
            set { SetProperty("DebugMode", value); }
        }

        public bool IgnoreDebugStubAttribute
        {
            get { return GetProperty("IgnoreDebugStubAttribute", false); }
            set { SetProperty("IgnoreDebugStubAttribute", value); }
        }

        public Boolean EnableGDB {
            get { return GetProperty("EnableGDB", false); }
            set { SetProperty("EnableGDB", value); }
        }

        public bool StartCosmosGDB
        {
            get { return GetProperty("StartCosmosGDB", false); }
            set { SetProperty("StartCosmosGDB", value); }
        }
    }
}
