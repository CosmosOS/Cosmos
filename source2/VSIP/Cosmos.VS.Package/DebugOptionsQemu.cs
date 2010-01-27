using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Cosmos.Build.Common;

namespace Cosmos.VS.Package
{
	public partial class DebugOptionsQemu : SubPropertyPageBase 
	{
		private DebugQemuProperties projProperties;

		public DebugOptionsQemu()
		{
			InitializeComponent();

			this.comboCommunication.Items.AddRange(EnumValue.GetEnumValues(typeof(DebugQemuCommunication)));
            this.comboDebugMode.Items.AddRange(EnumValue.GetEnumValues(typeof(DebugMode)));
            comboTraceMode.Items.AddRange(EnumValue.GetEnumValues(typeof(TraceAssemblies)));

			this.projProperties = new DebugQemuProperties();

			this.CreateUIMonitorEvents();
		}

		private void CreateUIMonitorEvents()
		{
			this.comboCommunication.SelectedIndexChanged += delegate(Object sender, EventArgs e)
			{
				DebugQemuCommunication value = (DebugQemuCommunication)((EnumValue)this.comboCommunication.SelectedItem).Value;
				if (value != this.PageProperties.Communication)
				{
					this.PageProperties.Communication = value;
					this.IsDirty = true;
				}
			};
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

			this.comboCommunication.SelectedItem = EnumValue.Find(this.comboCommunication.Items, this.PageProperties.Communication);
		}

        private void tableDebugQemu_Paint(object sender, PaintEventArgs e)
        {

        }
	}
}
