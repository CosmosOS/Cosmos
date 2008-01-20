namespace Cosmos.Unity
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
            this._mainSplitContainer = new System.Windows.Forms.SplitContainer();
            this._secondSplitContainer = new System.Windows.Forms.SplitContainer();
            this._mainSplitContainer.Panel1.SuspendLayout();
            this._mainSplitContainer.SuspendLayout();
            this._secondSplitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // _mainSplitContainer
            // 
            this._mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._mainSplitContainer.Location = new System.Drawing.Point(0, 0);
            this._mainSplitContainer.Name = "_mainSplitContainer";
            // 
            // _mainSplitContainer.Panel1
            // 
            this._mainSplitContainer.Panel1.Controls.Add(this._secondSplitContainer);
            this._mainSplitContainer.Size = new System.Drawing.Size(455, 270);
            this._mainSplitContainer.SplitterDistance = 249;
            this._mainSplitContainer.TabIndex = 0;
            // 
            // _secondSplitContainer
            // 
            this._secondSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._secondSplitContainer.Location = new System.Drawing.Point(0, 0);
            this._secondSplitContainer.Name = "_secondSplitContainer";
            this._secondSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this._secondSplitContainer.Size = new System.Drawing.Size(249, 270);
            this._secondSplitContainer.SplitterDistance = 136;
            this._secondSplitContainer.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(455, 270);
            this.Controls.Add(this._mainSplitContainer);
            this.Name = "MainForm";
            this.Text = "Cosmos Unity";
            this._mainSplitContainer.Panel1.ResumeLayout(false);
            this._mainSplitContainer.ResumeLayout(false);
            this._secondSplitContainer.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer _mainSplitContainer;
        private System.Windows.Forms.SplitContainer _secondSplitContainer;
    }
}