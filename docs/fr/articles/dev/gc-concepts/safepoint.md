---
source_hash: 8bfd475448048c47c4d4ddc44c203f5f2653fd2d
---
<a id="safepoint"></a>

# Point sûr

Un point sûr est un emplacement du code où les tables annexes du compilateur (GCInfo, dans NativeAOT) décrivent exactement l'état GC d'une méthode : quels registres et quels emplacements de pile contiennent des références vivantes à cet endroit. Entre deux points sûrs, les tables ne disent rien : un [parcours précis](conservative-vs-precise.md) n'est donc valide que pour un thread arrêté sur l'un d'eux.

Les sites d'appel sont les points sûrs qui comptent pour le parcours de la pile. Tant qu'une méthode attend le retour d'une méthode appelée, son cadre de pile reste figé sur l'instruction d'appel, si bien que chaque adresse de retour présente sur la pile d'un thread identifie un point sûr dans la méthode appelante. Un thread entré dans le ramasse-miettes par une chaîne d'appels peut ainsi être parcouru précisément, cadre par cadre, alors qu'un thread préempté sur une instruction quelconque ne le peut pas. Les runtimes comblent cet écart par le détournement d'adresse de retour : ils forcent le thread préempté à s'exécuter jusqu'à un point sûr, puis le parcourent.

Dans OrionGC, seul le thread qui déclenche la collecte est assuré de se trouver sur des points sûrs ; tous les autres threads sont parcourus de façon conservatrice tant que le détournement n'existe pas ([#385](https://github.com/valentinbreiz/nativeaot-patcher/issues/385)). Voir [La contrainte des points sûrs](../garbage-collector-gcinfo.md#the-safepoint-constraint).
