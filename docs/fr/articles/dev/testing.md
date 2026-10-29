---
source_hash: edf985c0eddbd5cc55626a43d392ca4bf9ca80bd
---
<a id="testing"></a>

# Tests

NativeAOT-Patcher dispose de deux niveaux de tests complémentaires : des **tests unitaires**, qui s'exécutent dans le processus hôte, pour la chaîne d'outils utilisée au moment du build (patcher, scanner, analyseur) et pour la logique des bibliothèques du noyau qui n'a besoin d'aucun matériel, et des **tests d'intégration du noyau**, qui exécutent des images de noyau compilées dans QEMU et rapportent leurs résultats via un protocole UART binaire.

---

<a id="unit-tests"></a>

## Tests unitaires

Les tests unitaires se trouvent dans les projets `tests/Cosmos.Tests.*` (xunit) et dans `tests/Cosmos.Kernel.Tests.System` (NUnit), et s'exécutent avec l'exécuteur de tests standard de .NET. Ils ne nécessitent ni QEMU ni aucune infrastructure particulière. La CI exécute chacun d'eux comme un job distinct du workflow `.NET Tests` (`.github/workflows/dotnet.yml`) à chaque push et à chaque pull request.

<a id="running-unit-tests"></a>

### Exécuter les tests unitaires

```bash
dotnet test tests/Cosmos.Tests.Patcher            # one toolchain project
dotnet test tests/Cosmos.Kernel.Tests.System      # the kernel library tests
```

<a id="test-projects"></a>

### Projets de test

- **Cosmos.Tests.Build.Asm** : vérifie que la tâche de build de l'assembleur s'exécute via clang.
  - `Test1`
- **Cosmos.Tests.Build.Analyzer.Patcher** : vérifie que le code ne contient pas d'erreurs d'architecture de plug.
  - `Test_AnalyzeAccessedMember`
  - `Test_MethodNotImplemented`
  - `Test_StaticConstructorTooManyParameters`
  - `Test_StaticConstructorNotImplemented`
- **Cosmos.Tests.Scanner** : vérifie que tous les plugs requis sont correctement détectés.
  - `LoadPlugMethods_ShouldReturnPublicStaticMethods`
  - `LoadPlugMethods_ShouldReturnEmpty_WhenNoMethodsExist`
  - `LoadPlugMethods_ShouldContainAddMethod_WhenPlugged`
  - `LoadPlugs_ShouldFindPluggedClasses`
  - `LoadPlugs_ShouldIgnoreClassesWithoutPlugAttribute`
  - `LoadPlugs_ShouldHandleOptionalPlugs`
  - `FindPluggedAssemblies_ShouldReturnMatchingAssemblies`
- **Cosmos.Tests.SourceGenerators** : exécute `CosmosEntryPointGenerator` et `DriverManifestGenerator` sur des compilations en mémoire et vérifie à l'identique le point d'entrée généré, le manifeste des pilotes et les diagnostics `COSMOSGEN` ; `FeatureParityTests` maintient `DriverFeature`, `KernelFeatures` et les stubs de test en phase, `ReferencedDriverTests` couvre les pilotes provenant d'assemblys référencés, publics ou sous une autorisation `InternalsVisibleTo`, et `CachingTests` vérifie que l'ajout d'un pilote laisse le point d'entrée en cache.
- **Cosmos.Tests.Patcher** : s'assure que les plugs sont correctement appliqués aux méthodes et aux types cibles.
  - `PatchAssembly_ShouldSkipWhenNoMatchingPlugs`
  - `PatchObjectWithAThis_ShouldPlugInstanceCorrectly`
  - `PatchConstructor_ShouldPlugCtorCorrectly`
  - `PatchProperty_ShouldPlugProperty`
  - `PatchType_ShouldReplaceAllMethodsCorrectly`
  - `PatchType_ShouldPlugAssembly`
  - `AddMethod_BehaviorBeforeAndAfterPlug`
- **Cosmos.Kernel.Tests.System** : exerce, dans le processus hôte, la logique de `Cosmos.Kernel.System` qui n'a besoin d'aucun matériel (le tampon de réception de `TcpConnection`, l'identité et le formatage d'`Address`, la lecture WAV de `WaveHeader`, `MemoryAudioStream` et `WaveAudioStream`, ce dernier sur un `IVfsFileHandle` en mémoire, et l'onde carrée que `ToneAudioStream` génère pour `Console.Beep`). Une fixture imbriquée par membre testé, avec une autorisation `InternalsVisibleTo` accordée par la bibliothèque.
  - `AppendToData.WhenBothData_AndOtherAreEmpty_DataIsEmpty`
  - `AppendToData.WhenDataIsNotEmpty_AndOtherIsEmpty_DataDoesNotChange`
  - `AppendToData.WhenDataIsNotEmpty_AndOtherIsNotEmpty_OtherIsAppendedToData`
  - `AppendToData.WhenPartOfDataWasRead_AndOtherIsAppended_TheUnreadDataIsKept`
  - `AppendToData.WhenPartOfDataWasRead_AndOtherOutgrowsTheBuffer_TheUnreadDataIsKept`
  - `AppendToData.WhenAllDataWasRead_AndOtherIsAppended_OnlyOtherIsKept`
  - `AdvanceDataOffset.WhenAdvancingByZero_NoChangesAreMade`
  - `AdvanceDataOffset.WhenAdvancingByOneAndLengthIsTwo_OnlyLastElementRemains`
  - `AdvanceDataOffset.WhenAdvancingByTwoAndLengthIsTwo_DataLengthIsZero`
  - `Constructors.GivenPackedValue_SplitsItMostSignificantOctetFirst`
  - `Constructors.GivenBufferAndOffset_ReadsFourBytesFromTheOffset`
  - `Constructors.GivenSpanOfWrongLength_Throws`
  - `Id.GivenOctets_PacksThemMostSignificantFirst`
  - `Equality.GivenTheSameOctets_TwoInstancesAreEqualAndHashAlike`
  - `Equality.GivenDifferentOctets_TwoInstancesAreNotEqual`
  - `Equality.GivenNull_IsNotEqual`
  - `CompareTo.GivenTwoAddresses_OrdersByPackedValue`
  - `CompareTo.GivenNull_OrdersAfterIt`
  - `Formatting.GivenAddress_WritesDottedDecimal`
  - `IsBroadcastAddress.GivenAllOnes_IsTrue`
  - `IsBroadcastAddress.GivenAnyOtherAddress_IsFalse`
- **Cosmos.Tests.NativeWrapper** : contient des ressources du runtime ; aucun test unitaire.
- **Cosmos.Tests.NativeLibrary** : fournit du code natif utilisé dans les tests ; aucun test unitaire.

---

<a id="kernel-integration-tests"></a>

## Tests d'intégration du noyau

Les tests d'intégration du noyau compilent un véritable noyau NativeAOT, le démarrent dans QEMU et renvoient leurs résultats à l'hôte via un protocole UART binaire. Ces tests exercent l'ensemble du pipeline de build et d'exécution.

<a id="test-suites"></a>

### Suites de tests

| Suite | Tests | Description |
|-------|-------|-------------|
| **HelloWorld** | 3 | Arithmétique de base, logique booléenne, comparaison d'entiers |
| **Memory** | 74 | Boxing/unboxing, égalité de structures, allocation mémoire, collections, copie mémoire, traduction d'adresses |
| **Drivers** | 63 par cellule | Kit de pilotes sur le bus synthétique : manifeste, arbitrage, publication, interruptions, travail différé, démontage, enfants, un écran qui atteint le gestionnaire d'écrans, un périphérique bloc qui atteint le gestionnaire de stockage ; sur x64, l'hôte PCI et le pilote E1000E sur la carte réseau par défaut de q35 ; sur la cellule usb-kbd, le pilote xHCI et le pilote de clavier USB sur un usb-kbd branché au démarrage, d'abord décliné par le propre pilote de la suite, avec une touche injectée via QMP, puis débranché et rebranché ; sur les cellules usb-kbd-ehci, usb-kbd-ohci, usb-kbd-ehci-ohci et usb-kbd-ohci-hub, les pilotes EHCI et OHCI et le pilote de clavier USB sur le même clavier, sur un usb-ehci, sur un pci-ohci, sur un EHCI ICH9 qui confie le clavier pleine vitesse à son compagnon pci-ohci, et derrière un usb-hub sur un pci-ohci ; sur la cellule usb-mouse, le pilote de souris USB sur un usb-mouse branché au démarrage, avec des événements de pointeur injectés via QMP ; sur x64, le pilote 8042 et les pilotes de clavier et de souris PS/2 sur le contrôleur intégré de q35, avec une touche et des événements de pointeur injectés via QMP ; sur les cellules virtio-blk-pci et virtio-blk-mmio, le pilote virtio-blk sur le disque de la cellule ; sur virtio-blk-pci, le pilote de port racine PCI Express sur le port derrière lequel se trouve le disque, avec le disque retiré puis rebranché |
| **Pci** | 8 | Le nœud d'hôte PCI du kit de pilotes, son mécanisme de configuration et les nœuds de fonction qu'il publie, lus via `DriverDiagnostics`, le `PciHostAccess` de l'hôte et le `PciAccess` des nœuds, comparés à un parcours brut des bus de l'hôte |
| **Virtio** | 14 par cellule | Les pilotes virtio sur le kit de pilotes, sur les deux transports et avec ACPI désactivé : les nœuds réseau, d'entrée et bloc liés, l'interface consommée avec son lien, son adresse MAC et son mode d'interruption par cellule, le disque enregistré, et la fonction PCI ou l'emplacement MMIO lié par son pilote de transport |
| **Graphic** | 25 | Le canvas 2D, les polices et les images sur chaque cellule ; le gestionnaire d'écrans et l'écran principal par cellule ; le pilote d'écran virtio-gpu sur les cellules virtio-gpu ; l'adaptateur VMware SVGA II via ses facettes et ses tests de flux FIFO SVGA3D sur vmware-svga |
| **Audio** | 22 par cellule | La pile audio sur deux cellules, `bare` et `hda` : le pilote Intel HD Audio lié à l'`intel-hda` de QEMU, la sortie qu'il publie et le gestionnaire audio qui la liste, le contrat de format, le moteur de flux qui vide le tampon circulaire et son interruption de complétion là où la ligne est connectée, ainsi que le lecteur et la lecture en arrière-plan qui mènent un flux jusqu'au bout, en arrêtent un avant la fin et lisent un fichier WAV depuis la mémoire, et `Console.Beep` qui joue sa tonalité sur la sortie et rend la main aussitôt lorsque la sortie est occupée ; sur bare, un gestionnaire sans sortie qui refuse de jouer et un `Console.Beep` qui rend la main aussitôt ; sur les deux, `Console.Beep` qui refuse les arguments que Windows refuse |
| **Storage** | 83 par cellule | Le gestionnaire de stockage et les périphériques bloc qui se trouvent derrière lui, sur sept profils, `ahci`, `nvme`, `usb`, `usb-ehci`, `usb-ohci`, `virtio-blk-pci` et `virtio-blk-mmio` (arm64), chacun combiné avec le modificateur `acpi-off` sur x64 (12 cellules) et avec `gicv2`, `gicv3` et `acpi-off` sur arm64 (42 cellules) : le disque publié par l'`AhciDriver`, le `NvmeDriver`, l'`UsbMassStorageDriver` ou le `VirtioBlkDriver` du kit, soumis aux mêmes assertions d'E/S bloc, de table de partitions, de cycle de vie des partitions et de redémarrage ; la vue du disque par le kit dans `DriverDiagnostics` sur chaque cellule ; le mode d'interruption NVMe par cellule ; le branchement à chaud USB sur les cellules usb et le branchement à chaud PCI Express sur les cellules virtio-blk-pci |
| **Random** | 16 | Le CSPRNG du noyau : tests à réponse connue de BLAKE2s et ChaCha20, effacement rapide de clé, détection de RDSEED/RDRAND et de RNDRRS/RNDR, ensemencement, et `RandomNumberGenerator`, `GetInt32` et `Guid.NewGuid` par-dessus, à travers un réensemencement et depuis deux threads |
| **Iso9660** | 47 | Le pilote ISO 9660 sur deux images que xorriso construit à la compilation depuis les arborescences de fixtures de la suite : montage sur les géométries CD et disque et refus d'un périphérique vide, corrompu ou trop court, noms, attributs, horodatages, liens symboliques, zones de continuation et répertoires relocalisés de Rock Ridge, noms bruts de niveau 1 recherchés sans la casse, lectures entières, par morceaux, après seek et ligne par ligne, le montage en lecture seule via `System.IO` (`IOException`) et `VfsManager` (`false`), démontage et remontage |

<a id="helloworld-tests"></a>

#### Tests HelloWorld

- `Test_BasicArithmetic` : addition (2+2=4)
- `Test_BooleanLogic` : assertions vrai/faux
- `Test_IntegerComparison` : opérateurs d'égalité et de comparaison

<a id="memory-tests"></a>

#### Tests Memory

**Boxing/unboxing (11 tests) :**
- `Boxing_Char`, `Boxing_Int32`, `Boxing_Byte`, `Boxing_Long`
- `Boxing_Nullable`, `Boxing_Interface`, `Boxing_CustomStruct`
- `Boxing_ArrayCopy`, `Boxing_Enum`, `Boxing_ValueTuple`, `Boxing_NullInterface`

**Égalité de structures (3 tests) :** le `ValueType.Equals`/`GetHashCode` hérité et `EqualityComparer<T>.Default` sur une structure comparée octet par octet, sur un `record struct` doté d'un champ de type référence (son propre `Equals` généré), et sur une structure simple dotée d'un champ de type référence, que CoreLib compare champ par champ via le `__GetFieldHelper` injecté par ILC et `RhBoxAny`
- `StructEquals_BitwiseFields`, `StructEquals_RecordStructReferenceField`, `StructEquals_ReferenceField`

**Allocation mémoire (8 tests) :**
- `Memory_CharArray`, `Memory_StringAllocation`, `Memory_IntArray`
- `Memory_StringConcat`, `Memory_StringBuilder`
- `Memory_ZeroLengthArray`, `Memory_EmptyString`, `Memory_LargeAllocation`

**Collections génériques - List (14 tests) :**
- `Collections_ListInt`, `Collections_ListString`, `Collections_ListByte`
- `Collections_ListLong`, `Collections_ListStruct`
- `Collections_ListContains`, `Collections_ListIndexOf`, `Collections_ListRemoveAt`
- `Collections_ListInsert`, `Collections_ListRemove`, `Collections_ListClear`
- `Collections_ListToArray`, `Collections_ListForeach`, `Collections_ListEmpty`

**Collections génériques - Dictionary (9 tests) :**
- `Collections_DictCustomComparer`, `Collections_DictAddGet`, `Collections_DictIndexer`
- `Collections_DictContains`, `Collections_DictRemove`, `Collections_DictClear`
- `Collections_DictTryGetValue`, `Collections_DictKeysValues`, `Collections_DictEmpty`

**Collections génériques - IEnumerable (1 test) :**
- `Collections_IEnumerable`

**Copie mémoire / SIMD (15 tests) :**
- `MemCopy_8Bytes`, `MemCopy_16Bytes`, `MemCopy_24Bytes`, `MemCopy_32Bytes`
- `MemCopy_48Bytes`, `MemCopy_64Bytes`, `MemCopy_80Bytes`, `MemCopy_128Bytes`
- `MemCopy_256Bytes`, `MemCopy_264Bytes`
- `MemSet_64Bytes`, `MemMove_Overlap`, `MemMove_Overlap_DestBeforeSrc`
- `MemCopy_0Bytes`, `MemCopy_1Byte`

**Array.Copy (5 tests) :**
- `ArrayCopy_IntArray`, `ArrayCopy_ByteArray`, `ArrayCopy_LargeArray`
- `ArrayCopy_ZeroLength`, `ArrayCopy_Overlap`

**Ramasse-miettes (22 tests) :**
- `GC_IsEnabled`, `GC_GetStats`, `GC_CollectBasic`, `GC_StatsIncrement`
- `GC_ExactCollectionCount`, `GC_ObjectSurvival`, `GC_StringSurvival`
- `GC_ArraySurvival`, `GC_ListSurvival`, `GC_UnreachableExactCount`
- `GC_ObjectGraphSurvival`, `GC_MixedTypeSurvival`, `GC_AllocAfterCollect`
- `GC_WeakReference`, `GC_LargeAllocCollect`, `GC_StructArraySurvival`
- `GC_DictSurvival`, `GC_PageAccounting`, `GC_DependentHandle`
- `GC_DependentHandleCleanup`, `GC_HandleStoreIntegrity`, `GC_PinnedHeapReuse`

<a id="drivers-tests"></a>

#### Tests Drivers

La suite se compose de deux projets. `tests/Kernels/Cosmos.Kernel.Tests.Drivers` est le noyau : le harnais (`Kernel.cs` et ses deux consommateurs, `TestKeyboardConsumer` et `TestPointerConsumer`), doté d'une autorisation `InternalsVisibleTo` accordée par `Cosmos.Kernel.HAL`, qu'il emploie pour ses consommateurs et, dans les groupes matériels, pour atteindre les états des pilotes livrés via `DriverEngine.Nodes` : `TestKeyboardConsumer` dérive du `KeyboardConsumer` interne et `TestPointerConsumer` du `PointerConsumer` interne, et tous deux sont installés via le `DeviceRegistry.SetConsumer` interne dans `BeforeRun`, où ils remplacent pour l'exécution les consommateurs clavier et pointeur propres à l'anneau supporté, ce qui illustre la règle d'un seul consommateur par type. `tests/Kernels/Cosmos.Kernel.Tests.Drivers.Library` contient toutes les classes `[Driver]` que la suite met en œuvre ainsi que les types d'état et d'identité dont elles ont besoin : un assembly de pilotes (`<CosmosDriverAssembly>true</CosmosDriverAssembly>`, listé dans `CosmosDriverAssemblyNames`) qui ne reçoit d'autorisation d'aucun projet et n'est écrit que sur le point d'extension public, si bien que le fait qu'il compile prouve qu'un tiers peut écrire chacun de ces pilotes. Le noyau référence la bibliothèque et s'exécute sur huit cellules sur x64 et neuf sur arm64 : `bare`, `usb-kbd` (un contrôleur qemu-xhci avec un usb-kbd branché au démarrage), `usb-kbd-ehci` (le même clavier sur un usb-ehci), `usb-kbd-ohci` (sur un pci-ohci), `usb-kbd-ehci-ohci` (un EHCI ICH9 avec un compagnon pci-ohci sur ses ports et un clavier `usb_version=1`, que l'EHCI confie au compagnon), `usb-kbd-ohci-hub` (un pci-ohci avec un usb-hub sur le port racine 1 et le clavier derrière lui), `usb-mouse` (le xHCI avec un usb-mouse), `virtio-blk-pci` (un disque virtio-blk derrière un port racine PCI Express) et, sur arm64, `virtio-blk-mmio` (le même disque dans la fenêtre de la machine virt) ; il fait fonctionner les pilotes de la bibliothèque via le bus synthétique, sans aucun matériel derrière aucun nœud, et les pilotes livrés sur ce que porte chaque cellule (les groupes Matériel, Hôte USB, Clavier USB, Souris USB, PS/2, virtio-blk et Port racine PCI Express ci-dessous), et attend le kit via `SyntheticBus.WaitForQueuedJobs`. Il est compilé avec `CosmosEnableFat` désactivé et avec un élément `CosmosDriverExclude` et un élément `CosmosDriverInclude` qui nomment des types de la bibliothèque, si bien que la politique du manifeste est elle aussi testée. Chaque assertion lit `DriverDiagnostics` ou les propres pilotes et consommateur de la suite, jamais le journal série.

Les quatre cellules EHCI et OHCI illustrent le champ `usbController` du catalogue de profils : `xhci` (la valeur par défaut, `qemu-xhci`), `ehci` (`usb-ehci`), `ohci` (`pci-ohci` avec six ports racines) ou `ehci-ohci` (`ich9-usb-ehci1` avec un compagnon `pci-ohci` sur ses six ports), avec `usbDeviceVersion` fixé à 1 sur la cellule à compagnon pour que le clavier de QEMU ne présente que le jeu de descripteurs pleine vitesse, et `usbHub`, qui place un usb-hub sur le port racine 1 avec le clavier derrière lui, ce que le lanceur et le catalogue n'acceptent que sur les deux modèles OHCI, le hub de QEMU étant pleine vitesse. Sur les trois modèles autres que le xHCI, le lanceur attribue un port racine fixe à chaque périphérique (le 1 au clavier, le 2 à la souris, les clés à partir du 3), car QEMU insérerait sinon son propre hub sur le dernier port libre, et les quatre profils indiquent `nic: none`, car l'e1000e par défaut de q35 partage la ligne héritée des contrôleurs et son pilote la réclamerait en premier, les laissant en scrutation. `cosmos run --usb-controller <xhci|ehci|ohci|ehci-ohci>` offre le même choix pour un noyau lancé à la main, pris en compte uniquement lorsqu'un périphérique USB est attaché.

L'ordre du manifeste pour les pilotes de la bibliothèque suit la règle des assemblys référencés : le générateur les trie par nom d'assembly puis par nom de type complet, tous deux en ordre ordinal, après les propres pilotes du noyau. `Manifest_Order_ReferencedByTypeName` vérifie que `TieFirstDriver` précède `TieSecondDriver` en vertu de cette seule règle (`F` est trié avant `S`) ; l'endroit où les deux sont déclarés n'y joue aucun rôle. `Interrupt_WorkItemDeferredToWorker` prouve le report sans demander au moteur où l'élément s'est exécuté : le gestionnaire d'interruption relève le nombre d'exécutions de l'élément de travail au moment où il retourne, interruptions encore désactivées, de sorte qu'un nombre inchangé signifie que l'élément ne s'est pas exécuté dans `RaiseInterrupt`, et l'exécution constatée après `WaitForQueuedJobs` est celle du worker.

**Manifeste (6 tests) :**
- `Manifest_HighPriorityDriver_Present`, `Manifest_FatFeatureDriver_Absent`, `Manifest_ExcludedDriver_Absent`
- `Manifest_OptInDriver_Present`, `Manifest_OptOutDriver_Absent`, `Manifest_Order_ReferencedByTypeName`

**Moteur (2 tests) :**
- `Engine_Started_WithWorker`, `BootPath_NodeFromConstructor_Bound`

**Arbitrage (5 tests) :**
- `Arbitration_ByPriority`, `Arbitration_BySpecificity`, `Arbitration_TieByManifestOrder`
- `Decline_UnwindsResources`, `ThrowingProbe_RecordedAsFailed`

**Périphérique clavier (7 tests) :**
- `Publish_ReachesConsumer`, `WindowAndDma_Contents`
- `Interrupt_HandlerReadsWindow_ReportsKey`, `Interrupt_WorkItemDeferredToWorker`, `Interrupt_MaskUnmask`
- `Periodic_FiresAtLeastThreeTimes`, `BlockingHandler_FaultRecorded`

**Écran (1 test) :**
- `Publish_Display_ReachesDisplayManager` : le `DisplayDriver` de la bibliothèque publie un `DisplayState`, un écran dans un unique mode fixe 64x32 sans framebuffer ; `DisplayManager.Count` augmente de un, l'écran est trouvé par son chemin de nœud dans `DriverDiagnostics` avec `IsConsumed`, rapporte le mode du pilote, est l'écran principal (un écran de pilote l'emporte sur celui du firmware) et fournit l'état comme facette mais aucun `IDisplayModes` ; après la rétractation, il a quitté le gestionnaire et la liste des publications, n'a plus de facette, et l'écran principal est de nouveau celui du firmware, ou aucun

**Périphérique bloc (1 test) :**
- `Publish_Block_ReachesStorageManager` : le `BlockDriver` de la bibliothèque publie un `BlockState`, un disque vierge de 64 blocs nommé `synthetic-block` sur un tableau d'octets ; `StorageManager.DeviceCount` augmente de un, `Devices` contient l'état, `GetPartitions` n'y trouve rien, il est le disque principal lorsque rien d'autre n'a été enregistré, et `DriverDiagnostics` le liste sous son nœud comme un périphérique bloc consommé, non retiré, nommé par le pilote ; après la rétractation, le nombre, la liste et le disque principal sont redevenus ce qu'ils étaient, l'entrée publiée a disparu, et le pilote a enregistré un sondage et un détachement. L'UART transporte `[StorageManager] synthetic-block registered by BlockDriver (primary)` et `[StorageManager] synthetic-block unregistered (no primary)`, lus dans le journal, sans assertion

**Rétractation (3 tests) :**
- `Retract_DetachOrderAndAccounting`, `Retract_DriverThreadExited`, `Retract_SinkReportDiscarded`

**Enfants (3 tests) :**
- `Children_PublishedFromProbe`, `UnmatchedNode_UnboundWithNoOffers`, `Children_RetractedWithParent`

**Diagnostics (1 test) :**
- `DriverDiagnostics_OutOfRange_ReturnsFalse`

**Matériel (5 tests) :**
- `Hardware_PciHost_Bound`, `Hardware_E1000E_NodeBound`, `Hardware_E1000E_DeviceConsumed`
- `Hardware_E1000E_LinkUp`, `Hardware_E1000E_Transmit`

Le groupe Matériel fait fonctionner les pilotes livrés sur du matériel réel (émulé) sans profil dédié : le q35 de QEMU ajoute un e1000e par défaut chaque fois qu'une cellule ne passe aucun `-netdev`, si bien que la cellule x64 bare de la suite porte déjà le contrôleur, tandis que la carte réseau par défaut de virt est une fonction virtio-net-pci que lie le `VirtioNetDriver` du kit ; les quatre tests E1000E sont donc ignorés sur arm64 avec `no e1000e on this machine`, et sur les quatre cellules EHCI et OHCI, dont les profils indiquent `nic: none` pour laisser libre la ligne héritée des contrôleurs, ils sont ignorés sur x64 aussi. Leur condition d'exécution est un nœud `DriverDiagnostics` dont le `BusName` est `pci` et dont la `Description` commence par `8086:10d3`. `Hardware_PciHost_Bound` est inconditionnel : un nœud `platform` dont la description contient `pci-host-` est `Bound` par `PciHostDriver`. `Hardware_E1000E_NodeBound` constate que la fonction est `Bound` par `E1000EDriver` avec un périphérique publié et au moins sept ressources détenues (la fenêtre de registres, les quatre tampons DMA, l'élément de travail de vidage et son enregistrement périodique, plus le handle d'interruption lorsque la ligne est connectée) ; `Hardware_E1000E_DeviceConsumed` trouve un périphérique publié de type `Network` à ce chemin de nœud, avec `IsConsumed` à vrai et un `NetworkManager.MacAddress` non nul ; `Hardware_E1000E_LinkUp` scrute `NetworkManager.LinkUp` pendant 2 s au plus ; et `Hardware_E1000E_Transmit` emploie l'autorisation HAL du noyau pour atteindre l'`E1000EState` du nœud via `DriverEngine.Nodes`, envoie une trame de diffusion de 60 octets via `NetworkManager.Send` et vérifie que `FramesTransmitted` a augmenté.

**Hôte USB (3 tests) :**
- `Usb_XhciHost_Bound`, `Usb_EhciHost_Bound`, `Usb_OhciHost_Bound`

Un test par pilote d'hôte livré, chacun s'exécutant sur les cellules dont le bus porte sa fonction et ignoré ailleurs avec `no xHCI controller on this cell`, `no EHCI controller on this cell` ou `no OHCI controller on this cell` : le test xHCI sur usb-kbd et usb-mouse, le test EHCI sur usb-kbd-ehci et usb-kbd-ehci-ohci, le test OHCI sur usb-kbd-ohci, usb-kbd-ohci-hub et usb-kbd-ehci-ohci, la seule cellule à deux contrôleurs. La condition d'exécution est un nœud `DriverDiagnostics` dont le `BusName` est `pci` et dont la `Description` contient `class 0c.03.30`, `class 0c.03.20` ou `class 0c.03.10`, soit la fonction xHCI, EHCI ou OHCI, trouvé dans `BeforeRun` quel que soit son état, de sorte qu'un sondage qui a échoué donne un test échoué plutôt qu'un test ignoré. Chaque test constate que la fonction est `Bound` par son pilote avec une ressource détenue et, via l'autorisation HAL, que son état (`XhciState`, `EhciState` ou `OhciState`) fait tourner le thread de branchement à chaud et reçoit ses événements par une interruption ou par scrutation, jamais les deux ni aucun des deux ; l'EHCI et l'OHCI sont des fonctions INTx uniquement, si bien que sur les cellules x64 conçues pour eux, où le profil a laissé la ligne libre, l'interruption doit être le chemin utilisé, tandis que sur arm64, qui ne route aucune ligne héritée, ils scrutent. Le numéro de bus suit l'ordre de liaison : le xHCI et un EHCI ou OHCI seul sont le bus 1 avec au moins un périphérique, le clavier ou la souris branché au démarrage, dont le nœud se trouve sous celui du contrôleur. Sur usb-kbd-ehci-ohci, l'EHCI, lié en premier, est le bus 1, sans enfant ni périphérique et avec `CompanionPortsReleased` au moins égal à 1, le clavier pleine vitesse étant passé au compagnon par le bit de propriété du port, et l'OHCI, lié en second, est le bus 2 avec le clavier ; sur usb-kbd-ohci-hub, le bus de l'OHCI contient au moins deux périphériques, le hub et le clavier derrière lui, dont le nœud se trouve sous celui du hub, lié par `UsbHubDriver`, lui-même sous celui du contrôleur.

**Clavier USB (6 tests) :**
- `Usb_Keyboard_BoundAfterDecline`, `Usb_Keyboard_SetLedsRoundTrip`, `Usb_Keyboard_KeyInjected`
- `Usb_Keyboard_Unplug_RetractsNode`, `Usb_Keyboard_Replug_PublishesAgain`, `Usb_Keyboard_Replug_SetLeds`

Ils s'exécutent sur les cinq cellules usb-kbd, quel que soit l'hôte auquel le clavier est rattaché, et sont ignorés avec `no USB host controller on this cell` sur les cellules sans contrôleur et avec `no USB keyboard on this cell` sur usb-mouse. La condition d'exécution est un nœud `usb` dont la `Description` se termine par `class 03.01.01`, l'interface HID de clavier en mode boot, surmonté d'un nœud de fonction hôte, trouvé dans `BeforeRun`, qui voit chaque périphérique présent au démarrage puisque les sondages des hôtes parcourent les ports racines à l'étape des pilotes (sur la cellule à hub, le clavier se trouve une offre plus bas, publié par le sondage du hub, et il est attendu) : `usb:1-5:0` sur le xHCI (le xHCI de QEMU numérote ses ports USB 3 de 1 à 4 et ses ports USB 2 de 5 à 8), `usb:1-1:0` sur un EHCI ou un OHCI seul, qui numérotent leurs ports à partir de 1, `usb:2-1:0` sur la cellule à compagnon, où le clavier appartient à l'OHCI, et `usb:1-1.1:0` derrière le hub. L'état de l'hôte est lu via le `UsbHostView` de la suite, les cinq mêmes membres (`HotPlugRunning`, `HasInterrupt`, `IsPolling`, `Bus`, `PipesClosed`) pris sur celui des trois types d'état qui détient le nœud. `Usb_Keyboard_BoundAfterDecline` est la preuve du refus après ouverture : l'`UsbDeclineDriver` de la bibliothèque correspond au même triplet d'interface avec la priorité 1, ouvre le pipe d'interruption du clavier dans son sondage et décline avec `declined after opening a pipe`, si bien que la première offre du nœud est déclinée avec `ReleasedResourceCount` à 1, le pipe que le kit a fermé, et que la seconde va à l'`UsbKeyboardDriver` livré, lié avec un périphérique publié, le clavier `usb-keyboard` consommé par le gestionnaire de l'anneau supporté. `Usb_Keyboard_SetLedsRoundTrip` appelle `SetLeds` sur l'`UsbKeyboardState` et relit `LastLedReport` à 0x03 et `LastLedStatus` à `Success`. `Usb_Keyboard_KeyInjected` demande au moteur `key-press a` et attend, en scrutant toutes les 50 ms pendant 8 s au plus, deux rapports du clavier USB dans le consommateur de la suite, le code d'appui 0x1E puis son relâchement, avec les `KeyEvents` du pilote augmentés de deux : c'est le seul test qui exécute une complétion de pipe d'interruption sur chaque hôte, ce que l'ordonnancement périodique de l'EHCI et la file des transferts terminés de l'OHCI n'obtiennent d'aucune autre cellule. `Usb_Keyboard_Unplug_RetractsNode` demande au moteur `usb-kbd-unplug` et attend, en scrutant toutes les 50 ms pendant 8 s au plus, que le nœud quitte l'arbre et que le bus libère son périphérique : le nœud est `Retracted`, sa liaison `IsDetaching`, le périphérique publié a disparu, le nombre de nœuds, le nombre de périphériques et le `ChildCount` du parent (le nœud de l'hôte, ou celui du hub sur la cellule à hub) ont baissé de un, le `WithdrawnCount` du consommateur de la suite a augmenté de un et le `PipesClosed` de l'hôte vaut au moins un. `Usb_Keyboard_Replug_PublishesAgain` demande `usb-kbd-plug` et attend un nouveau nœud de clavier lié, de nouveau décliné par `UsbDeclineDriver` et cette fois publié au consommateur de la suite : sur le xHCI, QEMU choisit le port libre suivant, si bien que le nœud est `usb:1-6:0`, tandis que sur l'EHCI et l'OHCI le moteur fixe le port que le clavier a quitté, si bien que le chemin est celui qu'il avait, et l'un ou l'autre est journalisé, `[DriversTests] replugged keyboard at usb:1-6:0`, sans assertion ; `Usb_Keyboard_Replug_SetLeds` écrit un rapport de LED via le nouvel état. Les lignes `[Monitor] usb-kbd-unplug: done` et `[Monitor] usb-kbd-plug: done` du moteur et les lignes `withdrew keyboard`, `retracted` et `usb 1-5: disconnected` du kit (`usb 1-1: disconnected` sur un EHCI ou un OHCI seul, `usb 2-1: disconnected` sur la cellule à compagnon et `usb 1-1.1: disconnected` derrière le hub) sont lues sur l'UART, sans assertion ; les décomptes par cellule figurent sous le groupe Port racine PCI Express ci-dessous.

**Souris USB (4 tests) :**
- `Usb_Mouse_Bound`, `Usb_Mouse_MovementInjected`, `Usb_Mouse_ButtonInjected`, `Usb_Mouse_WheelInjected`

Ils s'exécutent sur la cellule usb-mouse et sont ignorés avec `no USB host controller on this cell` sur les cellules sans contrôleur et avec `no USB mouse on this cell` sur les cellules usb-kbd. La condition d'exécution est un nœud `usb` dont la `Description` se termine par `class 03.01.02`, l'interface HID de souris en mode boot, `usb:1-5:0`, trouvé dans `BeforeRun` comme celui du clavier. `Usb_Mouse_Bound` constate qu'il est `Bound` par l'`UsbMouseDriver` livré dès la première offre, sous le nœud du contrôleur, sans ressource ni interruption et avec un périphérique publié, le pointeur `usb-mouse` consommé par le gestionnaire de souris de l'anneau supporté. Les trois autres demandent au moteur les événements de pointeur du groupe PS/2, que QEMU confie à son usb-mouse plutôt qu'à la souris PS/2 de q35 dès que le pipe du pilote l'a scrutée : `Usb_Mouse_MovementInjected` demande `mouse-move 10 0` et attend, en scrutant toutes les 50 ms pendant 8 s au plus, que le consommateur de pointeur de la suite totalise 10 par 0 depuis `usb-mouse`, sans bouton ni molette, et lit dans l'`UsbMouseState`, via l'autorisation HAL, un rapport transmis, aucun rapport tronqué et un dernier rapport de 4 octets, la partie boot plus l'octet de molette de QEMU ; `Usb_Mouse_ButtonInjected` demande `mouse-button left down` puis `up` et lit le bouton gauche depuis `usb-mouse`, puis aucun ; `Usb_Mouse_WheelInjected` demande `mouse-button wheel-down down` puis `up` et lit un total de molette de +1, le défilement vers le bas de l'anneau supporté, ce qui fixe la négation de la molette HID par le pilote.

**PS/2 (7 tests) :**
- `Ps2_Controller_Bound`, `Ps2_Keyboard_Bound`, `Ps2_Keyboard_KeyInjected`, `Ps2_Keyboard_SetLedsRoundTrip`
- `Ps2_Mouse_Bound`, `Ps2_Mouse_MovementInjected`, `Ps2_Mouse_ButtonInjected`

Ils s'exécutent sur chaque cellule x64 et sont ignorés sur arm64 avec `no 8042 on this cell` ; `Ps2_Keyboard_KeyInjected` est aussi ignoré sur chaque cellule usb-kbd avec `a usb-kbd on this cell takes the host's keys`, puisque QEMU confie `send-key` à un clavier USB lorsqu'il y en a un, et `Ps2_Mouse_MovementInjected` et `Ps2_Mouse_ButtonInjected` sont ignorés sur usb-mouse avec `a usb-mouse on this cell takes the host's pointer events`, puisque QEMU confie les événements de pointeur à une souris USB dès qu'elle est scrutée. La condition d'exécution est un nœud `DriverDiagnostics` dont le `BusName` est `platform` et dont la `Description` contient `pnp0303`. `Ps2_Controller_Bound` constate que `platform:i8042@60` est `Bound` par `I8042Driver` avec deux ressources, deux interruptions, deux enfants et quatre ressources détenues et, via l'autorisation HAL, que son `I8042State` est à double canal et piloté par interruptions. `Ps2_Keyboard_Bound` et `Ps2_Mouse_Bound` lisent les nœuds `ps2:kbd` et `ps2:aux` placés sous lui, liés par `Ps2KeyboardDriver` et `Ps2MouseDriver`, avec `ps2-keyboard` et `ps2-mouse` publiés et consommés, un clavier MF2 et une souris à molette. `Ps2_Keyboard_KeyInjected` demande au moteur `key-press a` et attend, en scrutant toutes les 50 ms pendant 8 s au plus, deux rapports dans le consommateur de la suite : le code d'appui 0x1E puis son relâchement, venant du clavier PS/2. `Ps2_Keyboard_SetLedsRoundTrip` appelle `SetLeds` sur le `Ps2KeyboardState` et relit l'octet 0x06 envoyé avec la commande 0xED, acquitté. `Ps2_Mouse_MovementInjected` demande `mouse-move 10 0` et lit un rapport relatif de 10 par 0 venant de `ps2-mouse` dans le consommateur de pointeur de la suite ; `Ps2_Mouse_ButtonInjected` demande `mouse-button left down` puis `up` et lit le bouton gauche dans les boutons, puis aucun. Les lignes `[Monitor] key-press a: done`, `[Monitor] mouse-move 10 0: done`, `[Monitor] mouse-button left down: done` et `[Monitor] mouse-button left up: done` du moteur sont lues dans sa sortie, sans assertion.

**virtio-blk (6 tests) :**
- `VirtioBlk_Bound`, `VirtioBlk_TransportMatchesCell`, `VirtioBlk_CapacityMatchesImage`
- `VirtioBlk_ReadWriteRoundTrip`, `VirtioBlk_FlushCompletes`, `VirtioBlk_InterruptOrPolled`

Ils s'exécutent sur les cellules virtio-blk-pci et virtio-blk-mmio. Le groupe est ignoré sur bare, sur les cellules usb-kbd et sur usb-mouse avec `no virtio-blk device on this cell` ; le pilote testé est le `VirtioBlkDriver` livré, que la bibliothèque de cette suite a d'abord hébergé. La condition d'exécution est un nœud `DriverDiagnostics` dont le `BusName` est `virtio` et dont la `Description` commence par `type 2 `, et chaque corps de test lit le `VirtioBlkState` du nœud via l'autorisation HAL. `VirtioBlk_Bound` constate que le nœud est `Bound` par `VirtioBlkDriver` avec un périphérique publié, consommé, non retiré et nommé d'après l'état, à savoir `vblk0`, d'indice 0 dans `StorageManager.Devices`. `VirtioBlk_TransportMatchesCell` constate que le chemin commence par `virtio:pci:` lorsqu'une fonction virtio-blk (`1af4:1001` ou `1af4:1042`) se trouve sur le bus PCI et par `virtio:mmio:` sinon, et que le parent est `Bound` par `VirtioPciTransportDriver` ou `VirtioMmioTransportDriver` avec un enfant. `VirtioBlk_CapacityMatchesImage` lit 524288 blocs de 512 octets, l'image de 256 Mio du moteur, accessible en écriture, avec au moins un bloc par requête. `VirtioBlk_ReadWriteRoundTrip` sauvegarde un bloc au LBA 200000 et les 130 blocs qui le suivent, écrit un motif sur chacun, relit les deux à l'identique, puis réécrit le contenu sauvegardé et le relit à l'identique ; 130 blocs font 66560 octets, plus que les 65536 qu'une requête transfère, si bien que le découpage s'exécute. `VirtioBlk_FlushCompletes` vide le cache et lit une requête terminée de plus lorsque FLUSH a été négocié, et aucune sinon. `VirtioBlk_InterruptOrPolled` constate que le disque reçoit ses complétions par l'interruption de la file ou par scrutation, jamais les deux ni aucun des deux, et que l'aller-retour a déclenché l'interruption lorsqu'il y en a une de connectée ; le mode n'est pas fixé, puisque ces cellules ne portent aucun modificateur GIC (le disque x64 et le disque MMIO arm64 fonctionnent par interruption, le disque PCI arm64 scrute sur le GICv2 par défaut). Sur arm64, le moteur attache l'ISO de démarrage via un contrôleur virtio-scsi plutôt que via `-cdrom`, que la machine virt transforme en une fonction virtio-blk à part entière, si bien que le seul périphérique virtio-blk d'une cellule est le disque que son profil attache.

**Port racine PCI Express (3 tests) :**
- `Pci_RootPort_Bound`, `Pci_HotPlug_Unplug_RetractsNode`, `Pci_HotPlug_Replug_PublishesAgain`

Ils s'exécutent sur la cellule virtio-blk-pci, dont le moteur place le disque derrière un `pcie-root-port` dédié, et sont ignorés ailleurs avec `no PCI Express root port on this cell`. La condition d'exécution est un nœud `pci` dont la `Description` commence par `1b36:000c`, le port racine générique de QEMU, et chaque corps de test lit son `PcieRootPortState` via l'autorisation HAL. `Pci_RootPort_Bound` constate que le port est `Bound` par `PcieRootPortDriver` avec un enfant ; que l'emplacement est occupé et alimenté, son thread en cours d'exécution, ses événements reçus par une interruption par message ou par scrutation, jamais les deux ni aucun des deux, derrière un bus secondaire ; que l'enfant est `Bound` par `VirtioPciTransportDriver` avec le nœud virtio sous lui ; que les ressources mémoire de l'enfant se trouvent dans la fenêtre mémoire ou préchargeable du port, décodée à partir des propres registres du port ; et que la ressource 0 du port est une fenêtre mémoire, le BAR0 qui contient sa table MSI-X, et que ses ressources 1 à 5 sont `None`. `Pci_HotPlug_Unplug_RetractsNode` demande au moteur `pci-unplug 0` et attend, en scrutant toutes les 50 ms pendant 8 s au plus, que le nœud de fonction quitte l'arbre et que le port ait traité le bouton d'attention : aucun enfant, un retrait de plus, l'emplacement vide et hors tension. Le nœud de fonction est alors `Retracted`, le nombre de nœuds a baissé de deux (la fonction et le nœud virtio sous elle), les périphériques publiés et les disques du gestionnaire de stockage ont baissé de un, et le port est toujours `Bound` avec un enfant de moins. `Pci_HotPlug_Replug_PublishesAgain` demande `pci-plug 0` et attend un nœud de fonction au même chemin, un nouvel objet, `Bound` par `VirtioPciTransportDriver` avec son nœud virtio `Bound` par `VirtioBlkDriver`, le disque enregistré, une arrivée de plus et l'emplacement sous tension ; le nombre de nœuds a alors remonté de deux, l'emplacement est occupé, les ressources mémoire de la nouvelle fonction se trouvent dans les fenêtres du port, où le kit les a placées puisque QEMU laisse non assignés les registres d'une fonction ajoutée à chaud, et `vblk0` est publié et consommé. Les lignes `slot 1: attention button, 1 functions retracted, slot powered off` et `slot 1: device arrived, 1 functions published` du port et `[Monitor] pci-unplug 0: done` et `[Monitor] pci-plug 0: done` du moteur sont lues sur l'UART et dans la sortie du moteur, sans assertion. Par cellule : sur x64, bare 41 réussis et 22 ignorés, usb-kbd 47 et 16, usb-kbd-ehci 43 et 20, usb-kbd-ohci 43 et 20, usb-kbd-ehci-ohci 44 et 19, usb-kbd-ohci-hub 43 et 20, usb-mouse 44 et 19, virtio-blk-pci 50 et 13 ; sur arm64, bare 30 et 33, usb-kbd 37 et 26, usb-kbd-ehci 37 et 26, usb-kbd-ohci 37 et 26, usb-kbd-ehci-ohci 38 et 25, usb-kbd-ohci-hub 37 et 26, usb-mouse 35 et 28, virtio-blk-pci 39 et 24, virtio-blk-mmio 36 et 27 ; 355 réussis, 149 ignorés et 0 échoué sur 504 sur x64, 326, 241 et 0 sur 567 sur arm64.

<a id="pci-tests"></a>

#### Tests Pci

`tests/Kernels/Cosmos.Kernel.Tests.Pci` examine la prise en charge de PCI par le kit de pilotes avec huit tests sur les deux architectures (le démarrage EDK2 d'arm64 fournit ACPI, si bien que MCFG est présent et que le nœud d'hôte ECAM y existe). Quatre tests d'hôte sont inconditionnels. `Host_PlatformNode_BoundByPciHostDriver` : un nœud `platform` dont la description contient `pci-host-` est `Bound` avec le `DriverName` `PciHostDriver`. `Host_ConfigMechanism_MatchesCompatible` : le nœud de plateforme qui porte un `PciHostAccess`, trouvé dans `DriverEngine.Nodes` via l'autorisation HAL du noyau, décode au moins un bus ; compatible avec `pci-host-legacy`, son mécanisme est `PciConfigSpace.Ports`, 256 octets par fonction, et son unique ressource est la plage de 8 ports à 0xCF8 ; compatible avec `pci-host-ecam-generic`, son mécanisme est un `PciEcamConfigSpace`, 4096 octets par fonction, et son unique ressource est une fenêtre mémoire de 1 Mio par bus que l'hôte mappe ; chaque nœud de fonction sur le segment et les bus de l'hôte est atteint via ce même mécanisme. `Host_PublishesPciNodes` : au moins un nœud `pci` a le chemin de l'hôte comme `ParentPath`, et chacun de ces nœuds a un `ResourceCount` de 6 et un `InterruptCount` égal à sa ligne héritée plus une source par message décrit : le `PciAccess` du nœud est lu et le nombre comparé à `1 + Math.Min(pci.MessageInterruptCount, PciHostAccess.MaxDescribedMessages)`, 32 au plus, lorsque `pci.IsMsiXCapable`, et à 1 sinon ; le `Describe()` de la première source commence par `line` et, sur une fonction compatible MSI-X, celui de la deuxième est égal à `message 0 of ` suivi de la taille de la table, comparé en ordinal. `Host_NodeCount_MatchesConfigSpaceScan` : un parcours brut de chaque bus que l'hôte décode, via `PciHostAccess.ReadConfig16` et `ReadConfig8` (fonction 0 des périphériques 0 à 31, et fonctions 1 à 7 lorsque le type d'en-tête porte le bit multifonction ; une fonction est présente lorsque son identifiant de fabricant ne vaut ni 0xFFFF ni 0x0000), trouve au moins une fonction, et exactement autant qu'il y a de nœuds dotés d'un `PciIdentity` sur le segment et les bus de l'hôte. Quatre tests d'espace de configuration s'exécutent sur chaque nœud de fonction (un nœud doté d'un `PciIdentity` et d'un `PciAccess`) et sont ignorés avec `no PCI function node in the tree, the PCI host was not described or published nothing` lorsqu'il n'y en a aucun. `ConfigSpace_VendorId_NotAllOnes` : `PciAccess.ReadConfig16(0x00)` ne vaut ni 0xFFFF ni 0x0000 et est égal au `VendorId` de l'identité. `ConfigSpace_DeviceId_NotAllOnes` : `ReadConfig16(0x02)` ne vaut pas 0xFFFF et est égal à `DeviceId`. `ConfigSpace_ClassCode_InRange` : le `ClassCode` de l'identité vaut au plus 0x13 et `ReadConfig8(0x0B)` lui est égal. `ConfigSpace_VendorRead_StableAcrossCalls` : deux lectures de l'identifiant de fabricant via le nœud concordent, et concordent avec la lecture brute du même registre par l'hôte. Les deux cellules par défaut portent une fonction compatible MSI-X (l'e1000e de q35, le virtio-net-pci de virt) et des fonctions sans cette capacité (le pont hôte), si bien que la formule par nœud couvre tous les cas de figure ; le projet supprime `COSMOS0003`, puisque `DeviceNode`, `DeviceResource`, `PlatformIdentity`, `PciHostAccess`, `PciIdentity` et `PciAccess` sont expérimentaux.

<a id="virtio-tests"></a>

#### Tests Virtio

`tests/Kernels/Cosmos.Kernel.Tests.Virtio` valide les pilotes virtio de `Cosmos.Kernel.Drivers` sur les deux transports avec les mêmes assertions. Ses deux profils attachent une carte réseau, un clavier, une souris et un disque bloc virtio à chaque cellule, et chacun s'exécute avec et sans ACPI : `virtio-pci` et `virtio-pci+acpi-off` sur x64 et arm64 (les cellules arm64 démarrent avec `gic-version=3`, puisque c'est l'ITS qui route les messages MSI-X de la fonction), `virtio-mmio` et `virtio-mmio+acpi-off` sur arm64 seulement, via la fenêtre virtio-mmio de la machine virt (q35 n'en a pas) ; les cellules arm64 acpi-off sont celles sur lesquelles le chargeur d'amorçage transmet un arbre de périphériques, si bien que l'hôte ECAM et les emplacements en proviennent. Le transport que présente une cellule est une propriété du profil, non de l'architecture, si bien que le même noyau sert chaque cellule arm64 et détecte le transport à l'exécution : `BeforeRun` capture le nœud virtio-net (un nœud `DriverDiagnostics` dont le `BusName` est `virtio` et dont la `Description` commence par `type 1 `), les nœuds virtio-input (`type 18 `), le nœud virtio-blk (`type 2 `) et le nœud de fonction PCI (`BusName` `pci`, `Description` commençant par `1af4:1041` ou `1af4:1000`), dont la présence fait de la cellule une cellule PCI. Le noyau conserve une autorisation HAL à deux fins, atteindre l'état de liaison d'un nœud via `DriverEngine.Nodes` comme le fait la suite Drivers et lire `PlatformHAL.Architecture` pour la seule cellule dont le mode d'interruption en dépend, et supprime `COSMOS0003`.

Les quatorze tests : `Net_DriverBound` (le nœud réseau est `Bound` par `VirtioNetDriver`), `Net_TransportMatchesCell` (son `Path` commence par `virtio:pci:` sur la cellule PCI et par `virtio:mmio:` sinon), `Net_DeviceReady` (un périphérique `Network` publié au chemin du nœud avec `IsConsumed` à vrai, `NetworkManager.DeviceCount` au moins égal à 1 et `NetworkManager.Ready`), `Net_LinkUp` (`NetworkManager.LinkUp` : le backend user de QEMU signale le lien actif immédiatement), `Net_MacAddressProgrammed` (`NetworkManager.MacAddress` non nul et pas entièrement à zéro), `Net_InterruptModeMatchesCell` (`VirtioNetState.HasInterrupt` du nœud à vrai et `IsPolling` à faux sur chaque cellule sauf la cellule PCI arm64 sous acpi-off, où l'ITS n'est pas découvert et le pilote scrute, si bien que les deux indicateurs y valent l'inverse ; ailleurs, MSI-X route sur PCI, la ligne GIC sur MMIO), `Input_KeyboardBound` et `Input_MouseBound` (un nœud `type 18` `Bound` par `VirtioInputDriver` dont le périphérique publié `Keyboard`, respectivement `Pointer`, est consommé), `Blk_DriverBound` (un nœud `type 2` `Bound` par `VirtioBlkDriver` dont le périphérique `Block` publié est `vblk0`, consommé et non retiré, le seul disque du gestionnaire de stockage) et `Blk_TransportMatchesCell` (son chemin nomme le transport de la cellule et, sur la cellule PCI, la fonction virtio-blk est `Bound` par `VirtioPciTransportDriver` avec un enfant), puis deux par transport. Sur la cellule PCI, `Pci_FunctionBoundByTransport` (le nœud de fonction est `Bound` par `VirtioPciTransportDriver` avec un `ChildCount` de 1) et `Pci_Version1Negotiated` (`VirtioNetState.Version1Negotiated` ; le virtio-mmio de QEMU est legacy par défaut, si bien que la cellule MMIO ne le vérifie pas), ignorés ailleurs avec `this cell presents virtio over MMIO`. Sur la cellule MMIO, `Mmio_SlotBoundByTransport` (un nœud `platform` dont la `Description` contient `virtio,mmio` est `Bound` par `VirtioMmioTransportDriver` avec un `ChildCount` de 1, l'assertion qui détecte une fenêtre MMIO perdue) et `Mmio_AnyLayoutNegotiated` (`VirtioNetState.AnyLayoutNegotiated` sur le périphérique legacy, dont QEMU active par défaut la propriété `any_layout`), ignorés ailleurs avec `this cell presents virtio over PCI`. Les cellules `virtio-net-pci` et `virtio-net-mmio` de la suite Network exercent le chemin de données du même pilote sans modification des tests. Par cellule, 12 réussis et 2 ignorés sur chacune des deux cellules de x64 (`24/28`) et sur chacune des quatre d'arm64 (`48/56`).

<a id="graphic-tests"></a>

#### Tests Graphic

`tests/Kernels/Cosmos.Kernel.Tests.Graphic` s'exécute sur trois profils, `bare`, `vmware-svga` (x64 uniquement, l'adaptateur se programme par E/S de ports) et `virtio-gpu` (un virtio-gpu-pci ajouté à côté de l'adaptateur par défaut de la machine, sur les deux architectures), et enregistre les mêmes 25 tests sur chaque cellule : un test propre à une cellule s'exécute là où se trouve son périphérique et est ignoré ailleurs, avec `no virtio-gpu device on this cell, needs the virtio-gpu profile` ou `VMware SVGA II adapter not present, needs the vmware-svga profile`. La cellule est déterminée dans `BeforeRun` à partir de l'arbre des périphériques, jamais à partir du nom du profil ni du pilote qui s'est lié, si bien qu'un pilote d'écran qui n'a pas réussi à se lier fait échouer la suite au lieu de la faire ignorer : un nœud `pci` dont la `Description` commence par `15ad:0405` désigne la cellule vmware-svga, un nœud `pci` commençant par `1af4:1050` ou un nœud `virtio` commençant par `type 16 ` désigne la cellule virtio-gpu, et l'absence des deux désigne bare. La suite atteint les états des pilotes de `Cosmos.Kernel.Drivers` via les facettes de `DisplayManager.Primary` et ne détient aucune autorisation ; le projet supprime `COSMOS0003` pour les facettes.

Les tests 2D (9), sur chaque cellule : `PCScreenFont_ChangeFont`, `Bitmap_Basic`, `Png_Decode`, `Ttf_Render`, `ColorClass_Basic`, `Canvas_Basic`, `VirtualCanvas_Basic`, `Canvas_CopyPixels_Overlap`, `Canvas_Compositing` (valeurs de mélange exactes de `DrawImage` et d'un `DrawPoint` translucide sur un pixel opaque, `DrawArray` qui copie sans transformation, l'alpha conservé sur un calque transparent et combiné sur un calque translucide, `DrawCanvas` et `DrawImage` avec et sans opacité, le `DrawCanvas` étiré à 200 % et à 150 %, découpé en haut à gauche et avec une opacité qui mélange chaque ligne répétée sur son propre arrière-plan, ainsi que les surcharges de `DrawImage` par région et mise à l'échelle) ; `Bitmap_Basic` charge aussi un BMP 32 bits sans alpha comme opaque. Les tests d'écran (4), sur chaque cellule : `Display_PrimaryPresent` (un écran principal existe, le canvas de la console a sa largeur et un `Name` commençant par son `DriverName`), `Display_PrimaryMatchesCell` (bare : l'écran principal est celui du firmware et c'est le seul ; virtio-gpu : l'écran de `VirtioGpuDriver` est principal à côté de celui du firmware ; vmware-svga : l'écran de `VmwareSvgaDriver` est le seul et `DriverDiagnostics` ne liste aucun écran sans chemin de nœud, l'écran du firmware ayant été retiré), `Display_ListedInDriverDiagnostics` (chaque écran du gestionnaire est un périphérique `Display` consommé dans `DriverDiagnostics`, avec un `NodePath` nul pour celui du firmware et pour lui seul) et `Display_ModeRequest_FollowsFacet` (`Canvas.GetFullScreen(new Mode(640, 480, ColorDepth.ColorDepth32))` rapporte 640x480 lorsque l'écran principal propose `IDisplayModes`, et la taille réelle sinon ; il s'exécute après les tests de facettes, seul `Canvas3D_Discovery` venant après lui, puisqu'il change le mode qu'ils mesurent). `VirtioGpu_DriverState` (1), sur les cellules virtio-gpu : l'écran principal fournit un `VirtioGpuState` qui n'est pas en faute, a une interruption ou scrute, compte au moins un scanout, correspond à la taille de l'écran principal et a envoyé les quatre commandes du sondage ; un `Display()` du canvas de la console augmente `FlushCount` de un et `CommandsSent` de deux. Les tests SVGA (9), sur vmware-svga : `Svga_AdapterFacet` (`ISvgaAdapter` avec des capacités, `FifoMin` inférieur à `FifoMax`, aucune 3D négociée sur QEMU, pas d'`ICanvas3DFactory`, le scanout activé puisque la console a programmé un mode), `Svga_DisplayModesFacet` (`IDisplayModes` listant 1024x768x32, le mode par défaut choisi par la console), `Svga_HardwareCursorFacet` (`IHardwareCursor` ; `TryDefine` renvoie false sur QEMU, qui n'a pas de curseur alpha ; `Set(1, 1, false)` ne lève pas d'exception), puis les tests de flux FIFO, dont chacun capture `NextCommand`, relit mot double par mot double les commandes que ses appels ont écrites, puis revient en arrière : `Canvas3D_SceneSetup_Fifo` ouvre le bloc en reprogrammant le propre mode de l'écran (pour que la FIFO revienne à son début), en désactivant le scanout via la facette (pour que l'hôte ne consomme rien) et en créant via `CreateCanvas3D` le canvas SVGA3D que partagent les tests suivants, et fixe la mise en place de la scène (contexte, cibles de couleur et de profondeur dimensionnées au canvas, les deux liaisons de cibles de rendu, viewport, plage de profondeur, sept états de rendu, l'étage sans texture) ; `Canvas3D_MeshUpload_Fifo` (pour chaque flux, un `SURFACE_DEFINE` de tampon et un `SURFACE_DMA` depuis la région du framebuffer, surfaces 3, 4 et 5 pour le cube), `Canvas3D_MeshValidation`, `Canvas3D_DrawCube_Fifo`, `Canvas3D_CameraCaching_Fifo` et `Canvas3D_DisposedTexture_Rejected` (`CreateTexture` définit et téléverse une surface A8R8G8B8, `Dispose` la détruit, et un maillage qui la référence est rejeté sans écriture) suivent, et le dernier rétablit le scanout. `Camera3D_Defaults` et `Canvas3D_Discovery` (2), sur chaque cellule : `Canvas.GetFullScreen()` n'est un `Canvas3D` sur aucune cellule de la CI. Par cellule sur x64 : bare 15 réussis et 10 ignorés, vmware-svga 24 réussis et 1 ignoré, virtio-gpu 16 réussis et 9 ignorés ; arm64 exécute bare et virtio-gpu. Le journal UART contient le démarrage de chaque cellule dans l'ordre, avec la ligne `[Display] primary` de chacune.

<a id="audio-tests"></a>

#### Tests Audio

`tests/Kernels/Cosmos.Kernel.Tests.Audio` s'exécute sur deux profils sur les deux architectures, `bare` et `hda`, qui ajoute l'`intel-hda` de QEMU avec le codec `hda-duplex` que le lanceur attache à côté et laisse la carte réseau par défaut désactivée, et enregistre les mêmes 22 tests sur chaque cellule. La carte réseau est désactivée parce que le kit attribue une ligne héritée à un seul gestionnaire d'interruption et en refuse un second, et sur q35 l'e1000e par défaut prend la ligne que le contrôleur partage avec lui. Le moteur fait jouer le contrôleur dans le backend `none` de QEMU (`-audiodev none`) : un runner de CI n'a pas de serveur audio, et le backend par défaut de QEMU échoue alors à ouvrir la voix de sortie du codec, tandis que `none` absorbe quand même les trames au débit du flux, si bien que le moteur de flux de l'invité avance en temps réel. La cellule est déterminée dans `BeforeRun` à partir de l'arbre des périphériques, un nœud `pci` dont la `Description` se termine par `class 04.03.00`, jamais à partir du nom du profil, si bien qu'un pilote qui n'a pas réussi à se lier fait échouer la suite au lieu de la faire ignorer ; les tests de périphérique sont ignorés sur bare avec `no HD Audio controller on this cell, needs the hda profile`. La suite détient une autorisation `InternalsVisibleTo` de `Cosmos.Kernel.System` pour compter les complétions que la sortie principale a signalées (`AudioManager.Completions`) et pour lire les formats que liste son contrat, une autre de `Cosmos.Kernel.Drivers` pour savoir si le `HdAudioState` du pilote a connecté la ligne de la fonction, et supprime `COSMOS0003` pour `AudioFormat` et `AudioBitDepth` ainsi que `CA1416` pour `Console.Beep(int, int)`, que .NET n'implémente que sous Windows.

Les tests du gestionnaire (3) : `Feature_Enabled` (`KernelFeatures.Audio` et `AudioManager.IsEnabled`), `Manager_CountMatchesCell` (une sortie à l'indice 0, la principale, sur hda ; aucune, et chaque indice vide, sur bare) et, sur bare seulement, `Manager_PlayWithoutOutputRefused` (`AudioManager.Play` et `TryStartPlayback` refusent tous deux et ne rendent aucune lecture). Les tests du pilote (2), sur hda : `Driver_BoundByHdAudioDriver` (le nœud du contrôleur est `Bound` par `HdAudioDriver`, qui a publié un périphérique et n'a enregistré aucune faute) et `Driver_OutputListedInDiagnostics` (`DriverDiagnostics` liste un périphérique `hda` sur le nœud du contrôleur, publié par `HdAudioDriver`, consommé et non retiré). Les tests de sortie (7), sur hda : `Output_IdentityMatchesNode`, `Output_DefaultFormat` (16 bits signé, stéréo, à 48 kHz, inactive, non réservée, et l'unique disposition que liste le contrat), `Output_TrySetFormat_AcceptsDerivableRates` (44,1, 96, 22,05, 8 et 192 kHz, puis retour à 48), `Output_TrySetFormat_RefusesOtherLayouts` (mono, 16 bits non signé, 8, 24 et 32 bits, une fréquence que le lien ne peut pas dériver et une fréquence nulle, le format restant inchangé), `Output_WritableWhileStopped`, `Output_EngineDrainsRing` (une sortie démarrée refuse un changement de format, accepte des trames entières jusqu'à ce qu'une écriture revienne incomplète, et libère de la place à mesure que son moteur lit le tampon circulaire) et `Output_CompletionsFollowLine` (avec la ligne connectée, un flux en cours déclenche l'interruption de tampon terminé et le pilote la signale via son récepteur ; sans ligne, le flux s'exécute et rien n'est signalé ; le journal UART indique lequel des deux : `line connected` sur x64, `no line` sur arm64, où le kit ne route aucune ligne héritée PCI). Les tests du lecteur (5), sur hda : `Player_PlaysStreamToEnd` et `Player_RefusesUnsupportedFormat` sur le thread appelant, `Playback_CompletesOnThread` (`AudioManager.TryStartPlayback` réserve la sortie immédiatement, si bien qu'une seconde lecture est refusée, et la lecture se termine `Completed` avec le flux lu jusqu'au bout et l'objet qu'elle possède libéré), `Playback_StopEndsEarly` (un flux de trois secondes arrêté une fois la sortie en marche se termine `Stopped` sans avoir été lu jusqu'au bout) et `Wave_FromMemoryPlays` (un fichier WAV construit en mémoire, avec un bloc `LIST` de longueur impaire avant `data`, se lit avec la disposition et la longueur qu'il déclare, et est joué). Les tests de `Console.Beep` (5) : `Beep_RejectsOutOfRange` sur les deux cellules (36 Hz, 32768 Hz, une durée nulle et une durée négative lèvent `ArgumentOutOfRangeException`, comme sous Windows), puis sur hda `Beep_PlaysForItsDuration` (`Console.Beep(440, 300)` retient l'appelant au moins la moitié de sa durée, mesurée avec le `Stopwatch`, rend la main dans la limite de 2 s et laisse la sortie libérée et arrêtée), `Beep_DefaultPlaysWindowsTone` (`Console.Beep()` joue ses 200 ms de la même façon au lieu d'écrire un caractère de sonnerie dans un terminal) et `Beep_WhileOutputBusyReturnsAtOnce` (avec une lecture en arrière-plan qui occupe la sortie, un bip d'une seconde rend la main en moins de 500 ms et la lecture continue), et sur bare seulement `Beep_WithoutOutputReturnsAtOnce` (les deux surcharges rendent la main en moins de 500 ms, sans lever d'exception). Chaque attente scrute avec `TimerManager.Wait`, qui laisse l'ordonnanceur exécuter le thread de lecture entre-temps, avec une limite de 2 s mesurée sur le `Stopwatch`, jamais un nombre de pauses, si bien que les attentes d'un test restent ensemble dans la fenêtre de blocage de 10 s du moteur. Par architecture, bare rapporte 5 réussis et 17 ignorés et hda 20 réussis et 2 ignorés, soit 25 réussis et 19 ignorés sur 44.

<a id="storage-tests"></a>

#### Tests Storage

`tests/Kernels/Cosmos.Kernel.Tests.Storage` s'exécute sur sept profils, `ahci`, `nvme`, `usb`, `usb-ehci`, `usb-ohci`, `virtio-blk-pci` et `virtio-blk-mmio` (arm64 uniquement), chacun attachant un disque (les trois profils usb attachent la même clé sur un qemu-xhci, un usb-ehci et un pci-ohci, en SuperSpeed, haute vitesse et pleine vitesse), combinés avec le modificateur `acpi-off` sur x64 (12 cellules) et avec `gicv2`, `gicv3` et `acpi-off` sur arm64 (42 cellules), et enregistre les mêmes 83 tests sur chaque cellule : 3 de gestionnaire, 1 de scan au démarrage, 2 de profil, 1 de diagnostics des pilotes, 12 de périphérique, 7 de partitions, 42 de cycle de vie des partitions (modification du MBR, la chaîne d'EBR, `PartitionManager`, le superfloppy), 2 sondages de bornes, 2 MMIO/PCI, 5 de branchement à chaud USB, 5 de branchement à chaud PCI et 1 de redémarrage. `BeforeRun` prend `StorageManager.GetDevice(0)` comme disque de la cellule, et chaque test qui en a besoin est ignoré là où aucun ne s'est lié, avec `no block device bound for this profile` (`no block device bound for partition-table tests` dans les deux groupes de partitions), ce qui constitue un échec de `Manager_ExactlyOneDevice` sur chaque cellule : sur x64, PCI s'énumère avec ou sans ACPI, et sur arm64 les cellules acpi-off obtiennent l'hôte ECAM de l'arbre de périphériques que le firmware n'expose que dans ce cas. Les disques des cellules ahci et nvme sont publiés par l'`AhciDriver` et le `NvmeDriver` du kit, ceux des cellules usb, usb-ehci et usb-ohci par l'`UsbMassStorageDriver` du kit, ceux des cellules virtio-blk par le `VirtioBlkDriver` du kit, et le consommateur du gestionnaire de stockage enregistre chacun d'eux pendant l'étape des pilotes. `Profile_DeviceKindMatches` compare le nom du disque au profil (`sata0`, `nvme0n1`, `usb0`, `vblk0`) ; `Manager_DeviceListedInDriverDiagnostics` le trouve dans `DriverDiagnostics` comme un périphérique bloc consommé, non retiré, avec le `DriverName` que la cellule attend, sur chaque profil ; `Profile_NvmeInterruptModeMatches` lit `HasInterrupt` sur le `Controller` du `NvmeNamespace` et le fixe là où la cellule le détermine (interruption sur les deux cellules nvme x64, puisque l'APIC local démarre à partir d'`IA32_APIC_BASE` et route MSI-X même lorsque acpi-off ne laisse aucune MADT, et sur arm64 gicv3 ; scrutation sur gicv2 et sur les cellules arm64 acpi-off, où l'ITS n'est pas découvert), et est ignoré ailleurs (`not an NVMe profile`, `interrupt mode not pinned by this cell`). Les groupes de périphérique et de partitions pilotent le disque via `IBlockDevice` et les tables de partitions de l'anneau supporté ; `Boot_PartitionScanMatchesBootState` valide le scan que le gestionnaire a effectué au démarrage, avant `BeforeRun` (au sein de la publication, sur chaque cellule) ; les deux sondages MMIO/PCI relogent le BAR du contrôleur NVMe au-dessus de 4 Gio sur x64 pendant qu'aucune commande n'est en cours, via le `PciAccess` du nœud `pci` du contrôleur (trouvé dans `DriverEngine.Nodes` via l'autorisation HAL par la classe 01, sous-classe 08) : `Mmio_HighBar_RemappedOnDemand` relit le registre VS à travers le BAR relogé après `EnsureMmioMapped`, et `Pci_DescribeBar64_ReadsLiveConfig` compare le BAR0 du nœud publié aux registres bruts, puis décrit à nouveau la fonction via l'hôte PCI et lit l'adresse relogée en entier dans la nouvelle description ; les tests de branchement à chaud retirent puis rebranchent la clé via les requêtes à l'hôte du moteur, en vérifiant que le disque quitte le gestionnaire et son nœud `usb:` l'arbre, qu'un fichier ouvert dessus échoue avec `IOException`, et que la clé revient en tant que `usb0` avec ses données, sur le port racine libre suivant sur le xHCI et sur le port qu'elle a quitté sur l'EHCI et l'OHCI, où le moteur la fixe, et, sur les cellules virtio-blk-pci, les tests `PciHotPlug_*` retirent le disque de son port racine et le rebranchent via `pci-unplug` et `pci-plug`, en vérifiant que le disque quitte le gestionnaire et ses nœuds de fonction et virtio l'arbre, qu'une lecture sur le disque retiré échoue avec `IOException`, et qu'il revient en tant que `vblk0` au même chemin de nœud avec ses données. Le premier test de chaque groupe de branchement à chaud vérifie que le thread qui suit le changement s'exécute, le thread de branchement à chaud de l'état du contrôleur hôte (`XhciState`, `EhciState` ou `OhciState`, selon celui que porte la cellule) ou le thread d'emplacement du `PcieRootPortState`, lu via l'autorisation HAL, et toutes les attentes qui suivent une requête à l'hôte partagent une même échéance de 8 s sur le `Stopwatch` : la requête est le dernier message du protocole avant elles, si bien qu'un changement qui ne se produit jamais fait échouer ses assertions dans la fenêtre de blocage de 10 s du moteur au lieu de coûter la cellule ; et `Boot_RebootAfterGptWrite`, le dernier, écrit une GPT et redémarre, si bien que chaque cellule dotée d'un disque démarre deux fois et que le scan du second démarrage trouve la partition. Sur x64, les douze cellules rapportent 886 réussis, 110 ignorés et 0 échoué sur 996 ; sur arm64, les quarante-deux exécutent chacune les mêmes 83 tests, 3486 au total, avec les mêmes résultats par cellule. Les suites Fat et File pilotent un périphérique bloc en mémoire et ne nomment jamais le gestionnaire, si bien que le déplacement des pilotes de stockage dans le kit les a laissées inchangées.

<a id="random-tests"></a>

#### Tests Random

`tests/Kernels/Cosmos.Kernel.Tests.Random` vérifie le CSPRNG du noyau dans `Cosmos.Kernel.Core/Security` (voir [Nombres aléatoires](../user/random.md)) via l'autorisation que Core accorde à la suite. Les tests à réponse connue (6) : `ChaCha20_Block_Rfc8439` (RFC 8439, section 2.3.2), `ChaCha20_Keystream_Rfc8439` (section 2.4.2, le texte clair « sunscreen » de 114 octets, donc un dernier bloc partiel), `Blake2s_Abc_Rfc7693` (annexe B), `Blake2s_Empty` (`69217a30...1ed0eef9`), `Blake2s_SelfTest_Rfc7693` (le hachage global de l'annexe E sur des hachages avec et sans clé de 0 à 1024 octets, pour des tailles d'empreinte de 16 à 32) et `FastKeyErasure_NextKeyNeverOutput` (une requête via `FastKeyErasure` : la clé suivante est formée des 32 premiers octets du flux de clé de la clé retirée, la sortie en est le reste et ne contient jamais la clé suivante, et la zone de travail est effacée). Les tests d'entropie (2) : `Hardware_Detection` (la détection par CPUID ou `ID_AA64ISAR0_EL1` s'exécute, et une instruction annoncée fournit des mots qui ne sont ni nuls, ni entièrement à un, ni répétés) et `KernelRandom_Seeding` (l'ensemencement se termine, et puise dans l'instruction lorsqu'une instruction est annoncée) ; les deux journalisent ce dont dispose la cellule, et le verdict de santé de la gigue est journalisé, sans assertion. Les tests de la BCL (8) : `Fill_TwoDrawsDiffer`, `Fill_ZeroLength` (les requêtes vides de la BCL, et `KernelRandom.Fill` appelé directement avec un nombre de 0, un nombre négatif et un tampon nul, que la BCL ne transmet jamais), `Fill_64KiB_Statistics` (équilibre des bits à 1 % près de la moitié, chaque valeur d'octet entre 144 et 368 fois, aucun mot de 8 octets en double), `Create_GetBytes_NonZero`, `GetInt32_Range`, `Guid_NewGuid_Differ` (version 4), `Fill_AcrossReseed` (au-delà des intervalles de réensemencement par requêtes et par octets, chacun devant faire augmenter `KernelRandom.ReseedCount`) et `Fill_ConcurrentThreads` (deux threads libérés ensemble par une barrière de départ effectuent des tirages pendant 300 ms, soit de nombreux quanta de l'ordonnanceur, pendant que le thread principal attend sur le minuteur : un numéro de séquence partagé pris avant chaque tirage doit montrer leurs tirages entrelacés, le générateur doit se réensemencer entre-temps, et deux de leurs tirages ne peuvent pas partager leurs 8 premiers octets). Les cellules x64 s'exécutent sous le CPU `host` de KVM ou le CPU `max` de TCG, qui disposent de RDSEED et RDRAND ; le `cortex-a72` d'arm64 n'a pas FEAT_RNG, si bien que cette cellule exerce le chemin reposant sur la gigue seule, et son journal contient l'avertissement signalant la gigue seule.

<a id="iso9660-tests"></a>

#### Tests Iso9660

`tests/Kernels/Cosmos.Kernel.Tests.Iso9660` vérifie le pilote ISO 9660 de `Cosmos.Kernel.System/FileSystem/Iso9660` sur les deux architectures, sans profil. Ses images ne sont pas commitées : le fichier projet lance xorriso, l'outil avec lequel chaque ISO Cosmos est construit (résolu par la cible `GetXorriso` de `Cosmos.Build.Common`), sur les arborescences de `Fixtures/` à la compilation, avec le propriétaire, les modes et tous les horodatages figés (2026-10-28 12:00:00 UTC) pour que les octets ne dépendent ni du checkout, ni de l'horloge, ni du fuseau horaire, et embarque les deux images dans le noyau, qui les lit via le `ResourceManager` de Core (l'autorisation est dans `Cosmos.Kernel.Core.csproj`). `Fixtures/rockridge/` est mappé avec Rock Ridge : un nom de 140 caractères dont l'entrée `NM` déborde dans une continuation `CE`, un répertoire au-delà de la profondeur 8 que xorriso relocalise dans `rr_moved` (`CL`, `RE`, `PL`), quarante entrées qui étalent un répertoire sur quatre blocs logiques, un fichier de 10 000 octets qui se termine par un bloc partiel, un motif de 200 Kio que la compilation génère, un fichier vide, des liens symboliques vers un fichier, vers un répertoire et vers rien, et un répertoire vide. `Fixtures/level1/` est mappé avec les seuls identifiants de niveau 1 et un descripteur Joliet que le pilote doit ignorer. L'image Rock Ridge est montée deux fois, depuis un périphérique à secteurs de 2048 octets (un CD) et depuis un à secteurs de 512 octets (une clé USB sur laquelle l'image a été écrite).

Les cellules : le montage (`Test_Mount_*`, `Test_StatFs`, `Test_FormatAndDestroy_Refused`), le listage (`Test_RootListing_*`, `Test_Listing_*`), la recherche (`Test_Lookup_*` : noms exacts avec Rock Ridge, sans la casse au niveau 1, le nom long, le répertoire relocalisé, `.` et `..`), la lecture (`Test_Read_*` : entière, par morceaux de 4 097 octets pour que la tête, le corps et la queue d'une lecture s'exécutent tous, après seek, ligne par ligne, avec le nombre de lectures du périphérique borné pour un fichier de 200 Kio), les attributs (`Test_Attributes_*` : lecture seule, les horodatages figés, des numéros d'inode distincts pour les entrées qui partagent un extent), les liens symboliques (`Test_Symlink_*`), les refus (`Test_ReadOnly_*` : chaque modification est une simple `IOException` via `System.IO` et `false` via `VfsManager`), et `Test_Unmount_And_Remount` et `Test_TwoMounts_SameDevice`.

<a id="running-kernel-tests"></a>

### Exécuter les tests du noyau

<a id="from-vs-code"></a>

#### Depuis VS Code

**Avec les tâches (recommandé) :**
1. Appuyez sur `Ctrl+Shift+P` → « Tasks: Run Task »
2. Sélectionnez l'une des tâches suivantes :
   - **Run Test: HelloWorld (x64)** : sortie console + XML
   - **Run Test: HelloWorld (x64, Console Only)** : sortie console uniquement
   - **Run Test: HelloWorld (ARM64)** : test ARM64 avec sortie XML
   - **Dev Test: HelloWorld (x64)** : mode développeur avec sortie détaillée

**Déboguer l'exécuteur de tests :**
1. Ouvrez le panneau Exécuter et déboguer (`Ctrl+Shift+D`)
2. Sélectionnez une configuration :
   - **Debug Test Runner (HelloWorld x64)**
   - **Debug Test Runner (HelloWorld ARM64)**
3. Appuyez sur `F5`

<a id="from-the-command-line"></a>

#### Depuis la ligne de commande

```bash
# Run test with XML output
dotnet run --project tests/Cosmos.TestRunner.Engine/Cosmos.TestRunner.Engine.csproj -- \
  tests/Kernels/Cosmos.Kernel.Tests.HelloWorld \
  x64 \
  60 \
  test-results.xml

# Run test with console output only
dotnet run --project tests/Cosmos.TestRunner.Engine/Cosmos.TestRunner.Engine.csproj -- \
  tests/Kernels/Cosmos.Kernel.Tests.HelloWorld \
  x64 \
  60
```

**Arguments :**
1. Chemin du projet de noyau (absolu ou relatif)
2. Architecture : `x64` ou `arm64`
3. Délai d'expiration en secondes
4. *(Facultatif)* Chemin de la sortie XML (format JUnit)
5. *(Facultatif)* Mode : `ci` ou `dev`

**Délais d'expiration recommandés :**

| Suite | x64 | ARM64 |
|-------|-----|-------|
| HelloWorld | 60 s | 90 s |
| Memory | 180 s | 300 s |
| Drivers | 60 s | 120 s |
| Pci | 60 s | 90 s |
| Graphic | 60 s | 90 s |
| Storage | 90 s | 180 s |

<a id="output-formats"></a>

### Formats de sortie

<a id="console-colored"></a>

#### Console (en couleurs)

```
================================================================================
Starting test suite: HelloWorld Basic Tests
Architecture: x64
Time: 2025-11-05 04:06:48
================================================================================
[1] Test_BasicArithmetic: PASSED (15ms)
[2] Test_BooleanLogic: PASSED (12ms)
[3] Test_IntegerComparison: PASSED (10ms)
================================================================================
Suite: HelloWorld Basic Tests
Total tests: 3 | Passed: 3 | Failed: 0 | Skipped: 0 | Duration: 0.04s
================================================================================
ALL TESTS PASSED
================================================================================
```

<a id="xml-junit-format"></a>

#### XML (format JUnit)

```xml
<?xml version="1.0" encoding="utf-16"?>
<testsuites name="HelloWorld Basic Tests" tests="3" failures="0" skipped="0" time="0.037">
  <testsuite name="HelloWorld Basic Tests" tests="3" failures="0" skipped="0" time="0.037">
    <properties>
      <property name="architecture" value="x64" />
    </properties>
    <testcase name="Test_BasicArithmetic" classname="HelloWorld Basic Tests" time="0.015" />
    <testcase name="Test_BooleanLogic" classname="HelloWorld Basic Tests" time="0.012" />
    <testcase name="Test_IntegerComparison" classname="HelloWorld Basic Tests" time="0.010" />
  </testsuite>
</testsuites>
```

<a id="exit-codes"></a>

### Codes de sortie

| Code | Signification |
|------|---------|
| 0 | Tous les tests ont réussi (les tests ignorés sont acceptés) |
| 1 | Des tests ont échoué ou une erreur d'exécution s'est produite |
| 137 | Délai dépassé (SIGKILL) |

---

<a id="uart-debug-protocol"></a>

## Protocole de débogage UART

Les noyaux de test renvoient leurs résultats au moteur hôte via un protocole binaire intégré au flux série (UART) de QEMU. Le protocole est défini dans `tests/Cosmos.TestRunner.Protocol/` et a été porté depuis le connecteur de débogage de CosmosOS.

<a id="framing"></a>

### Trames

Chaque message commence par une signature magique de 4 octets, suivie de l'octet de commande et d'une longueur de charge utile de 2 octets en little-endian :

```
[Magic: 4 bytes][Command: 1 byte][Length: 2 bytes LE][Payload: N bytes]
```

- **Magic** : `0x07 0x08 0x74 0x19` (c'est-à-dire `0x19740807` en little-endian, `SerialSignature` dans `Consts.cs`)
- **Command** : l'une des constantes `Ds2Vs` (voir le tableau ci-dessous)
- **Length** : nombre d'octets de charge utile qui suivent

Après le message `TestSuiteEnd` final, le noyau envoie aussi un marqueur de fin de 8 octets (`0xDE 0xAD 0xBE 0xEF 0xCA 0xFE 0xBA 0xBE`) pour que le moteur puisse tuer QEMU immédiatement, sans attendre l'expiration complète du délai.

<a id="commands-kernel--host-ds2vs"></a>

### Commandes (noyau → hôte, `Ds2Vs`)

Les commandes propres à l'exécuteur de tests occupent la plage **100-109**. Les commandes de débogage d'origine de CosmosOS (0-25) sont également définies, mais l'exécuteur de tests ne les utilise pas.

| Commande | Valeur | Format de la charge utile | Description |
|---------|-------|----------------|-------------|
| `TestSuiteStart` | 100 | `[ExpectedTests: 2 LE][SuiteName: UTF-8]` | Envoyée une fois, au début de la suite de tests |
| `TestStart` | 101 | `[TestNumber: 2 LE][TestName: UTF-8]` | Envoyée avant l'exécution de chaque test |
| `TestPass` | 102 | `[TestNumber: 2 LE][DurationMs: 4 LE]` | Envoyée lorsqu'un test réussit |
| `TestFail` | 103 | `[TestNumber: 2 LE][ErrorMessage: UTF-8]` | Envoyée lorsqu'une assertion échoue |
| `TestSkip` | 104 | `[TestNumber: 2 LE][SkipReason: UTF-8]` | Envoyée lorsqu'un test est explicitement ignoré |
| `TestSuiteEnd` | 105 | `[Total: 2 LE][Passed: 2 LE][Failed: 2 LE]` | Envoyée une fois, à la fin de la suite de tests |
| `ArchitectureInfo` | 106 | `[ArchId: 1][CpuCount: 1]` | Envoyée au démarrage du noyau (identifiants d'architecture : 1=x86, 2=x64, 3=ARM32, 4=ARM64) |
| `CoverageData` | 107 | `[HitCount: 2 LE][MethodId: 2 LE]...` | Envoyée après la fin de la suite par un noyau compilé avec la couverture de code |
| `TestDestructiveReached` | 108 | `[TestNumber: 2 LE]` | Envoyée par `TR.RunDestructive` juste avant une action qui ne revient jamais (redémarrage, arrêt) |
| `HostRequest` | 109 | `[Request: ASCII]` | Demande au moteur de modifier la machine sous l'invité en cours d'exécution (voir ci-dessous) |

<a id="message-flow"></a>

### Flux de messages

Une session typique se présente ainsi :

```
→ TestSuiteStart  (suiteName="HelloWorld Basic Tests", expectedTests=3)
→ TestStart       (testNumber=1, testName="Test_BasicArithmetic")
→ TestPass        (testNumber=1, durationMs=15)
→ TestStart       (testNumber=2, testName="Test_BooleanLogic")
→ TestPass        (testNumber=2, durationMs=12)
→ TestStart       (testNumber=3, testName="Test_IntegerComparison")
→ TestPass        (testNumber=3, durationMs=10)
→ TestSuiteEnd    (total=3, passed=3, failed=0)
→ [0xDE 0xAD 0xBE 0xEF 0xCA 0xFE 0xBA 0xBE]  ← termination marker
```

<a id="parser"></a>

### Analyseur

`tests/Cosmos.TestRunner.Engine/Protocol/UartMessageParser.cs` lit le journal UART brut capturé par QEMU (`uart-output.log`), y recherche octet par octet la signature magique, valide l'octet de commande et la longueur, puis transmet le message à la fonction d'analyse appropriée.

Détection de corruption : la charge utile de `TestSuiteEnd` est validée en vérifiant `total == passed + failed`. Si cet invariant n'est pas respecté (par exemple parce qu'une interruption de minuteur intercalée a corrompu des octets UART), le message de fin est ignoré et les résultats se rabattent sur les compteurs suivis individuellement.

<a id="host-requests"></a>

### Requêtes à l'hôte

Un test appelle `TR.RequestHost(request)` lorsqu'il a besoin que la machine soit modifiée sous lui, ou que l'hôte agisse, et le moteur exécute la requête dès que la trame apparaît sur l'UART, via le moniteur QMP de QEMU pour les requêtes qui portent sur la machine et via son propre serveur de test TCP pour `tcp-connect` :

| Requête | Effet |
|---------|--------|
| `usb-unplug [n]` | Retire la clé USB `n` (0 par défaut) du contrôleur hôte USB que nomme le profil |
| `usb-plug [n]` | La rebranche, sur la même image disque et le même bus, et sur le port racine qu'elle a quitté là où le contrôleur fixe les ports (les modèles EHCI et OHCI) |
| `pci-unplug [n]` | Supprime de QEMU le disque PCI branchable à chaud `n` (0 par défaut), ce qui actionne le bouton d'attention de l'emplacement et achève le retrait lorsque l'invité met l'emplacement hors tension |
| `pci-plug [n]` | Le rajoute derrière le même port racine, sur la même image disque ; l'invité met l'emplacement sous tension et place les registres de la fonction |
| `usb-kbd-unplug` | Retire le clavier USB du profil du contrôleur hôte USB que nomme le profil |
| `usb-kbd-plug` | Le rebranche, sur le même bus, sur le port qu'il a quitté là où le contrôleur fixe les ports (derrière le hub lorsque le profil en demande un) et avec le même `usb_version` |
| `key-press <qcode>` | Appuie sur une touche puis la relâche, désignée par son nom QKeyCode de QEMU (`a`, `ret`, `spc`, ...) ; QEMU la relâche après sa durée de maintien par défaut de 100 ms |
| `mouse-move <dx> <dy>` | Déplace le pointeur de `dx`, `dy` en unités du périphérique, sous la forme d'un seul `input-send-event` portant deux événements relatifs |
| `mouse-button <left or middle or right> <down or up>` | Appuie sur un bouton du pointeur ou le relâche |
| `tcp-connect` | Connecte le serveur de test TCP du moteur au serveur TCP du noyau sur le port 5558 et exécute l'échange d'écho `TEST_FROM_HOST` ; le `TCP_ServerAccept` de la suite Network l'envoie juste après `listener.Start()`, si bien que le SYN part pendant que l'écouteur est actif (le port 5558 est un `hostfwd`, que slirp accepte immédiatement côté hôte et livre à l'invité selon son propre calendrier de retransmission, à 6 puis 12 s d'intervalle : une connexion effectuée selon une horloge manquait un écouteur apparu tardivement, #3361) |

Rien ne répond. Le test attend lui-même le changement, et doit le constater dans les 10 s : passé ce délai sans message du protocole, le moteur considère que le noyau est figé. Les requêtes clavier ne prennent pas d'indice : un profil a au plus un clavier `usb-kbd` ; les requêtes PCI indexent les disques branchables à chaud du profil (`"hotplug": true` sur un disque `virtio-blk`). Chaque cellule lance QEMU avec un moniteur ; une requête que l'exécution ne peut pas mener à bien est signalée puis abandonnée (`[Monitor] usb-kbd-unplug failed: the profile attaches no USB keyboard`), et le test de l'invité échoue par expiration du délai. Les tests `UsbHotPlug_*` et `PciHotPlug_*` de la suite Storage et les tests `Usb_Keyboard_*`, `Usb_Mouse_*`, `Ps2_*` et `Pci_HotPlug_*` de la suite Drivers en sont des exemples.

<a id="host--kernel-commands-vs2ds"></a>

### Commandes hôte → noyau (`Vs2Ds`)

L'exécuteur de tests n'envoie actuellement aucune commande au noyau. La classe `Vs2Ds` (`Noop=0`, `Continue=4`, `Ping=17`) est héritée du connecteur de débogage de CosmosOS et réservée à un usage futur.

---

<a id="project-structure"></a>

## Structure du projet

```
tests/
├── Cosmos.TestRunner.Engine/        # Host-side test runner
│   ├── Engine.cs                    # Main orchestration
│   ├── Engine.Build.cs              # NativeAOT build pipeline
│   ├── Program.cs                   # CLI entry point
│   ├── TestConfiguration.cs         # Configuration
│   ├── TestResults.cs               # Result model
│   ├── Hosts/                       # QEMU host implementations
│   │   ├── IQemuHost.cs
│   │   ├── QemuMonitor.cs           # The QMP monitor: USB and PCI hot-plug, key and pointer injection
│   │   ├── QemuX64Host.cs
│   │   └── QemuARM64Host.cs
│   ├── OutputHandlers/              # Result output formats
│   │   ├── OutputHandlerBase.cs
│   │   ├── OutputHandlerConsole.cs  # Colored terminal output
│   │   ├── OutputHandlerXml.cs      # JUnit XML output
│   │   └── MultiplexingOutputHandler.cs
│   └── Protocol/
│       └── UartMessageParser.cs     # Binary message parser
├── Cosmos.TestRunner.Framework/     # In-kernel test framework
│   ├── TestRunner.cs                # Start / Run / Skip / Finish
│   └── Assert.cs                    # Assertion helpers
├── Cosmos.TestRunner.Protocol/      # Shared protocol definitions
│   ├── Consts.cs                    # Magic signature and constants
│   └── Messages.cs                  # Typed message classes
├── Cosmos.Kernel.Tests.System/      # Unit tests: Cosmos.Kernel.System (NUnit)
├── Cosmos.Tests.Build.Asm/          # Unit tests: Clang assembly build task
├── Cosmos.Tests.Build.Analyzer.Patcher/ # Unit tests: plug analyzer
├── Cosmos.Tests.Scanner/            # Unit tests: plug scanner
├── Cosmos.Tests.Patcher/            # Unit tests: IL patcher
├── Cosmos.Tests.SourceGenerators/   # Unit tests: entry point and driver manifest generator
├── Cosmos.Tests.NativeWrapper/      # Runtime assets (no tests)
├── Cosmos.Tests.NativeLibrary/      # Native code for tests (no tests)
└── Kernels/                         # Kernel test projects
    ├── Cosmos.Kernel.Tests.HelloWorld/
    │   ├── Kernel.cs
    │   └── Bootloader/limine.conf
    └── Cosmos.Kernel.Tests.Memory/
        ├── Kernel.cs
        └── Bootloader/limine.conf
```

`tests/Cosmos.Kernel.Tests.System/` contient les tests côté hôte de `Cosmos.Kernel.System`. Il prend une référence de projet sur la bibliothèque et une autorisation `InternalsVisibleTo` accordée par celle-ci, et ses dossiers reflètent ceux de la bibliothèque (`Network/`, `Network/Protocols/`, `Input/`, `FileSystem/Ext2/`).

---

<a id="writing-a-test-kernel"></a>

## Écrire un noyau de test

<a id="minimal-example"></a>

### Exemple minimal

Les noyaux de test héritent de `Cosmos.Kernel.System.Kernel` et exécutent tous les tests dans `BeforeRun()`. Utilisez l'alias `TR` pour `TestRunner` et `Assert` pour les assertions.

```csharp
using Cosmos.Kernel.Core.IO;
using Cosmos.TestRunner.Framework;
using Sys = Cosmos.Kernel.System;
using TR = Cosmos.TestRunner.Framework.TestRunner;

namespace Cosmos.Kernel.Tests.MyTests;

public class Kernel : Sys.Kernel
{
    protected override void BeforeRun()
    {
        // Initialize test suite (expectedTests must equal the total number of TR.Run + TR.Skip calls)
        TR.Start("My Test Suite", expectedTests: 3);

        TR.Run("Test_Addition", () =>
        {
            int result = 2 + 2;
            Assert.Equal(4, result);
        });

        TR.Run("Test_StringOps", () =>
        {
            string str = "Hello";
            Assert.Equal("Hello", str);
            Assert.NotNull(str);
        });

        // Mark unsupported operations as skipped rather than letting them crash
        // TR.Skip also counts toward expectedTests
        TR.Skip("Test_Unsupported", "Feature not implemented");

        TR.Finish();

        Serial.WriteString("[Tests Complete - System Halting]\n");
        Stop();
    }

    protected override void Run()
    {
        // Tests completed in BeforeRun, nothing to do here
    }

    protected override void AfterRun()
    {
        Cosmos.Kernel.Kernel.Halt();
    }
}
```

<a id="kernel-lifecycle"></a>

### Cycle de vie du noyau

La classe de base `Sys.Kernel` pilote un cycle de vie fixe :

1. `OnBoot()` : initialisation du système (appelée automatiquement, rarement redéfinie)
2. `BeforeRun()` : **exécutez tous les tests ici**, puis appelez `Stop()`
3. `Run()` : appelée en boucle jusqu'à l'appel de `Stop()` ; laissez-la vide pour les noyaux de test
4. `AfterRun()` : appelée une fois après la sortie de la boucle ; appelez `Cosmos.Kernel.Kernel.Halt()` ici

<a id="available-assertions"></a>

### Assertions disponibles

```csharp
// Equality: typed overloads (int, uint, long, byte, bool, string, byte[], int[])
Assert.Equal(expected, actual);
Assert.Equal<T>(expected, actual);   // Generic overload (requires IEquatable<T>)

// Null checks
Assert.Null(obj);
Assert.NotNull(obj);

// Boolean
Assert.True(condition);
Assert.True(condition, "message");
Assert.False(condition);

// Manual failure
Assert.Fail("Custom error message");
```

> `Assert` utilise un état d'échec statique (sans exceptions) pour rester compatible avec NativeAOT.
> Seul le premier échec de chaque test est enregistré ; les assertions suivantes du même bloc `TR.Run` sont tout de même évaluées.

<a id="test-status"></a>

### Statut des tests

| Statut | Quand |
|--------|------|
| **Réussi** | Le test s'est terminé sans échec d'assertion |
| **Échoué** | Une assertion a positionné l'état d'échec dans `TR.Run` |
| **Ignoré** | Le test a été explicitement marqué via `TR.Skip(name, reason)` |

<a id="adding-a-new-test-suite"></a>

### Ajouter une suite de tests

1. Créez un projet de noyau sous `tests/Kernels/Cosmos.Kernel.Tests.{Name}/`
2. Copiez le `.csproj` et `Bootloader/limine.conf` d'une suite existante ; mettez à jour le chemin de l'ELF dans `limine.conf`
3. Implémentez les tests avec `TestRunner.Framework`
4. Ajoutez un job de CI dans `.github/workflows/kernel-tests.yml` :
   - Copiez un job `*-tests` existant, renommez-le et mettez à jour le chemin du noyau
   - Ajoutez un job `{name}-results` correspondant, qui produit le commentaire de PR
   - Ajoutez le nouveau job aux dépendances de `test-summary`
5. Ajoutez des tâches VS Code dans `.vscode/tasks.json`

---

<a id="ci-integration"></a>

## Intégration CI

Le workflow de CI (`.github/workflows/kernel-tests.yml`) exécute les tests d'intégration du noyau sur x64 et sur ARM64.

**Jobs :**
- `helloworld-tests` : build matriciel pour x64/arm64
- `helloworld-results` : produit le commentaire de PR combiné dans un artefact `pr-comment-helloworld`
- `memory-tests` : build matriciel pour x64/arm64
- `memory-results` : produit le commentaire de PR combiné dans un artefact `pr-comment-memory`
- une paire de ce type par suite, jusqu'à `fat-tests`/`fat-results`, `file-tests`/`file-results` et `iso9660-tests`/`iso9660-results`
- `test-summary` : récapitulatif final de l'état

**Déclencheurs :**
- Push sur `main`
- Pull requests (n'importe quelle branche)
- Déclenchement manuel avec choix de l'architecture

**Commentaires de PR :** chaque suite de tests reçoit un commentaire avec des lignes distinctes pour x64 et arm64, qui indique le nombre de tests, la durée et des liens vers les artefacts. Les jobs `*-results` ne font que produire le commentaire : une exécution `pull_request` pour une pull request issue d'un fork dispose d'un jeton en lecture seule et ne peut pas publier. `.github/workflows/kernel-test-comment.yml` s'exécute sur `workflow_run` une fois que Kernel Tests ou Kernel Coverage se termine, dans le dépôt de base et avec un accès en écriture quelle que soit l'origine de la pull request, télécharge les artefacts `pr-comment-*` et publie ou met à jour les commentaires. Il ne fait confiance au numéro de PR d'un artefact que lorsque la tête de cette pull request est le commit de tête de l'exécution, et GitHub l'exécute depuis la branche par défaut, si bien qu'une modification de ce fichier ne prend effet qu'après la fusion.

**Artefacts (conservés 30 jours) :**
- `test-results-{suite}-{arch}.xml` : résultats JUnit XML
- `uart-log-{suite}-{arch}` : sortie UART complète
- `{Suite}-Test-ISO-{arch}` : ISO de noyau démarrable + ELF

<a id="example-ci-step"></a>

### Exemple d'étape CI

```yaml
- name: Run Cosmos Tests
  run: |
    dotnet run --project tests/Cosmos.TestRunner.Engine/Cosmos.TestRunner.Engine.csproj -- \
      tests/Kernels/Cosmos.Kernel.Tests.HelloWorld \
      x64 \
      120 \
      test-results.xml \
      ci

- name: Publish Test Results
  uses: dorny/test-reporter@v1
  if: always()
  with:
    name: Cosmos Tests
    path: test-results.xml
    reporter: java-junit
```

---

<a id="performance-reference"></a>

## Performances de référence

| Étape | x64 | ARM64 |
|-------|-----|-------|
| Build du noyau | ~60 s | ~70 s |
| Exécution de HelloWorld | 2-5 s | 5-10 s |
| Exécution de Memory | 60-120 s | 120-240 s |

---

<a id="troubleshooting"></a>

## Dépannage

<a id="timeout"></a>

### Délai dépassé
- Augmentez l'argument de délai d'expiration
- Consultez `uart-output.log` pour repérer des problèmes de démarrage
- Vérifiez que QEMU est installé : `qemu-system-x86_64 --version`

<a id="build-failures"></a>

### Échecs de build
- Exécutez `.devcontainer/postCreateCommand.sh` pour recompiler le framework
- Restaurez les paquets NuGet : `dotnet restore`
- Vérifiez que le SDK .NET 9 est installé

<a id="arm64-issues"></a>

### Problèmes sur ARM64
- Assurez-vous que le firmware UEFI est présent : `~/.cosmos/tools/qemu/share/qemu/edk2-aarch64-code.fd`
- Utilisez un délai d'expiration plus long (90 s et plus pour HelloWorld, 180 s et plus pour Memory)

<a id="ud-exceptions-invalid-opcode"></a>

### Exceptions #UD (opcode invalide)
Certaines opérations peuvent déclencher des fautes d'opcode invalide selon l'état du runtime. Utilisez `TR.Skip()` pour les marquer au lieu de laisser le noyau planter.
