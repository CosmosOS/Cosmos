THISDIR := $(shell dirname $(realpath $(firstword $(MAKEFILE_LIST))))
DESTDIR ?= /opt/cosmos

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
	$(DOTNET) clean $(IL2CPU_DIR)
	$(DOTNET) build $(IL2CPU_DIR) $(DOTNETFLAGS)
	$(DOTNET) pack $(IL2CPU_DIR) $(DOTNETFLAGS)

	@echo "Building Cosmos"
	$(DOTNET) clean $(THISDIR)/source/Cosmos.Common
	$(DOTNET) clean $(THISDIR)/source/Cosmos.Debug.Kernel
	$(DOTNET) clean $(THISDIR)/source/Cosmos.Debug.Kernel.Plugs.Asm
	$(DOTNET) clean $(THISDIR)/source/Cosmos.Core
	$(DOTNET) clean $(THISDIR)/source/Cosmos.Core_Asm
	$(DOTNET) clean $(THISDIR)/source/Cosmos.Core_Plugs
	$(DOTNET) clean $(THISDIR)/source/Cosmos.HAL2
	$(DOTNET) clean $(THISDIR)/source/Cosmos.System2
	$(DOTNET) clean $(THISDIR)/source/Cosmos.System2_Plugs
	$(DOTNET) clean $(THISDIR)/source/Cosmos.Build.Tasks


	$(DOTNET) pack $(THISDIR)/source/Cosmos.Common $(DOTNETFLAGS)
	$(DOTNET) pack $(THISDIR)/source/Cosmos.Debug.Kernel $(DOTNETFLAGS)
	$(DOTNET) pack $(THISDIR)/source/Cosmos.Debug.Kernel.Plugs.Asm $(DOTNETFLAGS)
	$(DOTNET) pack $(THISDIR)/source/Cosmos.Core $(DOTNETFLAGS)
	$(DOTNET) pack $(THISDIR)/source/Cosmos.Core_Asm $(DOTNETFLAGS)
	$(DOTNET) pack $(THISDIR)/source/Cosmos.Core_Plugs $(DOTNETFLAGS)
	$(DOTNET) pack $(THISDIR)/source/Cosmos.HAL2 $(DOTNETFLAGS)
	$(DOTNET) pack $(THISDIR)/source/Cosmos.System2 $(DOTNETFLAGS)
	$(DOTNET) pack $(THISDIR)/source/Cosmos.System2_Plugs $(DOTNETFLAGS)
	$(DOTNET) pack $(THISDIR)/source/Cosmos.Build.Tasks $(DOTNETFLAGS)

.PHONY: publish
publish:
	@echo "Publishing IL2CPU"
	$(DOTNET) publish $(IL2CPU_DIR)/source/IL2CPU -r linux-x64 --self-contained $(DOTNETFLAGS)

	@echo "Publishing Cosmos"
	$(DOTNET) publish $(THISDIR)/source/Cosmos.Core_Plugs $(DOTNETFLAGS)
	$(DOTNET) publish $(THISDIR)/source/Cosmos.Debug.Kernel.Plugs.Asm $(DOTNETFLAGS)
	$(DOTNET) publish $(THISDIR)/source/Cosmos.HAL2 $(DOTNETFLAGS)
	$(DOTNET) publish $(THISDIR)/source/Cosmos.System2_Plugs $(DOTNETFLAGS)

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

	@cp -r $(IL2CPU_DIR)/source/IL2CPU/bin/Debug/*/linux-x64/publish/* $(DESTDIR)/Build/IL2CPU/
	@cp -r $(THISDIR)/source/Cosmos.Core_Plugs/bin/Debug/*/publish/*.dll $(DESTDIR)/Kernel/
	@cp -r $(THISDIR)/source/Cosmos.System2_Plugs/bin/Debug/*/publish/*.dll $(DESTDIR)/Kernel/
	@cp -r $(THISDIR)/source/Cosmos.HAL2/bin/Debug/*/publish/*.dll $(DESTDIR)/Kernel/
	@cp -r $(THISDIR)/source/Cosmos.Debug.Kernel.Plugs.Asm/bin/Debug/netstandard2.0/publish/*.dll $(DESTDIR)/Kernel/

	@cp -r $(THISDIR)/Build/HyperV/*.vhdx $(DESTDIR)/Build/HyperV/
	@cp -r $(THISDIR)/Build/VMWare/Workstation/* $(DESTDIR)/Build/VMware/Workstation/
	@cp -r $(THISDIR)/Build/syslinux/* $(DESTDIR)/Build/ISO/
ifneq ($(shell id -u), 0)
# remove old packages from cache
# change this when https://github.com/NuGet/Home/issues/5713 is done
	@rm -rf ~/.nuget/packages/cosmos.*
	@rm -rf ~/.nuget/packages/ip2cpu.*
	@rm -rf ~/.nuget/packages/spruce
	@rm -rf ~/.nuget/packages/xsharp
ifeq ("$(wildcard $(/bin/sudo))","")
	@echo "using sudo to update /etc/CosmosUserKit.cfg"
	@echo $(DESTDIR) | sudo tee /etc/CosmosUserKit.cfg > /dev/null
else
ifneq ("$(wildcard $(/etc/CosmosUserKit.cfg))","")
	@echo "not running as root you need to make a file at /etc/CosmosUserKit.cfg with the content >" $(DESTDIR)
else
	@echo "/etc/CosmosUserKit.cfg not updated"
endif
endif
else
	@sudo echo $(DESTDIR) > /etc/CosmosUserKit.cfg
endif
	@echo "if this is your first time installing cosmos you will want to run 'make nuget-install'"

.PHONY: nuget-install
nuget-install:
	@echo "Installing Nuget packages"
	$(DOTNET) nuget remove source "Cosmos Local Package Feed" || true
	$(DOTNET) nuget add source $(DESTDIR)/Packages/ -n "Cosmos Local Package Feed"
