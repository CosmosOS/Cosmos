using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Setup.Configuration;

namespace Cosmos.Build.Installer
{
  public static class Paths
  {
    /// <summary>
    /// ID of the instance of Visual Studio for which paths should be detected.
    /// </summary>
    private static string vsInstanceID;

    public static readonly string ProgFiles32;
    public static readonly string ProgFiles64;
    public static readonly string Windows;

    static Paths()
    {
      if (Environment.Is64BitOperatingSystem)
      {
        ProgFiles32 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        ProgFiles64 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
      }
      else
      {
        ProgFiles32 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
      }

      Windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
    }

    /// <summary>
    /// Gets the path where Visual Studio installed
    /// </summary>
    public static string VSPath { get; set; }

    /// <summary>
    /// Gets the ID of the instance of Visual Studio to use
    /// </summary>
    public static string VSInstanceID
    {
      get
      {
        return vsInstanceID;
      }
      set
      {
        vsInstanceID = value;
        UpdateVSPath();
      }
    }

    /// <summary>
    /// Gets the instance of Visual Studio to use
    /// </summary>
    public static ISetupInstance2 VSInstance { get; private set; }

    /// <summary>
    /// Gets the packages installed in the Visual Studio instance
    /// </summary>
    public static List<string> VSInstancePackages { get; private set; }

    /// <summary>
    /// Updates path to VS version
    /// </summary>
    public static void UpdateVSPath()
    {
      List<ISetupInstance2> xInstances = GetInstances();

      ISetupInstance2 xCurrentInstance;

      if (xInstances.Count == 0)
      {
        throw new Exception("No Visual Studio 2017 instances found!");
      }
      else if (xInstances.Count == 1)
      {
        xCurrentInstance = xInstances[0];
      }
      else
      {
        xCurrentInstance = xInstances.Find(i => string.Equals(i.GetInstallationPath(), VSPath, StringComparison.OrdinalIgnoreCase));

        if (xCurrentInstance == null)
        {
          throw new Exception("There is more than one Visual Studio instance installed, but neither one of them matches the specified path: " + VSPath);
        }
      }

      VSInstance = xCurrentInstance;
      VSInstancePackages = xCurrentInstance.GetPackages().Select(package => package.GetId()).ToList();
    }

    // Code adapted from: https://github.com/Microsoft/vssetup.powershell/blob/develop/src/VSSetup.PowerShell/PowerShell/GetInstanceCommand.cs#L112
    private static List<ISetupInstance2> GetInstances()
    {
      List<ISetupInstance2> xInstances = new List<ISetupInstance2>();
      IEnumSetupInstances xEnumerator = new SetupConfiguration().EnumAllInstances();
      int fetched;

      do
      {
        ISetupInstance[] instances = new ISetupInstance[1];

        xEnumerator.Next(1, instances, out fetched);
        if (fetched != 0)
        {
          xInstances.Add((ISetupInstance2)instances[0]);
        }
      } while (fetched != 0);

      return xInstances;
    }
  }
}
