namespace Cosmos.Debug.GDB {
    partial class FormBreakpoints {
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
            this.lboxBreakpoints = new System.Windows.Forms.ListBox();
            this.panel6 = new System.Windows.Forms.Panel();
            this.butnBreakpointAdd = new System.Windows.Forms.Button();
            this.textBreakpoint = new System.Windows.Forms.TextBox();
            this.menuBreakpoints = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mitmBreakpointDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.panel6.SuspendLayout();
            this.menuBreakpoints.SuspendLayout();
            this.SuspendLayout();
            // 
            // lboxBreakpoints
            // 
            this.lboxBreakpoints.ContextMenuStrip = this.menuBreakpoints;
            this.lboxBreakpoints.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lboxBreakpoints.FormattingEnabled = true;
            this.lboxBreakpoints.Location = new System.Drawing.Point(0, 32);
            this.lboxBreakpoints.Name = "lboxBreakpoints";
            this.lboxBreakpoints.Size = new System.Drawing.Size(284, 225);
            this.lboxBreakpoints.Sorted = true;
            this.lboxBreakpoints.TabIndex = 67;
            // 
            // panel6
            // 
            this.panel6.Controls.Add(this.butnBreakpointAdd);
            this.panel6.Controls.Add(this.textBreakpoint);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel6.Location = new System.Drawing.Point(0, 0);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(284, 32);
            this.panel6.TabIndex = 66;
            // 
            // butnBreakpointAdd
            // 
            this.butnBreakpointAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butnBreakpointAdd.Location = new System.Drawing.Point(252, 7);
            this.butnBreakpointAdd.Name = "butnBreakpointAdd";
            this.butnBreakpointAdd.Size = new System.Drawing.Size(24, 23);
            this.butnBreakpointAdd.TabIndex = 5;
            this.butnBreakpointAdd.Text = "+";
            this.butnBreakpointAdd.UseVisualStyleBackColor = true;
            this.butnBreakpointAdd.Click += new System.EventHandler(this.butnBreakpointAdd_Click);
            // 
            // textBreakpoint
            // 
            this.textBreakpoint.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBreakpoint.Location = new System.Drawing.Point(8, 7);
            this.textBreakpoint.Name = "textBreakpoint";
            this.textBreakpoint.Size = new System.Drawing.Size(238, 20);
            this.textBreakpoint.TabIndex = 4;
            this.textBreakpoint.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBreakpoint_KeyPress);
            // 
            // menuBreakpoints
            // 
            this.menuBreakpoints.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mitmBreakpointDelete});
            this.menuBreakpoints.Name = "menuBreakpoints";
            this.menuBreakpoints.Size = new System.Drawing.Size(108, 26);
            // 
            // mitmBreakpointDelete
            // 
            this.mitmBreakpointDelete.Name = "mitmBreakpointDelete";
            this.mitmBreakpointDelete.Size = new System.Drawing.Size(107, 22);
            this.mitmBreakpointDelete.Text = "&Delete";
            this.mitmBreakpointDelete.Click += new System.EventHandler(this.mitmBreakpointDelete_Click);
            // 
            // FormBreakpoints
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.lboxBreakpoints);
            this.Controls.Add(this.panel6);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "FormBreakpoints";
            this.ShowInTaskbar = false;
            this.Text = "Breakpoints";
            this.panel6.ResumeLayout(false);
            this.panel6.PerformLayout();
            this.menuBreakpoints.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox lboxBreakpoints;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.Button butnBreakpointAdd;
        private System.Windows.Forms.TextBox textBreakpoint;
        private System.Windows.Forms.ContextMenuStrip menuBreakpoints;
        private System.Windows.Forms.ToolStripMenuItem mitmBreakpointDelete;
    }
}