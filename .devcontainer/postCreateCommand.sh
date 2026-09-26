#!/usr/bin/env bash

set -e

echo "=== Starting postCreate setup (multi-arch) ==="

# ── Resolve Cosmos package version ───────────────────────────────────────────
# Single source of truth, in order of precedence:
#   1. $VersionPrefix env var (set by Release CI from the git tag)
#   2. Latest `v*` git tag (local dev on a full clone)
#   3. global.json `msbuild-sdks.Cosmos.Sdk` (PR CI / shallow checkouts where
#      git tags aren't fetched; the committed value is the current release)
# If none of these resolve a 3-component base version, we bail out loudly
# rather than silently building broken packages with a hardcoded literal.
if [[ -z "${VersionPrefix:-}" ]]; then
  BASE_TAG=$(git describe --tags --match "v*" --abbrev=0 2>/dev/null || true)
  BASE_TAG="${BASE_TAG#v}"
  if [[ -z "$BASE_TAG" ]]; then
    BASE_TAG=$(sed -nE 's/.*"Cosmos\.Sdk"[[:space:]]*:[[:space:]]*"([0-9]+\.[0-9]+\.[0-9]+).*/\1/p' global.json | head -n1)
  fi
  if [[ -z "$BASE_TAG" ]]; then
    echo "ERROR: could not resolve Cosmos base version from git tags or global.json" >&2
    exit 1
  fi
  # yyyyMMdd (not ddMMyyyy) to avoid NuGet stripping a leading zero from the
  # day component when normalizing the package version.
  DATE_SUFFIX=$(date +%Y%m%d)
  VersionPrefix="${BASE_TAG}.${DATE_SUFFIX}"
fi
export VersionPrefix
echo "Using Cosmos package version: ${VersionPrefix}"

# Rewrite global.json `msbuild-sdks.Cosmos.Sdk` so kernel projects
# (`<Sdk Name="Cosmos.Sdk" />` without a literal Version) resolve to the
# version we're about to build. Uses sed since jq isn't always available.
sed -i.bak -E "s|(\"Cosmos\.Sdk\"[[:space:]]*:[[:space:]]*\")[^\"]+(\")|\1${VersionPrefix}\2|" global.json
rm -f global.json.bak

# Clear Cosmos packages from NuGet cache
echo "Clearing Cosmos packages from NuGet cache..."
rm -rf ~/.nuget/packages/cosmos.* 2>/dev/null || true

# Remove all build artifacts for clean build
echo "Cleaning all build artifacts..."
rm -rf artifacts/ 2>/dev/null || true
# Also clean obj folders in src (in case they exist outside artifacts)
find src -name "obj" -type d -exec rm -rf {} + 2>/dev/null || true

# Remove local source if it exists (to avoid duplicates)
dotnet nuget remove source local-packages >/dev/null 2>&1 || true

# Create artifacts directories
mkdir -p artifacts/package/release
mkdir -p artifacts/multiarch

# Add local source
dotnet nuget add source "$PWD/artifacts/package/release" --name local-packages

# Download Limine bootloader (bundled in Cosmos.Build.Common NuGet package)
echo "Downloading Limine bootloader..."
rm -rf artifacts/limine
git clone https://github.com/Limine-Bootloader/Limine.git --branch=v10.x-binary --depth=1 artifacts/limine
rm -rf artifacts/limine/.git

# Build and pack each project individually in dependency order
# Note: GeneratePackageOnBuild=true in Directory.Build.props means build also packs
echo "Building and packing base projects..."
dotnet build src/Cosmos.Build.API/Cosmos.Build.API.csproj -c Release --no-incremental
dotnet build src/Cosmos.Build.Common/Cosmos.Build.Common.csproj -c Release --no-incremental

echo "Building and packing build tools..."
dotnet build src/Cosmos.Build.Asm/Cosmos.Build.Asm.csproj -c Release --no-incremental
dotnet build src/Cosmos.Build.CC/Cosmos.Build.CC.csproj -c Release --no-incremental
dotnet build src/Cosmos.Build.Ilc/Cosmos.Build.Ilc.csproj -c Release --no-incremental
dotnet build src/Cosmos.Build.Patcher/Cosmos.Build.Patcher.csproj -c Release --no-incremental
dotnet build src/Cosmos.Build.Analyzer.Patcher.Package/Cosmos.Build.Analyzer.Patcher.Package.csproj -c Release --no-incremental
dotnet build src/Cosmos.Patcher/Cosmos.Patcher.csproj -c Release --no-incremental
dotnet build src/Cosmos.Tools/Cosmos.Tools.csproj -c Release --no-incremental

# Native packages (content-only)
echo "Packing native packages..."
dotnet pack src/Cosmos.Kernel.Native.X64/Cosmos.Kernel.Native.X64.csproj -c Release -o artifacts/package/release
dotnet pack src/Cosmos.Kernel.Native.ARM64/Cosmos.Kernel.Native.ARM64.csproj -c Release -o artifacts/package/release
dotnet pack src/Cosmos.Kernel.Native.MultiArch/Cosmos.Kernel.Native.MultiArch.csproj -c Release -o artifacts/package/release

echo "Verifying native packages..."
ls -la artifacts/package/release/Cosmos.Kernel.Native.*.nupkg

# Architecture-independent kernel packages (build first, then pack)
echo "Building and packing architecture-independent kernel packages..."
dotnet build src/Cosmos.Kernel.HAL.Interfaces/Cosmos.Kernel.HAL.Interfaces.csproj -c Release -p:GeneratePackageOnBuild=false
dotnet pack src/Cosmos.Kernel.HAL.Interfaces/Cosmos.Kernel.HAL.Interfaces.csproj -c Release --no-build -o artifacts/package/release
dotnet build src/Cosmos.Kernel.Debug/Cosmos.Kernel.Debug.csproj -c Release -p:GeneratePackageOnBuild=false
dotnet pack src/Cosmos.Kernel.Debug/Cosmos.Kernel.Debug.csproj -c Release --no-build -o artifacts/package/release
dotnet build src/Cosmos.Kernel.Boot.Limine/Cosmos.Kernel.Boot.Limine.csproj -c Release -p:GeneratePackageOnBuild=false
dotnet pack src/Cosmos.Kernel.Boot.Limine/Cosmos.Kernel.Boot.Limine.csproj -c Release --no-build -o artifacts/package/release
dotnet build src/Cosmos.Kernel.SourceGenerators/Cosmos.Kernel.SourceGenerators.csproj -c Release -p:GeneratePackageOnBuild=false
dotnet pack src/Cosmos.Kernel.SourceGenerators/Cosmos.Kernel.SourceGenerators.csproj -c Release --no-build -o artifacts/package/release

echo "Verifying arch-independent packages..."
ls -la artifacts/package/release/Cosmos.Kernel.HAL.Interfaces.*.nupkg
ls -la artifacts/package/release/Cosmos.Kernel.Debug.*.nupkg
ls -la artifacts/package/release/Cosmos.Kernel.Boot.*.nupkg

# Architecture-specific Core bridge packages (single-arch, host all LibraryImport
# declarations consumed by HAL.X64/HAL.ARM64 — must be built before them)
echo "Building and packing architecture-specific Core bridge packages..."
dotnet build src/Cosmos.Kernel.Core.X64/Cosmos.Kernel.Core.X64.csproj -c Release -p:GeneratePackageOnBuild=false
dotnet pack src/Cosmos.Kernel.Core.X64/Cosmos.Kernel.Core.X64.csproj -c Release --no-build -o artifacts/package/release
dotnet build src/Cosmos.Kernel.Core.ARM64/Cosmos.Kernel.Core.ARM64.csproj -c Release -p:GeneratePackageOnBuild=false
dotnet pack src/Cosmos.Kernel.Core.ARM64/Cosmos.Kernel.Core.ARM64.csproj -c Release --no-build -o artifacts/package/release

# Architecture-specific HAL packages (build first, then pack)
echo "Building and packing architecture-specific HAL packages..."
dotnet build src/Cosmos.Kernel.HAL.X64/Cosmos.Kernel.HAL.X64.csproj -c Release -p:GeneratePackageOnBuild=false
dotnet pack src/Cosmos.Kernel.HAL.X64/Cosmos.Kernel.HAL.X64.csproj -c Release --no-build -o artifacts/package/release
dotnet build src/Cosmos.Kernel.HAL.ARM64/Cosmos.Kernel.HAL.ARM64.csproj -c Release -p:GeneratePackageOnBuild=false
dotnet pack src/Cosmos.Kernel.HAL.ARM64/Cosmos.Kernel.HAL.ARM64.csproj -c Release --no-build -o artifacts/package/release

echo "Verifying HAL packages..."
ls -la artifacts/package/release/Cosmos.Kernel.HAL.X64.*.nupkg
ls -la artifacts/package/release/Cosmos.Kernel.HAL.ARM64.*.nupkg

# Multi-arch packages (have #if ARCH_* conditional code or depend on multi-arch)
MULTIARCH_PROJECTS=(
    "Cosmos.Kernel.Core"
    "Cosmos.Kernel.HAL"
    "Cosmos.Kernel.System"
    "Cosmos.Kernel.Plugs"
    "Cosmos.Kernel"
)

# Build all multi-arch packages for x64
echo "Building all multi-arch packages for x64..."
dotnet build src/Cosmos.Kernel/Cosmos.Kernel.csproj -c Release -r linux-x64 -p:CosmosArch=x64 --no-incremental

# Stage x64 builds: the assembly and its XML docs, which the package
# ships next to it so a kernel's IDE shows them.
echo "Staging x64 builds..."
for proj in "${MULTIARCH_PROJECTS[@]}"; do
    mkdir -p "artifacts/multiarch/$proj/x64"
    for ext in dll xml; do
        cp "artifacts/bin/$proj/release_linux-x64/$proj.$ext" "artifacts/multiarch/$proj/x64/" 2>/dev/null || \
        cp "artifacts/bin/$proj/release/$proj.$ext" "artifacts/multiarch/$proj/x64/" 2>/dev/null || true
    done
done

# Build all multi-arch packages for arm64
echo "Building all multi-arch packages for arm64..."
dotnet build src/Cosmos.Kernel/Cosmos.Kernel.csproj -c Release -r linux-arm64 -p:CosmosArch=arm64 --no-incremental

# Stage arm64 builds: the assembly and its XML docs, which the package
# ships next to it so a kernel's IDE shows them.
echo "Staging arm64 builds..."
for proj in "${MULTIARCH_PROJECTS[@]}"; do
    mkdir -p "artifacts/multiarch/$proj/arm64"
    for ext in dll xml; do
        cp "artifacts/bin/$proj/release_linux-arm64/$proj.$ext" "artifacts/multiarch/$proj/arm64/" 2>/dev/null || \
        cp "artifacts/bin/$proj/release/$proj.$ext" "artifacts/multiarch/$proj/arm64/" 2>/dev/null || true
    done
done

# No ref assembly needed - NuGet will select the correct RID-specific assembly
echo "Multi-arch staging complete (no ref assembly - NuGet selects by RID)"

# Pack multi-arch packages (these use pre-staged DLLs via Directory.MultiArch.targets)
echo "Packing multi-arch packages..."
for proj in "${MULTIARCH_PROJECTS[@]}"; do
    echo "Packing $proj..."
    find "artifacts/obj/$proj" -name "*.nuspec" -delete 2>/dev/null || true
    # Only delete exact package name (not prefix matches like Cosmos.Kernel.* which would
    # delete Native, HAL, etc). Anchoring on a digit catches any version (real package
    # names start with a letter after the dot, real versions start with a digit).
    rm -f "artifacts/package/release/${proj}".[0-9]*.nupkg 2>/dev/null || true
    dotnet pack "src/$proj/$proj.csproj" -c Release -o artifacts/package/release -p:NoBuild=true
done

# SDK and Templates
echo "Building SDK and Templates..."
dotnet build src/Cosmos.Sdk/Cosmos.Sdk.csproj -c Release --no-incremental
dotnet build src/Cosmos.Build.Templates/Cosmos.Build.Templates.csproj -c Release --no-incremental

# List all created packages
echo "=== Created packages ==="
ls -la artifacts/package/release/*.nupkg

# Clear Cosmos packages again to ensure fresh restore from local source
echo "Clearing Cosmos packages to force fresh restore..."
rm -rf ~/.nuget/packages/cosmos.* 2>/dev/null || true

# Restore main solution
echo "Restoring main solution..."
dotnet restore ./nativeaot-patcher.slnx

# Install global tools
echo "Installing global tools..."
dotnet tool uninstall -g Cosmos.Patcher 2>/dev/null || true
dotnet tool install -g Cosmos.Patcher --add-source artifacts/package/release
dotnet tool uninstall -g Cosmos.Tools 2>/dev/null || true
dotnet tool install -g Cosmos.Tools --add-source artifacts/package/release

echo "=== PostCreate setup completed (multi-arch) ==="
