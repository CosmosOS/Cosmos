---
source_hash: 7de2922808c9c174261bed64f942bd6026aa8967
---
<a id="runtime-object-header"></a>

# En-tête d'objet du runtime

En .NET, chaque objet porte un mot d'en-tête juste avant la référence d'objet, à un décalage négatif, en plus du pointeur `MethodTable` au décalage 0. Le runtime y range des informations de gestion qui n'ont pas de champ propre : le code de hachage d'identité mis en cache (ce que `GetHashCode` renvoie pour un objet ordinaire) et le verrou léger que `Monitor.Enter` (l'instruction `lock`) prend sur un objet sans contention.

Le ramasse-miettes ne lit jamais le code de hachage ni le verrou, mais il doit laisser de la place pour ce mot : le runtime écrit à `objRef - 4`, il faut donc des octets accessibles en écriture avant chaque objet, y compris le tout premier objet d'un segment et tout objet qui suit directement un espace libre recyclé. CoreLib réserve les trois bits de poids fort du mot au ramasse-miettes, et ses boucles de compare-exchange les laissent tels quels ; OrionGC en utilise un, le bit 30 (le `BIT_SBLK_FINALIZER_RUN` de .NET), comme indicateur que positionne `GC.SuppressFinalize`, voir [Finalisation](../garbage-collector.md#finalization).

Dans OrionGC, c'est pourquoi chaque segment réserve 8 octets avant son premier objet, et pourquoi chaque bloc libre garde ses 8 derniers octets hors de l'allocation. Cet en-tête est distinct de la [disposition GCObject](../garbage-collector.md#object-header), qui commence à la référence d'objet elle-même. Voir [Segments](../garbage-collector.md#segments) et [Listes de blocs libres](../garbage-collector.md#free-lists).
