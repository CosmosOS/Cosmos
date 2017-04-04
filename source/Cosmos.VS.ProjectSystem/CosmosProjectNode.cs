using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Microsoft.Build.Evaluation;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell.Interop;

namespace Cosmos.VS.ProjectSystem
{
    public class CosmosProjectNode : ProjectNode
    {
        private static ImageList imageList;
        internal static int imageIndex;
        EnvDTE.BuildEvents buildEvents;

        protected override bool SupportsProjectDesigner
        {
            get { return true; }
            set { }
        }

        static CosmosProjectNode()
        {
            imageList =
                Utilities.GetImageList(
                    typeof(CosmosProjectNode).Assembly.GetManifestResourceStream(
                        "Cosmos.VS.ProjectSystem.Resources.CosmosProjectNode.bmp"));
        }

        public CosmosProjectNode(CosmosProjectPackage package)
        {
            this.Package = package;
        var dte = (EnvDTE.DTE)((IServiceProvider)this.Package).GetService(typeof(EnvDTE.DTE));
        buildEvents = dte.Events.BuildEvents;
        buildEvents.OnBuildProjConfigDone += buildEvents_OnBuildProjConfigDone;

        imageIndex = ImageHandler.ImageList.Images.Count;

            foreach (Image img in imageList.Images)
            {
                ImageHandler.AddImage(img);
            }
        }

        public override Guid ProjectGuid => Guids.guidCosmosProjectFactory;

        public override Guid ProjectIDGuid { get; set; }

        public override string ProjectType => "Cosmos";

        public override int ImageIndex => imageIndex;

        protected override ConfigProvider CreateConfigProvider()
        {
            return new CosmosConfigProvider(this);
        }

            void buildEvents_OnBuildProjConfigDone(string Project, string ProjectConfig, string Platform, string SolutionConfig, bool Success)
            {
                if (false == Success)
                {
                    var dte = (EnvDTE.DTE)((IServiceProvider)this.Package).GetService(typeof(EnvDTE.DTE));
                    dte.DTE.ExecuteCommand("Build.Cancel");
                }
            }

            internal override void BuildAsync(uint vsopts, string config, IVsOutputWindowPane output, string target, Action<MSBuildResult, string> uiThreadCallback)
            {
                var xSolutionBuildManager = (IVsSolutionBuildManager)this.GetService(typeof(IVsSolutionBuildManager));
                var xSolution = (IVsSolution)this.GetService(typeof(IVsSolution));
                if (xSolutionBuildManager != null && xSolution != null)
                {
                    IVsHierarchy xStartupProj;
                    xSolutionBuildManager.get_StartupProject(out xStartupProj);
                    if (xStartupProj != null)
                    {
                        var xProj = xStartupProj as IVsProject3;
                        Guid xGuid;
                        xSolution.GetGuidOfProject(xStartupProj, out xGuid);
                        if (xGuid != Guid.Empty)
                        {
                            if (xGuid != this.ProjectIDGuid)
                            {
                                uiThreadCallback(MSBuildResult.Successful, "Skipped");
                                output.OutputStringThreadSafe("Project skipped, as it's not necessary for running\r\n\r\n");
                                return;
                            }
                        }
                    }
                }
                base.BuildAsync(vsopts, config, output, target, uiThreadCallback);
            }

            public override void AddFileFromTemplate(string source, string target)
        {
            string nameSpace = FileTemplateProcessor.GetFileNamespace(target, this);
            string className = Path.GetFileNameWithoutExtension(target);

            FileTemplateProcessor.AddReplace("$nameSpace$", nameSpace);
            FileTemplateProcessor.AddReplace("$className$", className);

            FileTemplateProcessor.UntokenFile(source, target);
            FileTemplateProcessor.Reset();
        }

        public override int GetProperty(uint itemId, int propId, out object property)
        {
            int result = base.GetProperty(itemId, propId, out property);
            if (result != VSConstants.S_OK)
                return result;

            switch ((__VSHPROPID2)propId)
            {
                case __VSHPROPID2.VSHPROPID_PropertyPagesCLSIDList:
                    {
                        //Get a semicolon-delimited list of clsids of the configuration-independent property pages  
                        ErrorHandler.ThrowOnFailure(base.GetProperty(itemId, propId, out property));
                        string propertyPagesList = ((string)property).ToUpper(CultureInfo.InvariantCulture);
                        //Remove the property page here  

                        var properties = new List<string>(property.ToString().Split(';'));
                        //properties.Add(typeof(CosmosPage).GUID.ToString("B"));
                        //property = properties.Aggregate("", (a, next) => a + ';' + next).Substring(1);
                        return VSConstants.S_OK;
                    }
                case __VSHPROPID2.VSHPROPID_PriorityPropertyPagesCLSIDList:
                    {
                        // set the order for the project property pages
                        var properties = new List<string>(property.ToString().Split(';'));
                        //properties.Insert(1, typeof(CosmosPage).GUID.ToString("B"));
                        //property = properties.Aggregate("", (a, next) => a + ';' + next).Substring(1);
                        return VSConstants.S_OK;
                    }
                default:
                    break;
            }

            return base.GetProperty(itemId, propId, out property);
        }
    }
}