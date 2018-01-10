namespace Cosmos.VS.ProjectSystem.VS.PropertyPages
{
    partial class CosmosPropertyPageControl
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
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CosmosPropertyPageControl));
            this.panel1 = new System.Windows.Forms.Panel();
            this.lablCurrentProfile = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.TabControl1 = new System.Windows.Forms.TabControl();
            this.tabProfile = new System.Windows.Forms.TabPage();
            this.lablPreset = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.lboxProfile = new System.Windows.Forms.ListBox();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.butnProfileClone = new System.Windows.Forms.ToolStripButton();
            this.butnProfileDelete = new System.Windows.Forms.ToolStripButton();
            this.butnProfileRename = new System.Windows.Forms.ToolStripButton();
            this.lablDeployText = new System.Windows.Forms.Label();
            this.lablBuildOnly = new System.Windows.Forms.Label();
            this.tabCompile = new System.Windows.Forms.TabPage();
            this.labelBinFormat = new System.Windows.Forms.Label();
            this.comboBinFormat = new System.Windows.Forms.ComboBox();
            this.comboFramework = new System.Windows.Forms.ComboBox();
            this.buttonOutputBrowse = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.textOutputPath = new System.Windows.Forms.TextBox();
            this.labelFramework = new System.Windows.Forms.Label();
            this.tabAssembler = new System.Windows.Forms.TabPage();
            this.checkUseInternalAssembler = new System.Windows.Forms.CheckBox();
            this.labelInternalAssembler = new System.Windows.Forms.Label();
            this.tabDebug = new System.Windows.Forms.TabPage();
            this.chckEnableDebugStub = new System.Windows.Forms.CheckBox();
            this.panlDebugSettings = new System.Windows.Forms.Panel();
            this.stackCorruptionDetectionGroupBox = new System.Windows.Forms.GroupBox();
            this.label12 = new System.Windows.Forms.Label();
            this.comboStackCorruptionDetectionLevel = new System.Windows.Forms.ComboBox();
            this.chkEnableStackCorruptionDetection = new System.Windows.Forms.CheckBox();
            this.debugLevelGroupBox = new System.Windows.Forms.GroupBox();
            this.comboTraceMode = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.comboDebugMode = new System.Windows.Forms.ComboBox();
            this.debugStubGroupBox = new System.Windows.Forms.GroupBox();
            this.checkIgnoreDebugStubAttribute = new System.Windows.Forms.CheckBox();
            this.label9 = new System.Windows.Forms.Label();
            this.cmboVisualStudioDebugPort = new System.Windows.Forms.ComboBox();
            this.cmboCosmosDebugPort = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.tabDeployment = new System.Windows.Forms.TabPage();
            this.lboxDeployment = new System.Windows.Forms.ListBox();
            this.tabLaunch = new System.Windows.Forms.TabPage();
            this.lboxLaunch = new System.Windows.Forms.ListBox();
            this.tabVMware = new System.Windows.Forms.TabPage();
            this.checkEnableGDB = new System.Windows.Forms.CheckBox();
            this.checkStartCosmosGDB = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cmboVMwareEdition = new System.Windows.Forms.ComboBox();
            this.tabBochs = new System.Windows.Forms.TabPage();
            this.checkStartBochsDebugGui = new System.Windows.Forms.CheckBox();
            this.checkEnableBochsDebug = new System.Windows.Forms.CheckBox();
            this.tabPXE = new System.Windows.Forms.TabPage();
            this.butnPxeRefresh = new System.Windows.Forms.Button();
            this.comboPxeInterface = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabUSB = new System.Windows.Forms.TabPage();
            this.label7 = new System.Windows.Forms.Label();
            this.tabISO = new System.Windows.Forms.TabPage();
            this.label8 = new System.Windows.Forms.Label();
            this.tabSlave = new System.Windows.Forms.TabPage();
            this.cmboSlavePort = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.tabHyperV = new System.Windows.Forms.TabPage();
            this.panel1.SuspendLayout();
            this.TabControl1.SuspendLayout();
            this.tabProfile.SuspendLayout();
            this.panel2.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.tabCompile.SuspendLayout();
            this.tabAssembler.SuspendLayout();
            this.tabDebug.SuspendLayout();
            this.panlDebugSettings.SuspendLayout();
            this.stackCorruptionDetectionGroupBox.SuspendLayout();
            this.debugLevelGroupBox.SuspendLayout();
            this.debugStubGroupBox.SuspendLayout();
            this.tabDeployment.SuspendLayout();
            this.tabLaunch.SuspendLayout();
            this.tabVMware.SuspendLayout();
            this.tabBochs.SuspendLayout();
            this.tabPXE.SuspendLayout();
            this.tabUSB.SuspendLayout();
            this.tabISO.SuspendLayout();
            this.tabSlave.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lablCurrentProfile);
            this.panel1.Controls.Add(this.label11);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(635, 43);
            this.panel1.TabIndex = 3;
            // 
            // lablCurrentProfile
            // 
            this.lablCurrentProfile.AutoSize = true;
            this.lablCurrentProfile.Location = new System.Drawing.Point(99, 17);
            this.lablCurrentProfile.Name = "lablCurrentProfile";
            this.lablCurrentProfile.Size = new System.Drawing.Size(41, 13);
            this.lablCurrentProfile.TabIndex = 1;
            this.lablCurrentProfile.Text = "label12";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(17, 17);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(76, 13);
            this.label11.TabIndex = 0;
            this.label11.Text = "Current Profile:";
            // 
            // TabControl1
            // 
            this.TabControl1.Controls.Add(this.tabProfile);
            this.TabControl1.Controls.Add(this.tabCompile);
            this.TabControl1.Controls.Add(this.tabAssembler);
            this.TabControl1.Controls.Add(this.tabDebug);
            this.TabControl1.Controls.Add(this.tabDeployment);
            this.TabControl1.Controls.Add(this.tabLaunch);
            this.TabControl1.Controls.Add(this.tabVMware);
            this.TabControl1.Controls.Add(this.tabHyperV);
            this.TabControl1.Controls.Add(this.tabBochs);
            this.TabControl1.Controls.Add(this.tabPXE);
            this.TabControl1.Controls.Add(this.tabUSB);
            this.TabControl1.Controls.Add(this.tabISO);
            this.TabControl1.Controls.Add(this.tabSlave);
            this.TabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TabControl1.Location = new System.Drawing.Point(0, 43);
            this.TabControl1.Multiline = true;
            this.TabControl1.Name = "TabControl1";
            this.TabControl1.SelectedIndex = 0;
            this.TabControl1.Size = new System.Drawing.Size(635, 512);
            this.TabControl1.TabIndex = 1;
            // 
            // tabProfile
            // 
            this.tabProfile.Controls.Add(this.lablPreset);
            this.tabProfile.Controls.Add(this.panel2);
            this.tabProfile.Controls.Add(this.lablDeployText);
            this.tabProfile.Controls.Add(this.lablBuildOnly);
            this.tabProfile.Location = new System.Drawing.Point(4, 22);
            this.tabProfile.Name = "tabProfile";
            this.tabProfile.Padding = new System.Windows.Forms.Padding(3);
            this.tabProfile.Size = new System.Drawing.Size(627, 486);
            this.tabProfile.TabIndex = 8;
            this.tabProfile.Text = "Profile";
            this.tabProfile.UseVisualStyleBackColor = true;
            // 
            // lablPreset
            // 
            this.lablPreset.AutoSize = true;
            this.lablPreset.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lablPreset.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.lablPreset.Location = new System.Drawing.Point(217, 17);
            this.lablPreset.Name = "lablPreset";
            this.lablPreset.Size = new System.Drawing.Size(247, 13);
            this.lablPreset.TabIndex = 7;
            this.lablPreset.Text = "** This is a preset. Some options are restricted.";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.lboxProfile);
            this.panel2.Controls.Add(this.toolStrip1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel2.Location = new System.Drawing.Point(3, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(200, 480);
            this.panel2.TabIndex = 6;
            // 
            // lboxProfile
            // 
            this.lboxProfile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lboxProfile.FormattingEnabled = true;
            this.lboxProfile.Location = new System.Drawing.Point(0, 27);
            this.lboxProfile.Name = "lboxProfile";
            this.lboxProfile.Size = new System.Drawing.Size(200, 453);
            this.lboxProfile.Sorted = true;
            this.lboxProfile.TabIndex = 3;
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.butnProfileClone,
            this.butnProfileDelete,
            this.butnProfileRename});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(200, 27);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // butnProfileClone
            // 
            this.butnProfileClone.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butnProfileClone.Image = ((System.Drawing.Image)(resources.GetObject("butnProfileClone.Image")));
            this.butnProfileClone.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butnProfileClone.Name = "butnProfileClone";
            this.butnProfileClone.Size = new System.Drawing.Size(24, 24);
            this.butnProfileClone.Text = "Clone";
            this.butnProfileClone.ToolTipText = "Create a new profile from an existing one.";
            // 
            // butnProfileDelete
            // 
            this.butnProfileDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butnProfileDelete.Image = ((System.Drawing.Image)(resources.GetObject("butnProfileDelete.Image")));
            this.butnProfileDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butnProfileDelete.Name = "butnProfileDelete";
            this.butnProfileDelete.Size = new System.Drawing.Size(24, 24);
            this.butnProfileDelete.Text = "Delete";
            this.butnProfileDelete.ToolTipText = "Delete selected profile";
            // 
            // butnProfileRename
            // 
            this.butnProfileRename.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butnProfileRename.Image = ((System.Drawing.Image)(resources.GetObject("butnProfileRename.Image")));
            this.butnProfileRename.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butnProfileRename.Name = "butnProfileRename";
            this.butnProfileRename.Size = new System.Drawing.Size(24, 24);
            this.butnProfileRename.Text = "Rename";
            this.butnProfileRename.ToolTipText = "Rename selected profile.";
            // 
            // lablDeployText
            // 
            this.lablDeployText.Location = new System.Drawing.Point(217, 44);
            this.lablDeployText.Name = "lablDeployText";
            this.lablDeployText.Size = new System.Drawing.Size(228, 137);
            this.lablDeployText.TabIndex = 4;
            this.lablDeployText.Text = "label1";
            // 
            // lablBuildOnly
            // 
            this.lablBuildOnly.AutoSize = true;
            this.lablBuildOnly.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lablBuildOnly.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.lablBuildOnly.Location = new System.Drawing.Point(217, 4);
            this.lablBuildOnly.Name = "lablBuildOnly";
            this.lablBuildOnly.Size = new System.Drawing.Size(310, 13);
            this.lablBuildOnly.TabIndex = 3;
            this.lablBuildOnly.Text = "** This is a build only option. No process will be launched.";
            // 
            // tabCompile
            // 
            this.tabCompile.AutoScroll = true;
            this.tabCompile.Controls.Add(this.labelBinFormat);
            this.tabCompile.Controls.Add(this.comboBinFormat);
            this.tabCompile.Controls.Add(this.comboFramework);
            this.tabCompile.Controls.Add(this.buttonOutputBrowse);
            this.tabCompile.Controls.Add(this.label2);
            this.tabCompile.Controls.Add(this.textOutputPath);
            this.tabCompile.Controls.Add(this.labelFramework);
            this.tabCompile.Location = new System.Drawing.Point(4, 22);
            this.tabCompile.Name = "tabCompile";
            this.tabCompile.Padding = new System.Windows.Forms.Padding(3);
            this.tabCompile.Size = new System.Drawing.Size(627, 486);
            this.tabCompile.TabIndex = 0;
            this.tabCompile.Text = "Compile";
            this.tabCompile.UseVisualStyleBackColor = true;
            // 
            // labelBinFormat
            // 
            this.labelBinFormat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.labelBinFormat.AutoSize = true;
            this.labelBinFormat.Enabled = false;
            this.labelBinFormat.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelBinFormat.Location = new System.Drawing.Point(18, 113);
            this.labelBinFormat.Margin = new System.Windows.Forms.Padding(22, 3, 0, 3);
            this.labelBinFormat.Name = "labelBinFormat";
            this.labelBinFormat.Size = new System.Drawing.Size(66, 15);
            this.labelBinFormat.TabIndex = 23;
            this.labelBinFormat.Text = "Bin format:";
            this.labelBinFormat.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // comboBinFormat
            // 
            this.comboBinFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBinFormat.FormattingEnabled = true;
            this.comboBinFormat.Location = new System.Drawing.Point(34, 136);
            this.comboBinFormat.Name = "comboBinFormat";
            this.comboBinFormat.Size = new System.Drawing.Size(228, 21);
            this.comboBinFormat.TabIndex = 22;
            // 
            // comboFramework
            // 
            this.comboFramework.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboFramework.Enabled = false;
            this.comboFramework.FormattingEnabled = true;
            this.comboFramework.Location = new System.Drawing.Point(34, 87);
            this.comboFramework.Name = "comboFramework";
            this.comboFramework.Size = new System.Drawing.Size(228, 21);
            this.comboFramework.TabIndex = 5;
            // 
            // buttonOutputBrowse
            // 
            this.buttonOutputBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOutputBrowse.Location = new System.Drawing.Point(430, 37);
            this.buttonOutputBrowse.Name = "buttonOutputBrowse";
            this.buttonOutputBrowse.Size = new System.Drawing.Size(21, 23);
            this.buttonOutputBrowse.TabIndex = 20;
            this.buttonOutputBrowse.Text = "..";
            this.buttonOutputBrowse.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(18, 17);
            this.label2.Margin = new System.Windows.Forms.Padding(22, 3, 0, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 15);
            this.label2.TabIndex = 19;
            this.label2.Text = "Output path:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textOutputPath
            // 
            this.textOutputPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textOutputPath.Location = new System.Drawing.Point(34, 38);
            this.textOutputPath.Name = "textOutputPath";
            this.textOutputPath.Size = new System.Drawing.Size(390, 20);
            this.textOutputPath.TabIndex = 4;
            // 
            // labelFramework
            // 
            this.labelFramework.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.labelFramework.AutoSize = true;
            this.labelFramework.Enabled = false;
            this.labelFramework.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelFramework.Location = new System.Drawing.Point(18, 66);
            this.labelFramework.Margin = new System.Windows.Forms.Padding(22, 3, 0, 3);
            this.labelFramework.Name = "labelFramework";
            this.labelFramework.Size = new System.Drawing.Size(69, 15);
            this.labelFramework.TabIndex = 21;
            this.labelFramework.Text = "Framework:";
            this.labelFramework.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tabAssembler
            // 
            this.tabAssembler.Controls.Add(this.checkUseInternalAssembler);
            this.tabAssembler.Controls.Add(this.labelInternalAssembler);
            this.tabAssembler.Location = new System.Drawing.Point(4, 22);
            this.tabAssembler.Name = "tabAssembler";
            this.tabAssembler.Size = new System.Drawing.Size(627, 486);
            this.tabAssembler.TabIndex = 10;
            this.tabAssembler.Text = "Assembler";
            this.tabAssembler.UseVisualStyleBackColor = true;
            // 
            // checkUseInternalAssembler
            // 
            this.checkUseInternalAssembler.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.checkUseInternalAssembler.AutoSize = true;
            this.checkUseInternalAssembler.Enabled = false;
            this.checkUseInternalAssembler.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkUseInternalAssembler.Location = new System.Drawing.Point(18, 12);
            this.checkUseInternalAssembler.Margin = new System.Windows.Forms.Padding(22, 12, 3, 3);
            this.checkUseInternalAssembler.Name = "checkUseInternalAssembler";
            this.checkUseInternalAssembler.Size = new System.Drawing.Size(146, 19);
            this.checkUseInternalAssembler.TabIndex = 6;
            this.checkUseInternalAssembler.Text = "Use Internal Assembler";
            this.checkUseInternalAssembler.UseVisualStyleBackColor = true;
            // 
            // labelInternalAssembler
            // 
            this.labelInternalAssembler.Enabled = false;
            this.labelInternalAssembler.Location = new System.Drawing.Point(40, 32);
            this.labelInternalAssembler.Margin = new System.Windows.Forms.Padding(44, 0, 3, 0);
            this.labelInternalAssembler.Name = "labelInternalAssembler";
            this.labelInternalAssembler.Size = new System.Drawing.Size(224, 18);
            this.labelInternalAssembler.TabIndex = 20;
            this.labelInternalAssembler.Text = "Experimental. Check if you like to crash!";
            // 
            // tabDebug
            // 
            this.tabDebug.AutoScroll = true;
            this.tabDebug.Controls.Add(this.chckEnableDebugStub);
            this.tabDebug.Controls.Add(this.panlDebugSettings);
            this.tabDebug.Location = new System.Drawing.Point(4, 22);
            this.tabDebug.Name = "tabDebug";
            this.tabDebug.Padding = new System.Windows.Forms.Padding(3);
            this.tabDebug.Size = new System.Drawing.Size(627, 486);
            this.tabDebug.TabIndex = 2;
            this.tabDebug.Text = "Debug";
            this.tabDebug.UseVisualStyleBackColor = true;
            // 
            // chckEnableDebugStub
            // 
            this.chckEnableDebugStub.AutoSize = true;
            this.chckEnableDebugStub.Location = new System.Drawing.Point(14, 6);
            this.chckEnableDebugStub.Name = "chckEnableDebugStub";
            this.chckEnableDebugStub.Size = new System.Drawing.Size(154, 17);
            this.chckEnableDebugStub.TabIndex = 7;
            this.chckEnableDebugStub.Text = "Enable Remote Debugging";
            this.chckEnableDebugStub.UseVisualStyleBackColor = true;
            // 
            // panlDebugSettings
            // 
            this.panlDebugSettings.Controls.Add(this.stackCorruptionDetectionGroupBox);
            this.panlDebugSettings.Controls.Add(this.debugLevelGroupBox);
            this.panlDebugSettings.Controls.Add(this.debugStubGroupBox);
            this.panlDebugSettings.Location = new System.Drawing.Point(6, 29);
            this.panlDebugSettings.Name = "panlDebugSettings";
            this.panlDebugSettings.Size = new System.Drawing.Size(280, 400);
            this.panlDebugSettings.TabIndex = 33;
            // 
            // stackCorruptionDetectionGroupBox
            // 
            this.stackCorruptionDetectionGroupBox.Controls.Add(this.label12);
            this.stackCorruptionDetectionGroupBox.Controls.Add(this.comboStackCorruptionDetectionLevel);
            this.stackCorruptionDetectionGroupBox.Controls.Add(this.chkEnableStackCorruptionDetection);
            this.stackCorruptionDetectionGroupBox.Location = new System.Drawing.Point(10, 10);
            this.stackCorruptionDetectionGroupBox.Name = "stackCorruptionDetectionGroupBox";
            this.stackCorruptionDetectionGroupBox.Size = new System.Drawing.Size(260, 90);
            this.stackCorruptionDetectionGroupBox.TabIndex = 34;
            this.stackCorruptionDetectionGroupBox.TabStop = false;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(3, 42);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(85, 13);
            this.label12.TabIndex = 30;
            this.label12.Text = "Detection Level:";
            // 
            // comboStackCorruptionDetectionLevel
            // 
            this.comboStackCorruptionDetectionLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboStackCorruptionDetectionLevel.FormattingEnabled = true;
            this.comboStackCorruptionDetectionLevel.Location = new System.Drawing.Point(33, 58);
            this.comboStackCorruptionDetectionLevel.Name = "comboStackCorruptionDetectionLevel";
            this.comboStackCorruptionDetectionLevel.Size = new System.Drawing.Size(220, 21);
            this.comboStackCorruptionDetectionLevel.TabIndex = 9;
            this.comboStackCorruptionDetectionLevel.SelectedIndexChanged += new System.EventHandler(this.stackCorruptionDetectionLevelComboBox_SelectedIndexChanged);
            // 
            // chkEnableStackCorruptionDetection
            // 
            this.chkEnableStackCorruptionDetection.AutoSize = true;
            this.chkEnableStackCorruptionDetection.Location = new System.Drawing.Point(6, 19);
            this.chkEnableStackCorruptionDetection.Name = "chkEnableStackCorruptionDetection";
            this.chkEnableStackCorruptionDetection.Size = new System.Drawing.Size(190, 17);
            this.chkEnableStackCorruptionDetection.TabIndex = 8;
            this.chkEnableStackCorruptionDetection.Text = "Enable Stack Corruption Detection";
            this.chkEnableStackCorruptionDetection.UseVisualStyleBackColor = true;
            this.chkEnableStackCorruptionDetection.CheckedChanged += new System.EventHandler(this.chkEnableStacckCorruptionDetection_CheckedChanged);
            // 
            // debugLevelGroupBox
            // 
            this.debugLevelGroupBox.Controls.Add(this.comboTraceMode);
            this.debugLevelGroupBox.Controls.Add(this.label5);
            this.debugLevelGroupBox.Controls.Add(this.label4);
            this.debugLevelGroupBox.Controls.Add(this.comboDebugMode);
            this.debugLevelGroupBox.Location = new System.Drawing.Point(10, 110);
            this.debugLevelGroupBox.Name = "debugLevelGroupBox";
            this.debugLevelGroupBox.Size = new System.Drawing.Size(260, 125);
            this.debugLevelGroupBox.TabIndex = 34;
            this.debugLevelGroupBox.TabStop = false;
            // 
            // comboTraceMode
            // 
            this.comboTraceMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboTraceMode.FormattingEnabled = true;
            this.comboTraceMode.Location = new System.Drawing.Point(34, 94);
            this.comboTraceMode.Name = "comboTraceMode";
            this.comboTraceMode.Size = new System.Drawing.Size(220, 21);
            this.comboTraceMode.TabIndex = 10;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(3, 73);
            this.label5.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(49, 15);
            this.label5.TabIndex = 26;
            this.label5.Text = "Tracing:";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(3, 19);
            this.label4.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(98, 21);
            this.label4.TabIndex = 24;
            this.label4.Text = "Debug Level:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // comboDebugMode
            // 
            this.comboDebugMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboDebugMode.FormattingEnabled = true;
            this.comboDebugMode.Location = new System.Drawing.Point(34, 46);
            this.comboDebugMode.Name = "comboDebugMode";
            this.comboDebugMode.Size = new System.Drawing.Size(220, 21);
            this.comboDebugMode.TabIndex = 9;
            // 
            // debugStubGroupBox
            // 
            this.debugStubGroupBox.Controls.Add(this.checkIgnoreDebugStubAttribute);
            this.debugStubGroupBox.Controls.Add(this.label9);
            this.debugStubGroupBox.Controls.Add(this.cmboVisualStudioDebugPort);
            this.debugStubGroupBox.Controls.Add(this.cmboCosmosDebugPort);
            this.debugStubGroupBox.Controls.Add(this.label10);
            this.debugStubGroupBox.Location = new System.Drawing.Point(10, 245);
            this.debugStubGroupBox.Name = "debugStubGroupBox";
            this.debugStubGroupBox.Size = new System.Drawing.Size(260, 140);
            this.debugStubGroupBox.TabIndex = 31;
            this.debugStubGroupBox.TabStop = false;
            // 
            // checkIgnoreDebugStubAttribute
            // 
            this.checkIgnoreDebugStubAttribute.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkIgnoreDebugStubAttribute.Location = new System.Drawing.Point(6, 19);
            this.checkIgnoreDebugStubAttribute.Name = "checkIgnoreDebugStubAttribute";
            this.checkIgnoreDebugStubAttribute.Size = new System.Drawing.Size(218, 20);
            this.checkIgnoreDebugStubAttribute.TabIndex = 11;
            this.checkIgnoreDebugStubAttribute.Text = "Ignore DebugStub Attribute Settings";
            this.checkIgnoreDebugStubAttribute.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 42);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(69, 13);
            this.label9.TabIndex = 29;
            this.label9.Text = "Cosmos Port:";
            // 
            // cmboVisualStudioDebugPort
            // 
            this.cmboVisualStudioDebugPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmboVisualStudioDebugPort.FormattingEnabled = true;
            this.cmboVisualStudioDebugPort.Items.AddRange(new object[] {
            "Ethernet (not supported yet)",
            "Serial Com1",
            "Serial Com2",
            "Serial Com3",
            "Serial Com4"});
            this.cmboVisualStudioDebugPort.Location = new System.Drawing.Point(34, 106);
            this.cmboVisualStudioDebugPort.Name = "cmboVisualStudioDebugPort";
            this.cmboVisualStudioDebugPort.Size = new System.Drawing.Size(220, 21);
            this.cmboVisualStudioDebugPort.Sorted = true;
            this.cmboVisualStudioDebugPort.TabIndex = 13;
            // 
            // cmboCosmosDebugPort
            // 
            this.cmboCosmosDebugPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmboCosmosDebugPort.FormattingEnabled = true;
            this.cmboCosmosDebugPort.Items.AddRange(new object[] {
            "Ethernet (not supported yet)",
            "Serial Com1",
            "Serial Com2",
            "Serial Com3",
            "Serial Com4"});
            this.cmboCosmosDebugPort.Location = new System.Drawing.Point(34, 58);
            this.cmboCosmosDebugPort.Name = "cmboCosmosDebugPort";
            this.cmboCosmosDebugPort.Size = new System.Drawing.Size(220, 21);
            this.cmboCosmosDebugPort.Sorted = true;
            this.cmboCosmosDebugPort.TabIndex = 12;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(6, 90);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(93, 13);
            this.label10.TabIndex = 30;
            this.label10.Text = "Visual Studio Port:";
            // 
            // tabDeployment
            // 
            this.tabDeployment.Controls.Add(this.lboxDeployment);
            this.tabDeployment.Location = new System.Drawing.Point(4, 22);
            this.tabDeployment.Name = "tabDeployment";
            this.tabDeployment.Size = new System.Drawing.Size(627, 486);
            this.tabDeployment.TabIndex = 11;
            this.tabDeployment.Text = "Deployment";
            this.tabDeployment.UseVisualStyleBackColor = true;
            // 
            // lboxDeployment
            // 
            this.lboxDeployment.Dock = System.Windows.Forms.DockStyle.Left;
            this.lboxDeployment.FormattingEnabled = true;
            this.lboxDeployment.Location = new System.Drawing.Point(0, 0);
            this.lboxDeployment.Name = "lboxDeployment";
            this.lboxDeployment.Size = new System.Drawing.Size(206, 486);
            this.lboxDeployment.Sorted = true;
            this.lboxDeployment.TabIndex = 15;
            // 
            // tabLaunch
            // 
            this.tabLaunch.Controls.Add(this.lboxLaunch);
            this.tabLaunch.Location = new System.Drawing.Point(4, 22);
            this.tabLaunch.Name = "tabLaunch";
            this.tabLaunch.Size = new System.Drawing.Size(627, 486);
            this.tabLaunch.TabIndex = 12;
            this.tabLaunch.Text = "Launch";
            this.tabLaunch.UseVisualStyleBackColor = true;
            // 
            // lboxLaunch
            // 
            this.lboxLaunch.Dock = System.Windows.Forms.DockStyle.Left;
            this.lboxLaunch.FormattingEnabled = true;
            this.lboxLaunch.Location = new System.Drawing.Point(0, 0);
            this.lboxLaunch.Name = "lboxLaunch";
            this.lboxLaunch.Size = new System.Drawing.Size(206, 486);
            this.lboxLaunch.Sorted = true;
            this.lboxLaunch.TabIndex = 16;
            // 
            // tabVMware
            // 
            this.tabVMware.Controls.Add(this.checkEnableGDB);
            this.tabVMware.Controls.Add(this.checkStartCosmosGDB);
            this.tabVMware.Controls.Add(this.label3);
            this.tabVMware.Controls.Add(this.cmboVMwareEdition);
            this.tabVMware.Location = new System.Drawing.Point(4, 22);
            this.tabVMware.Name = "tabVMware";
            this.tabVMware.Padding = new System.Windows.Forms.Padding(3);
            this.tabVMware.Size = new System.Drawing.Size(627, 486);
            this.tabVMware.TabIndex = 4;
            this.tabVMware.Text = "VMware";
            this.tabVMware.UseVisualStyleBackColor = true;
            // 
            // checkEnableGDB
            // 
            this.checkEnableGDB.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkEnableGDB.Location = new System.Drawing.Point(9, 77);
            this.checkEnableGDB.Name = "checkEnableGDB";
            this.checkEnableGDB.Size = new System.Drawing.Size(218, 20);
            this.checkEnableGDB.TabIndex = 19;
            this.checkEnableGDB.Text = "Enable GDB Debugger";
            this.checkEnableGDB.UseVisualStyleBackColor = true;
            // 
            // checkStartCosmosGDB
            // 
            this.checkStartCosmosGDB.AutoSize = true;
            this.checkStartCosmosGDB.Enabled = false;
            this.checkStartCosmosGDB.Location = new System.Drawing.Point(24, 103);
            this.checkStartCosmosGDB.Margin = new System.Windows.Forms.Padding(25, 3, 3, 3);
            this.checkStartCosmosGDB.Name = "checkStartCosmosGDB";
            this.checkStartCosmosGDB.Size = new System.Drawing.Size(140, 17);
            this.checkStartCosmosGDB.TabIndex = 20;
            this.checkStartCosmosGDB.Text = "Use Cosmos GDB Client";
            this.checkStartCosmosGDB.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 12);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(42, 13);
            this.label3.TabIndex = 18;
            this.label3.Text = "Edition:";
            // 
            // cmboVMwareEdition
            // 
            this.cmboVMwareEdition.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmboVMwareEdition.FormattingEnabled = true;
            this.cmboVMwareEdition.Location = new System.Drawing.Point(26, 37);
            this.cmboVMwareEdition.Name = "cmboVMwareEdition";
            this.cmboVMwareEdition.Size = new System.Drawing.Size(143, 21);
            this.cmboVMwareEdition.Sorted = true;
            this.cmboVMwareEdition.TabIndex = 18;
            // 
            // tabBochs
            // 
            this.tabBochs.Controls.Add(this.checkStartBochsDebugGui);
            this.tabBochs.Controls.Add(this.checkEnableBochsDebug);
            this.tabBochs.Location = new System.Drawing.Point(4, 22);
            this.tabBochs.Name = "tabBochs";
            this.tabBochs.Padding = new System.Windows.Forms.Padding(3);
            this.tabBochs.Size = new System.Drawing.Size(627, 486);
            this.tabBochs.TabIndex = 5;
            this.tabBochs.Text = "Bochs";
            this.tabBochs.UseVisualStyleBackColor = true;
            // 
            // checkStartBochsDebugGui
            // 
            this.checkStartBochsDebugGui.Enabled = false;
            this.checkStartBochsDebugGui.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkStartBochsDebugGui.Location = new System.Drawing.Point(24, 43);
            this.checkStartBochsDebugGui.Name = "checkStartBochsDebugGui";
            this.checkStartBochsDebugGui.Size = new System.Drawing.Size(218, 20);
            this.checkStartBochsDebugGui.TabIndex = 35;
            this.checkStartBochsDebugGui.Text = "Use Bochs Debugger GUI";
            this.checkStartBochsDebugGui.UseVisualStyleBackColor = true;
            // 
            // checkEnableBochsDebug
            // 
            this.checkEnableBochsDebug.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkEnableBochsDebug.Location = new System.Drawing.Point(9, 17);
            this.checkEnableBochsDebug.Name = "checkEnableBochsDebug";
            this.checkEnableBochsDebug.Size = new System.Drawing.Size(218, 20);
            this.checkEnableBochsDebug.TabIndex = 21;
            this.checkEnableBochsDebug.Text = "Enable Bochs Debugger";
            this.checkEnableBochsDebug.UseVisualStyleBackColor = true;
            // 
            // tabPXE
            // 
            this.tabPXE.Controls.Add(this.butnPxeRefresh);
            this.tabPXE.Controls.Add(this.comboPxeInterface);
            this.tabPXE.Controls.Add(this.label1);
            this.tabPXE.Location = new System.Drawing.Point(4, 22);
            this.tabPXE.Name = "tabPXE";
            this.tabPXE.Padding = new System.Windows.Forms.Padding(3);
            this.tabPXE.Size = new System.Drawing.Size(627, 486);
            this.tabPXE.TabIndex = 6;
            this.tabPXE.Text = "PXE";
            this.tabPXE.UseVisualStyleBackColor = true;
            // 
            // butnPxeRefresh
            // 
            this.butnPxeRefresh.AutoSize = true;
            this.butnPxeRefresh.Image = ((System.Drawing.Image)(resources.GetObject("butnPxeRefresh.Image")));
            this.butnPxeRefresh.Location = new System.Drawing.Point(177, 31);
            this.butnPxeRefresh.Margin = new System.Windows.Forms.Padding(0);
            this.butnPxeRefresh.Name = "butnPxeRefresh";
            this.butnPxeRefresh.Size = new System.Drawing.Size(23, 23);
            this.butnPxeRefresh.TabIndex = 23;
            this.butnPxeRefresh.UseVisualStyleBackColor = true;
            // 
            // comboPxeInterface
            // 
            this.comboPxeInterface.Location = new System.Drawing.Point(28, 32);
            this.comboPxeInterface.Name = "comboPxeInterface";
            this.comboPxeInterface.Size = new System.Drawing.Size(146, 21);
            this.comboPxeInterface.TabIndex = 22;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Interface:";
            // 
            // tabUSB
            // 
            this.tabUSB.Controls.Add(this.label7);
            this.tabUSB.Location = new System.Drawing.Point(4, 22);
            this.tabUSB.Name = "tabUSB";
            this.tabUSB.Padding = new System.Windows.Forms.Padding(3);
            this.tabUSB.Size = new System.Drawing.Size(627, 486);
            this.tabUSB.TabIndex = 7;
            this.tabUSB.Text = "USB";
            this.tabUSB.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(16, 15);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(375, 102);
            this.label7.TabIndex = 1;
            this.label7.Text = "There are no current USB options. The target drive will be requested when you run" +
    " the project.";
            // 
            // tabISO
            // 
            this.tabISO.Controls.Add(this.label8);
            this.tabISO.Location = new System.Drawing.Point(4, 22);
            this.tabISO.Name = "tabISO";
            this.tabISO.Padding = new System.Windows.Forms.Padding(3);
            this.tabISO.Size = new System.Drawing.Size(627, 486);
            this.tabISO.TabIndex = 8;
            this.tabISO.Text = "ISO";
            this.tabISO.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(17, 16);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(375, 102);
            this.label8.TabIndex = 1;
            this.label8.Text = "There are currently no ISO options.";
            // 
            // tabSlave
            // 
            this.tabSlave.Controls.Add(this.cmboSlavePort);
            this.tabSlave.Controls.Add(this.label6);
            this.tabSlave.Location = new System.Drawing.Point(4, 22);
            this.tabSlave.Name = "tabSlave";
            this.tabSlave.Padding = new System.Windows.Forms.Padding(3);
            this.tabSlave.Size = new System.Drawing.Size(627, 486);
            this.tabSlave.TabIndex = 13;
            this.tabSlave.Text = "Slave";
            this.tabSlave.UseVisualStyleBackColor = true;
            // 
            // cmboSlavePort
            // 
            this.cmboSlavePort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmboSlavePort.FormattingEnabled = true;
            this.cmboSlavePort.Items.AddRange(new object[] {
            "Ethernet (not supported yet)",
            "Serial Com1",
            "Serial Com2",
            "Serial Com3",
            "Serial Com4"});
            this.cmboSlavePort.Location = new System.Drawing.Point(30, 31);
            this.cmboSlavePort.Name = "cmboSlavePort";
            this.cmboSlavePort.Size = new System.Drawing.Size(146, 21);
            this.cmboSlavePort.Sorted = true;
            this.cmboSlavePort.TabIndex = 23;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(15, 15);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(59, 13);
            this.label6.TabIndex = 34;
            this.label6.Text = "Slave Port:";
            // 
            // tabHyperV
            // 
            this.tabHyperV.Location = new System.Drawing.Point(4, 22);
            this.tabHyperV.Name = "tabHyperV";
            this.tabHyperV.Padding = new System.Windows.Forms.Padding(3);
            this.tabHyperV.Size = new System.Drawing.Size(627, 486);
            this.tabHyperV.TabIndex = 14;
            this.tabHyperV.Text = "Hyper-V";
            this.tabHyperV.UseVisualStyleBackColor = true;
            // 
            // CosmosPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.TabControl1);
            this.Controls.Add(this.panel1);
            this.Name = "CosmosPage";
            this.Size = new System.Drawing.Size(635, 555);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.TabControl1.ResumeLayout(false);
            this.tabProfile.ResumeLayout(false);
            this.tabProfile.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.tabCompile.ResumeLayout(false);
            this.tabCompile.PerformLayout();
            this.tabAssembler.ResumeLayout(false);
            this.tabAssembler.PerformLayout();
            this.tabDebug.ResumeLayout(false);
            this.tabDebug.PerformLayout();
            this.panlDebugSettings.ResumeLayout(false);
            this.stackCorruptionDetectionGroupBox.ResumeLayout(false);
            this.stackCorruptionDetectionGroupBox.PerformLayout();
            this.debugLevelGroupBox.ResumeLayout(false);
            this.debugLevelGroupBox.PerformLayout();
            this.debugStubGroupBox.ResumeLayout(false);
            this.debugStubGroupBox.PerformLayout();
            this.tabDeployment.ResumeLayout(false);
            this.tabLaunch.ResumeLayout(false);
            this.tabVMware.ResumeLayout(false);
            this.tabVMware.PerformLayout();
            this.tabBochs.ResumeLayout(false);
            this.tabPXE.ResumeLayout(false);
            this.tabPXE.PerformLayout();
            this.tabUSB.ResumeLayout(false);
            this.tabISO.ResumeLayout(false);
            this.tabSlave.ResumeLayout(false);
            this.tabSlave.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lablCurrentProfile;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TabControl TabControl1;
        private System.Windows.Forms.TabPage tabProfile;
        private System.Windows.Forms.Label lablDeployText;
        private System.Windows.Forms.Label lablBuildOnly;
        private System.Windows.Forms.TabPage tabDeployment;
        private System.Windows.Forms.ListBox lboxDeployment;
        private System.Windows.Forms.TabPage tabLaunch;
        private System.Windows.Forms.ListBox lboxLaunch;
        private System.Windows.Forms.TabPage tabCompile;
        private System.Windows.Forms.ComboBox comboFramework;
        private System.Windows.Forms.Button buttonOutputBrowse;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textOutputPath;
        private System.Windows.Forms.Label labelFramework;
        private System.Windows.Forms.TabPage tabAssembler;
        private System.Windows.Forms.CheckBox checkUseInternalAssembler;
        private System.Windows.Forms.Label labelInternalAssembler;
        private System.Windows.Forms.TabPage tabDebug;
        private System.Windows.Forms.CheckBox chckEnableDebugStub;
        private System.Windows.Forms.Panel panlDebugSettings;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cmboVisualStudioDebugPort;
        private System.Windows.Forms.ComboBox comboTraceMode;
        private System.Windows.Forms.ComboBox cmboCosmosDebugPort;
        private System.Windows.Forms.ComboBox comboDebugMode;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.CheckBox checkIgnoreDebugStubAttribute;
        private System.Windows.Forms.TabPage tabBochs;
        private System.Windows.Forms.CheckBox checkEnableBochsDebug;
        private System.Windows.Forms.TabPage tabVMware;
        private System.Windows.Forms.CheckBox checkEnableGDB;
        private System.Windows.Forms.CheckBox checkStartCosmosGDB;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmboVMwareEdition;
        private System.Windows.Forms.TabPage tabPXE;
        private System.Windows.Forms.TabPage tabUSB;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TabPage tabISO;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ListBox lboxProfile;
        private System.Windows.Forms.ToolStripButton butnProfileClone;
        private System.Windows.Forms.ToolStripButton butnProfileDelete;
        private System.Windows.Forms.Label lablPreset;
        private System.Windows.Forms.ToolStripButton butnProfileRename;
        private System.Windows.Forms.ComboBox comboPxeInterface;
        private System.Windows.Forms.Button butnPxeRefresh;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage tabSlave;
        private System.Windows.Forms.ComboBox cmboSlavePort;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox chkEnableStackCorruptionDetection;
        private System.Windows.Forms.Label labelBinFormat;
        private System.Windows.Forms.ComboBox comboBinFormat;
        private System.Windows.Forms.CheckBox checkStartBochsDebugGui;
        private System.Windows.Forms.GroupBox stackCorruptionDetectionGroupBox;
        private System.Windows.Forms.ComboBox comboStackCorruptionDetectionLevel;
        private System.Windows.Forms.GroupBox debugLevelGroupBox;
        private System.Windows.Forms.GroupBox debugStubGroupBox;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TabPage tabHyperV;
    }
}
