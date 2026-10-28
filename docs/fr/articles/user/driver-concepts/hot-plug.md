---
source_hash: e4f00358274c512eb7b964024a809b9578bf3563
---
<a id="hot-plug"></a>

# Branchement à chaud

Le branchement à chaud signifie qu'un périphérique peut arriver ou partir pendant que la machine fonctionne. L'arrivée est la moitié la plus simple : le pilote de bus remarque le nouveau périphérique, le décrit comme il le ferait au démarrage, et le noyau le propose aux pilotes. Le retrait est plus difficile, car le pilote lié au périphérique peut être en train de s'en servir. Un retrait ordonné demande d'abord l'autorisation : sur un emplacement PCI Express, l'utilisateur appuie sur le bouton d'attention de l'emplacement, le système d'exploitation arrête le périphérique et coupe l'alimentation de l'emplacement, et ce n'est qu'ensuite que la carte est retirée. Un retrait inopiné ne demande rien, et la carte ou le câble a disparu avant que le moindre logiciel n'en soit averti.

Après un retrait inopiné, le piège est de toucher au périphérique. Une lecture sur un périphérique PCI qui n'est plus là renvoie tous les bits à un, valeur qu'un pilote peut prendre pour un registre d'état dont chaque bit est positionné, ou sur laquelle il peut attendre indéfiniment qu'un bit repasse à zéro. Une écriture peut provoquer une faute, ou atteindre le périphérique, quel qu'il soit, auquel la plage d'adresses est attribuée ensuite. Le démontage d'un pilote doit donc savoir si le matériel est toujours là avant de l'arrêter.

PCI Express ajoute un second problème. Le firmware n'a donné d'adresses qu'aux périphériques présents au démarrage, si bien qu'une fonction qui arrive plus tard a des BAR non attribués, qu'il faut placer dans les fenêtres d'adresses que relaie le port situé au-dessus de l'emplacement ([PCI](pci.md)).

[USB](usb.md) et [PS/2](ps2.md) se situent aux deux extrêmes. USB a été conçu pour le branchement à chaud : les hubs signalent chaque branchement et chaque débranchement, et l'hôte énumère chaque nouveau périphérique. PS/2 ne l'a pas été, et son clavier et sa souris ne sont détectés qu'une fois, au démarrage.

Dans le kit de pilotes, `OnDetach` reçoit une `DetachReason` dont `HardwarePresent` vaut `false` une fois le périphérique parti, et le pilote ne doit alors pas toucher à ses registres ([Démontage et OnDetach](../drivers.md#teardown-and-ondetach)). `PcieRootPortDriver` publie une fonction qui arrive dans un emplacement de branchement à chaud en plaçant ses BAR non attribués dans les fenêtres du port, et la retire quand elle est enlevée ([Emplacements de branchement à chaud](../drivers.md#hot-plug-slots)). Le `UsbAccess.IsDisconnected` d'une interface USB passe à vrai une fois son périphérique débranché ([L'objet d'accès USB](../drivers.md#the-usb-access-object)), tandis que les ports PS/2 ne sont publiés qu'au démarrage ([Les pilotes de classe PS/2](../drivers.md#the-ps2-class-drivers)).
