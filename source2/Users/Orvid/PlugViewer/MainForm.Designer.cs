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
            this.button1 = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.splitContainer1.Panel2.Controls.Add(this.button1);
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
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Fuchsia;
            this.imageList1.Images.SetKeyName(0, "Assembly.bmp");
            this.imageList1.Images.SetKeyName(1, "BrokenReference.bmp");
            this.imageList1.Images.SetKeyName(2, "BSC.bmp");
            this.imageList1.Images.SetKeyName(3, "Class.bmp");
            this.imageList1.Images.SetKeyName(4, "Class_Internal.bmp");
            this.imageList1.Images.SetKeyName(5, "Class_Private.bmp");
            this.imageList1.Images.SetKeyName(6, "Class_Protected.bmp");
            this.imageList1.Images.SetKeyName(7, "Class_Sealed.bmp");
            this.imageList1.Images.SetKeyName(8, "Class_Shortcut.bmp");
            this.imageList1.Images.SetKeyName(9, "Constant.bmp");
            this.imageList1.Images.SetKeyName(10, "Constant_Internal.bmp");
            this.imageList1.Images.SetKeyName(11, "Constant_Private.bmp");
            this.imageList1.Images.SetKeyName(12, "Constant_Protected.bmp");
            this.imageList1.Images.SetKeyName(13, "Constant_Sealed.bmp");
            this.imageList1.Images.SetKeyName(14, "Constant_Shortcut.bmp");
            this.imageList1.Images.SetKeyName(15, "Delegate.bmp");
            this.imageList1.Images.SetKeyName(16, "Delegate_Friend.bmp");
            this.imageList1.Images.SetKeyName(17, "Delegate_Private.bmp");
            this.imageList1.Images.SetKeyName(18, "Delegate_Protected.bmp");
            this.imageList1.Images.SetKeyName(19, "Delegate_Sealed.bmp");
            this.imageList1.Images.SetKeyName(20, "Delegate_Shortcut.bmp");
            this.imageList1.Images.SetKeyName(21, "DialogID.bmp");
            this.imageList1.Images.SetKeyName(22, "Enum.bmp");
            this.imageList1.Images.SetKeyName(23, "Enum_Internal.bmp");
            this.imageList1.Images.SetKeyName(24, "Enum_Protected.bmp");
            this.imageList1.Images.SetKeyName(25, "Enum_Sealed.bmp");
            this.imageList1.Images.SetKeyName(26, "Enum_Shortcut.bmp");
            this.imageList1.Images.SetKeyName(27, "EnumItem.bmp");
            this.imageList1.Images.SetKeyName(28, "EnumItem_Internal.bmp");
            this.imageList1.Images.SetKeyName(29, "EnumItem_Private.bmp");
            this.imageList1.Images.SetKeyName(30, "EnumItem_Protected.bmp");
            this.imageList1.Images.SetKeyName(31, "EnumItem_Sealed.bmp");
            this.imageList1.Images.SetKeyName(32, "EnumItem_Shortcut.bmp");
            this.imageList1.Images.SetKeyName(33, "EnumPrivate.bmp");
            this.imageList1.Images.SetKeyName(34, "Event.bmp");
            this.imageList1.Images.SetKeyName(35, "Event_Internal.bmp");
            this.imageList1.Images.SetKeyName(36, "Event_Private.bmp");
            this.imageList1.Images.SetKeyName(37, "Event_Protected.bmp");
            this.imageList1.Images.SetKeyName(38, "Event_Sealed.bmp");
            this.imageList1.Images.SetKeyName(39, "Event_Shortcut.bmp");
            this.imageList1.Images.SetKeyName(40, "Exception.bmp");
            this.imageList1.Images.SetKeyName(41, "Exception_Internal.bmp");
            this.imageList1.Images.SetKeyName(42, "Exception_Protected.bmp");
            this.imageList1.Images.SetKeyName(43, "Exception_Sealed.bmp");
            this.imageList1.Images.SetKeyName(44, "Exception_Shortcut.bmp");
            this.imageList1.Images.SetKeyName(45, "ExceptionPrivate.bmp");
            this.imageList1.Images.SetKeyName(46, "Field.bmp");
            this.imageList1.Images.SetKeyName(47, "Field_Internal.bmp");
            this.imageList1.Images.SetKeyName(48, "Field_Private.bmp");
            this.imageList1.Images.SetKeyName(49, "Field_Protected.bmp");
            this.imageList1.Images.SetKeyName(50, "Field_Sealed.bmp");
            this.imageList1.Images.SetKeyName(51, "Field_Shortcut.bmp");
            this.imageList1.Images.SetKeyName(52, "Interface.bmp");
            this.imageList1.Images.SetKeyName(53, "Interface_Internal.bmp");
            this.imageList1.Images.SetKeyName(54, "Interface_Private.bmp");
            this.imageList1.Images.SetKeyName(55, "Interface_Protected.bmp");
            this.imageList1.Images.SetKeyName(56, "Interface_Sealed.bmp");
            this.imageList1.Images.SetKeyName(57, "Interface_Shortcut.bmp");
            this.imageList1.Images.SetKeyName(58, "Library.bmp");
            this.imageList1.Images.SetKeyName(59, "Macro.bmp");
            this.imageList1.Images.SetKeyName(60, "Macro_Internal.bmp");
            this.imageList1.Images.SetKeyName(61, "Macro_Private.bmp");
            this.imageList1.Images.SetKeyName(62, "Macro_Protected.bmp");
            this.imageList1.Images.SetKeyName(63, "Macro_Sealed.bmp");
            this.imageList1.Images.SetKeyName(64, "Macro_Shortcut.bmp");
            this.imageList1.Images.SetKeyName(65, "Map.bmp");
            this.imageList1.Images.SetKeyName(66, "Map_Internal.bmp");
            this.imageList1.Images.SetKeyName(67, "Map_Private.bmp");
            this.imageList1.Images.SetKeyName(68, "Map_Protected.bmp");
            this.imageList1.Images.SetKeyName(69, "Map_Sealed.bmp");
            this.imageList1.Images.SetKeyName(70, "Map_Shortcut.bmp");
            this.imageList1.Images.SetKeyName(71, "MapItem.bmp");
            this.imageList1.Images.SetKeyName(72, "MapItem_Internal.bmp");
            this.imageList1.Images.SetKeyName(73, "MapItem_Private.bmp");
            this.imageList1.Images.SetKeyName(74, "MapItem_Protected.bmp");
            this.imageList1.Images.SetKeyName(75, "MapItem_Sealed.bmp");
            this.imageList1.Images.SetKeyName(76, "MapItem_Shortcut.bmp");
            this.imageList1.Images.SetKeyName(77, "Method.bmp");
            this.imageList1.Images.SetKeyName(78, "Method_Internal.bmp");
            this.imageList1.Images.SetKeyName(79, "Method_Private.bmp");
            this.imageList1.Images.SetKeyName(80, "Method_Protected.bmp");
            this.imageList1.Images.SetKeyName(81, "Method_Sealed.bmp");
            this.imageList1.Images.SetKeyName(82, "Method_Shortcut.bmp");
            this.imageList1.Images.SetKeyName(83, "MethodOverload.bmp");
            this.imageList1.Images.SetKeyName(84, "MethodOverload_Internal.bmp");
            this.imageList1.Images.SetKeyName(85, "MethodOverload_Private.bmp");
            this.imageList1.Images.SetKeyName(86, "MethodOverload_Protected.bmp");
            this.imageList1.Images.SetKeyName(87, "MethodOverload_Sealed.bmp");
            this.imageList1.Images.SetKeyName(88, "MethodOverload_Shortcut.bmp");
            this.imageList1.Images.SetKeyName(89, "Module.bmp");
            this.imageList1.Images.SetKeyName(90, "Module_Internal.bmp");
            this.imageList1.Images.SetKeyName(91, "Module_Private.bmp");
            this.imageList1.Images.SetKeyName(92, "Module_Protected.bmp");
            this.imageList1.Images.SetKeyName(93, "Module_Sealed.bmp");
            this.imageList1.Images.SetKeyName(94, "Module_Shortcut.bmp");
            this.imageList1.Images.SetKeyName(95, "Namespace.bmp");
            this.imageList1.Images.SetKeyName(96, "Namespace_Internal.bmp");
            this.imageList1.Images.SetKeyName(97, "Namespace_Private.bmp");
            this.imageList1.Images.SetKeyName(98, "Namespace_Protected.bmp");
            this.imageList1.Images.SetKeyName(99, "Namespace_Sealed.bmp");
            this.imageList1.Images.SetKeyName(100, "Namespace_Shortcut.bmp");
            this.imageList1.Images.SetKeyName(101, "Object.bmp");
            this.imageList1.Images.SetKeyName(102, "Object_Internal.bmp");
            this.imageList1.Images.SetKeyName(103, "Object_Private.bmp");
            this.imageList1.Images.SetKeyName(104, "Object_Protected.bmp");
            this.imageList1.Images.SetKeyName(105, "Object_Sealed.bmp");
            this.imageList1.Images.SetKeyName(106, "Object_Shortcut.bmp");
            this.imageList1.Images.SetKeyName(107, "Operator.bmp");
            this.imageList1.Images.SetKeyName(108, "Operator_Internal.bmp");
            this.imageList1.Images.SetKeyName(109, "Operator_Private.bmp");
            this.imageList1.Images.SetKeyName(110, "Operator_Protected.bmp");
            this.imageList1.Images.SetKeyName(111, "Operator_Sealed.bmp");
            this.imageList1.Images.SetKeyName(112, "Operator_Shortcut.bmp");
            this.imageList1.Images.SetKeyName(113, "Properties.bmp");
            this.imageList1.Images.SetKeyName(114, "Properties_Internal.bmp");
            this.imageList1.Images.SetKeyName(115, "Properties_Private.bmp");
            this.imageList1.Images.SetKeyName(116, "Properties_Protected.bmp");
            this.imageList1.Images.SetKeyName(117, "Properties_Sealed.bmp");
            this.imageList1.Images.SetKeyName(118, "Properties_Shortcut.bmp");
            this.imageList1.Images.SetKeyName(119, "Reference.bmp");
            this.imageList1.Images.SetKeyName(120, "Structure.bmp");
            this.imageList1.Images.SetKeyName(121, "Structure_Internal.bmp");
            this.imageList1.Images.SetKeyName(122, "Structure_Private.bmp");
            this.imageList1.Images.SetKeyName(123, "Structure_Protected.bmp");
            this.imageList1.Images.SetKeyName(124, "Structure_Sealed.bmp");
            this.imageList1.Images.SetKeyName(125, "Structure_Shortcut.bmp");
            this.imageList1.Images.SetKeyName(126, "Template.bmp");
            this.imageList1.Images.SetKeyName(127, "Template_Internal.bmp");
            this.imageList1.Images.SetKeyName(128, "Template_Private.bmp");
            this.imageList1.Images.SetKeyName(129, "Template_Protected.bmp");
            this.imageList1.Images.SetKeyName(130, "Template_Sealed.bmp");
            this.imageList1.Images.SetKeyName(131, "Template_Shortcut.bmp");
            this.imageList1.Images.SetKeyName(132, "Type.bmp");
            this.imageList1.Images.SetKeyName(133, "Type_Internal.bmp");
            this.imageList1.Images.SetKeyName(134, "Type_Private.bmp");
            this.imageList1.Images.SetKeyName(135, "Type_Protected.bmp");
            this.imageList1.Images.SetKeyName(136, "Type_Sealed.bmp");
            this.imageList1.Images.SetKeyName(137, "Type_Shortcut.bmp");
            this.imageList1.Images.SetKeyName(138, "TypeDef.bmp");
            this.imageList1.Images.SetKeyName(139, "TypeDef_Internal.bmp");
            this.imageList1.Images.SetKeyName(140, "TypeDef_Private.bmp");
            this.imageList1.Images.SetKeyName(141, "TypeDef_Protected.bmp");
            this.imageList1.Images.SetKeyName(142, "TypeDef_Sealed.bmp");
            this.imageList1.Images.SetKeyName(143, "TypeDef_Shortcut.bmp");
            this.imageList1.Images.SetKeyName(144, "Union.bmp");
            this.imageList1.Images.SetKeyName(145, "Union_Internal.bmp");
            this.imageList1.Images.SetKeyName(146, "Union_Private.bmp");
            this.imageList1.Images.SetKeyName(147, "Union_Protected.bmp");
            this.imageList1.Images.SetKeyName(148, "Union_Sealed.bmp");
            this.imageList1.Images.SetKeyName(149, "Union_Shortcut.bmp");
            this.imageList1.Images.SetKeyName(150, "ValueType.bmp");
            this.imageList1.Images.SetKeyName(151, "ValueType_Internal.bmp");
            this.imageList1.Images.SetKeyName(152, "ValueType_Private.bmp");
            this.imageList1.Images.SetKeyName(153, "ValueType_Protected.bmp");
            this.imageList1.Images.SetKeyName(154, "ValueType_Sealed.bmp");
            this.imageList1.Images.SetKeyName(155, "ValueType_Shortcut.bmp");
            this.imageList1.Images.SetKeyName(156, "warning.bmp");
            this.imageList1.Images.SetKeyName(157, "Error.bmp");
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(218, 200);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = ".Net Dll |*.dll";
            this.openFileDialog1.DereferenceLinks = true;
            this.openFileDialog1.CheckFileExists = true;
            this.openFileDialog1.CheckPathExists = true;
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
            this.openToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
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
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
    }
}

