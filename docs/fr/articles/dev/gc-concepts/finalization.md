---
source_hash: 28f09e332e3850f07b74a172adfb03de8db7646a
---
<a id="finalization-and-resurrection"></a>

# Finalisation et résurrection

Un finaliseur (`~ClassName` en C#) exécute du code de nettoyage avant que la mémoire d'un objet ne soit récupérée, généralement pour libérer une ressource native. La finalisation change la tâche du ramasse-miettes : un objet inaccessible dont le finaliseur est en attente ne peut pas encore être libéré, si bien que le GC doit le mettre en file, laisser un thread dédié exécuter le finaliseur, et ne récupérer la mémoire que lors d'une collecte ultérieure.

La résurrection en est l'effet secondaire : tant qu'il est en file, l'objet est de nouveau accessible (la file le référence), et son finaliseur peut même stocker `this` à un endroit accessible, ce qui rend de nouveau vivant, de façon permanente, un objet qui avait été trouvé mort. Les deux variantes de référence faible diffèrent précisément sur ce point : une référence faible courte (`WeakReference` par défaut) est effacée dès que l'objet est trouvé inaccessible, tandis qu'une référence faible qui suit la résurrection (`trackResurrection: true`, le type de handle `WeakTrackResurrection`) continue de renvoyer l'objet jusqu'à ce qu'il ait réellement disparu.

> [!NOTE]
> Documentation officielle : [Finaliseurs](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/finalizers).

OrionGC conserve les objets en attente de leur finaliseur dans une file qui est une racine de chaque collecte, efface les handles faibles courts avant de mettre ces objets en file et ceux qui suivent la résurrection après, et exécute les finaliseurs après la collecte. Ils s'exécutent sur un thread dédié aux finaliseurs, que `Kernel.Start` démarre lorsque le commutateur `CosmosEnableFinalizerThread` est activé (ce qui est le cas par défaut dès que l'ordonnanceur l'est), et sinon sur le thread qui a effectué la collecte ou l'allocation. Voir [Finalisation](../garbage-collector.md#finalization) et [Les handles pendant le marquage](../garbage-collector.md#handles-during-marking).
