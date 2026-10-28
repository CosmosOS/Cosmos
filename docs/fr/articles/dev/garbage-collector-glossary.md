---
source_hash: 4ee8d070cc9a05ba415e67fdfdb1e34456614b93
---
<a id="garbage-collector-glossary"></a>

# Glossaire du ramasse-miettes

De courtes notes de contexte sur les concepts généraux du GC sur lesquels s'appuient les articles [Ramasse-miettes](garbage-collector.md) et [Parcours précis de la pile](garbage-collector-gcinfo.md). Chaque page explique d'abord le concept en lui-même, puis se termine par une ligne sur la façon dont OrionGC l'applique. Leurs pendants pour l'ordonnancement se trouvent dans le [Glossaire de l'ordonnanceur](scheduler-glossary.md).

| Concept | Résumé |
|---------|---------|
| [Stop-the-world](gc-concepts/stop-the-world.md) | Tous les threads sont en pause pendant toute la durée d'une collecte |
| [Marquage-balayage](gc-concepts/mark-and-sweep.md) | Marquer ce qui est atteignable, balayer le reste sur place |
| [Déplaçant ou non déplaçant](gc-concepts/non-moving.md) | Si les objets vivants peuvent un jour être déplacés ou non |
| [Générations](gc-concepts/gc-generations.md) | Des tas séparés selon l'âge des objets, et ce qu'une génération unique évite |
| [Allocation par incrément de pointeur](gc-concepts/bump-allocation.md) | Allouer en faisant avancer un pointeur dans un espace encore vierge |
| [TLAB](gc-concepts/tlab.md) | Des tampons par thread qui rendent le chemin rapide de l'allocation libre de toute synchronisation |
| [Racines du GC](gc-concepts/gc-roots.md) | Le point de départ de l'atteignabilité |
| [Parcours précis et parcours conservateur](gc-concepts/conservative-vs-precise.md) | Savoir quels mots de la pile sont des références, ou le deviner |
| [Point sûr](gc-concepts/safepoint.md) | Les emplacements du code où les tables du compilateur décrivent exactement l'état du GC |
| [Funclet](gc-concepts/funclet.md) | Des blocs de traitement d'exception compilés sous forme de petites fonctions séparées, dotées de leurs propres cadres de pile |
| [Finalisation et résurrection](gc-concepts/finalization.md) | Du code de nettoyage exécuté avant la récupération, et des objets morts qui redeviennent atteignables |
| [En-tête d'objet du runtime](gc-concepts/object-header.md) | Le mot de gestion placé juste avant chaque objet (code de hachage, verrou léger) |
| [Problème ABA](gc-concepts/aba-problem.md) | Pourquoi le code sans verrou a besoin d'étiquettes de version quand des valeurs peuvent être recyclées |
