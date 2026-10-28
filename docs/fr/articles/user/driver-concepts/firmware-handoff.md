---
source_hash: 4f29db838034cda1a53981710d5f17662167d6b6
---
<a id="firmware-handoff"></a>

# Passage de relais du firmware

Le noyau n'est pas le premier logiciel à s'exécuter sur la machine. Le firmware UEFI s'exécute avant le chargeur d'amorçage, et au moment où il passe la main, il a configuré une grande partie du matériel pour son propre usage. Il a énuméré PCI, donné aux ponts leurs numéros de bus et une adresse aux BAR de chaque fonction ([PCI](pci.md)), et activé les périphériques depuis lesquels il a démarré. Par son Graphics Output Protocol, il a défini un mode d'affichage et laissé à l'écran un framebuffer linéaire, dont le chargeur d'amorçage transmet au noyau l'adresse, la taille et la disposition des pixels. Les tables qui décrivent ce qui ne peut pas être découvert parviennent au noyau de la même façon : ACPI sur les PC et les serveurs ARM, un device tree sur de nombreuses cartes ARM ([bus](buses.md)).

Un noyau peut refaire ce travail ou s'appuyer dessus. S'appuyer dessus épargne du travail, le mode d'affichage avant tout : définir un mode, c'est programmer les timings de l'affichage et établir le lien avec le moniteur, ce que le firmware a déjà fait. Le prix à payer est que le noyau hérite de périphériques qui peuvent être encore actifs, avec le bus mastering activé et des transferts DMA en cours, ou d'un contrôleur USB dont le firmware se sert encore pour émuler un clavier PS/2.

Le piège est le démarrage hérité. Avec son CSM (module de support de compatibilité), le firmware UEFI émule un ancien BIOS de PC, et l'affichage reste en mode VGA au lieu d'un framebuffer linéaire, si bien qu'un pilote qui attend la surface du firmware n'en trouve aucune.

Dans le kit de pilotes, le pilote de l'hôte PCI s'appuie sur ce que le firmware a laissé : il conserve les numéros de bus et l'attribution des BAR du firmware plutôt que de les refaire. Le kit désactive le bus mastering et les interruptions d'une fonction avant le premier sondage, afin qu'aucun pilote ne voie un périphérique que le firmware a laissé en marche, et rétablit les réglages du firmware quand aucun pilote ne se lie à cette fonction ([L'objet d'accès PCI](../drivers.md#the-pci-access-object)). Le framebuffer du chargeur d'amorçage est publié comme un écran nommé `framebuffer`, et retiré quand un pilote se lie à la fonction PCI dont la mémoire le contient ([Publier un périphérique](../drivers.md#publishing-a-device)). `AmdDcnDriver` et `IntelGraphicsDriver` conservent le mode UEFI, déclinent après un démarrage CSM, et remettent la surface du firmware à l'écran dans `OnDetach` ([Les pilotes d'écran](../drivers.md#the-display-drivers)).
