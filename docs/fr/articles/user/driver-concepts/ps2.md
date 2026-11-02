---
source_hash: bad3900fae58ff07421f30661e3c880e287d1ea3
---
<a id="ps2-and-the-8042-controller"></a>

# PS/2 et le contrôleur 8042

PS/2 est l'interface clavier et souris des PC compatibles IBM. Chaque périphérique dialogue par une ligne série avec un contrôleur de la carte mère, le 8042, qui doit son nom au microcontrôleur Intel du PC/AT ; beaucoup de PC en fournissent encore un, parfois émulé par le firmware. Son premier port sert au clavier et son second, le port auxiliaire, à une souris ; le CPU l'atteint par deux [ports d'E/S](registers.md) : `0x60` transporte les octets de données dans les deux sens, et `0x64` est le registre d'état en lecture et reçoit les commandes du contrôleur en écriture. Le port clavier lève l'IRQ 1, le port souris l'IRQ 12.

Les commandes se situent à deux niveaux. Celles du contrôleur lui-même, écrites sur `0x64`, lisent et écrivent son octet de configuration, lancent son auto-test et testent ou activent chaque port. Les commandes d'un périphérique s'écrivent comme des données sur `0x60`, à destination du second port après un `0xD4` écrit sur `0x64` : réinitialiser, identifier, régler les voyants du clavier, activer le balayage. Le périphérique acquitte chaque octet de commande par `0xFA`, ou le redemande par `0xFE`, puis envoie sa réponse éventuelle ; une réinitialisation reçoit la réponse `0xAA` une fois que le périphérique a réussi son auto-test.

Une fois le balayage activé, le clavier envoie un code pour chaque appui de touche et un autre pour chaque relâchement, dans l'un des trois jeux de scancodes, et le contrôleur peut traduire le jeu 2 dans l'ancien jeu 1. Une souris, une fois qu'elle signale ses mouvements, envoie des paquets de mouvement de trois octets, quatre pour une souris à molette. Le piège est le port de données partagé : les acquittements et les réponses d'une commande arrivent sans étiquette au milieu du flux, si bien qu'un pilote qui envoie une commande pendant que l'utilisateur tape doit trier les octets qu'il lit.

Dans le kit de pilotes, `I8042Driver` possède le contrôleur : il le teste ainsi que ses deux ports, active la traduction pour que le port clavier délivre des codes du jeu 1, connecte l'IRQ 1 et l'IRQ 12 ou scrute à la place, et publie un nœud par port fonctionnel. Un pilote de classe dialogue à travers `Ps2Access`, dont `TryCommand` attend chaque `Acknowledge`, renvoie à chaque `Resend` jusqu'à `MaxResends` fois et recueille la réponse ; tout autre octet rejoint le flux que le gestionnaire d'interruption vide avec `TryReceive`. Voir [Périphériques PS/2](../drivers.md#ps2-devices) et [L'objet d'accès PS/2](../drivers.md#the-ps2-access-object).
