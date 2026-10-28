---
source_hash: 151b0dcc68bc2f67fba66be3d84577c793378407
---
<a id="kernel-compilation-steps"></a>

# Étapes de compilation du noyau

Ce document décrit le pipeline de build complet qui transforme le code C# du noyau en une image ISO amorçable, au moyen de la compilation .NET NativeAOT, d'un patching IL sur mesure et de la génération de code natif.

<a id="compilation-flow-chart"></a>

## Diagramme du flux de compilation

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

<a id="prerequisites"></a>

## Prérequis

| Outil | Rôle | Version requise |
|------|---------|-----------------|
| **.NET SDK** | Compilation principale | 10.0+ |
| **Clang** | Compilation du C et de l'assembleur (x64/ARM64 via --target) | Chaîne d'outils LLVM |
| **ld.lld** | Édition de liens ELF | Chaîne d'outils LLVM |
| **xorriso** | Création de l'ISO | Dernière version |

<a id="key-components"></a>

## Composants principaux

- [`Cosmos.Sdk`](https://github.com/CosmosOS/Cosmos/blob/gen3/src/Cosmos.Sdk) : orchestration par le SDK MSBuild
- [`Cosmos.Build.Patcher`](https://github.com/CosmosOS/Cosmos/blob/gen3/src/Cosmos.Build.Patcher) : infrastructure de patching IL
- [`Cosmos.Build.Ilc`](https://github.com/CosmosOS/Cosmos/blob/gen3/src/Cosmos.Build.Ilc) : intégration de NativeAOT
- [`Cosmos.Build.Asm`](https://github.com/CosmosOS/Cosmos/blob/gen3/src/Cosmos.Build.Asm) : compilation de l'assembleur
- [`Cosmos.Build.CC`](https://github.com/CosmosOS/Cosmos/blob/gen3/src/Cosmos.Build.CC) : compilation du C (Clang)
- [`Cosmos.Build.Common`](https://github.com/CosmosOS/Cosmos/blob/gen3/src/Cosmos.Build.Common) : édition de liens et création de l'ISO

<a id="example-project"></a>

## Projet d'exemple

Implémentation de référence : `examples/DevKernel/DevKernel.csproj`

<a id="output-summary"></a>

## Récapitulatif des sorties

| Étape | Chemin de sortie | Contenu |
|-------|------------|---------|
| Patching | `$(IntermediateOutputPath)/cosmos/` | Assembly principal patché |
| Patching | `$(IntermediateOutputPath)/cosmos/ref/` | Assemblies de référence |
| NativeAOT | `$(IntermediateOutputPath)/cosmos/native/` | Fichiers objets natifs (.o) |
| Assembleur | `$(IntermediateOutputPath)/cosmos/asm/` | Objets produits par l'assembleur Clang (.obj) |
| Code C | `$(IntermediateOutputPath)/cosmos/cobj/` | Objets Clang (.o/.obj) |
| Édition de liens | `$(OutputPath)/$(AssemblyName).elf` | Noyau ELF lié |
| ISO | `$(OutputPath)/cosmos/$(AssemblyName).iso` | Image ISO amorçable |
| Publication | `$(PublishDir)/$(AssemblyName).iso` | ISO publiée |
