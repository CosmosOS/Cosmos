
###  Prerequisites

  * (Free) source code of Devkit from [Cosmos on CodePlex](http://cosmos.codeplex.com/SourceControl/list/changesets)
    * (Optional) [ Visual Studio Team Explorer 2010](http://www.microsoft.com/downloads/en/details.aspx?FamilyID=fe4f9904-0480-4c9d-a264-02fedd78ab38)  
If you do not wish to use the integrated source control, you can use the SVN
bridge instead.

    * If using Visual Studio 2010 Express, you can use the [SVN bridge](http://cosmos.codeplex.com/SourceControl/list/changesets) i.e. direct download current changeset in a zip file, or one of the many TFS standalone clients available at CodePlex.
  * (Free) [InnoSetup QuickStart Kit](http://www.jrsoftware.org/isdl.php#qsp)
    * This is required to build the setup kit which is used to build and install the Visual Studio integration libaries for Cosmos.
    * During install it will ask you about optional components to install. Be sure you check "Install Inno Setup Preprocessor".
  * (Free) [ Visual Studio 2010 SDK](http://www.microsoft.com/downloads/en/details.aspx?FamilyID=47305cf4-2bea-43c0-91cd-1b853602dcc5)

###  Installation

  * Look in the downloaded sources and **run /build/VSIP/install.bat** with admin privileges (Windows XP and lower version, Vista and upper will ask you for it), needed for install in system directories
  * On end it will start retail versions of Visual Studio and you can begin with OS development.

