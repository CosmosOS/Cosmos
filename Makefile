THISDIR := $(shell dirname $(realpath $(firstword $(MAKEFILE_LIST))))
DESTDIR = /opt/cosmos

IL2CPU_URL = https://github.com/CosmosOS/IL2CPU
XSHARP_URL = https://github.com/CosmosOS/XSharp
COMMON_URL = https://github.com/CosmosOS/Common

IL2CPU_BRANCH = crossplatform
XSHARP_BRANCH = master
COMMON_BRANCH = master

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
	$(MAKE) install

$(IL2CPU_DIR):
	@echo "Cloning Cosmos/IL2CPU"
	$(GIT) $(GITFLAGS) --branch=$(IL2CPU_BRANCH) $(IL2CPU_URL) $(THISDIR)/../IL2CPU

$(XSHARP_DIR):
	@echo "Cloning Cosmos/XSharp"
	$(GIT) $(GITFLAGS) --branch=$(XSHARP_BRANCH) $(XSHARP_URL) $(THISDIR)/../XSharp

$(COMMON_DIR):
	@echo "Cloning Cosmos/Common"
	$(GIT) $(GITFLAGS) --branch=$(COMMON_BRANCH) $(COMMON_URL) $(THISDIR)/../Common

.PHONY: build
build:
	@echo "Building IL2CPU"
	$(DOTNET) build $(IL2CPU_DIR) -o $(IL2CPU_DIR)/source/IL2CPU/bin/publish $(DOTNETFLAGS)
	$(DOTNET) pack $(IL2CPU_DIR)  -o $(IL2CPU_DIR)/source/IL2CPU/bin/publish $(DOTNETFLAGS)

	@echo "Building Cosmos"
	$(DOTNET) pack $(THISDIR)/source/Cosmos.Common -o $(THISDIR)/source/Cosmos.Common/bin/publish $(DOTNETFLAGS)
	$(DOTNET) pack $(THISDIR)/source/Cosmos.Debug.Kernel -o $(THISDIR)/source/Cosmos.Debug.Kernel/bin/publish $(DOTNETFLAGS)
	$(DOTNET) pack $(THISDIR)/source/Cosmos.Debug.Kernel.Plugs.Asm -o $(THISDIR)/source/Cosmos.Debug.Kernel.Plugs.Asm/bin/publish $(DOTNETFLAGS)
	$(DOTNET) pack $(THISDIR)/source/Cosmos.Core -o $(THISDIR)/source/Cosmos.Core/bin/publish $(DOTNETFLAGS)
	$(DOTNET) pack $(THISDIR)/source/Cosmos.Core_Asm -o $(THISDIR)/source/Cosmos.Core_Asm/bin/publish $(DOTNETFLAGS)
	$(DOTNET) pack $(THISDIR)/source/Cosmos.Core_Plugs -o $(THISDIR)/source/Cosmos.Core_Plugs/bin/publish $(DOTNETFLAGS)
	$(DOTNET) pack $(THISDIR)/source/Cosmos.HAL2 -o $(THISDIR)/source/Cosmos.HAL2/bin/publish $(DOTNETFLAGS)
	$(DOTNET) pack $(THISDIR)/source/Cosmos.System2 -o $(THISDIR)/source/Cosmos.System2/bin/publish $(DOTNETFLAGS)
	$(DOTNET) pack $(THISDIR)/source/Cosmos.System2_Plugs -o $(THISDIR)/source/Cosmos.System2_Plugs/bin/publish $(DOTNETFLAGS)
	$(DOTNET) pack $(THISDIR)/source/Cosmos.Build.Tasks -o $(THISDIR)/source/Cosmos.Build.Tasks/bin/publish $(DOTNETFLAGS)

.PHONY: publish
publish:
	@echo "Publishing IL2CPU"
	$(DOTNET) publish $(IL2CPU_DIR)/source/IL2CPU --self-contained -o $(IL2CPU_DIR)/source/IL2CPU/bin/publish $(DOTNETFLAGS)

	@echo "Publishing Cosmos"
	$(DOTNET) publish $(THISDIR)/source/Cosmos.Core_Plugs -o $(THISDIR)/source/Cosmos.Core_Plugs/bin/publish $(DOTNETFLAGS)
	$(DOTNET) publish $(THISDIR)/source/Cosmos.Debug.Kernel.Plugs.Asm -o $(THISDIR)/source/Cosmos.Debug.Kernel.Plugs.Asm/bin/publish $(DOTNETFLAGS)
	$(DOTNET) publish $(THISDIR)/source/Cosmos.HAL2 -o $(THISDIR)/source/Cosmos.HAL2/bin/publish $(DOTNETFLAGS)
	$(DOTNET) publish $(THISDIR)/source/Cosmos.System2_Plugs -o $(THISDIR)/source/Cosmos.System2_Plugs/bin/publish $(DOTNETFLAGS)

.PHONY: install
install:
	@echo "Installing to" $(DESTDIR)
	@mkdir -p $(DESTDIR)/Cosmos
	@mkdir -p $(DESTDIR)/XSharp/DebugStub
	@mkdir -p $(DESTDIR)/Build/ISO
	@mkdir -p $(DESTDIR)/Build/IL2CPU
	@mkdir -p $(DESTDIR)/Build/HyperV
	@mkdir -p $(DESTDIR)/Build/VMware/Workstation
	@mkdir -p $(DESTDIR)/Packages
	@mkdir -p $(DESTDIR)/Kernel
	@cp -r $(IL2CPU_DIR)/artifacts/Debug/nupkg/*.nupkg $(DESTDIR)/Packages/
	@cp -r $(THISDIR)/artifacts/Debug/nupkg/*.nupkg $(DESTDIR)/Packages/
	@cp -r $(IL2CPU_DIR)/source/Cosmos.Core.DebugStub/*.xs $(DESTDIR)/XSharp/DebugStub/

	@cp -r $(THISDIR)/Artwork/XSharp/XSharp.ico $(DESTDIR)/XSharp/
	@cp -r $(THISDIR)/Artwork/Cosmos.ico $(DESTDIR)/

	@cp -r $(IL2CPU_DIR)/source/IL2CPU/bin/publish/* $(DESTDIR)/Build/IL2CPU/
	@cp -r $(THISDIR)/source/Cosmos.Core_Plugs/bin/publish/*.dll $(DESTDIR)/Kernel/
	@cp -r $(THISDIR)/source/Cosmos.System2_Plugs/bin/publish/*.dll $(DESTDIR)/Kernel/
	@cp -r $(THISDIR)/source/Cosmos.HAL2/bin/publish/*.dll $(DESTDIR)/Kernel/
	@cp -r $(THISDIR)/source/Cosmos.Debug.Kernel.Plugs.Asm/bin/publish/*.dll $(DESTDIR)/Kernel/

	@cp -r $(THISDIR)/Build/HyperV/*.vhdx $(DESTDIR)/Build/HyperV/
	@cp -r $(THISDIR)/Build/VMWare/Workstation/* $(DESTDIR)/Build/VMware/Workstation/
	@cp -r $(THISDIR)/Build/syslinux/* $(DESTDIR)/Build/ISO/
	@echo $(DESTDIR) > /etc/CosmosUserKit.cfg || true # we dont need to update this each time so an error should be okay

	$(MAKE) nuget-install

.PHONY: nuget-install
nuget-install:
	@echo "Installing Nuget packages"
	$(DOTNET) nuget remove source "Cosmos Local Package Feed" || true
	$(DOTNET) nuget add source $(DESTDIR)/Packages/ -n "Cosmos Local Package Feed"
