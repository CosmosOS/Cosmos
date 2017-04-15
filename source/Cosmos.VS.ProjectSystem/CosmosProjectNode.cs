using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Cosmos.VS.ProjectSystem.PropertyPages;
using EnvDTE;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell.Interop;

namespace Cosmos.VS.ProjectSystem
{
    public class CosmosProjectNode : ProjectNode
    {
        private static readonly ImageList mImageList;
        internal static int mImageIndex;
        readonly BuildEvents mBuildEvents;

        protected override bool SupportsProjectDesigner
        {
            get { return true; }
            set { }
        }

        static CosmosProjectNode()
        {
            Logger.TraceMethod(MethodBase.GetCurrentMethod());

            mImageList = Utilities.GetImageList(typeof(CosmosProjectNode).Assembly.GetManifestResourceStream("Cosmos.VS.ProjectSystem.Resources.CosmosProjectNode.bmp"));
        }

        public CosmosProjectNode(CosmosProjectPackage package)
        {
            Logger.TraceMethod(MethodBase.GetCurrentMethod());

            Package = package;
            var dte = (DTE)((IServiceProvider)Package).GetService(typeof(DTE));
            mBuildEvents = dte.Events.BuildEvents;
            mBuildEvents.OnBuildProjConfigDone += OnBuildProjConfigDone;

            mImageIndex = ImageHandler.ImageList.Images.Count;

            foreach (Image img in mImageList.Images)
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


        //public override int GetProperty(uint itemId, int propId, out object property)
        //{
        //    switch ((__VSHPROPID2)propId)
        //    {
        //        case __VSHPROPID2.VSHPROPID_CfgPropertyPagesCLSIDList:
        //        {
        //            var res = base.GetProperty(itemId, propId, out property);
        //            if (ErrorHandler.Succeeded(res))
        //            {
        //                var guids = GetGuidsFromList(property as string);
        //                guids.RemoveAll(g => CfgSpecificPropertyPagesToRemove.Contains(g));
        //                guids.AddRange(CfgSpecificPropertyPagesToAdd);
        //                property = MakeListFromGuids(guids);
        //            }
        //            return res;
        //        }
        //        case __VSHPROPID2.VSHPROPID_PropertyPagesCLSIDList:
        //        {
        //            var res = base.GetProperty(itemId, propId, out property);
        //            if (ErrorHandler.Succeeded(res))
        //            {
        //                var guids = GetGuidsFromList(property as string);
        //                guids.RemoveAll(g => PropertyPagesToRemove.Contains(g));
        //                guids.AddRange(PropertyPagesToAdd);
        //                property = MakeListFromGuids(guids);
        //            }
        //            return res;
        //        }
        //    }

        //    return base.GetProperty(itemId, propId, out property);
        //}

        public override Guid ProjectGuid => Guids.guidCosmosProjectFactory;

        public override Guid ProjectIDGuid { get; set; }

        public override string ProjectType => "Cosmos";

        public override int ImageIndex => mImageIndex;

        protected override ConfigProvider CreateConfigProvider()
        {
            Logger.TraceMethod(MethodBase.GetCurrentMethod());

            return new CosmosConfigProvider(this);
        }

        private void OnBuildProjConfigDone(string Project, string ProjectConfig, string Platform, string SolutionConfig, bool Success)
        {
            Logger.TraceMethod(MethodBase.GetCurrentMethod());

            if (false == Success)
            {
                var dte = (DTE)((IServiceProvider)Package).GetService(typeof(DTE));
                dte.DTE.ExecuteCommand("Build.Cancel");
            }
        }

        internal override void BuildAsync(uint vsopts, string config, IVsOutputWindowPane output, string target, Action<MSBuildResult, string> uiThreadCallback)
        {
            Logger.TraceMethod(MethodBase.GetCurrentMethod());

            var xSolutionBuildManager = (IVsSolutionBuildManager)GetService(typeof(IVsSolutionBuildManager));
            var xSolution = (IVsSolution)GetService(typeof(IVsSolution));
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
                        if (xGuid != ProjectIDGuid)
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
            Logger.TraceMethod(MethodBase.GetCurrentMethod());

            string nameSpace = FileTemplateProcessor.GetFileNamespace(target, this);
            string className = Path.GetFileNameWithoutExtension(target);

            FileTemplateProcessor.AddReplace("$nameSpace$", nameSpace);
            FileTemplateProcessor.AddReplace("$className$", className);

            FileTemplateProcessor.UntokenFile(source, target);
            FileTemplateProcessor.Reset();
        }
    }
}