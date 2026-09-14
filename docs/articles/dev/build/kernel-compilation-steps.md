# Kernel Compilation Steps

This document outlines the complete build pipeline that transforms C# kernel code into a bootable ISO image using .NET NativeAOT compilation, custom IL patching, and native code generation.

## Compilation Flow Chart

```mermaid
flowchart TD
    CS[C# Source Code] --> ROSLYN[Roslyn Compiler]
    PLUGS[Plug Definitions] --> ANALYZER[Cosmos.Build.Analyzer.Patcher]
    ANALYZER -.-> |Validates| ROSLYN
    ROSLYN --> IL[IL Assembly]

    subgraph "Cosmos.Build.Patcher"
        PATCHER[IL Patcher]
        PATCHER --> |cosmos.patcher| PATCHED[Patched Assemblies]
        PATCHED --> COSMOS_REF["$(IntermediateOutputPath)/cosmos/ref/"]
        PATCHED --> COSMOS_ROOT["$(IntermediateOutputPath)/cosmos/"]
    end
    IL --> PATCHER
    PLUGS --> PATCHER

    subgraph "Cosmos.Build.Ilc"
        ILC[NativeAOT Compiler]
        ILC --> |ilc| NATIVE["Native Object (.o)"]
        NATIVE --> COSMOS_NATIVE["$(IntermediateOutputPath)/cosmos/native/"]
    end
    COSMOS_REF --> ILC
    COSMOS_ROOT --> ILC

    subgraph "Cosmos.Build.Asm"
        ASM_BUILD[Clang Integrated Assembler]
        ASM_BUILD --> |clang --target=...-elf -c| ASM_OBJ["Assembly Objects (.obj)"]
        ASM_OBJ --> COSMOS_ASM["$(IntermediateOutputPath)/cosmos/asm/"]
    end
    ASM_SRC["Assembly Files (.s)"] --> ASM_BUILD

    subgraph "Cosmos.Build.CC"
        CC_BUILD[Clang Compiler]
        CC_BUILD --> |"clang --target=..."| C_OBJ["C Objects (.obj)"]
        C_OBJ --> COSMOS_COBJ["$(IntermediateOutputPath)/cosmos/cobj/"]
    end
    C_SRC["C Source Files (.c)"] --> CC_BUILD

    subgraph "Cosmos.Build.Common"
        LINK[Link Target]
        LINK --> |ld.lld| ELF["ELF Binary ($(OutputPath)/$(AssemblyName).elf)"]
        ELF --> FINAL_ISO[BuildISO Target]
        FINAL_ISO  --> |xorriso + Limine| ISO_OUT["$(OutputPath)/cosmos/$(AssemblyName).iso"]
        ISO_OUT --> |PublishISO| PUBLISH["$(PublishDir)/$(AssemblyName).iso"]
    end
    
    COSMOS_NATIVE --> LINK
    COSMOS_ASM --> LINK
    COSMOS_COBJ --> LINK
    LINKER_SCRIPT["linker.ld"] --> LINK
    LIMINE_CONF["limine.conf"] --> FINAL_ISO

    style CS fill:#e1f5e1
    style ASM_SRC fill:#e1f5e1
    style C_SRC fill:#e1f5e1
    style PLUGS fill:#e1f5e1
    style LINKER_SCRIPT fill:#e1f5e1
    style LIMINE_CONF fill:#e1f5e1
    
    style PUBLISH fill:#ffe1e1
```

## Prerequisites

| Tool | Purpose | Required Version |
|------|---------|-----------------|
| **.NET SDK** | Core compilation | 10.0+ |
| **Clang** | C and assembly compilation (x64/ARM64 via --target) | LLVM toolchain |
| **ld.lld** | ELF linking | LLVM toolchain |
| **xorriso** | ISO creation | Latest |

## Key Components

- [`Cosmos.Sdk`](https://github.com/valentinbreiz/nativeaot-patcher/blob/main/src/Cosmos.Sdk) - MSBuild SDK orchestration
- [`Cosmos.Build.Patcher`](https://github.com/valentinbreiz/nativeaot-patcher/blob/main/src/Cosmos.Build.Patcher) - IL patching infrastructure
- [`Cosmos.Build.Ilc`](https://github.com/valentinbreiz/nativeaot-patcher/blob/main/src/Cosmos.Build.Ilc) - NativeAOT integration
- [`Cosmos.Build.Asm`](https://github.com/valentinbreiz/nativeaot-patcher/blob/main/src/Cosmos.Build.Asm) - Assembly compilation
- [`Cosmos.Build.CC`](https://github.com/valentinbreiz/nativeaot-patcher/blob/main/src/Cosmos.Build.CC) - C compilation (Clang)
- [`Cosmos.Build.Common`](https://github.com/valentinbreiz/nativeaot-patcher/blob/main/src/Cosmos.Build.Common) - Linking and ISO creation

## Example Project

Reference implementation: `examples/DevKernel/DevKernel.csproj`

## Output Summary

| Stage | Output Path | Content |
|-------|------------|---------|
| Patching | `$(IntermediateOutputPath)/cosmos/` | Main patched assembly |
| Patching | `$(IntermediateOutputPath)/cosmos/ref/` | Reference assemblies |
| NativeAOT | `$(IntermediateOutputPath)/cosmos/native/` | Native object files (.o) |
| Assembly | `$(IntermediateOutputPath)/cosmos/asm/` | Clang assembler objects (.obj) |
| C Code | `$(IntermediateOutputPath)/cosmos/cobj/` | Clang objects (.o/.obj) |
| Linking | `$(OutputPath)/$(AssemblyName).elf` | Linked ELF kernel |
| ISO | `$(OutputPath)/cosmos/$(AssemblyName).iso` | Bootable ISO image |
| Publish | `$(PublishDir)/$(AssemblyName).iso` | Published ISO |

