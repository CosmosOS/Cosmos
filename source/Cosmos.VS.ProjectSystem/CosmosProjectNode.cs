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
            // Default C# property pages
            // Unfortunately just adding them to the list does not work.
            // It causes AV's, but its specific to each page
            // loading and getting confused under a different project type.
            // Maybe they need to be added as Dependent instead of Independent pages?
            // Adding them as dependent ones is better, but at least build events is added, but is disabled.
            //
            //5E9A8AC2-4F34-4521-858F-4C248BA31532 - Application
            //43E38D2E-43B8-4204-8225-9357316137A4 - Services
            //031911C8-6148-4E25-B1B1-44BCA9A0C45C - Reference Paths
            //F8D6553F-F752-4DBF-ACB6-F291B744A792 - Signing
            //1E78F8DB-6C07-4D61-A18F-7514010ABD56 - Build Events

            return new Guid[] { typeof(CosmosPage).GUID };
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