# DevKit Installation 

## Windows

###  Prerequisites

* Source code of Development Kit from [Cosmos on GitHub](https://github.com/CosmosOS/Cosmos)
   * You must clone the repository using Git. For a detailed walkthrough, [see here](https://help.github.com/articles/fork-a-repo/).
* [Visual Studio 2022 Community](https://visualstudio.microsoft.com/vs/)  
* [InnoSetup](http://www.jrsoftware.org/isdl.php#qsp)
   * This is required to build the setup kit which is used to build and install the Visual Studio integration libaries for Cosmos.
   * During install it will ask you about optional components to install. Be sure you check "Install Inno Setup Preprocessor".

###  Installation

* Git clone the current source code of Cosmos and rename the folder to simply 'Cosmos'.
* Look in the downloaded sources and run **install-VS2022.bat** with admin privileges (UAC will ask for permission), needed for install in system directories.
* When the installation is complete, Visual Studio will automatically open and you may begin programming with your new, modified copy of Cosmos.


## Linux

###  Prerequisites

* .NET SDK (6+): [Download .NET SDK](https://learn.microsoft.com/en-us/dotnet/core/install/linux)
* Make (`apt install make`)
* Yasm (`apt install yasm`)
* Xorriso (`apt install xorriso`)
* QEMU or any other virtual machine. See [Running](https://cosmosos.github.io/articles/Installation/Running.html) for more information.

###  Installation
Git cline the current source code of Cosmos.
Run `make` to build Cosmos. Cosmos will clone all the required repos, build itself, and install it and its nuget packages to the system automatically.

## MacOS (Apple Silicon/Intel)
It is currently difficult to build Cosmos on Apple Silicon Devices. So, we are going to build DevKit on Docker (Virtual Environment Service).

### Prerequisites

* Docker (`brew cask install docker`)
* QEMU or any other virtual machine. See [Running](https://cosmosos.github.io/articles/Installation/Running.html) for more information.


###  Installation
First, we need to set directory to the path of Cosmos source directory and build the Dockerfile:
```
cd /path/to/Cosmos/
docker build -t cosmos .
```
This will take a while. If there is no error, you successfully installed Cosmos on a Virtual Environment!

## Notes / FAQ

### Custom Cosmos Repos

If you are using custom Cosmos repos, you will need to clone them all manually, as the installer script will only pull from https://github.com/CosmosOS/

A tree diagram of the source should look like the following:   

<img src="https://raw.githubusercontent.com/CosmosOS/Cosmos/master/Docs/images/Dir.png" width="200">


### I get stuck pulling the git repos!

Check if [Git](https://git-scm.com/) is installed on your machine and within your path. We use Git to pull the repos from GitHub.

### I just updated my DevKit and my project is still using the older version!

Although Cosmos typically uninstalls previous kits before installing a new one; it is possible that your project can reference an older-version. In the rare case this does occur, you may need to clear the NuGet cache. Try running `dotnet clean` to clear the project-level cache. 

### The project templates are not showing in Visual Studio!

If you have more than 1 version of Visual Studio *(such as both 2022 and 2019)*, this bug can occur. Try running `dotnet Project Templates` to get them back.

### dotnet Project Templates
If you are using Linux, or prefer not using Visual Studio for your projects, you can install the dotnet project template using `dotnet new --install ./source/templates/csharp/` assuming you are currently in the Cosmos base directory. After installing the template, use `dotnet new cosmosCSKernel -n {name}` to create a new Cosmos Kernel project. 
The dotnet template can be removed at a later time using `dotnet new --uninstall ./source/templates/csharp/`.
