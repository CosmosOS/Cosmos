---
source_hash: 20e12fa8884fd513f7b0e772c773d30aadfb4ff0
---
<a id="interrupts-at-instruction-boundaries"></a>

# Interruptions aux frontières d'instruction

Un cœur de CPU exécute une instruction machine à la fois, et il n'accepte une interruption en attente qu'à la jointure entre deux d'entre elles : l'instruction courante se termine, la suivante n'a pas commencé, et le cœur saute vers le gestionnaire d'interruption depuis ce point précis. « Une interruption peut se déclencher entre deux instructions quelconques » signifie exactement cela : toute paire d'instructions adjacentes dans le flux d'instructions est un point d'acheminement valide. Une instruction est atomique vis-à-vis des interruptions ; rien de plus long ne l'est.

Le code compilé transforme une ligne de code source en de nombreuses instructions, si bien qu'une ligne contient de nombreuses frontières. Même `x++` est un chargement, une addition et un stockage : une interruption peut tomber après le chargement et avant le stockage, le gestionnaire d'interruption peut s'exécuter longtemps, réveiller d'autres threads ou passer entièrement à un autre thread, et le code interrompu reprend ensuite sans aucun moyen de savoir que quoi que ce soit s'est produit, sur un état qui a pu changer à son insu. Une séquence de plusieurs instructions n'est atomique vis-à-vis des interruptions que si les interruptions sont masquées autour d'elle, ou si le matériel propose l'opération entière sous la forme d'une seule instruction atomique (un `CompareExchange`, par exemple).

Dans le noyau Cosmos, c'est la contrainte autour de laquelle l'ordonnanceur est construit : chaque transition du cycle de vie s'exécute avec les interruptions désactivées, et les primitives bloquantes suivent le [protocole de mise en attente](../scheduler.md#the-park-protocol), pour qu'un réveil acheminé sur l'une de ces frontières, au milieu d'une mise en attente, ne soit jamais perdu. C'est aussi pourquoi un thread préempté est rarement arrêté sur un [point sûr](../gc-concepts/safepoint.md) et fait l'objet du [parcours conservateur](../gc-concepts/conservative-vs-precise.md). Voir [Préemption](../scheduler.md#preemption).
