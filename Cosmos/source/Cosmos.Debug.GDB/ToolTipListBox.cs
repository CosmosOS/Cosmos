using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Cosmos.Debug.GDB
{
	class ToolTipListBox : ListView
	{
		private ToolTip mToolTip = new ToolTip();
		protected int mLastIndex = -1;
		protected IList<AsmLine> mItems;
		protected ListViewItem[] mCache;

		public ToolTipListBox()
		{
			base.VirtualMode = true;
			base.Scrollable = true;
			base.View = System.Windows.Forms.View.Details;
			var header = new ColumnHeader();
			header.Text = "Source";
			this.Columns.Add(header);
			this.HeaderStyle = ColumnHeaderStyle.None;
			this.FullRowSelect = true;
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			if(base.Columns.Count > 0)
				base.Columns[0].Width = this.Width - SystemInformation.VerticalScrollBarWidth - 4;
		}

		protected override void OnRetrieveVirtualItem(RetrieveVirtualItemEventArgs e)
		{
			base.OnRetrieveVirtualItem(e);

			if (mCache[e.ItemIndex] != null) {
				e.Item = mCache[e.ItemIndex];
			}
			else {
				ListViewItem item = new ListViewItem(mItems[e.ItemIndex].OrignalLine.Replace("\t", "  "));
				e.Item = item;
				mCache[e.ItemIndex] = item;
			}
		}

		protected override void Dispose(bool disposing)
		{
			mToolTip.Dispose();
			base.Dispose(disposing);
		}

		#region ToolTip
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			Point p = base.PointToClient(Cursor.Position);
			if (this.mCache == null)
				return;
			var item = base.GetItemAt(p.X, p.Y);
			if (item == null)
				return;
			int index = item.Index;
			if (index > -1)
			{
				if (index != mLastIndex)
				{
					AsmLine line = (AsmLine)mItems[index];

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
		#endregion

		public void SetItems(IList<AsmLine> o)
		{
			this.mItems = o;
			base.VirtualListSize = o.Count;
			this.mCache = new ListViewItem[o.Count];
		}

		public uint SelectedAddress
		{
			get
			{
				if (base.SelectedIndices.Count > 0)
					return mItems[base.SelectedIndices[0]].Address;
				return 0u;
			}
			set
			{
				for (int i = 0; i < mItems.Count; i++)
				{
					if (mItems[i].Address == value)
					{
						base.SelectedIndices.Clear();
						base.SelectedIndices.Add(i);
						base.EnsureVisible(i);
						base.Select();
						break;
					}
				}
			}
		}
	}
}