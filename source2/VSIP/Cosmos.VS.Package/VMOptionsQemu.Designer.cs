namespace Cosmos.VS.Package
{
	partial class VMOptionsQemu
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
			this.tableVMQemu = new System.Windows.Forms.TableLayoutPanel();
			this.tableAdvanced = new System.Windows.Forms.TableLayoutPanel();
			this.checkEnableGDB = new System.Windows.Forms.CheckBox();
			this.labelGDB = new System.Windows.Forms.Label();
			this.titleAdvanced = new Cosmos.VS.Package.TitleDivision();
			this.titleDevices = new Cosmos.VS.Package.TitleDivision();
			this.tableDevices = new System.Windows.Forms.TableLayoutPanel();
			this.labelNetworkCard = new System.Windows.Forms.Label();
			this.labelAudioCard = new System.Windows.Forms.Label();
			this.tableHardDisk = new System.Windows.Forms.TableLayoutPanel();
			this.checkEnableDiskImages = new System.Windows.Forms.CheckBox();
			this.labelInternalAssembler = new System.Windows.Forms.Label();
			this.panelNetwork = new System.Windows.Forms.Panel();
			this.checkNetworkTAP = new System.Windows.Forms.CheckBox();
			this.comboNetworkCard = new System.Windows.Forms.ComboBox();
			this.comboAudioCard = new System.Windows.Forms.ComboBox();
			this.tableVMQemu.SuspendLayout();
			this.tableAdvanced.SuspendLayout();
			this.tableDevices.SuspendLayout();
			this.tableHardDisk.SuspendLayout();
			this.panelNetwork.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableVMQemu
			// 
			this.tableVMQemu.ColumnCount = 1;
			this.tableVMQemu.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableVMQemu.Controls.Add(this.tableAdvanced, 0, 3);
			this.tableVMQemu.Controls.Add(this.titleAdvanced, 0, 2);
			this.tableVMQemu.Controls.Add(this.titleDevices, 0, 0);
			this.tableVMQemu.Controls.Add(this.tableDevices, 0, 1);
			this.tableVMQemu.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableVMQemu.Location = new System.Drawing.Point(0, 0);
			this.tableVMQemu.Margin = new System.Windows.Forms.Padding(0);
			this.tableVMQemu.Name = "tableVMQemu";
			this.tableVMQemu.RowCount = 5;
			this.tableVMQemu.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableVMQemu.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableVMQemu.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableVMQemu.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableVMQemu.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableVMQemu.Size = new System.Drawing.Size(492, 288);
			this.tableVMQemu.TabIndex = 1;
			// 
			// tableAdvanced
			// 
			this.tableAdvanced.ColumnCount = 2;
			this.tableAdvanced.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableAdvanced.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableAdvanced.Controls.Add(this.checkEnableGDB, 0, 0);
			this.tableAdvanced.Controls.Add(this.labelGDB, 0, 1);
			this.tableAdvanced.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableAdvanced.Location = new System.Drawing.Point(22, 180);
			this.tableAdvanced.Margin = new System.Windows.Forms.Padding(22, 0, 0, 0);
			this.tableAdvanced.Name = "tableAdvanced";
			this.tableAdvanced.RowCount = 3;
			this.tableAdvanced.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableAdvanced.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableAdvanced.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableAdvanced.Size = new System.Drawing.Size(470, 56);
			this.tableAdvanced.TabIndex = 4;
			// 
			// checkEnableGDB
			// 
			this.checkEnableGDB.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkEnableGDB.Location = new System.Drawing.Point(3, 3);
			this.checkEnableGDB.Name = "checkEnableGDB";
			this.checkEnableGDB.Size = new System.Drawing.Size(163, 20);
			this.checkEnableGDB.TabIndex = 9;
			this.checkEnableGDB.Text = "Enable GDB";
			this.checkEnableGDB.UseVisualStyleBackColor = true;
			// 
			// labelGDB
			// 
			this.labelGDB.Location = new System.Drawing.Point(22, 26);
			this.labelGDB.Margin = new System.Windows.Forms.Padding(22, 0, 3, 0);
			this.labelGDB.Name = "labelGDB";
			this.labelGDB.Size = new System.Drawing.Size(210, 28);
			this.labelGDB.TabIndex = 10;
			this.labelGDB.Text = "Enable QEMU\'s GDB support.";
			// 
			// titleAdvanced
			// 
			this.titleAdvanced.BackColor = System.Drawing.Color.Transparent;
			this.titleAdvanced.Dock = System.Windows.Forms.DockStyle.Top;
			this.titleAdvanced.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
			this.titleAdvanced.LineColor = System.Drawing.SystemColors.ControlDark;
			this.titleAdvanced.Location = new System.Drawing.Point(3, 153);
			this.titleAdvanced.Margin = new System.Windows.Forms.Padding(3, 12, 3, 12);
			this.titleAdvanced.Name = "titleAdvanced";
			this.titleAdvanced.Size = new System.Drawing.Size(486, 15);
			this.titleAdvanced.TabIndex = 3;
			this.titleAdvanced.Title = "Advanced";
			// 
			// titleDevices
			// 
			this.titleDevices.BackColor = System.Drawing.Color.Transparent;
			this.titleDevices.Dock = System.Windows.Forms.DockStyle.Top;
			this.titleDevices.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
			this.titleDevices.LineColor = System.Drawing.SystemColors.ControlDark;
			this.titleDevices.Location = new System.Drawing.Point(3, 3);
			this.titleDevices.Margin = new System.Windows.Forms.Padding(3, 3, 3, 12);
			this.titleDevices.Name = "titleDevices";
			this.titleDevices.Size = new System.Drawing.Size(486, 15);
			this.titleDevices.TabIndex = 0;
			this.titleDevices.Title = "Devices";
			// 
			// tableDevices
			// 
			this.tableDevices.ColumnCount = 2;
			this.tableDevices.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
			this.tableDevices.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
			this.tableDevices.Controls.Add(this.labelNetworkCard, 0, 0);
			this.tableDevices.Controls.Add(this.labelAudioCard, 0, 1);
			this.tableDevices.Controls.Add(this.tableHardDisk, 0, 2);
			this.tableDevices.Controls.Add(this.panelNetwork, 1, 0);
			this.tableDevices.Controls.Add(this.comboAudioCard, 1, 1);
			this.tableDevices.Location = new System.Drawing.Point(22, 30);
			this.tableDevices.Margin = new System.Windows.Forms.Padding(22, 0, 0, 0);
			this.tableDevices.Name = "tableDevices";
			this.tableDevices.RowCount = 3;
			this.tableDevices.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableDevices.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
			this.tableDevices.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 14F));
			this.tableDevices.Size = new System.Drawing.Size(470, 111);
			this.tableDevices.TabIndex = 1;
			// 
			// labelNetworkCard
			// 
			this.labelNetworkCard.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelNetworkCard.Location = new System.Drawing.Point(0, 3);
			this.labelNetworkCard.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
			this.labelNetworkCard.Name = "labelNetworkCard";
			this.labelNetworkCard.Size = new System.Drawing.Size(141, 20);
			this.labelNetworkCard.TabIndex = 2;
			this.labelNetworkCard.Text = "Network Card:";
			this.labelNetworkCard.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelAudioCard
			// 
			this.labelAudioCard.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelAudioCard.Location = new System.Drawing.Point(0, 30);
			this.labelAudioCard.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
			this.labelAudioCard.Name = "labelAudioCard";
			this.labelAudioCard.Size = new System.Drawing.Size(141, 20);
			this.labelAudioCard.TabIndex = 14;
			this.labelAudioCard.Text = "Audio Card:";
			this.labelAudioCard.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// tableHardDisk
			// 
			this.tableHardDisk.ColumnCount = 2;
			this.tableDevices.SetColumnSpan(this.tableHardDisk, 2);
			this.tableHardDisk.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableHardDisk.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableHardDisk.Controls.Add(this.checkEnableDiskImages, 0, 0);
			this.tableHardDisk.Controls.Add(this.labelInternalAssembler, 0, 1);
			this.tableHardDisk.Location = new System.Drawing.Point(0, 53);
			this.tableHardDisk.Margin = new System.Windows.Forms.Padding(0);
			this.tableHardDisk.Name = "tableHardDisk";
			this.tableHardDisk.RowCount = 2;
			this.tableHardDisk.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableHardDisk.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableHardDisk.Size = new System.Drawing.Size(470, 56);
			this.tableHardDisk.TabIndex = 16;
			// 
			// checkEnableDiskImages
			// 
			this.checkEnableDiskImages.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkEnableDiskImages.Location = new System.Drawing.Point(3, 3);
			this.checkEnableDiskImages.Name = "checkEnableDiskImages";
			this.checkEnableDiskImages.Size = new System.Drawing.Size(176, 20);
			this.checkEnableDiskImages.TabIndex = 8;
			this.checkEnableDiskImages.Text = "Enable Hard Disk Images";
			this.checkEnableDiskImages.UseVisualStyleBackColor = true;
			// 
			// labelInternalAssembler
			// 
			this.labelInternalAssembler.Location = new System.Drawing.Point(22, 26);
			this.labelInternalAssembler.Margin = new System.Windows.Forms.Padding(22, 0, 3, 0);
			this.labelInternalAssembler.Name = "labelInternalAssembler";
			this.labelInternalAssembler.Size = new System.Drawing.Size(210, 30);
			this.labelInternalAssembler.TabIndex = 7;
			this.labelInternalAssembler.Text = "Attach any *.img file in the build path as a storage device.";
			// 
			// panelNetwork
			// 
			this.panelNetwork.Controls.Add(this.checkNetworkTAP);
			this.panelNetwork.Controls.Add(this.comboNetworkCard);
			this.panelNetwork.Location = new System.Drawing.Point(141, 0);
			this.panelNetwork.Margin = new System.Windows.Forms.Padding(0);
			this.panelNetwork.Name = "panelNetwork";
			this.panelNetwork.Size = new System.Drawing.Size(329, 27);
			this.panelNetwork.TabIndex = 17;
			// 
			// checkNetworkTAP
			// 
			this.checkNetworkTAP.AutoSize = true;
			this.checkNetworkTAP.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkNetworkTAP.Location = new System.Drawing.Point(152, 5);
			this.checkNetworkTAP.Name = "checkNetworkTAP";
			this.checkNetworkTAP.Size = new System.Drawing.Size(134, 19);
			this.checkNetworkTAP.TabIndex = 15;
			this.checkNetworkTAP.Text = "Enable Network TAP";
			this.checkNetworkTAP.UseVisualStyleBackColor = true;
			// 
			// comboNetworkCard
			// 
			this.comboNetworkCard.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboNetworkCard.FormattingEnabled = true;
			this.comboNetworkCard.Location = new System.Drawing.Point(3, 4);
			this.comboNetworkCard.Name = "comboNetworkCard";
			this.comboNetworkCard.Size = new System.Drawing.Size(143, 21);
			this.comboNetworkCard.TabIndex = 14;
			// 
			// comboAudioCard
			// 
			this.comboAudioCard.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboAudioCard.FormattingEnabled = true;
			this.comboAudioCard.Location = new System.Drawing.Point(144, 30);
			this.comboAudioCard.Name = "comboAudioCard";
			this.comboAudioCard.Size = new System.Drawing.Size(143, 21);
			this.comboAudioCard.TabIndex = 15;
			// 
			// VMOptionsQemu
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableVMQemu);
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Name = "VMOptionsQemu";
			this.Size = new System.Drawing.Size(492, 288);
			this.tableVMQemu.ResumeLayout(false);
			this.tableAdvanced.ResumeLayout(false);
			this.tableDevices.ResumeLayout(false);
			this.tableHardDisk.ResumeLayout(false);
			this.panelNetwork.ResumeLayout(false);
			this.panelNetwork.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableVMQemu;
		private TitleDivision titleDevices;
		private System.Windows.Forms.TableLayoutPanel tableDevices;
		private System.Windows.Forms.Label labelNetworkCard;
		private System.Windows.Forms.Label labelAudioCard;
		private System.Windows.Forms.ComboBox comboAudioCard;
		private System.Windows.Forms.TableLayoutPanel tableHardDisk;
		private System.Windows.Forms.Label labelInternalAssembler;
		private System.Windows.Forms.CheckBox checkEnableDiskImages;
		private System.Windows.Forms.CheckBox checkEnableGDB;
		private System.Windows.Forms.Panel panelNetwork;
		private System.Windows.Forms.CheckBox checkNetworkTAP;
		private System.Windows.Forms.ComboBox comboNetworkCard;
		private TitleDivision titleAdvanced;
		private System.Windows.Forms.TableLayoutPanel tableAdvanced;
		private System.Windows.Forms.Label labelGDB;
	}
}
