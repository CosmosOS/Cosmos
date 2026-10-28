---
source_hash: 4f1e0b4cc82ee1adba023c4016ca2067326c9954
---
# Funclet

Les compilateurs .NET ne compilent pas un bloc `catch`, `filter` ou `finally` directement dans le corps de sa méthode. Chaque bloc de traitement d'exception devient un funclet : une petite fonction à part entière, appelée par le dispatcher d'exceptions lorsque ce bloc s'exécute, avec son propre cadre de pile mais un accès direct aux variables locales de la méthode parente par l'intermédiaire du cadre parent.

Le parcours de la pile doit en tenir compte. Pendant l'exécution d'un bloc de traitement, la pile contient des cadres de funclet dont les références vivantes sont décrites par les métadonnées de la méthode parente, et non par quoi que ce soit qui leur soit propre, et le cadre parent, plus profond dans la pile, reste vivant au même moment. Un parcours qui traiterait un funclet comme une méthode ordinaire ne trouverait pour lui aucun [GCInfo](conservative-vs-precise.md) (les tables de références vivantes produites par le compilateur) et perdrait la trace de références vivantes.

Dans OrionGC, les métadonnées d'un funclet redirigent vers l'enregistrement GCInfo de la méthode parente, et le funclet est parcouru en appliquant le GCInfo du parent au cadre sur lequel le funclet s'exécute réellement. Voir [Comment le parcours précis remonte un thread](../garbage-collector-gcinfo.md#how-the-precise-scan-walks-a-thread).
