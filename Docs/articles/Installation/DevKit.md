# Dev Kit Installation 

###  Prerequisites for Windows

* (Free) source code of Development Kit from [Cosmos on GitHub](https://github.com/CosmosOS/Cosmos)
   * You must clone the repository using Git. For a detailed walkthrough, [see here](https://help.github.com/articles/fork-a-repo/).
* (Free) [Visual Studio 2022 Community](https://visualstudio.microsoft.com/vs/)  
* (Free) [InnoSetup](http://www.jrsoftware.org/isdl.php#qsp)
   * This is required to build the setup kit which is used to build and install the Visual Studio integration libaries for Cosmos.
    * During install it will ask you about optional components to install. Be sure you check "Install Inno Setup Preprocessor".

###  Prerequisites for Linux

* .NET SDK (6+): [Download .NET SDK](https://learn.microsoft.com/en-us/dotnet/core/install/linux)
* Make (`apt install make`)
* Yasm (`apt install yasm`)
* Xorriso (`apt install xorriso`)
* QEMU or any other virtual machine. See [Running](https://cosmosos.github.io/articles/Installation/Running.html) for more information.

###  Installation on Windows

* Look in the downloaded sources and run **install-VS2022.bat** with admin privileges (UAC will ask for permission), needed for install in system directories.
* When the installation is complete, Visual Studio will automatically open and you may begin programming with your new, modified copy of Cosmos.

###  Installation on Linux
Run `make` to build Cosmos. Cosmos will clone all the required repos, build itself, and install it and it's nuget packages to the system automatically.

### dotnet Project Templates
If you are using linux or prefer not using Visual Studio for your projects, you can install the dotnet project template using `dotnet new --install ./source/templates/csharp/` assuming you are currently in the Cosmos base directory.
After installing the template use `dotnet new cosmosCSKernel -n {name}` to create a new Cosmos Kernel project. 
The dotnet template can be removed at a later time using `dotnet new --uninstall ./source/templates/csharp/`.
