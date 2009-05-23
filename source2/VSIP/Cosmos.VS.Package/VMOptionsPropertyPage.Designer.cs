namespace Cosmos.VS.Package
{
	partial class VMOptionsPropertyPage
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
			this.panelVMConfig = new System.Windows.Forms.Panel();
			this.labelNoConfig = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// panelVMConfig
			// 
			this.panelVMConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.panelVMConfig.AutoScroll = true;
			this.panelVMConfig.Location = new System.Drawing.Point(0, 41);
			this.panelVMConfig.Margin = new System.Windows.Forms.Padding(0);
			this.panelVMConfig.Name = "panelVMConfig";
			this.panelVMConfig.Size = new System.Drawing.Size(620, 208);
			this.panelVMConfig.TabIndex = 4;
			this.panelVMConfig.Visible = false;
			// 
			// labelNoConfig
			// 
			this.labelNoConfig.ForeColor = System.Drawing.SystemColors.GrayText;
			this.labelNoConfig.Location = new System.Drawing.Point(0, 41);
			this.labelNoConfig.Name = "labelNoConfig";
			this.labelNoConfig.Size = new System.Drawing.Size(620, 17);
			this.labelNoConfig.TabIndex = 5;
			this.labelNoConfig.Text = "<no virtual machine configuration avaliable for current build target>";
			this.labelNoConfig.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// VMOptionsPropertyPage
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.panelVMConfig);
			this.Controls.Add(this.labelNoConfig);
			this.Name = "VMOptionsPropertyPage";
			this.Title = "Virtual Machine";
			this.Controls.SetChildIndex(this.labelNoConfig, 0);
			this.Controls.SetChildIndex(this.panelVMConfig, 0);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panelVMConfig;
		private System.Windows.Forms.Label labelNoConfig;
	}
}
