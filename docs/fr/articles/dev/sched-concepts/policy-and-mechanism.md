---
source_hash: 7ed792470c460d5d0f8fd2b7912aef8bd382f5c8
---
<a id="policy-and-mechanism"></a>

# Politique et mécanisme

Un ordonnanceur assure deux tâches différentes. Le mécanisme effectue le travail : il sauvegarde les registres du thread qui quitte le CPU et restaure ceux du thread qui arrive, tient la liste des threads existants, et fait passer chacun d'eux entre les états en cours d'exécution, en attente et prêt. La politique prend les décisions : quel thread prêt s'exécute ensuite, pendant combien de temps, et à quelle place un thread réveillé s'insère. Séparer les deux permet de changer la politique sans toucher au code qui doit être parfaitement juste pour que le noyau survive à un changement de contexte, et permet à un même mécanisme de servir une politique à partage équitable, une politique temps réel ou une simple rotation.

La frontière entre les deux est une interface. Le mécanisme appelle la politique à des points fixes, quand un thread est créé, devient prêt, se bloque, se termine ou épuise son temps, et lui demande le thread suivant quand il s'apprête à changer de thread. La politique répond à partir de ses propres données de suivi et n'effectue jamais elle-même le changement : tout ce qu'elle doit savoir lui parvient par ces appels, et tout ce qu'elle décide repart par leurs valeurs de retour.

Le piège, c'est que la frontière porte ses propres règles. Le mécanisme appelle la politique dans un ordre qu'il a choisi, dans un contexte qu'il a choisi, et s'appuie sur des invariants que la politique doit maintenir, comme ne jamais tenir deux fois le même thread dans sa file. Une politique qui prend la bonne décision au mauvais moment, ou qui garde une entrée en double, casse le mécanisme aussi sûrement qu'un bug dans le changement de contexte lui-même, et la panne se manifeste loin de la politique, sous la forme d'un thread qui ne s'exécute plus jamais.

Dans Cosmos, la politique est `IScheduler`, une interface calquée sur le trait `EkibenScheduler` d'Ekiben, une API d'ordonnanceurs interchangeables, et le mécanisme est `SchedulerManager`. Le gestionnaire tient le registre des threads, fixe chaque `SchedulerThread.State` (une politique lit l'état mais ne l'écrit jamais) et reçoit le tick du minuteur, et les stubs d'interruption sauvegardent et restaurent les registres ; la politique conserve ses données de suivi dans l'emplacement `SchedulerData` de chaque thread et de chaque CPU. La politique installée au démarrage est Stride, et un noyau en installe une autre à l'exécution avec `SchedulerManager.SetScheduler`, qui lui confie tous les threads vivants. Voir [Vue d'ensemble](../scheduler.md#overview) et [L'interface](../../user/scheduler-plugging.md#the-interface).
