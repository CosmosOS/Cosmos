using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Build.Common
{
	public class DebugQemuProperties : PropertiesBase
	{
        public TraceAssemblies TraceAssemblies
        {
            get
            {
                return GetProperty("TraceAssemblies", TraceAssemblies.User);
            }
            set
            {
                SetProperty("TraceAssemblies", value);
            }
        }

        public DebugMode DebugMode
        {
            get
            {
                return GetProperty("DebugMode", DebugMode.None);
            }
            set
            {
                SetProperty("DebugMode", value);
            }
        }

		public DebugQemuCommunication Communication
		{
			get { return this.GetProperty("QemuCommunication", DebugQemuCommunication.None); }
			set { this.SetProperty("QemuCommunication", value); }
		}

	}
}
