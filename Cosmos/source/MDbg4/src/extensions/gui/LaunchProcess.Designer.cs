namespace gui
{
    partial class LaunchProcess
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
            this.textBoxProcessName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxArgs = new System.Windows.Forms.TextBox();
            this.buttonLaunch = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxWorkingDir = new System.Windows.Forms.TextBox();
            this.buttonOpenProcess = new System.Windows.Forms.Button();
            this.SuspendLayout();
// 
// textBoxProcessName
// 
            this.textBoxProcessName.AutoSize = false;
            this.textBoxProcessName.Location = new System.Drawing.Point(27, 37);
            this.textBoxProcessName.Multiline = true;
            this.textBoxProcessName.Name = "textBoxProcessName";
            this.textBoxProcessName.Size = new System.Drawing.Size(347, 18);
            this.textBoxProcessName.TabIndex = 0;
// 
// label1
// 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(27, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(308, 14);
            this.label1.TabIndex = 1;
            this.label1.Text = "Please enter the process name to start managed-debugging:";
// 
// label2
// 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(24, 74);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(305, 14);
            this.label2.TabIndex = 2;
            this.label2.Text = "Please enter the command line arguments for the debuggee";
// 
// textBoxArgs
// 
            this.textBoxArgs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxArgs.AutoSize = false;
            this.textBoxArgs.Location = new System.Drawing.Point(27, 98);
            this.textBoxArgs.Multiline = true;
            this.textBoxArgs.Name = "textBoxArgs";
            this.textBoxArgs.Size = new System.Drawing.Size(380, 21);
            this.textBoxArgs.TabIndex = 3;
// 
// buttonLaunch
// 
            this.buttonLaunch.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonLaunch.Location = new System.Drawing.Point(340, 206);
            this.buttonLaunch.Name = "buttonLaunch";
            this.buttonLaunch.Size = new System.Drawing.Size(67, 23);
            this.buttonLaunch.TabIndex = 4;
            this.buttonLaunch.Text = "Launch!";
            this.buttonLaunch.Click += new System.EventHandler(this.buttonLaunch_Click);
// 
// label3
// 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(29, 144);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(294, 14);
            this.label3.TabIndex = 5;
            this.label3.Text = "Please set the current working directory for the debuggee:";
// 
// textBoxWorkingDir
// 
            this.textBoxWorkingDir.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxWorkingDir.Location = new System.Drawing.Point(29, 167);
            this.textBoxWorkingDir.Name = "textBoxWorkingDir";
            this.textBoxWorkingDir.Size = new System.Drawing.Size(378, 20);
            this.textBoxWorkingDir.TabIndex = 6;
// 
// buttonOpenProcess
// 
            this.buttonOpenProcess.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOpenProcess.Location = new System.Drawing.Point(380, 36);
            this.buttonOpenProcess.Name = "buttonOpenProcess";
            this.buttonOpenProcess.Size = new System.Drawing.Size(26, 18);
            this.buttonOpenProcess.TabIndex = 7;
            this.buttonOpenProcess.Text = "...";
            this.buttonOpenProcess.Click += new System.EventHandler(this.buttonOpenProcess_Click);
// 
// LaunchProcess
// 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(417, 242);
            this.Controls.Add(this.buttonOpenProcess);
            this.Controls.Add(this.textBoxWorkingDir);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.buttonLaunch);
            this.Controls.Add(this.textBoxArgs);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxProcessName);
            this.Name = "LaunchProcess";
            this.Text = "LaunchProcess";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxProcessName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxArgs;
        private System.Windows.Forms.Button buttonLaunch;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxWorkingDir;
        private System.Windows.Forms.Button buttonOpenProcess;
    }
}