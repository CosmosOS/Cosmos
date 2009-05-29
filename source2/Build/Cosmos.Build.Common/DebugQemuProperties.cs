using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Build.Common
{
	public class DebugQemuProperties : PropertiesBase
	{

		public DebugQemuCommunication Communication
		{
			get { return this.GetProperty("QemuCommunication", DebugQemuCommunication.None); }
			set { this.SetProperty("QemuCommunication", value); }
		}

	}
}
