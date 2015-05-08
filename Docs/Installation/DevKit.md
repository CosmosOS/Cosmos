
###  Prerequisites

	* (Free) source code of Devkit from [Cosmos on CodePlex](http://cosmos.codeplex.com/SourceControl/list/changesets)
	* (Optional) [Visual Studio 2013 Community](http://go.microsoft.com/fwlink/?LinkId=517284)  
If you do not wish to use the integrated source control, you can use the SVN
bridge instead.

    * If using Visual Studio 2010 Express, you can use the [SVN bridge](http://cosmos.codeplex.com/SourceControl/list/changesets) i.e. direct download current changeset in a zip file, or one of the many TFS standalone clients available at CodePlex.
	* (Free) [InnoSetup QuickStart Kit](http://www.jrsoftware.org/isdl.php#qsp)
    * This is required to build the setup kit which is used to build and install the Visual Studio integration libaries for Cosmos.
    * During install it will ask you about optional components to install. Be sure you check "Install Inno Setup Preprocessor".
	* (Free) [ Visual Studio 2010 SDK](http://www.microsoft.com/downloads/en/details.aspx?FamilyID=47305cf4-2bea-43c0-91cd-1b853602dcc5)

###  Installation

	* Look in the downloaded sources and **run install.bat** with admin privileges (Vista and upper will ask you for it), needed for install in system directories
	* On end it will start retail versions of Visual Studio and you can begin with OS development.

## Parameters to the install.bat
The `install.bat` accepts following parameters

- `-USERKIT` Run installer for the User Kit only. By default installer build and install Dev Kit.
- `-RESETHIVE` Reset Visual Studio Experimental Hive after installation.
- `-NOTASK` When specified installer would be run directly instead of running as the task in the Task Scheduler
- `-NOCLEAN` Don't clean solution before run installer.
- `-NOVSLAUNCH` Don't launch VS after installation.
- `-IGNOREVS` Ignore running VS during installation.
- `-VS2015` or `/VS2015` Run installer for the VS 2015.
- `-VS2013` or `/VS2013` Run installer for the VS 2013. This is the default.
- `-VSEXPHIVE` or `/VSEXPHIVE` Use Visual Studio Experimental Hive for installation.
