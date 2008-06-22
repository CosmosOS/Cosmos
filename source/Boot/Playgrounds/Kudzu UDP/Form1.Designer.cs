namespace WindowsFormsApplication1
{
    partial class Form1
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
            this.button1 = new System.Windows.Forms.Button();
            this.textResults = new System.Windows.Forms.TextBox();
            this.butnClear = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(141, 220);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(139, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Send Self Test Packet";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // textResults
            // 
            this.textResults.Location = new System.Drawing.Point(12, 12);
            this.textResults.Multiline = true;
            this.textResults.Name = "textResults";
            this.textResults.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textResults.Size = new System.Drawing.Size(268, 202);
            this.textResults.TabIndex = 1;
            // 
            // butnClear
            // 
            this.butnClear.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butnClear.Location = new System.Drawing.Point(13, 221);
            this.butnClear.Name = "butnClear";
            this.butnClear.Size = new System.Drawing.Size(75, 23);
            this.butnClear.TabIndex = 2;
            this.butnClear.Text = "Clear";
            this.butnClear.UseVisualStyleBackColor = true;
            this.butnClear.Click += new System.EventHandler(this.butnClear_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.butnClear;
            this.ClientSize = new System.Drawing.Size(292, 250);
            this.Controls.Add(this.butnClear);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textResults);
            this.Name = "Form1";
            this.Text = "Cosmos UDP Listener";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textResults;
        private System.Windows.Forms.Button butnClear;
    }
}

