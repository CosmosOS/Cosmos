### Common Issues
- When installing the Dev Kit, if .NET Framework 4.6.2 isn't being found, try installing it from: https://www.microsoft.com/en-us/download/details.aspx?id=53321
- (User Kit 20170620) When using Visual Studio 2017 15.3, Cosmos projects fail to build. To fix this, go to "%appdata%\Cosmos User Kit\Build\VSIP\", open Cosmos.targets in a text editor and add these lines to the first PropertyGroup:
```xml
<ImplicitlyExpandNETStandardFacades>False</ImplicitlyExpandNETStandardFacades>
<DesignTimeBuild>True</DesignTimeBuild>
<ProjectAssetsFile>NULL</ProjectAssetsFile>
```
