
###  Prerequisites

* (Free) source code of Devloppement Kit from [Cosmos on GitHub](https://github.com/CosmosOS/Cosmos)
   * You must clone the repository using Git. For a detailed walkthrough, [see here](https://help.github.com/articles/fork-a-repo/).
   * When following the tutorial, replace *OctoCat* with *CosmosOS* and *Spoon-Knife* with *Cosmos*.
* (Free) [Visual Studio 2019 Community](https://visualstudio.microsoft.com/vs/)  
* (Free) [InnoSetup QuickStart Kit](http://www.jrsoftware.org/isdl.php#qsp)
   * This is required to build the setup kit which is used to build and install the Visual Studio integration libaries for Cosmos.
    * During install it will ask you about optional components to install. Be sure you check "Install Inno Setup Preprocessor".
* Visual Studio SDK: [download here](https://www.microsoft.com/en-us/download/details.aspx?id=40758).

###  Installation

* Look in the downloaded sources and run **install-VS2019.bat** with admin privileges (UAC will ask for permission), needed for install in system directories.
* When the installation is complete, Visual Studio will automatically open and you may begin programming with your new, modified copy of Cosmos.

## Arguments for the 'install-VS2015.bat' file
The `install-VS2019.bat` accepts the following parameters :

- `-USERKIT` Run installer for the User Kit only. By default installer build and install Dev Kit.
- `-RESETHIVE` Reset Visual Studio Experimental Hive after installation.
- `-NOTASK` When specified installer would be run directly instead of running as the task in the Task Scheduler
- `-NOCLEAN` Don't clean solution before run installer.
- `-NOVSLAUNCH` Don't launch VS after installation.
- `-IGNOREVS` Ignore running VS during installation.
- `-VSEXPHIVE` or `/VSEXPHIVE` Use Visual Studio Experimental Hive for installation.
