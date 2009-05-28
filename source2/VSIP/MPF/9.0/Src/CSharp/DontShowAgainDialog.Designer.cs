namespace Microsoft.VisualStudio.Project
{
	partial class DontShowAgainDialog
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
			try
			{
				if(disposing)
				{
					if(components != null)
					{
						components.Dispose();
					}

					if(this.bitmap != null)
					{
						this.bitmap.Dispose();
					}
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DontShowAgainDialog));
			this.messageText = new System.Windows.Forms.Label();
			this.dontShowAgain = new System.Windows.Forms.CheckBox();
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// messageText
			// 
			this.messageText.AutoEllipsis = true;
			resources.ApplyResources(this.messageText, "messageText");
			this.messageText.Name = "messageText";
			// 
			// dontShowAgain
			// 
			resources.ApplyResources(this.dontShowAgain, "dontShowAgain");
			this.dontShowAgain.Name = "dontShowAgain";
			this.dontShowAgain.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			resources.ApplyResources(this.okButton, "okButton");
			this.okButton.Name = "okButton";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.OnOKButtonClicked);
			// 
			// cancelButton
			// 
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			resources.ApplyResources(this.cancelButton, "cancelButton");
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.OnCancelButtonClicked);
			// 
			// DontShowAgainDialog
			// 
			this.AcceptButton = this.okButton;
			this.CancelButton = this.cancelButton;
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.dontShowAgain);
			this.Controls.Add(this.messageText);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.HelpButton = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "DontShowAgainDialog";
			this.HelpButtonClicked += new System.ComponentModel.CancelEventHandler(this.OnHelpButtonClicked);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.OnPaint);
			this.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.OnHelpRequested);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label messageText;
		private System.Windows.Forms.CheckBox dontShowAgain;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
	}
}