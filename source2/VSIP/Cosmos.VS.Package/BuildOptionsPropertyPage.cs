using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Cosmos.Builder.Common;
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
                delegate(Object sender, EventArgs e) { OutputPath = textOutputPath.Text; };
			this.comboTarget.SelectedIndexChanged += 
                delegate(Object sender, EventArgs e) { Target = (TargetHost)comboTarget.SelectedIndex; };
			this.comboFramework.SelectedIndexChanged += 
                delegate(Object sender, EventArgs e) { this.IsDirty = true; };
			this.checkUseInternalAssembler.CheckedChanged += 
                delegate(Object sender, EventArgs e) { this.IsDirty = true; };
		}

		protected override void FillProperties()
		{
			base.IgnoreDirty = true;
			base.FillProperties();

			//TODO: fill in properties

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

				while( (System.IO.Directory.Exists(folderPath)==false) || (String.IsNullOrEmpty(folderPath) == true))
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
        [DisplayName("Target")]
        [SRDescriptionAttribute("Description")]
        public string OutputPath
        {
            get { return mOutputPath; }
            set
            {
                mOutputPath = value;

                if (PropertyTable.ContainsKey("OutputPath"))
                {
                    PropertyTable["OutputPath"] = value;
                }
                else
                {
                    base.PropertyTable.Add("OutputPath", value);
                }
                
                this.IsDirty = true;
            }
        }

	    protected TargetHost mTarget;
	    [SRCategoryAttribute("Category")]
	    [DisplayName("Target")]
	    [SRDescriptionAttribute("Description")]
	    public TargetHost Target
	    {
            get { return mTarget; }
            set
            {
                mTarget = value;

                if (PropertyTable.ContainsKey("BuildTarget"))
                {
                    PropertyTable["BuildTarget"] = Enum.GetName(typeof(TargetHost), value);
                }
                else
                {
                    PropertyTable.Add("BuildTarget", Enum.GetName(typeof(TargetHost), value));
                }

                this.IsDirty = true;
            }
	    }
	}
}
