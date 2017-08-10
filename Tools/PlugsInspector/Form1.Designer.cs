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
            this.CosmosPlugClassesListBox = new System.Windows.Forms.ListBox();
            this.ExceptionsListBox = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.NetPlugClassesListBox = new System.Windows.Forms.ListBox();
            this.PluggedMethodsListBox = new System.Windows.Forms.ListBox();
            this.UnPluggedMethodsListBox = new System.Windows.Forms.ListBox();
            this.RunWithNullParamsButton = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.RunResultBox = new System.Windows.Forms.TextBox();
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
            // CosmosPlugClassesListBox
            // 
            this.CosmosPlugClassesListBox.FormattingEnabled = true;
            this.CosmosPlugClassesListBox.HorizontalScrollbar = true;
            this.CosmosPlugClassesListBox.Location = new System.Drawing.Point(355, 58);
            this.CosmosPlugClassesListBox.Name = "CosmosPlugClassesListBox";
            this.CosmosPlugClassesListBox.Size = new System.Drawing.Size(417, 498);
            this.CosmosPlugClassesListBox.Sorted = true;
            this.CosmosPlugClassesListBox.TabIndex = 1;
            // 
            // ExceptionsListBox
            // 
            this.ExceptionsListBox.FormattingEnabled = true;
            this.ExceptionsListBox.HorizontalScrollbar = true;
            this.ExceptionsListBox.Location = new System.Drawing.Point(781, 500);
            this.ExceptionsListBox.Name = "ExceptionsListBox";
            this.ExceptionsListBox.Size = new System.Drawing.Size(385, 56);
            this.ExceptionsListBox.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 43);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(108, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = ".Net Classes Plugged";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(352, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(107, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Cosmos Plug Classes";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(778, 43);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(90, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Plugged Methods";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(778, 259);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(103, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Unplugged Methods";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(778, 484);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(86, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "Load Exceptions";
            // 
            // NetPlugClassesListBox
            // 
            this.NetPlugClassesListBox.FormattingEnabled = true;
            this.NetPlugClassesListBox.HorizontalScrollbar = true;
            this.NetPlugClassesListBox.Location = new System.Drawing.Point(16, 59);
            this.NetPlugClassesListBox.Name = "NetPlugClassesListBox";
            this.NetPlugClassesListBox.Size = new System.Drawing.Size(333, 498);
            this.NetPlugClassesListBox.Sorted = true;
            this.NetPlugClassesListBox.TabIndex = 8;
            this.NetPlugClassesListBox.SelectedIndexChanged += new System.EventHandler(this.NetPlugCalssesListBox_SelectedIndexChanged);
            // 
            // PluggedMethodsListBox
            // 
            this.PluggedMethodsListBox.FormattingEnabled = true;
            this.PluggedMethodsListBox.HorizontalScrollbar = true;
            this.PluggedMethodsListBox.Location = new System.Drawing.Point(781, 59);
            this.PluggedMethodsListBox.Name = "PluggedMethodsListBox";
            this.PluggedMethodsListBox.Size = new System.Drawing.Size(385, 95);
            this.PluggedMethodsListBox.Sorted = true;
            this.PluggedMethodsListBox.TabIndex = 9;
            // 
            // UnPluggedMethodsListBox
            // 
            this.UnPluggedMethodsListBox.FormattingEnabled = true;
            this.UnPluggedMethodsListBox.HorizontalScrollbar = true;
            this.UnPluggedMethodsListBox.Location = new System.Drawing.Point(781, 275);
            this.UnPluggedMethodsListBox.Name = "UnPluggedMethodsListBox";
            this.UnPluggedMethodsListBox.Size = new System.Drawing.Size(385, 186);
            this.UnPluggedMethodsListBox.Sorted = true;
            this.UnPluggedMethodsListBox.TabIndex = 10;
            // 
            // RunWithNullParamsButton
            // 
            this.RunWithNullParamsButton.Location = new System.Drawing.Point(781, 161);
            this.RunWithNullParamsButton.Name = "RunWithNullParamsButton";
            this.RunWithNullParamsButton.Size = new System.Drawing.Size(128, 23);
            this.RunWithNullParamsButton.TabIndex = 11;
            this.RunWithNullParamsButton.Text = "Run with null params";
            this.RunWithNullParamsButton.UseVisualStyleBackColor = true;
            this.RunWithNullParamsButton.Click += new System.EventHandler(this.RunWithNullParamsButton_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(945, 171);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(61, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "Run result :";
            // 
            // RunResultBox
            // 
            this.RunResultBox.Location = new System.Drawing.Point(781, 191);
            this.RunResultBox.Multiline = true;
            this.RunResultBox.Name = "RunResultBox";
            this.RunResultBox.Size = new System.Drawing.Size(385, 54);
            this.RunResultBox.TabIndex = 13;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1178, 568);
            this.Controls.Add(this.RunResultBox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.RunWithNullParamsButton);
            this.Controls.Add(this.UnPluggedMethodsListBox);
            this.Controls.Add(this.PluggedMethodsListBox);
            this.Controls.Add(this.NetPlugClassesListBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ExceptionsListBox);
            this.Controls.Add(this.CosmosPlugClassesListBox);
            this.Controls.Add(this.LoadPlugsButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Plugs Inspector";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button LoadPlugsButton;
        private System.Windows.Forms.ListBox CosmosPlugClassesListBox;
        private System.Windows.Forms.ListBox ExceptionsListBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ListBox NetPlugClassesListBox;
        private System.Windows.Forms.ListBox PluggedMethodsListBox;
        private System.Windows.Forms.ListBox UnPluggedMethodsListBox;
        private System.Windows.Forms.Button RunWithNullParamsButton;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox RunResultBox;

    }
}

