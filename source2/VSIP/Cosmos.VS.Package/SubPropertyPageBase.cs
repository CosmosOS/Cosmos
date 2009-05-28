using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Cosmos.VS.Package
{
	public partial class SubPropertyPageBase : UserControl
	{
		private CustomPropertyPage subOwner;

		public SubPropertyPageBase(CustomPropertyPage owner)
		{
			InitializeComponent();

			this.subOwner = owner;
		}

		protected CustomPropertyPage Owner
		{ get { return this.subOwner; } }

		public virtual void FillProperties()
		{}

		public virtual void ApplyChanges()
		{}

	}
}
