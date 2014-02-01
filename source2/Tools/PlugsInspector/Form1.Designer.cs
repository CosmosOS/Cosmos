namespace PlugsInspector
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
            this.LoadPlugsButton = new System.Windows.Forms.Button();
            this.PlugsListBox = new System.Windows.Forms.ListBox();
            this.ExceptionsListBox = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // LoadPlugsButton
            // 
            this.LoadPlugsButton.Location = new System.Drawing.Point(13, 13);
            this.LoadPlugsButton.Name = "LoadPlugsButton";
            this.LoadPlugsButton.Size = new System.Drawing.Size(75, 23);
            this.LoadPlugsButton.TabIndex = 0;
            this.LoadPlugsButton.Text = "Load Plugs";
            this.LoadPlugsButton.UseVisualStyleBackColor = true;
            this.LoadPlugsButton.Click += new System.EventHandler(this.LoadPlugsButton_Click);
            // 
            // PlugsListBox
            // 
            this.PlugsListBox.FormattingEnabled = true;
            this.PlugsListBox.Location = new System.Drawing.Point(13, 42);
            this.PlugsListBox.Name = "PlugsListBox";
            this.PlugsListBox.Size = new System.Drawing.Size(642, 511);
            this.PlugsListBox.Sorted = true;
            this.PlugsListBox.TabIndex = 1;
            // 
            // ExceptionsListBox
            // 
            this.ExceptionsListBox.FormattingEnabled = true;
            this.ExceptionsListBox.Location = new System.Drawing.Point(661, 42);
            this.ExceptionsListBox.Name = "ExceptionsListBox";
            this.ExceptionsListBox.Size = new System.Drawing.Size(505, 511);
            this.ExceptionsListBox.TabIndex = 2;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1178, 568);
            this.Controls.Add(this.ExceptionsListBox);
            this.Controls.Add(this.PlugsListBox);
            this.Controls.Add(this.LoadPlugsButton);
            this.Name = "Form1";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Plugs Inspector";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button LoadPlugsButton;
        private System.Windows.Forms.ListBox PlugsListBox;
        private System.Windows.Forms.ListBox ExceptionsListBox;

    }
}

