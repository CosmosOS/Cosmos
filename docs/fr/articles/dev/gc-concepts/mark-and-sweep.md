---
source_hash: 5b018280d71a7409d4d5187bf5affc9fe83b136c
---
<a id="mark-and-sweep"></a>

# Marquage-balayage

Le marquage-balayage récupère la mémoire en deux phases. La phase de marquage part des [racines](gc-roots.md) et suit les références, en positionnant un bit de marquage sur chaque objet qu'elle atteint ; tout ce qui reste non marqué est inaccessible. La phase de balayage parcourt ensuite le tas de façon linéaire et transforme chaque objet non marqué en espace libre, en laissant les objets marqués exactement là où ils sont.

Sa propriété déterminante est que les objets vivants ne se déplacent jamais, ce qui en fait le partenaire naturel d'une conception [non déplaçante](non-moving.md). Le prix à payer est la fragmentation : les trous libérés restent des trous, si bien que l'allocateur doit les recycler au moyen de listes de blocs libres au lieu de récupérer une seule grande région contiguë, comme le font les ramasse-miettes par copie ou par compactage.

Dans OrionGC, le bit de marquage est le bit 0 du pointeur `MethodTable` de l'objet, et le balayage reconstruit les listes de blocs libres par classe de taille. Voir [Phase de marquage](../garbage-collector.md#mark-phase) et [Phase de balayage](../garbage-collector.md#sweep-phase).
