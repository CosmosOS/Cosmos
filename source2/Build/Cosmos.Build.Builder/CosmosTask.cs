using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Build.Installer;
using System.IO;

namespace Cosmos.Build.Builder {
  public class CosmosTask : Task {

    public void Run(string aCosmosPath) {
      string xOutputPath = aCosmosPath + @"\Build\VSIP";

      EchoOff();

      Echo("Compiling Cosmos");

      CD(aCosmosPath + @"\source");
      Start(Paths.Windows + @"\Microsoft.NET\Framework\v4.0.30319\msbuild.exe", @"Cosmos.sln /maxcpucount /verbosity:normal /nologo /p:Configuration=Bootstrap /p:Platform=x86 /p:OutputPath=" + Quoted(xOutputPath));
      //%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild Cosmos.sln /maxcpucount /verbosity:normal /nologo /p:Configuration=Bootstrap /p:Platform=x86 /p:OutputPath="%THE_OUTPUT_PATH%"

      CD(xOutputPath);

      Echo("Copying files");
      // Copy templates
      // .iss does some of this as well.. why some here? And why is VB disabled in .iss?
      SrcPath = aCosmosPath + @"source2\VSIP\Cosmos.VS.Package\obj\x86\Debug";
      Copy("CosmosProject (C#).zip");
      Copy("CosmosKernel (C#).zip");
      Copy("CosmosProject (F#).zip");
      Copy("Cosmos.zip");
      Copy("CosmosProject (VB).zip");
      Copy("CosmosKernel (VB).zip");

      Echo();
      Echo();
      Echo();

      Echo("Removing old Cosmos");
      //IF NOT EXIST ..\..\Setup2\Output\CosmosUserKit.exe goto afterSetupDelete
      //  ren ..\..\Setup2\Output\CosmosUserKit.exe tmp 2> nul
      //  if ERRORLEVEL 1 (
      //    echo Old COSMOS setup could not be removed, it is locked.
      //    pause
      //    exit /B 1
      //  )
      //  del /F ..\..\Setup2\Output\tmp
      //:afterSetupDelete

      Echo("Searching for Inno");
      //IF NOT EXIST "%ProgFiles%\Inno Setup 5\ISCC.exe" (
      //  echo Cannot find Inno Setup!
      //  pause
      //  exit
      //)

      Echo("Creating setup.exe");
      //"%ProgFiles%\Inno Setup 5\ISCC" /Q ..\..\Setup2\Cosmos.iss /dBuildConfiguration=Devkit

      Echo("Running setup.exe");
      //..\..\Setup2\Output\CosmosUserKit.exe /SILENT

      Echo("Install Completed.");

      // Relaunch VS
      //IF EXIST "%ProgFiles%\Microsoft Visual Studio 10.0\Common7\IDE\devenv.exe" (
      //    IF "%1"=="HIVE" goto ResetHive
      //  goto ResetHiveAfter
      //:ResetHive
      Echo("Resetting hive keys");
      //  "%ProgFiles%\Microsoft Visual Studio 10.0\Common7\IDE\devenv.exe" /setup /rootsuffix Exp /ranu
      //:ResetHiveAfter
      Echo("Launching Visual Studio");
      //  echo You can close this window now.
      //  "%ProgFiles%\Microsoft Visual Studio 10.0\Common7\IDE\devenv.exe" ..\..\source\Cosmos.sln
      //)
    }
  }
}
