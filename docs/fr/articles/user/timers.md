---
source_hash: 2887234aca8973282e3c09d77045754365502467
---
<a id="timers-and-alarms"></a>

# Minuteurs et alarmes

Dans cet article, nous verrons le travail différé et périodique sur Cosmos Gen3 : bloquer pendant un certain temps, exécuter un callback plus tard, et choisir entre les deux gestionnaires qui proposent de l'exécuter.

C'est par ces deux gestionnaires qu'il faut commencer. `TimerManager` exécute un callback depuis l'interruption du minuteur ; `AlarmManager` en exécute un sur un thread du noyau. Ils prennent les mêmes arguments et se lisent presque à l'identique au point d'appel, et choisir le mauvais fige le noyau.

| | `TimerManager` | `AlarmManager` |
|---|---|---|
| Le callback s'exécute en | Contexte d'interruption | Contexte de thread |
| Le callback peut bloquer, allouer ou prendre un verrou | Non | Oui |
| Les appels de planification peuvent être faits depuis | N'importe où, gestionnaires d'interruption compris | Contexte de thread uniquement |
| Nécessite l'ordonnanceur | Non | Oui |
| Résolution | La période de la source de ticks | Le tick de l'ordonnanceur |
| `Schedule` renvoie | Un handle `SoftwareTimer`, ou null | Un identifiant d'alarme, ou 0 |
| Commutateur de fonctionnalité | `CosmosEnableTimer` | `CosmosEnableScheduler` |

Si vous trouvez des bugs ou quelque chose d'anormal, merci de [soumettre une issue](https://github.com/CosmosOS/Cosmos/issues/new/choose) sur notre dépôt.

---

<a id="enable-the-timer-in-your-kernel"></a>

## Activer le minuteur dans votre noyau

La prise en charge du minuteur dépend d'un commutateur de fonctionnalité. Assurez-vous que le `.csproj` de votre noyau ne la désactive pas (sa valeur par défaut est `true`), et laissez aussi l'ordonnanceur activé si vous voulez des alarmes :

```xml
<PropertyGroup>
  <CosmosEnableTimer>true</CosmosEnableTimer>
  <CosmosEnableScheduler>true</CosmosEnableScheduler>
</PropertyGroup>
```

Voici les directives `using` sur lesquelles s'appuient les extraits ci-dessous :

```csharp
using System;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.System.Timers;
```

La source de ticks est trouvée et enregistrée au démarrage, si bien que `TimerManager` est prêt dès que votre noyau s'exécute. `TimerManager.IsEnabled` répond à la question posée à la compilation et `TimerManager.IsInitialized` à celle posée à l'exécution.

---

<a id="waiting"></a>

## Attendre

`TimerManager.Wait` bloque le thread appelant pendant un nombre donné de millisecondes. C'est l'élément le plus simple de cette page et celui auquel la plupart des noyaux ont recours, généralement pour cadencer une boucle de scrutation :

```csharp
for (int i = 0; i < 10; i++)
{
    Console.WriteLine("tick " + i);
    TimerManager.Wait(500);
}
```

Avec `CosmosEnableTimer=false`, cet appel lève une `InvalidOperationException` qui nomme le commutateur, plutôt que de rendre la main immédiatement et de laisser une boucle tourner à vide.

---

<a id="running-a-callback-later"></a>

## Exécuter un callback plus tard

`TimerManager.Schedule` exécute un callback une seule fois après un délai, et `ScheduleRecurring` en exécute un à chaque période. Les deux renvoient un handle `SoftwareTimer` :

```csharp
bool cursorVisible = false;

/* Flipping a field is the shape an interrupt-context callback should have:
   the render loop reads it and does the drawing. */
SoftwareTimer? blink = TimerManager.ScheduleRecurring(
    () => cursorVisible = !cursorVisible,
    TimeSpan.FromMilliseconds(250));

/* ... later ... */
TimerManager.Cancel(blink);
```

Le handle est en lecture seule : `TimeoutNs`, `Recurring` et `IsActive` sont ce qu'un appelant peut interroger sur un minuteur en attente. Rendez-le à `Cancel` pour l'arrêter. `Cancel` ne renvoie `true` que si le minuteur était encore en attente, si bien que l'appeler deux fois, ou sur un minuteur ponctuel qui s'est déjà déclenché, n'est pas une erreur.

**Le callback s'exécute en contexte d'interruption.** Il ne doit pas bloquer, ne doit pas allouer et ne doit pas prendre de verrou, car le thread qu'il a interrompu en détient peut-être un. Il ne doit pas non plus laisser s'échapper d'exception : rien au-dessus de la distribution des interruptions ne la rattrape, et le noyau s'arrête.

`Schedule` et `ScheduleRecurring` renvoient `null` quand le minuteur est exclu de la compilation ou qu'aucune source de ticks n'est enregistrée, c'est pourquoi le handle est nullable. `Cancel` accepte null et répond `false`.

---

<a id="running-a-callback-that-needs-to-do-real-work"></a>

## Exécuter un callback qui doit faire un vrai travail

Tout ce que le paragraphe ci-dessus interdit relève d'une alarme. `AlarmManager` possède son propre thread du noyau et y exécute les callbacks, si bien qu'un callback d'alarme peut bloquer, allouer, utiliser les primitives de l'ordonnanceur et prendre des verrous :

```csharp
ulong id = AlarmManager.ScheduleRecurring(
    () => Console.WriteLine("free pages: " + MemoryDiagnostics.FreePages),
    TimeSpan.FromSeconds(5));

/* ... later ... */
AlarmManager.Cancel(id);
```

La forme est volontairement la même que celle de `TimerManager`, afin que passer de l'un à l'autre se résume à changer un mot. Ce qui diffère, c'est ce que vous récupérez : une alarme appartient au système d'alarmes plutôt qu'au registre d'une source de ticks, elle est donc désignée par un identifiant `ulong`. Zéro signifie que l'alarme n'a pas été planifiée, ce qui arrive quand l'ordonnanceur ne tourne pas ou que la période n'est pas positive.

**Les appels de planification eux-mêmes sont réservés au contexte de thread.** Chaque membre d'`AlarmManager` prend le mutex de la liste des alarmes et se met en attente s'il est déjà détenu. En appeler un depuis un gestionnaire d'interruption provoque cette mise en attente à l'intérieur du gestionnaire d'interruption, qui se fige, et un callback de `TimerManager` est un gestionnaire d'interruption. Les membres de `TimerManager` masquent les interruptions au lieu de se mettre en attente et ne sont soumis à aucune restriction de ce genre, si bien qu'un callback de minuteur qui doit faire un vrai travail doit signaler un thread plutôt que planifier une alarme.

La période d'une alarme récurrente repart au moment où le callback se déclenche, et non au moment où il se termine, si bien qu'un callback qui dure plus longtemps que sa période trouve le déclenchement suivant déjà échu.

---

<a id="changing-the-tick-rate"></a>

## Changer la fréquence des ticks

`TimerManager.Frequency` lit et écrit la fréquence de la source de ticks en hertz. Chaque source de ticks divise un compteur fixe, chacune accepte donc une plage bornée : de 19 Hz à 1193180 Hz sur le PIT x64, et de 1 Hz jusqu'à `CNTFRQ_EL0` sur le minuteur générique ARM64. Une valeur hors de cette plage lève `ArgumentOutOfRangeException` plutôt que d'être ignorée en silence :

```csharp
Console.WriteLine("timer runs at " + TimerManager.Frequency + " Hz");
TimerManager.Frequency = 1000;
```

L'augmenter rapproche le déclenchement des callbacks planifiés de leur échéance et rend l'interruption elle-même plus coûteuse. Il y a rarement une raison de la modifier.

---

<a id="summary"></a>

## Résumé

| Tâche | Appel |
|---|---|
| Bloquer pendant un certain temps | `TimerManager.Wait(ms)` |
| Exécuter un petit callback plus tard | `TimerManager.Schedule(callback, delay)` |
| Exécuter un petit callback de façon répétée | `TimerManager.ScheduleRecurring(callback, period)` |
| En arrêter un | `TimerManager.Cancel(timer)` |
| Exécuter un callback qui bloque ou alloue | `AlarmManager.Schedule(callback, delay)` |
| Exécuter un tel callback de façon répétée | `AlarmManager.ScheduleRecurring(callback, period)` |
| En arrêter une | `AlarmManager.Cancel(id)` |
| Lire ou définir la fréquence des ticks | `TimerManager.Frequency` |
