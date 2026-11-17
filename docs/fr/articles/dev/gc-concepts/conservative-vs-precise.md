---
source_hash: 794caf74170b2baa9f029dc06d81ff6193933c63
---
<a id="precise-vs-conservative-scanning"></a>

# Parcours précis et parcours conservateur

Un parcours de pile doit décider quels mots de la pile d'un thread sont des références d'objet. Il y a deux façons de le faire.

Un parcours précis sait. Le compilateur émet des tables (GCInfo, dans NativeAOT) qui décrivent, pour chaque [point sûr](safepoint.md) de chaque méthode, exactement quels emplacements de pile et quels registres contiennent des références vivantes. Le ramasse-miettes signale ces emplacements et rien d'autre. Un parcours précis ne prend jamais un entier pour un pointeur, et c'est lui qui rend possibles les ramasse-miettes déplaçants, mais il ne fonctionne qu'aux emplacements du code que décrivent les tables.

Un parcours conservateur devine. Chaque mot de la pile de la taille d'un pointeur est traité comme une référence potentielle, et tout ce qui pointe par hasard dans le tas maintient sa cible en vie. Cela fonctionne sur n'importe quelle instruction, sans aucune aide du compilateur, mais avec deux coûts : un entier qui ressemble à une adresse du tas retient des objets morts (une fausse racine), et aucun objet référencé de façon conservatrice ne peut jamais être déplacé, puisque le mot ne peut pas être réécrit sans danger.

Concrètement, voici la pile d'un thread parcourue 8 octets à la fois :

| Mot de pile | Ce qu'il est réellement | Verdict conservateur |
|------------|--------------------|--------------------|
| `0x0000000000000007` | un int de valeur 7 | Hors de la plage du tas, ignoré |
| `0xFFFF8001A2B3C4D0` | une référence vivante vers une `List<int>` | Dans la plage du tas, marqué ✅ correct |
| `0xFFFF8001DEADBEEF` | un emplacement de débordement mort | Toujours dans la plage, marqué ❌ fausse racine |
| `0xFFFFFFFF80123400` | une adresse de retour dans le code du noyau | Adresse du noyau, mais hors des segments du tas, ignorée |
| `0xFFFF8001CAFE0000` | un pointeur d'appelé périmé | Toujours dans la plage, marqué ❌ fausse racine |

OrionGC utilise les deux. Le thread qui a déclenché la collecte se trouve sur une chaîne de sites d'appel managés où le GCInfo est valide, il est donc parcouru précisément ; tous les autres threads ont été préemptés sur une instruction arbitraire et sont parcourus de façon conservatrice. Voir [Parcours précis de la pile (GCInfo)](../garbage-collector-gcinfo.md).
