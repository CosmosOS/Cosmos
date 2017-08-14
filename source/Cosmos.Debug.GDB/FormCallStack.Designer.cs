namespace Cosmos.Debug.GDB {
    partial class FormCallStack {
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
            this.components = new System.ComponentModel.Container();
            this.lboxCallStack = new System.Windows.Forms.ListBox();
            this.menuCallStack = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuCallStackGoto = new System.Windows.Forms.ToolStripMenuItem();
            this.menuCallStack.SuspendLayout();
            this.SuspendLayout();
            // 
            // lboxCallStack
            // 
            this.lboxCallStack.ContextMenuStrip = this.menuCallStack;
            this.lboxCallStack.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lboxCallStack.FormattingEnabled = true;
            this.lboxCallStack.HorizontalScrollbar = true;
            this.lboxCallStack.Location = new System.Drawing.Point(0, 0);
            this.lboxCallStack.Name = "lboxCallStack";
            this.lboxCallStack.Size = new System.Drawing.Size(284, 251);
            this.lboxCallStack.TabIndex = 71;
            this.lboxCallStack.DoubleClick += new System.EventHandler(this.lboxCallStack_DoubleClick);
            // 
            // menuCallStack
            // 
            this.menuCallStack.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuCallStackGoto});
            this.menuCallStack.Name = "menuCallStack";
            this.menuCallStack.Size = new System.Drawing.Size(215, 26);
            // 
            // menuCallStackGoto
            // 
            this.menuCallStackGoto.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.menuCallStackGoto.Name = "menuCallStackGoto";
            this.menuCallStackGoto.Size = new System.Drawing.Size(214, 22);
            this.menuCallStackGoto.Text = "&Go To (Disassemble Only)";
            this.menuCallStackGoto.Click += new System.EventHandler(this.menuCallStackGoto_Click);
            // 
            // FormCallStack
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.lboxCallStack);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "FormCallStack";
            this.ShowInTaskbar = false;
            this.Text = "CallStack";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormCallStack_FormClosing);
            this.menuCallStack.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox lboxCallStack;
        private System.Windows.Forms.ContextMenuStrip menuCallStack;
        private System.Windows.Forms.ToolStripMenuItem menuCallStackGoto;

    }
}