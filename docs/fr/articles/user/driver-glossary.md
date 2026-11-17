---
source_hash: 1deddb819ca1cc5ef80b26e2c096f38584d59efc
---
<a id="driver-glossary"></a>

# Glossaire des pilotes

Ces notes donnent le contexte sur les concepts de matériel, de système d'exploitation et de build sur lesquels s'appuient [Écrire un pilote](drivers.md) et la page contributeur consacrée au [Manifeste des pilotes](../dev/build/driver-manifest.md), pour un lecteur qui n'a jamais écrit de pilote. Chaque page explique d'abord le concept en lui-même, puis se termine par la façon dont Cosmos l'applique. Le tableau les regroupe en quatre parties : comment un pilote atteint un périphérique, comment le périphérique et le pilote se signalent l'un à l'autre, les bus, puis comment un noyau en vient à embarquer un pilote et en choisit un pour chaque périphérique. Le contexte d'interruption s'appuie aussi sur deux notes d'ordonnancement de la documentation contributeur, la [préemption](../dev/sched-concepts/preemption.md) et la [frontière d'instruction](../dev/sched-concepts/instruction-boundary.md).

| Concept | Résumé |
|---------|---------|
| [MMIO et E/S de port](driver-concepts/registers.md) | Atteindre les registres d'un périphérique par la mémoire ou par des ports, et la façon dont les mappages les mettent en cache |
| [Adresses physiques](driver-concepts/physical-addresses.md) | Adresses virtuelles face aux adresses physiques, contiguïté physique, et l'alignement qu'exigent les périphériques |
| [DMA](driver-concepts/dma.md) | Des périphériques qui lisent et écrivent eux-mêmes la mémoire : bus mastering, cohérence, limites à 32 bits |
| [Barrières mémoire](driver-concepts/barriers.md) | Ordonner les lectures et les écritures mémoire du CPU pour qu'un périphérique les voie dans l'ordre |
| [Anneaux de descripteurs](driver-concepts/descriptor-rings.md) | Des tableaux circulaires de descripteurs partagés avec un périphérique : propriété, tête et queue, doorbells |
| [Acheminement des interruptions](driver-concepts/interrupt-delivery.md) | Les IRQ, l'acquittement du périphérique, le déclenchement sur front et sur niveau, INTx face à MSI-X |
| [Contexte d'interruption](driver-concepts/interrupt-context.md) | Pourquoi un gestionnaire d'interruption ne peut pas attendre, pourquoi celui d'un pilote ne doit pas allouer, et comment son travail est découpé |
| [Bus et énumération](driver-concepts/buses.md) | Les bus énumérables, les périphériques de plateforme décrits par le firmware, et l'arbre des nœuds |
| [Passage de relais du firmware](driver-concepts/firmware-handoff.md) | Ce que l'UEFI configure avant que le noyau ne s'exécute, et ce que Cosmos en conserve |
| [PCI](driver-concepts/pci.md) | La fonction PCI, son espace de configuration, les codes de classe, les capacités et les BAR |
| [Branchement à chaud](driver-concepts/hot-plug.md) | Des périphériques qui arrivent et repartent pendant l'exécution, et le retrait inopiné |
| [Virtio](driver-concepts/virtio.md) | Les périphériques paravirtuels : les transports, l'échange d'état, la négociation des fonctionnalités et les virtqueues |
| [USB](driver-concepts/usb.md) | Interfaces, endpoints, types de transfert, endpoints interrupt scrutés, énumération, les trois interfaces de contrôleur hôte avec leurs compagnons et leurs hubs, le protocole boot HID |
| [PS/2 et le contrôleur 8042](driver-concepts/ps2.md) | Le contrôleur 8042, ses deux ports, les commandes, les acquittements et le balayage |
| [Générateurs de source](driver-concepts/source-generators.md) | Des plug-ins du compilateur qui ajoutent du code au moment du build, à la place de la réflexion, et la façon dont les propriétés de build leur parviennent |
| [Trimming et commutateurs de fonctionnalité](driver-concepts/trimming.md) | Laisser le code inaccessible hors de l'image, et les commutateurs fixés au build qui rendent une branche inaccessible |
| [Correspondance des pilotes](driver-concepts/driver-matching.md) | Tables de correspondances, spécificité et rang, et une dernière clé déterministe quand plusieurs pilotes conviennent à un même périphérique |
