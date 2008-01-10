using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Specialized;

namespace Cosmos.Build.Windows.Config.MSCorCfg
{
    public class Reflected
    {
        static object Get(object src, string strName, BindingFlags type)
        {
            bool bStatic = (src is Type);
            Type t = bStatic ? (Type)src : src.GetType();
            BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Public |
                                        BindingFlags.DeclaredOnly |
                                        (bStatic ? BindingFlags.Static : BindingFlags.Instance) |
                                        type;
            object target = (bStatic ? null : src);

            return (t.InvokeMember(strName, bindingFlags, null, target, null));
        }

        public static object GetProperty(object src, string strName)
        {
            return (Get(src, strName, BindingFlags.GetProperty));
        }

        public static object GetField(object src, string strName)
        {
            return (Get(src, strName, BindingFlags.GetField));
        }
    }

    public struct AssemInfo
    {
        public readonly string Name;
        public readonly string Locale;
        public readonly string Codebase;
        public readonly string Modified;
        public readonly string OSType;
        public readonly string OSVersion;
        public readonly string ProcType;
        public readonly string PublicKey;
        public readonly string PublicKeyToken;
        public readonly string Version;
        public readonly Fusion.CacheType CacheType;
        public readonly string sCustom;
        public readonly string sFusionName;

        public AssemInfo(object assemInfo)
        {
            Name = (string)Reflected.GetField(assemInfo, "Name");
            Locale = (string)Reflected.GetField(assemInfo, "Locale");
            Codebase = (string)Reflected.GetField(assemInfo, "Codebase");
            Modified = (string)Reflected.GetField(assemInfo, "Modified");
            OSType = (string)Reflected.GetField(assemInfo, "OSType");
            OSVersion = (string)Reflected.GetField(assemInfo, "OSVersion");
            ProcType = (string)Reflected.GetField(assemInfo, "ProcType");
            PublicKey = (string)Reflected.GetField(assemInfo, "PublicKey");
            PublicKeyToken = (string)Reflected.GetField(assemInfo, "PublicKeyToken");
            Version = (string)Reflected.GetField(assemInfo, "Version");
            uint nCacheType = (uint)Reflected.GetField(assemInfo, "nCacheType");
            CacheType = (Fusion.CacheType)nCacheType;
            sCustom = (string)Reflected.GetField(assemInfo, "sCustom");
            sFusionName = (string)Reflected.GetField(assemInfo, "sFusionName");
        }
    }

    public class Fusion
    {
        public enum CacheType
        {
            Zap = 0x1,
            GAC = 0x2,
            Download = 0x4
        }

        static Type FusionType;

        static Fusion()
        {
            Assembly a = System.Reflection.Assembly.Load("mscorcfg, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            FusionType = a.GetType("Microsoft.CLRAdmin.Fusion");
        }

        public static String GetCacheTypeString(UInt32 nFlag)
        {
            object[] args = new object[] { nFlag };
            BindingFlags bindingFlags = (BindingFlags)314;
            return ((String)(FusionType.InvokeMember("GetCacheTypeString", bindingFlags, null, null, args)));
        }

        public static void ReadCache(ArrayList alAssems, UInt32 nFlag)
        {
            object[] args = new object[] { alAssems, nFlag };
            BindingFlags bindingFlags = (BindingFlags)314;
            FusionType.InvokeMember("ReadCache", bindingFlags, null, null, args);
        }

        public static StringCollection GetKnownFusionApps()
        {
            object[] args = new object[0];
            BindingFlags bindingFlags = (BindingFlags)314;
            return ((StringCollection)(FusionType.InvokeMember("GetKnownFusionApps", bindingFlags, null, null, args)));
        }

        public static int GacInstall(string assemblyFileName)
        {
            object[] args = new object[1];
            args[0] = assemblyFileName;
            BindingFlags bindingFlags = (BindingFlags)314;
            return (int)FusionType.InvokeMember("AddAssemblytoGac", bindingFlags, null, null, args);
        }

        public static bool GacUninstall(string assemblyName)
        {
            object assemInfo = null;
            ArrayList fcGAC = new ArrayList();
            Fusion.ReadCache(fcGAC, (uint)Fusion.CacheType.GAC);
            foreach (object oAssemInfo in fcGAC)
            {
                string name = (string)Reflected.GetField(oAssemInfo, "Name");
                if (name == assemblyName)
                    assemInfo = oAssemInfo;
            }
            if (assemInfo == null)
                return false;

            object[] args = new object[1];
            BindingFlags bindingFlags = (BindingFlags)314;
            args[0] = assemInfo;
            return (bool)FusionType.InvokeMember("RemoveAssemblyFromGac", bindingFlags, null, null, args);
        }
    }
}
