namespace Cosmos.VS.Package
{
	partial class DebugOptionsQemu
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.tableDebugQemu = new System.Windows.Forms.TableLayoutPanel();
			this.titleGeneral = new Cosmos.VS.Package.TitleDivision();
			this.tableGeneral = new System.Windows.Forms.TableLayoutPanel();
			this.labelCommunication = new System.Windows.Forms.Label();
			this.comboCommunication = new System.Windows.Forms.ComboBox();
			this.tableDebugQemu.SuspendLayout();
			this.tableGeneral.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableDebugQemu
			// 
			this.tableDebugQemu.ColumnCount = 1;
			this.tableDebugQemu.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableDebugQemu.Controls.Add(this.titleGeneral, 0, 0);
			this.tableDebugQemu.Controls.Add(this.tableGeneral, 0, 1);
			this.tableDebugQemu.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableDebugQemu.Location = new System.Drawing.Point(0, 0);
			this.tableDebugQemu.Margin = new System.Windows.Forms.Padding(0);
			this.tableDebugQemu.Name = "tableDebugQemu";
			this.tableDebugQemu.RowCount = 3;
			this.tableDebugQemu.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableDebugQemu.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableDebugQemu.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableDebugQemu.Size = new System.Drawing.Size(620, 213);
			this.tableDebugQemu.TabIndex = 0;
			// 
			// titleGeneral
			// 
			this.titleGeneral.BackColor = System.Drawing.Color.Transparent;
			this.titleGeneral.Dock = System.Windows.Forms.DockStyle.Top;
			this.titleGeneral.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
			this.titleGeneral.LineColor = System.Drawing.SystemColors.ControlDark;
			this.titleGeneral.Location = new System.Drawing.Point(3, 3);
			this.titleGeneral.Margin = new System.Windows.Forms.Padding(3, 3, 3, 12);
			this.titleGeneral.Name = "titleGeneral";
			this.titleGeneral.Size = new System.Drawing.Size(614, 15);
			this.titleGeneral.TabIndex = 0;
			this.titleGeneral.Title = "General";
			// 
			// tableGeneral
			// 
			this.tableGeneral.ColumnCount = 2;
			this.tableGeneral.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
			this.tableGeneral.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
			this.tableGeneral.Controls.Add(this.labelCommunication, 0, 0);
			this.tableGeneral.Controls.Add(this.comboCommunication, 1, 0);
			this.tableGeneral.Location = new System.Drawing.Point(22, 30);
			this.tableGeneral.Margin = new System.Windows.Forms.Padding(22, 0, 0, 0);
			this.tableGeneral.Name = "tableGeneral";
			this.tableGeneral.RowCount = 1;
			this.tableGeneral.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableGeneral.Size = new System.Drawing.Size(598, 27);
			this.tableGeneral.TabIndex = 1;
			// 
			// labelCommunication
			// 
			this.labelCommunication.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelCommunication.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelCommunication.Location = new System.Drawing.Point(0, 3);
			this.labelCommunication.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
			this.labelCommunication.Name = "labelCommunication";
			this.labelCommunication.Size = new System.Drawing.Size(179, 21);
			this.labelCommunication.TabIndex = 2;
			this.labelCommunication.Text = "Communication Mode:";
			this.labelCommunication.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// comboCommunication
			// 
			this.comboCommunication.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboCommunication.FormattingEnabled = true;
			this.comboCommunication.Location = new System.Drawing.Point(182, 3);
			this.comboCommunication.Name = "comboCommunication";
			this.comboCommunication.Size = new System.Drawing.Size(265, 21);
			this.comboCommunication.TabIndex = 13;
			// 
			// DebugOptionsQemu
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableDebugQemu);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Name = "DebugOptionsQemu";
			this.Size = new System.Drawing.Size(620, 213);
			this.tableDebugQemu.ResumeLayout(false);
			this.tableGeneral.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableDebugQemu;
		private TitleDivision titleGeneral;
		private System.Windows.Forms.TableLayoutPanel tableGeneral;
		private System.Windows.Forms.Label labelCommunication;
		private System.Windows.Forms.ComboBox comboCommunication;
	}
}
