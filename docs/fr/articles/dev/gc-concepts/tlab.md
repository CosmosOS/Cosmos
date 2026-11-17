---
source_hash: 718968d73d90c8a0b512cfad67833f0e284c429a
---
<a id="tlab-thread-local-allocation-buffer"></a>

# TLAB (tampon d'allocation local au thread)

Lorsque de nombreux threads allouent dans un même tas, un curseur d'allocation partagé exigerait un verrou ou une opération atomique à chaque allocation. Un TLAB supprime ce coût : chaque thread réserve une portion privée du tas en une seule opération sur l'état partagé, puis [alloue par incrément de pointeur](bump-allocation.md) à l'intérieur sans aucune synchronisation. Ce n'est que lorsque cette portion est épuisée que le thread revient à l'état partagé pour une recharge.

La contrepartie est le gaspillage : la fin inutilisée du TLAB de chaque thread est de la mémoire qu'aucun autre thread ne peut utiliser tant que ce TLAB n'a pas été restitué.

Dans OrionGC, le TLAB est l'`AllocContext` stocké dans chaque thread de l'ordonnanceur (un unique contexte statique en tient lieu avant que l'ordonnanceur ne tourne, ou lorsque celui-ci est exclu de la compilation), la taille par défaut d'un TLAB est de 8 Kio, et chaque collecte commence par restituer tous les TLAB afin que le balayage voie un tas cohérent. Voir [AllocContext](../garbage-collector.md#alloccontext-tlab), [Recharge du TLAB](../garbage-collector.md#tlab-refill) et [Restitution des TLAB](../garbage-collector.md#returning-tlabs).
