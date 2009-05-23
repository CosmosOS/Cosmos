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
	public partial class DebugOptionsQemu : SubPropertyPageBase
	{
		public DebugOptionsQemu(CustomPropertyPage owner) : base(owner)
		{

			InitializeComponent();

			this.comboCommunication.Items.AddRange(EnumValue.GetEnumValues(typeof(DebugQemuCommunication)));
		}
	}
}
