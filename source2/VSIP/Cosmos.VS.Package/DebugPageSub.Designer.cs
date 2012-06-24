namespace Cosmos.VS.Package
{
	partial class DebugPageSub
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
      this.checkStartCosmosGDB = new System.Windows.Forms.CheckBox();
      this.checkEnableGDB = new System.Windows.Forms.CheckBox();
      this.comboTraceMode = new System.Windows.Forms.ComboBox();
      this.comboDebugMode = new System.Windows.Forms.ComboBox();
      this.label2 = new System.Windows.Forms.Label();
      this.checkIgnoreDebugStubAttribute = new System.Windows.Forms.CheckBox();
      this.panel1 = new System.Windows.Forms.Panel();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.label1 = new System.Windows.Forms.Label();
      this.panel1.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
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
      // comboTraceMode
      // 
      this.comboTraceMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboTraceMode.FormattingEnabled = true;
      this.comboTraceMode.Location = new System.Drawing.Point(36, 96);
      this.comboTraceMode.Name = "comboTraceMode";
      this.comboTraceMode.Size = new System.Drawing.Size(221, 21);
      this.comboTraceMode.TabIndex = 15;
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
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label2.Location = new System.Drawing.Point(18, 75);
      this.label2.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(50, 15);
      this.label2.TabIndex = 18;
      this.label2.Text = "Tracing:";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
      // panel1
      // 
      this.panel1.Controls.Add(this.groupBox2);
      this.panel1.Controls.Add(this.groupBox1);
      this.panel1.Location = new System.Drawing.Point(3, 17);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(277, 247);
      this.panel1.TabIndex = 1;
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.checkIgnoreDebugStubAttribute);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.comboDebugMode);
      this.groupBox1.Controls.Add(this.comboTraceMode);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(277, 163);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Visual Studio";
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.checkEnableGDB);
      this.groupBox2.Controls.Add(this.checkStartCosmosGDB);
      this.groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox2.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.groupBox2.Location = new System.Drawing.Point(0, 163);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(277, 79);
      this.groupBox2.TabIndex = 1;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Assembly";
      // 
      // label1
      // 
      this.label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.Location = new System.Drawing.Point(18, 21);
      this.label1.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(98, 21);
      this.label1.TabIndex = 15;
      this.label1.Text = "Debug Level:";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // DebugPageSub
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.panel1);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.Name = "DebugPageSub";
      this.Size = new System.Drawing.Size(353, 325);
      this.panel1.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.ResumeLayout(false);

		}

		#endregion

        private System.Windows.Forms.CheckBox checkStartCosmosGDB;
        private System.Windows.Forms.CheckBox checkEnableGDB;
        private System.Windows.Forms.ComboBox comboTraceMode;
        private System.Windows.Forms.ComboBox comboDebugMode;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox checkIgnoreDebugStubAttribute;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;

    }
}
