---
source_hash: 41d4d2e2b60f69ad12d015587a404401c8693962
---
<a id="aba-problem"></a>

# Problème ABA

Le code sans verrou met à jour l'état partagé par compare-and-swap (`Interlocked.CompareExchange`) : lire la valeur, en calculer une nouvelle, et ne la réécrire que si la valeur est toujours celle qui a été lue. Cette vérification a un angle mort : si un autre thread a fait passer la valeur de A à B puis de nouveau à A entre-temps, la comparaison réussit alors que l'état qu'elle décrivait n'existe plus. C'est le problème ABA. Pour des pointeurs et des indices vers des emplacements recyclables, « même valeur » ne veut pas dire « même signification ».

La correction habituelle est une étiquette de version : placer un compteur à côté de la valeur et l'incrémenter à chaque mise à jour, si bien qu'une valeur recyclée n'est plus jugée égale lors de la comparaison.

Dans OrionGC, le magasin de handles utilise ce schéma : chaque segment de handles regroupe la tête de sa liste d'emplacements libres, le nombre de handles vivants et une étiquette de version dans un seul mot de 64 bits mis à jour avec `CompareExchange`, de sorte qu'un emplacement libéré puis réalloué entre deux lectures ne peut pas être pris pour une liste d'emplacements libres inchangée. Voir [Magasin de handles](../garbage-collector.md#handle-store).
