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
            this.lboxDisassemble = new System.Windows.Forms.ListBox();
            this.menuDisassembly = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.panel4 = new System.Windows.Forms.Panel();
            this.lablCurrentFunction = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.lboxBreakpoints = new System.Windows.Forms.ListBox();
            this.menuBreakpoints = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.panel6 = new System.Windows.Forms.Panel();
            this.butnBreakpointAdd = new System.Windows.Forms.Button();
            this.textBreakpoint = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.panel5 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.lablFlagsText = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lablFlags = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.lablALText = new System.Windows.Forms.Label();
            this.lablGS = new System.Windows.Forms.Label();
            this.lablEAX = new System.Windows.Forms.Label();
            this.label32 = new System.Windows.Forms.Label();
            this.lablAX = new System.Windows.Forms.Label();
            this.lablFS = new System.Windows.Forms.Label();
            this.lablAH = new System.Windows.Forms.Label();
            this.label29 = new System.Windows.Forms.Label();
            this.lablEBXLabel = new System.Windows.Forms.Label();
            this.lablES = new System.Windows.Forms.Label();
            this.lablBXLabel = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.lablDS = new System.Windows.Forms.Label();
            this.lablEBX = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.lablBX = new System.Windows.Forms.Label();
            this.lablCS = new System.Windows.Forms.Label();
            this.lablBH = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.lablECXLabel = new System.Windows.Forms.Label();
            this.lablSS = new System.Windows.Forms.Label();
            this.lablCXLabel = new System.Windows.Forms.Label();
            this.label25 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.lablEDI = new System.Windows.Forms.Label();
            this.lablECX = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.lablCX = new System.Windows.Forms.Label();
            this.lablESI = new System.Windows.Forms.Label();
            this.lablCH = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.lablEDXLabel = new System.Windows.Forms.Label();
            this.lablEBP = new System.Windows.Forms.Label();
            this.lablDXLabel = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.lablESP = new System.Windows.Forms.Label();
            this.lablEDX = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.lablDX = new System.Windows.Forms.Label();
            this.lablEIPText = new System.Windows.Forms.Label();
            this.lablDH = new System.Windows.Forms.Label();
            this.lablEIP = new System.Windows.Forms.Label();
            this.label30 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lablAL = new System.Windows.Forms.Label();
            this.lablDL = new System.Windows.Forms.Label();
            this.label28 = new System.Windows.Forms.Label();
            this.label24 = new System.Windows.Forms.Label();
            this.lablBL = new System.Windows.Forms.Label();
            this.lablCL = new System.Windows.Forms.Label();
            this.label26 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.lboxDebug = new System.Windows.Forms.ListBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.butnCopyDebugLogToClipboard = new System.Windows.Forms.Button();
            this.butnDebugLogClear = new System.Windows.Forms.Button();
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
            this.mitmBreakpointDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.mitemDisassemblyAddBreakpoint = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.menuDisassembly.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel3.SuspendLayout();
            this.menuBreakpoints.SuspendLayout();
            this.panel6.SuspendLayout();
            this.panel5.SuspendLayout();
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
            this.tabControl1.Size = new System.Drawing.Size(1043, 619);
            this.tabControl1.TabIndex = 4;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.lboxDisassemble);
            this.tabPage1.Controls.Add(this.panel4);
            this.tabPage1.Controls.Add(this.panel3);
            this.tabPage1.Location = new System.Drawing.Point(4, 4);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1016, 611);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Main";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // lboxDisassemble
            // 
            this.lboxDisassemble.ContextMenuStrip = this.menuDisassembly;
            this.lboxDisassemble.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lboxDisassemble.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lboxDisassemble.FormattingEnabled = true;
            this.lboxDisassemble.ItemHeight = 19;
            this.lboxDisassemble.Location = new System.Drawing.Point(3, 35);
            this.lboxDisassemble.Name = "lboxDisassemble";
            this.lboxDisassemble.Size = new System.Drawing.Size(696, 555);
            this.lboxDisassemble.TabIndex = 5;
            // 
            // menuDisassembly
            // 
            this.menuDisassembly.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mitemDisassemblyAddBreakpoint});
            this.menuDisassembly.Name = "menuDisassembly";
            this.menuDisassembly.Size = new System.Drawing.Size(157, 26);
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.lablCurrentFunction);
            this.panel4.Controls.Add(this.label5);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel4.Location = new System.Drawing.Point(3, 3);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(696, 32);
            this.panel4.TabIndex = 4;
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
            // panel3
            // 
            this.panel3.Controls.Add(this.lboxBreakpoints);
            this.panel3.Controls.Add(this.panel6);
            this.panel3.Controls.Add(this.panel5);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel3.Location = new System.Drawing.Point(699, 3);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(314, 605);
            this.panel3.TabIndex = 2;
            // 
            // lboxBreakpoints
            // 
            this.lboxBreakpoints.ContextMenuStrip = this.menuBreakpoints;
            this.lboxBreakpoints.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lboxBreakpoints.FormattingEnabled = true;
            this.lboxBreakpoints.Location = new System.Drawing.Point(0, 313);
            this.lboxBreakpoints.Name = "lboxBreakpoints";
            this.lboxBreakpoints.Size = new System.Drawing.Size(314, 290);
            this.lboxBreakpoints.Sorted = true;
            this.lboxBreakpoints.TabIndex = 63;
            // 
            // menuBreakpoints
            // 
            this.menuBreakpoints.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mitmBreakpointDelete});
            this.menuBreakpoints.Name = "menuBreakpoints";
            this.menuBreakpoints.Size = new System.Drawing.Size(108, 26);
            // 
            // panel6
            // 
            this.panel6.Controls.Add(this.butnBreakpointAdd);
            this.panel6.Controls.Add(this.textBreakpoint);
            this.panel6.Controls.Add(this.label9);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel6.Location = new System.Drawing.Point(0, 262);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(314, 51);
            this.panel6.TabIndex = 62;
            // 
            // butnBreakpointAdd
            // 
            this.butnBreakpointAdd.Location = new System.Drawing.Point(256, 19);
            this.butnBreakpointAdd.Name = "butnBreakpointAdd";
            this.butnBreakpointAdd.Size = new System.Drawing.Size(24, 23);
            this.butnBreakpointAdd.TabIndex = 5;
            this.butnBreakpointAdd.Text = "+";
            this.butnBreakpointAdd.UseVisualStyleBackColor = true;
            this.butnBreakpointAdd.Click += new System.EventHandler(this.butnBreakpointAdd_Click);
            // 
            // textBreakpoint
            // 
            this.textBreakpoint.Location = new System.Drawing.Point(9, 19);
            this.textBreakpoint.Name = "textBreakpoint";
            this.textBreakpoint.Size = new System.Drawing.Size(241, 20);
            this.textBreakpoint.TabIndex = 4;
            this.textBreakpoint.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBreakpoint_KeyPress);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(6, 3);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(74, 13);
            this.label9.TabIndex = 3;
            this.label9.Text = "Breakpoints";
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.label2);
            this.panel5.Controls.Add(this.lablFlagsText);
            this.panel5.Controls.Add(this.label3);
            this.panel5.Controls.Add(this.lablFlags);
            this.panel5.Controls.Add(this.label4);
            this.panel5.Controls.Add(this.label11);
            this.panel5.Controls.Add(this.lablALText);
            this.panel5.Controls.Add(this.lablGS);
            this.panel5.Controls.Add(this.lablEAX);
            this.panel5.Controls.Add(this.label32);
            this.panel5.Controls.Add(this.lablAX);
            this.panel5.Controls.Add(this.lablFS);
            this.panel5.Controls.Add(this.lablAH);
            this.panel5.Controls.Add(this.label29);
            this.panel5.Controls.Add(this.lablEBXLabel);
            this.panel5.Controls.Add(this.lablES);
            this.panel5.Controls.Add(this.lablBXLabel);
            this.panel5.Controls.Add(this.label17);
            this.panel5.Controls.Add(this.label8);
            this.panel5.Controls.Add(this.lablDS);
            this.panel5.Controls.Add(this.lablEBX);
            this.panel5.Controls.Add(this.label19);
            this.panel5.Controls.Add(this.lablBX);
            this.panel5.Controls.Add(this.lablCS);
            this.panel5.Controls.Add(this.lablBH);
            this.panel5.Controls.Add(this.label22);
            this.panel5.Controls.Add(this.lablECXLabel);
            this.panel5.Controls.Add(this.lablSS);
            this.panel5.Controls.Add(this.lablCXLabel);
            this.panel5.Controls.Add(this.label25);
            this.panel5.Controls.Add(this.label14);
            this.panel5.Controls.Add(this.lablEDI);
            this.panel5.Controls.Add(this.lablECX);
            this.panel5.Controls.Add(this.label7);
            this.panel5.Controls.Add(this.lablCX);
            this.panel5.Controls.Add(this.lablESI);
            this.panel5.Controls.Add(this.lablCH);
            this.panel5.Controls.Add(this.label10);
            this.panel5.Controls.Add(this.lablEDXLabel);
            this.panel5.Controls.Add(this.lablEBP);
            this.panel5.Controls.Add(this.lablDXLabel);
            this.panel5.Controls.Add(this.label12);
            this.panel5.Controls.Add(this.label20);
            this.panel5.Controls.Add(this.lablESP);
            this.panel5.Controls.Add(this.lablEDX);
            this.panel5.Controls.Add(this.label15);
            this.panel5.Controls.Add(this.lablDX);
            this.panel5.Controls.Add(this.lablEIPText);
            this.panel5.Controls.Add(this.lablDH);
            this.panel5.Controls.Add(this.lablEIP);
            this.panel5.Controls.Add(this.label30);
            this.panel5.Controls.Add(this.label6);
            this.panel5.Controls.Add(this.lablAL);
            this.panel5.Controls.Add(this.lablDL);
            this.panel5.Controls.Add(this.label28);
            this.panel5.Controls.Add(this.label24);
            this.panel5.Controls.Add(this.lablBL);
            this.panel5.Controls.Add(this.lablCL);
            this.panel5.Controls.Add(this.label26);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel5.Location = new System.Drawing.Point(0, 0);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(314, 262);
            this.panel5.TabIndex = 61;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(6, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Registers";
            // 
            // lablFlagsText
            // 
            this.lablFlagsText.AutoSize = true;
            this.lablFlagsText.Location = new System.Drawing.Point(36, 152);
            this.lablFlagsText.Name = "lablFlagsText";
            this.lablFlagsText.Size = new System.Drawing.Size(55, 13);
            this.lablFlagsText.TabIndex = 60;
            this.lablFlagsText.Text = "12345678";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 37);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(28, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "EAX";
            // 
            // lablFlags
            // 
            this.lablFlags.AutoSize = true;
            this.lablFlags.Location = new System.Drawing.Point(36, 139);
            this.lablFlags.Name = "lablFlags";
            this.lablFlags.Size = new System.Drawing.Size(55, 13);
            this.lablFlags.TabIndex = 59;
            this.lablFlags.Text = "12345678";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(107, 37);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(21, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "AX";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(6, 139);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(32, 13);
            this.label11.TabIndex = 58;
            this.label11.Text = "Flags";
            // 
            // lablALText
            // 
            this.lablALText.AutoSize = true;
            this.lablALText.Location = new System.Drawing.Point(176, 37);
            this.lablALText.Name = "lablALText";
            this.lablALText.Size = new System.Drawing.Size(22, 13);
            this.lablALText.TabIndex = 5;
            this.lablALText.Text = "AH";
            // 
            // lablGS
            // 
            this.lablGS.AutoSize = true;
            this.lablGS.Location = new System.Drawing.Point(162, 240);
            this.lablGS.Name = "lablGS";
            this.lablGS.Size = new System.Drawing.Size(55, 13);
            this.lablGS.TabIndex = 57;
            this.lablGS.Text = "12345678";
            // 
            // lablEAX
            // 
            this.lablEAX.AutoSize = true;
            this.lablEAX.Location = new System.Drawing.Point(36, 37);
            this.lablEAX.Name = "lablEAX";
            this.lablEAX.Size = new System.Drawing.Size(55, 13);
            this.lablEAX.TabIndex = 6;
            this.lablEAX.Text = "12345678";
            this.lablEAX.Visible = false;
            // 
            // label32
            // 
            this.label32.AutoSize = true;
            this.label32.Location = new System.Drawing.Point(132, 240);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(22, 13);
            this.label32.TabIndex = 56;
            this.label32.Text = "GS";
            // 
            // lablAX
            // 
            this.lablAX.AutoSize = true;
            this.lablAX.Location = new System.Drawing.Point(130, 37);
            this.lablAX.Name = "lablAX";
            this.lablAX.Size = new System.Drawing.Size(31, 13);
            this.lablAX.TabIndex = 7;
            this.lablAX.Text = "1234";
            // 
            // lablFS
            // 
            this.lablFS.AutoSize = true;
            this.lablFS.Location = new System.Drawing.Point(162, 227);
            this.lablFS.Name = "lablFS";
            this.lablFS.Size = new System.Drawing.Size(55, 13);
            this.lablFS.TabIndex = 55;
            this.lablFS.Text = "12345678";
            // 
            // lablAH
            // 
            this.lablAH.AutoSize = true;
            this.lablAH.Location = new System.Drawing.Point(196, 37);
            this.lablAH.Name = "lablAH";
            this.lablAH.Size = new System.Drawing.Size(19, 13);
            this.lablAH.TabIndex = 8;
            this.lablAH.Text = "12";
            this.lablAH.Visible = false;
            // 
            // label29
            // 
            this.label29.AutoSize = true;
            this.label29.Location = new System.Drawing.Point(132, 227);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(20, 13);
            this.label29.TabIndex = 54;
            this.label29.Text = "FS";
            // 
            // lablEBXLabel
            // 
            this.lablEBXLabel.AutoSize = true;
            this.lablEBXLabel.Location = new System.Drawing.Point(6, 50);
            this.lablEBXLabel.Name = "lablEBXLabel";
            this.lablEBXLabel.Size = new System.Drawing.Size(28, 13);
            this.lablEBXLabel.TabIndex = 9;
            this.lablEBXLabel.Text = "EBX";
            // 
            // lablES
            // 
            this.lablES.AutoSize = true;
            this.lablES.Location = new System.Drawing.Point(162, 214);
            this.lablES.Name = "lablES";
            this.lablES.Size = new System.Drawing.Size(55, 13);
            this.lablES.TabIndex = 53;
            this.lablES.Text = "12345678";
            // 
            // lablBXLabel
            // 
            this.lablBXLabel.AutoSize = true;
            this.lablBXLabel.Location = new System.Drawing.Point(107, 50);
            this.lablBXLabel.Name = "lablBXLabel";
            this.lablBXLabel.Size = new System.Drawing.Size(21, 13);
            this.lablBXLabel.TabIndex = 10;
            this.lablBXLabel.Text = "BX";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(132, 214);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(21, 13);
            this.label17.TabIndex = 52;
            this.label17.Text = "ES";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(176, 50);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(22, 13);
            this.label8.TabIndex = 11;
            this.label8.Text = "BH";
            // 
            // lablDS
            // 
            this.lablDS.AutoSize = true;
            this.lablDS.Location = new System.Drawing.Point(162, 201);
            this.lablDS.Name = "lablDS";
            this.lablDS.Size = new System.Drawing.Size(55, 13);
            this.lablDS.TabIndex = 51;
            this.lablDS.Text = "12345678";
            // 
            // lablEBX
            // 
            this.lablEBX.AutoSize = true;
            this.lablEBX.Location = new System.Drawing.Point(36, 50);
            this.lablEBX.Name = "lablEBX";
            this.lablEBX.Size = new System.Drawing.Size(55, 13);
            this.lablEBX.TabIndex = 12;
            this.lablEBX.Text = "12345678";
            this.lablEBX.Visible = false;
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(132, 201);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(22, 13);
            this.label19.TabIndex = 50;
            this.label19.Text = "DS";
            // 
            // lablBX
            // 
            this.lablBX.AutoSize = true;
            this.lablBX.Location = new System.Drawing.Point(130, 50);
            this.lablBX.Name = "lablBX";
            this.lablBX.Size = new System.Drawing.Size(31, 13);
            this.lablBX.TabIndex = 13;
            this.lablBX.Text = "1234";
            this.lablBX.Visible = false;
            // 
            // lablCS
            // 
            this.lablCS.AutoSize = true;
            this.lablCS.Location = new System.Drawing.Point(162, 188);
            this.lablCS.Name = "lablCS";
            this.lablCS.Size = new System.Drawing.Size(55, 13);
            this.lablCS.TabIndex = 49;
            this.lablCS.Text = "12345678";
            // 
            // lablBH
            // 
            this.lablBH.AutoSize = true;
            this.lablBH.Location = new System.Drawing.Point(196, 50);
            this.lablBH.Name = "lablBH";
            this.lablBH.Size = new System.Drawing.Size(19, 13);
            this.lablBH.TabIndex = 14;
            this.lablBH.Text = "12";
            this.lablBH.Visible = false;
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(132, 188);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(21, 13);
            this.label22.TabIndex = 48;
            this.label22.Text = "CS";
            // 
            // lablECXLabel
            // 
            this.lablECXLabel.AutoSize = true;
            this.lablECXLabel.Location = new System.Drawing.Point(6, 63);
            this.lablECXLabel.Name = "lablECXLabel";
            this.lablECXLabel.Size = new System.Drawing.Size(28, 13);
            this.lablECXLabel.TabIndex = 15;
            this.lablECXLabel.Text = "ECX";
            // 
            // lablSS
            // 
            this.lablSS.AutoSize = true;
            this.lablSS.Location = new System.Drawing.Point(162, 175);
            this.lablSS.Name = "lablSS";
            this.lablSS.Size = new System.Drawing.Size(55, 13);
            this.lablSS.TabIndex = 47;
            this.lablSS.Text = "12345678";
            // 
            // lablCXLabel
            // 
            this.lablCXLabel.AutoSize = true;
            this.lablCXLabel.Location = new System.Drawing.Point(107, 63);
            this.lablCXLabel.Name = "lablCXLabel";
            this.lablCXLabel.Size = new System.Drawing.Size(21, 13);
            this.lablCXLabel.TabIndex = 16;
            this.lablCXLabel.Text = "CX";
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(132, 175);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(21, 13);
            this.label25.TabIndex = 46;
            this.label25.Text = "SS";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(176, 63);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(22, 13);
            this.label14.TabIndex = 17;
            this.label14.Text = "CH";
            // 
            // lablEDI
            // 
            this.lablEDI.AutoSize = true;
            this.lablEDI.Location = new System.Drawing.Point(36, 214);
            this.lablEDI.Name = "lablEDI";
            this.lablEDI.Size = new System.Drawing.Size(55, 13);
            this.lablEDI.TabIndex = 45;
            this.lablEDI.Text = "12345678";
            // 
            // lablECX
            // 
            this.lablECX.AutoSize = true;
            this.lablECX.Location = new System.Drawing.Point(36, 63);
            this.lablECX.Name = "lablECX";
            this.lablECX.Size = new System.Drawing.Size(55, 13);
            this.lablECX.TabIndex = 18;
            this.lablECX.Text = "12345678";
            this.lablECX.Visible = false;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 214);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(25, 13);
            this.label7.TabIndex = 44;
            this.label7.Text = "EDI";
            // 
            // lablCX
            // 
            this.lablCX.AutoSize = true;
            this.lablCX.Location = new System.Drawing.Point(130, 63);
            this.lablCX.Name = "lablCX";
            this.lablCX.Size = new System.Drawing.Size(31, 13);
            this.lablCX.TabIndex = 19;
            this.lablCX.Text = "1234";
            this.lablCX.Visible = false;
            // 
            // lablESI
            // 
            this.lablESI.AutoSize = true;
            this.lablESI.Location = new System.Drawing.Point(36, 201);
            this.lablESI.Name = "lablESI";
            this.lablESI.Size = new System.Drawing.Size(55, 13);
            this.lablESI.TabIndex = 43;
            this.lablESI.Text = "12345678";
            // 
            // lablCH
            // 
            this.lablCH.AutoSize = true;
            this.lablCH.Location = new System.Drawing.Point(196, 63);
            this.lablCH.Name = "lablCH";
            this.lablCH.Size = new System.Drawing.Size(19, 13);
            this.lablCH.TabIndex = 20;
            this.lablCH.Text = "12";
            this.lablCH.Visible = false;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(6, 201);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(24, 13);
            this.label10.TabIndex = 42;
            this.label10.Text = "ESI";
            // 
            // lablEDXLabel
            // 
            this.lablEDXLabel.AutoSize = true;
            this.lablEDXLabel.Location = new System.Drawing.Point(6, 76);
            this.lablEDXLabel.Name = "lablEDXLabel";
            this.lablEDXLabel.Size = new System.Drawing.Size(29, 13);
            this.lablEDXLabel.TabIndex = 21;
            this.lablEDXLabel.Text = "EDX";
            // 
            // lablEBP
            // 
            this.lablEBP.AutoSize = true;
            this.lablEBP.Location = new System.Drawing.Point(36, 188);
            this.lablEBP.Name = "lablEBP";
            this.lablEBP.Size = new System.Drawing.Size(55, 13);
            this.lablEBP.TabIndex = 41;
            this.lablEBP.Text = "12345678";
            // 
            // lablDXLabel
            // 
            this.lablDXLabel.AutoSize = true;
            this.lablDXLabel.Location = new System.Drawing.Point(107, 76);
            this.lablDXLabel.Name = "lablDXLabel";
            this.lablDXLabel.Size = new System.Drawing.Size(22, 13);
            this.lablDXLabel.TabIndex = 22;
            this.lablDXLabel.Text = "DX";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(6, 188);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(28, 13);
            this.label12.TabIndex = 40;
            this.label12.Text = "EBP";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(176, 76);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(23, 13);
            this.label20.TabIndex = 23;
            this.label20.Text = "DH";
            // 
            // lablESP
            // 
            this.lablESP.AutoSize = true;
            this.lablESP.Location = new System.Drawing.Point(36, 175);
            this.lablESP.Name = "lablESP";
            this.lablESP.Size = new System.Drawing.Size(55, 13);
            this.lablESP.TabIndex = 39;
            this.lablESP.Text = "12345678";
            // 
            // lablEDX
            // 
            this.lablEDX.AutoSize = true;
            this.lablEDX.Location = new System.Drawing.Point(36, 76);
            this.lablEDX.Name = "lablEDX";
            this.lablEDX.Size = new System.Drawing.Size(55, 13);
            this.lablEDX.TabIndex = 24;
            this.lablEDX.Text = "12345678";
            this.lablEDX.Visible = false;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(6, 175);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(28, 13);
            this.label15.TabIndex = 38;
            this.label15.Text = "ESP";
            // 
            // lablDX
            // 
            this.lablDX.AutoSize = true;
            this.lablDX.Location = new System.Drawing.Point(130, 76);
            this.lablDX.Name = "lablDX";
            this.lablDX.Size = new System.Drawing.Size(31, 13);
            this.lablDX.TabIndex = 25;
            this.lablDX.Text = "1234";
            this.lablDX.Visible = false;
            // 
            // lablEIPText
            // 
            this.lablEIPText.AutoSize = true;
            this.lablEIPText.Location = new System.Drawing.Point(36, 117);
            this.lablEIPText.Name = "lablEIPText";
            this.lablEIPText.Size = new System.Drawing.Size(55, 13);
            this.lablEIPText.TabIndex = 37;
            this.lablEIPText.Text = "12345678";
            // 
            // lablDH
            // 
            this.lablDH.AutoSize = true;
            this.lablDH.Location = new System.Drawing.Point(196, 76);
            this.lablDH.Name = "lablDH";
            this.lablDH.Size = new System.Drawing.Size(19, 13);
            this.lablDH.TabIndex = 26;
            this.lablDH.Text = "12";
            this.lablDH.Visible = false;
            // 
            // lablEIP
            // 
            this.lablEIP.AutoSize = true;
            this.lablEIP.Location = new System.Drawing.Point(36, 104);
            this.lablEIP.Name = "lablEIP";
            this.lablEIP.Size = new System.Drawing.Size(55, 13);
            this.lablEIP.TabIndex = 36;
            this.lablEIP.Text = "12345678";
            // 
            // label30
            // 
            this.label30.AutoSize = true;
            this.label30.Location = new System.Drawing.Point(230, 37);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(20, 13);
            this.label30.TabIndex = 27;
            this.label30.Text = "AL";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 104);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(24, 13);
            this.label6.TabIndex = 35;
            this.label6.Text = "EIP";
            // 
            // lablAL
            // 
            this.lablAL.AutoSize = true;
            this.lablAL.Location = new System.Drawing.Point(250, 37);
            this.lablAL.Name = "lablAL";
            this.lablAL.Size = new System.Drawing.Size(19, 13);
            this.lablAL.TabIndex = 28;
            this.lablAL.Text = "12";
            this.lablAL.Visible = false;
            // 
            // lablDL
            // 
            this.lablDL.AutoSize = true;
            this.lablDL.Location = new System.Drawing.Point(250, 76);
            this.lablDL.Name = "lablDL";
            this.lablDL.Size = new System.Drawing.Size(19, 13);
            this.lablDL.TabIndex = 34;
            this.lablDL.Text = "12";
            this.lablDL.Visible = false;
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Location = new System.Drawing.Point(230, 50);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(20, 13);
            this.label28.TabIndex = 29;
            this.label28.Text = "BL";
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(230, 76);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(21, 13);
            this.label24.TabIndex = 33;
            this.label24.Text = "DL";
            // 
            // lablBL
            // 
            this.lablBL.AutoSize = true;
            this.lablBL.Location = new System.Drawing.Point(250, 50);
            this.lablBL.Name = "lablBL";
            this.lablBL.Size = new System.Drawing.Size(19, 13);
            this.lablBL.TabIndex = 30;
            this.lablBL.Text = "12";
            // 
            // lablCL
            // 
            this.lablCL.AutoSize = true;
            this.lablCL.Location = new System.Drawing.Point(250, 63);
            this.lablCL.Name = "lablCL";
            this.lablCL.Size = new System.Drawing.Size(19, 13);
            this.lablCL.TabIndex = 32;
            this.lablCL.Text = "12";
            this.lablCL.Visible = false;
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(230, 63);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(20, 13);
            this.label26.TabIndex = 31;
            this.label26.Text = "CL";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.lboxDebug);
            this.tabPage2.Controls.Add(this.panel2);
            this.tabPage2.Location = new System.Drawing.Point(4, 4);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1016, 611);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Debug Log";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // lboxDebug
            // 
            this.lboxDebug.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lboxDebug.FormattingEnabled = true;
            this.lboxDebug.Location = new System.Drawing.Point(3, 36);
            this.lboxDebug.Name = "lboxDebug";
            this.lboxDebug.Size = new System.Drawing.Size(1010, 563);
            this.lboxDebug.TabIndex = 4;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.butnCopyDebugLogToClipboard);
            this.panel2.Controls.Add(this.butnDebugLogClear);
            this.panel2.Controls.Add(this.butnSendCmd);
            this.panel2.Controls.Add(this.textSendCmd);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(3, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1010, 33);
            this.panel2.TabIndex = 3;
            // 
            // butnCopyDebugLogToClipboard
            // 
            this.butnCopyDebugLogToClipboard.Location = new System.Drawing.Point(565, 7);
            this.butnCopyDebugLogToClipboard.Name = "butnCopyDebugLogToClipboard";
            this.butnCopyDebugLogToClipboard.Size = new System.Drawing.Size(143, 23);
            this.butnCopyDebugLogToClipboard.TabIndex = 4;
            this.butnCopyDebugLogToClipboard.Text = "Copy Log to Clipboard";
            this.butnCopyDebugLogToClipboard.UseVisualStyleBackColor = true;
            this.butnCopyDebugLogToClipboard.Click += new System.EventHandler(this.butnCopyDebugLogToClipboard_Click);
            // 
            // butnDebugLogClear
            // 
            this.butnDebugLogClear.Location = new System.Drawing.Point(484, 7);
            this.butnDebugLogClear.Name = "butnDebugLogClear";
            this.butnDebugLogClear.Size = new System.Drawing.Size(75, 23);
            this.butnDebugLogClear.TabIndex = 3;
            this.butnDebugLogClear.Text = "Clear Log";
            this.butnDebugLogClear.UseVisualStyleBackColor = true;
            this.butnDebugLogClear.Click += new System.EventHandler(this.butnDebugLogClear_Click);
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
            this.executeToolStripMenuItem});
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
            // mitmBreakpointDelete
            // 
            this.mitmBreakpointDelete.Name = "mitmBreakpointDelete";
            this.mitmBreakpointDelete.Size = new System.Drawing.Size(107, 22);
            this.mitmBreakpointDelete.Text = "&Delete";
            this.mitmBreakpointDelete.Click += new System.EventHandler(this.mitmBreakpointDelete_Click);
            // 
            // mitemDisassemblyAddBreakpoint
            // 
            this.mitemDisassemblyAddBreakpoint.Name = "mitemDisassemblyAddBreakpoint";
            this.mitemDisassemblyAddBreakpoint.Size = new System.Drawing.Size(156, 22);
            this.mitemDisassemblyAddBreakpoint.Text = "&Add Breakpoint";
            this.mitemDisassemblyAddBreakpoint.Click += new System.EventHandler(this.mitemDisassemblyAddBreakpoint_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1043, 675);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FormMain";
            this.Text = "Cosmos GDB Debugger";
            this.Shown += new System.EventHandler(this.FormMain_Shown);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.menuDisassembly.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.menuBreakpoints.ResumeLayout(false);
            this.panel6.ResumeLayout(false);
            this.panel6.PerformLayout();
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
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
        private System.Windows.Forms.ListBox lboxDebug;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button butnSendCmd;
        private System.Windows.Forms.TextBox textSendCmd;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button butnDebugLogClear;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label lablAH;
        private System.Windows.Forms.Label lablAX;
        private System.Windows.Forms.Label lablEAX;
        private System.Windows.Forms.Label lablALText;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button butnCopyDebugLogToClipboard;
        private System.Windows.Forms.Label lablDH;
        private System.Windows.Forms.Label lablDX;
        private System.Windows.Forms.Label lablEDX;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Label lablDXLabel;
        private System.Windows.Forms.Label lablEDXLabel;
        private System.Windows.Forms.Label lablCH;
        private System.Windows.Forms.Label lablCX;
        private System.Windows.Forms.Label lablECX;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label lablCXLabel;
        private System.Windows.Forms.Label lablECXLabel;
        private System.Windows.Forms.Label lablBH;
        private System.Windows.Forms.Label lablBX;
        private System.Windows.Forms.Label lablEBX;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label lablBXLabel;
        private System.Windows.Forms.Label lablEBXLabel;
        private System.Windows.Forms.Label lablDL;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.Label lablCL;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.Label lablBL;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.Label lablAL;
        private System.Windows.Forms.Label label30;
        private System.Windows.Forms.Label lablEIPText;
        private System.Windows.Forms.Label lablEIP;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lablES;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label lablDS;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label lablCS;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.Label lablSS;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.Label lablEDI;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lablESI;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label lablEBP;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label lablESP;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label lablGS;
        private System.Windows.Forms.Label label32;
        private System.Windows.Forms.Label lablFS;
        private System.Windows.Forms.Label label29;
        private System.Windows.Forms.Label lablFlagsText;
        private System.Windows.Forms.Label lablFlags;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mitmExit;
        private System.Windows.Forms.ToolStripMenuItem executeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mitmStepInto;
        private System.Windows.Forms.ToolStripMenuItem mitmStepOver;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem continueToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mitmConnect;
        private System.Windows.Forms.ListBox lboxDisassemble;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Label lablCurrentFunction;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ToolStripMenuItem mitmRefresh;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.ListBox lboxBreakpoints;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.Button butnBreakpointAdd;
        private System.Windows.Forms.TextBox textBreakpoint;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ContextMenuStrip menuDisassembly;
        private System.Windows.Forms.ContextMenuStrip menuBreakpoints;
        private System.Windows.Forms.ToolStripMenuItem mitmBreakpointDelete;
        private System.Windows.Forms.ToolStripMenuItem mitemDisassemblyAddBreakpoint;
    }
}

