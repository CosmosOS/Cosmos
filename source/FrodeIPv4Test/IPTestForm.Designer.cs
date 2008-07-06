namespace FrodeIPv4Test
{
    partial class IPTestForm
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
            this.destinationTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.bytesTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.sourceTextBox = new System.Windows.Forms.TextBox();
            this.identificationTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.fragmentOffsetTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.dataTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // destinationTextBox
            // 
            this.destinationTextBox.Location = new System.Drawing.Point(12, 204);
            this.destinationTextBox.Name = "destinationTextBox";
            this.destinationTextBox.Size = new System.Drawing.Size(165, 20);
            this.destinationTextBox.TabIndex = 0;
            this.destinationTextBox.Text = "172.28.6.1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 185);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Destination";
            // 
            // bytesTextBox
            // 
            this.bytesTextBox.Location = new System.Drawing.Point(315, 47);
            this.bytesTextBox.Multiline = true;
            this.bytesTextBox.Name = "bytesTextBox";
            this.bytesTextBox.Size = new System.Drawing.Size(474, 406);
            this.bytesTextBox.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(312, 31);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "As bytes";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(15, 383);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(274, 70);
            this.button1.TabIndex = 4;
            this.button1.Text = "Convert using Cosmos implementation";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 143);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Source";
            // 
            // sourceTextBox
            // 
            this.sourceTextBox.Location = new System.Drawing.Point(12, 162);
            this.sourceTextBox.Name = "sourceTextBox";
            this.sourceTextBox.Size = new System.Drawing.Size(165, 20);
            this.sourceTextBox.TabIndex = 5;
            this.sourceTextBox.Text = "172.28.6.6";
            // 
            // identificationTextBox
            // 
            this.identificationTextBox.Location = new System.Drawing.Point(12, 89);
            this.identificationTextBox.Name = "identificationTextBox";
            this.identificationTextBox.Size = new System.Drawing.Size(162, 20);
            this.identificationTextBox.TabIndex = 7;
            this.identificationTextBox.Text = "3";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 70);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(67, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Identification";
            // 
            // fragmentOffsetTextBox
            // 
            this.fragmentOffsetTextBox.Location = new System.Drawing.Point(12, 47);
            this.fragmentOffsetTextBox.Name = "fragmentOffsetTextBox";
            this.fragmentOffsetTextBox.Size = new System.Drawing.Size(162, 20);
            this.fragmentOffsetTextBox.TabIndex = 7;
            this.fragmentOffsetTextBox.Text = "1";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 28);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(82, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Fragment Offset";
            // 
            // dataTextBox
            // 
            this.dataTextBox.Location = new System.Drawing.Point(12, 283);
            this.dataTextBox.Name = "dataTextBox";
            this.dataTextBox.Size = new System.Drawing.Size(162, 20);
            this.dataTextBox.TabIndex = 7;
            this.dataTextBox.Text = "1";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 264);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(30, 13);
            this.label6.TabIndex = 8;
            this.label6.Text = "Data";
            // 
            // IPTestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(889, 465);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.dataTextBox);
            this.Controls.Add(this.fragmentOffsetTextBox);
            this.Controls.Add(this.identificationTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.sourceTextBox);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.bytesTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.destinationTextBox);
            this.Name = "IPTestForm";
            this.Text = "Testproject for converting IPv4 packet to bytes";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox destinationTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox bytesTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox sourceTextBox;
        private System.Windows.Forms.TextBox identificationTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox fragmentOffsetTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox dataTextBox;
        private System.Windows.Forms.Label label6;
    }
}

