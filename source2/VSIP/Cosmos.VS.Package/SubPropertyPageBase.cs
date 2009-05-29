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
	public partial class SubPropertyPageBase : UserControl
	{
		private CustomPropertyPage subpageOwner;

		public SubPropertyPageBase()
		{
			InitializeComponent();
		}

		public void SetOwner( CustomPropertyPage owner )
		{
			this.subpageOwner = owner;
		}

		protected Boolean IsDirty
		{
			get { return this.subpageOwner.IsDirty; }
			set { this.subpageOwner.IsDirty = value; }
		}

		protected Boolean IgnoreDirty
		{
			get { return this.subpageOwner.IgnoreDirty; }
			set { this.subpageOwner.IgnoreDirty = value; }
		}

		public void SetConfigProperty(String name, String value)
		{ this.subpageOwner.SetConfigProperty(name, value); }

		public String GetConfigProperty(String name)
		{ return this.subpageOwner.GetConfigProperty(name); }

		public virtual PropertiesBase Properties
		{ get{ return null; } }

		public virtual void FillProperties()
		{}
	}
}
