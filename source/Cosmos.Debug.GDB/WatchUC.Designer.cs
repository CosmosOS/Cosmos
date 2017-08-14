using System.Windows.Forms;

namespace Cosmos.Debug.GDB {
    partial class WatchUC {
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
			this.lablAddress = new Label();
			this.cboxEnabled = new System.Windows.Forms.CheckBox();
			this.lablValue = new System.Windows.Forms.TextBox();
			this.lablDelete = new Label();
			this.SuspendLayout();
			//
			// lablAddress
			//
			this.lablAddress.AutoSize = true;
			this.lablAddress.Location = new System.Drawing.Point(24, 4);
			this.lablAddress.Name = "lablAddress";
			this.lablAddress.Size = new System.Drawing.Size(66, 13);
			this.lablAddress.TabIndex = 0;
			this.lablAddress.Text = "0x00000000";
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
			// lablValue
			//
			this.lablValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lablValue.BackColor = System.Drawing.SystemColors.Control;
			this.lablValue.Location = new System.Drawing.Point(106, 4);
			this.lablValue.Multiline = true;
			this.lablValue.Name = "lablValue";
			this.lablValue.ReadOnly = true;
			this.lablValue.Size = new System.Drawing.Size(165, 22);
			this.lablValue.TabIndex = 2;
			this.lablValue.Text = "Watch Value";
			this.lablValue.TextChanged += new System.EventHandler(this.lablName_TextChanged);
			//
			// lablDelete
			//
			this.lablDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lablDelete.AutoSize = true;
			this.lablDelete.BackColor = System.Drawing.SystemColors.Control;
			this.lablDelete.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lablDelete.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lablDelete.ForeColor = System.Drawing.Color.Red;
			this.lablDelete.Location = new System.Drawing.Point(277, 5);
			this.lablDelete.Name = "lablDelete";
			this.lablDelete.Size = new System.Drawing.Size(17, 15);
			this.lablDelete.TabIndex = 3;
			this.lablDelete.Text = "X";
			this.lablDelete.Click += new System.EventHandler(this.lablDelete_Click);
			//
			// WatchUC
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.Controls.Add(this.lablDelete);
			this.Controls.Add(this.lablValue);
			this.Controls.Add(this.cboxEnabled);
			this.Controls.Add(this.lablAddress);
			this.Name = "WatchUC";
			this.Size = new System.Drawing.Size(294, 29);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.Label lablAddress;
        public System.Windows.Forms.CheckBox cboxEnabled;
        public System.Windows.Forms.TextBox lablValue;
        public System.Windows.Forms.Label lablDelete;

    }
}
