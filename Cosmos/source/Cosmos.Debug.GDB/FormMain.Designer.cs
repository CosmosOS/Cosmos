using System.Windows.Forms;

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
			this.components = new System.ComponentModel.Container();
			this.panel1 = new System.Windows.Forms.Panel();
			this.butnBreak = new System.Windows.Forms.Button();
			this.butnBreakpoints = new System.Windows.Forms.Button();
			this.lablRunning = new Label();
			this.lablConnected = new Label();
			this.butnContinue = new System.Windows.Forms.Button();
			this.butnConnect = new System.Windows.Forms.Button();
			this.menuMain = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.mitmConnect = new System.Windows.Forms.ToolStripMenuItem();
			this.mitmSave = new System.Windows.Forms.ToolStripMenuItem();
			this.mitmRefresh = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
			this.mitmExit = new System.Windows.Forms.ToolStripMenuItem();
			this.executeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.mitmStepInto = new System.Windows.Forms.ToolStripMenuItem();
			this.mitmStepOver = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.mitmContinue = new System.Windows.Forms.ToolStripMenuItem();
			this.mitmBreak = new System.Windows.Forms.ToolStripMenuItem();
			this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.mitmViewBreakpoints = new System.Windows.Forms.ToolStripMenuItem();
			this.mitmMainViewCallStack = new System.Windows.Forms.ToolStripMenuItem();
			this.mitmViewLog = new System.Windows.Forms.ToolStripMenuItem();
			this.mitmRegisters = new System.Windows.Forms.ToolStripMenuItem();
			this.mitmMainViewWatches = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
			this.mitmWindowsToForeground = new System.Windows.Forms.ToolStripMenuItem();
			this.panel4 = new System.Windows.Forms.Panel();
			this.textCurrentFunction = new System.Windows.Forms.TextBox();
			this.label5 = new Label();
			this.lboxDisassemble = new Cosmos.Debug.GDB.ToolTipListBox();
			this.menuDisassembly = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.mitemDisassemblyAddBreakpoint = new System.Windows.Forms.ToolStripMenuItem();
			this.mitmCopyToClipboard = new System.Windows.Forms.ToolStripMenuItem();
			this.panel1.SuspendLayout();
			this.menuMain.SuspendLayout();
			this.panel4.SuspendLayout();
			this.menuDisassembly.SuspendLayout();
			this.SuspendLayout();
			//
			// panel1
			//
			this.panel1.Controls.Add(this.butnBreak);
			this.panel1.Controls.Add(this.butnBreakpoints);
			this.panel1.Controls.Add(this.lablRunning);
			this.panel1.Controls.Add(this.lablConnected);
			this.panel1.Controls.Add(this.butnContinue);
			this.panel1.Controls.Add(this.butnConnect);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = new System.Drawing.Point(0, 24);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(589, 30);
			this.panel1.TabIndex = 3;
			//
			// butnBreak
			//
			this.butnBreak.Location = new System.Drawing.Point(117, 4);
			this.butnBreak.Name = "butnBreak";
			this.butnBreak.Size = new System.Drawing.Size(26, 23);
			this.butnBreak.TabIndex = 4;
			this.butnBreak.Text = "| |";
			this.butnBreak.UseVisualStyleBackColor = true;
			this.butnBreak.Click += new System.EventHandler(this.mitmBreak_Click);
			//
			// butnBreakpoints
			//
			this.butnBreakpoints.Location = new System.Drawing.Point(152, 4);
			this.butnBreakpoints.Name = "butnBreakpoints";
			this.butnBreakpoints.Size = new System.Drawing.Size(75, 23);
			this.butnBreakpoints.TabIndex = 2;
			this.butnBreakpoints.Text = "Breakpoints";
			this.butnBreakpoints.UseVisualStyleBackColor = true;
			this.butnBreakpoints.Click += new System.EventHandler(this.butnBreakpoints_Click);
			//
			// lablRunning
			//
			this.lablRunning.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lablRunning.AutoSize = true;
			this.lablRunning.Location = new System.Drawing.Point(462, 8);
			this.lablRunning.Name = "lablRunning";
			this.lablRunning.Size = new System.Drawing.Size(47, 13);
			this.lablRunning.TabIndex = 3;
			this.lablRunning.Text = "Running";
			this.lablRunning.Visible = false;
			//
			// lablConnected
			//
			this.lablConnected.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lablConnected.AutoSize = true;
			this.lablConnected.Location = new System.Drawing.Point(527, 8);
			this.lablConnected.Name = "lablConnected";
			this.lablConnected.Size = new System.Drawing.Size(59, 13);
			this.lablConnected.TabIndex = 2;
			this.lablConnected.Text = "Connected";
			this.lablConnected.Visible = false;
			//
			// butnContinue
			//
			this.butnContinue.Location = new System.Drawing.Point(87, 4);
			this.butnContinue.Name = "butnContinue";
			this.butnContinue.Size = new System.Drawing.Size(26, 23);
			this.butnContinue.TabIndex = 1;
			this.butnContinue.Text = ">";
			this.butnContinue.UseVisualStyleBackColor = true;
			this.butnContinue.Click += new System.EventHandler(this.mitmContinue_Click);
			//
			// butnConnect
			//
			this.butnConnect.Location = new System.Drawing.Point(3, 3);
			this.butnConnect.Name = "butnConnect";
			this.butnConnect.Size = new System.Drawing.Size(78, 23);
			this.butnConnect.TabIndex = 0;
			this.butnConnect.Text = "Connect";
			this.butnConnect.UseVisualStyleBackColor = true;
			this.butnConnect.Click += new System.EventHandler(this.mitmConnect_Click);
			//
			// menuMain
			//
			this.menuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.executeToolStripMenuItem,
            this.viewToolStripMenuItem});
			this.menuMain.Location = new System.Drawing.Point(0, 0);
			this.menuMain.Name = "menuMain";
			this.menuMain.Size = new System.Drawing.Size(589, 24);
			this.menuMain.TabIndex = 5;
			this.menuMain.Text = "E&xecute";
			//
			// fileToolStripMenuItem
			//
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mitmConnect,
            this.mitmSave,
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
			// mitmSave
			//
			this.mitmSave.Name = "mitmSave";
			this.mitmSave.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
			this.mitmSave.Size = new System.Drawing.Size(162, 22);
			this.mitmSave.Text = "&Save";
			this.mitmSave.Click += new System.EventHandler(this.mitmSave_Click);
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
            this.mitmContinue,
            this.mitmBreak});
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
			this.mitmContinue.Click += new System.EventHandler(this.mitmContinue_Click);
			//
			// mitmBreak
			//
			this.mitmBreak.Name = "mitmBreak";
			this.mitmBreak.ShortcutKeys = System.Windows.Forms.Keys.F6;
			this.mitmBreak.Size = new System.Drawing.Size(150, 22);
			this.mitmBreak.Text = "&Break";
			this.mitmBreak.Click += new System.EventHandler(this.mitmBreak_Click);
			//
			// viewToolStripMenuItem
			//
			this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mitmViewBreakpoints,
            this.mitmMainViewCallStack,
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
			// panel4
			//
			this.panel4.Controls.Add(this.textCurrentFunction);
			this.panel4.Controls.Add(this.label5);
			this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel4.Location = new System.Drawing.Point(0, 54);
			this.panel4.Name = "panel4";
			this.panel4.Size = new System.Drawing.Size(589, 32);
			this.panel4.TabIndex = 12;
			//
			// textCurrentFunction
			//
			this.textCurrentFunction.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.textCurrentFunction.Location = new System.Drawing.Point(102, 7);
			this.textCurrentFunction.Name = "textCurrentFunction";
			this.textCurrentFunction.ReadOnly = true;
			this.textCurrentFunction.Size = new System.Drawing.Size(57, 13);
			this.textCurrentFunction.TabIndex = 1;
			this.textCurrentFunction.Text = "_function_";
			this.textCurrentFunction.Visible = false;
			this.textCurrentFunction.TextChanged += new System.EventHandler(this.textCurrentFunction_TextChanged);
			//
			// label5
			//
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(8, 7);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(88, 13);
			this.label5.TabIndex = 0;
			this.label5.Text = "Current Function:";
			//
			// lboxDisassemble
			//
			this.lboxDisassemble.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lboxDisassemble.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lboxDisassemble.Location = new System.Drawing.Point(0, 86);
			this.lboxDisassemble.Name = "lboxDisassemble";
			this.lboxDisassemble.Size = new System.Drawing.Size(589, 310);
			this.lboxDisassemble.TabIndex = 13;
			this.lboxDisassemble.UseCompatibleStateImageBehavior = false;
			//
			// menuDisassembly
			//
			this.menuDisassembly.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mitemDisassemblyAddBreakpoint,
            this.mitmCopyToClipboard});
			this.menuDisassembly.Name = "menuDisassembly";
			this.menuDisassembly.Size = new System.Drawing.Size(172, 48);
			//
			// mitemDisassemblyAddBreakpoint
			//
			this.mitemDisassemblyAddBreakpoint.Name = "mitemDisassemblyAddBreakpoint";
			this.mitemDisassemblyAddBreakpoint.Size = new System.Drawing.Size(171, 22);
			this.mitemDisassemblyAddBreakpoint.Text = "&Add Breakpoint";
			this.mitemDisassemblyAddBreakpoint.Click += new System.EventHandler(this.mitemDisassemblyAddBreakpoint_Click);
			//
			// mitmCopyToClipboard
			//
			this.mitmCopyToClipboard.Name = "mitmCopyToClipboard";
			this.mitmCopyToClipboard.Size = new System.Drawing.Size(171, 22);
			this.mitmCopyToClipboard.Text = "&Copy to Clipboard";
			this.mitmCopyToClipboard.Click += new System.EventHandler(this.mitmCopyToClipboard_Click);
			//
			// FormMain
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(589, 396);
			this.Controls.Add(this.lboxDisassemble);
			this.Controls.Add(this.panel4);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.menuMain);
			this.MainMenuStrip = this.menuMain;
			this.MaximizeBox = false;
			this.Name = "FormMain";
			this.Text = "Cosmos GDB Debugger";
			this.Activated += new System.EventHandler(this.FormMain_Activated);
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormMain_FormClosed);
			this.Load += new System.EventHandler(this.FormMain_Load);
			this.Shown += new System.EventHandler(this.FormMain_Shown);
			this.Resize += new System.EventHandler(this.FormMain_Resize);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.menuMain.ResumeLayout(false);
			this.menuMain.PerformLayout();
			this.panel4.ResumeLayout(false);
			this.panel4.PerformLayout();
			this.menuDisassembly.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.MenuStrip menuMain;
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
        private System.Windows.Forms.ToolStripMenuItem mitmRegisters;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem mitmWindowsToForeground;
        private System.Windows.Forms.Button butnConnect;
        private System.Windows.Forms.Button butnContinue;
        private System.Windows.Forms.Label lablConnected;
        private System.Windows.Forms.Label lablRunning;
        private System.Windows.Forms.ToolStripMenuItem mitmSave;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.TextBox textCurrentFunction;
        private System.Windows.Forms.Label label5;
		private ToolTipListBox lboxDisassemble;
        private System.Windows.Forms.ContextMenuStrip menuDisassembly;
        private System.Windows.Forms.ToolStripMenuItem mitemDisassemblyAddBreakpoint;
        private System.Windows.Forms.ToolStripMenuItem mitmCopyToClipboard;
        private System.Windows.Forms.Button butnBreakpoints;
		private System.Windows.Forms.Button butnBreak;
		private System.Windows.Forms.ToolStripMenuItem mitmBreak;
    }
}

