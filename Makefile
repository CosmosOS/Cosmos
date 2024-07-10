THISDIR := $(shell dirname $(realpath $(firstword $(MAKEFILE_LIST))))
DESTDIR ?= /opt/cosmos

IL2CPU_URL = https://github.com/CosmosOS/IL2CPU
XSHARP_URL = https://github.com/CosmosOS/XSharp
COMMON_URL = https://github.com/CosmosOS/Common

IL2CPU_BRANCH = master
XSHARP_BRANCH = master
COMMON_BRANCH = master

IL2CPU_DIR = $(THISDIR)/../IL2CPU
XSHARP_DIR = $(THISDIR)/../XSharp
COMMON_DIR = $(THISDIR)/../Common

GIT = git
DOTNET = dotnet

BUILDMODE=Release
GITFLAGS = clone --depth=1
DOTNETFLAGS = -nologo -v:q -c:$(BUILDMODE)
GREEN = \033[0;32m
YELLOW = \033[1;33m
DEFAULT = \033[0m

.PHONY: all
all: $(IL2CPU_DIR) $(XSHARP_DIR) $(COMMON_DIR)
	@printf "${YELLOW}Cosmos${DEFAULT} DevKit Installer\n"
	@# Elapsed time is stored in a temporary file, deleted post-install.
	@date +%s > _time_$@.txt
	@$(MAKE) build
	@$(MAKE) publish
	@sudo $(MAKE) install
	@$(MAKE) nuget-install
	@$(MAKE) template-install
	@printf "To create a Cosmos kernel, run \'dotnet new cosmosCSKernel -n \{name\}\'"
	@printf "Build log file saved to ${GREEN}$(THISDIR)/build${date}.log${DEFAULT}\n"
	@printf "============================================\n"
	@printf "| ${YELLOW}Cosmos${DEFAULT} has been installed successfully!  |\n"
	@printf "============================================\n"
	@printf "Took ${YELLOW}$$(($$(date +%s)-$$(cat  _time_$@.txt)))s${DEFAULT} to build\n"
	@rm _time_$@.txt

$(IL2CPU_DIR):
	@printf "Cloning ${GREEN}Cosmos/IL2CPU${DEFAULT}\n"
	@$(GIT) $(GITFLAGS) --branch=$(IL2CPU_BRANCH) $(IL2CPU_URL) $(IL2CPU_DIR)

$(XSHARP_DIR):
	@printf "Cloning ${GREEN}Cosmos/XSharp${DEFAULT}\n"
	@$(GIT) $(GITFLAGS) --branch=$(XSHARP_BRANCH) $(XSHARP_URL) $(XSHARP_DIR)

$(COMMON_DIR):
	@printf "Cloning ${GREEN}Cosmos/Common${DEFAULT}\n"
	@$(GIT) $(GITFLAGS) --branch=$(COMMON_BRANCH) $(COMMON_URL) $(COMMON_DIR)


.PHONY: build
build:
	@printf "Building ${GREEN}IL2CPU${DEFAULT}\n"
	@$(DOTNET) clean $(IL2CPU_DIR)
	@$(DOTNET) build $(IL2CPU_DIR) $(DOTNETFLAGS)
	@$(DOTNET) pack $(IL2CPU_DIR) $(DOTNETFLAGS)

	@printf "Building ${GREEN}Cosmos${DEFAULT}\n"

	@$(DOTNET) clean $(THISDIR)/source/Cosmos.Common
	@$(DOTNET) clean $(THISDIR)/source/Cosmos.Debug.Kernel
	@$(DOTNET) clean $(THISDIR)/source/Cosmos.Debug.Kernel.Plugs.Asm
	@$(DOTNET) clean $(THISDIR)/source/Cosmos.Core
	@$(DOTNET) clean $(THISDIR)/source/Cosmos.Core_Asm
	@$(DOTNET) clean $(THISDIR)/source/Cosmos.Core_Plugs
	@$(DOTNET) clean $(THISDIR)/source/Cosmos.HAL2
	@$(DOTNET) clean $(THISDIR)/source/Cosmos.System2
	@$(DOTNET) clean $(THISDIR)/source/Cosmos.System2_Plugs
	@$(DOTNET) clean $(THISDIR)/source/Cosmos.Build.Tasks
	@$(DOTNET) clean $(THISDIR)/source/Cosmos.Plugs

	@$(DOTNET) pack $(THISDIR)/source/Cosmos.Common $(DOTNETFLAGS)
	@$(DOTNET) pack $(THISDIR)/source/Cosmos.Debug.Kernel $(DOTNETFLAGS)
	@$(DOTNET) pack $(THISDIR)/source/Cosmos.Debug.Kernel.Plugs.Asm $(DOTNETFLAGS)
	@$(DOTNET) pack $(THISDIR)/source/Cosmos.Core $(DOTNETFLAGS)
	@$(DOTNET) pack $(THISDIR)/source/Cosmos.Core_Asm $(DOTNETFLAGS)
	@$(DOTNET) pack $(THISDIR)/source/Cosmos.Core_Plugs $(DOTNETFLAGS)
	@$(DOTNET) pack $(THISDIR)/source/Cosmos.HAL2 $(DOTNETFLAGS)
	@$(DOTNET) pack $(THISDIR)/source/Cosmos.System2 $(DOTNETFLAGS)
	@$(DOTNET) pack $(THISDIR)/source/Cosmos.System2_Plugs $(DOTNETFLAGS)
	@$(DOTNET) pack $(THISDIR)/source/Cosmos.Build.Tasks $(DOTNETFLAGS)
	@$(DOTNET) pack $(THISDIR)/source/Cosmos.Plugs $(DOTNETFLAGS)

	@printf "Building ${GREEN}X#${DEFAULT}\n"
	@$(DOTNET) clean $(XSHARP_DIR)/source/XSharp/XSharp
	@$(DOTNET) clean $(XSHARP_DIR)/source/Spruce

	$(DOTNET) pack $(XSHARP_DIR)/source/XSharp/XSharp $(DOTNETFLAGS)
	$(DOTNET) pack $(XSHARP_DIR)/source/Spruce $(DOTNETFLAGS)


.PHONY: publish
publish:
	@printf "Publishing ${GREEN}IL2CPU${DEFAULT}\n"
	$(DOTNET) publish $(IL2CPU_DIR)/source/IL2CPU -r linux-x64 --self-contained $(DOTNETFLAGS)

	@printf "Publishing ${GREEN}Cosmos${DEFAULT}\n"
	@$(DOTNET) publish $(THISDIR)/source/Cosmos.Core_Plugs $(DOTNETFLAGS)
	@$(DOTNET) publish $(THISDIR)/source/Cosmos.Debug.Kernel.Plugs.Asm $(DOTNETFLAGS)
	@$(DOTNET) publish $(THISDIR)/source/Cosmos.HAL2 $(DOTNETFLAGS)
	@$(DOTNET) publish $(THISDIR)/source/Cosmos.System2_Plugs $(DOTNETFLAGS)
	@$(DOTNET) publish $(THISDIR)/source/Cosmos.Plugs $(DOTNETFLAGS)

	@printf "Publishing ${GREEN}X#${DEFAULT}\n"
	@$(DOTNET) publish $(XSHARP_DIR)/source/XSharp/XSharp $(DOTNETFLAGS)
	@$(DOTNET) publish $(XSHARP_DIR)/source/Spruce $(DOTNETFLAGS)

.PHONY: install
install:
	@printf "Installing to ${YELLOW}$(DESTDIR)${DEFAULT}\n"
	@mkdir -p $(DESTDIR)/Cosmos
	@mkdir -p $(DESTDIR)/XSharp/DebugStub
	@mkdir -p $(DESTDIR)/Build/ISO
	@mkdir -p $(DESTDIR)/Build/IL2CPU
	@mkdir -p $(DESTDIR)/Build/HyperV
	@mkdir -p $(DESTDIR)/Build/VMware/Workstation
	@mkdir -p $(DESTDIR)/Packages
	@mkdir -p $(DESTDIR)/Kernel
	@cp -r $(IL2CPU_DIR)/artifacts/$(BUILDMODE)/nupkg/*.nupkg $(DESTDIR)/Packages/
	@cp -r $(THISDIR)/artifacts/$(BUILDMODE)/nupkg/*.nupkg $(DESTDIR)/Packages/
	@cp -r $(XSHARP_DIR)/artifacts/$(BUILDMODE)/nupkg/*.nupkg $(DESTDIR)/Packages/
	@cp -r $(IL2CPU_DIR)/source/Cosmos.Core.DebugStub/*.xs $(DESTDIR)/XSharp/DebugStub/

	@cp -r $(THISDIR)/Artwork/XSharp/XSharp.ico $(DESTDIR)/XSharp/
	@cp -r $(THISDIR)/Artwork/Cosmos.ico $(DESTDIR)/

	@cp -r $(IL2CPU_DIR)/source/IL2CPU/bin/$(BUILDMODE)/*/linux-x64/publish/* $(DESTDIR)/Build/IL2CPU/
	@cp -r $(THISDIR)/source/Cosmos.Core_Plugs/bin/$(BUILDMODE)/*/publish/*.dll $(DESTDIR)/Kernel/
	@cp -r $(THISDIR)/source/Cosmos.System2_Plugs/bin/$(BUILDMODE)/*/publish/*.dll $(DESTDIR)/Kernel/
	@cp -r $(THISDIR)/source/Cosmos.HAL2/bin/$(BUILDMODE)/*/publish/*.dll $(DESTDIR)/Kernel/
	@cp -r $(THISDIR)/source/Cosmos.Debug.Kernel.Plugs.Asm/bin/$(BUILDMODE)/netstandard2.0/publish/*.dll $(DESTDIR)/Kernel/

	@cp -r $(THISDIR)/Build/HyperV/*.vhdx $(DESTDIR)/Build/HyperV/
	@cp -r $(THISDIR)/Build/VMWare/Workstation/* $(DESTDIR)/Build/VMware/Workstation/
	@cp -r $(THISDIR)/Build/syslinux/* $(DESTDIR)/Build/ISO/
	@printf $(DESTDIR) > /etc/CosmosUserKit.cfg

.PHONY: nuget-install
nuget-install:
	@printf "Installing ${GREEN}Nuget packages${DEFAULT}\n"

	@rm -r -f ~/.nuget/packages/cosmos.*/
	@rm -r -f ~/.nuget/packages/il2cpu.*/

	@$(DOTNET) nuget remove source "Cosmos Local Package Feed" || true
	@$(DOTNET) nuget add source $(DESTDIR)/Packages/ -n "Cosmos Local Package Feed"

.PHONY: template-install
template-install:
	@printf "Installing ${GREEN}C# Template packages${DEFAULT}\n"
	@-dotnet new --uninstall $(THISDIR)/source/templates/csharp/
	@printf "If the template was not installed, you can ignore this\n"
	@dotnet new -i $(THISDIR)/source/templates/csharp/
#TODO: Uninstall
