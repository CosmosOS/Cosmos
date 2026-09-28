; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
COSMOSGEN001 | Cosmos.DriverKit | Warning | A [Driver] class the manifest cannot construct is left out
COSMOSGEN002 | Cosmos.DriverKit | Warning | A CosmosDriverExclude or CosmosDriverInclude item matches no driver
COSMOSGEN003 | Cosmos.DriverKit | Error | A [Driver] names a DriverFeature with no KernelFeatures property
