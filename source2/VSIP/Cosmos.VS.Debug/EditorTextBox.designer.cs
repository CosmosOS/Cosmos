
namespace Cosmos.Cosmos_VS_Debug
{
    partial class EditorTextBox
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
            this.SuspendLayout();
            // 
            // EditorTextBox
            // 
            this.MouseEnter += new System.EventHandler(this.richTextBoxCtrl_MouseRecording);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.richTextBoxCtrl_MouseRecording);
            this.MouseLeave += new System.EventHandler(this.richTextBoxCtrl_MouseLeave);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.richTextBoxCtrl_MouseRecording);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
