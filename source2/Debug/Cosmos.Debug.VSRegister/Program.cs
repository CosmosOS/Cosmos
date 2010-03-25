using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using Cosmos.Debug.VSDebugEngine;
using System.IO;

namespace Cosmos.Debugger.VSRegister
{
    //Note - This can maybe be moved to the COM registration routine, except that it appears 
    //the debugger does not have to be fully registered as a COM lib. But still this could
    //be moved somewhere else, maybe into the VSIP pkg, or install routine.
    class Program
    {
        static void Main(string[] args)
        {
            Register();
        }

        static protected string GuidStr(Type aType)
        {
            return "{" + aType.GUID.ToString().ToUpper() + "}";
        }

        static protected void RegisterCLSID(TextWriter aOut, string aKey, string aASMName, Type aType)
        {
            var xKey = aKey + @"\CLSID\" + GuidStr(aType);
            WriteKey(aOut, xKey);
            ////TODO: Extract from class directly
            WriteValue(aOut, "Assembly", aASMName);
            WriteValue(aOut, "Class", aType.FullName);
            //TODO: Get windows system32 path from API
            WriteValue(aOut, "InprocServer32", @"c:\windows\system32\mscoree.dll");
            WriteValue(aOut, "CodeBase", aType.Assembly.Location);
        }

        private static void WriteKey(TextWriter aOut, string key)
        {
            aOut.WriteLine("[{0}]", key);
        }

        private static void WriteValue(TextWriter aOut, string name, string value)
        {
            if (String.IsNullOrEmpty(name))
            {
                name = "@";
            }
            else
            {
                name = "\"" + name + "\"";
            }
            aOut.WriteLine("{0}=\"{1}\"", name, value.Replace("\\", "\\\\"));
        }

        private static void WriteValue(TextWriter aOut, string name, int value)
        {
            if (String.IsNullOrEmpty(name))
            {
                name = "@";
            }
            else
            {
                name = "\"" + name + "\"";
            }
            aOut.WriteLine("{0}=dword:{1}", name, value.ToString("X8"));
        }

        static protected void RegisterRoot(TextWriter aOut, string aRootKey, string aVSVer)
        {
            var xVSKey = aRootKey + "\\" + @"Software\Microsoft\VisualStudio\" + aVSVer;

            var xKey = xVSKey + @"\AD7Metrics\Engine";
            xKey += "\\{" + AD7Engine.ID + "}";
            WriteKey(aOut, xKey);
            WriteValue(aOut, null, "guidCosmosDebugEngine");
            WriteValue(aOut, "CLSID", GuidStr(typeof(Cosmos.Debug.VSDebugEngine.AD7Engine)));
            WriteValue(aOut, "ProgramProvider", GuidStr(typeof(Cosmos.Debug.VSDebugEngine.AD7ProgramProvider)));
            WriteValue(aOut, "Attach", 0);
            WriteValue(aOut, "AddressBP", 0);
            WriteValue(aOut, "AutoSelectPriority", 4);
            WriteValue(aOut, "CallstackBP", 1);
            WriteValue(aOut, "Name", "Cosmos Debug Engine");
            // Default VS Port Supplier. I think we bypass this anyways? Or it reads from DebugEngine?
            // I think the former
            WriteValue(aOut, "PortSupplier", "{708C1ECA-FF48-11D2-904F-00C04FA302A1}");

            xKey = xKey + @"\IncompatibleList";
            WriteValue(aOut, "guidCOMPlusNativeEng", "{92EF0900-2251-11D2-B72E-0000F87572EF}");
            WriteValue(aOut, "guidCOMPlusOnlyEng", "{449EC4CC-30D2-4032-9256-EE18EB41B62B}");
            WriteValue(aOut, "guidNativeOnlyEng", "{449EC4CC-30D2-4032-9256-EE18EB41B62B}");
            WriteValue(aOut, "guidScriptEng", "{F200A7E7-DEA5-11D0-B854-00A0244A1DE2}");

            RegisterCLSID(aOut, xVSKey, "Cosmos.Debug.VSDebugEngine", typeof(Cosmos.Debug.VSDebugEngine.AD7Engine));
            RegisterCLSID(aOut, xVSKey, "Cosmos.Debug.VSDebugEngine", typeof(Cosmos.Debug.VSDebugEngine.AD7ProgramProvider));
        }

        static public void Register()
        {
            var xStreamWriter = Console.Out;
            {
                xStreamWriter.WriteLine("REGEDIT4");
                xStreamWriter.WriteLine();
                RegisterRoot(xStreamWriter, "HKEY_LOCAL_MACHINE", "9.0");
                RegisterRoot(xStreamWriter, "HKEY_CURRENT_USER", @"9.0Exp\Configuration");
            }
        }
    }
}
