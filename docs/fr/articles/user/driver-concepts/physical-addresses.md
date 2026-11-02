---
source_hash: 46819728bf1b3c52d76bac9d0f3b5bc400cea56c
---
<a id="physical-addresses"></a>

# Adresses physiques

Le code qui s'exécute sur un CPU où la pagination est active ne désigne jamais la mémoire directement. Chaque pointeur qu'il manipule est une adresse virtuelle, que l'unité de gestion mémoire traduit à travers les tables de pages en une adresse physique, c'est-à-dire l'emplacement que transporte réellement le bus mémoire, dans la RAM ou dans l'espace de registres d'un périphérique. La traduction se fait page par page, généralement 4 Kio à la fois, et des pages virtuelles voisines ne correspondent pas forcément à des pages physiques voisines.

Un périphérique se trouve en dehors de cette traduction. Quand il lit ou écrit la mémoire par [DMA](dma.md), il place une adresse sur le bus sans aucune table de pages à consulter ; un pilote lui donne donc l'adresse physique d'un tampon, jamais le pointeur qu'utilise son propre code. Une IOMMU peut offrir aux périphériques une traduction qui leur est propre ; sans elle, l'adresse vue par le périphérique est l'adresse physique. La séparation joue aussi dans l'autre sens : les [registres](registers.md) d'un périphérique se trouvent à une adresse physique, que le noyau mappe dans l'espace d'adressage virtuel avant que le CPU puisse les atteindre.

Deux contraintes en découlent. Un tampon contigu en mémoire virtuelle peut être dispersé sur plusieurs pages physiques, alors qu'un périphérique à qui l'on demande de transférer N octets depuis une adresse lit N octets physiquement consécutifs ; un pilote alloue donc de la mémoire physiquement contiguë, ou décrit un tampon dispersé sous la forme d'une liste de fragments. Les périphériques imposent aussi un alignement : un registre qui contient une adresse de base ignore souvent ses bits de poids faible, et certaines structures ne doivent pas franchir une frontière de page.

Le piège est que les deux sortes d'adresses sont de simples entiers. Rien n'empêche un pilote de remettre une adresse virtuelle à un périphérique, et celui-ci lit ou écrit alors un emplacement physique sans rapport, sans qu'aucune faute ne signale où cela se produit.

Dans le kit de pilotes, un `DmaBuffer` obtenu par `binding.AllocateDma(length, alignment)` fournit les deux vues à la fois : il est physiquement contigu, commence au moins sur une frontière de 4 Kio, et porte `Span` pour le CPU et `PhysicalAddress` pour le périphérique. Les fenêtres mémoire d'un nœud (`DeviceResource.PhysicalBase`, et `PciBar.Base` pour un BAR mémoire) sont des adresses physiques que `MapRegisters` et `MapRegion` rendent accessibles, et le noyau ne configure aucune IOMMU : c'est donc une adresse physique qu'utilise un périphérique. Voir [Mémoire DMA](../drivers.md#dma-memory) et [Ressources BAR](../drivers.md#bar-resources).
