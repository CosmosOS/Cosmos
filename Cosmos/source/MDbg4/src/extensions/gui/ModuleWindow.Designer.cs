namespace gui
{
    partial class ModuleWindow
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
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader("");
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader("");
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader("");
            this.columnHeader4 = new System.Windows.Forms.ColumnHeader("");
            this.SuspendLayout();
// 
// listView1
// 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader2,
            this.columnHeader1,
            this.columnHeader4,
            this.columnHeader3});
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.FullRowSelect = true;
            this.listView1.Location = new System.Drawing.Point(0, 0);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(421, 258);
            this.listView1.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.listView1.TabIndex = 0;
            this.listView1.View = System.Windows.Forms.View.Details;
// 
// columnHeader1
// 
            this.columnHeader1.Text = "Module Name";
// 
// columnHeader2
// 
            this.columnHeader2.Text = "ID";
            this.columnHeader2.Width = 30;
// 
// columnHeader3
// 
            this.columnHeader3.Text = "Flags";
// 
// columnHeader4
// 
            this.columnHeader4.Text = "Path";
            this.columnHeader4.Width = 200;
// 
// ModuleWindow
// 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(421, 258);
            this.Controls.Add(this.listView1);
            this.Name = "ModuleWindow";
            this.Text = "ModuleWindow";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;

    }
}