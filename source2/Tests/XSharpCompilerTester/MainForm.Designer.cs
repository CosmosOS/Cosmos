namespace XSharpCompilerTester
{
    partial class MainForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
      this.components = new System.ComponentModel.Container();
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.tboxInput = new System.Windows.Forms.TextBox();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.textOutput = new System.Windows.Forms.TextBox();
      this.timerConvert = new System.Windows.Forms.Timer(this.components);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // splitContainer1
      // 
      this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer1.Location = new System.Drawing.Point(0, 0);
      this.splitContainer1.Name = "splitContainer1";
      this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitContainer1.Panel1
      // 
      this.splitContainer1.Panel1.Controls.Add(this.groupBox1);
      // 
      // splitContainer1.Panel2
      // 
      this.splitContainer1.Panel2.Controls.Add(this.groupBox2);
      this.splitContainer1.Size = new System.Drawing.Size(1218, 718);
      this.splitContainer1.SplitterDistance = 224;
      this.splitContainer1.TabIndex = 0;
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.tboxInput);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(1218, 224);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Input";
      // 
      // tboxInput
      // 
      this.tboxInput.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tboxInput.Location = new System.Drawing.Point(3, 16);
      this.tboxInput.Multiline = true;
      this.tboxInput.Name = "tboxInput";
      this.tboxInput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.tboxInput.Size = new System.Drawing.Size(1212, 205);
      this.tboxInput.TabIndex = 0;
      this.tboxInput.TextChanged += new System.EventHandler(this.textInput_TextChanged);
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.textOutput);
      this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox2.Location = new System.Drawing.Point(0, 0);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(1218, 490);
      this.groupBox2.TabIndex = 0;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Output";
      // 
      // textOutput
      // 
      this.textOutput.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textOutput.Location = new System.Drawing.Point(3, 16);
      this.textOutput.Multiline = true;
      this.textOutput.Name = "textOutput";
      this.textOutput.ReadOnly = true;
      this.textOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.textOutput.Size = new System.Drawing.Size(1212, 471);
      this.textOutput.TabIndex = 0;
      // 
      // timerConvert
      // 
      this.timerConvert.Enabled = true;
      this.timerConvert.Interval = 350;
      this.timerConvert.Tick += new System.EventHandler(this.timerConvert_Tick);
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1218, 718);
      this.Controls.Add(this.splitContainer1);
      this.Name = "MainForm";
      this.Text = "Form1";
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
      this.splitContainer1.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox tboxInput;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox textOutput;
        private System.Windows.Forms.Timer timerConvert;
    }
}

