using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Build.Common
{
	public class VMQemuProperties : PropertiesBase
	{

		public VMQemuNetworkCard NetworkCard
		{
			get { return this.GetProperty("QemuNetworkCard", VMQemuNetworkCard.None); }
			set { this.SetProperty("QemuNetworkCard", value); }
		}

		public Boolean EnableNetworkTAP
		{
			get { return this.GetProperty("QemuNetworkTAP", false); }
			set { this.SetProperty("QemuNetworkTAP", value); }
		}

		public VMQemuAudioCard AudioCard
		{
			get { return this.GetProperty("QemuAudioCard", VMQemuAudioCard.None); }
			set { this.SetProperty("QemuAudioCard", value); }
		}

	}
}
