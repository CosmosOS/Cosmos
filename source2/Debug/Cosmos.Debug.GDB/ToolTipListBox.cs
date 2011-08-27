using System;
using System.Windows.Forms;
using System.Drawing;

namespace Cosmos.Debug.GDB
{
	class ToolTipListBox : ListBox
	{
		private ToolTip mToolTip = new ToolTip();
		protected int mLastIndex = -1;

		protected override void Dispose(bool disposing)
		{
			mToolTip.Dispose();
			base.Dispose(disposing);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			Point p = base.PointToClient(Cursor.Position);
			int index = base.IndexFromPoint(p);
			if (index > -1)
			{
				if (index != mLastIndex)
				{
					AsmLine line = (AsmLine)base.Items[index];

					if (line.GDBLine != null)
					{
						this.mToolTip.SetToolTip(this, line.GDBLine);
						mLastIndex = index;
					}
					else
						ResetToolTip();
				}
			}
			else
			{
				ResetToolTip();
			}
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			ResetToolTip();
		}

		protected void ResetToolTip()
		{
			this.mToolTip.SetToolTip(this, null);
			mLastIndex = -1;
		}
	}
}