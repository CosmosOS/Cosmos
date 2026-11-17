---
source_hash: f60a5f1bb89b90d7a97d4d5541a34c9417e27865
---
<a id="mouse"></a>

# Souris

Dans cet article, nous verrons les entrées souris sur Cosmos Gen3 : comment suivre la position du pointeur, réagir aux appuis sur les boutons et à la molette, et dessiner un curseur avec l'API graphique.

Les principales différences si vous venez de Gen2 :

| | Gen2 | Gen3 |
|---|---|---|
| API du gestionnaire | `Cosmos.System.MouseManager` | Le même modèle, dans `Cosmos.Kernel.System.Input` |
| État des boutons | Énumération de flags `MouseState` | Booléens `LeftButton`, `RightButton`, `MiddleButton` |
| Position | `X`, `Y` limités à la taille de l'écran | Identique |
| Molette | `ScrollDelta` + `ResetScrollDelta()` | Identique |
| Curseur | Dessiné par votre code | Dessiné par votre code |
| Périphériques | Souris PS/2 | Souris PS/2 avec molette (x64), virtio-mouse et souris USB, toutes sur le kit de pilotes (le 8042 sur q35 et sur tout PC ; virtio sur PCI sur les deux architectures, MMIO sur ARM64 ; USB sur un contrôleur xHCI, EHCI ou OHCI sur les deux) |

Si vous trouvez des bugs ou quelque chose d'anormal, merci de [soumettre une issue](https://github.com/CosmosOS/Cosmos/issues/new/choose) sur notre dépôt.

<a id="enable-the-mouse-in-your-kernel"></a>

## Activer la souris dans votre noyau

La prise en charge de la souris dépend d'un commutateur de fonctionnalité. Vérifiez que le `.csproj` de votre noyau ne le désactive pas (sa valeur par défaut est `true`) :

```xml
<PropertyGroup>
  <CosmosEnableMouse>true</CosmosEnableMouse>
</PropertyGroup>
```

Voici les directives `using` dont dépendent les extraits ci-dessous ; les types de dessin viennent de l'article [Graphismes](graphics.md) :

```csharp
using System.Drawing;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.Graphics.Fonts;
using Cosmos.Kernel.System.Input;
```

Comme pour le clavier, pendant l'étape des pilotes, la souris PS/2 (x64) est liée par le `Ps2MouseDriver` du kit de pilotes, une souris virtio par `VirtioInputDriver` et une souris USB par `UsbMouseDriver`, et chacune est publiée auprès du consommateur du gestionnaire, qui l'enregistre ([Périphériques PS/2](drivers.md#ps2-devices), [Périphériques virtio](drivers.md#virtio-devices), [Périphériques USB](drivers.md#usb-devices)) ; `MouseManager` est prêt dès que votre noyau s'exécute.

Les souris USB ont aussi besoin des pilotes USB, `CosmosEnableUsb`. Vous n'avez pas à le définir : laissé non défini, il est activé dès que `CosmosEnableMouse` l'est. Le mettre à `false` conserve les souris PS/2 et virtio et retire les souris USB en même temps que les pilotes de contrôleur hôte.

Pour essayer une souris USB dans QEMU, passez `--mouse usb-mouse` à `cosmos run` : le lanceur ajoute un contrôleur `qemu-xhci` (ou réutilise celui qu'ont déjà amené des disques USB ou un clavier `usb-kbd`) et y branche la souris, sur l'une ou l'autre architecture. `--usb-controller ehci`, `ohci` ou `ehci-ohci` met à sa place un EHCI, un OHCI ou un EHCI accompagné d'un OHCI compagnon, chaque périphérique USB étant rattaché à l'un de ses ports racines.

<a id="position-and-buttons"></a>

## Position et boutons

`MouseManager` est un ensemble de propriétés statiques que le gestionnaire d'interruption tient à jour : `X` et `Y` pour la position du pointeur, `LeftButton`, `RightButton` et `MiddleButton` pour les boutons. Aucun curseur n'apparaît à l'écran tant que vous n'en dessinez pas un, si bien que le code de souris est une boucle de rendu qui lit ces propriétés à chaque image :

```csharp
Canvas canvas = Canvas.GetFullScreen();
Font font = PCScreenFont.DefaultFont;

/* Clamp the pointer to the actual screen */
MouseManager.SetScreenSize(canvas.Width, canvas.Height);

while (true)
{
    canvas.Clear(Color.MidnightBlue);

    canvas.DrawString("X: " + MouseManager.X + "   Y: " + MouseManager.Y,
        font, Color.White, 40, 40);
    canvas.DrawString("Buttons: "
        + (MouseManager.LeftButton ? "L" : "-")
        + (MouseManager.MiddleButton ? "M" : "-")
        + (MouseManager.RightButton ? "R" : "-"),
        font, Color.White, 40, 100);

    /* A minimal cursor: a white dot with a black outline */
    canvas.DrawFilledCircle(Color.White, MouseManager.X, MouseManager.Y, 6);
    canvas.DrawCircle(Color.Black, MouseManager.X, MouseManager.Y, 6);

    canvas.Display();
    Thread.Sleep(15);
}
```

<!-- video: le point du curseur suit la souris pendant que l'affichage de la position se met à jour et que les indicateurs L, M et R s'allument à chaque appui sur un bouton -->
<video src="images/mouse-cursor.mp4" controls autoplay muted loop playsinline style="max-width:100%"></video>

Appelez `SetScreenSize` une fois au démarrage : le gestionnaire limite `X` et `Y` à ces bornes (la valeur par défaut est 1024x768, ce qui correspond rarement au framebuffer). Pour un vrai curseur en flèche, copiez un petit bitmap à l'écran à la place du cercle.

<a id="painting-with-the-mouse"></a>

## Peindre avec la souris

Les boutons sont de simples booléens, un programme de dessin se résume donc à quelques `if`. Les traits s'accumulent sur un canvas hors écran (voir [Canvas hors écran](graphics.md#off-screen-canvases)), de sorte que le curseur peut être dessiné par-dessus sans les effacer :

```csharp
Canvas screen = Canvas.GetFullScreen();
MouseManager.SetScreenSize(screen.Width, screen.Height);

Canvas paint = new Canvas(screen.Width, screen.Height);
paint.Clear(Color.MidnightBlue);

while (true)
{
    if (MouseManager.LeftButton)
    {
        paint.DrawFilledCircle(Color.Gold, MouseManager.X, MouseManager.Y, 6);
    }

    if (MouseManager.RightButton)
    {
        paint.Clear(Color.MidnightBlue);
    }

    screen.DrawCanvas(paint, 0, 0);
    screen.DrawFilledCircle(Color.White, MouseManager.X, MouseManager.Y, 6);
    screen.DrawCircle(Color.Black, MouseManager.X, MouseManager.Y, 6);
    screen.Display();

    Thread.Sleep(15);
}
```

<!-- video: des traits dorés peints en maintenant le bouton gauche, le canvas effacé d'un clic droit, puis de nouveaux traits peints -->
<video src="images/mouse-paint.mp4" controls autoplay muted loop playsinline style="max-width:100%"></video>

<a id="the-scroll-wheel"></a>

## La molette

Le mouvement de la molette s'accumule dans `MouseManager.ScrollDelta`, à raison d'une unité par cran. Le pilote ne le remet jamais à zéro : lisez-le, appliquez-le, puis appelez `ResetScrollDelta()` pour que le même mouvement ne soit pas appliqué deux fois.

```csharp
Canvas canvas = Canvas.GetFullScreen();
Font font = PCScreenFont.DefaultFont;

int radius = 60;

while (true)
{
    int delta = MouseManager.ScrollDelta;
    if (delta != 0)
    {
        radius = Math.Clamp(radius + delta * 10, 12, 320);
        MouseManager.ResetScrollDelta();
    }

    canvas.Clear(Color.MidnightBlue);
    canvas.DrawString("Scroll to resize the circle. Radius: " + radius,
        font, Color.White, 40, 40);
    canvas.DrawFilledCircle(Color.DeepSkyBlue,
        canvas.Width / 2, canvas.Height / 2, radius);
    canvas.Display();

    Thread.Sleep(15);
}
```

<!-- video: le cercle grandit et rétrécit à mesure que la molette tourne dans un sens puis dans l'autre -->
<video src="images/mouse-scroll.mp4" controls autoplay muted loop playsinline style="max-width:100%"></video>

<a id="sensitivity-and-pointer-control"></a>

## Sensibilité et contrôle du pointeur

Deux autres réglages sur `MouseManager` :

- `Sensitivity` multiplie chaque delta de mouvement (`1.0f` par défaut) ; les valeurs inférieures à 1 ralentissent le pointeur, les valeurs supérieures l'accélèrent.
- `SetPosition(x, y)` déplace le pointeur par programme, en le limitant aux bornes de l'écran, par exemple pour le centrer à l'ouverture d'une scène.

<a id="current-limitations"></a>

## Limitations actuelles

- Seuls les périphériques de pointage relatifs sont pris en charge. Il n'y a pas d'entrée absolue (tablette), si bien que dans la fenêtre d'une VM, le pointeur de l'invité ne suit pas le curseur de l'hôte point pour point.
- L'inclinaison horizontale de la molette est ignorée ; seule la molette verticale atteint `ScrollDelta`.
- Une souris PS/2 est publiée une seule fois, pendant l'étape des pilotes, lorsque le pilote de souris s'est lié au nœud du port auxiliaire du pilote 8042 et que sa réinitialisation a obtenu une réponse ; une souris virtio présente au démarrage l'est une seule fois, lorsque l'étape propose son périphérique. Une souris USB est publiée quand son nœud est lié et retirée quand le nœud est retiré, de même qu'une souris virtio dont la fonction PCI, que le pilote de port racine publie et retire, se trouve derrière un emplacement PCI Express à branchement à chaud : ce sont donc ces souris-là qui peuvent être branchées et débranchées pendant que le noyau s'exécute.
- Une souris USB est pilotée dans le protocole de démarrage HID, que toute souris de PC prend en charge : trois boutons, des deltas X et Y sur 8 bits et, sur une souris à molette, la molette dans l'octet qui les suit. Les boutons supplémentaires, l'inclinaison horizontale et la résolution plus fine que certaines souris rapportent dans leur propre protocole de rapport ne sont pas lus.

<a id="how-it-works"></a>

## Fonctionnement

Sur x64, la souris PS/2 lève l'IRQ 12 pour chaque octet d'un paquet de mouvement, que le pilote 8042 lit et transmet au `Ps2Access` du port auxiliaire ; le gestionnaire d'interruption du pilote de souris assemble les 3 octets de boutons et de deltas X/Y, portés à 4 par l'octet de la molette une fois que son sondage a activé le protocole IntelliMouse (la séquence de fréquences d'échantillonnage 200, 100, 80), écarte un octet qui ne peut pas commencer un paquet, et transmet un rapport par paquet, à travers le récepteur de pointeur, au consommateur du gestionnaire. Une souris virtio fournit les mêmes informations sous forme d'enregistrements d'événements dans sa file, sur PCI ou MMIO : le `VirtioInputDriver` du kit de pilotes les retire de la file sur le thread de travail du kit (réveillé par l'interruption de la file, ou toutes les 20 ms quand aucune n'a pu être routée), regroupe les événements d'axe et de bouton en un rapport par événement de synchronisation, et le transmet, à travers le récepteur de pointeur, au consommateur du gestionnaire. Les rapports de démarrage d'une souris USB arrivent sur son pipe IN d'interruption, livrés par l'interruption du pilote de contrôleur hôte (un message MSI-X sur xHCI, la ligne d'interruption classique de la fonction sur EHCI et OHCI) ou, quand aucune n'a pu être routée, par sa scrutation toutes les 20 ms ; la routine de `UsbMouseDriver` qui les traite transforme chaque rapport en un mouvement relatif (HID oriente Y vers le bas, comme le gestionnaire, et sa molette est inversée pour qu'une valeur positive fasse défiler vers le bas, comme sur PS/2) et le transmet à travers le même récepteur. Dans tous les cas, les deltas parviennent à `MouseManager`, qui applique `Sensitivity`, les ajoute à `X` et `Y`, les limite à la taille de l'écran et met à jour les booléens des boutons ; les deltas de la molette s'accumulent dans `ScrollDelta` jusqu'à ce que le code qui les scrute les consomme. Votre boucle de rendu ne fait jamais que lire l'état, c'est pourquoi aucun verrouillage n'est nécessaire.

```
Render loop (reads X/Y, buttons, ScrollDelta)
        │
MouseManager ── Sensitivity, screen clamping, ScrollDelta accumulation
        │
PS/2 mouse over the driver kit (the 8042, IRQ 12, x64)  /  virtio-mouse over the driver kit (PCI, MMIO)  /  USB mouse over the driver kit (xHCI, EHCI, OHCI)
```

<a id="summary"></a>

## Résumé

| Tâche | Appel |
|---|---|
| Lire la position du pointeur | `MouseManager.X`, `MouseManager.Y` |
| Lire les boutons | `MouseManager.LeftButton`, `MouseManager.RightButton`, `MouseManager.MiddleButton` |
| Dessiner un curseur à la position du pointeur | `canvas.DrawFilledCircle(color, MouseManager.X, MouseManager.Y, radius)` |
| Limiter le pointeur à l'écran | `MouseManager.SetScreenSize(width, height)` |
| Lire le mouvement de la molette | `MouseManager.ScrollDelta` |
| Le remettre à zéro une fois appliqué | `MouseManager.ResetScrollDelta()` |
| Ralentir ou accélérer le pointeur | `MouseManager.Sensitivity` |
| Déplacer le pointeur | `MouseManager.SetPosition(x, y)` |
