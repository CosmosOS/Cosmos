---
source_hash: 3e852f55a03fe1006569a40566e73a0da96812ee
---
<a id="scheduler-glossary"></a>

# Glossaire de l'ordonnanceur

Ces notes donnent le contexte sur les concepts d'ordonnancement sur lesquels s'appuient l'article contributeur consacré à l'[Ordonnanceur](scheduler.md) et le guide [Écrire un ordonnanceur](../user/scheduler-plugging.md), pour un lecteur qui n'a jamais écrit d'ordonnanceur. Chaque page explique d'abord le concept en lui-même, puis se termine par la façon dont Cosmos l'applique. Le tableau les regroupe en trois parties : comment un thread obtient le CPU et le perd, ce que doit respecter le code qui s'exécute en concurrence avec le tick, puis les propriétés à l'aune desquelles on juge une politique. Les règles applicables au code des hooks s'appuient aussi sur une note du glossaire des pilotes, le [contexte d'interruption](../user/driver-concepts/interrupt-context.md).

| Concept | Résumé |
|---------|---------|
| [Politique et mécanisme](sched-concepts/policy-and-mechanism.md) | Décider quel thread s'exécute, à l'écart du code qui bascule d'un thread à l'autre |
| [États des threads et file d'exécution](sched-concepts/run-queue.md) | Les threads en cours d'exécution, prêts et bloqués, la structure dans laquelle attendent les threads prêts, et le thread inactif |
| [Préemption](sched-concepts/preemption.md) | Le noyau reprend le CPU sur une interruption ; les threads n'ont jamais à le céder d'eux-mêmes |
| [Le tick et le quantum](sched-concepts/scheduler-tick.md) | L'interruption du minuteur qui donne la main à l'ordonnanceur, la tranche de temps, et la comptabilité par échantillonnage |
| [Changement de contexte](sched-concepts/context-switch.md) | Sauvegarder les registres d'un thread et restaurer ceux d'un autre, et le moment où le changement a réellement lieu |
| [Interruptions aux frontières d'instruction](sched-concepts/instruction-boundary.md) | Une interruption tombe après une instruction et avant la suivante ; rien de plus long n'est atomique face à elle |
| [Spinlocks et masquage des interruptions](sched-concepts/spinlocks-and-masking.md) | Pourquoi un verrou ne tient pas à l'écart un gestionnaire d'interruption sur le même CPU, et ce qui le fait |
| [Partage équitable par temps virtuel](sched-concepts/virtual-time-fair-share.md) | Des parts de CPU pondérées, imposées en exécutant toujours le thread dont l'horloge virtuelle est la plus en retard |
| [Famine et vieillissement](sched-concepts/starvation.md) | Des threads prêts qui ne s'exécutent jamais, et les protections contre ce problème |
| [Inversion et héritage de priorité](sched-concepts/priority-inversion.md) | Un thread de haute priorité bloqué derrière un détenteur de verrou qu'un thread de priorité moyenne préempte |
| [Ordonnancement temps réel](sched-concepts/real-time-scheduling.md) | Échéances, Rate Monotonic et EDF, bornes d'utilisation et contrôle d'admission |
| [Affinité CPU et répartition de charge](sched-concepts/cpu-affinity.md) | Files d'exécution par CPU, placement, migration et threads épinglés |
