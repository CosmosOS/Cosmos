<!-- source_hash: d5ff5f01a22b6c6c817dd35bad89a06658cb6a45 -->
<p align="center">
  <img src="https://user-images.githubusercontent.com/63316499/89792973-43587480-daf3-11ea-99d6-82f89dd2ffc3.png" width="25%" />
</p>

<p align="center">
  <a href="https://chat.gocosmos.org"><img src="https://img.shields.io/badge/chat-officiel-blue?logo=matrix" /></a>
  <a href="https://github.com/CosmosOS/Cosmos/actions/workflows/dotnet.yml"><img src="https://img.shields.io/github/actions/workflow/status/CosmosOS/Cosmos/dotnet.yml?branch=gen3&label=tests%20.NET&logo=github" /></a>
  <a href="https://github.com/CosmosOS/Cosmos/actions/workflows/kernel-tests.yml"><img src="https://img.shields.io/github/actions/workflow/status/CosmosOS/Cosmos/kernel-tests.yml?branch=gen3&label=tests%20du%20noyau&logo=github" /></a>
  <a href="https://github.com/CosmosOS/Cosmos/actions/workflows/release.yml"><img src="https://img.shields.io/github/actions/workflow/status/CosmosOS/Cosmos/release.yml?label=publication&logo=github" /></a>
</p>

[English](README.md) | Français | [简体中文](README_CN.md) | [Polski](README_PL.md)

# Cosmos gen3

Un framework de noyau C# bare-metal construit sur **NativeAOT**. Cosmos gen3 est la nouvelle génération du projet de système d'exploitation [Cosmos](https://github.com/CosmosOS/Cosmos) : il remplace le transpileur IL2CPU par le compilateur AOT (ahead-of-time) officiel de .NET. Le résultat est un `dotnet build` ordinaire qui produit un ELF de noyau amorçable pour **x64 ou ARM64**, lié à un runtime intégré, adapté par le système de plugs de Cosmos et empaqueté dans une ISO avec le chargeur d'amorçage Limine.

À l'origine basé sur le [patcher NativeAOT de Zarlo](https://gitlab.com/liquip/nativeaot-patcher). Consultez [CosmosOS/Cosmos#3088](https://github.com/CosmosOS/Cosmos/issues/3088) pour la discussion de conception qui a mené à gen3.

## Pourquoi gen3 ?

Cosmos gen2 compilait l'IL C# en assembleur x86 au moyen d'**IL2CPU**, un transpileur maison. IL2CPU est puissant, mais il entretient son propre backend, proche d'un JIT, à l'écart de l'écosystème .NET. Gen3 le remplace par **NativeAOT**, la chaîne d'outils AOT officielle de .NET : les noyaux profitent ainsi du même optimiseur que le reste de l'écosystème .NET et suivent les évolutions de .NET en amont. Cela permet aussi de prendre en charge les fonctionnalités modernes de .NET et d'autres architectures (ARM64 aujourd'hui, RISC-V à l'avenir) sans avoir à les réimplémenter dans la chaîne d'outils.

## Fonctionnalités

- Compilation NativeAOT
- x64 et ARM64
- Protocole d'amorçage [Limine](https://github.com/Limine-Bootloader/Limine)
- [Système de plugs de Cosmos](https://cosmosos.github.io/fr/articles/dev/plugs.html)
- Prise en charge du runtime .NET
- [Ramasse-miettes à marquage-balayage](https://cosmosos.github.io/fr/articles/dev/garbage-collector.html)
- [Ordonnanceur Stride à priorités](https://cosmosos.github.io/fr/articles/dev/scheduler.html)
- Gestion des exceptions
- Interruptions (APIC sur x64, GIC sur ARM64)
- ACPI (avec [LAI](https://github.com/managarm/lai))
- Pilotes PCI et MMIO
- Port série UART
- [Sous-système graphique de Cosmos](https://cosmosos.github.io/fr/articles/user/graphics.html)
- Saisie au [clavier](https://cosmosos.github.io/fr/articles/user/keyboard.html) et à la [souris](https://cosmosos.github.io/fr/articles/user/mouse.html)
- [Pile réseau](https://cosmosos.github.io/fr/articles/user/network.html)
- Pilotes de stockage (AHCI/SATA, NVMe, virtio-blk, stockage de masse USB) avec partitionnement MBR, GPT et EBR
- [Systèmes de fichiers FAT12/16/32, ext2 et ISO 9660](https://cosmosos.github.io/fr/articles/user/filesystem.html) sur un VFS de type Unix (montage, superblocs, inodes, montages en lecture seule)
- Minuteur / horloge

## Documentation

[Site de documentation](https://cosmosos.github.io/fr/index.html) : divisé en un **Guide utilisateur** (créer votre propre OS avec Cosmos) et une **Documentation contributeur** (contribuer à Cosmos lui-même, fonctionnement interne de l'architecture).

## Licence

[BSD 3-Clause](LICENSE), la licence d'origine de Cosmos. Copyright (c) 2007-2026, CosmosOS, COSMOS Project.
