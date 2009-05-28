using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace Cosmos.Debugger.VSRegister {
    //Note - This can maybe be moved to the COM registration routine, except that it appears 
    //the debugger does not have to be fully registered as a COM lib. But still this could
    //be moved somewhere else, maybe into the VSIP pkg, or install routine.
    class Program {
      static void Main(string[] args) {
          Register();
      }

      static protected string GuidStr(Type aType) {
          return "{" + aType.GUID.ToString().ToUpper() + "}";
      }

      static protected void RegisterCLSID(RegistryKey aKey, string aASMName, Type aType) {
        var xKey = aKey.CreateSubKey(@"CLSID\" + GuidStr(aType));
        ////TODO: Extract from class directly
        xKey.SetValue("Assembly", aASMName);
        xKey.SetValue("Class", aType.FullName);
        //TODO: Get windows system32 path from API
        xKey.SetValue("InprocServer32", @"c:\windows\system32\mscoree.dll");
        xKey.SetValue("CodeBase", aType.Assembly.Location);
      }

      static protected void RegisterRoot(RegistryKey aRootKey, string aVSVer) {
        var xVSKey = aRootKey.OpenSubKey(@"Software\Microsoft\VisualStudio\" + aVSVer, true);
        RegistryKey xKey;

        xKey = xVSKey.CreateSubKey(@"AD7Metrics\Engine");
        xKey = xKey.CreateSubKey(Cosmos.Debugger.Common.Consts.EngineGUID);
        xKey.SetValue("CLSID", GuidStr(typeof(Cosmos.Debugger.VSDebugEngine.AD7Engine)));
        xKey.SetValue("ProgramProvider", GuidStr(typeof(Cosmos.Debugger.VSDebugEngine.AD7ProgramProvider)));
        xKey.SetValue("Attach", 1);
        xKey.SetValue("AddressBP", 0);
        xKey.SetValue("AutoSelectPriority", 4);
        xKey.SetValue("CallstackBP", 1);
        xKey.SetValue("Name", "Cosmos Debug Engine");
        // Default VS Port Supplier. I think we bypass this anyways? Or it reads from DebugEngine?
        // I think the former
        xKey.SetValue("PortSupplier", "{708C1ECA-FF48-11D2-904F-00C04FA302A1}");

        xKey = xKey.CreateSubKey("IncompatibleList");
        xKey.SetValue("guidCOMPlusNativeEng", "{92EF0900-2251-11D2-B72E-0000F87572EF}");
        xKey.SetValue("guidCOMPlusOnlyEng", "{449EC4CC-30D2-4032-9256-EE18EB41B62B}");
        xKey.SetValue("guidNativeOnlyEng", "{449EC4CC-30D2-4032-9256-EE18EB41B62B}");
        xKey.SetValue("guidScriptEng", "{F200A7E7-DEA5-11D0-B854-00A0244A1DE2}");

        RegisterCLSID(xVSKey, "Cosmos.Debugger.VSDebugEngine", typeof(Cosmos.Debugger.VSDebugEngine.AD7Engine));
        RegisterCLSID(xVSKey, "Cosmos.Debugger.VSDebugEngine", typeof(Cosmos.Debugger.VSDebugEngine.AD7ProgramProvider));
      }

      // Note: On x64 some registry paths are different. This routine does not
      // currently handle them.
      static public void Register() {
          RegisterRoot(Registry.LocalMachine, "9.0");
          RegisterRoot(Registry.CurrentUser, @"9.0exp\Configuration");
      }
    }
}
