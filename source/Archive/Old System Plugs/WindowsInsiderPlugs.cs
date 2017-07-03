using System;
using System.IO;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System
{
    // plugs introduced for Windows Insider preview 14342. Needs reviewing and merged into other classes in case of RTM of the windows version.
    [Plug(TargetName = "System.Security.CodeAccessSecurityEngine, mscorlib", IsMicrosoftdotNETOnly = true, IsOptional = true)]
    public class CodeAccessSecurityEngineImpl
    {
        public static bool QuickCheckForAllDemands()
        {
            return false;
        }

        public static void CCtor()
        {
            //
        }
    }

    [Plug(TargetName = "System.IO.LongPathHelper, mscorlib", IsMicrosoftdotNETOnly = true, IsOptional = true)]
    public static class LongPathHelperImpl
    {
        public static string GetLongPathName(string path)
        {
            return path;
        }
    }

    [Plug(Target = typeof(Path))]
    //[PlugField(FieldId = "System.Char[] System.IO.Path.InvalidPathCharsWithAdditionalChecks", FieldType = typeof(char[]))]
    public static class PathImpl
    {
        public static string NewNormalizePath(string path, int length, bool check)
        {
            return path;
        }
    }

    [Plug(TargetName = "System.AppContextDefaultValues, mscorlib", IsOptional=true)]
    public static class AppContextDefaultValuesImpl
    {
        public static void CCtor()
        {
            
        }

        public static bool TryGetSwitchOverride(string key, ref bool value)
        {
            return false;
        }

        public static void PopulateDefaultValues()
        {
            //
        }

    }

    [Plug(TargetName = "System.AppContext, mscorlib", IsOptional = true)]
    public static class AppContextImpl
    {
        public static bool TryGetSwitch(string key, ref bool value)
        {
            return false;
        }

        public static void CCtor()
        {
            
        }
    }
}