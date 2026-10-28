---
source_hash: 61efc3aa0889868f154374a0ada65b6be50881f7
---
<a id="virtual-time-fair-share"></a>

# Partage équitable par temps virtuel

L'ordonnancement à partage équitable (ou à partage proportionnel) répartit le temps CPU selon le poids plutôt que selon le rang. Chaque thread détient un poids, et l'objectif de l'ordonnanceur est que, sur n'importe quelle fenêtre de temps, les threads reçoivent du temps CPU en proportion de leur poids : un thread de poids double reçoit deux fois plus de temps, et aucun poids, si petit soit-il, ne subit de famine. L'ordonnancement à priorités fixes répond à une autre question, celle de savoir qui s'exécute en premier ; le partage équitable répond à celle de la part que chacun reçoit.

Le temps virtuel est la comptabilité qui fait respecter ce partage. Chaque thread porte sa propre horloge, qui n'avance que pendant qu'il s'exécute, et à un rythme inversement proportionnel à son poids : l'horloge d'un thread lourd avance lentement, celle d'un thread léger avance vite. L'ordonnanceur exécute toujours le thread dont l'horloge est la plus en retard, si bien que les threads lourds accumulent davantage de temps réel avant que leur horloge ne rattrape son retard. Une horloge virtuelle globale marque « maintenant » : un thread qui sort du sommeil est recalé sur elle, assez près pour s'exécuter bientôt, mais pas si loin derrière qu'il monopolise le CPU pour récupérer le temps passé à dormir. L'ordonnancement Stride et le CFS de Linux sont les membres les plus connus de cette famille.

Dans le noyau Cosmos, la politique par défaut, [`StrideScheduler`](https://github.com/CosmosOS/Cosmos/blob/gen3/src/Cosmos.Kernel.Core/Scheduler/Stride/StrideScheduler.cs), applique exactement ce schéma : le poids est `Tickets`, le rythme de l'horloge est le stride (une constante divisée par le nombre de tickets), l'horloge propre à chaque thread est le `Pass`, et `PickNext` retire toujours le pass le plus bas d'une file triée selon cette valeur. Voir [La politique Stride](../scheduler.md#the-stride-policy).
