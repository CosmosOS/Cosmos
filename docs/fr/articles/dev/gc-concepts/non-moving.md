---
source_hash: a861bbfcc78dd04cf8a73924f415bb949d4f94a4
---
<a id="moving-vs-non-moving"></a>

# Déplaçant ou non déplaçant

Un ramasse-miettes déplaçant relocalise les objets vivants pendant la collecte, soit en les copiant dans une région neuve, soit en les compactant vers une extrémité du tas. Le déplacement vient à bout de la fragmentation et garde l'allocation simple et rapide, mais chaque référence vers un objet déplacé doit être retrouvée et réécrite, ce qui exige de savoir précisément où se trouve chaque référence.

Un ramasse-miettes non déplaçant laisse chaque objet à l'adresse où il a été alloué, pendant toute sa durée de vie. Les références n'ont jamais besoin d'être corrigées, les adresses des objets peuvent être transmises librement au code natif, et le [parcours conservateur](conservative-vs-precise.md) devient possible. Le prix à payer est la fragmentation, gérée avec des listes de blocs libres.

Les références conservatrices et le déplacement sont incompatibles : une « référence » trouvée de façon conservatrice peut en réalité être un entier qui ressemble par hasard à une adresse du tas ; elle ne peut donc pas être réécrite, et sa cible ne doit donc jamais être déplacée.

Dans OrionGC, rien ne se déplace jamais, et c'est aussi pour cela que l'épinglage ne coûte rien : l'adresse d'un objet épinglé est aussi stable que celle de n'importe quel autre objet. Voir [Vue d'ensemble](../garbage-collector.md#overview).
