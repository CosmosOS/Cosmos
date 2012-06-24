namespace Cosmos.VS.Package
{
	partial class BuildPage
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
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.checkUseInternalAssembler = new System.Windows.Forms.CheckBox();
      this.labelInternalAssembler = new System.Windows.Forms.Label();
      this.comboFramework = new System.Windows.Forms.ComboBox();
      this.buttonOutputBrowse = new System.Windows.Forms.Button();
      this.label2 = new System.Windows.Forms.Label();
      this.textOutputPath = new System.Windows.Forms.TextBox();
      this.labelFramework = new System.Windows.Forms.Label();
      this.tabPage2 = new System.Windows.Forms.TabPage();
      this.tabVMWare = new System.Windows.Forms.TabPage();
      this.comboFlavor = new System.Windows.Forms.ComboBox();
      this.label3 = new System.Windows.Forms.Label();
      this.TabControl1.SuspendLayout();
      this.tabDeploy.SuspendLayout();
      this.panel1.SuspendLayout();
      this.tabCompile.SuspendLayout();
      this.panel2.SuspendLayout();
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
      this.panel2.Controls.Add(this.groupBox1);
      this.panel2.Location = new System.Drawing.Point(2, 3);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(453, 212);
      this.panel2.TabIndex = 3;
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.checkUseInternalAssembler);
      this.groupBox1.Controls.Add(this.labelInternalAssembler);
      this.groupBox1.Controls.Add(this.comboFramework);
      this.groupBox1.Controls.Add(this.buttonOutputBrowse);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.textOutputPath);
      this.groupBox1.Controls.Add(this.labelFramework);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(453, 175);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Target";
      // 
      // checkUseInternalAssembler
      // 
      this.checkUseInternalAssembler.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
      this.checkUseInternalAssembler.AutoSize = true;
      this.checkUseInternalAssembler.Enabled = false;
      this.checkUseInternalAssembler.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkUseInternalAssembler.Location = new System.Drawing.Point(13, 127);
      this.checkUseInternalAssembler.Margin = new System.Windows.Forms.Padding(22, 12, 3, 3);
      this.checkUseInternalAssembler.Name = "checkUseInternalAssembler";
      this.checkUseInternalAssembler.Size = new System.Drawing.Size(146, 19);
      this.checkUseInternalAssembler.TabIndex = 12;
      this.checkUseInternalAssembler.Text = "Use Internal Assembler";
      this.checkUseInternalAssembler.UseVisualStyleBackColor = true;
      // 
      // labelInternalAssembler
      // 
      this.labelInternalAssembler.Enabled = false;
      this.labelInternalAssembler.Location = new System.Drawing.Point(35, 147);
      this.labelInternalAssembler.Margin = new System.Windows.Forms.Padding(44, 0, 3, 0);
      this.labelInternalAssembler.Name = "labelInternalAssembler";
      this.labelInternalAssembler.Size = new System.Drawing.Size(224, 18);
      this.labelInternalAssembler.TabIndex = 11;
      this.labelInternalAssembler.Text = "Experimental. Check if you like to crash!";
      // 
      // comboFramework
      // 
      this.comboFramework.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboFramework.Enabled = false;
      this.comboFramework.FormattingEnabled = true;
      this.comboFramework.Location = new System.Drawing.Point(22, 91);
      this.comboFramework.Name = "comboFramework";
      this.comboFramework.Size = new System.Drawing.Size(228, 21);
      this.comboFramework.TabIndex = 9;
      // 
      // buttonOutputBrowse
      // 
      this.buttonOutputBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonOutputBrowse.Location = new System.Drawing.Point(418, 41);
      this.buttonOutputBrowse.Name = "buttonOutputBrowse";
      this.buttonOutputBrowse.Size = new System.Drawing.Size(21, 23);
      this.buttonOutputBrowse.TabIndex = 1;
      this.buttonOutputBrowse.Text = "..";
      this.buttonOutputBrowse.UseVisualStyleBackColor = true;
      // 
      // label2
      // 
      this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
      this.label2.AutoSize = true;
      this.label2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label2.Location = new System.Drawing.Point(6, 21);
      this.label2.Margin = new System.Windows.Forms.Padding(22, 3, 0, 3);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(75, 15);
      this.label2.TabIndex = 1;
      this.label2.Text = "Output path:";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // textOutputPath
      // 
      this.textOutputPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.textOutputPath.Location = new System.Drawing.Point(22, 42);
      this.textOutputPath.Name = "textOutputPath";
      this.textOutputPath.Size = new System.Drawing.Size(390, 22);
      this.textOutputPath.TabIndex = 0;
      // 
      // labelFramework
      // 
      this.labelFramework.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
      this.labelFramework.AutoSize = true;
      this.labelFramework.Enabled = false;
      this.labelFramework.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelFramework.Location = new System.Drawing.Point(6, 70);
      this.labelFramework.Margin = new System.Windows.Forms.Padding(22, 3, 0, 3);
      this.labelFramework.Name = "labelFramework";
      this.labelFramework.Size = new System.Drawing.Size(69, 15);
      this.labelFramework.TabIndex = 4;
      this.labelFramework.Text = "Framework:";
      this.labelFramework.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // tabPage2
      // 
      this.tabPage2.AutoScroll = true;
      this.tabPage2.Location = new System.Drawing.Point(4, 4);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage2.Size = new System.Drawing.Size(478, 215);
      this.tabPage2.TabIndex = 2;
      this.tabPage2.Text = "Debug";
      this.tabPage2.UseVisualStyleBackColor = true;
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
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(6, 12);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(94, 13);
      this.label3.TabIndex = 18;
      this.label3.Text = "VMWare Edition:";
      // 
      // BuildPage
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.TabControl1);
      this.Name = "BuildPage";
      this.Title = "Build";
      this.Controls.SetChildIndex(this.TabControl1, 0);
      this.TabControl1.ResumeLayout(false);
      this.tabDeploy.ResumeLayout(false);
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      this.tabCompile.ResumeLayout(false);
      this.panel2.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.tabVMWare.ResumeLayout(false);
      this.tabVMWare.PerformLayout();
      this.ResumeLayout(false);

		}

		#endregion

    private System.Windows.Forms.TabControl TabControl1;
    private System.Windows.Forms.TabPage tabCompile;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Button buttonOutputBrowse;
    private System.Windows.Forms.TextBox textOutputPath;
    private System.Windows.Forms.Label labelFramework;
    private System.Windows.Forms.TabPage tabPage2;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.ComboBox comboFramework;
    private System.Windows.Forms.CheckBox checkUseInternalAssembler;
    private System.Windows.Forms.Label labelInternalAssembler;
    private System.Windows.Forms.TabPage tabDeploy;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.TabPage tabVMWare;
    private System.Windows.Forms.ComboBox comboFlavor;
    private System.Windows.Forms.ListBox lboxDeployTarget;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label3;



  }
}
