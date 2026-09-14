ARCH       ?= x64
TIMEOUT    ?= 30
KERNEL     ?= HelloWorld

OUTPUT     := ./output-$(ARCH)

ifeq ($(ARCH),arm64)
  RID          := linux-arm64
  DEFINE       := ARCH_ARM64
  COSMOS_ARCH  := arm64
  QEMU         := qemu-system-aarch64 -M virt,gic-version=3 -cpu cortex-a72 -m 512M \
                    -bios ~/.cosmos/tools/qemu/share/qemu/edk2-aarch64-code.fd
  DEV_QEMU     := qemu-system-aarch64 -M virt,highmem=off,gic-version=3 -cpu cortex-a53 -m 1G \
                    -bios ~/.cosmos/tools/qemu/share/qemu/edk2-aarch64-code.fd
  DEV_ISO_FLAGS := -drive if=none,id=cd,file=$(OUTPUT)/DevKernel.iso \
                   -device virtio-scsi-pci -device scsi-cd,drive=cd,bootindex=0 \
                   -device virtio-keyboard-device -device virtio-mouse-device \
                   -device ramfb -serial stdio
  DEV_READY    := QEMU-DEVKERNEL-ARM64-DEBUG-READY
else
  RID          := linux-x64
  DEFINE       := ARCH_X64
  COSMOS_ARCH  := x64
  # KVM when the host exposes it (TCG is ~10x slower and tanks guest FPS);
  # -cpu host needs KVM, so the TCG fallback keeps -cpu max (CI, containers).
  KVM_FLAGS    := $(shell test -w /dev/kvm && echo "-enable-kvm -cpu host" || echo "-cpu max")
  QEMU         := qemu-system-x86_64 $(KVM_FLAGS)
  DEV_QEMU     := qemu-system-x86_64 -M q35 $(KVM_FLAGS) -m 512M
  DEV_ISO_FLAGS := -drive file=$(OUTPUT)/DevKernel.iso,if=none,id=cosmoscd,format=raw,readonly=on \
                   -device ide-cd,drive=cosmoscd,bootindex=0 \
                   -vga std -device i8042 -serial stdio
  DEV_READY    := QEMU-DEVKERNEL-DEBUG-READY
endif

DEVKERNEL  := ./examples/DevKernel/DevKernel.csproj
TEST_ENGINE := ./tests/Cosmos.TestRunner.Engine/Cosmos.TestRunner.Engine.csproj

DOCS_DIR   := ./docs
DOCS_PORT  ?= 8080
# Cosmos.Kernel transitively project-references every project DocFX reads, so
# restoring it alone covers the metadata step. Restoring the slnx instead pulls
# in examples/DevKernel, whose NuGet.Config needs artifacts/package/release.
DOCS_RESTORE := ./src/Cosmos.Kernel/Cosmos.Kernel.csproj

AHCI_IMG   := disk-ahci.img
NVME_IMG   := disk-nvme.img
DEV_DISK_FLAGS := -drive file=$(AHCI_IMG),if=none,id=ahcidisk,format=raw \
                  -device ich9-ahci,id=ahci0 \
                  -device ide-hd,drive=ahcidisk,bus=ahci0.0 \
                  -drive file=$(NVME_IMG),if=none,id=nvmedisk,format=raw \
                  -device nvme,drive=nvmedisk,serial=cosmos-nvme

.PHONY: setup build clean distclean run run-dev debug-dev disks test test-cache api \
        docs docs-serve docs-clean

setup:
	./.devcontainer/postCreateCommand.sh

# Reseed the PublicAPI.*.txt files of the API-tracked projects: builds the
# project, then applies the RS0016/RS0017 code fixes (declare new public
# symbols, drop deleted ones). Run after any deliberate public surface change.
api:
	dotnet format analyzers src/Cosmos.Kernel.System/Cosmos.Kernel.System.csproj \
		--diagnostics RS0016 RS0017 --severity info
	dotnet format analyzers src/Cosmos.Kernel.Core/Cosmos.Kernel.Core.csproj \
		--diagnostics RS0016 RS0017 --severity info
	dotnet format analyzers src/Cosmos.Kernel.HAL/Cosmos.Kernel.HAL.csproj \
		--diagnostics RS0016 RS0017 --severity info
	dotnet format analyzers src/Cosmos.Kernel.HAL.Interfaces/Cosmos.Kernel.HAL.Interfaces.csproj \
		--diagnostics RS0016 RS0017 --severity info

build:
	dotnet publish -c Debug -r $(RID) \
		-p:DefineConstants="$(DEFINE)" -p:CosmosArch=$(COSMOS_ARCH) \
		$(DEVKERNEL) -o $(OUTPUT)

clean:
	rm -rf ./output-x64 ./output-arm64 uart.log

distclean: clean
	rm -rf ./artifacts
	dotnet nuget remove source local-packages 2>/dev/null || true
	rm -rf ~/.nuget/packages/cosmos.* 2>/dev/null || true

run: build
	@echo "Starting QEMU ($(ARCH))... Press Ctrl+A X to exit."
	@QEMU_PID=""; \
	trap 'test -n "$$QEMU_PID" && kill $$QEMU_PID 2>/dev/null || true' EXIT; \
	$(QEMU) \
	  -cdrom $(OUTPUT)/DevKernel.iso \
	  -boot d \
	  -m 512M \
	  -serial file:uart.log \
	  -nographic \
	  -no-reboot \
	  -no-shutdown & \
	QEMU_PID=$$!; \
	sleep $(TIMEOUT); \
	echo "Stopping QEMU..."; \
	kill $$QEMU_PID 2>/dev/null || true

$(AHCI_IMG) $(NVME_IMG):
	truncate -s 256M $@

disks: $(AHCI_IMG) $(NVME_IMG)

run-dev: build disks
	$(DEV_QEMU) $(DEV_ISO_FLAGS) $(DEV_DISK_FLAGS) -no-reboot -no-shutdown

debug-dev: build disks
# @ so make does NOT echo the recipe text: the VSCode background
# matcher's endsPattern would fire on the echoed $(DEV_READY) before
# QEMU listens on :1234, bypassing the sleep gate and handing gdb a
# connection-refused. The deliberate post-sleep echo below must be the
# only READY occurrence in the output.
	@$(DEV_QEMU) $(DEV_ISO_FLAGS) $(DEV_DISK_FLAGS) -s -S -no-reboot -no-shutdown & \
	sleep 1; \
	echo "$(DEV_READY)"; \
	wait

test:
	dotnet build $(TEST_ENGINE) -c Debug
	dotnet run --project $(TEST_ENGINE) --no-build \
		-- tests/Kernels/Cosmos.Kernel.Tests.$(KERNEL) $(ARCH) $(TIMEOUT) \
		test-results-$(KERNEL)-$(ARCH).xml ci

test-cache:
	dotnet test tests/Cosmos.Tests.BuildCache/ -c Debug

# Generate the API metadata and render the static site into docs/_site.
docs:
	@command -v docfx >/dev/null 2>&1 || dotnet tool update -g docfx
	dotnet restore $(DOCS_RESTORE)
	cd $(DOCS_DIR) && docfx docfx.json

# Serve the built site on http://localhost:$(DOCS_PORT) (Ctrl+C to stop).
docs-serve: docs
	@echo "Serving docs on http://localhost:$(DOCS_PORT) - Ctrl+C to stop."
	docfx serve $(DOCS_DIR)/_site --port $(DOCS_PORT)

# Drop the generated site and the generated api/*.yml (both gitignored).
docs-clean:
	rm -rf $(DOCS_DIR)/_site
	find $(DOCS_DIR)/api -name '*.yml' -delete
	rm -f $(DOCS_DIR)/api/.manifest
