namespace Cosmos.Build.StandAlone {
    partial class MainForm {
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
            this.butnBuild = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // butnBuild
            // 
            this.butnBuild.Location = new System.Drawing.Point(359, 12);
            this.butnBuild.Name = "butnBuild";
            this.butnBuild.Size = new System.Drawing.Size(75, 23);
            this.butnBuild.TabIndex = 0;
            this.butnBuild.Text = "Build";
            this.butnBuild.UseVisualStyleBackColor = true;
            this.butnBuild.Click += new System.EventHandler(this.butnBuild_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(464, 402);
            this.Controls.Add(this.butnBuild);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button butnBuild;
    }
}

