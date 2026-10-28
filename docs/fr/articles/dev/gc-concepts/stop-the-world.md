---
source_hash: 941f8268965c23151738f5b985704de799fb7e1b
---
# Stop-the-world

Un ramasse-miettes stop-the-world met en pause chaque mutateur (chaque thread qui exécute du code applicatif) pendant toute la durée d'une collecte. Rien n'alloue ni n'écrit de références pendant que le ramasse-miettes s'exécute : le graphe d'objets ne peut donc pas changer sous lui, et le ramasse-miettes n'a besoin d'aucune synchronisation avec le code en cours d'exécution.

La famille opposée, celle des ramasse-miettes concurrents et incrémentaux, laisse les threads continuer à s'exécuter pendant une partie de la collecte. Cela supprime la pause, mais exige une lourde mécanique : des barrières d'écriture qui signalent au ramasse-miettes les références modifiées en cours de parcours, et un ordre des opérations soigneusement établi pour qu'un objet vivant ne soit jamais libéré.

Dans OrionGC, chaque collecte est stop-the-world. Elle s'exécute avec les interruptions désactivées sur le thread qui l'a déclenchée, si bien qu'aucun changement de thread ni aucun gestionnaire d'interruption ne peut s'exécuter avant qu'elle se termine. Voir [Collecte](../garbage-collector.md#collection).
