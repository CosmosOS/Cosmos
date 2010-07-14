namespace Cosmos.Debug.GDB {
    partial class FormDisassembly {
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
            this.panel4 = new System.Windows.Forms.Panel();
            this.lablCurrentFunction = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lboxDisassemble = new System.Windows.Forms.ListBox();
            this.menuDisassembly = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mitemDisassemblyAddBreakpoint = new System.Windows.Forms.ToolStripMenuItem();
            this.panel4.SuspendLayout();
            this.menuDisassembly.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.lablCurrentFunction);
            this.panel4.Controls.Add(this.label5);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel4.Location = new System.Drawing.Point(0, 0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(484, 32);
            this.panel4.TabIndex = 11;
            // 
            // lablCurrentFunction
            // 
            this.lablCurrentFunction.AutoSize = true;
            this.lablCurrentFunction.Location = new System.Drawing.Point(102, 7);
            this.lablCurrentFunction.Name = "lablCurrentFunction";
            this.lablCurrentFunction.Size = new System.Drawing.Size(35, 13);
            this.lablCurrentFunction.TabIndex = 1;
            this.lablCurrentFunction.Text = "label9";
            this.lablCurrentFunction.Visible = false;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 7);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(88, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Current Function:";
            // 
            // lboxDisassemble
            // 
            this.lboxDisassemble.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lboxDisassemble.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lboxDisassemble.FormattingEnabled = true;
            this.lboxDisassemble.ItemHeight = 19;
            this.lboxDisassemble.Location = new System.Drawing.Point(0, 32);
            this.lboxDisassemble.Name = "lboxDisassemble";
            this.lboxDisassemble.Size = new System.Drawing.Size(484, 441);
            this.lboxDisassemble.TabIndex = 12;
            // 
            // menuDisassembly
            // 
            this.menuDisassembly.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mitemDisassemblyAddBreakpoint});
            this.menuDisassembly.Name = "menuDisassembly";
            this.menuDisassembly.Size = new System.Drawing.Size(157, 48);
            // 
            // mitemDisassemblyAddBreakpoint
            // 
            this.mitemDisassemblyAddBreakpoint.Name = "mitemDisassemblyAddBreakpoint";
            this.mitemDisassemblyAddBreakpoint.Size = new System.Drawing.Size(156, 22);
            this.mitemDisassemblyAddBreakpoint.Text = "&Add Breakpoint";
            this.mitemDisassemblyAddBreakpoint.Click += new System.EventHandler(this.mitemDisassemblyAddBreakpoint_Click);
            // 
            // FormDisassembly
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 477);
            this.Controls.Add(this.lboxDisassemble);
            this.Controls.Add(this.panel4);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "FormDisassembly";
            this.ShowInTaskbar = false;
            this.Text = "Disassembly";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormDisassembly_FormClosing);
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.menuDisassembly.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Label lablCurrentFunction;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ListBox lboxDisassemble;
        private System.Windows.Forms.ContextMenuStrip menuDisassembly;
        private System.Windows.Forms.ToolStripMenuItem mitemDisassemblyAddBreakpoint;

    }
}