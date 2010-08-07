using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Project;

namespace Cosmos.VS.Package
{
    [Guid(Guids.KernelPage)]
    public partial class KernelPage : CustomPropertyPage
    {
        public KernelPage()
        {
            InitializeComponent();
            comboKernelAssembly.SelectedIndexChanged += delegate(Object sender, EventArgs e)
            {
                IsDirty=true;
            };
            
        }

        //protected DebugProperties mProps = new DebugProperties();
        //public override PropertiesBase Properties
        //{
        //    get { return mProps; }
        //}

        protected override void FillProperties()
        {
            base.FillProperties();

            var xReferenceContainer = ProjectMgr.GetReferenceContainer();
            if (xReferenceContainer != null)
            {
                var xReferences = xReferenceContainer.EnumReferences().ToArray();
                comboKernelAssembly.DataSource = xReferences;
                var xId = Project.Project.BuildProject.GetEvaluatedProperty("KernelAssemblyReferenceId");
                comboKernelAssembly.SelectedItem = (from item in xReferences
                                                    where item.ReferenceIdentifier == xId
                                                    select item).SingleOrDefault();
            }
        }

        public override void ApplyChanges()
        {
            base.ApplyChanges();
            var xItem = (ReferenceNode)comboKernelAssembly.SelectedItem;
            
            Project.Project.BuildProject.SetProperty("KernelAssemblyReferenceId", xItem.ReferenceIdentifier);
            Project.Project.BuildProject.SetProperty("KernelAssemblyReference", xItem.AssemblyFilename);
        }

    }
}