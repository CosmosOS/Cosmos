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
            this.tableBuildOptions = new System.Windows.Forms.TableLayoutPanel();
            this.titleCompiler = new Cosmos.VS.Package.TitleDivision();
            this.titleOutput = new Cosmos.VS.Package.TitleDivision();
            this.label2 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.buttonOutputBrowse = new System.Windows.Forms.Button();
            this.textOutputPath = new System.Windows.Forms.TextBox();
            this.labelFramework = new System.Windows.Forms.Label();
            this.comboFramework = new System.Windows.Forms.ComboBox();
            this.labelTarget = new System.Windows.Forms.Label();
            this.comboTarget = new System.Windows.Forms.ComboBox();
            this.checkUseInternalAssembler = new System.Windows.Forms.CheckBox();
            this.labelInternalAssembler = new System.Windows.Forms.Label();
            this.tableBuildOptions.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableBuildOptions
            // 
            this.tableBuildOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tableBuildOptions.ColumnCount = 2;
            this.tableBuildOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableBuildOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableBuildOptions.Controls.Add(this.titleCompiler, 0, 3);
            this.tableBuildOptions.Controls.Add(this.titleOutput, 0, 0);
            this.tableBuildOptions.Controls.Add(this.label2, 0, 1);
            this.tableBuildOptions.Controls.Add(this.panel1, 1, 1);
            this.tableBuildOptions.Controls.Add(this.labelFramework, 0, 4);
            this.tableBuildOptions.Controls.Add(this.comboFramework, 1, 4);
            this.tableBuildOptions.Controls.Add(this.labelTarget, 0, 2);
            this.tableBuildOptions.Controls.Add(this.comboTarget, 1, 2);
            this.tableBuildOptions.Controls.Add(this.checkUseInternalAssembler, 0, 5);
            this.tableBuildOptions.Controls.Add(this.labelInternalAssembler, 0, 6);
            this.tableBuildOptions.Location = new System.Drawing.Point(0, 41);
            this.tableBuildOptions.Margin = new System.Windows.Forms.Padding(0);
            this.tableBuildOptions.Name = "tableBuildOptions";
            this.tableBuildOptions.RowCount = 8;
            this.tableBuildOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableBuildOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableBuildOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableBuildOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableBuildOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableBuildOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableBuildOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableBuildOptions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableBuildOptions.Size = new System.Drawing.Size(492, 247);
            this.tableBuildOptions.TabIndex = 1;
            // 
            // titleCompiler
            // 
            this.titleCompiler.BackColor = System.Drawing.Color.Transparent;
            this.tableBuildOptions.SetColumnSpan(this.titleCompiler, 2);
            this.titleCompiler.Dock = System.Windows.Forms.DockStyle.Top;
            this.titleCompiler.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.titleCompiler.LineColor = System.Drawing.SystemColors.ControlDark;
            this.titleCompiler.Location = new System.Drawing.Point(3, 99);
            this.titleCompiler.Margin = new System.Windows.Forms.Padding(3, 12, 3, 12);
            this.titleCompiler.Name = "titleCompiler";
            this.titleCompiler.Size = new System.Drawing.Size(486, 15);
            this.titleCompiler.TabIndex = 3;
            this.titleCompiler.Title = "Compilation";
            // 
            // titleOutput
            // 
            this.titleOutput.BackColor = System.Drawing.Color.Transparent;
            this.tableBuildOptions.SetColumnSpan(this.titleOutput, 2);
            this.titleOutput.Dock = System.Windows.Forms.DockStyle.Top;
            this.titleOutput.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.titleOutput.LineColor = System.Drawing.SystemColors.ControlDark;
            this.titleOutput.Location = new System.Drawing.Point(3, 3);
            this.titleOutput.Margin = new System.Windows.Forms.Padding(3, 3, 3, 12);
            this.titleOutput.Name = "titleOutput";
            this.titleOutput.Size = new System.Drawing.Size(486, 15);
            this.titleOutput.TabIndex = 0;
            this.titleOutput.Title = "Target";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(22, 33);
            this.label2.Margin = new System.Windows.Forms.Padding(22, 3, 0, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 24);
            this.label2.TabIndex = 1;
            this.label2.Text = "Output path:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.buttonOutputBrowse);
            this.panel1.Controls.Add(this.textOutputPath);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(147, 30);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(345, 30);
            this.panel1.TabIndex = 2;
            // 
            // buttonOutputBrowse
            // 
            this.buttonOutputBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOutputBrowse.Location = new System.Drawing.Point(321, 4);
            this.buttonOutputBrowse.Name = "buttonOutputBrowse";
            this.buttonOutputBrowse.Size = new System.Drawing.Size(21, 23);
            this.buttonOutputBrowse.TabIndex = 1;
            this.buttonOutputBrowse.Text = "..";
            this.buttonOutputBrowse.UseVisualStyleBackColor = true;
            this.buttonOutputBrowse.Click += new System.EventHandler(this.OutputBrowse_Click);
            // 
            // textOutputPath
            // 
            this.textOutputPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textOutputPath.Location = new System.Drawing.Point(4, 4);
            this.textOutputPath.Name = "textOutputPath";
            this.textOutputPath.Size = new System.Drawing.Size(311, 22);
            this.textOutputPath.TabIndex = 0;
            // 
            // labelFramework
            // 
            this.labelFramework.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.labelFramework.AutoSize = true;
            this.labelFramework.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelFramework.Location = new System.Drawing.Point(22, 129);
            this.labelFramework.Margin = new System.Windows.Forms.Padding(22, 3, 0, 3);
            this.labelFramework.Name = "labelFramework";
            this.labelFramework.Size = new System.Drawing.Size(69, 21);
            this.labelFramework.TabIndex = 4;
            this.labelFramework.Text = "Framework:";
            this.labelFramework.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // comboFramework
            // 
            this.comboFramework.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboFramework.Enabled = false;
            this.comboFramework.FormattingEnabled = true;
            this.comboFramework.Location = new System.Drawing.Point(150, 129);
            this.comboFramework.Name = "comboFramework";
            this.comboFramework.Size = new System.Drawing.Size(143, 21);
            this.comboFramework.TabIndex = 8;
            // 
            // labelTarget
            // 
            this.labelTarget.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.labelTarget.AutoSize = true;
            this.labelTarget.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTarget.Location = new System.Drawing.Point(22, 63);
            this.labelTarget.Margin = new System.Windows.Forms.Padding(22, 3, 0, 3);
            this.labelTarget.Name = "labelTarget";
            this.labelTarget.Size = new System.Drawing.Size(78, 21);
            this.labelTarget.TabIndex = 11;
            this.labelTarget.Text = "Environment:";
            this.labelTarget.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // comboTarget
            // 
            this.comboTarget.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboTarget.FormattingEnabled = true;
            this.comboTarget.Location = new System.Drawing.Point(150, 63);
            this.comboTarget.Name = "comboTarget";
            this.comboTarget.Size = new System.Drawing.Size(143, 21);
            this.comboTarget.TabIndex = 12;
            this.comboTarget.SelectedIndexChanged += new System.EventHandler(this.comboTarget_SelectedIndexChanged);
            // 
            // checkUseInternalAssembler
            // 
            this.checkUseInternalAssembler.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.checkUseInternalAssembler.AutoSize = true;
            this.tableBuildOptions.SetColumnSpan(this.checkUseInternalAssembler, 2);
            this.checkUseInternalAssembler.Enabled = false;
            this.checkUseInternalAssembler.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkUseInternalAssembler.Location = new System.Drawing.Point(22, 165);
            this.checkUseInternalAssembler.Margin = new System.Windows.Forms.Padding(22, 12, 3, 3);
            this.checkUseInternalAssembler.Name = "checkUseInternalAssembler";
            this.checkUseInternalAssembler.Size = new System.Drawing.Size(146, 19);
            this.checkUseInternalAssembler.TabIndex = 10;
            this.checkUseInternalAssembler.Text = "Use Internal Assembler";
            this.checkUseInternalAssembler.UseVisualStyleBackColor = true;
            // 
            // labelInternalAssembler
            // 
            this.tableBuildOptions.SetColumnSpan(this.labelInternalAssembler, 2);
            this.labelInternalAssembler.Location = new System.Drawing.Point(44, 187);
            this.labelInternalAssembler.Margin = new System.Windows.Forms.Padding(44, 0, 3, 0);
            this.labelInternalAssembler.Name = "labelInternalAssembler";
            this.labelInternalAssembler.Size = new System.Drawing.Size(401, 43);
            this.labelInternalAssembler.TabIndex = 6;
            this.labelInternalAssembler.Text = "Experimental. Leave unchecked unless you like debugging deep level assembly code." +
                "";
            // 
            // BuildPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableBuildOptions);
            this.Name = "BuildPage";
            this.Title = "Build";
            this.Controls.SetChildIndex(this.tableBuildOptions, 0);
            this.tableBuildOptions.ResumeLayout(false);
            this.tableBuildOptions.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableBuildOptions;
		private TitleDivision titleOutput;
		private TitleDivision titleCompiler;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button buttonOutputBrowse;
		private System.Windows.Forms.TextBox textOutputPath;
		private System.Windows.Forms.Label labelFramework;
		private System.Windows.Forms.Label labelInternalAssembler;
		private System.Windows.Forms.ComboBox comboFramework;
		private System.Windows.Forms.CheckBox checkUseInternalAssembler;
		private System.Windows.Forms.Label labelTarget;
		private System.Windows.Forms.ComboBox comboTarget;


	}
}
