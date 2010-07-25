using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Cosmos.Build.Common;
//using Cosmos.Build.Common;

namespace Cosmos.VS.Package
{
	public partial class VMPageQemu : SubPropertyPageBase
	{
		private Cosmos.Build.Common.VMQemuProperties projProperties;

		public VMPageQemu()
		{
			InitializeComponent();

			this.comboNetworkCard.Items.AddRange(EnumValue.GetEnumValues(typeof(VMQemuNetworkCard)));
			this.comboAudioCard.Items.AddRange(EnumValue.GetEnumValues(typeof(VMQemuAudioCard)));

			this.projProperties = new VMQemuProperties();

			this.CreateUIMonitorEvents();
		}

		private void CreateUIMonitorEvents()
		{
			this.comboNetworkCard.SelectedIndexChanged += delegate(Object sender, EventArgs e)
			{
				VMQemuNetworkCard value = (VMQemuNetworkCard)((EnumValue)this.comboNetworkCard.SelectedItem).Value;
				if (value != this.PageProperties.NetworkCard)
				{
					this.PageProperties.NetworkCard = value;
					this.IsDirty = true;
				}
			};

			this.comboAudioCard.SelectedIndexChanged += delegate(Object sender, EventArgs e)
			{
				VMQemuAudioCard value = (VMQemuAudioCard)((EnumValue)this.comboAudioCard.SelectedItem).Value;
				if (value != this.PageProperties.AudioCard)
				{
					this.PageProperties.AudioCard = value;
					this.IsDirty = true;
				}
			};

			this.checkNetworkTAP.CheckedChanged += delegate(Object sender, EventArgs e)
			{
				Boolean value = this.checkNetworkTAP.Checked;
				if (value != this.PageProperties.EnableNetworkTAP)
				{
					this.PageProperties.EnableNetworkTAP = value;
					this.IsDirty = true;
				}
			};
		}

		public override PropertiesBase Properties
		{ get { return this.projProperties; } }

		protected VMQemuProperties PageProperties
		{ get { return (VMQemuProperties)this.projProperties; } }

		public override void FillProperties()
		{
			base.FillProperties();

			this.PageProperties.Reset();

			this.PageProperties.SetProperty("QemuNetworkCard", this.GetConfigProperty("QemuNetworkCard"));
			this.PageProperties.SetProperty("QemuAudioCard", this.GetConfigProperty("QemuAudioCard"));
			this.PageProperties.SetProperty("QemuNetworkTAP", this.GetConfigProperty("QemuNetworkTAP"));
			this.PageProperties.SetProperty("EnableGDB", this.GetConfigProperty("EnableGDB"));

			this.comboNetworkCard.SelectedItem = EnumValue.Find(this.comboNetworkCard.Items, this.PageProperties.NetworkCard);
			this.comboAudioCard.SelectedItem = EnumValue.Find(this.comboAudioCard.Items, this.PageProperties.AudioCard);
			this.checkNetworkTAP.Checked = this.PageProperties.EnableNetworkTAP;
		}
	}
}
