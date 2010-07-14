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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.panel8 = new System.Windows.Forms.Panel();
            this.lboxDisassemble = new System.Windows.Forms.ListBox();
            this.menuDisassembly = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mitemDisassemblyAddBreakpoint = new System.Windows.Forms.ToolStripMenuItem();
            this.panel4 = new System.Windows.Forms.Panel();
            this.lablCurrentFunction = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.panel2 = new System.Windows.Forms.Panel();
            this.butnSendCmd = new System.Windows.Forms.Button();
            this.textSendCmd = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
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
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.panel8.SuspendLayout();
            this.menuDisassembly.SuspendLayout();
            this.panel4.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.panel2.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 24);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1043, 32);
            this.panel1.TabIndex = 3;
            // 
            // tabControl1
            // 
            this.tabControl1.Alignment = System.Windows.Forms.TabAlignment.Right;
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 56);
            this.tabControl1.Multiline = true;
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1043, 643);
            this.tabControl1.TabIndex = 4;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.panel8);
            this.tabPage1.Location = new System.Drawing.Point(4, 4);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1016, 635);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Main";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // panel8
            // 
            this.panel8.Controls.Add(this.lboxDisassemble);
            this.panel8.Controls.Add(this.panel4);
            this.panel8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel8.Location = new System.Drawing.Point(3, 3);
            this.panel8.Name = "panel8";
            this.panel8.Size = new System.Drawing.Size(1010, 629);
            this.panel8.TabIndex = 9;
            // 
            // lboxDisassemble
            // 
            this.lboxDisassemble.ContextMenuStrip = this.menuDisassembly;
            this.lboxDisassemble.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lboxDisassemble.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lboxDisassemble.FormattingEnabled = true;
            this.lboxDisassemble.ItemHeight = 19;
            this.lboxDisassemble.Location = new System.Drawing.Point(0, 32);
            this.lboxDisassemble.Name = "lboxDisassemble";
            this.lboxDisassemble.Size = new System.Drawing.Size(1010, 593);
            this.lboxDisassemble.TabIndex = 8;
            // 
            // menuDisassembly
            // 
            this.menuDisassembly.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mitemDisassemblyAddBreakpoint});
            this.menuDisassembly.Name = "menuDisassembly";
            this.menuDisassembly.Size = new System.Drawing.Size(157, 26);
            // 
            // mitemDisassemblyAddBreakpoint
            // 
            this.mitemDisassemblyAddBreakpoint.Name = "mitemDisassemblyAddBreakpoint";
            this.mitemDisassemblyAddBreakpoint.Size = new System.Drawing.Size(156, 22);
            this.mitemDisassemblyAddBreakpoint.Text = "&Add Breakpoint";
            this.mitemDisassemblyAddBreakpoint.Click += new System.EventHandler(this.mitemDisassemblyAddBreakpoint_Click);
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.lablCurrentFunction);
            this.panel4.Controls.Add(this.label5);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel4.Location = new System.Drawing.Point(0, 0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(1010, 32);
            this.panel4.TabIndex = 7;
            // 
            // lablCurrentFunction
            // 
            this.lablCurrentFunction.AutoSize = true;
            this.lablCurrentFunction.Location = new System.Drawing.Point(102, 7);
            this.lablCurrentFunction.Name = "lablCurrentFunction";
            this.lablCurrentFunction.Size = new System.Drawing.Size(35, 13);
            this.lablCurrentFunction.TabIndex = 1;
            this.lablCurrentFunction.Text = "label9";
            this.lablCurrentFunction.Visible = false;
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
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.panel2);
            this.tabPage2.Location = new System.Drawing.Point(4, 4);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1016, 635);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Debug Log";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.butnSendCmd);
            this.panel2.Controls.Add(this.textSendCmd);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(3, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1010, 33);
            this.panel2.TabIndex = 3;
            // 
            // butnSendCmd
            // 
            this.butnSendCmd.Location = new System.Drawing.Point(315, 7);
            this.butnSendCmd.Name = "butnSendCmd";
            this.butnSendCmd.Size = new System.Drawing.Size(137, 23);
            this.butnSendCmd.TabIndex = 2;
            this.butnSendCmd.Text = "Send Command";
            this.butnSendCmd.UseVisualStyleBackColor = true;
            this.butnSendCmd.Click += new System.EventHandler(this.butnSendCmd_Click);
            // 
            // textSendCmd
            // 
            this.textSendCmd.Location = new System.Drawing.Point(45, 7);
            this.textSendCmd.Name = "textSendCmd";
            this.textSendCmd.Size = new System.Drawing.Size(264, 20);
            this.textSendCmd.TabIndex = 1;
            this.textSendCmd.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textSendCmd_KeyPress);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "label1";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.executeToolStripMenuItem,
            this.viewToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1043, 24);
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
            this.mitmMainViewWatches});
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
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1043, 699);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FormMain";
            this.Text = "Cosmos GDB Debugger";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.Shown += new System.EventHandler(this.FormMain_Shown);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.Resize += new System.EventHandler(this.FormMain_Resize);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.panel8.ResumeLayout(false);
            this.menuDisassembly.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button butnSendCmd;
        private System.Windows.Forms.TextBox textSendCmd;
        private System.Windows.Forms.Label label1;
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
        private System.Windows.Forms.ContextMenuStrip menuDisassembly;
        private System.Windows.Forms.ToolStripMenuItem mitemDisassemblyAddBreakpoint;
        private System.Windows.Forms.Panel panel8;
        private System.Windows.Forms.ListBox lboxDisassemble;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Label lablCurrentFunction;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mitmMainViewCallStack;
        private System.Windows.Forms.ToolStripMenuItem mitmMainViewWatches;
    }
}

