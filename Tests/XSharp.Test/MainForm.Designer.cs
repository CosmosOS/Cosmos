namespace XSharp.Test
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
      this.tabsMain = new System.Windows.Forms.TabControl();
      this.SuspendLayout();
      // 
      // tabsMain
      // 
      this.tabsMain.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tabsMain.Location = new System.Drawing.Point(0, 0);
      this.tabsMain.Name = "tabsMain";
      this.tabsMain.SelectedIndex = 0;
      this.tabsMain.Size = new System.Drawing.Size(1218, 718);
      this.tabsMain.TabIndex = 0;
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1218, 718);
      this.Controls.Add(this.tabsMain);
      this.Name = "MainForm";
      this.Text = "X# Test App";
      this.Load += new System.EventHandler(this.MainForm_Load);
      this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabsMain;

    }
}

