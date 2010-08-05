namespace Cosmos.VS.Package
{
    partial class DebugPageVMWare
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
            this.titleGeneral = new Cosmos.VS.Package.TitleDivision();
            this.label1 = new System.Windows.Forms.Label();
            this.comboDebugMode = new System.Windows.Forms.ComboBox();
            this.table = new System.Windows.Forms.TableLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.comboTraceMode = new System.Windows.Forms.ComboBox();
            this.titleDivision1 = new Cosmos.VS.Package.TitleDivision();
            this.checkEnableGDB = new System.Windows.Forms.CheckBox();
            this.checkStartCosmosGDB = new System.Windows.Forms.CheckBox();
            this.table.SuspendLayout();
            this.SuspendLayout();
            // 
            // titleGeneral
            // 
            this.titleGeneral.BackColor = System.Drawing.Color.Transparent;
            this.table.SetColumnSpan(this.titleGeneral, 2);
            this.titleGeneral.Dock = System.Windows.Forms.DockStyle.Top;
            this.titleGeneral.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.titleGeneral.LineColor = System.Drawing.SystemColors.ControlDark;
            this.titleGeneral.Location = new System.Drawing.Point(3, 3);
            this.titleGeneral.Margin = new System.Windows.Forms.Padding(3, 3, 3, 12);
            this.titleGeneral.Name = "titleGeneral";
            this.titleGeneral.Size = new System.Drawing.Size(257, 15);
            this.titleGeneral.TabIndex = 0;
            this.titleGeneral.Title = "Visual Studio";
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(39, 33);
            this.label1.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(224, 21);
            this.label1.TabIndex = 14;
            this.label1.Text = "Debug Level:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // comboDebugMode
            // 
            this.comboDebugMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboDebugMode.FormattingEnabled = true;
            this.comboDebugMode.Location = new System.Drawing.Point(266, 33);
            this.comboDebugMode.Name = "comboDebugMode";
            this.comboDebugMode.Size = new System.Drawing.Size(223, 21);
            this.comboDebugMode.TabIndex = 19;
            // 
            // table
            // 
            this.table.ColumnCount = 3;
            this.table.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 39F));
            this.table.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 49.66887F));
            this.table.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.33113F));
            this.table.Controls.Add(this.checkStartCosmosGDB, 1, 7);
            this.table.Controls.Add(this.label2, 1, 3);
            this.table.Controls.Add(this.titleGeneral, 0, 0);
            this.table.Controls.Add(this.label1, 1, 1);
            this.table.Controls.Add(this.comboDebugMode, 2, 1);
            this.table.Controls.Add(this.comboTraceMode, 2, 3);
            this.table.Controls.Add(this.titleDivision1, 2, 3);
            this.table.Controls.Add(this.checkEnableGDB, 1, 5);
            this.table.Dock = System.Windows.Forms.DockStyle.Fill;
            this.table.Location = new System.Drawing.Point(0, 0);
            this.table.Margin = new System.Windows.Forms.Padding(0);
            this.table.Name = "table";
            this.table.RowCount = 12;
            this.table.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.table.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
            this.table.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.table.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.table.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
            this.table.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.table.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.table.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
            this.table.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.table.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.table.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.table.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.table.Size = new System.Drawing.Size(492, 288);
            this.table.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(39, 60);
            this.label2.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(224, 21);
            this.label2.TabIndex = 18;
            this.label2.Text = "Tracing:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // comboTraceMode
            // 
            this.comboTraceMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboTraceMode.FormattingEnabled = true;
            this.comboTraceMode.Location = new System.Drawing.Point(266, 60);
            this.comboTraceMode.Name = "comboTraceMode";
            this.comboTraceMode.Size = new System.Drawing.Size(223, 21);
            this.comboTraceMode.TabIndex = 15;
            // 
            // titleDivision1
            // 
            this.titleDivision1.BackColor = System.Drawing.Color.Transparent;
            this.table.SetColumnSpan(this.titleDivision1, 2);
            this.titleDivision1.Dock = System.Windows.Forms.DockStyle.Top;
            this.titleDivision1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.titleDivision1.LineColor = System.Drawing.SystemColors.ControlDark;
            this.titleDivision1.Location = new System.Drawing.Point(3, 87);
            this.titleDivision1.Margin = new System.Windows.Forms.Padding(3, 3, 3, 12);
            this.titleDivision1.Name = "titleDivision1";
            this.titleDivision1.Size = new System.Drawing.Size(257, 12);
            this.titleDivision1.TabIndex = 25;
            this.titleDivision1.Title = "Machine Level";
            // 
            // checkEnableGDB
            // 
            this.checkEnableGDB.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkEnableGDB.Location = new System.Drawing.Point(42, 114);
            this.checkEnableGDB.Name = "checkEnableGDB";
            this.checkEnableGDB.Size = new System.Drawing.Size(218, 20);
            this.checkEnableGDB.TabIndex = 24;
            this.checkEnableGDB.Text = "Enable GDB Debugger";
            this.checkEnableGDB.UseVisualStyleBackColor = true;
            // 
            // checkStartCosmosGDB
            // 
            this.checkStartCosmosGDB.AutoSize = true;
            this.checkStartCosmosGDB.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkStartCosmosGDB.Location = new System.Drawing.Point(42, 140);
            this.checkStartCosmosGDB.Name = "checkStartCosmosGDB";
            this.checkStartCosmosGDB.Size = new System.Drawing.Size(199, 19);
            this.checkStartCosmosGDB.TabIndex = 27;
            this.checkStartCosmosGDB.Text = "AutoStart Cosmos GDB Frontend";
            this.checkStartCosmosGDB.UseVisualStyleBackColor = true;
            // 
            // DebugPageVMWare
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.table);
            this.Name = "DebugPageVMWare";
            this.Size = new System.Drawing.Size(492, 288);
            this.table.ResumeLayout(false);
            this.table.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private TitleDivision titleGeneral;
        private System.Windows.Forms.TableLayoutPanel table;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboDebugMode;
        private System.Windows.Forms.ComboBox comboTraceMode;
        private TitleDivision titleDivision1;
        private System.Windows.Forms.CheckBox checkEnableGDB;
        private System.Windows.Forms.CheckBox checkStartCosmosGDB;

    }
}
