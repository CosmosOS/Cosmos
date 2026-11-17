---
source_hash: 86a75862e5053e5fdca2ca093d3a1a3f11e645a6
---
<a id="credits"></a>

# Crédits

Cosmos Gen3 repose sur les épaules d'autres projets open source. Cette page liste les composants tiers livrés avec Cosmos, avec leurs licences.

<a id="vendored-source-code"></a>

## Code source intégré

Code copié dans ce dépôt (les adaptations locales sont notées dans un `README.md` placé à côté des sources) :

| Projet | Licence | Utilisé pour | Emplacement |
|---|---|---|---|
| [BigGustave](https://github.com/EliotJones/BigGustave) | Unlicense | Décodage PNG (`Cosmos.Kernel.System.Graphics.Png`) | `src/Cosmos.Kernel.System/ThirdParty/BigGustave/` |
| [SharpZipLib](https://github.com/icsharpcode/SharpZipLib) | MIT | Décompression DEFLATE/zlib managée pour le décodeur PNG | `src/Cosmos.Kernel.System/ThirdParty/SharpZipLib/` |
| [LunarFonts](https://github.com/Relfos/LunarFonts) | MIT | Chargement et rastérisation TrueType (`TrueTypeFont`) | `src/Cosmos.Kernel.System/ThirdParty/LunarFonts/` |
| [Spleen](https://github.com/fcambus/spleen) | BSD-2-Clause | La police 16x32 par défaut de la console et du canvas | `src/Cosmos.Kernel.System/Graphics/Fonts/` |
| [Cosm3D](https://github.com/Samma2009/Cosm3D) | MIT | La couche de commandes SVGA3D et le canvas du pilote d'écran VMware SVGA II (`Cosmos.Kernel.Drivers.Pci.Display.VmwareSvga.VmwareSvga3D`, `VmwareSvgaCanvas3D`) | `src/Cosmos.Kernel.Drivers/Pci/Display/VmwareSvga/` : `VmwareSvga3D.cs`, `VmwareSvgaCanvas3D.cs` ainsi que le vocabulaire de `Enums/` et `Structs/` |

L'intégration PNG s'inspire de [CosmosPNG](https://github.com/Szymekk44/CosmosPNG) (Unlicense), qui a le premier apporté BigGustave à Cosmos Gen2, et l'intégration TrueType s'inspire de [CosmosTTF](https://github.com/GoldenretriverYT/CosmosTTF) (MIT), qui y a le premier apporté LunarFonts.

L'intégration de VMware SVGA II provient d'un portage vers gen3 de [Cosm3D](https://github.com/Samma2009/Cosm3D) (une implémentation de la [spécification 3D SVGA II de VMware](https://sourceforge.net/projects/vmware-svga)), le portage reposant lui-même sur le pilote SVGA II de gen2 ; elle se trouve dans `Cosmos.Kernel.Drivers` sous la forme de `VmwareSvgaDriver`, au-dessus du kit de pilotes.

<a id="shipped-binaries-and-dependencies"></a>

## Binaires et dépendances livrés

| Projet | Licence | Utilisé pour |
|---|---|---|
| [Limine](https://github.com/limine-bootloader/limine) | BSD-2-Clause | Le chargeur d'amorçage inclus dans chaque ISO de noyau |
| [.NET Runtime](https://github.com/dotnet/runtime) | MIT | Les bibliothèques de classes de base et le compilateur NativeAOT (ILC) sur lesquels le noyau est construit |
| [Mono.Cecil](https://github.com/jbevain/cecil) | MIT | Lecture et réécriture de l'IL dans `cosmos-patcher` |

<a id="build-and-test-tools"></a>

## Outils de build et de test

Non distribués avec Cosmos, mais nécessaires pour le compiler ou le tester : [YASM](https://yasm.tortall.net/), GNU binutils/GCC, [xorriso](https://www.gnu.org/software/xorriso/) et [QEMU](https://www.qemu.org/).
