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
	public partial class DebugOptionsQemu : SubPropertyPageBase 
	{
		private DebugQemuProperties projProperties;

		public DebugOptionsQemu()
		{
			InitializeComponent();

			this.comboDebugMode.Items.AddRange(EnumValue.GetEnumValues(typeof(DebugMode)));
            comboTraceMode.Items.AddRange(EnumValue.GetEnumValues(typeof(TraceAssemblies)));

			this.projProperties = new DebugQemuProperties();

			this.CreateUIMonitorEvents();
		}

		private void CreateUIMonitorEvents()
		{
			this.comboDebugMode.SelectedIndexChanged += delegate(Object sender, EventArgs e)
            {
                var value = (DebugMode)((EnumValue)this.comboDebugMode.SelectedItem).Value;
                if (value != this.PageProperties.DebugMode)
                {
                    this.PageProperties.DebugMode = value;
                    this.IsDirty = true;
                }
            };
            this.comboTraceMode.SelectedIndexChanged += delegate(Object sender, EventArgs e)
            {
                var value = (TraceAssemblies)((EnumValue)this.comboTraceMode.SelectedItem).Value;
                if (value != this.PageProperties.TraceAssemblies)
                {
                    this.PageProperties.TraceAssemblies = value;
                    this.IsDirty = true;
                }
            };
            this.checkEnableGDB.CheckedChanged += delegate(Object sender, EventArgs e) {
                Boolean value = this.checkEnableGDB.Checked;
                if (value != this.PageProperties.EnableGDB) {
                    this.PageProperties.EnableGDB = value;
                    this.IsDirty = true;
                }
            };
        }

		public override PropertiesBase Properties
		{ get { return this.projProperties; } }

		protected DebugQemuProperties PageProperties
		{ get { return (DebugQemuProperties)this.Properties; } }

		public override void FillProperties()
		{
			base.FillProperties();
			this.PageProperties.Reset();
			this.PageProperties.SetProperty("QemuCommunication", this.GetConfigProperty("QemuCommunication"));
			this.checkEnableGDB.Checked = this.PageProperties.EnableGDB;
        }

        private void tableDebugQemu_Paint(object sender, PaintEventArgs e)
        {

        }
	}
}
