using System.Windows.Forms;

namespace Cosmos.Debug.GDB {
    partial class BreakpointUC {
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.lablNum = new Label();
            this.cboxEnabled = new System.Windows.Forms.CheckBox();
            this.lablName = new Label();
            this.lablDelete = new Label();
            this.SuspendLayout();
            //
            // lablNum
            //
            this.lablNum.AutoSize = true;
            this.lablNum.Location = new System.Drawing.Point(24, 4);
            this.lablNum.Name = "lablNum";
            this.lablNum.Size = new System.Drawing.Size(28, 13);
            this.lablNum.TabIndex = 0;
            this.lablNum.Text = "XXX";
            //
            // cboxEnabled
            //
            this.cboxEnabled.AutoSize = true;
            this.cboxEnabled.Location = new System.Drawing.Point(3, 4);
            this.cboxEnabled.Name = "cboxEnabled";
            this.cboxEnabled.Size = new System.Drawing.Size(15, 14);
            this.cboxEnabled.TabIndex = 1;
            this.cboxEnabled.UseVisualStyleBackColor = true;
            //
            // lablName
            //
            this.lablName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lablName.Location = new System.Drawing.Point(58, 4);
            this.lablName.Name = "lablName";
            this.lablName.Size = new System.Drawing.Size(175, 13);
            this.lablName.TabIndex = 2;
            this.lablName.Text = "Breakpoint Name";
            //
            // lablDelete
            //
            this.lablDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lablDelete.AutoSize = true;
            this.lablDelete.BackColor = System.Drawing.SystemColors.Control;
            this.lablDelete.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lablDelete.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lablDelete.ForeColor = System.Drawing.Color.Red;
            this.lablDelete.Location = new System.Drawing.Point(239, 5);
            this.lablDelete.Name = "lablDelete";
            this.lablDelete.Size = new System.Drawing.Size(17, 15);
            this.lablDelete.TabIndex = 3;
            this.lablDelete.Text = "X";
            this.lablDelete.Click += new System.EventHandler(this.lablDelete_Click);
            //
            // BreakpointUC
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lablDelete);
            this.Controls.Add(this.lablName);
            this.Controls.Add(this.cboxEnabled);
            this.Controls.Add(this.lablNum);
            this.Name = "BreakpointUC";
            this.Size = new System.Drawing.Size(256, 22);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.Label lablNum;
        public System.Windows.Forms.CheckBox cboxEnabled;
        public System.Windows.Forms.Label lablName;
        public System.Windows.Forms.Label lablDelete;

    }
}
