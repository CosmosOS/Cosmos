namespace Cosmos.Kernel.LogTail.Handlers
{
	partial class HeapLogHandler
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
			this.lblTotalUsage = new System.Windows.Forms.Label();
			this.lblValue = new System.Windows.Forms.Label();
			this.lblAvailMemory = new System.Windows.Forms.Label();
			this.lblAvailValue = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// lblTotalUsage
			// 
			this.lblTotalUsage.AutoSize = true;
			this.lblTotalUsage.Location = new System.Drawing.Point(16, 36);
			this.lblTotalUsage.Name = "lblTotalUsage";
			this.lblTotalUsage.Size = new System.Drawing.Size(95, 13);
			this.lblTotalUsage.TabIndex = 0;
			this.lblTotalUsage.Text = "Total heap Usage:";
			// 
			// lblValue
			// 
			this.lblValue.Location = new System.Drawing.Point(117, 36);
			this.lblValue.Name = "lblValue";
			this.lblValue.Size = new System.Drawing.Size(75, 13);
			this.lblValue.TabIndex = 1;
			this.lblValue.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// lblAvailMemory
			// 
			this.lblAvailMemory.AutoSize = true;
			this.lblAvailMemory.Location = new System.Drawing.Point(16, 12);
			this.lblAvailMemory.Name = "lblAvailMemory";
			this.lblAvailMemory.Size = new System.Drawing.Size(93, 13);
			this.lblAvailMemory.TabIndex = 2;
			this.lblAvailMemory.Text = "Available Memory:";
			// 
			// lblAvailValue
			// 
			this.lblAvailValue.Location = new System.Drawing.Point(117, 12);
			this.lblAvailValue.Name = "lblAvailValue";
			this.lblAvailValue.Size = new System.Drawing.Size(75, 13);
			this.lblAvailValue.TabIndex = 3;
			this.lblAvailValue.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// HeapLogHandler
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.lblAvailValue);
			this.Controls.Add(this.lblAvailMemory);
			this.Controls.Add(this.lblValue);
			this.Controls.Add(this.lblTotalUsage);
			this.Name = "HeapLogHandler";
			this.Size = new System.Drawing.Size(315, 265);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

		private System.Windows.Forms.Label lblTotalUsage;
		private System.Windows.Forms.Label lblValue;
		private System.Windows.Forms.Label lblAvailMemory;
		private System.Windows.Forms.Label lblAvailValue;

	}
}
