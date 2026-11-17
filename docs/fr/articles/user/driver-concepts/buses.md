---
source_hash: 03900f459ed7ef8145a698d1c5c003048694709c
---
<a id="buses-and-enumeration"></a>

# Bus et énumération

Un bus relie des périphériques au processeur, et l'énumération est la façon dont le logiciel découvre ce qui s'y trouve. PCI et USB peuvent être énumérés parce que chaque périphérique répond à un endroit prévisible avec une description standard de lui-même : le logiciel lit l'identifiant de fabricant à chaque adresse PCI, où un emplacement vide se lit avec tous les bits à un ([PCI](pci.md)), et demande ses descripteurs à chaque nouveau périphérique [USB](usb.md).

D'autres périphériques n'offrent aucun endroit de ce genre où les interroger. Le contrôleur [PS/2](ps2.md) d'un PC se trouve à des ports d'E/S fixes, les périphériques d'une machine ARM à des adresses mémoire fixes, et le pont hôte PCI, d'où part tout balayage PCI, en fait partie. Lire une adresse où rien ne répond peut provoquer une faute, et un périphérique qui répond n'a aucun moyen standard de dire ce qu'il est : ces périphériques de plateforme doivent donc être décrits au noyau. Sur les PC et les serveurs ARM, ce sont les tables ACPI du firmware qui les décrivent. Sur de nombreuses cartes ARM, c'est un device tree : une structure que le chargeur d'amorçage transmet, dont les nœuds listent les registres, les interruptions et les chaînes compatibles de chaque périphérique, c'est-à-dire les noms des modèles de programmation qu'il suit. Une machine à la disposition fixe, comme la machine virt de QEMU, peut aussi être décrite par une table dans le noyau.

Les deux sortes de périphériques se combinent en un arbre. La description fournit les racines, et le pilote d'une racine qui est un contrôleur de bus énumère son bus et publie ce qu'il y trouve comme enfants, auxquels d'autres pilotes se lieront : un hôte PCI donne des fonctions PCI, dont l'une peut être un contrôleur USB qui donne des périphériques USB.

Le piège est que rien ne vient confirmer une description. Un périphérique qu'elle omet est invisible, et un périphérique qu'elle liste mais qui est absent laisse un pilote sonder des adresses vides.

Dans le kit de pilotes, les nœuds de plateforme sont publiés au démarrage par la description de la machine : sur x64, la disposition fixe d'un PC avec le contrôleur 8042 (`pnp0303`) et l'hôte PCI (`pci-host-legacy`) ; sur ARM64, l'hôte PCI issu d'ACPI ou du device tree, et chaque emplacement `virtio,mmio` occupé, issu du device tree ou de la disposition fixe de la machine virt ([Nœuds de plateforme](../drivers.md#platform-nodes)). Les pilotes les mettent en correspondance avec `PlatformMatch.Compatible`, et un pilote de bus ajoute des enfants avec `binding.PublishChild`, si bien que les `Parent` et `Children` de chaque nœud forment l'arbre ([Publier des nœuds enfants](../drivers.md#publishing-child-nodes), [Bus](../drivers.md#buses)).
