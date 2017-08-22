### Common Issues
- When installing the Dev Kit, if .NET Framework 4.6.2 isn't being found, try installing it from: https://www.microsoft.com/en-us/download/details.aspx?id=53321
- (User Kit 20170620) When using Visual Studio 2017 15.3, Cosmos projects fail to build. To fix this, go to "%appdata%\Cosmos User Kit\Build\VSIP\" and replace Cosmos.targets by this one: [Cosmos.targets](https://github.com/CosmosOS/Cosmos/raw/ea2354dcadfc44e229b8e2c5ab57fff8dbc97f5d/source/Cosmos.Build.MSBuild/Cosmos.targets).
