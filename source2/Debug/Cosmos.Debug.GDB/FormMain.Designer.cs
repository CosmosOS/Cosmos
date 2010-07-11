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
            this.lboxDebug = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // butnConnect
            // 
            this.butnConnect.Location = new System.Drawing.Point(12, 12);
            this.butnConnect.Name = "butnConnect";
            this.butnConnect.Size = new System.Drawing.Size(75, 23);
            this.butnConnect.TabIndex = 0;
            this.butnConnect.Text = "Connect";
            this.butnConnect.UseVisualStyleBackColor = true;
            this.butnConnect.Click += new System.EventHandler(this.butnConnect_Click);
            // 
            // lboxDebug
            // 
            this.lboxDebug.FormattingEnabled = true;
            this.lboxDebug.Location = new System.Drawing.Point(393, 56);
            this.lboxDebug.Name = "lboxDebug";
            this.lboxDebug.Size = new System.Drawing.Size(221, 394);
            this.lboxDebug.TabIndex = 1;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(650, 475);
            this.Controls.Add(this.lboxDebug);
            this.Controls.Add(this.butnConnect);
            this.Name = "FormMain";
            this.Text = "Cosmos GDB Debugger";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button butnConnect;
        private System.Windows.Forms.ListBox lboxDebug;
    }
}

