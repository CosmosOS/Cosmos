namespace Cosmos.VS.Package
{
	partial class DebugOptionsPropertyPage
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
			this.labelNoConfig = new System.Windows.Forms.Label();
			this.panelDebugConfig = new System.Windows.Forms.Panel();
			this.SuspendLayout();
			// 
			// labelNoConfig
			// 
			this.labelNoConfig.ForeColor = System.Drawing.SystemColors.GrayText;
			this.labelNoConfig.Location = new System.Drawing.Point(0, 41);
			this.labelNoConfig.Name = "labelNoConfig";
			this.labelNoConfig.Size = new System.Drawing.Size(620, 16);
			this.labelNoConfig.TabIndex = 1;
			this.labelNoConfig.Text = "<no debug configuration avaliable for current build target>";
			this.labelNoConfig.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// panelDebugConfig
			// 
			this.panelDebugConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.panelDebugConfig.AutoScroll = true;
			this.panelDebugConfig.Location = new System.Drawing.Point(0, 41);
			this.panelDebugConfig.Margin = new System.Windows.Forms.Padding(0);
			this.panelDebugConfig.Name = "panelDebugConfig";
			this.panelDebugConfig.Size = new System.Drawing.Size(620, 208);
			this.panelDebugConfig.TabIndex = 3;
			this.panelDebugConfig.Visible = false;
			// 
			// DebugOptionsPropertyPage
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.panelDebugConfig);
			this.Controls.Add(this.labelNoConfig);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
			this.Name = "DebugOptionsPropertyPage";
			this.Title = "Debug";
			this.Controls.SetChildIndex(this.labelNoConfig, 0);
			this.Controls.SetChildIndex(this.panelDebugConfig, 0);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label labelNoConfig;
		private System.Windows.Forms.Panel panelDebugConfig;


	}
}
