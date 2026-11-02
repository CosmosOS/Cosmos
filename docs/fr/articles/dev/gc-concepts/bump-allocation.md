---
source_hash: df068ca56b8967afcd96fe18942e77988bd3cfe5
---
<a id="bump-allocation"></a>

# Allocation par incrément de pointeur

L'allocation par incrément de pointeur distribue la mémoire en faisant avancer un pointeur : l'allocateur conserve un curseur dans une région libre, retourne la position courante du curseur pour chaque demande, puis le fait avancer de la taille demandée. Une allocation se réduit à une comparaison et une addition, et les allocations successives se suivent en mémoire, ce qui favorise la localité de cache.

Sa limite est qu'elle ne fonctionne que sur un espace encore jamais touché. Dès que des objets ont été libérés au milieu d'une région, les trous ne se trouvent plus derrière le curseur ; ils doivent donc être recyclés au moyen de listes de blocs libres.

Dans OrionGC, chaque segment possède un curseur `Bump` pour l'espace neuf, les threads incrémentent leur pointeur en privé, à l'intérieur de leurs [TLAB](tlab.md), et les trous laissés par le balayage vont dans des listes de blocs libres par classe de taille. Voir [Segments](../garbage-collector.md#segments) et [Listes de blocs libres](../garbage-collector.md#free-lists).
