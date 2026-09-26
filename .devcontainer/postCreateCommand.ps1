#!/usr/bin/env pwsh
# PowerShell script to build Cosmos packages (Windows equivalent of postCreateCommand.sh)

$ErrorActionPreference = "Stop"

Write-Host "=== Starting postCreate setup (multi-arch) ===" -ForegroundColor Cyan

# ── Resolve Cosmos package version ───────────────────────────────────────────
# Single source of truth, in order of precedence:
#   1. $env:VersionPrefix (set by Release CI from the git tag)
#   2. Latest `v*` git tag (local dev on a full clone)
#   3. global.json `msbuild-sdks.Cosmos.Sdk` (PR CI / shallow checkouts)
# See postCreateCommand.sh for the full explanation.
if (-not $env:VersionPrefix) {
    $baseTag = $null
    try {
        $gitTag = git describe --tags --match "v*" --abbrev=0 2>$null
        if ($LASTEXITCODE -eq 0 -and $gitTag) {
            $baseTag = $gitTag.Trim().TrimStart('v')
        }
    } catch { }
    if (-not $baseTag) {
        $match = Select-String -Path "global.json" -Pattern '"Cosmos\.Sdk"\s*:\s*"([0-9]+\.[0-9]+\.[0-9]+)' -ErrorAction SilentlyContinue
        if ($match) { $baseTag = $match.Matches[0].Groups[1].Value }
    }
    if (-not $baseTag) {
        Write-Error "ERROR: could not resolve Cosmos base version from git tags or global.json"
        exit 1
    }
    # yyyyMMdd (not ddMMyyyy) to avoid NuGet stripping a leading zero from the
    # day component when normalizing the package version.
    $dateSuffix = Get-Date -Format "yyyyMMdd"
    $env:VersionPrefix = "$baseTag.$dateSuffix"
}
Write-Host "Using Cosmos package version: $env:VersionPrefix" -ForegroundColor Cyan

# Rewrite global.json `msbuild-sdks.Cosmos.Sdk` so kernel projects
# (<Sdk Name="Cosmos.Sdk" /> without a literal Version) resolve to the
# version we're about to build.
$globalJson = Get-Content "global.json" -Raw | ConvertFrom-Json
if (-not $globalJson.PSObject.Properties.Name.Contains("msbuild-sdks")) {
    $globalJson | Add-Member -NotePropertyName "msbuild-sdks" -NotePropertyValue ([pscustomobject]@{})
}
$globalJson.'msbuild-sdks' | Add-Member -NotePropertyName "Cosmos.Sdk" -NotePropertyValue $env:VersionPrefix -Force
($globalJson | ConvertTo-Json -Depth 10) | Set-Content "global.json" -NoNewline
Add-Content "global.json" "`n"

# Clear Cosmos packages from NuGet cache
Write-Host "Clearing Cosmos packages from NuGet cache..."
Remove-Item -Path "$env:USERPROFILE\.nuget\packages\cosmos.*" -Recurse -Force -ErrorAction SilentlyContinue

# Remove all build artifacts for clean build
Write-Host "Cleaning all build artifacts..."
Remove-Item -Path "artifacts" -Recurse -Force -ErrorAction SilentlyContinue
# Also clean obj folders in src (in case they exist outside artifacts)
Get-ChildItem -Path "src" -Directory -Recurse -Filter "obj" | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue

# Remove local source if it exists
dotnet nuget remove source local-packages 2>$null

# Create artifacts directories
New-Item -ItemType Directory -Force -Path "artifacts/package/release" | Out-Null
New-Item -ItemType Directory -Force -Path "artifacts/multiarch" | Out-Null

# Add local source
dotnet nuget add source "$PWD/artifacts/package/release" --name local-packages

# Build and pack each project in dependency order
# Download Limine bootloader (bundled in Cosmos.Build.Common NuGet package)
Write-Host "Downloading Limine bootloader..." -ForegroundColor Cyan
Remove-Item -Path "artifacts/limine" -Recurse -Force -ErrorAction SilentlyContinue
git clone https://github.com/Limine-Bootloader/Limine.git --branch=v10.x-binary --depth=1 artifacts/limine
Remove-Item -Path "artifacts/limine/.git" -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "Building and packing base projects..." -ForegroundColor Cyan
dotnet build src/Cosmos.Build.API/Cosmos.Build.API.csproj -c Release --no-incremental
dotnet build src/Cosmos.Build.Common/Cosmos.Build.Common.csproj -c Release --no-incremental

Write-Host "Building and packing build tools..." -ForegroundColor Cyan
dotnet build src/Cosmos.Build.Asm/Cosmos.Build.Asm.csproj -c Release --no-incremental
dotnet build src/Cosmos.Build.CC/Cosmos.Build.CC.csproj -c Release --no-incremental
dotnet build src/Cosmos.Build.Ilc/Cosmos.Build.Ilc.csproj -c Release --no-incremental
dotnet build src/Cosmos.Build.Patcher/Cosmos.Build.Patcher.csproj -c Release --no-incremental
dotnet build src/Cosmos.Build.Analyzer.Patcher.Package/Cosmos.Build.Analyzer.Patcher.Package.csproj -c Release --no-incremental
dotnet build src/Cosmos.Patcher/Cosmos.Patcher.csproj -c Release --no-incremental
dotnet build src/Cosmos.Tools/Cosmos.Tools.csproj -c Release --no-incremental

# Native packages (content-only)
Write-Host "Packing native packages..." -ForegroundColor Cyan
dotnet pack src/Cosmos.Kernel.Native.X64/Cosmos.Kernel.Native.X64.csproj -c Release -o artifacts/package/release
dotnet pack src/Cosmos.Kernel.Native.ARM64/Cosmos.Kernel.Native.ARM64.csproj -c Release -o artifacts/package/release
dotnet pack src/Cosmos.Kernel.Native.MultiArch/Cosmos.Kernel.Native.MultiArch.csproj -c Release -o artifacts/package/release

Write-Host "Verifying native packages..." -ForegroundColor Yellow
Get-ChildItem -Path "artifacts/package/release/Cosmos.Kernel.Native.*.nupkg" | ForEach-Object { Write-Host $_.Name }

# Architecture-independent kernel packages (build first, then pack)
Write-Host "Building and packing architecture-independent kernel packages..." -ForegroundColor Cyan
dotnet build src/Cosmos.Kernel.HAL.Interfaces/Cosmos.Kernel.HAL.Interfaces.csproj -c Release -p:GeneratePackageOnBuild=false
dotnet pack src/Cosmos.Kernel.HAL.Interfaces/Cosmos.Kernel.HAL.Interfaces.csproj -c Release --no-build -o artifacts/package/release
dotnet build src/Cosmos.Kernel.Debug/Cosmos.Kernel.Debug.csproj -c Release -p:GeneratePackageOnBuild=false
dotnet pack src/Cosmos.Kernel.Debug/Cosmos.Kernel.Debug.csproj -c Release --no-build -o artifacts/package/release
dotnet build src/Cosmos.Kernel.Boot.Limine/Cosmos.Kernel.Boot.Limine.csproj -c Release -p:GeneratePackageOnBuild=false
dotnet pack src/Cosmos.Kernel.Boot.Limine/Cosmos.Kernel.Boot.Limine.csproj -c Release --no-build -o artifacts/package/release
dotnet build src/Cosmos.Kernel.SourceGenerators/Cosmos.Kernel.SourceGenerators.csproj -c Release -p:GeneratePackageOnBuild=false
dotnet pack src/Cosmos.Kernel.SourceGenerators/Cosmos.Kernel.SourceGenerators.csproj -c Release --no-build -o artifacts/package/release

Write-Host "Verifying arch-independent packages..." -ForegroundColor Yellow
Get-ChildItem -Path "artifacts/package/release/Cosmos.Kernel.HAL.Interfaces.*.nupkg" | ForEach-Object { Write-Host $_.Name }
Get-ChildItem -Path "artifacts/package/release/Cosmos.Kernel.Debug.*.nupkg" | ForEach-Object { Write-Host $_.Name }
Get-ChildItem -Path "artifacts/package/release/Cosmos.Kernel.Boot.*.nupkg" | ForEach-Object { Write-Host $_.Name }

# Architecture-specific HAL packages (build first, then pack)
Write-Host "Building and packing architecture-specific HAL packages..." -ForegroundColor Cyan
dotnet build src/Cosmos.Kernel.HAL.X64/Cosmos.Kernel.HAL.X64.csproj -c Release -p:GeneratePackageOnBuild=false
dotnet pack src/Cosmos.Kernel.HAL.X64/Cosmos.Kernel.HAL.X64.csproj -c Release --no-build -o artifacts/package/release
dotnet build src/Cosmos.Kernel.HAL.ARM64/Cosmos.Kernel.HAL.ARM64.csproj -c Release -p:GeneratePackageOnBuild=false
dotnet pack src/Cosmos.Kernel.HAL.ARM64/Cosmos.Kernel.HAL.ARM64.csproj -c Release --no-build -o artifacts/package/release

Write-Host "Verifying HAL packages..." -ForegroundColor Yellow
Get-ChildItem -Path "artifacts/package/release/Cosmos.Kernel.HAL.X64.*.nupkg" | ForEach-Object { Write-Host $_.Name }
Get-ChildItem -Path "artifacts/package/release/Cosmos.Kernel.HAL.ARM64.*.nupkg" | ForEach-Object { Write-Host $_.Name }

# Multi-arch packages list
$MultiArchProjects = @(
    "Cosmos.Kernel.Core",
    "Cosmos.Kernel.HAL",
    "Cosmos.Kernel.System",
    "Cosmos.Kernel.Plugs",
    "Cosmos.Kernel"
)

# Build all multi-arch packages for x64
Write-Host "Building all multi-arch packages for x64..." -ForegroundColor Cyan
dotnet build src/Cosmos.Kernel/Cosmos.Kernel.csproj -c Release -r linux-x64 -p:CosmosArch=x64 --no-incremental

# Stage x64 builds: the assembly and its XML docs, which the package
# ships next to it so a kernel's IDE shows them.
Write-Host "Staging x64 builds..." -ForegroundColor Cyan
foreach ($proj in $MultiArchProjects) {
    New-Item -ItemType Directory -Force -Path "artifacts/multiarch/$proj/x64" | Out-Null
    $destPath = "artifacts/multiarch/$proj/x64/"
    foreach ($ext in @("dll", "xml")) {
        $sourcePath1 = "artifacts/bin/$proj/release_linux-x64/$proj.$ext"
        $sourcePath2 = "artifacts/bin/$proj/release/$proj.$ext"

        if (Test-Path $sourcePath1) {
            Copy-Item $sourcePath1 $destPath -ErrorAction SilentlyContinue
        } elseif (Test-Path $sourcePath2) {
            Copy-Item $sourcePath2 $destPath -ErrorAction SilentlyContinue
        }
    }
}

# Build all multi-arch packages for arm64
Write-Host "Building all multi-arch packages for arm64..." -ForegroundColor Cyan
dotnet build src/Cosmos.Kernel/Cosmos.Kernel.csproj -c Release -r linux-arm64 -p:CosmosArch=arm64 --no-incremental

# Stage arm64 builds: the assembly and its XML docs, which the package
# ships next to it so a kernel's IDE shows them.
Write-Host "Staging arm64 builds..." -ForegroundColor Cyan
foreach ($proj in $MultiArchProjects) {
    New-Item -ItemType Directory -Force -Path "artifacts/multiarch/$proj/arm64" | Out-Null
    $destPath = "artifacts/multiarch/$proj/arm64/"
    foreach ($ext in @("dll", "xml")) {
        $sourcePath1 = "artifacts/bin/$proj/release_linux-arm64/$proj.$ext"
        $sourcePath2 = "artifacts/bin/$proj/release/$proj.$ext"

        if (Test-Path $sourcePath1) {
            Copy-Item $sourcePath1 $destPath -ErrorAction SilentlyContinue
        } elseif (Test-Path $sourcePath2) {
            Copy-Item $sourcePath2 $destPath -ErrorAction SilentlyContinue
        }
    }
}

# No ref assembly needed - NuGet will select the correct RID-specific assembly
Write-Host "Multi-arch staging complete (no ref assembly - NuGet selects by RID)" -ForegroundColor Cyan

# Pack multi-arch packages (these use pre-staged DLLs via Directory.MultiArch.targets)
Write-Host "Packing multi-arch packages..." -ForegroundColor Cyan
foreach ($proj in $MultiArchProjects) {
    Write-Host "Packing $proj..." -ForegroundColor Yellow
    Get-ChildItem -Path "artifacts/obj/$proj" -Filter "*.nuspec" -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force
    # Only delete exact package name (not prefix matches like Cosmos.Kernel.* which would
    # delete Native, HAL, etc). Anchoring on a digit catches any version (real package
    # names start with a letter after the dot, real versions start with a digit).
    Remove-Item -Path "artifacts/package/release/$proj.[0-9]*.nupkg" -Force -ErrorAction SilentlyContinue
    dotnet pack "src/$proj/$proj.csproj" -c Release -o artifacts/package/release -p:NoBuild=true
}

# SDK and Templates
Write-Host "Building SDK and Templates..." -ForegroundColor Cyan
dotnet build src/Cosmos.Sdk/Cosmos.Sdk.csproj -c Release --no-incremental
dotnet build src/Cosmos.Build.Templates/Cosmos.Build.Templates.csproj -c Release --no-incremental

# List all created packages
Write-Host "=== Created packages ===" -ForegroundColor Green
Get-ChildItem -Path "artifacts/package/release/*.nupkg" | ForEach-Object { Write-Host $_.Name }

# Clear Cosmos packages again to force fresh restore
Write-Host "Clearing Cosmos packages to force fresh restore..." -ForegroundColor Cyan
Remove-Item -Path "$env:USERPROFILE\.nuget\packages\cosmos.*" -Recurse -Force -ErrorAction SilentlyContinue

# Restore main solution
Write-Host "Restoring main solution..." -ForegroundColor Cyan
dotnet restore ./nativeaot-patcher.slnx

# Install global tools
Write-Host "Installing global tools..." -ForegroundColor Cyan
dotnet tool uninstall -g Cosmos.Patcher 2>$null
dotnet tool install -g Cosmos.Patcher --add-source artifacts/package/release
dotnet tool uninstall -g Cosmos.Tools 2>$null
dotnet tool install -g Cosmos.Tools --add-source artifacts/package/release

Write-Host "=== PostCreate setup completed (multi-arch) ===" -ForegroundColor Green
