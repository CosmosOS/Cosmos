using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace XSharp.VS
{
  /// This is the class that implements the package exposed by this assembly.
  ///
  /// The minimum requirement for a class to be considered a valid package for Visual Studio
  /// is to implement the IVsPackage interface and register itself with the shell.
  /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
  /// to do it: it derives from the Package class that provides the implementation of the
  /// IVsPackage interface and uses the registration attributes defined in the framework to
  /// register itself and its components with the shell.
  //
  [PackageRegistration(UseManagedResourcesOnly = true)]
  [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
  [ProvideService(typeof(XSharpLanguageService))]
  [ProvideLanguageExtension(typeof(XSharpLanguageService), ".xs")]
  [ProvideLanguageService(typeof(XSharpLanguageService), "X#", 0, RequestStockColors = true)]
  [Guid(Guids.guidCosmos_VS_XSharpPkgString)]
  public sealed class XSharpPackage : Package, IOleComponent
  {
    uint mComponentID;

    // Default constructor of the package.
    // Inside this method you can place any initialization code that does not require
    // any Visual Studio service because at this point the package object is created but
    // not sited yet inside Visual Studio environment. The place to do all the other
    // initialization is the Initialize method.
    public XSharpPackage()
    {
    }

    /// Initialization of the package; this method is called right after the package is sited, so this is the place
    /// where you can put all the initilaization code that rely on services provided by VisualStudio.
    protected override void Initialize()
    {
      base.Initialize();
      // Proffer the service.
      var serviceContainer = this as IServiceContainer;
      var langService = new XSharpLanguageService();
      langService.SetSite(this);
      serviceContainer.AddService(typeof(XSharpLanguageService), langService, true);

      // Register a timer to call our language service during idle periods.
      var xMgr = GetService(typeof(SOleComponentManager)) as IOleComponentManager;
      if (mComponentID == 0 && xMgr != null)
      {
        OLECRINFO[] crinfo = new OLECRINFO[1];
        crinfo[0].cbSize = (uint)Marshal.SizeOf(typeof(OLECRINFO));
        crinfo[0].grfcrf = (uint)_OLECRF.olecrfNeedIdleTime | (uint)_OLECRF.olecrfNeedPeriodicIdleTime;
        crinfo[0].grfcadvf = (uint)_OLECADVF.olecadvfModal | (uint)_OLECADVF.olecadvfRedrawOff | (uint)_OLECADVF.olecadvfWarningsOff;
        crinfo[0].uIdleTimeInterval = 1000;
        xMgr.FRegisterComponent(this, crinfo, out mComponentID);
      }
    }

    protected override void Dispose(bool disposing)
    {
      if (mComponentID != 0)
      {
        var xMgr = GetService(typeof(SOleComponentManager)) as IOleComponentManager;
        if (xMgr != null)
        {
          xMgr.FRevokeComponent(mComponentID);
        }
        mComponentID = 0;
      }
      base.Dispose(disposing);
    }

    public int FDoIdle(uint grfidlef)
    {
      bool bPeriodic = (grfidlef & (uint)_OLEIDLEF.oleidlefPeriodic) != 0;
      // Use typeof(TestLanguageService) because we need to reference the GUID for our language service.
      LanguageService xService = GetService(typeof(XSharpLanguageService)) as LanguageService;
      if (xService != null)
      {
        xService.OnIdle(bPeriodic);
      }
      return 0;
    }

    public int FContinueMessageLoop(uint uReason, IntPtr pvLoopData, MSG[] pMsgPeeked)
    {
      return 1;
    }

    public int FPreTranslateMessage(MSG[] pMsg)
    {
      return 0;
    }

    public int FQueryTerminate(int fPromptUser)
    {
      return 1;
    }

    public int FReserved1(uint dwReserved, uint message, IntPtr wParam, IntPtr lParam)
    {
      return 1;
    }

    public IntPtr HwndGetWindow(uint dwWhich, uint dwReserved)
    {
      return IntPtr.Zero;
    }

    public void OnActivationChange(IOleComponent pic, int fSameComponent, OLECRINFO[] pcrinfo, int fHostIsActivating, OLECHOSTINFO[] pchostinfo, uint dwReserved) { }
    public void OnAppActivate(int fActive, uint dwOtherThreadID) { }
    public void OnEnterState(uint uStateID, int fEnter) { }
    public void OnLoseActivation() { }
    public void Terminate() { }
  }
}
