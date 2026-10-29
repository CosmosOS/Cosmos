<p align="center">
  <img src="https://user-images.githubusercontent.com/63316499/89792973-43587480-daf3-11ea-99d6-82f89dd2ffc3.png" width="25%" />
</p>

<p align="center">
  <a href="https://chat.gocosmos.org"><img src="https://img.shields.io/badge/official-chat-blue?logo=matrix" /></a>
  <a href="https://github.com/CosmosOS/Cosmos/actions/workflows/dotnet.yml"><img src="https://img.shields.io/github/actions/workflow/status/CosmosOS/Cosmos/dotnet.yml?branch=gen3&label=.NET%20tests&logo=github" /></a>
  <a href="https://github.com/CosmosOS/Cosmos/actions/workflows/kernel-tests.yml"><img src="https://img.shields.io/github/actions/workflow/status/CosmosOS/Cosmos/kernel-tests.yml?branch=gen3&label=kernel%20tests&logo=github" /></a>
  <a href="https://github.com/CosmosOS/Cosmos/actions/workflows/release.yml"><img src="https://img.shields.io/github/actions/workflow/status/CosmosOS/Cosmos/release.yml?label=release&logo=github" /></a>
</p>

[English](README.md) | [Français](README_FR.md) | [简体中文](README_CN.md) | Polski

# Cosmos gen3

Framework jądra C# bare-metal zbudowany na **NativeAOT**. Cosmos gen3 to następna generacja projektu systemu operacyjnego [Cosmos](https://github.com/CosmosOS/Cosmos), zastępująca transpilator IL2CPU oficjalnym kompilatorem AOT .NET. Efektem jest zwykły `dotnet build`, który tworzy rozruchowe jądro ELF dla architektur **x64 lub ARM64**, zlinkowane ze zintegrowanym środowiskiem uruchomieniowym, zintegrowane z systemem wtyczek Cosmosa i zapakowane do pliku ISO przy użyciu programu rozruchowego Limine.

Projekt pierwotnie powstał na bazie [patchera NativeAOT stworzonego przez Zarlo](https://gitlab.com/liquip/nativeaot-patcher). Zapoznaj się z [CosmosOS/Cosmos#3088](https://github.com/CosmosOS/Cosmos/issues/3088), aby przeczytać dyskusję projektową dotyczącą przedsięwzięcia gen3.

## Dlaczego gen3?

Cosmos gen2 kompilował IL C# do asemblera x86 przy pomocy **IL2CPU**, czyli własnego transpilatora. IL2CPU jest mocny, ale utrzymuje własne zaplecze w rodzaju JIT, odrębne od ekosystemu .NET. W gen3 IL2CPU zostaje zastąpiony przez **NativeAOT** (oficjalnym zestawem narzędzi AOT .NET), dzięki czemu jądra korzystają z tego samego optymalizatora, co szerszy ekosystem .NET, i pozostają zgodne z upstreamem, gdy ten się rozwija. Pozwala to także obsługiwać nowoczesne funkcje .NET oraz dodatkowe architektury (obecnie ARM64, w przyszłości RISC-V) bez ponownego implementowania ich w zestawie narzędzi.

## Funkcje

- Kompilacja NativeAOT
- x64 oraz ARM64
- Protokół rozruchu [Limine](https://github.com/Limine-Bootloader/Limine)
- [System wtyczek Cosmosa](https://cosmosos.github.io/articles/dev/plugs.html)
- Obsługa środowiska uruchomieniowego .NET
- [Garbage collector typu mark-and-sweep](https://cosmosos.github.io/articles/dev/garbage-collector.html)
- [Scheduler typu stride z priorytetami](https://cosmosos.github.io/articles/dev/scheduler.html)
- Obsługa wyjątków
- Przerwania (APIC w x64, GIC w ARM64)
- ACPI (za pomocą [LAI](https://github.com/managarm/lai))
- Sterowniki PCI oraz MMIO
- Port szeregowy UART
- [Podsystem graficzny Cosmos](https://cosmosos.github.io/articles/user/graphics.html)
- Obsługa wejścia z [klawiatury](https://cosmosos.github.io/articles/user/keyboard.html) i [myszy](https://cosmosos.github.io/articles/user/mouse.html)
- [Stos sieciowy](https://cosmosos.github.io/articles/user/network.html)
- Sterowniki pamięci masowej (AHCI/SATA, NVMe, virtio-blk, pamięć masowa USB) z partycjonowaniem MBR, GPT oraz EBR
- [System plików FAT12/16/32](https://cosmosos.github.io/articles/user/filesystem.html) w uniksowym VFS (montowanie, superbloki, i-węzły)
- Timer / Zegar

## Dokumentacja

[Strona dokumentacji](https://cosmosos.github.io/index.html): podzielona na **przewodnik użytkownika** (zbuduj własny system operacyjny przy użyciu Cosmosa) oraz **dokumentację kontrybutora** (współtworzenie samego Cosmosa / wewnętrzna architektura)

## Licencja

[BSD 3-Clause](LICENSE), oryginalna licencja Cosmosa. Prawa autorskie (c) 2007-2026, CosmosOS, Projekt COSMOS.
