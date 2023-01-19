
###  Prerequisites for Windows

* (Free) source code of Devloppement Kit from [Cosmos on GitHub](https://github.com/CosmosOS/Cosmos)
   * You must clone the repository using Git. For a detailed walkthrough, [see here](https://help.github.com/articles/fork-a-repo/).
   * When following the tutorial, replace *OctoCat* with *CosmosOS* and *Spoon-Knife* with *Cosmos*.
* (Free) [Visual Studio 2022 Community](https://visualstudio.microsoft.com/vs/)  
* (Free) [InnoSetup QuickStart Kit](http://www.jrsoftware.org/isdl.php#qsp)
   * This is required to build the setup kit which is used to build and install the Visual Studio integration libaries for Cosmos.
    * During install it will ask you about optional components to install. Be sure you check "Install Inno Setup Preprocessor".
* Visual Studio SDK: [download here](https://www.microsoft.com/en-us/download/details.aspx?id=40758).

###  Prerequisites for Linux

* .NET 6 SDK: [Download .NET SDK](https://learn.microsoft.com/en-us/dotnet/core/install/linux)
* Make
* Yasm (`apt install yasm`)
* Xorriso (`apt install xorriso`)
* QEMU or any other virtual machine

###  Installation on Windows

* Look in the downloaded sources and run **install-VS2022.bat** with admin privileges (UAC will ask for permission), needed for install in system directories.
* When the installation is complete, Visual Studio will automatically open and you may begin programming with your new, modified copy of Cosmos.

## Arguments for the 'install-VS2022.bat' file
The `install-VS2022.bat` accepts the following parameters :

- `-USERKIT` Run installer for the User Kit only. By default installer build and install Dev Kit.
- `-RESETHIVE` Reset Visual Studio Experimental Hive after installation.
- `-NOTASK` When specified installer would be run directly instead of running as the task in the Task Scheduler
- `-NOCLEAN` Don't clean solution before run installer.
- `-NOVSLAUNCH` Don't launch VS after installation.
- `-IGNOREVS` Ignore running VS during installation.
- `-VSEXPHIVE` or `/VSEXPHIVE` Use Visual Studio Experimental Hive for installation.

###  Installation on Linux
Run `make all` to build Cosmos. Run `sudo make install`. Make sure to run `make nuget-install` under your user account, and not as sudo.

### dotnet Project Templates
If you are using linux or prefer not using Visual Studio for your projects, you can install the dotnet project template using `dotnet new --install .\source\templates\csharp\` assuming you are currently in the Cosmos base directory.
After installing the template use `dotnet new cosmosCSKernel -n {name}` to create a new Cosmos Kernel project. 
The dotnet template can be removed at a later time using `dotnet new --uninstall .\source\templates\csharp\`.
