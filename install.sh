#!/bin/bash

error() {
  printf '\E[31m'; echo "$@"; printf '\E[0m'
}

if [[ $EUID -ne 0 ]]; then
    error "Please run this script as root"
    exit 1
fi

export InstallDir=/opt/cosmos

echo Building IL2CPU
cd ../IL2CPU
dotnet build -v:q -nologo
dotnet pack -v:q -nologo
cd ../Cosmos
echo Building Cosmos
dotnet pack source/Cosmos.Common -v:q -nologo
dotnet pack source/Cosmos.Debug.Kernel -v:q -nologo
dotnet pack source/Cosmos.Debug.Kernel.Plugs.Asm -v:q -nologo
dotnet pack source/Cosmos.Core -v:q -nologo
dotnet pack source/Cosmos.Core_Asm -v:q -nologo
dotnet pack source/Cosmos.Core_Plugs -v:q -nologo
dotnet pack source/Cosmos.HAL2 -v:q -nologo
dotnet pack source/Cosmos.System2 -v:q -nologo
dotnet pack source/Cosmos.System2_Plugs -v:q -nologo

echo Publishing IL2CPU
cd ../IL2CPU
dotnet publish source/IL2CPU -r linux-x64 --self-contained -v:q -nologo

echo Publishing Cosmos
cd ../Cosmos
dotnet publish source/Cosmos.Core_Plugs -v:q -nologo
dotnet publish source/Cosmos.Debug.Kernel.Plugs.Asm -v:q -nologo
dotnet publish source/Cosmos.HAL2 -v:q -nologo
dotnet publish source/Cosmos.System2_Plugs -v:q -nologo

echo Installing to $InstallDir
mkdir -p $InstallDir/
mkdir -p $InstallDir/Cosmos/
mkdir -p $InstallDir/XSharp/
mkdir -p $InstallDir/XSharp/DebugStub
mkdir -p $InstallDir/Build/
mkdir -p $InstallDir/Build/ISO
mkdir -p $InstallDir/Build/VMware/Workstation
mkdir -p $InstallDir/Cosmos/Packages
mkdir -p $InstallDir/Kernel
mkdir -p $InstallDir/Build/IL2CPU
mkdir -p $InstallDir/Build/HyperV/



cp ../IL2CPU/artifacts/Debug/nupkg/*.nupkg $InstallDir/Cosmos/Packages/
cp artifacts/Debug/nupkg/*.nupkg $InstallDir/Cosmos/
cp ../IL2CPU/source/Cosmos.Core.DebugStub/*.xs $InstallDir/XSharp/DebugStub/

cp Artwork/XSharp/XSharp.ico $InstallDir/XSharp/
cp Artwork/Cosmos.ico $InstallDir/

cp ../IL2CPU/source/IL2CPU/bin/Debug/net5.0/linux-x64/publish/* $InstallDir/Build/IL2CPU/
cp source/Cosmos.Core_Plugs/bin/Debug/net5.0/publish/*.dll $InstallDir/Kernel/
cp source/Cosmos.System2_Plugs/bin/Debug/net5.0/publish/*.dll $InstallDir/Kernel/
cp source/Cosmos.HAL2/bin/Debug/net5.0/publish/*.dll $InstallDir/Kernel/
cp source/Cosmos.Debug.Kernel.Plugs.Asm/bin/Debug/netstandard2.0/publish/*.dll $InstallDir/Kernel/

cp build/HyperV/*.vhdx $InstallDir/Build/HyperV/
cp build/VMWare/Workstation/* $InstallDir/Build/VMware/Workstation/
cp build/syslinux/* $InstallDir/Build/ISO/
cp build/ISO/pxe* $InstallDir/Build/PXE/