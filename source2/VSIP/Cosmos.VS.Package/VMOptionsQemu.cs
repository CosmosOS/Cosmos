using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Cosmos.Builder.Common;

namespace Cosmos.VS.Package
{
	public partial class VMOptionsQemu : UserControl
	{
		public VMOptionsQemu()
		{
			InitializeComponent();

			this.comboNetworkCard.Items.AddRange(EnumValue.GetEnumValues(typeof(VMQemuNetworkCard)));
			this.comboAudioCard.Items.AddRange(EnumValue.GetEnumValues(typeof(VMQemuAudioCard))); 
		}
	}
}
