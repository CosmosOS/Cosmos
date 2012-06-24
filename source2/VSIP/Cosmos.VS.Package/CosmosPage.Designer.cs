namespace Cosmos.VS.Package
{
	partial class CosmosPage
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
      this.TabControl1 = new System.Windows.Forms.TabControl();
      this.tabDeploy = new System.Windows.Forms.TabPage();
      this.panel1 = new System.Windows.Forms.Panel();
      this.label1 = new System.Windows.Forms.Label();
      this.lboxDeployTarget = new System.Windows.Forms.ListBox();
      this.tabCompile = new System.Windows.Forms.TabPage();
      this.panel2 = new System.Windows.Forms.Panel();
      this.checkUseInternalAssembler = new System.Windows.Forms.CheckBox();
      this.labelInternalAssembler = new System.Windows.Forms.Label();
      this.comboFramework = new System.Windows.Forms.ComboBox();
      this.buttonOutputBrowse = new System.Windows.Forms.Button();
      this.label2 = new System.Windows.Forms.Label();
      this.textOutputPath = new System.Windows.Forms.TextBox();
      this.labelFramework = new System.Windows.Forms.Label();
      this.tabPage2 = new System.Windows.Forms.TabPage();
      this.panel3 = new System.Windows.Forms.Panel();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.checkEnableGDB = new System.Windows.Forms.CheckBox();
      this.checkStartCosmosGDB = new System.Windows.Forms.CheckBox();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.checkIgnoreDebugStubAttribute = new System.Windows.Forms.CheckBox();
      this.label4 = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.comboDebugMode = new System.Windows.Forms.ComboBox();
      this.comboTraceMode = new System.Windows.Forms.ComboBox();
      this.tabVMWare = new System.Windows.Forms.TabPage();
      this.label3 = new System.Windows.Forms.Label();
      this.comboFlavor = new System.Windows.Forms.ComboBox();
      this.TabControl1.SuspendLayout();
      this.tabDeploy.SuspendLayout();
      this.panel1.SuspendLayout();
      this.tabCompile.SuspendLayout();
      this.panel2.SuspendLayout();
      this.tabPage2.SuspendLayout();
      this.panel3.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.tabVMWare.SuspendLayout();
      this.SuspendLayout();
      // 
      // TabControl1
      // 
      this.TabControl1.Alignment = System.Windows.Forms.TabAlignment.Bottom;
      this.TabControl1.Controls.Add(this.tabDeploy);
      this.TabControl1.Controls.Add(this.tabCompile);
      this.TabControl1.Controls.Add(this.tabPage2);
      this.TabControl1.Controls.Add(this.tabVMWare);
      this.TabControl1.Location = new System.Drawing.Point(3, 44);
      this.TabControl1.Multiline = true;
      this.TabControl1.Name = "TabControl1";
      this.TabControl1.SelectedIndex = 0;
      this.TabControl1.Size = new System.Drawing.Size(486, 241);
      this.TabControl1.TabIndex = 2;
      // 
      // tabDeploy
      // 
      this.tabDeploy.AutoScroll = true;
      this.tabDeploy.Controls.Add(this.panel1);
      this.tabDeploy.Location = new System.Drawing.Point(4, 4);
      this.tabDeploy.Name = "tabDeploy";
      this.tabDeploy.Padding = new System.Windows.Forms.Padding(3);
      this.tabDeploy.Size = new System.Drawing.Size(478, 215);
      this.tabDeploy.TabIndex = 3;
      this.tabDeploy.Text = "Deploy";
      this.tabDeploy.UseVisualStyleBackColor = true;
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.label1);
      this.panel1.Controls.Add(this.lboxDeployTarget);
      this.panel1.Location = new System.Drawing.Point(2, 3);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(453, 212);
      this.panel1.TabIndex = 4;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(16, 5);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(138, 13);
      this.label1.TabIndex = 1;
      this.label1.Text = "Select target deployment:";
      // 
      // lboxDeployTarget
      // 
      this.lboxDeployTarget.FormattingEnabled = true;
      this.lboxDeployTarget.Location = new System.Drawing.Point(15, 27);
      this.lboxDeployTarget.Name = "lboxDeployTarget";
      this.lboxDeployTarget.Size = new System.Drawing.Size(206, 173);
      this.lboxDeployTarget.Sorted = true;
      this.lboxDeployTarget.TabIndex = 0;
      // 
      // tabCompile
      // 
      this.tabCompile.AutoScroll = true;
      this.tabCompile.Controls.Add(this.panel2);
      this.tabCompile.Location = new System.Drawing.Point(4, 4);
      this.tabCompile.Name = "tabCompile";
      this.tabCompile.Padding = new System.Windows.Forms.Padding(3);
      this.tabCompile.Size = new System.Drawing.Size(478, 215);
      this.tabCompile.TabIndex = 0;
      this.tabCompile.Text = "Compile";
      this.tabCompile.UseVisualStyleBackColor = true;
      // 
      // panel2
      // 
      this.panel2.Controls.Add(this.checkUseInternalAssembler);
      this.panel2.Controls.Add(this.labelInternalAssembler);
      this.panel2.Controls.Add(this.comboFramework);
      this.panel2.Controls.Add(this.buttonOutputBrowse);
      this.panel2.Controls.Add(this.label2);
      this.panel2.Controls.Add(this.textOutputPath);
      this.panel2.Controls.Add(this.labelFramework);
      this.panel2.Location = new System.Drawing.Point(2, 3);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(453, 212);
      this.panel2.TabIndex = 3;
      // 
      // checkUseInternalAssembler
      // 
      this.checkUseInternalAssembler.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
      this.checkUseInternalAssembler.AutoSize = true;
      this.checkUseInternalAssembler.Enabled = false;
      this.checkUseInternalAssembler.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkUseInternalAssembler.Location = new System.Drawing.Point(13, 118);
      this.checkUseInternalAssembler.Margin = new System.Windows.Forms.Padding(22, 12, 3, 3);
      this.checkUseInternalAssembler.Name = "checkUseInternalAssembler";
      this.checkUseInternalAssembler.Size = new System.Drawing.Size(146, 19);
      this.checkUseInternalAssembler.TabIndex = 19;
      this.checkUseInternalAssembler.Text = "Use Internal Assembler";
      this.checkUseInternalAssembler.UseVisualStyleBackColor = true;
      // 
      // labelInternalAssembler
      // 
      this.labelInternalAssembler.Enabled = false;
      this.labelInternalAssembler.Location = new System.Drawing.Point(35, 138);
      this.labelInternalAssembler.Margin = new System.Windows.Forms.Padding(44, 0, 3, 0);
      this.labelInternalAssembler.Name = "labelInternalAssembler";
      this.labelInternalAssembler.Size = new System.Drawing.Size(224, 18);
      this.labelInternalAssembler.TabIndex = 18;
      this.labelInternalAssembler.Text = "Experimental. Check if you like to crash!";
      // 
      // comboFramework
      // 
      this.comboFramework.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboFramework.Enabled = false;
      this.comboFramework.FormattingEnabled = true;
      this.comboFramework.Location = new System.Drawing.Point(22, 82);
      this.comboFramework.Name = "comboFramework";
      this.comboFramework.Size = new System.Drawing.Size(228, 21);
      this.comboFramework.TabIndex = 17;
      // 
      // buttonOutputBrowse
      // 
      this.buttonOutputBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonOutputBrowse.Location = new System.Drawing.Point(418, 32);
      this.buttonOutputBrowse.Name = "buttonOutputBrowse";
      this.buttonOutputBrowse.Size = new System.Drawing.Size(21, 23);
      this.buttonOutputBrowse.TabIndex = 15;
      this.buttonOutputBrowse.Text = "..";
      this.buttonOutputBrowse.UseVisualStyleBackColor = true;
      // 
      // label2
      // 
      this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
      this.label2.AutoSize = true;
      this.label2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label2.Location = new System.Drawing.Point(6, 12);
      this.label2.Margin = new System.Windows.Forms.Padding(22, 3, 0, 3);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(75, 15);
      this.label2.TabIndex = 14;
      this.label2.Text = "Output path:";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // textOutputPath
      // 
      this.textOutputPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.textOutputPath.Location = new System.Drawing.Point(22, 33);
      this.textOutputPath.Name = "textOutputPath";
      this.textOutputPath.Size = new System.Drawing.Size(390, 22);
      this.textOutputPath.TabIndex = 13;
      // 
      // labelFramework
      // 
      this.labelFramework.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
      this.labelFramework.AutoSize = true;
      this.labelFramework.Enabled = false;
      this.labelFramework.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelFramework.Location = new System.Drawing.Point(6, 61);
      this.labelFramework.Margin = new System.Windows.Forms.Padding(22, 3, 0, 3);
      this.labelFramework.Name = "labelFramework";
      this.labelFramework.Size = new System.Drawing.Size(69, 15);
      this.labelFramework.TabIndex = 16;
      this.labelFramework.Text = "Framework:";
      this.labelFramework.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // tabPage2
      // 
      this.tabPage2.AutoScroll = true;
      this.tabPage2.Controls.Add(this.panel3);
      this.tabPage2.Location = new System.Drawing.Point(4, 4);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage2.Size = new System.Drawing.Size(478, 215);
      this.tabPage2.TabIndex = 2;
      this.tabPage2.Text = "Debug";
      this.tabPage2.UseVisualStyleBackColor = true;
      // 
      // panel3
      // 
      this.panel3.Controls.Add(this.groupBox2);
      this.panel3.Controls.Add(this.groupBox1);
      this.panel3.Location = new System.Drawing.Point(3, 3);
      this.panel3.Name = "panel3";
      this.panel3.Size = new System.Drawing.Size(452, 247);
      this.panel3.TabIndex = 2;
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.checkEnableGDB);
      this.groupBox2.Controls.Add(this.checkStartCosmosGDB);
      this.groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox2.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.groupBox2.Location = new System.Drawing.Point(0, 163);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(452, 79);
      this.groupBox2.TabIndex = 1;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Assembly";
      // 
      // checkEnableGDB
      // 
      this.checkEnableGDB.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkEnableGDB.Location = new System.Drawing.Point(21, 21);
      this.checkEnableGDB.Name = "checkEnableGDB";
      this.checkEnableGDB.Size = new System.Drawing.Size(218, 20);
      this.checkEnableGDB.TabIndex = 20;
      this.checkEnableGDB.Text = "Enable GDB Debugger";
      this.checkEnableGDB.UseVisualStyleBackColor = true;
      // 
      // checkStartCosmosGDB
      // 
      this.checkStartCosmosGDB.AutoSize = true;
      this.checkStartCosmosGDB.Enabled = false;
      this.checkStartCosmosGDB.Location = new System.Drawing.Point(36, 47);
      this.checkStartCosmosGDB.Margin = new System.Windows.Forms.Padding(25, 3, 3, 3);
      this.checkStartCosmosGDB.Name = "checkStartCosmosGDB";
      this.checkStartCosmosGDB.Size = new System.Drawing.Size(148, 17);
      this.checkStartCosmosGDB.TabIndex = 22;
      this.checkStartCosmosGDB.Text = "Use Cosmos GDB Client";
      this.checkStartCosmosGDB.UseVisualStyleBackColor = true;
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.checkIgnoreDebugStubAttribute);
      this.groupBox1.Controls.Add(this.label4);
      this.groupBox1.Controls.Add(this.label5);
      this.groupBox1.Controls.Add(this.comboDebugMode);
      this.groupBox1.Controls.Add(this.comboTraceMode);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(452, 163);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Visual Studio";
      // 
      // checkIgnoreDebugStubAttribute
      // 
      this.checkIgnoreDebugStubAttribute.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkIgnoreDebugStubAttribute.Location = new System.Drawing.Point(21, 137);
      this.checkIgnoreDebugStubAttribute.Name = "checkIgnoreDebugStubAttribute";
      this.checkIgnoreDebugStubAttribute.Size = new System.Drawing.Size(218, 20);
      this.checkIgnoreDebugStubAttribute.TabIndex = 23;
      this.checkIgnoreDebugStubAttribute.Text = "Ignore DebugStub Attribute Settings";
      this.checkIgnoreDebugStubAttribute.UseVisualStyleBackColor = true;
      // 
      // label4
      // 
      this.label4.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label4.Location = new System.Drawing.Point(18, 21);
      this.label4.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(98, 21);
      this.label4.TabIndex = 15;
      this.label4.Text = "Debug Level:";
      this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label5.Location = new System.Drawing.Point(18, 75);
      this.label5.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(50, 15);
      this.label5.TabIndex = 18;
      this.label5.Text = "Tracing:";
      this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // comboDebugMode
      // 
      this.comboDebugMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboDebugMode.FormattingEnabled = true;
      this.comboDebugMode.Location = new System.Drawing.Point(36, 48);
      this.comboDebugMode.Name = "comboDebugMode";
      this.comboDebugMode.Size = new System.Drawing.Size(221, 21);
      this.comboDebugMode.TabIndex = 19;
      // 
      // comboTraceMode
      // 
      this.comboTraceMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboTraceMode.FormattingEnabled = true;
      this.comboTraceMode.Location = new System.Drawing.Point(36, 96);
      this.comboTraceMode.Name = "comboTraceMode";
      this.comboTraceMode.Size = new System.Drawing.Size(221, 21);
      this.comboTraceMode.TabIndex = 15;
      // 
      // tabVMWare
      // 
      this.tabVMWare.Controls.Add(this.label3);
      this.tabVMWare.Controls.Add(this.comboFlavor);
      this.tabVMWare.Location = new System.Drawing.Point(4, 4);
      this.tabVMWare.Name = "tabVMWare";
      this.tabVMWare.Padding = new System.Windows.Forms.Padding(3);
      this.tabVMWare.Size = new System.Drawing.Size(478, 215);
      this.tabVMWare.TabIndex = 4;
      this.tabVMWare.Text = "VMWare";
      this.tabVMWare.UseVisualStyleBackColor = true;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(6, 12);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(94, 13);
      this.label3.TabIndex = 18;
      this.label3.Text = "VMWare Edition:";
      // 
      // comboFlavor
      // 
      this.comboFlavor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboFlavor.FormattingEnabled = true;
      this.comboFlavor.Location = new System.Drawing.Point(26, 37);
      this.comboFlavor.Name = "comboFlavor";
      this.comboFlavor.Size = new System.Drawing.Size(143, 21);
      this.comboFlavor.Sorted = true;
      this.comboFlavor.TabIndex = 17;
      // 
      // CosmosPage
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.TabControl1);
      this.Name = "CosmosPage";
      this.Title = "Cosmos";
      this.Controls.SetChildIndex(this.TabControl1, 0);
      this.TabControl1.ResumeLayout(false);
      this.tabDeploy.ResumeLayout(false);
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      this.tabCompile.ResumeLayout(false);
      this.panel2.ResumeLayout(false);
      this.panel2.PerformLayout();
      this.tabPage2.ResumeLayout(false);
      this.panel3.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.tabVMWare.ResumeLayout(false);
      this.tabVMWare.PerformLayout();
      this.ResumeLayout(false);

		}

		#endregion

    private System.Windows.Forms.TabControl TabControl1;
    private System.Windows.Forms.TabPage tabCompile;
    private System.Windows.Forms.TabPage tabPage2;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.TabPage tabDeploy;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.TabPage tabVMWare;
    private System.Windows.Forms.ComboBox comboFlavor;
    private System.Windows.Forms.ListBox lboxDeployTarget;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.CheckBox checkUseInternalAssembler;
    private System.Windows.Forms.Label labelInternalAssembler;
    private System.Windows.Forms.ComboBox comboFramework;
    private System.Windows.Forms.Button buttonOutputBrowse;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox textOutputPath;
    private System.Windows.Forms.Label labelFramework;
    private System.Windows.Forms.Panel panel3;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.CheckBox checkEnableGDB;
    private System.Windows.Forms.CheckBox checkStartCosmosGDB;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.CheckBox checkIgnoreDebugStubAttribute;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.ComboBox comboDebugMode;
    private System.Windows.Forms.ComboBox comboTraceMode;



  }
}
