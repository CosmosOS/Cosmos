namespace Cosmos.Debug.GDB {
    partial class FormWatches {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
			this.panel3 = new System.Windows.Forms.Panel();
			this.butAddWatch = new System.Windows.Forms.Button();
			this.textWatch = new System.Windows.Forms.TextBox();
			this.panl = new System.Windows.Forms.Panel();
			this.panel3.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel3
			// 
			this.panel3.Controls.Add(this.butAddWatch);
			this.panel3.Controls.Add(this.textWatch);
			this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel3.Location = new System.Drawing.Point(0, 0);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(284, 32);
			this.panel3.TabIndex = 67;
			// 
			// butAddWatch
			// 
			this.butAddWatch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.butAddWatch.Location = new System.Drawing.Point(253, 6);
			this.butAddWatch.Name = "butAddWatch";
			this.butAddWatch.Size = new System.Drawing.Size(24, 23);
			this.butAddWatch.TabIndex = 5;
			this.butAddWatch.Text = "+";
			this.butAddWatch.UseVisualStyleBackColor = true;
			this.butAddWatch.Click += new System.EventHandler(this.butAddWatch_Click);
			// 
			// textWatch
			// 
			this.textWatch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textWatch.Location = new System.Drawing.Point(8, 7);
			this.textWatch.Name = "textWatch";
			this.textWatch.Size = new System.Drawing.Size(238, 20);
			this.textWatch.TabIndex = 4;
			this.textWatch.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textWatch_KeyPress);
			// 
			// panl
			// 
			this.panl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panl.Location = new System.Drawing.Point(0, 32);
			this.panl.Name = "panl";
			this.panl.Size = new System.Drawing.Size(284, 230);
			this.panl.TabIndex = 69;
			// 
			// FormWatches
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 262);
			this.Controls.Add(this.panl);
			this.Controls.Add(this.panel3);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Name = "FormWatches";
			this.ShowInTaskbar = false;
			this.Text = "Watches";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormWatches_FormClosing);
			this.panel3.ResumeLayout(false);
			this.panel3.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button butAddWatch;
        private System.Windows.Forms.TextBox textWatch;
		private System.Windows.Forms.Panel panl;
    }
}