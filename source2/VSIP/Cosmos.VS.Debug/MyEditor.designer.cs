namespace Cosmos.Cosmos_VS_Debug
{
    partial class MyEditor
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.richTextBoxCtrl = new Cosmos.Cosmos_VS_Debug.EditorTextBox();
            this.SuspendLayout();
            // 
            // richTextBoxCtrl
            // 
            this.richTextBoxCtrl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxCtrl.Location = new System.Drawing.Point(0, 0);
            this.richTextBoxCtrl.Name = "richTextBoxCtrl";
            this.richTextBoxCtrl.Size = new System.Drawing.Size(150, 150);
            this.richTextBoxCtrl.TabIndex = 0;
            this.richTextBoxCtrl.MouseEnter += new System.EventHandler(this.richTextBoxCtrl_MouseEnter);
            this.richTextBoxCtrl.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.richTextBoxCtrl_KeyPress);
            this.richTextBoxCtrl.KeyDown += new System.Windows.Forms.KeyEventHandler(this.richTextBoxCtrl_KeyDown);
            // 
            // MyEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.richTextBoxCtrl);
            this.Name = "MyEditor";
            this.ResumeLayout(false);

        }

        #endregion

        private EditorTextBox richTextBoxCtrl;


    }
}
