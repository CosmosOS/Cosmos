using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Project;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace Cosmos.VS.Package {
  public class VSProjectNode : ProjectNode {
    internal static int imageIndex;
    public override int ImageIndex {
      get { return imageIndex; }
    }

    private VSProject package;

    public VSProjectNode(VSProject package) {
      this.package = package;

      imageIndex = this.ImageHandler.ImageList.Images.Count;

      foreach (Image img in imageList.Images) {
        this.ImageHandler.AddImage(img);
      }
    }

    private static ImageList imageList;

    protected override Guid[] GetConfigurationIndependentPropertyPages() {
      // Default C# property pages
      // Unfortunately just adding them to the list does not work.
      // It causes AV's, but its specific to each page
      // loading and getting confused under a different project type
      //5E9A8AC2-4F34-4521-858F-4C248BA31532 - Application
      //43E38D2E-43B8-4204-8225-9357316137A4 - Services
      //031911C8-6148-4E25-B1B1-44BCA9A0C45C - Reference Paths
      //F8D6553F-F752-4DBF-ACB6-F291B744A792 - Signing
      //1E78F8DB-6C07-4D61-A18F-7514010ABD56 - Build Events
      return new Guid[] {
          typeof(PropPageEnvironment).GUID,
      };
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
      string nameSpace =
          this.FileTemplateProcessor.GetFileNamespace(target, this);
      string className = Path.GetFileNameWithoutExtension(target);

      this.FileTemplateProcessor.AddReplace("$nameSpace$", nameSpace);
      this.FileTemplateProcessor.AddReplace("$className$", className);

      this.FileTemplateProcessor.UntokenFile(source, target);
      this.FileTemplateProcessor.Reset();
    }
  }
}
