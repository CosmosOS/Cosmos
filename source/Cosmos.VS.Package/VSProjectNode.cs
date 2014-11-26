using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Project;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using Microsoft.VisualStudio.Shell.Interop;

namespace Cosmos.VS.Package {
  public class VSProjectNode : ProjectNode {
    internal static int imageIndex;
    public override int ImageIndex {
      get { return imageIndex; }
    }

    // we hold reference, that it is not collected by GC, get still events
    EnvDTE.BuildEvents buildEvents;

    protected override bool SupportsProjectDesigner {
      get { return true; }
      set { }
    }

    public VSProjectNode(VSProject package) {
      LogUtility.LogString("Entering Cosmos.VS.Package.VSProjectNode.ctor(VSProject)");
      try {
        this.Package = package;

        var dte = (EnvDTE.DTE)((IServiceProvider)this.Package).GetService(typeof(EnvDTE.DTE));
        buildEvents = dte.Events.BuildEvents;
        buildEvents.OnBuildProjConfigDone += buildEvents_OnBuildProjConfigDone;

        imageIndex = this.ImageHandler.ImageList.Images.Count;

        foreach (Image img in imageList.Images) {
          this.ImageHandler.AddImage(img);
        }
      } catch (Exception E) {
        LogUtility.LogException(E);
      } finally {
        LogUtility.LogString("Exiting Cosmos.VS.Package.VSProjectNode.ctor(VSProject)");
      }
    }

    void buildEvents_OnBuildProjConfigDone(string Project, string ProjectConfig, string Platform, string SolutionConfig, bool Success)
    {
        if (false == Success)
        {
            var dte = (EnvDTE.DTE)((IServiceProvider)this.Package).GetService(typeof(EnvDTE.DTE));
            dte.DTE.ExecuteCommand("Build.Cancel");
        }
    }

    public override MSBuildResult Build(uint vsopts, string config, IVsOutputWindowPane output, string target) {
      // remove it, no really function in it Trivalik
      //var xReferenceContainer = GetReferenceContainer();

      return base.Build(vsopts, config, output, target);
    }

    internal override void BuildAsync(uint vsopts, string config, IVsOutputWindowPane output, string target, Action<MSBuildResult, string> uiThreadCallback) {
      var xSolutionBuildManager = (IVsSolutionBuildManager)this.GetService(typeof(IVsSolutionBuildManager));
      var xSolution = (IVsSolution)this.GetService(typeof(IVsSolution));
      if (xSolutionBuildManager != null && xSolution != null) {
        IVsHierarchy xStartupProj;
        xSolutionBuildManager.get_StartupProject(out xStartupProj);
        if (xStartupProj != null) {
          var xProj = xStartupProj as IVsProject3;
          Guid xGuid;
          xSolution.GetGuidOfProject(xStartupProj, out xGuid);
          if (xGuid != Guid.Empty) {
            if (xGuid != this.ProjectIDGuid) {
              uiThreadCallback(MSBuildResult.Successful, "Skipped");
              output.OutputStringThreadSafe("Project skipped, as it's not necessary for running\r\n\r\n");
              return;
            }
          }
        }
      }
      base.BuildAsync(vsopts, config, output, target, uiThreadCallback);
    }

    protected override MSBuildResult InvokeMsBuild(string target) {
      // if the project is not set as startup project, don't build the iso

      return base.InvokeMsBuild(target);
    }

    private static ImageList imageList;

    protected override ConfigProvider CreateConfigProvider() {
      return new VsConfigProvider(this);
    }

    // See comments in CosmosPage.cs about this.
    //protected override Guid[] GetConfigurationDependentPropertyPages() {
    //  return new Guid[] { 
    //    typeof(CosmosPage).GUID 
    // };
    //}

    protected override Guid[] GetConfigurationIndependentPropertyPages() {
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

    static VSProjectNode() {
      imageList = Utilities.GetImageList(typeof(VSProjectNode).Assembly.GetManifestResourceStream("Cosmos.VS.Package.Resources.CosmosProjectNode.bmp"));
    }

    public override Guid ProjectGuid {
      get { return Guids.guidProjectFactory; }
    }

    public override string ProjectType {
      get { return "CosmosProjectType"; }
    }

    public override void AddFileFromTemplate(
        string source, string target) {
      LogUtility.LogString("Entering Cosmos.VS.Package.VSProjectNode.AddFileFromTemplate('{0}', '{1}')", source, target);
      try {
        string nameSpace =
            this.FileTemplateProcessor.GetFileNamespace(target, this);
        string className = Path.GetFileNameWithoutExtension(target);

        this.FileTemplateProcessor.AddReplace("$nameSpace$", nameSpace);
        this.FileTemplateProcessor.AddReplace("$className$", className);

        this.FileTemplateProcessor.UntokenFile(source, target);
        this.FileTemplateProcessor.Reset();
      } catch (Exception E) {
        LogUtility.LogException(E);
      } finally {
        LogUtility.LogString("Exiting Cosmos.VS.Package.VSProjectNode.AddFileFromTemplate");
      }
    }
  }
}
