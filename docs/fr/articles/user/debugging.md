---
source_hash: 24dfc48bd1d8b598536dcee440b53d3c1af4e5b2
---
<a id="debugging-with-vs-code-and-qemu"></a>

# Déboguer avec VS Code et QEMU

Le débogage du noyau passe par GDB à distance : QEMU expose un serveur GDB sur `localhost:1234`, VS Code s'y connecte avec `cppdbg`, et les points d'arrêt se posent directement dans l'éditeur. Le modèle `cosmos new` fournit les fichiers `.vscode/launch.json` et `.vscode/tasks.json` nécessaires, déjà configurés.

---

<a id="prerequisites"></a>

## Prérequis

| Outil | Rôle | Remarques |
|------|---------|-------|
| `gdb` | Débogage x64 | Doit être dans le `PATH` ; la configuration de lancement l'invoque sous le nom `gdb` |
| `gdb-multiarch` | Débogage ARM64 | Doit être dans le `PATH` ; requis pour les cibles aarch64 |
| QEMU | Exécute le noyau | Installé par `cosmos install` |
| Extension C/C++ de VS Code | Adaptateur de débogage `cppdbg` | `ms-vscode.cpptools` |

`cosmos check` vérifie la chaîne d'outils. Sous Debian/Ubuntu, `apt install gdb gdb-multiarch` installe les deux débogueurs.

---

<a id="debugging-a-kernel-created-with-cosmos-new"></a>

## Déboguer un noyau créé avec `cosmos new`

1. Ouvrez le dossier du noyau dans VS Code.
2. Placez des points d'arrêt dans le code source de votre noyau.
3. Ouvrez la vue **Exécuter et déboguer** et sélectionnez **Debug x64 Kernel** (ou **Debug ARM64 Kernel**).
4. Appuyez sur F5. La tâche préalable au lancement compile le noyau, démarre QEMU avec `-s -S` (serveur GDB, exécution figée au démarrage), puis le débogueur s'y attache. L'exécution s'arrête sur vos points d'arrêt.

---

<a id="serial-log"></a>

## Journal série

Chaque phase de démarrage du noyau écrit dans le port série (COM1 sur x64, PL011 sur ARM64), que `cosmos run` relie à votre terminal. Quand un noyau ne démarre pas, ou plante avant que le débogueur ne soit utile, le journal série est la première chose à lire ; voir [Démarrage du noyau](startup.md) pour un parcours phase par phase et pour savoir comment résoudre les adresses de plantage en symboles.

---

<a id="writing-to-the-serial-log"></a>

## Écrire dans le journal série

`Log` place votre propre sortie sur ce même flux. C'est le pendant de `Console.WriteLine`, et la différence compte précisément dans les cas où vous avez besoin d'un journal : `Console` dessine sur le canvas du framebuffer, il lui faut donc des graphismes déjà initialisés, il redessine l'écran à chaque écriture et il alloue. `Log` écrit directement sur le port série, de manière synchrone, sans allocation pour les chaînes et les nombres, et fonctionne dès la première ligne de `BeforeRun`, avant qu'il n'existe une console où écrire comme après qu'un plantage a emporté le framebuffer avec lui.

```csharp
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.System.Storage;

Log.WriteString("storage: ");
Log.WriteNumber(StorageManager.DeviceCount);
Log.WriteString(" device(s), ");
Log.WriteNumber(StorageManager.Partitions.Count);
Log.WriteString(" partition(s)\n");
```

Il n'y a pas de `WriteLine` : ajoutez `"\n"` vous-même, comme le fait le noyau partout.

| Appel | Écrit |
|---|---|
| `Log.WriteString(text)` | Une chaîne. Prend un argument non null |
| `Log.Write(a, b, c)` | Le `ToString()` de chaque argument, dans l'ordre. Accepte null et écrit `null` à sa place |
| `Log.WriteNumber(n)` | Un nombre en décimal. Surcharges pour `int`, `uint`, `long` et `ulong` |
| `Log.WriteHex(n)` | Un nombre en hexadécimal |
| `Log.WriteHexWithPrefix(n)` | La même chose, précédée de `0x` |
| `Log.WriteBytes(span)` | Des octets bruts, sans mise en forme |

Préférez `WriteString` et `WriteNumber` à `Write` sur tout chemin qui s'exécute souvent ou tôt. La liste d'arguments de `Write` est construite sur la pile et ne coûte rien, mais un nombre ou un `bool` qui lui est passé subit d'abord un boxing, et cet objet boxé est une allocation sur le tas que les surcharges typées n'effectuent pas.

Le port série lui-même dépend du commutateur de fonctionnalité `CosmosEnableUART`. Quand il est désactivé, les appels retournent toujours normalement, mais rien n'atteint le flux série : un noyau qui désactive ce commutateur ne doit donc pas compter sur `Log` pour quoi que ce soit qu'il a besoin de relire.

---

<a id="known-limitations"></a>

## Limitations connues

- L'expérience de débogage dans VS Code souffre de bugs de correspondance avec le code source et d'inspection des variables (voir la [feuille de route](../../roadmap.md)) ; le pas à pas et les points d'arrêt fonctionnent, mais l'inspection de certaines variables locales peut afficher des valeurs fausses ou absentes.
- Le débogage suppose QEMU. VMware, VirtualBox et Hyper-V sont des cibles non testées.
- Le débogage ARM64 sous émulation TCG est lent : attendez-vous à des pauses de plusieurs secondes lors des opérations de pas à pas sur les gros noyaux.
