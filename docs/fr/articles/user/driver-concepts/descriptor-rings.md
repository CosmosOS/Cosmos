---
source_hash: 47cec1a57f60d5564b4778d8ffaf05c26081e4b4
---
<a id="descriptor-rings"></a>

# Anneaux de descripteurs

Un périphérique qui fait transiter un flux de paquets ou de blocs de disque ne peut pas recevoir un tampon à la fois : le pilote attendrait chaque transfert, et une rafale de trames entrantes ne trouverait nulle part où arriver. Un anneau de descripteurs résout les deux problèmes : c'est un tableau fixe de petits enregistrements, les descripteurs, placé en mémoire [DMA](dma.md), que le pilote et le périphérique lisent et écrivent tous les deux. Chaque descripteur contient l'[adresse physique](physical-addresses.md) et la longueur d'un tampon de données, ainsi que des bits d'état, et le tableau sert de file circulaire qui revient à la première entrée après la dernière.

À tout moment, chaque descripteur appartient à l'un des deux côtés. Le pilote remplit les descripteurs qu'il possède et les cède ; le périphérique traite les siens dans l'ordre et rend chacun une fois qu'il en a terminé. Deux conventions, souvent combinées, marquent la frontière. Dans la première, l'indice de queue est l'endroit où le pilote place le prochain descripteur, l'indice de tête celui où le périphérique prend le suivant, et les entrées de la tête à la queue appartiennent au périphérique. Dans la seconde, chaque descripteur porte un bit de propriété, ou bit « terminé », que bascule le côté qui le cède. Une carte réseau tient deux anneaux : un anneau de réception que le pilote approvisionne en tampons vides, et un anneau d'émission qu'il remplit de trames sortantes.

Le pilote signale au périphérique que de nouveaux descripteurs sont prêts en écrivant dans un registre, le doorbell, qui est souvent le registre de queue lui-même. Le retour n'a besoin d'aucun registre : le pilote lit quels descripteurs sont terminés dans l'état que le périphérique y écrit, si bien qu'une seule interruption, ou une seule scrutation, récupère tout un lot.

Le piège est l'ordre. Le périphérique peut lire un descripteur dès qu'il voit le doorbell ou le bit de propriété, si bien que le contenu du descripteur doit d'abord atteindre la mémoire, et le pilote ne doit pas lire la longueur ni les données d'un tampon avant le bit « terminé » qui les protège. Les [barrières](barriers.md) imposent l'un et l'autre.

Dans le kit de pilotes, un anneau réside dans un `DmaBuffer` obtenu par `binding.AllocateDma`, avec `DmaBuffer.WriteBarrier()` avant de céder un descripteur, `DmaBuffer.ReadBarrier()` après avoir lu un bit « terminé », et le doorbell écrit à travers une `RegisterWindow`, dont les écritures portent leur propre barrière. Les périphériques virtio utilisent l'anneau propre au kit, la `Virtqueue` ([virtio](virtio.md)) ; voir [Mémoire DMA](../drivers.md#dma-memory) et [Files](../drivers.md#queues).
