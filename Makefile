THISDIR := $(shell dirname $(realpath $(firstword $(MAKEFILE_LIST))))
DESTDIR = /opt/cosmos

IL2CPU_URL = https://github.com/CosmosOS/IL2CPU --branch=master
XSHARP_URL = https://github.com/CosmosOS/XSharp --branch=master
COMMON_URL = https://github.com/CosmosOS/Common --branch=master

IL2CPU_DIR = $(THISDIR)/../IL2CPU
XSHARP_DIR = $(THISDIR)/../XSharp
COMMON_DIR = $(THISDIR)/../Common

GIT = git
DOTNET = dotnet

GITFLAGS = clone --depth=1
DOTNETFLAGS = -v:q -nologo

.PHONY: all
all: $(IL2CPU_DIR) $(XSHARP_DIR) $(COMMON_DIR)
	$(MAKE) build
	$(MAKE) publish
	@if ! [ "$(shell id -u)" = 0 ];then
		sudo $(MAKE) install
		exit 0
	fi
	$(MAKE) install

$(IL2CPU_DIR):
	@echo "Cloning Cosmos/IL2CPU"
	$(GIT) $(GITFLAGS) $(IL2CPU_URL) $(THISDIR)/../IL2CPU

$(XSHARP_DIR):
	@echo "Cloning Cosmos/XSharp"
	$(GIT) $(GITFLAGS) $(XSHARP_URL) $(THISDIR)/../XSharp

$(COMMON_DIR):
	@echo "Cloning Cosmos/Common"
	$(GIT) $(GITFLAGS) $(COMMON_URL) $(THISDIR)/../Common

.PHONY: build
build:
	@echo "Building IL2CPU"
	$(DOTNET) build $(IL2CPU_DIR) $(DOTNETFLAGS)
	$(DOTNET) pack $(IL2CPU_DIR) $(DOTNETFLAGS)

	@echo "Building Cosmos"
	$(DOTNET) pack $(THISDIR)/source/Cosmos.Common $(DOTNETFLAGS)
	$(DOTNET) pack $(THISDIR)/source/Cosmos.Debug.Kernel $(DOTNETFLAGS)
	$(DOTNET) pack $(THISDIR)/source/Cosmos.Debug.Kernel.Plugs.Asm $(DOTNETFLAGS)
	$(DOTNET) pack $(THISDIR)/source/Cosmos.Core $(DOTNETFLAGS)
	$(DOTNET) pack $(THISDIR)/source/Cosmos.Core_Asm $(DOTNETFLAGS)
	$(DOTNET) pack $(THISDIR)/source/Cosmos.Core_Plugs $(DOTNETFLAGS)
	$(DOTNET) pack $(THISDIR)/source/Cosmos.HAL2 $(DOTNETFLAGS)
	$(DOTNET) pack $(THISDIR)/source/Cosmos.System2 $(DOTNETFLAGS)
	$(DOTNET) pack $(THISDIR)/source/Cosmos.System2_Plugs $(DOTNETFLAGS)

.PHONY: publish
publish:
	@echo "Publishing IL2CPU
	$(DOTNET) publish $(IL2CPU_DIR)/source/IL2CPU -r linux-x64 --self-contained $(DOTNETFLAGS)

	@echo "Publishing Cosmos"
	$(DOTNET) publish $(THISDIR)/source/Cosmos.Core_Plugs $(DOTNETFLAGS)
	$(DOTNET) publish $(THISDIR)/source/Cosmos.Debug.Kernel.Plugs.Asm $(DOTNETFLAGS)
	$(DOTNET) publish $(THISDIR)/source/Cosmos.HAL2 $(DOTNETFLAGS)
	$(DOTNET) publish $(THISDIR)/source/Cosmos.System2_Plugs $(DOTNETFLAGS)

.PHONY: install
install:
	@echo "Installing to" $(DESTDIR)
	@mkdir -p $(DESTDIR)/Cosmos/
	@mkdir -p $(DESTDIR)/XSharp/DebugStub
	@mkdir -p $(DESTDIR)/Build/ISO
	@mkdir -p $(DESTDIR)/Build/IL2CPU
	@mkdir -p $(DESTDIR)/Build/HyperV/
	@mkdir -p $(DESTDIR)/Build/VMware/Workstation
	@mkdir -p $(DESTDIR)/Cosmos/Packages
	@mkdir -p $(DESTDIR)/Kernel
	@cp $(IL2CPU_DIR)/artifacts/Debug/nupkg/*.nupkg $(DESTDIR)/Cosmos/Packages/
	@cp $(THISDIR)/artifacts/Debug/nupkg/*.nupkg $(DESTDIR)/Cosmos/
	@cp $(IL2CPU_DIR)/source/Cosmos.Core.DebugStub/*.xs $(DESTDIR)/XSharp/DebugStub/

	@cp $(THISDIR)/Artwork/XSharp/XSharp.ico $(DESTDIR)/XSharp/
	@cp $(THISDIR)/Artwork/Cosmos.ico $(DESTDIR)/

	@cp -r $(IL2CPU_DIR)/source/IL2CPU/bin/Debug/net5.0/linux-x64/publish/* $(DESTDIR)/Build/IL2CPU/
	@cp $(THISDIR)/source/Cosmos.Core_Plugs/bin/Debug/net5.0/publish/*.dll $(DESTDIR)/Kernel/
	@cp $(THISDIR)/source/Cosmos.System2_Plugs/bin/Debug/net5.0/publish/*.dll $(DESTDIR)/Kernel/
	@cp $(THISDIR)/source/Cosmos.HAL2/bin/Debug/net5.0/publish/*.dll $(DESTDIR)/Kernel/
	@cp $(THISDIR)/source/Cosmos.Debug.Kernel.Plugs.Asm/bin/Debug/netstandard2.0/publish/*.dll $(DESTDIR)/Kernel/

	@cp $(THISDIR)/build/HyperV/*.vhdx $(DESTDIR)/Build/HyperV/
	@cp $(THISDIR)/build/VMWare/Workstation/* $(DESTDIR)/Build/VMware/Workstation/
	@cp $(THISDIR)/build/syslinux/* $(DESTDIR)/Build/ISO/