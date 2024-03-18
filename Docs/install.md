
  
### Dev Kit
  
##### **Additional Prerequisites**
  
*   **Visual Studio 2022 Workload: Visual Studio Extension Development**
*   **Inno Setup (Free)** – Install with defaults - [Download](http://www.jrsoftware.org/isdl.php#qsp)
*   **.NET 6 SDK** - [Download](https://dotnet.microsoft.com/en-us/download)
  
##### Get the Source
  
Cosmos source is hosted at GitHub. The simplest method to obtain the source is download a .zip file, however this makes updating and getting the latest updates a bit tricky.  
To update the Cosmos source more efficiently, use the Git command line, or any of the many Git User Interfaces. Any frontend may work fine but for users new to Git, we suggest [GitHub Desktop](https://desktop.github.com/). For more experienced Git users, we really like [Git Kraken](https://www.gitkraken.com/). Git Kraken is free for non-commercial use such as Cosmos. SourceTree, Git GUI, and others are also available.

To aid in better encapsulation and to invite more developers to assist in various areas, Cosmos is split into four Git repositories. Each of these is designed to be an independent project although Cosmos relies on the other three.  
You will need to pull the source for all four repositories and they must exist in sibling directories as shown in the diagram below. The Cosmos base directory can be named anything and exist anywhere. But the directories inside it must match exactly.  

Windows is not case sensitive for files, but many of the ._**NET Core tools used to build are case sensitive even on Windows for file paths**_. Make sure to create the subdirectories exactly as shown. For demonstration purposes let us assume that you will use c:\\source\\Cosmos to install to.   
This base directory is referred to as the Cosmos directory. Each of the four repositories then must be cloned or extracted to the corresponding sub folder of the Cosmos directory. Using this example, the set up should look like this:  

```
C:\\source\\CosmosOS\\
C:\\source\\CosmosOS\\Cosmos\\ - [Git Repository for Cosmos](https://github.com/CosmosOS/Cosmos) 
C:\\source\\CosmosOS\\IL2CPU\\ - [Git Repository for IL2CPU](https://github.com/CosmosOS/IL2CPU) 
C:\\source\\CosmosOS\\XSharp\\ - [Git Repository for XSharp](https://github.com/CosmosOS/XSharp) 
C:\\source\\CosmosOS\\Common\\ - [Git Repository for Common](https://github.com/CosmosOS/Common) 
```

A tree diagram of the source should look like the following:   

<img src="https://raw.githubusercontent.com/CosmosOS/Cosmos/master/Docs/images/Dir.png" width="200">

We are working to make syncing code easier. We have already experimented with Git Submodules, however the simple use of submodules presents a few issues, but the biggest of them is that they trigger a fireworks show of problems and errors in every Git UI we tried and would have limited developers options and forced them to the command line in most cases. We are currently investigating subtrees.  
  
For now, it's much easier to handle the 4 repositories as most of the time, new Cosmos developers may only have to work in the Cosmos repository. Only when you are comfortable with the concept of Operating System development and C# should you venture into IL2CPU and XSharp.  
  
If you want to keep your source up to date in a one-click method, paste this code into a .bat file in the CosmosOS folder ( as shown in the above example). This .bat file will only work after the repositories have been cloned with git at least once.

```cmd
cd C:\\XSharp  
git pull  
cd ..\\IL2CPU  
git pull  
cd ..\\Cosmos  
git pull  
cd ..\\Common  
git pull  
cd ..  
```
    
##### Building and Installing  
  
(if you have already installed) If you edited the Cosmos DevKit source using _Cosmos.sln_ or _Test.sln_, be sure to set solution config to **Debug x86**.  
  
1.  Make sure Visual Studio is **NOT running**.  
2.  In the root directory of the DevKit files, you downloaded earlier, run `install-VS2022.bat`.  
3.  Wait for the install to progress. (**Tip:** At the end the installer may look like it is stalling, it is still doing something, just in the background)
4.  VS will open with Cosmos loaded. You can now make changes to core assemblies of Cosmos. If you don't want to, you can close this VS window and create a new Cosmos project as with the user kit.

For more information about Dev Kit, see [here](articles/Installation/DevKit.html)

Happy Cosmos-ing!
