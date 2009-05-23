using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Cosmos.Builder.Common;

namespace Cosmos.VS.Package
{
	[Guid(Guids.BuildOptionsPropertyPage)]
	public partial class BuildOptionsPropertyPage : ConfigurationBase
	{
		public BuildOptionsPropertyPage()
		{
			InitializeComponent();

			this.comboTarget.Items.AddRange(EnumValue.GetEnumValues(typeof(TargetHost)));
			this.comboFramework.Items.AddRange(EnumValue.GetEnumValues(typeof(Framework)));
		}
	}
}
