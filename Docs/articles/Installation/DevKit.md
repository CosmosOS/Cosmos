# Dev Kit Installation 

## Windows

###  Prerequisites

* (Free) source code of Development Kit from [Cosmos on GitHub](https://github.com/CosmosOS/Cosmos)
   * You must clone the repository using Git. For a detailed walkthrough, [see here](https://help.github.com/articles/fork-a-repo/).
* (Free) [Visual Studio 2022 Community](https://visualstudio.microsoft.com/vs/)  
* (Free) [InnoSetup](http://www.jrsoftware.org/isdl.php#qsp)
   * This is required to build the setup kit which is used to build and install the Visual Studio integration libaries for Cosmos.
   * During install it will ask you about optional components to install. Be sure you check "Install Inno Setup Preprocessor".

###  Installation

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
Run `make` to build Cosmos. Cosmos will clone all the required repos, build itself, and install it and it's nuget packages to the system automatically.

## MacOS (Apple Silicon/Intel)
There is currently hard to build on Apple Silicon Devices. So are going to build DevKit on Docker (Virtual Environment Service).

### Prerequisites

* Docker(`brew cask install docker`)
* QEMU or any other virtual machine. See [Running](https://cosmosos.github.io/articles/Installation/Running.html) for more information.


###  Installation
First we need to set directory to the path of Cosmos source directory and build the Dockerfile:
```
cd /path/to/Cosmos/
docker build -t cosmos .
```
This will take a while. If there is no error, you successfully installed Cosmos on a Virtual Environment!

## notes / FAQ

### custom cosmos repos

if you are using custom cosmos repos you will need to clone them all manually as the installer script will pull from https://github.com/CosmosOS/

A tree diagram of the source should look like the following:   

<img src="https://raw.githubusercontent.com/CosmosOS/Cosmos/master/Docs/images/Dir.png" width="200">


### get stuck pulling the git repos

check if git is installed and in your path as we use git to pull the repos

### i just updated and my project is sill using the old version

you may need to clear you nuget cache try ``dotnet clean`` to clear the project level cache

### the project templates are not showing in visual studio

if you have more then 1 version of visual studio installed this can bug this follow "dotnet Project Templates" should fix it

### dotnet Project Templates
If you are using linux or prefer not using Visual Studio for your projects, you can install the dotnet project template using `dotnet new --install ./source/templates/csharp/` assuming you are currently in the Cosmos base directory.
After installing the template use `dotnet new cosmosCSKernel -n {name}` to create a new Cosmos Kernel project. 
The dotnet template can be removed at a later time using `dotnet new --uninstall ./source/templates/csharp/`.
