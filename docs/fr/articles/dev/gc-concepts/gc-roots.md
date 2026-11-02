---
source_hash: 64a40939d2441173952a7b4e7818954af5d6ada5
---
<a id="gc-roots"></a>

# Racines du GC

L'accessibilité a besoin d'un point de départ. Les racines sont les emplacements contenant des références qui existent hors du tas et sont considérés comme vivants par définition : les variables locales et les registres sauvegardés sur les piles des threads, les champs statiques, et les tables de handles par lesquelles le code natif ou celui du runtime maintient des objets en vie.

Un objet est vivant si et seulement s'il peut être atteint depuis une racine en suivant des références. Tout le reste est un déchet, même si d'autres objets morts pointent encore vers lui : un cycle d'objets qui pointent les uns vers les autres meurt en bloc dès qu'aucune racine ne l'atteint.

Dans OrionGC, les racines sont la pile du thread déclencheur (parcourue [précisément](conservative-vs-precise.md)), les piles et les registres de tous les autres threads (parcourus de façon conservatrice), et les handles GC forts ; les champs statiques sont atteints par un handle fort plutôt que parcourus directement. Voir [Phase de marquage](../garbage-collector.md#mark-phase).
