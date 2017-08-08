namespace Cosmos.VS.Package
{
	partial class UnsavedConfigChangesDialog
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.labelMessage = new System.Windows.Forms.Label();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonNo = new System.Windows.Forms.Button();
			this.buttonYes = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// labelMessage
			// 
			this.labelMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.labelMessage.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelMessage.Location = new System.Drawing.Point(12, 9);
			this.labelMessage.Name = "labelMessage";
			this.labelMessage.Size = new System.Drawing.Size(366, 91);
			this.labelMessage.TabIndex = 0;
			this.labelMessage.Text = "The current configuration \'{0}\' has unsaved changes, would you like to save these" +
				" changes now?";
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonCancel.Location = new System.Drawing.Point(303, 103);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(75, 26);
			this.buttonCancel.TabIndex = 1;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// buttonNo
			// 
			this.buttonNo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonNo.DialogResult = System.Windows.Forms.DialogResult.No;
			this.buttonNo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonNo.Location = new System.Drawing.Point(222, 103);
			this.buttonNo.Name = "buttonNo";
			this.buttonNo.Size = new System.Drawing.Size(75, 26);
			this.buttonNo.TabIndex = 2;
			this.buttonNo.Text = "No";
			this.buttonNo.UseVisualStyleBackColor = true;
			this.buttonNo.Click += new System.EventHandler(this.buttonNo_Click);
			// 
			// buttonYes
			// 
			this.buttonYes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonYes.DialogResult = System.Windows.Forms.DialogResult.Yes;
			this.buttonYes.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonYes.Location = new System.Drawing.Point(141, 103);
			this.buttonYes.Name = "buttonYes";
			this.buttonYes.Size = new System.Drawing.Size(75, 26);
			this.buttonYes.TabIndex = 3;
			this.buttonYes.Text = "Yes";
			this.buttonYes.UseVisualStyleBackColor = true;
			this.buttonYes.Click += new System.EventHandler(this.buttonYes_Click);
			// 
			// UnsavedConfigurationChanges
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(390, 141);
			this.Controls.Add(this.buttonYes);
			this.Controls.Add(this.buttonNo);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.labelMessage);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "UnsavedConfigurationChanges";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Project Properties";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label labelMessage;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonNo;
		private System.Windows.Forms.Button buttonYes;
	}
}