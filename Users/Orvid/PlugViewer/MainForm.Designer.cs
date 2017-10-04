namespace PlugViewer
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.Rtb = new System.Windows.Forms.RichTextBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.treeView1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.Rtb);
            this.splitContainer1.Size = new System.Drawing.Size(627, 478);
            this.splitContainer1.SplitterDistance = 209;
            this.splitContainer1.TabIndex = 1;
            this.splitContainer1.TabStop = false;
            // 
            // treeView1
            // 
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView1.ImageIndex = 0;
            this.treeView1.ImageList = this.imageList1;
            this.treeView1.Location = new System.Drawing.Point(0, 0);
            this.treeView1.Name = "treeView1";
            this.treeView1.SelectedImageIndex = 0;
            this.treeView1.ShowNodeToolTips = true;
            this.treeView1.Size = new System.Drawing.Size(209, 478);
            this.treeView1.TabIndex = 0;
            this.treeView1.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeView1_NodeMouseClick);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Fuchsia;
            this.imageList1.Images.SetKeyName(0, "Assembly.bmp");
            this.imageList1.Images.SetKeyName(1, "BrokenReference.bmp");
            this.imageList1.Images.SetKeyName(2, "Class.bmp");
            this.imageList1.Images.SetKeyName(3, "Class_Internal.bmp");
            this.imageList1.Images.SetKeyName(4, "Class_Private.bmp");
            this.imageList1.Images.SetKeyName(5, "Class_Protected.bmp");
            this.imageList1.Images.SetKeyName(6, "Constant.bmp");
            this.imageList1.Images.SetKeyName(7, "Delegate.bmp");
            this.imageList1.Images.SetKeyName(8, "Delegate_Friend.bmp");
            this.imageList1.Images.SetKeyName(9, "Delegate_Private.bmp");
            this.imageList1.Images.SetKeyName(10, "Delegate_Protected.bmp");
            this.imageList1.Images.SetKeyName(11, "Enum.bmp");
            this.imageList1.Images.SetKeyName(12, "Enum_Internal.bmp");
            this.imageList1.Images.SetKeyName(13, "Enum_Private.bmp");
            this.imageList1.Images.SetKeyName(14, "Enum_Protected.bmp");
            this.imageList1.Images.SetKeyName(15, "EnumItem.bmp");
            this.imageList1.Images.SetKeyName(16, "EnumItem_Internal.bmp");
            this.imageList1.Images.SetKeyName(17, "EnumItem_Private.bmp");
            this.imageList1.Images.SetKeyName(18, "EnumItem_Protected.bmp");
            this.imageList1.Images.SetKeyName(19, "Error.bmp");
            this.imageList1.Images.SetKeyName(20, "Event.bmp");
            this.imageList1.Images.SetKeyName(21, "Exception.bmp");
            this.imageList1.Images.SetKeyName(22, "Exception_Internal.bmp");
            this.imageList1.Images.SetKeyName(23, "Exception_Private.bmp");
            this.imageList1.Images.SetKeyName(24, "Exception_Protected.bmp");
            this.imageList1.Images.SetKeyName(25, "Field.bmp");
            this.imageList1.Images.SetKeyName(26, "Field_Internal.bmp");
            this.imageList1.Images.SetKeyName(27, "Field_Private.bmp");
            this.imageList1.Images.SetKeyName(28, "Field_Protected.bmp");
            this.imageList1.Images.SetKeyName(29, "Interface.bmp");
            this.imageList1.Images.SetKeyName(30, "Interface_Internal.bmp");
            this.imageList1.Images.SetKeyName(31, "Interface_Private.bmp");
            this.imageList1.Images.SetKeyName(32, "Interface_Protected.bmp");
            this.imageList1.Images.SetKeyName(33, "InterfaceImpl.bmp");
            this.imageList1.Images.SetKeyName(34, "InterfaceImpl_Internal.bmp");
            this.imageList1.Images.SetKeyName(35, "InterfaceImpl_Private.bmp");
            this.imageList1.Images.SetKeyName(36, "InterfaceImpl_Protected.bmp");
            this.imageList1.Images.SetKeyName(37, "Library.bmp");
            this.imageList1.Images.SetKeyName(38, "Method.bmp");
            this.imageList1.Images.SetKeyName(39, "Method_Internal.bmp");
            this.imageList1.Images.SetKeyName(40, "Method_Private.bmp");
            this.imageList1.Images.SetKeyName(41, "Method_Protected.bmp");
            this.imageList1.Images.SetKeyName(42, "MethodOverload.bmp");
            this.imageList1.Images.SetKeyName(43, "MethodOverload_Internal.bmp");
            this.imageList1.Images.SetKeyName(44, "MethodOverload_Private.bmp");
            this.imageList1.Images.SetKeyName(45, "MethodOverload_Protected.bmp");
            this.imageList1.Images.SetKeyName(46, "Namespace.bmp");
            this.imageList1.Images.SetKeyName(47, "Operator.bmp");
            this.imageList1.Images.SetKeyName(48, "Operator_Internal.bmp");
            this.imageList1.Images.SetKeyName(49, "Operator_Private.bmp");
            this.imageList1.Images.SetKeyName(50, "Operator_Protected.bmp");
            this.imageList1.Images.SetKeyName(51, "Plug.png");
            this.imageList1.Images.SetKeyName(52, "Properties.bmp");
            this.imageList1.Images.SetKeyName(53, "Properties_Internal.bmp");
            this.imageList1.Images.SetKeyName(54, "Properties_Private.bmp");
            this.imageList1.Images.SetKeyName(55, "Properties_Protected.bmp");
            this.imageList1.Images.SetKeyName(56, "Properties-ReadOnly.bmp");
            this.imageList1.Images.SetKeyName(57, "Properties-ReadOnly_Internal.bmp");
            this.imageList1.Images.SetKeyName(58, "Properties-ReadOnly_Private.bmp");
            this.imageList1.Images.SetKeyName(59, "Properties-ReadOnly_Protected.bmp");
            this.imageList1.Images.SetKeyName(60, "Structure.bmp");
            this.imageList1.Images.SetKeyName(61, "Structure_Internal.bmp");
            this.imageList1.Images.SetKeyName(62, "Structure_Private.bmp");
            this.imageList1.Images.SetKeyName(63, "Structure_Protected.bmp");
            this.imageList1.Images.SetKeyName(64, "Union.bmp");
            this.imageList1.Images.SetKeyName(65, "Union_Internal.bmp");
            this.imageList1.Images.SetKeyName(66, "Union_Private.bmp");
            this.imageList1.Images.SetKeyName(67, "Union_Protected.bmp");
            this.imageList1.Images.SetKeyName(68, "ValueType.bmp");
            this.imageList1.Images.SetKeyName(69, "ValueType_Internal.bmp");
            this.imageList1.Images.SetKeyName(70, "ValueType_Private.bmp");
            this.imageList1.Images.SetKeyName(71, "ValueType_Protected.bmp");
            this.imageList1.Images.SetKeyName(72, "Warning.bmp");
            // 
            // Rtb
            // 
            this.Rtb.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Rtb.Location = new System.Drawing.Point(0, 0);
            this.Rtb.Name = "Rtb";
            this.Rtb.Size = new System.Drawing.Size(414, 478);
            this.Rtb.TabIndex = 0;
            this.Rtb.Text = "";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = ".Net Dll |*.dll;*.exe";
            this.openFileDialog1.Multiselect = true;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(627, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            this.menuStrip1.Items.Add(fileToolStripMenuItem);
            this.menuStrip1.Items.Add(debugToolStripMenuItem);
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(111, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // debugToolStripMenuItem
            // 
            this.debugToolStripMenuItem.Name = "debugToolStripMenuItem";
            this.debugToolStripMenuItem.Size = new System.Drawing.Size(50, 20);
            this.debugToolStripMenuItem.Text = "Debug";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(627, 502);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "Cosmos Plug Viewer";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugToolStripMenuItem;
        private System.Windows.Forms.RichTextBox Rtb;
    }
}

