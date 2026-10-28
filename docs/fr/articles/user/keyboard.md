---
source_hash: 049e8aed74cf7cf5090635dc78c7ceb48e901a9d
---
<a id="keyboard"></a>

# Clavier

Dans cet article, nous verrons la saisie au clavier sur Cosmos Gen3 : comment lire du texte au moyen de l'API standard `Console`, comment réagir à chaque appui de touche, et comment changer de disposition de clavier.

Les principales différences si vous venez de Gen2 :

| | Gen2 | Gen3 |
|---|---|---|
| API du gestionnaire | `Cosmos.System.KeyboardManager` | La même API, dans `Cosmos.Kernel.System.Input`, avec `SetKeyLayout`/`GetKeyLayout` renommées en `SetLayout`/`GetLayout` |
| `Console.ReadLine` / `Console.ReadKey` | Remplacées par un plug, adossées au gestionnaire | Remplacées par un plug, adossées au gestionnaire |
| Événements de touche | `KeyEvent` (`KeyChar`, `Key`, `Modifiers`), touches représentées par `ConsoleKeyEx` | Identique, touches représentées par `Key` avec les mêmes membres |
| Dispositions | Dispositions US, FR, DE, ES, GB, TR et Dvorak bâties sur `ScanMapBase` | Le même ensemble bâti sur `KeyboardLayout`, dans `Cosmos.Kernel.System.Input.Layouts` |
| Périphériques | Clavier PS/2 | Clavier PS/2 (x64), virtio-keyboard et clavier USB, tous sur le kit de pilotes (le 8042 sur q35 et sur tout PC ; virtio sur PCI sur les deux architectures et sur MMIO sur ARM64 ; USB sur un contrôleur xHCI, EHCI ou OHCI sur les deux) |

Si vous trouvez des bugs ou quelque chose d'anormal, merci de [soumettre une issue](https://github.com/CosmosOS/Cosmos/issues/new/choose) sur notre dépôt.

<a id="enable-the-keyboard-in-your-kernel"></a>

## Activer le clavier dans votre noyau

La prise en charge du clavier dépend d'un commutateur de fonctionnalité. Assurez-vous que le `.csproj` de votre noyau ne le désactive pas (il vaut `true` par défaut) :

```xml
<PropertyGroup>
  <CosmosEnableKeyboard>true</CosmosEnableKeyboard>
</PropertyGroup>
```

Voici les directives `using` sur lesquelles reposent les extraits ci-dessous :

```csharp
using System.Drawing;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.Graphics.Fonts;
using Cosmos.Kernel.System.Input;
using Cosmos.Kernel.System.Input.Layouts;
```

Il n'y a rien à initialiser à la main : le clavier PS/2 (x64), un clavier virtio ou un clavier USB est lié par le `Ps2KeyboardDriver`, le `VirtioInputDriver` ou le `UsbKeyboardDriver` du kit de pilotes pendant l'étape des pilotes, puis publié auprès du consommateur du gestionnaire, qui l'enregistre ([Périphériques PS/2](drivers.md#ps2-devices), [Périphériques virtio](drivers.md#virtio-devices), [Périphériques USB](drivers.md#usb-devices)).

Les claviers USB ont aussi besoin des pilotes USB, `CosmosEnableUsb`. Vous n'avez pas à le définir : s'il est laissé non défini, il est activé dès que `CosmosEnableKeyboard` l'est. Le mettre à `false` conserve les claviers PS/2 et virtio et retire les claviers USB en même temps que les pilotes de contrôleur hôte.

<a id="reading-a-line"></a>

## Lire une ligne

`Console.ReadLine()` fonctionne comme partout ailleurs dans .NET, si bien que la plupart des noyaux n'ont jamais besoin de rien d'autre :

```csharp
Console.WriteLine("What is your name?");
Console.Write("> ");

string? name = Console.ReadLine();
Console.WriteLine("Hello, " + name + "!");
```

L'éditeur de ligne ne se limite pas à la saisie : les flèches gauche et droite déplacent le curseur dans la saisie, Début et Fin sautent à l'une ou l'autre extrémité, Retour arrière et Suppr effacent des caractères, et une saisie au milieu de la ligne insère au lieu de remplacer.

<!-- video: saisie de "Comsos Gen3", puis correction de la faute de frappe avec Début, les flèches et Suppr avant d'appuyer sur Entrée -->
<video src="images/keyboard-readline.mp4" controls autoplay muted loop playsinline style="max-width:100%"></video>

<a id="reading-single-keys"></a>

## Lire des touches une à une

Pour tout ce qui se situe sous le niveau de la ligne, `KeyboardManager.ReadKey()` bloque jusqu'à l'appui d'une touche et renvoie un `KeyEvent` :

```csharp
Console.WriteLine("Press keys to inspect them (Escape to leave):");

while (true)
{
    KeyEvent key = KeyboardManager.ReadKey();
    if (key.Key == Key.Escape)
    {
        break;
    }

    Console.WriteLine("Key: " + key.Key + "   Char: '" + key.KeyChar
        + "'   Modifiers: " + key.Modifiers);
}
```

Un `KeyEvent` porte trois informations :

| Propriété | Type | Signification |
|---|---|---|
| `Key` | `Key` | La touche physique, indépendante de la disposition et des modificateurs (`A`, `D5`, `F1`, `LeftArrow`, ...) |
| `KeyChar` | `char` | Le caractère de texte que produit la touche, `'\0'` si elle n'en produit aucun (touches de fonction, flèches, combinaisons avec Ctrl) |
| `Modifiers` | `ConsoleModifiers` | Les indicateurs Shift, Alt et Control actifs au moment de l'appui |

<!-- video: la boucle d'inspection affiche Key/Char/Modifiers pour des lettres, Shift+B, Shift+5, Ctrl+A, Alt+X, F1, F12, LeftArrow et Espace -->
<video src="images/keyboard-readkey.mp4" controls autoplay muted loop playsinline style="max-width:100%"></video>

La méthode standard `Console.ReadKey()` fonctionne aussi et renvoie un `ConsoleKeyInfo` ordinaire ; la surcharge `ReadKey(true)` supprime l'écho. Ce n'est qu'une fine enveloppe autour de `KeyboardManager.ReadKey()` : utilisez celle qui convient le mieux à votre code.

Dès qu'un noyau ouvre une deuxième session de console, le clavier saisit dans la session affichée à l'écran, et `Console` comme `KeyboardManager` lisent les touches de la session du thread appelant ([Sessions de console et Telnet](sessions.md)).

<a id="polling-without-blocking"></a>

## Scruter sans bloquer

Une boucle de rendu ne peut pas se permettre de bloquer sur `ReadKey()`. `KeyboardManager.TryReadKey()` retire de la file un appui de touche en attente et renvoie `false` quand il n'y en a pas, si bien que l'image suit son cours. Ici, il déplace un carré sur un canvas tiré de l'article [Graphismes](graphics.md) :

```csharp
Canvas canvas = Canvas.GetFullScreen();
Font font = PCScreenFont.DefaultFont;

int x = (canvas.Width - 60) / 2;
int y = (canvas.Height - 60) / 2;
const int Step = 20;

bool running = true;
while (running)
{
    /* Drain every key pressed since the last frame */
    while (KeyboardManager.TryReadKey(out KeyEvent? key))
    {
        switch (key.Key)
        {
            case Key.UpArrow: y -= Step; break;
            case Key.DownArrow: y += Step; break;
            case Key.LeftArrow: x -= Step; break;
            case Key.RightArrow: x += Step; break;
            case Key.Escape: running = false; break;
        }
    }

    canvas.Clear(Color.MidnightBlue);
    canvas.DrawString("Move the square with the arrow keys", font, Color.White, 40, 40);
    canvas.DrawFilledRectangle(Color.Gold, x, y, 60, 60);
    canvas.Display();

    Thread.Sleep(15);
}
```

<!-- video: le carré doré se déplace sur le canvas sous l'action des flèches -->
<video src="images/keyboard-square.mp4" controls autoplay muted loop playsinline style="max-width:100%"></video>

`KeyboardManager.KeyAvailable` (ou `Console.KeyAvailable`, remplacé par un plug) vous indique si un appui de touche attend, sans le consommer, et `KeyboardManager.Peek()` renvoie le `KeyEvent` suivant tout en le laissant dans la file.

<a id="modifiers-and-lock-keys"></a>

## Modificateurs et touches de verrouillage

`KeyboardManager` suit l'état des modificateurs et des verrouillages de façon globale, en dehors de tout événement de touche :

| Propriété | Signification |
|---|---|
| `ShiftPressed` | Une touche Maj est actuellement maintenue |
| `ControlPressed` | Une touche Ctrl est actuellement maintenue |
| `AltPressed` | Une touche Alt est actuellement maintenue |
| `CapsLock` | Verr. Maj est activé |
| `NumLock` | Verr. Num est activé |
| `ScrollLock` | Arrêt défil est activé |

Les touches de verrouillage basculent leur état à chaque appui et mettent à jour les voyants du clavier (ceux d'un clavier virtio restent tels quels : le pilote ne gère pas la file d'état du périphérique). Les modificateurs maintenus arrivent aussi dans chaque `KeyEvent` par ses indicateurs `Modifiers`, ce qui est en général la forme la plus pratique.

AltGr est la touche Alt droite sur les dispositions qui donnent à leurs touches un troisième niveau (allemande, espagnole, turque). Tant qu'elle est maintenue, une touche est convertie par la colonne Control+Alt de la disposition, et le `KeyEvent` porte à la fois `Control` et `Alt` dans ses `Modifiers`, comme Windows signale cette même touche. `ControlPressed` et `AltPressed` restent l'état des touches Ctrl et Alt physiques. Sur les autres dispositions, la touche Alt droite est une seconde touche Alt.

<a id="keyboard-layouts"></a>

## Dispositions de clavier

Les appuis de touche sortent du matériel sous forme de scancodes indépendants de la disposition ; une disposition les transforme en caractères. La disposition par défaut est US QWERTY, et `SetLayout` en change à tout moment :

```csharp
KeyboardManager.SetLayout(new FRStandardLayout());
```

Sept dispositions sont fournies dans `Cosmos.Kernel.System.Input.Layouts` :

| Classe | Disposition |
|---|---|
| `USStandardLayout` | US QWERTY (par défaut) |
| `FRStandardLayout` | AZERTY français |
| `DEStandardLayout` | QWERTZ allemand |
| `ESStandardLayout` | QWERTY espagnol |
| `GBStandardLayout` | QWERTY britannique |
| `TRStandardLayout` | Q turc |
| `USDvorakLayout` | Dvorak US |

Le changement est visible immédiatement : ci-dessous, les six mêmes touches physiques sont tapées deux fois, d'abord avec la disposition US, puis avec la disposition française.

<!-- video: saisie des six touches à droite de Tab avec la disposition US ("qwerty"), passage à FRStandardLayout, nouvelle saisie des mêmes touches ("azerty") -->
<video src="images/keyboard-layouts.mp4" controls autoplay muted loop playsinline style="max-width:100%"></video>

`KeyboardManager.GetLayout()` renvoie la disposition active, et une disposition personnalisée est une classe dérivée de `KeyboardLayout` qui redéfinit `InitializeKeys()` pour remplir la liste `Keys` d'entrées `KeyMapping`. Cette méthode s'exécute une seule fois, la première fois qu'un scancode atteint la disposition.

Les scancodes sont les codes d'enfoncement (make codes) du jeu 1, débarrassés du préfixe `E0` des touches étendues : c'est ainsi que les dispositions fournies listent les touches Windows et le bloc de navigation. La seule exception est la touche Alt droite, dont le code nu est celui de l'Alt gauche : les pilotes la signalent sous la forme `0x60`, un code que le jeu 1 laisse inutilisé. Une disposition associe ce code à `Key.AltGr` pour faire de la touche son modificateur de troisième niveau, et à `Key.RAlt` pour qu'elle reste un simple Alt. Une disposition qui ne l'associe à aucun des deux perd la touche. Le troisième niveau d'une touche est l'argument `ctrlAlt` de `KeyMapping`, et `ctrlAltShift` en est le quatrième :

```csharp
/* Right Alt selects the third level on this layout */
Keys.Add(new KeyMapping(0x60, Key.AltGr));
/* Q: q, Q, and @ under AltGr */
Keys.Add(new KeyMapping(0x10, 'q', 'Q', 'q', 'Q', 'q', 'Q', '@', Key.Q));
```

<a id="current-limitations"></a>

## Limitations actuelles

- Les relâchements de touche ne sont pas mis en file : `KeyEvent.Type` a une valeur `Break`, mais seuls les appuis atteignent le tampon. Les relâchements de Maj, Ctrl et Alt mettent à jour l'état des modificateurs et sont sinon ignorés.
- Les dispositions allemande, espagnole et turque portent leurs caractères AltGr ; les dispositions française et britannique pas encore (`@`, `#`, `{` en AZERTY), si bien que leur touche Alt droite reste un simple Alt. Les touches mortes ne sont pas composées : les `¨`, `~`, `´` et `` ` `` de la disposition turque sortent tels quels.
- Un clavier PS/2 est publié une seule fois, pendant l'étape des pilotes, quand le pilote de clavier s'est lié au nœud du port clavier du pilote 8042 et que sa réinitialisation a reçu une réponse ; un clavier virtio présent au démarrage l'est une seule fois, quand l'étape propose son périphérique. Un clavier USB est publié quand son nœud est lié et retiré quand ce nœud est retiré, tout comme un clavier virtio dont la fonction PCI se trouve derrière un emplacement PCI Express de branchement à chaud, que le pilote du port racine publie et retire : ce sont donc ces claviers-là que l'on peut brancher et débrancher pendant que le noyau s'exécute ; un modificateur maintenu sur un clavier débranché reste enfoncé jusqu'à ce qu'on l'enfonce sur un autre clavier.

<a id="how-it-works"></a>

## Fonctionnement

Chaque appui de touche sur le clavier PS/2 lève l'IRQ 1 sur x64 ; le gestionnaire d'interruption du pilote 8042 lit l'octet sur le port de données du contrôleur et le remet au `Ps2Access` du port clavier, dont la source réveille le gestionnaire d'interruption du pilote de clavier : celui-ci intègre le préfixe 0xE0 et le bit de relâchement dans des scancodes du jeu 1 (l'Alt droite devenant 0x60) et les signale, à travers le récepteur du clavier, au consommateur du gestionnaire. Un clavier virtio dépose plutôt un enregistrement d'événement dans sa file : l'interruption de la file (un message MSI-X sur PCI, la ligne GIC de l'emplacement sur MMIO) planifie la vidange du pilote sur le thread de travail du kit de pilotes, ou bien la vidange s'exécute toutes les 20 ms quand aucune interruption n'a pu être routée, et la vidange convertit le code de touche Linux en ce même scancode du jeu 1 et le signale, à travers le récepteur du clavier, au consommateur du gestionnaire, qui alimente `KeyboardManager` comme le fait celui du clavier PS/2. Les rapports de 8 octets du protocole boot d'un clavier USB arrivent sur son pipe d'interruption IN, livrés par l'interruption du pilote de contrôleur hôte (un message MSI-X sur xHCI, la ligne d'interruption héritée de la fonction sur EHCI et OHCI) ou, quand aucune n'a pu être routée, par sa scrutation toutes les 20 ms ; la routine de `UsbKeyboardDriver` qui les traite compare chaque rapport au précédent pour en tirer les mêmes scancodes du jeu 1 et les signale à travers le même récepteur, et les touches de verrouillage allument ses voyants par `SetLeds`, un transfert de contrôle SET_REPORT sur le thread de travail du kit. Les touches de verrouillage allument aussi les voyants du clavier PS/2, par `SetLeds` (0xED et l'octet des voyants) sur le thread de travail du kit. Le gestionnaire du clavier oriente les touches de verrouillage et de modification vers les propriétés d'état, et convertit tout le reste, à travers la disposition active, en un `KeyEvent` placé dans le tampon de touches. `ReadKey()` met le CPU en pause jusqu'à ce qu'une interruption livre l'événement suivant ; `TryReadKey()` se contente de retirer de la file. `Console.ReadLine` et `Console.ReadKey` sont des plugs posés sur la même file, si bien que la saisie de la console et les événements de touche bruts n'entrent jamais en conflit.

```
Console.ReadLine / Console.ReadKey        (plugs, Cosmos.Kernel.Plugs)
        │
KeyboardManager ── KeyEvent queue ◀── active layout
        │                                    ▲
        │                              raw scan codes
        │                                    │
PS/2 keyboard over the driver kit (the 8042, IRQ 1, x64)  /  virtio-keyboard over the driver kit (PCI, MMIO)  /  USB keyboard over the driver kit (xHCI, EHCI, OHCI)
```
