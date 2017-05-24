using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE;

using Cosmos.Build.Common;
using Cosmos.VS.ProjectSystem.PropertyPages;

namespace Cosmos.VS.ProjectSystem
{
    public class CosmosProjectNode : ProjectNode, IVsReferenceManagerUser
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
            //mBuildEvents.OnBuildProjConfigDone += OnBuildProjConfigDone;

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

        internal string AssemblySearchPaths => CosmosPaths.Kernel;

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

        public override int AddProjectReference()
        {
            var referenceManager = this.GetService(typeof(SVsReferenceManager)) as IVsReferenceManager;
            if (referenceManager != null)
            {
                referenceManager.ShowReferenceManager(
                    this,
                    SR.GetString(SR.AddReferenceDialogTitle),
                    "VS.ReferenceManager",
                    VSConstants.ProjectReferenceProvider_Guid,
                    false);
                return VSConstants.S_OK;
            }
            else
            {
                return VSConstants.E_NOINTERFACE;
            }
        }

        protected override void SetMSBuildProjectProperty(string propertyName, string propertyValue)
        {
            var xPropertyGroupsCount = BuildProject.Xml.PropertyGroups.Count;

            if (xPropertyGroupsCount == 0)
            {
                throw new Exception("The Cosmos project is invalid.");
            }

            if (xPropertyGroupsCount == 1)
            {
                var xPropertyGroup = BuildProject.Xml.AddPropertyGroup();
                xPropertyGroup.SetProperty(propertyName, propertyValue);
            }
            else
            {
                BuildProject.Xml.PropertyGroups.ElementAt(xPropertyGroupsCount - 1).SetProperty(propertyName, propertyValue);
            }
        }

        #region IVsReferenceManagerUser methods

        public void ChangeReferences(uint operation, IVsReferenceProviderContext changedContext)
        {
            var op = (__VSREFERENCECHANGEOPERATION)operation;
            __VSREFERENCECHANGEOPERATIONRESULT result;

            try
            {
                if (op == __VSREFERENCECHANGEOPERATION.VSREFERENCECHANGEOPERATION_ADD)
                {
                    result = this.AddReferences(changedContext);
                }
                else
                {
                    result = this.RemoveReferences(changedContext);
                }
            }
            catch (InvalidOperationException e)
            {
                System.Diagnostics.Debug.Fail(e.ToString());
                result = __VSREFERENCECHANGEOPERATIONRESULT.VSREFERENCECHANGEOPERATIONRESULT_DENY;
            }

            if (result == __VSREFERENCECHANGEOPERATIONRESULT.VSREFERENCECHANGEOPERATIONRESULT_DENY)
            {
                throw new InvalidOperationException();
            }
        }

        public Array GetProviderContexts()
        {
            var referenceManager = this.GetService(typeof(SVsReferenceManager)) as IVsReferenceManager;
            return this.GetProviderContexts(referenceManager).ToArray();
        }

        protected virtual IEnumerable<IVsReferenceProviderContext> GetProviderContexts(IVsReferenceManager mgr)
        {
            var ctxt = CreateAssemblyReferenceProviderContext(mgr);
            if (ctxt != null)
            {
                yield return ctxt;
            }
            ctxt = CreateProjectReferenceProviderContext(mgr);
            if (ctxt != null)
            {
                yield return ctxt;
            }
            ctxt = CreateFileReferenceProviderContext(mgr);
            if (ctxt != null)
            {
                yield return ctxt;
            }
        }

        protected virtual IVsReferenceProviderContext CreateAssemblyReferenceProviderContext(IVsReferenceManager mgr)
        {
            var moniker = TargetFrameworkMoniker.FullName;
            if (string.IsNullOrEmpty(moniker))
            {
                return null;
            }

            var context = (IVsAssemblyReferenceProviderContext)mgr.CreateProviderContext(VSConstants.AssemblyReferenceProvider_Guid);
            context.TargetFrameworkMoniker = moniker;
            context.AssemblySearchPaths = AssemblySearchPaths;

            var referenceContainer = this.GetReferenceContainer();
            var references = referenceContainer
                .EnumReferences()
                .OfType<AssemblyReferenceNode>();
            foreach (var reference in references)
            {
                var newReference = (IVsAssemblyReference)context.CreateReference();
                newReference.FullPath = reference.Url ?? reference.AssemblyName.ToString();
                newReference.Name = reference.AssemblyName.Name;
                newReference.AlreadyReferenced = true;
                context.AddReference(newReference);
            }

            return context;
        }

        protected virtual IVsReferenceProviderContext CreateProjectReferenceProviderContext(IVsReferenceManager mgr)
        {
            var context = (IVsProjectReferenceProviderContext)mgr.CreateProviderContext(VSConstants.ProjectReferenceProvider_Guid);
            context.CurrentProject = this;

            var referenceContainer = this.GetReferenceContainer();
            var references = referenceContainer
                .EnumReferences()
                .OfType<ProjectReferenceNode>();
            foreach (var reference in references)
            {
                var newReference = (IVsProjectReference)context.CreateReference();
                newReference.FullPath = reference.Url;
                newReference.Name = reference.ReferencedProjectName;
                newReference.Identity = reference.ReferencedProjectGuid.ToString("B");
                newReference.ReferenceSpecification = reference.ReferencedProjectGuid.ToString();
                newReference.AlreadyReferenced = true;
                context.AddReference(newReference);
            }

            return context;
        }

        protected virtual IVsReferenceProviderContext CreateFileReferenceProviderContext(IVsReferenceManager mgr)
        {
            var exts = AddReferenceExtensions;
            if (string.IsNullOrEmpty(exts))
            {
                return null;
            }

            var context = (IVsFileReferenceProviderContext)mgr.CreateProviderContext(VSConstants.FileReferenceProvider_Guid);
            context.BrowseFilter = AddReferenceExtensions.Replace('|', '\0') + "\0";

            var referenceContainer = this.GetReferenceContainer();
            var references = referenceContainer
                .EnumReferences()
                .Where(n => !(n is AssemblyReferenceNode) && !(n is ProjectReferenceNode));
            foreach (var reference in references)
            {
                var newReference = (IVsFileReference)context.CreateReference();
                newReference.FullPath = reference.Url;
                newReference.AlreadyReferenced = true;
                context.AddReference(newReference);
            }

            return context;
        }

        protected virtual string AddReferenceExtensions
        {
            get
            {
                return "Dynamic Link Libraries (*.dll)|*.dll|All Files (*.*)|*.*";
            }
        }

        private __VSREFERENCECHANGEOPERATIONRESULT AddReferences(IVsReferenceProviderContext context)
        {
            var addedReferences = this.GetAddedReferences(context);

            var referenceContainer = this.GetReferenceContainer();
            foreach (var selectorData in addedReferences)
            {
                referenceContainer.AddReferenceFromSelectorData(selectorData);
            }

            return __VSREFERENCECHANGEOPERATIONRESULT.VSREFERENCECHANGEOPERATIONRESULT_ALLOW;
        }

        protected virtual IEnumerable<VSCOMPONENTSELECTORDATA> GetAddedReferences(IVsReferenceProviderContext context)
        {
            var addedReferences = Enumerable.Empty<VSCOMPONENTSELECTORDATA>();

            if (context.ProviderGuid == VSConstants.AssemblyReferenceProvider_Guid)
            {
                addedReferences = GetAddedReferences(context as IVsAssemblyReferenceProviderContext);
            }
            else if (context.ProviderGuid == VSConstants.ProjectReferenceProvider_Guid)
            {
                addedReferences = GetAddedReferences(context as IVsProjectReferenceProviderContext);
            }
            else if (context.ProviderGuid == VSConstants.FileReferenceProvider_Guid)
            {
                addedReferences = GetAddedReferences(context as IVsFileReferenceProviderContext);
            }

            return addedReferences;
        }

        private __VSREFERENCECHANGEOPERATIONRESULT RemoveReferences(IVsReferenceProviderContext context)
        {
            var removedReferences = this.GetRemovedReferences(context);

            foreach (var refNode in removedReferences)
            {
                refNode.Remove(true /* delete from storage*/);
            }

            return __VSREFERENCECHANGEOPERATIONRESULT.VSREFERENCECHANGEOPERATIONRESULT_ALLOW;
        }

        protected virtual IEnumerable<ReferenceNode> GetRemovedReferences(IVsReferenceProviderContext context)
        {
            var removedReferences = Enumerable.Empty<ReferenceNode>();

            if (context.ProviderGuid == VSConstants.AssemblyReferenceProvider_Guid)
            {
                removedReferences = GetRemovedReferences(context as IVsAssemblyReferenceProviderContext);
            }
            else if (context.ProviderGuid == VSConstants.ProjectReferenceProvider_Guid)
            {
                removedReferences = GetRemovedReferences(context as IVsProjectReferenceProviderContext);
            }
            else if (context.ProviderGuid == VSConstants.FileReferenceProvider_Guid)
            {
                removedReferences = GetRemovedReferences(context as IVsFileReferenceProviderContext);
            }

            return removedReferences;
        }

        private IEnumerable<VSCOMPONENTSELECTORDATA> GetAddedReferences(IVsAssemblyReferenceProviderContext context)
        {
            var selectedReferences = context
                .References
                .OfType<IVsAssemblyReference>()
                .Select(reference => new VSCOMPONENTSELECTORDATA()
                {
                    type = VSCOMPONENTTYPE.VSCOMPONENTTYPE_ComPlus,
                    bstrFile = reference.FullPath,
                    bstrTitle = reference.Name
                });

            return selectedReferences;
        }

        private IEnumerable<AssemblyReferenceNode> GetRemovedReferences(IVsAssemblyReferenceProviderContext context)
        {
            var selectedReferences = context
                .References
                .OfType<IVsAssemblyReference>()
                .Select(fileRef => fileRef.FullPath);

            var referenceContainer = this.GetReferenceContainer();
            var references = referenceContainer
                .EnumReferences()
                .OfType<AssemblyReferenceNode>()
                .Where(refNode => selectedReferences.Contains(refNode.Url));

            return references;
        }

        private IEnumerable<VSCOMPONENTSELECTORDATA> GetAddedReferences(IVsProjectReferenceProviderContext context)
        {
            var selectedReferences = context
                .References
                .OfType<IVsProjectReference>()
                .Select(reference => new VSCOMPONENTSELECTORDATA()
                {
                    type = VSCOMPONENTTYPE.VSCOMPONENTTYPE_Project,
                    bstrTitle = reference.Name,
                    bstrFile = new FileInfo(reference.FullPath).Directory.FullName,
                    bstrProjRef = reference.ReferenceSpecification
                });

            return selectedReferences;
        }

        private IEnumerable<ReferenceNode> GetRemovedReferences(IVsProjectReferenceProviderContext context)
        {
            var selectedReferences = context
                .References
                .OfType<IVsProjectReference>()
                .Select(asmRef => new Guid(asmRef.Identity));

            var referenceContainer = this.GetReferenceContainer();
            var references = referenceContainer
                .EnumReferences()
                .OfType<ProjectReferenceNode>()
                .Where(refNode => selectedReferences.Contains(refNode.ReferencedProjectGuid));

            return references;
        }

        private IEnumerable<VSCOMPONENTSELECTORDATA> GetAddedReferences(IVsFileReferenceProviderContext context)
        {
            var selectedReferences = context
                .References
                .OfType<IVsFileReference>()
                .Select(reference => new VSCOMPONENTSELECTORDATA()
                {
                    type = VSCOMPONENTTYPE.VSCOMPONENTTYPE_File,
                    bstrFile = reference.FullPath
                });

            return selectedReferences;
        }

        private IEnumerable<ReferenceNode> GetRemovedReferences(IVsFileReferenceProviderContext context)
        {
            var selectedReferences = context
                .References
                .OfType<IVsFileReference>()
                .Select(fileRef => fileRef.FullPath);

            var referenceContainer = this.GetReferenceContainer();
            var references = referenceContainer
                .EnumReferences()
                .OfType<ReferenceNode>()
                .Where(refNode => selectedReferences.Contains(refNode.Url));

            return references;
        }

        #endregion
    }
}