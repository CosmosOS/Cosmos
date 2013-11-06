// The symbol defined below enables method call on LogUtility. Should you wish to disable logging
// please DO NOT remove this code line and DO comment it out.
#define FULL_DEBUG
using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using System.IO;
using Microsoft.Win32;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio;

namespace Cosmos.VS.Package {
  /// This is the class that implements the package exposed by this assembly.
  ///
  /// The minimum requirement for a class to be considered a valid package for Visual Studio
  /// is to implement the IVsPackage interface and register itself with the shell.
  /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
  /// to do it: it derives from the Package class that provides the implementation of the 
  /// IVsPackage interface and uses the registration attributes defined in the framework to 
  /// register itself and its components with the shell.
  //
  // This attribute tells the registration utility (regpkg.exe) that this class needs
  // to be registered as package.
  [PackageRegistration(UseManagedResourcesOnly = true)]
  // A Visual Studio component can be registered under different regitry roots; for instance
  // when you debug your package you want to register it in the experimental hive. This
  // attribute specifies the registry root to use if no one is provided to regpkg.exe with
  // the /root switch.
  [DefaultRegistryRoot("Software\\Microsoft\\VisualStudio\\12.0")]
  // This attribute is used to register the informations needed to show the this package
  // in the Help/About dialog of Visual Studio.
  [InstalledProductRegistration("Cosmos Visual Studio Integration Package", "www.gocosmos.org", "1.0", IconResourceID = 400,
              LanguageIndependentName = "Cosmos Visual Studio Integration Package")]
  // In order be loaded inside Visual Studio in a machine that has not the VS SDK installed, 
  // package needs to have a valid load key (it can be requested at 
  // http://msdn.microsoft.com/vstudio/extend/). This attributes tells the shell that this 
  // package has a load key embedded in its resources.
  [ProvideLoadKey("Standard", "1.0", "Cosmos Visual Studio Integration Package", "Cosmos", 1001)]
  //[ProvideProjectFactory(
  //  typeof(VSProjectFactory),
  //  "Cosmos", // This is the overall group name in new project on left side
  //  "Cosmos Project Files (*.Cosmos);*.Cosmos",
  //  "Cosmos", "Cosmos",
  //  @"..\Templates\Projects\CosmosProject"
  //  , LanguageVsTemplate = "CosmosProject"
  //  , NewProjectRequireNewFolderVsTemplate = false)]

  // Property Pages
  [ProvideObject(typeof(CosmosPage), RegisterUsing = RegistrationMethod.CodeBase)]

  [Guid(Guids.guidProjectPkgString)]
  public sealed class VSProject : ProjectPackage, IVsInstalledProduct {
    /// Default constructor of the package.
    /// Inside this method you can place any initialization code that does not require 
    /// any Visual Studio service because at this point the package object is created but 
    /// not sited yet inside Visual Studio environment. The place to do all the other 
    /// initialization is the Initialize method.
    public VSProject() {
      LogUtility.LogString("Cosmos.VS.Package.VSProject instanciated");
    }

    // This is used in the MSBuild files to locate Cosmos tasks
    // Will likely be used by other things in the future as well
    private void SetCosmosVar() {
      // MtW: we can just use typeof(VSProject).Assembly.Location
      //Trace.WriteLine("Todo: implement set cosmos var, or find something else for it..");
      //string xPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
      //int xPos = xPath.LastIndexOf(@"\source2\", StringComparison.InvariantCultureIgnoreCase);
      //xPath = xPath.Substring(0, xPos);

      //  var xPath = @"E:\Cosmos";
      //System.Environment.SetEnvironmentVariable("Cosmos", xPath, EnvironmentVariableTarget.User);
    }

    /////////////////////////////////////////////////////////////////////////////
    // Overriden Package Implementation

    /// Initialization of the package; this method is called right after the package is sited, so this is the place
    /// where you can put all the initilaization code that rely on services provided by VisualStudio.
    protected override void Initialize() {
      LogUtility.LogString("Cosmos.VS.Package initializing");
      try {
        SetCosmosVar();
        base.Initialize();
        this.RegisterProjectFactory(new VSProjectFactory(this));
        LogUtility.LogString("Cosmos.VS.Package successfully initialized");
      }
      catch (Exception E) {
        LogUtility.LogException(E);
      }
    }

    //[InstalledProductRegistration(false, "Cosmos Visual Studio Integration Package", "www.gocosmos.org", "1.0", IconResourceID = 400,
    //               LanguageIndependentName = "Cosmos Visual Studio Integration Package")]
    int IVsInstalledProduct.IdBmpSplash(out uint pIdBmp) {
      pIdBmp = 400;
      return VSConstants.S_OK;
    }

    int IVsInstalledProduct.IdIcoLogoForAboutbox(out uint pIdIco) {
      pIdIco = 400;
      return VSConstants.S_OK;
    }

    int IVsInstalledProduct.OfficialName(out string pbstrName) {
      pbstrName = "Cosmos";
      return VSConstants.S_OK;
    }

    int IVsInstalledProduct.ProductDetails(out string pbstrProductDetails) {
      pbstrProductDetails = "www.goCosmos.org";
      return VSConstants.S_OK;
    }

    int IVsInstalledProduct.ProductID(out string pbstrPID) {
      pbstrPID = "User Kit";
      return VSConstants.S_OK;
    }

    public override string ProductUserContext {
      get { throw new NotImplementedException(); }
    }
  }
}