---
source_hash: ab8aa3258dbf81e84a5cfb2ad016a23e6e500cf4
---
<a id="console-sessions-and-telnet"></a>

# Sessions de console et Telnet

Dans cet article, nous verrons les sessions de console sur Cosmos Gen3 : exécuter plusieurs shells côte à côte sur l'écran local, passer de l'un à l'autre comme Linux passe d'un tty à l'autre, et servir un shell à des terminaux distants par Telnet.

Une session de console est une console à laquelle `System.Console` peut être liée : un écran, et les touches qui y sont tapées. `Console` suit le thread appelant. Un thread démarré sur une session lit les touches de cette session et écrit sur son écran, si bien que le même code `Console.ReadLine()` et `Console.WriteLine()` s'exécute sans modification sur chaque session, locale ou distante.

| Type | Touches venant de | Écran affiché sur | Créée par |
|---|---|---|---|
| Console principale, `tty1` | Clavier local | L'écran | Le noyau, sur `KernelConsole.Default` |
| Console virtuelle, à partir de `tty2` | Clavier local | L'écran, tant qu'elle est la session active | `SessionManager.CreateVirtualConsole()` |
| Session Telnet | Le client Telnet | Le terminal du client, et l'écran tant qu'elle est la session active | `TelnetServer` du package [Cosmos.Network.Telnet](https://github.com/CosmosOS/Cosmos.Network.Telnet), un par connexion |

Les extraits ci-dessous supposent `using Cosmos.Kernel.System.Sessions;`.

Les sessions ont besoin de l'ordonnanceur (`CosmosEnableScheduler`), puisque chaque session autre que la console principale s'exécute sur son propre thread. Elles ont aussi besoin de la console du noyau, puisque chaque écran est une grille sur le canvas de l'écran. Le serveur Telnet a en plus besoin du réseau (`CosmosEnableNetwork`).

<a id="the-primary-console"></a>

## La console principale

`tty1` est la console que tout noyau possède déjà. La boucle principale du noyau (`Run()`) s'exécute dessus, tout comme chaque thread qui n'a pas été démarré sur une autre session, et elle ne peut pas être fermée. Un noyau qui n'ouvre jamais de session se comporte exactement comme avant : `Console` et `KeyboardManager` lisent directement le clavier.

<a id="virtual-consoles"></a>

## Consoles virtuelles

`SessionManager.CreateVirtualConsole()` ouvre une console locale de la taille de la console principale. Elle démarre masquée, et `SessionManager.Start` démarre un thread dessus :

```csharp
ConsoleSession session = SessionManager.CreateVirtualConsole();

SessionManager.Start(session, () =>
{
    Console.WriteLine("Hello from " + session.Name);

    while (Console.ReadLine() is { } line)
    {
        Console.WriteLine("You typed: " + line);
    }
});
```

La `Console` du thread est la session. Quand le corps se termine, la session se ferme : une console virtuelle disparaît, et une session distante raccroche. Une exception qui s'échappe du corps est journalisée sur le port série et met fin à la session de la même façon.

Les threads que le corps démarre ne sont pas liés à la session : leur `Console` est la console principale. Liez-les avec `SessionManager.Start` s'ils doivent écrire sur la session.

<a id="switching-sessions"></a>

## Changer de session

Les sessions sont numérotées de 1 à 12 (`SessionManager.MaxSessions`), et le numéro est aussi celui de la touche de fonction qui l'affiche :

| Touches | Affiche |
|---|---|
| Alt+F1 … Alt+F12 | La session 1 … 12 |
| Alt+Gauche, Alt+Droite | La session ouverte précédente ou suivante |

La session à l'écran est `SessionManager.Active`. Ce qui est tapé au clavier local va dans cette session, qu'elle soit locale ou distante : vous pouvez donc prendre la main sur une session Telnet depuis la machine elle-même. Depuis le code :

```csharp
foreach (ConsoleSession session in SessionManager.GetSessions())
{
    Console.WriteLine(session.Id + "  " + session.Name + (session.IsActive ? "  (on the display)" : ""));
}

SessionManager.Switch(2);   // show tty2
```

Dès qu'une deuxième session existe, le clavier est aiguillé : chaque touche va directement à la session à l'écran. À partir de là, les membres de lecture de `KeyboardManager` (`KeyAvailable`, `TryReadKey`, `ReadKey`, `Peek`) lisent eux aussi la session du thread appelant, comme `Console`.

Quand QEMU s'exécute dans une fenêtre, le bureau de l'hôte peut intercepter Alt avec une touche de fonction avant que l'invité ne la voie ; Alt+F4 en particulier peut fermer la fenêtre. Alt avec les flèches, ou `SessionManager.Switch` depuis une commande du shell, évitent ce problème.

<a id="serving-a-shell-over-telnet"></a>

## Servir un shell par Telnet

Le serveur Telnet est un package à part entière, [Cosmos.Network.Telnet](https://github.com/CosmosOS/Cosmos.Network.Telnet). Ajoutez-le à votre noyau :

```xml
<ItemGroup>
    <PackageReference Include="Cosmos.Network.Telnet" Version="1.0.0" />
</ItemGroup>
```

`TelnetServer` écoute sur un port TCP et donne à chaque client sa propre session, avec un thread qui exécute le shell que vous lui passez :

```csharp
using Cosmos.Network.Telnet;

TelnetServer server = new(session =>
{
    Console.WriteLine("Welcome, " + session.Name);

    while (Console.ReadLine() is { } line && line != "exit")
    {
        Console.WriteLine("You typed: " + line);
    }
});

server.Start();   // port 23 by default: new TelnetServer(shell, 2323) picks another
```

`Start()` retourne immédiatement ; le serveur accepte les connexions sur son propre thread jusqu'à `Stop()`, qui laisse connectées les sessions déjà ouvertes. Affectez `Log` pour recevoir une ligne à chaque connexion, déconnexion et échec, par exemple `Log = message => Cosmos.Kernel.System.Diagnostics.Log.WriteString(message + "\n")`. Le réseau doit être configuré, par DHCP ou à la main ([Réseau](network.md#configure-ipv4)), pour qu'un client puisse atteindre le serveur.

Le serveur négocie de la manière qu'attend un terminal :

- Il renvoie en écho ce qui est tapé et supprime le go-ahead, ce qui place le client en mode caractère par caractère, si bien que l'éditeur de ligne fonctionne à distance : flèches, Début et Fin, Retour arrière et Suppr.
- Il demande la taille de la fenêtre (NAWS), si bien que `Console.WindowWidth` et `WindowHeight` de la session sont ceux du client, et suivent ses changements quand la fenêtre est redimensionnée.
- La sortie est envoyée sous forme de séquences VT100 : les couleurs, les déplacements du curseur et `Console.Clear()` s'affichent sur le client comme sur l'écran.

Avec le réseau utilisateur de QEMU, redirigez un port de l'hôte vers le port 23 de l'invité, puis connectez-vous depuis l'hôte :

```
$ cosmos run --nic e1000e --hostfwd tcp::2323-:23
$ telnet localhost 2323
```

Avec QEMU lancé directement, la redirection est une option de la carte réseau en mode utilisateur : `-nic user,model=e1000e,hostfwd=tcp::2323-:23`.

> [!WARNING]
> Telnet envoie tout en clair, et le serveur ne demande aucun mot de passe : quiconque atteint le port obtient un shell. Servez-le sur un réseau de confiance, comme le réseau utilisateur privé de QEMU.

<a id="current-limitations"></a>

## Limitations actuelles

- Il n'y a pas d'authentification : un client Telnet obtient un shell dès qu'il se connecte.
- Il n'y a pas encore de SSH. Telnet est un premier transport au-dessus des mêmes sessions ; SSH demande un échange de clés, un algorithme de chiffrement et une source d'aléa dont le noyau ne dispose pas.
- La pile TCP ne retransmet pas. Sur le réseau utilisateur de QEMU rien ne se perd, mais sur un vrai réseau un segment perdu bloque la session.
- L'écran d'une session vit sur le canvas de l'écran, si bien qu'un noyau sans écran n'a aucune session, distantes comprises.
- Ctrl+C parvient à la session comme une simple touche ; rien n'interrompt une commande en cours.

<a id="remote-sessions-in-a-library"></a>

## Sessions distantes dans une bibliothèque

Un serveur pour un autre type de terminal distant est une classe dérivée de `ConsoleSession`, comme l'est celle du serveur Telnet :

- Le constructeur `ConsoleSession(int cols, int rows)` donne à la session son écran, masqué jusqu'à ce qu'il soit affiché. Redéfinissez `Name`, le nom sous lequel elle est listée, et `IsRemote`, qui doit valoir true.
- `EnqueueInput(KeyEvent)` met en file une touche envoyée par le terminal et réveille le lecteur de la session ; elle peut être appelée depuis n'importe quel thread. `Resize(cols, rows)` suit la fenêtre du terminal.
- Les hooks `On` signalent chaque changement à envoyer au terminal : `OnWritten` pour chaque caractère, `OnCleared`, `OnCursorMoved`, `OnCursorVisibilityChanged`, `OnColorsChanged`, avec `CursorLeft`, `CursorTop`, `ExplicitForeground` et `ExplicitBackground` pour en lire l'état. Ils s'exécutent au sein du changement, interruptions masquées : enregistrez ce qui a changé et retournez, sans bloquer ni faire d'E/S. `OnFlush` et `OnClosed` s'exécutent sans masquage.
- `SessionManager.TryRegister(session)` inscrit la session dans la liste, puis `SessionManager.Start(session, body)` exécute un thread dessus.

Les sockets du noyau ne peuvent pas être utilisés sans risque par deux threads à la fois, si bien que le serveur Telnet lit et écrit toutes ses connexions sur un seul thread qui lui est propre : ses hooks ajoutent leurs données à un tampon que ce thread envoie.

<a id="how-it-works"></a>

## Fonctionnement

Chaque session conserve son écran sous la forme d'une grille `KernelConsole` sur le canvas de l'écran. Seule la grille de la session active peint le canvas ; les autres tiennent leurs cellules à jour et repeignent tout l'écran quand elles sont affichées. Une session Telnet envoie aussi chaque changement à son client au moment où il se produit : chaque caractère, et la séquence d'échappement de chaque déplacement du curseur et de chaque changement de couleur, tirés de la grille. Les deux s'accordent donc sur l'endroit où chaque ligne est coupée : la grille passe à la ligne dès qu'une ligne est pleine, et la session termine explicitement la ligne sur le client.

Le lecteur d'une session se bloque sur un `InterruptEvent` qui est signalé quand une touche est mise en file pour elle. Les touches viennent soit du décodeur du clavier, qui les met en file sur la session active, éventuellement en contexte d'interruption, soit du serveur d'une session distante : le thread du serveur Telnet examine ses connexions toutes les 10 ms tant qu'elles sont inactives, et met en file les touches qu'un client a envoyées. Une touche de changement de session réveille un thread qui resterait sinon bloqué. Le lecteur de la console principale s'exécute sur le thread inactif de l'ordonnanceur : il attend donc l'interruption suivante au lieu de se bloquer, comme le clavier l'a toujours fait.
