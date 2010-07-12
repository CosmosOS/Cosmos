namespace Cosmos.Debug.GDB {
    partial class FormLog {
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
            this.lboxDebug = new System.Windows.Forms.ListBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mitmCopyToClipboard = new System.Windows.Forms.ToolStripMenuItem();
            this.mitmClear = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lboxDebug
            // 
            this.lboxDebug.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lboxDebug.FormattingEnabled = true;
            this.lboxDebug.Location = new System.Drawing.Point(0, 24);
            this.lboxDebug.Name = "lboxDebug";
            this.lboxDebug.Size = new System.Drawing.Size(284, 238);
            this.lboxDebug.TabIndex = 5;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(284, 24);
            this.menuStrip1.TabIndex = 6;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mitmCopyToClipboard,
            this.mitmClear});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // mitmCopyToClipboard
            // 
            this.mitmCopyToClipboard.Name = "mitmCopyToClipboard";
            this.mitmCopyToClipboard.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.mitmCopyToClipboard.Size = new System.Drawing.Size(213, 22);
            this.mitmCopyToClipboard.Text = "&Copy to Clipboard";
            this.mitmCopyToClipboard.Click += new System.EventHandler(this.mitmCopyToClipboard_Click);
            // 
            // mitmClear
            // 
            this.mitmClear.Name = "mitmClear";
            this.mitmClear.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.L)));
            this.mitmClear.Size = new System.Drawing.Size(213, 22);
            this.mitmClear.Text = "C&lear";
            this.mitmClear.Click += new System.EventHandler(this.mitmClear_Click);
            // 
            // FormLog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.lboxDebug);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FormLog";
            this.Text = "Debug Log";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lboxDebug;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mitmCopyToClipboard;
        private System.Windows.Forms.ToolStripMenuItem mitmClear;
    }
}