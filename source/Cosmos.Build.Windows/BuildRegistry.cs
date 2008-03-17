using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace Cosmos.Build.Windows
{
    public static class BuildRegistry
    {
        public static void Write(string key, string value)
        {
            RegistryKey xKey = Registry.CurrentUser.CreateSubKey(@"Software\Cosmos");
            xKey.SetValue(key, value);
        }

        public static string Read(string key)
        {
            RegistryKey xKey = Registry.CurrentUser.OpenSubKey(@"Software\Cosmos");
            return (string)xKey.GetValue(key);
        }
    }
}
