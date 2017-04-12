using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Cosmos.VS.ProjectSystem.PropertyPages;
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

        protected override Guid[] GetConfigurationIndependentPropertyPages()
        {
            Guid[] result = new Guid[1];
            result[0] = typeof(CosmosPage).GUID;
            return result;
        }

        protected override Guid[] GetPriorityProjectDesignerPages()
        {
            Guid[] result = new Guid[1];
            result[0] = typeof(CosmosPage).GUID;
            return result;
        }

        public override Guid ProjectGuid => Guids.guidCosmosProjectFactory;

        public override Guid ProjectIDGuid { get; set; }

        public override string ProjectType => "Cosmos";

        public override int ImageIndex => imageIndex;

        protected override ConfigProvider CreateConfigProvider()
        {
            return new CosmosConfigProvider(this);
        }

        void buildEvents_OnBuildProjConfigDone(string Project, string ProjectConfig, string Platform,
            string SolutionConfig, bool Success)
        {
            if (false == Success)
            {
                var dte = (EnvDTE.DTE)((IServiceProvider)this.Package).GetService(typeof(EnvDTE.DTE));
                dte.DTE.ExecuteCommand("Build.Cancel");
            }
        }

        internal override void BuildAsync(uint vsopts, string config, IVsOutputWindowPane output, string target,
            Action<MSBuildResult, string> uiThreadCallback)
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
    }
}