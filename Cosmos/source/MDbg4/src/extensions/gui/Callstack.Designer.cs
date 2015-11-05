namespace gui
{
    partial class Callstack
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
            this.listBoxCallstack = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
// 
// listBoxCallstack
// 
            this.listBoxCallstack.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxCallstack.AutoSize = true;
            this.listBoxCallstack.FormattingEnabled = true;
            this.listBoxCallstack.Location = new System.Drawing.Point(0, 0);
            this.listBoxCallstack.Name = "listBoxCallstack";
            this.listBoxCallstack.Size = new System.Drawing.Size(296, 277);
            this.listBoxCallstack.TabIndex = 0;
            this.listBoxCallstack.DoubleClick += new System.EventHandler(this.listBoxCallstack_DoubleClicked);
// 
// Callstack
// 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.Controls.Add(this.listBoxCallstack);
            this.Name = "Callstack";
            this.Text = "Callstack";
            this.Load += new System.EventHandler(this.Callstack_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBoxCallstack;
    }
}