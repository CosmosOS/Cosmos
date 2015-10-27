//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using System.Diagnostics;

namespace Microsoft.Samples.Tools.Mdbg.Extension
{
    /// <summary>
    /// Summary description for Form1.
    /// </summary>
    public class AboutForm : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button closeBtn;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private Label labelVersion;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public AboutForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            Type tGui = typeof(AboutForm);
            Type tEngine = typeof(Microsoft.Samples.Debugging.MdbgEngine.MDbgEngine);
            // Set version info
            this.labelVersion.Text =
                "Version:" +
                "Gui=" + tGui.Assembly.GetName().Version.ToString() +
                ",  MdbgEng=" + tEngine.Assembly.GetName().Version.ToString();

        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if(components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }

        //#region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.closeBtn = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.labelVersion = new System.Windows.Forms.Label();
            this.SuspendLayout();
// 
// label1
// 
            this.label1.Location = new System.Drawing.Point(16, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(328, 24);
            this.label1.TabIndex = 0;
            this.label1.Text = "GUI: Simple Extension for MDbg debugger";
// 
// closeBtn
// 
            this.closeBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.closeBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.closeBtn.Location = new System.Drawing.Point(292, 255);
            this.closeBtn.Name = "closeBtn";
            this.closeBtn.Size = new System.Drawing.Size(80, 23);
            this.closeBtn.TabIndex = 1;
            this.closeBtn.Text = "Close";
            this.closeBtn.Click += new System.EventHandler(this.closeBtn_Click);
// 
// richTextBox1
// 
            this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBox1.Location = new System.Drawing.Point(8, 53);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(364, 195);
            this.richTextBox1.TabIndex = 2;
            this.richTextBox1.Text = @"Defined Hotkeys:
F9  - Set / Remove Breakpoint at line
F10 - Step Over
F11 - Step Into
Shift+F11 - Step Out

Visit the MDbg forum (http://forums.microsoft.com/MSDN/ShowForum.aspx?ForumID=868&SiteID=1) for MDbg issues or further questions about writing your own .NET diagnostic tools.
";
            this.richTextBox1.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.richTextBox1_LinkClicked);
            this.richTextBox1.TextChanged += new System.EventHandler(this.richTextBox1_TextChanged);
// 
// labelVersion
// 
            this.labelVersion.AutoSize = true;
            this.labelVersion.Location = new System.Drawing.Point(16, 32);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(43, 14);
            this.labelVersion.TabIndex = 3;
            this.labelVersion.Text = "Version";
// 
// AboutForm
// 
            this.AcceptButton = this.closeBtn;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.CancelButton = this.closeBtn;
            this.ClientSize = new System.Drawing.Size(384, 290);
            this.Controls.Add(this.labelVersion);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.closeBtn);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "About";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        //#endregion

        private void closeBtn_Click(object sender, System.EventArgs e)
        {
            this.Dispose();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
        
        }

        // Launch the target of the link
        private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {            
            string target = e.LinkText;
            
            try
            {
                // More general start.
                Process.Start(e.LinkText);

                return;
            }
            catch(Exception)
            {
                // Swallow exception.
            }

            // Try again.
            // This is silly. Start() appears to look at the extension.
            // But URLs are based off the prefix (http) and may have any random extension.
            // So try again and explicitly use IE.
            try
            {
                Process.Start("IExplore.exe", target);
                return;
            }
            catch (Exception)
            {
                // Swallow exception.
            }

        }
    }
}
