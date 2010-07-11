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
            this.butnConnect = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.butnStep = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.panel3 = new System.Windows.Forms.Panel();
            this.lablDL = new System.Windows.Forms.Label();
            this.label24 = new System.Windows.Forms.Label();
            this.lablCL = new System.Windows.Forms.Label();
            this.label26 = new System.Windows.Forms.Label();
            this.lablBL = new System.Windows.Forms.Label();
            this.label28 = new System.Windows.Forms.Label();
            this.lablAL = new System.Windows.Forms.Label();
            this.label30 = new System.Windows.Forms.Label();
            this.lablDH = new System.Windows.Forms.Label();
            this.lablDX = new System.Windows.Forms.Label();
            this.lablEDX = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.lablDXLabel = new System.Windows.Forms.Label();
            this.lablEDXLabel = new System.Windows.Forms.Label();
            this.lablCH = new System.Windows.Forms.Label();
            this.lablCX = new System.Windows.Forms.Label();
            this.lablECX = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.lablCXLabel = new System.Windows.Forms.Label();
            this.lablECXLabel = new System.Windows.Forms.Label();
            this.lablBH = new System.Windows.Forms.Label();
            this.lablBX = new System.Windows.Forms.Label();
            this.lablEBX = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.lablBXLabel = new System.Windows.Forms.Label();
            this.lablEBXLabel = new System.Windows.Forms.Label();
            this.lablAH = new System.Windows.Forms.Label();
            this.lablAX = new System.Windows.Forms.Label();
            this.lablEAX = new System.Windows.Forms.Label();
            this.lablALText = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.lboxDebug = new System.Windows.Forms.ListBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.butnCopyDebugLogToClipboard = new System.Windows.Forms.Button();
            this.butnDebugLogClear = new System.Windows.Forms.Button();
            this.butnSendCmd = new System.Windows.Forms.Button();
            this.textSendCmd = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lablEIP = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lablEIPText = new System.Windows.Forms.Label();
            this.lablEDI = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.lablESI = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.lablEBP = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.lablESP = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.lablES = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.lablDS = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.lablCS = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.lablSS = new System.Windows.Forms.Label();
            this.label25 = new System.Windows.Forms.Label();
            this.lablFS = new System.Windows.Forms.Label();
            this.label29 = new System.Windows.Forms.Label();
            this.lablGS = new System.Windows.Forms.Label();
            this.label32 = new System.Windows.Forms.Label();
            this.lablFlagsText = new System.Windows.Forms.Label();
            this.lablFlags = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.lboxDisassemble = new System.Windows.Forms.ListBox();
            this.panel1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // butnConnect
            // 
            this.butnConnect.Location = new System.Drawing.Point(3, 3);
            this.butnConnect.Name = "butnConnect";
            this.butnConnect.Size = new System.Drawing.Size(75, 23);
            this.butnConnect.TabIndex = 0;
            this.butnConnect.Text = "Connect";
            this.butnConnect.UseVisualStyleBackColor = true;
            this.butnConnect.Click += new System.EventHandler(this.butnConnect_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.butnStep);
            this.panel1.Controls.Add(this.butnConnect);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1043, 32);
            this.panel1.TabIndex = 3;
            // 
            // butnStep
            // 
            this.butnStep.Location = new System.Drawing.Point(84, 3);
            this.butnStep.Name = "butnStep";
            this.butnStep.Size = new System.Drawing.Size(75, 23);
            this.butnStep.TabIndex = 1;
            this.butnStep.Text = "Step";
            this.butnStep.UseVisualStyleBackColor = true;
            this.butnStep.Click += new System.EventHandler(this.butnStepOne_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Alignment = System.Windows.Forms.TabAlignment.Right;
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 32);
            this.tabControl1.Multiline = true;
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1043, 643);
            this.tabControl1.TabIndex = 4;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.lboxDisassemble);
            this.tabPage1.Controls.Add(this.panel3);
            this.tabPage1.Location = new System.Drawing.Point(4, 4);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1016, 635);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Main";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.lablFlagsText);
            this.panel3.Controls.Add(this.lablFlags);
            this.panel3.Controls.Add(this.label11);
            this.panel3.Controls.Add(this.lablGS);
            this.panel3.Controls.Add(this.label32);
            this.panel3.Controls.Add(this.lablFS);
            this.panel3.Controls.Add(this.label29);
            this.panel3.Controls.Add(this.lablES);
            this.panel3.Controls.Add(this.label17);
            this.panel3.Controls.Add(this.lablDS);
            this.panel3.Controls.Add(this.label19);
            this.panel3.Controls.Add(this.lablCS);
            this.panel3.Controls.Add(this.label22);
            this.panel3.Controls.Add(this.lablSS);
            this.panel3.Controls.Add(this.label25);
            this.panel3.Controls.Add(this.lablEDI);
            this.panel3.Controls.Add(this.label7);
            this.panel3.Controls.Add(this.lablESI);
            this.panel3.Controls.Add(this.label10);
            this.panel3.Controls.Add(this.lablEBP);
            this.panel3.Controls.Add(this.label12);
            this.panel3.Controls.Add(this.lablESP);
            this.panel3.Controls.Add(this.label15);
            this.panel3.Controls.Add(this.lablEIPText);
            this.panel3.Controls.Add(this.lablEIP);
            this.panel3.Controls.Add(this.label6);
            this.panel3.Controls.Add(this.lablDL);
            this.panel3.Controls.Add(this.label24);
            this.panel3.Controls.Add(this.lablCL);
            this.panel3.Controls.Add(this.label26);
            this.panel3.Controls.Add(this.lablBL);
            this.panel3.Controls.Add(this.label28);
            this.panel3.Controls.Add(this.lablAL);
            this.panel3.Controls.Add(this.label30);
            this.panel3.Controls.Add(this.lablDH);
            this.panel3.Controls.Add(this.lablDX);
            this.panel3.Controls.Add(this.lablEDX);
            this.panel3.Controls.Add(this.label20);
            this.panel3.Controls.Add(this.lablDXLabel);
            this.panel3.Controls.Add(this.lablEDXLabel);
            this.panel3.Controls.Add(this.lablCH);
            this.panel3.Controls.Add(this.lablCX);
            this.panel3.Controls.Add(this.lablECX);
            this.panel3.Controls.Add(this.label14);
            this.panel3.Controls.Add(this.lablCXLabel);
            this.panel3.Controls.Add(this.lablECXLabel);
            this.panel3.Controls.Add(this.lablBH);
            this.panel3.Controls.Add(this.lablBX);
            this.panel3.Controls.Add(this.lablEBX);
            this.panel3.Controls.Add(this.label8);
            this.panel3.Controls.Add(this.lablBXLabel);
            this.panel3.Controls.Add(this.lablEBXLabel);
            this.panel3.Controls.Add(this.lablAH);
            this.panel3.Controls.Add(this.lablAX);
            this.panel3.Controls.Add(this.lablEAX);
            this.panel3.Controls.Add(this.lablALText);
            this.panel3.Controls.Add(this.label4);
            this.panel3.Controls.Add(this.label3);
            this.panel3.Controls.Add(this.label2);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel3.Location = new System.Drawing.Point(699, 3);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(314, 629);
            this.panel3.TabIndex = 2;
            // 
            // lablDL
            // 
            this.lablDL.AutoSize = true;
            this.lablDL.Location = new System.Drawing.Point(254, 75);
            this.lablDL.Name = "lablDL";
            this.lablDL.Size = new System.Drawing.Size(19, 13);
            this.lablDL.TabIndex = 34;
            this.lablDL.Text = "12";
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(234, 75);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(21, 13);
            this.label24.TabIndex = 33;
            this.label24.Text = "DL";
            // 
            // lablCL
            // 
            this.lablCL.AutoSize = true;
            this.lablCL.Location = new System.Drawing.Point(254, 62);
            this.lablCL.Name = "lablCL";
            this.lablCL.Size = new System.Drawing.Size(19, 13);
            this.lablCL.TabIndex = 32;
            this.lablCL.Text = "12";
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(234, 62);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(20, 13);
            this.label26.TabIndex = 31;
            this.label26.Text = "CL";
            // 
            // lablBL
            // 
            this.lablBL.AutoSize = true;
            this.lablBL.Location = new System.Drawing.Point(254, 49);
            this.lablBL.Name = "lablBL";
            this.lablBL.Size = new System.Drawing.Size(19, 13);
            this.lablBL.TabIndex = 30;
            this.lablBL.Text = "12";
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Location = new System.Drawing.Point(234, 49);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(20, 13);
            this.label28.TabIndex = 29;
            this.label28.Text = "BL";
            // 
            // lablAL
            // 
            this.lablAL.AutoSize = true;
            this.lablAL.Location = new System.Drawing.Point(254, 36);
            this.lablAL.Name = "lablAL";
            this.lablAL.Size = new System.Drawing.Size(19, 13);
            this.lablAL.TabIndex = 28;
            this.lablAL.Text = "12";
            // 
            // label30
            // 
            this.label30.AutoSize = true;
            this.label30.Location = new System.Drawing.Point(234, 36);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(20, 13);
            this.label30.TabIndex = 27;
            this.label30.Text = "AL";
            // 
            // lablDH
            // 
            this.lablDH.AutoSize = true;
            this.lablDH.Location = new System.Drawing.Point(200, 75);
            this.lablDH.Name = "lablDH";
            this.lablDH.Size = new System.Drawing.Size(19, 13);
            this.lablDH.TabIndex = 26;
            this.lablDH.Text = "12";
            // 
            // lablDX
            // 
            this.lablDX.AutoSize = true;
            this.lablDX.Location = new System.Drawing.Point(134, 75);
            this.lablDX.Name = "lablDX";
            this.lablDX.Size = new System.Drawing.Size(31, 13);
            this.lablDX.TabIndex = 25;
            this.lablDX.Text = "1234";
            // 
            // lablEDX
            // 
            this.lablEDX.AutoSize = true;
            this.lablEDX.Location = new System.Drawing.Point(40, 75);
            this.lablEDX.Name = "lablEDX";
            this.lablEDX.Size = new System.Drawing.Size(55, 13);
            this.lablEDX.TabIndex = 24;
            this.lablEDX.Text = "12345678";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(180, 75);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(23, 13);
            this.label20.TabIndex = 23;
            this.label20.Text = "DH";
            // 
            // lablDXLabel
            // 
            this.lablDXLabel.AutoSize = true;
            this.lablDXLabel.Location = new System.Drawing.Point(111, 75);
            this.lablDXLabel.Name = "lablDXLabel";
            this.lablDXLabel.Size = new System.Drawing.Size(22, 13);
            this.lablDXLabel.TabIndex = 22;
            this.lablDXLabel.Text = "DX";
            // 
            // lablEDXLabel
            // 
            this.lablEDXLabel.AutoSize = true;
            this.lablEDXLabel.Location = new System.Drawing.Point(10, 75);
            this.lablEDXLabel.Name = "lablEDXLabel";
            this.lablEDXLabel.Size = new System.Drawing.Size(29, 13);
            this.lablEDXLabel.TabIndex = 21;
            this.lablEDXLabel.Text = "EDX";
            // 
            // lablCH
            // 
            this.lablCH.AutoSize = true;
            this.lablCH.Location = new System.Drawing.Point(200, 62);
            this.lablCH.Name = "lablCH";
            this.lablCH.Size = new System.Drawing.Size(19, 13);
            this.lablCH.TabIndex = 20;
            this.lablCH.Text = "12";
            // 
            // lablCX
            // 
            this.lablCX.AutoSize = true;
            this.lablCX.Location = new System.Drawing.Point(134, 62);
            this.lablCX.Name = "lablCX";
            this.lablCX.Size = new System.Drawing.Size(31, 13);
            this.lablCX.TabIndex = 19;
            this.lablCX.Text = "1234";
            // 
            // lablECX
            // 
            this.lablECX.AutoSize = true;
            this.lablECX.Location = new System.Drawing.Point(40, 62);
            this.lablECX.Name = "lablECX";
            this.lablECX.Size = new System.Drawing.Size(55, 13);
            this.lablECX.TabIndex = 18;
            this.lablECX.Text = "12345678";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(180, 62);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(22, 13);
            this.label14.TabIndex = 17;
            this.label14.Text = "CH";
            // 
            // lablCXLabel
            // 
            this.lablCXLabel.AutoSize = true;
            this.lablCXLabel.Location = new System.Drawing.Point(111, 62);
            this.lablCXLabel.Name = "lablCXLabel";
            this.lablCXLabel.Size = new System.Drawing.Size(21, 13);
            this.lablCXLabel.TabIndex = 16;
            this.lablCXLabel.Text = "CX";
            // 
            // lablECXLabel
            // 
            this.lablECXLabel.AutoSize = true;
            this.lablECXLabel.Location = new System.Drawing.Point(10, 62);
            this.lablECXLabel.Name = "lablECXLabel";
            this.lablECXLabel.Size = new System.Drawing.Size(28, 13);
            this.lablECXLabel.TabIndex = 15;
            this.lablECXLabel.Text = "ECX";
            // 
            // lablBH
            // 
            this.lablBH.AutoSize = true;
            this.lablBH.Location = new System.Drawing.Point(200, 49);
            this.lablBH.Name = "lablBH";
            this.lablBH.Size = new System.Drawing.Size(19, 13);
            this.lablBH.TabIndex = 14;
            this.lablBH.Text = "12";
            // 
            // lablBX
            // 
            this.lablBX.AutoSize = true;
            this.lablBX.Location = new System.Drawing.Point(134, 49);
            this.lablBX.Name = "lablBX";
            this.lablBX.Size = new System.Drawing.Size(31, 13);
            this.lablBX.TabIndex = 13;
            this.lablBX.Text = "1234";
            // 
            // lablEBX
            // 
            this.lablEBX.AutoSize = true;
            this.lablEBX.Location = new System.Drawing.Point(40, 49);
            this.lablEBX.Name = "lablEBX";
            this.lablEBX.Size = new System.Drawing.Size(55, 13);
            this.lablEBX.TabIndex = 12;
            this.lablEBX.Text = "12345678";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(180, 49);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(22, 13);
            this.label8.TabIndex = 11;
            this.label8.Text = "BH";
            // 
            // lablBXLabel
            // 
            this.lablBXLabel.AutoSize = true;
            this.lablBXLabel.Location = new System.Drawing.Point(111, 49);
            this.lablBXLabel.Name = "lablBXLabel";
            this.lablBXLabel.Size = new System.Drawing.Size(21, 13);
            this.lablBXLabel.TabIndex = 10;
            this.lablBXLabel.Text = "BX";
            // 
            // lablEBXLabel
            // 
            this.lablEBXLabel.AutoSize = true;
            this.lablEBXLabel.Location = new System.Drawing.Point(10, 49);
            this.lablEBXLabel.Name = "lablEBXLabel";
            this.lablEBXLabel.Size = new System.Drawing.Size(28, 13);
            this.lablEBXLabel.TabIndex = 9;
            this.lablEBXLabel.Text = "EBX";
            // 
            // lablAH
            // 
            this.lablAH.AutoSize = true;
            this.lablAH.Location = new System.Drawing.Point(200, 36);
            this.lablAH.Name = "lablAH";
            this.lablAH.Size = new System.Drawing.Size(19, 13);
            this.lablAH.TabIndex = 8;
            this.lablAH.Text = "12";
            // 
            // lablAX
            // 
            this.lablAX.AutoSize = true;
            this.lablAX.Location = new System.Drawing.Point(134, 36);
            this.lablAX.Name = "lablAX";
            this.lablAX.Size = new System.Drawing.Size(31, 13);
            this.lablAX.TabIndex = 7;
            this.lablAX.Text = "1234";
            // 
            // lablEAX
            // 
            this.lablEAX.AutoSize = true;
            this.lablEAX.Location = new System.Drawing.Point(40, 36);
            this.lablEAX.Name = "lablEAX";
            this.lablEAX.Size = new System.Drawing.Size(55, 13);
            this.lablEAX.TabIndex = 6;
            this.lablEAX.Text = "12345678";
            // 
            // lablALText
            // 
            this.lablALText.AutoSize = true;
            this.lablALText.Location = new System.Drawing.Point(180, 36);
            this.lablALText.Name = "lablALText";
            this.lablALText.Size = new System.Drawing.Size(22, 13);
            this.lablALText.TabIndex = 5;
            this.lablALText.Text = "AH";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(111, 36);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(21, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "AX";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 36);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(28, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "EAX";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(10, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Registers";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.lboxDebug);
            this.tabPage2.Controls.Add(this.panel2);
            this.tabPage2.Location = new System.Drawing.Point(4, 4);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1016, 635);
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
            this.lboxDebug.Size = new System.Drawing.Size(1010, 589);
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
            // lablEIP
            // 
            this.lablEIP.AutoSize = true;
            this.lablEIP.Location = new System.Drawing.Point(40, 103);
            this.lablEIP.Name = "lablEIP";
            this.lablEIP.Size = new System.Drawing.Size(55, 13);
            this.lablEIP.TabIndex = 36;
            this.lablEIP.Text = "12345678";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(10, 103);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(24, 13);
            this.label6.TabIndex = 35;
            this.label6.Text = "EIP";
            // 
            // lablEIPText
            // 
            this.lablEIPText.AutoSize = true;
            this.lablEIPText.Location = new System.Drawing.Point(40, 116);
            this.lablEIPText.Name = "lablEIPText";
            this.lablEIPText.Size = new System.Drawing.Size(55, 13);
            this.lablEIPText.TabIndex = 37;
            this.lablEIPText.Text = "12345678";
            // 
            // lablEDI
            // 
            this.lablEDI.AutoSize = true;
            this.lablEDI.Location = new System.Drawing.Point(40, 213);
            this.lablEDI.Name = "lablEDI";
            this.lablEDI.Size = new System.Drawing.Size(55, 13);
            this.lablEDI.TabIndex = 45;
            this.lablEDI.Text = "12345678";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(10, 213);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(25, 13);
            this.label7.TabIndex = 44;
            this.label7.Text = "EDI";
            // 
            // lablESI
            // 
            this.lablESI.AutoSize = true;
            this.lablESI.Location = new System.Drawing.Point(40, 200);
            this.lablESI.Name = "lablESI";
            this.lablESI.Size = new System.Drawing.Size(55, 13);
            this.lablESI.TabIndex = 43;
            this.lablESI.Text = "12345678";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(10, 200);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(24, 13);
            this.label10.TabIndex = 42;
            this.label10.Text = "ESI";
            // 
            // lablEBP
            // 
            this.lablEBP.AutoSize = true;
            this.lablEBP.Location = new System.Drawing.Point(40, 187);
            this.lablEBP.Name = "lablEBP";
            this.lablEBP.Size = new System.Drawing.Size(55, 13);
            this.lablEBP.TabIndex = 41;
            this.lablEBP.Text = "12345678";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(10, 187);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(28, 13);
            this.label12.TabIndex = 40;
            this.label12.Text = "EBP";
            // 
            // lablESP
            // 
            this.lablESP.AutoSize = true;
            this.lablESP.Location = new System.Drawing.Point(40, 174);
            this.lablESP.Name = "lablESP";
            this.lablESP.Size = new System.Drawing.Size(55, 13);
            this.lablESP.TabIndex = 39;
            this.lablESP.Text = "12345678";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(10, 174);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(28, 13);
            this.label15.TabIndex = 38;
            this.label15.Text = "ESP";
            // 
            // lablES
            // 
            this.lablES.AutoSize = true;
            this.lablES.Location = new System.Drawing.Point(166, 213);
            this.lablES.Name = "lablES";
            this.lablES.Size = new System.Drawing.Size(55, 13);
            this.lablES.TabIndex = 53;
            this.lablES.Text = "12345678";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(136, 213);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(21, 13);
            this.label17.TabIndex = 52;
            this.label17.Text = "ES";
            // 
            // lablDS
            // 
            this.lablDS.AutoSize = true;
            this.lablDS.Location = new System.Drawing.Point(166, 200);
            this.lablDS.Name = "lablDS";
            this.lablDS.Size = new System.Drawing.Size(55, 13);
            this.lablDS.TabIndex = 51;
            this.lablDS.Text = "12345678";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(136, 200);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(22, 13);
            this.label19.TabIndex = 50;
            this.label19.Text = "DS";
            // 
            // lablCS
            // 
            this.lablCS.AutoSize = true;
            this.lablCS.Location = new System.Drawing.Point(166, 187);
            this.lablCS.Name = "lablCS";
            this.lablCS.Size = new System.Drawing.Size(55, 13);
            this.lablCS.TabIndex = 49;
            this.lablCS.Text = "12345678";
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(136, 187);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(21, 13);
            this.label22.TabIndex = 48;
            this.label22.Text = "CS";
            // 
            // lablSS
            // 
            this.lablSS.AutoSize = true;
            this.lablSS.Location = new System.Drawing.Point(166, 174);
            this.lablSS.Name = "lablSS";
            this.lablSS.Size = new System.Drawing.Size(55, 13);
            this.lablSS.TabIndex = 47;
            this.lablSS.Text = "12345678";
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(136, 174);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(21, 13);
            this.label25.TabIndex = 46;
            this.label25.Text = "SS";
            // 
            // lablFS
            // 
            this.lablFS.AutoSize = true;
            this.lablFS.Location = new System.Drawing.Point(166, 226);
            this.lablFS.Name = "lablFS";
            this.lablFS.Size = new System.Drawing.Size(55, 13);
            this.lablFS.TabIndex = 55;
            this.lablFS.Text = "12345678";
            // 
            // label29
            // 
            this.label29.AutoSize = true;
            this.label29.Location = new System.Drawing.Point(136, 226);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(20, 13);
            this.label29.TabIndex = 54;
            this.label29.Text = "FS";
            // 
            // lablGS
            // 
            this.lablGS.AutoSize = true;
            this.lablGS.Location = new System.Drawing.Point(166, 239);
            this.lablGS.Name = "lablGS";
            this.lablGS.Size = new System.Drawing.Size(55, 13);
            this.lablGS.TabIndex = 57;
            this.lablGS.Text = "12345678";
            // 
            // label32
            // 
            this.label32.AutoSize = true;
            this.label32.Location = new System.Drawing.Point(136, 239);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(22, 13);
            this.label32.TabIndex = 56;
            this.label32.Text = "GS";
            // 
            // lablFlagsText
            // 
            this.lablFlagsText.AutoSize = true;
            this.lablFlagsText.Location = new System.Drawing.Point(40, 151);
            this.lablFlagsText.Name = "lablFlagsText";
            this.lablFlagsText.Size = new System.Drawing.Size(55, 13);
            this.lablFlagsText.TabIndex = 60;
            this.lablFlagsText.Text = "12345678";
            // 
            // lablFlags
            // 
            this.lablFlags.AutoSize = true;
            this.lablFlags.Location = new System.Drawing.Point(40, 138);
            this.lablFlags.Name = "lablFlags";
            this.lablFlags.Size = new System.Drawing.Size(55, 13);
            this.lablFlags.TabIndex = 59;
            this.lablFlags.Text = "12345678";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(10, 138);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(32, 13);
            this.label11.TabIndex = 58;
            this.label11.Text = "Flags";
            // 
            // lboxDisassemble
            // 
            this.lboxDisassemble.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lboxDisassemble.FormattingEnabled = true;
            this.lboxDisassemble.Location = new System.Drawing.Point(3, 3);
            this.lboxDisassemble.Name = "lboxDisassemble";
            this.lboxDisassemble.Size = new System.Drawing.Size(696, 628);
            this.lboxDisassemble.TabIndex = 3;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1043, 675);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.panel1);
            this.Name = "FormMain";
            this.Text = "Cosmos GDB Debugger";
            this.panel1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button butnConnect;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button butnStep;
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
        private System.Windows.Forms.ListBox lboxDisassemble;
    }
}

