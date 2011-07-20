namespace TTF2OPFF_Converter
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
            this.label1 = new System.Windows.Forms.Label();
            this.FontComboBox = new System.Windows.Forms.ComboBox();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.BrowseButton = new System.Windows.Forms.Button();
            this.ConvertButton = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(80, 119);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(31, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Font:";
            // 
            // FontComboBox
            // 
            this.FontComboBox.FormattingEnabled = true;
            this.FontComboBox.Location = new System.Drawing.Point(117, 116);
            this.FontComboBox.Name = "FontComboBox";
            this.FontComboBox.Size = new System.Drawing.Size(200, 21);
            this.FontComboBox.Sorted = true;
            this.FontComboBox.TabIndex = 1;
            this.FontComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.FontComboBox_KeyPress);
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.Filter = "Orvid Pre-Compiled Font File (*.opff)|*.opff";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(50, 150);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Output File:";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(117, 147);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(200, 20);
            this.textBox1.TabIndex = 3;
            // 
            // BrowseButton
            // 
            this.BrowseButton.Location = new System.Drawing.Point(323, 144);
            this.BrowseButton.Name = "BrowseButton";
            this.BrowseButton.Size = new System.Drawing.Size(75, 23);
            this.BrowseButton.TabIndex = 4;
            this.BrowseButton.Text = "Browse";
            this.BrowseButton.UseVisualStyleBackColor = true;
            this.BrowseButton.Click += new System.EventHandler(this.BrowseButton_Click);
            // 
            // ConvertButton
            // 
            this.ConvertButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ConvertButton.Location = new System.Drawing.Point(181, 268);
            this.ConvertButton.Name = "ConvertButton";
            this.ConvertButton.Size = new System.Drawing.Size(87, 31);
            this.ConvertButton.TabIndex = 5;
            this.ConvertButton.Text = "Convert!";
            this.ConvertButton.UseVisualStyleBackColor = true;
            this.ConvertButton.Click += new System.EventHandler(this.ConvertButton_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(344, 106);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(32, 32);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 6;
            this.pictureBox1.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(449, 363);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.ConvertButton);
            this.Controls.Add(this.BrowseButton);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.FontComboBox);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Orvid\'s TTF to OPFF Converter";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox FontComboBox;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button BrowseButton;
        private System.Windows.Forms.Button ConvertButton;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}

