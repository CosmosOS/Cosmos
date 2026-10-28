---
source_hash: 7aa2a3169afd5d41363f78c3362e35ebefe75b76
---
<a id="gen2gen3-feature-comparison"></a>

## Comparaison des fonctionnalités de Gen2 et Gen3

| Fonctionnalité | Gen2 | Gen3 | Remarques |
|---------|------|-------------|-------|
| Accès bas niveau à l'assembleur | ✅ | ✅ | Auparavant X#, désormais assembleur NASM x64 + GAS ARM64. |
| ACPI | ✅ | ✅ | LAI (Lightweight ACPI Implementation) via l'interopérabilité C.  |
| Gestion des interruptions | ✅  | ✅  | x64 : APIC (local + I/O). ARM64 : GIC. |
| Gestion de la mémoire | ✅ | ✅ ||
| Prise en charge des pilotes | ✅ | ✅ | PCI et MMIO au-dessus du kit de pilotes ; le bus de plateforme peuplé à partir d'ACPI, du device tree (ARM64) ou de la table de la machine ; branchement à chaud PCI Express sur les ports racines |
| Ramasse-miettes | ✅ | ✅ | GC par marquage-balayage |
| Système de fichiers | ✅ | ✅ | FAT12/16/32 sur un VFS de style Unix (montage, superblocs, inodes) avec formatage ; partitionnement MBR/GPT/EBR. Exposé via `System.IO.File`/`FileStream`. |
| Fonctionnalités de la bibliothèque de base .NET | 🟡 | 🟡 Partiel | Les types de base fonctionnent (String, Collections, List, Dictionary). Math, Console, DateTime, Random, BitOperations et `System.IO.File` sont couverts par des plugs. |
| Système de plugs | ✅ | ✅  |  |
| Framework de tests | ✅ | ✅  |  |
| Débogueur| ✅ | 🟡 Partiel | Bugs de source link et des variables dans vscode |
| Calcul mathématique accéléré par CPU/FPU | ✅ | ✅ | x64 uniquement : FPU x87 (sin/cos/tan/exp/log/atan) |
| Sous-système graphique Cosmos | ✅ | ✅ | Le framebuffer du firmware via Limine, ainsi que les pilotes d'affichage virtio-gpu, VMware SVGA II, AMD DCN 3.1.5 et graphismes intégrés Intel au-dessus du kit de pilotes (SVGA3D sur celui de VMware). |
| Interface réseau | ✅ | ✅ | |
| Minuteur / horloge | ✅ | ✅ | |
| Saisie au clavier | ✅ | ✅ | Claviers PS/2 (x64), virtio et USB, tous sous forme de pilotes du kit de pilotes |
| Saisie à la souris | ✅ | ✅ | Souris PS/2 (x64), virtio et USB, toutes sous forme de pilotes du kit de pilotes |
| Interface audio | 🟡 | ✅ | Intel HD Audio : lecture WAV, flux générés et `Console.Beep`. Lecture uniquement, un flux par sortie. |

<a id="additional-gen3-features"></a>

## Fonctionnalités supplémentaires de Gen3

Au-delà de la parité avec Gen2, Gen3 apporte de nouvelles capacités :

| Fonctionnalité | État | Remarques |
|---------|--------|-------|
| **Runtime NativeAOT** | 🟡 Partiel | Compilation NativeAOT complète avec le runtime, sans IL2CPU. |
| **Prise en charge d'ARM64** | ✅  |  |
| **Protocole de démarrage Limine** | ✅ |  |
| **Threads et ordonnanceur** | ✅ | Ordonnanceur Stride fondé sur les priorités (x64 + ARM64). Mot-clé `lock` pris en charge. |
| **Commutateurs de fonctionnalité** | ✅ |  |
| **Extension Cosmos pour VS Code** | ✅ |  |
| **Prise en charge USB** | ✅ | Contrôleurs hôtes xHCI, EHCI et OHCI avec hubs et branchement à chaud (x64 + ARM64) sous forme de pilotes du kit de pilotes, avec les pilotes de classe clavier HID boot, souris HID boot et stockage de masse (Bulk-Only Transport). Pas d'UHCI (le contrôleur compagnon USB 1.1 d'Intel, accessible par E/S de port). |

<a id="future-releases"></a>

## Versions futures

Fonctionnalités prévues après la première version :

| Fonctionnalité | État | Remarques |
|---------|--------|-------|
| **SMP (multitraitement symétrique)** | ❌ Non commencé | Démarrage des AP sur plusieurs cœurs, ordonnancement par CPU, équilibreur de charge. |
| **HTTPS** | ❌ Non commencé | Implémentation de TLS/SSL, gestion des certificats, sockets sécurisés. |
| **GC générationnel** | ❌ Non commencé | Remplacer l'actuel marquage-balayage par un collecteur générationnel (Gen0/Gen1/Gen2) pour de meilleures performances. |
| **Exécution de code** | ❌ Non commencé | VM WASM en espace utilisateur |
| **Le GIC, l'ITS et les horloges à partir du device tree** | ❌ Non commencé | Aujourd'hui, le contrôleur d'interruptions ARM64 provient d'ACPI ou des valeurs par défaut de virt, si bien qu'un démarrage avec acpi=off n'a pas de MSI ; le bus de plateforme lit déjà l'arbre. x64 n'en a jamais. |
| **Branchement à chaud PCI au-delà des emplacements PCI Express natifs** | ❌ Non commencé | Branchement à chaud ACPI (GPE), que QEMU utilise sur les ports racines de q35 sans l'option global ICH9-LPC et sur les ponts PCI conventionnels ; les ports aval des commutateurs font l'objet d'une correspondance mais ne sont pas testés. |
