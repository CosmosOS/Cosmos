---
source_hash: d302768b2e42d019badc42f7ca4ec9e48270e310
---
<a id="preemption"></a>

# Préemption

Un ordonnanceur préemptif peut retirer le CPU à un thread en cours d'exécution sans sa coopération. Une interruption matérielle, le plus souvent un tick du minuteur, transfère le contrôle au noyau quoi que le thread soit en train de faire, et le noyau est alors libre de reprendre un autre thread à sa place. L'alternative est l'ordonnancement coopératif, où un changement n'a lieu que lorsque le thread en cours d'exécution s'y prête de lui-même, en cédant la main ou en se bloquant ; dans ce modèle, un seul thread qui ne cède jamais la main garde le CPU pour toujours, et une seule boucle boguée fige le système.

La préemption fait du temps CPU une ressource que le noyau attribue, et non une ressource que les threads prennent. La période du tick borne la durée pendant laquelle un thread peut monopoliser un cœur (son [quantum](scheduler-tick.md)), si bien qu'une boucle infinie dégénère en une part gaspillée au lieu de figer le système. C'est chaque thread qui en paie le prix : puisque le tick tombe à une [frontière d'instruction](instruction-boundary.md) quelconque, le code peut perdre le CPU entre deux instructions quelles qu'elles soient et doit protéger ses sections critiques en conséquence, en masquant les interruptions ou par des instructions atomiques.

Dans le noyau Cosmos, tout changement de contexte est aujourd'hui préemptif : le changement a lieu à la sortie d'une interruption (le tick de 10 ms, une interruption de périphérique dont le gestionnaire d'interruption a réveillé un autre thread, ou l'interruption qu'un thread qui se bloque déclenche sur son propre CPU), et sinon un thread s'exécute jusqu'à ce qu'il se mette en attente sur une primitive bloquante ou se termine ; il n'existe aucun changement volontaire qui fonctionne. Voir [Préemption](../scheduler.md#preemption) et [Ce qui n'existe pas : un changement volontaire](../scheduler.md#what-there-is-not-a-voluntary-switch).
