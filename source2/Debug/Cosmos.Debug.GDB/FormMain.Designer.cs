namespace Cosmos.Debug.GDB {
    partial class FormMain {
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mitmConnect = new System.Windows.Forms.ToolStripMenuItem();
            this.mitmRefresh = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.mitmExit = new System.Windows.Forms.ToolStripMenuItem();
            this.executeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mitmStepInto = new System.Windows.Forms.ToolStripMenuItem();
            this.mitmStepOver = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.continueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mitmMainViewCallStack = new System.Windows.Forms.ToolStripMenuItem();
            this.mitmMainViewWatches = new System.Windows.Forms.ToolStripMenuItem();
            this.mitmViewLog = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 24);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(160, 30);
            this.panel1.TabIndex = 3;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.executeToolStripMenuItem,
            this.viewToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(160, 24);
            this.menuStrip1.TabIndex = 5;
            this.menuStrip1.Text = "E&xecute";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mitmConnect,
            this.mitmRefresh,
            this.toolStripMenuItem2,
            this.mitmExit});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // mitmConnect
            // 
            this.mitmConnect.Name = "mitmConnect";
            this.mitmConnect.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.mitmConnect.Size = new System.Drawing.Size(162, 22);
            this.mitmConnect.Text = "&Connect";
            this.mitmConnect.Click += new System.EventHandler(this.mitmConnect_Click);
            // 
            // mitmRefresh
            // 
            this.mitmRefresh.Name = "mitmRefresh";
            this.mitmRefresh.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
            this.mitmRefresh.Size = new System.Drawing.Size(162, 22);
            this.mitmRefresh.Text = "Refresh";
            this.mitmRefresh.Click += new System.EventHandler(this.mitmRefresh_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(159, 6);
            // 
            // mitmExit
            // 
            this.mitmExit.Name = "mitmExit";
            this.mitmExit.Size = new System.Drawing.Size(162, 22);
            this.mitmExit.Text = "E&xit";
            this.mitmExit.Click += new System.EventHandler(this.mitmExit_Click);
            // 
            // executeToolStripMenuItem
            // 
            this.executeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mitmStepInto,
            this.mitmStepOver,
            this.toolStripMenuItem1,
            this.continueToolStripMenuItem});
            this.executeToolStripMenuItem.Name = "executeToolStripMenuItem";
            this.executeToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
            this.executeToolStripMenuItem.Text = "&Run";
            // 
            // mitmStepInto
            // 
            this.mitmStepInto.Name = "mitmStepInto";
            this.mitmStepInto.ShortcutKeys = System.Windows.Forms.Keys.F11;
            this.mitmStepInto.Size = new System.Drawing.Size(150, 22);
            this.mitmStepInto.Text = "Step &Into";
            this.mitmStepInto.Click += new System.EventHandler(this.mitmStepInto_Click);
            // 
            // mitmStepOver
            // 
            this.mitmStepOver.Name = "mitmStepOver";
            this.mitmStepOver.ShortcutKeys = System.Windows.Forms.Keys.F10;
            this.mitmStepOver.Size = new System.Drawing.Size(150, 22);
            this.mitmStepOver.Text = "Step &Over";
            this.mitmStepOver.Click += new System.EventHandler(this.mitmStepOver_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(147, 6);
            // 
            // continueToolStripMenuItem
            // 
            this.continueToolStripMenuItem.Name = "continueToolStripMenuItem";
            this.continueToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.continueToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.continueToolStripMenuItem.Text = "&Continue";
            this.continueToolStripMenuItem.Click += new System.EventHandler(this.continueToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mitmMainViewCallStack,
            this.mitmMainViewWatches,
            this.mitmViewLog});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "&View";
            // 
            // mitmMainViewCallStack
            // 
            this.mitmMainViewCallStack.Name = "mitmMainViewCallStack";
            this.mitmMainViewCallStack.Size = new System.Drawing.Size(122, 22);
            this.mitmMainViewCallStack.Text = "&CallStack";
            this.mitmMainViewCallStack.Click += new System.EventHandler(this.mitmMainViewCallStack_Click);
            // 
            // mitmMainViewWatches
            // 
            this.mitmMainViewWatches.Name = "mitmMainViewWatches";
            this.mitmMainViewWatches.Size = new System.Drawing.Size(122, 22);
            this.mitmMainViewWatches.Text = "&Watches";
            this.mitmMainViewWatches.Click += new System.EventHandler(this.mitmMainViewWatches_Click);
            // 
            // mitmViewLog
            // 
            this.mitmViewLog.Name = "mitmViewLog";
            this.mitmViewLog.Size = new System.Drawing.Size(122, 22);
            this.mitmViewLog.Text = "&Log";
            this.mitmViewLog.Click += new System.EventHandler(this.mitmViewLog_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(160, 54);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FormMain";
            this.Text = "Cosmos GDB Debugger";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.Resize += new System.EventHandler(this.FormMain_Resize);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mitmExit;
        private System.Windows.Forms.ToolStripMenuItem executeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mitmStepInto;
        private System.Windows.Forms.ToolStripMenuItem mitmStepOver;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem continueToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mitmConnect;
        private System.Windows.Forms.ToolStripMenuItem mitmRefresh;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mitmMainViewCallStack;
        private System.Windows.Forms.ToolStripMenuItem mitmMainViewWatches;
        private System.Windows.Forms.ToolStripMenuItem mitmViewLog;
    }
}

