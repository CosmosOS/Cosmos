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
            this.lablConnected = new System.Windows.Forms.Label();
            this.butnContinue = new System.Windows.Forms.Button();
            this.butnConnect = new System.Windows.Forms.Button();
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
            this.mitmContinue = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mitmViewBreakpoints = new System.Windows.Forms.ToolStripMenuItem();
            this.mitmMainViewCallStack = new System.Windows.Forms.ToolStripMenuItem();
            this.mitmViewDisassembly = new System.Windows.Forms.ToolStripMenuItem();
            this.mitmViewLog = new System.Windows.Forms.ToolStripMenuItem();
            this.mitmRegisters = new System.Windows.Forms.ToolStripMenuItem();
            this.mitmMainViewWatches = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.mitmWindowsToForeground = new System.Windows.Forms.ToolStripMenuItem();
            this.lablRunning = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lablRunning);
            this.panel1.Controls.Add(this.lablConnected);
            this.panel1.Controls.Add(this.butnContinue);
            this.panel1.Controls.Add(this.butnConnect);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 24);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(273, 30);
            this.panel1.TabIndex = 3;
            // 
            // lablConnected
            // 
            this.lablConnected.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lablConnected.AutoSize = true;
            this.lablConnected.Location = new System.Drawing.Point(211, 8);
            this.lablConnected.Name = "lablConnected";
            this.lablConnected.Size = new System.Drawing.Size(59, 13);
            this.lablConnected.TabIndex = 2;
            this.lablConnected.Text = "Connected";
            this.lablConnected.Visible = false;
            // 
            // butnContinue
            // 
            this.butnContinue.Location = new System.Drawing.Point(65, 3);
            this.butnContinue.Name = "butnContinue";
            this.butnContinue.Size = new System.Drawing.Size(26, 23);
            this.butnContinue.TabIndex = 1;
            this.butnContinue.Text = ">";
            this.butnContinue.UseVisualStyleBackColor = true;
            this.butnContinue.Click += new System.EventHandler(this.continueToolStripMenuItem_Click);
            // 
            // butnConnect
            // 
            this.butnConnect.Location = new System.Drawing.Point(3, 3);
            this.butnConnect.Name = "butnConnect";
            this.butnConnect.Size = new System.Drawing.Size(56, 23);
            this.butnConnect.TabIndex = 0;
            this.butnConnect.Text = "Connect";
            this.butnConnect.UseVisualStyleBackColor = true;
            this.butnConnect.Click += new System.EventHandler(this.mitmConnect_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.executeToolStripMenuItem,
            this.viewToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(273, 24);
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
            this.mitmContinue});
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
            // mitmContinue
            // 
            this.mitmContinue.Name = "mitmContinue";
            this.mitmContinue.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.mitmContinue.Size = new System.Drawing.Size(150, 22);
            this.mitmContinue.Text = "&Continue";
            this.mitmContinue.Click += new System.EventHandler(this.continueToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mitmViewBreakpoints,
            this.mitmMainViewCallStack,
            this.mitmViewDisassembly,
            this.mitmViewLog,
            this.mitmRegisters,
            this.mitmMainViewWatches,
            this.toolStripMenuItem3,
            this.mitmWindowsToForeground});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "&View";
            // 
            // mitmViewBreakpoints
            // 
            this.mitmViewBreakpoints.Name = "mitmViewBreakpoints";
            this.mitmViewBreakpoints.Size = new System.Drawing.Size(215, 22);
            this.mitmViewBreakpoints.Text = "&Breakpoints";
            this.mitmViewBreakpoints.Click += new System.EventHandler(this.mitmViewBreakpoints_Click);
            // 
            // mitmMainViewCallStack
            // 
            this.mitmMainViewCallStack.Name = "mitmMainViewCallStack";
            this.mitmMainViewCallStack.Size = new System.Drawing.Size(215, 22);
            this.mitmMainViewCallStack.Text = "&CallStack";
            this.mitmMainViewCallStack.Click += new System.EventHandler(this.mitmMainViewCallStack_Click);
            // 
            // mitmViewDisassembly
            // 
            this.mitmViewDisassembly.Name = "mitmViewDisassembly";
            this.mitmViewDisassembly.Size = new System.Drawing.Size(215, 22);
            this.mitmViewDisassembly.Text = "&Disassembly";
            this.mitmViewDisassembly.Click += new System.EventHandler(this.mitmViewDisassembly_Click);
            // 
            // mitmViewLog
            // 
            this.mitmViewLog.Name = "mitmViewLog";
            this.mitmViewLog.Size = new System.Drawing.Size(215, 22);
            this.mitmViewLog.Text = "&Log";
            this.mitmViewLog.Click += new System.EventHandler(this.mitmViewLog_Click);
            // 
            // mitmRegisters
            // 
            this.mitmRegisters.Name = "mitmRegisters";
            this.mitmRegisters.Size = new System.Drawing.Size(215, 22);
            this.mitmRegisters.Text = "&Registers";
            this.mitmRegisters.Click += new System.EventHandler(this.mitmRegisters_Click);
            // 
            // mitmMainViewWatches
            // 
            this.mitmMainViewWatches.Name = "mitmMainViewWatches";
            this.mitmMainViewWatches.Size = new System.Drawing.Size(215, 22);
            this.mitmMainViewWatches.Text = "&Watches";
            this.mitmMainViewWatches.Click += new System.EventHandler(this.mitmMainViewWatches_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(212, 6);
            // 
            // mitmWindowsToForeground
            // 
            this.mitmWindowsToForeground.Name = "mitmWindowsToForeground";
            this.mitmWindowsToForeground.Size = new System.Drawing.Size(215, 22);
            this.mitmWindowsToForeground.Text = "All windows to foreground";
            this.mitmWindowsToForeground.Click += new System.EventHandler(this.mitmWindowsToForeground_Click);
            // 
            // lablRunning
            // 
            this.lablRunning.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lablRunning.AutoSize = true;
            this.lablRunning.Location = new System.Drawing.Point(146, 8);
            this.lablRunning.Name = "lablRunning";
            this.lablRunning.Size = new System.Drawing.Size(47, 13);
            this.lablRunning.TabIndex = 3;
            this.lablRunning.Text = "Running";
            this.lablRunning.Visible = false;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(273, 54);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.Name = "FormMain";
            this.Text = "Cosmos GDB Debugger";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.Shown += new System.EventHandler(this.FormMain_Shown);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.Resize += new System.EventHandler(this.FormMain_Resize);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
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
        private System.Windows.Forms.ToolStripMenuItem mitmContinue;
        private System.Windows.Forms.ToolStripMenuItem mitmConnect;
        private System.Windows.Forms.ToolStripMenuItem mitmRefresh;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mitmMainViewCallStack;
        private System.Windows.Forms.ToolStripMenuItem mitmMainViewWatches;
        private System.Windows.Forms.ToolStripMenuItem mitmViewLog;
        private System.Windows.Forms.ToolStripMenuItem mitmViewBreakpoints;
        private System.Windows.Forms.ToolStripMenuItem mitmViewDisassembly;
        private System.Windows.Forms.ToolStripMenuItem mitmRegisters;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem mitmWindowsToForeground;
        private System.Windows.Forms.Button butnConnect;
        private System.Windows.Forms.Button butnContinue;
        private System.Windows.Forms.Label lablConnected;
        private System.Windows.Forms.Label lablRunning;
    }
}

