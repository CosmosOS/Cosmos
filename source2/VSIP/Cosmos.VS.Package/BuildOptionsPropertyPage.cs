using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Cosmos.Build.Common;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio;
using System.ComponentModel;

namespace Cosmos.VS.Package
{

	[Guid(Guids.BuildOptionsPropertyPage)]
	public partial class BuildOptionsPropertyPage : ConfigurationBase
	{
		public BuildOptionsPropertyPage()
		{
			InitializeComponent();

			this.comboTarget.Items.AddRange(EnumValue.GetEnumValues(typeof(TargetHost)));
			this.comboFramework.Items.AddRange(EnumValue.GetEnumValues(typeof(Framework)));

		    this.textOutputPath.TextChanged +=
		        delegate(Object sender, EventArgs e)
		            {
		                OutputPath = textOutputPath.Text;
		                IsDirty = true;
                    };
			this.comboTarget.SelectedIndexChanged += 
                delegate(Object sender, EventArgs e)
                    {
                        BuildTarget = (TargetHost)comboTarget.SelectedIndex;
                        IsDirty = true;
                    };
			this.comboFramework.SelectedIndexChanged +=
                delegate(Object sender, EventArgs e)
                    {
                        ChoosenFramework = (Framework)comboFramework.SelectedIndex;
                        IsDirty = true;
                    };
			this.checkUseInternalAssembler.CheckedChanged += 
                delegate(Object sender, EventArgs e)
                    {
                        UseInternalAssembler = checkUseInternalAssembler.Checked;
                        IsDirty = true;
                    };
		}

		protected override void FillProperties()
		{
			base.IgnoreDirty = true;
			base.FillProperties();

			//TODO: fill in properties
		    OutputPath = GetSetting("OutputPath");
		    BuildTarget = 
                (TargetHost)GetEnumValue(typeof(TargetHost), GetSetting("BuildTarget"));
		    ChoosenFramework =
		        (Framework)GetEnumValue(typeof(Framework), GetSetting("Framework"));
		    UseInternalAssembler = 
                bool.Parse(GetSetting("UseInternalAssembler") ?? "false");

			base.IgnoreDirty = false;
		}

		private void buttonOutputBrowse_Click(object sender, EventArgs e)
		{
			String folderPath = String.Empty;
			FolderBrowserDialog dialog;
			dialog = new FolderBrowserDialog();
			dialog.ShowNewFolderButton = true;

			folderPath = textOutputPath.Text;
			if( (String.IsNullOrEmpty(folderPath) == false) && (folderPath.IndexOfAny(System.IO.Path.GetInvalidPathChars()) == -1) )
			{
				if( System.IO.Path.IsPathRooted(folderPath) == false )
				{ folderPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(this.Project.FullName), folderPath); }

				while( (System.IO.Directory.Exists(folderPath)==false) && (String.IsNullOrEmpty(folderPath) == false))
				{
					Int32 index = -1;
					index = folderPath.IndexOfAny(new Char[]{System.IO.Path.PathSeparator, System.IO.Path.AltDirectorySeparatorChar});
					if( index > -1)
					{
						folderPath = folderPath.Substring(0, index-1);
					}else{folderPath = String.Empty;}
				}

				if(String.IsNullOrEmpty(folderPath) == true)
				{ folderPath = System.IO.Path.GetDirectoryName(this.Project.FullName); }
			}else{
				folderPath = System.IO.Path.GetDirectoryName(this.Project.FullName);
			}

			dialog.SelectedPath = folderPath;

			dialog.Description = "Select build output path";

			if(dialog.ShowDialog() == DialogResult.OK)
			{ textOutputPath.Text = dialog.SelectedPath; }
		}

        protected string mOutputPath;
        [SRCategoryAttribute("Category")]
        [DisplayName("Output Path")]
        [SRDescriptionAttribute("Description")]
        public string OutputPath
        {
            get { return mOutputPath; }
            set
            {
                string tmp = value;

                tmp = String.IsNullOrEmpty(value) ? String.Empty : value;

                if (PropertyTable.ContainsKey("OutputPath"))
                {
                    PropertyTable["OutputPath"] = tmp;
                }
                else
                {
                    base.PropertyTable.Add("OutputPath", tmp);
                }

                mOutputPath = tmp;
                textOutputPath.Text = tmp;
            }
        }

	    protected TargetHost mBuildTarget;
	    [SRCategoryAttribute("Category")]
	    [DisplayName("Build Target")]
	    [SRDescriptionAttribute("Description")]
	    public TargetHost BuildTarget
	    {
            get { return mBuildTarget; }
            set
            {
                if (PropertyTable.ContainsKey("BuildTarget"))
                {
                    PropertyTable["BuildTarget"] = Enum.GetName(typeof(TargetHost), value);
                }
                else
                {
                    PropertyTable.Add("BuildTarget", Enum.GetName(typeof(TargetHost), value));
                }

                mBuildTarget = value;
                comboTarget.SelectedIndex = (value == null) ? -1 : (int)value;
            }
	    }

	    protected Framework mChoosenFramework;
	    [SRCategoryAttribute("Category")]
	    [DisplayName("Framework")]
	    [SRDescriptionAttribute("Description")]
	    public Framework ChoosenFramework
	    {
            get { return mChoosenFramework; }
            set
            {
                if (PropertyTable.ContainsKey("Framework"))
                {
                    PropertyTable["Framework"] = Enum.GetName(typeof (Framework), value);
                }
                else
                {
                    PropertyTable.Add("Framework", Enum.GetName(typeof(Framework), value));
                }

                mChoosenFramework = value;
                comboFramework.SelectedIndex = (value == null) ? -1 : (int)value;
            }
	    }

	    protected bool mUseInternalAssembler;
	    [SRCategoryAttribute("Category")]
	    [DisplayName("Use Internal Assembler")]
	    [SRDescriptionAttribute("Description")]
	    public bool UseInternalAssembler
	    {
            get { return mUseInternalAssembler; }
            set
            {
                if (PropertyTable.ContainsKey("UseInternalAssembler"))
                {
                    PropertyTable["UseInternalAssembler"] = value.ToString();
                }
                else
                {
                    PropertyTable.Add("UseInternalAssembler", value.ToString());
                }

                mUseInternalAssembler = value;
                checkUseInternalAssembler.Checked = value;
            }
	    }
	}
}
