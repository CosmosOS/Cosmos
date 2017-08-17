using System.Windows.Forms;

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
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.mitmCopyToClipboard = new System.Windows.Forms.ToolStripMenuItem();
			this.mitmClear = new System.Windows.Forms.ToolStripMenuItem();
			this.panel2 = new System.Windows.Forms.Panel();
			this.butnSendCmd = new System.Windows.Forms.Button();
			this.textSendCmd = new System.Windows.Forms.TextBox();
			this.label1 = new Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.lboxDebug = new System.Windows.Forms.ListBox();
			this.lboxCmd = new System.Windows.Forms.ListBox();
			this.menuStrip1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			//
			// menuStrip1
			//
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(489, 24);
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
			// panel2
			//
			this.panel2.Controls.Add(this.butnSendCmd);
			this.panel2.Controls.Add(this.textSendCmd);
			this.panel2.Controls.Add(this.label1);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel2.Location = new System.Drawing.Point(0, 24);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(489, 33);
			this.panel2.TabIndex = 10;
			//
			// butnSendCmd
			//
			this.butnSendCmd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.butnSendCmd.Location = new System.Drawing.Point(438, 5);
			this.butnSendCmd.Name = "butnSendCmd";
			this.butnSendCmd.Size = new System.Drawing.Size(47, 23);
			this.butnSendCmd.TabIndex = 2;
			this.butnSendCmd.Text = "Send";
			this.butnSendCmd.UseVisualStyleBackColor = true;
			this.butnSendCmd.Click += new System.EventHandler(this.butnSendCmd_Click);
			//
			// textSendCmd
			//
			this.textSendCmd.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textSendCmd.Location = new System.Drawing.Point(63, 7);
			this.textSendCmd.Name = "textSendCmd";
			this.textSendCmd.Size = new System.Drawing.Size(370, 20);
			this.textSendCmd.TabIndex = 1;
			this.textSendCmd.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textSendCmd_KeyPress);
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(4, 10);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(57, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Command:";
			//
			// panel1
			//
			this.panel1.Controls.Add(this.lboxDebug);
			this.panel1.Controls.Add(this.lboxCmd);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(0, 57);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(489, 317);
			this.panel1.TabIndex = 11;
			//
			// lboxDebug
			//
			this.lboxDebug.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lboxDebug.FormattingEnabled = true;
			this.lboxDebug.Location = new System.Drawing.Point(188, 0);
			this.lboxDebug.Name = "lboxDebug";
			this.lboxDebug.Size = new System.Drawing.Size(301, 317);
			this.lboxDebug.TabIndex = 9;
			//
			// lboxCmd
			//
			this.lboxCmd.Dock = System.Windows.Forms.DockStyle.Left;
			this.lboxCmd.FormattingEnabled = true;
			this.lboxCmd.Location = new System.Drawing.Point(0, 0);
			this.lboxCmd.Name = "lboxCmd";
			this.lboxCmd.Size = new System.Drawing.Size(188, 317);
			this.lboxCmd.TabIndex = 7;
			this.lboxCmd.SelectedIndexChanged += new System.EventHandler(this.lboxCmd_SelectedIndexChanged);
			//
			// FormLog
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(489, 374);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.menuStrip1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "FormLog";
			this.ShowInTaskbar = false;
			this.Text = "Debug Log";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormLog_FormClosing);
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mitmCopyToClipboard;
        private System.Windows.Forms.ToolStripMenuItem mitmClear;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button butnSendCmd;
        private System.Windows.Forms.TextBox textSendCmd;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ListBox lboxDebug;
        private System.Windows.Forms.ListBox lboxCmd;
    }
}
