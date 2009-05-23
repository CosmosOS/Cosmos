using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio;

namespace Cosmos.VS.Package
{
	public partial class TitleDivision : UserControl
	{
		public TitleDivision()
		{
			InitializeComponent();

			this.Resize += new EventHandler(TitleDivision_Resize);
			this.Paint += new PaintEventHandler(TitleDivision_Paint);
			this.labelTitle.Resize += new EventHandler(Title_Resize);
			this.FontChanged += new EventHandler(TitleDivision_FontChanged);
			
			this.LineColor = SystemColors.ControlDark;
		}

		void TitleDivision_FontChanged(object sender, EventArgs e)
		{
			this.labelTitle.Font = this.Font;
		}

		void TitleDivision_Resize(object sender, EventArgs e)
		{
			Single halfHeight;
			halfHeight = this.Size.Height * 0.5f;
			halfHeight = halfHeight - (this.labelTitle.Size.Height * 0.5f);

			this.labelTitle.Location = new Point(0, (Int32)halfHeight);
			this.Invalidate();
		}

		void TitleDivision_Paint(object sender, PaintEventArgs e)
		{
			Pen linePen;
			Single halfHeight;
			Single start;
			Single end;

			halfHeight = (this.Size.Height * 0.5f) + 1.0f;
			start = (Single)this.labelTitle.Right;

			if (String.IsNullOrEmpty(this.labelTitle.Text) == false)
			{
				start += (Single)this.labelTitle.Margin.Right;
			}else{
				start += (Single)this.Padding.Left;
			}

			end = this.Size.Width - (Single)this.Padding.Right;

			linePen = new Pen(this.LineColor);

			e.Graphics.DrawLine(linePen, start, halfHeight, end, halfHeight);

			linePen.Dispose();
			linePen = null;
		}

		void  Title_Resize(object sender, EventArgs e)
		{
			this.Size = new Size(this.Size.Width, this.labelTitle.Size.Height);
			this.Invalidate();
		}

		[SRCategoryAttribute("Category")]
		[DisplayName("Line Color")]
		[SRDescriptionAttribute("Description")]
		public Color LineColor
		{ get; set; }

		[SRCategoryAttribute("Category")]
		[DisplayName("Title")]
		[SRDescriptionAttribute("Description")]
		public String Title
		{
			get{ return this.labelTitle.Text; }
			set{ this.labelTitle.Text = value; }
		}

	}
}
