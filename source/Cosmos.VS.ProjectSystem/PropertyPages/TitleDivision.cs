using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.VisualStudio.Project;

namespace Cosmos.VS.ProjectSystem.PropertyPages
{
	public partial class TitleDivision : UserControl
	{
		public TitleDivision()
		{
			InitializeComponent();

			Resize += TitleDivision_Resize;
			Paint += TitleDivision_Paint;
			labelTitle.Resize += Title_Resize;
			FontChanged += TitleDivision_FontChanged;
			
			LineColor = SystemColors.ControlDark;
		}

		void TitleDivision_FontChanged(object sender, EventArgs e)
		{
			labelTitle.Font = Font;
		}

		void TitleDivision_Resize(object sender, EventArgs e)
		{
			Single halfHeight;
			halfHeight = Size.Height * 0.5f;
			halfHeight = halfHeight - (labelTitle.Size.Height * 0.5f);

			labelTitle.Location = new Point(0, (Int32)halfHeight);
			Invalidate();
		}

		void TitleDivision_Paint(object sender, PaintEventArgs e)
		{
			Pen linePen;
			Single halfHeight;
			Single start;
			Single end;

			halfHeight = (Size.Height * 0.5f) + 1.0f;
			start = (Single)labelTitle.Right;

			if (String.IsNullOrEmpty(labelTitle.Text) == false)
			{
				start += (Single)labelTitle.Margin.Right;
			}else{
				start += (Single)Padding.Left;
			}

			end = Size.Width - (Single)Padding.Right;

			linePen = new Pen(LineColor);

			e.Graphics.DrawLine(linePen, start, halfHeight, end, halfHeight);

			linePen.Dispose();
			linePen = null;
		}

		void  Title_Resize(object sender, EventArgs e)
		{
			Size = new Size(Size.Width, labelTitle.Size.Height);
			Invalidate();
		}

		[SRCategory("Category")]
		[DisplayName("Line Color")]
		[SRDescription("Description")]
		public Color LineColor
		{ get; set; }

		[SRCategory("Category")]
		[DisplayName("Title")]
		[SRDescription("Description")]
		public String Title
		{
			get{ return labelTitle.Text; }
			set{ labelTitle.Text = value; }
		}

	}
}
