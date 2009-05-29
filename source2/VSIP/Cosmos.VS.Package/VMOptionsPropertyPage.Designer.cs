namespace Cosmos.VS.Package
{
	partial class VMOptionsPropertyPage
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.panelSubPage = new System.Windows.Forms.Panel();
			this.labelNoConfig = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// panelSubPage
			// 
			this.panelSubPage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.panelSubPage.AutoScroll = true;
			this.panelSubPage.Location = new System.Drawing.Point(0, 41);
			this.panelSubPage.Margin = new System.Windows.Forms.Padding(0);
			this.panelSubPage.Name = "panelSubPage";
			this.panelSubPage.Size = new System.Drawing.Size(492, 247);
			this.panelSubPage.TabIndex = 4;
			this.panelSubPage.Visible = false;
			// 
			// labelNoConfig
			// 
			this.labelNoConfig.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.labelNoConfig.ForeColor = System.Drawing.SystemColors.GrayText;
			this.labelNoConfig.Location = new System.Drawing.Point(0, 41);
			this.labelNoConfig.Name = "labelNoConfig";
			this.labelNoConfig.Size = new System.Drawing.Size(492, 17);
			this.labelNoConfig.TabIndex = 5;
			this.labelNoConfig.Text = "<no virtual machine configuration avaliable for current build target>";
			this.labelNoConfig.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// VMOptionsPropertyPage
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.panelSubPage);
			this.Controls.Add(this.labelNoConfig);
			this.Name = "VMOptionsPropertyPage";
			this.Title = "Virtual Machine";
			this.Controls.SetChildIndex(this.labelNoConfig, 0);
			this.Controls.SetChildIndex(this.panelSubPage, 0);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panelSubPage;
		private System.Windows.Forms.Label labelNoConfig;
	}
}
