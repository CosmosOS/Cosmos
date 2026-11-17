---
source_hash: e8136c959f988b79227caa1f4d9b32c923eb1235
---
<a id="generations"></a>

# Générations

Les ramasse-miettes générationnels découpent le tas selon l'âge des objets. L'hypothèse générationnelle constate que la plupart des objets meurent jeunes : collecter uniquement la région la plus jeune (la génération 0) récupère donc l'essentiel des objets morts pour une fraction du coût d'un parcours du tas entier ; les générations plus anciennes sont collectées rarement. La génération la plus haute que couvre une collecte donnée s'appelle la génération condamnée.

Cette optimisation demande toute une mécanique. Le ramasse-miettes doit trouver les références des objets anciens vers la jeune génération sans parcourir l'ancienne : chaque écriture de référence passe donc par une barrière d'écriture, un petit morceau de code qui consigne les écritures d'un objet ancien vers un objet jeune dans une structure annexe, ensemble mémorisé (remembered set) ou table de cartes (card table), que le ramasse-miettes consulte lors des collectes de la jeune génération.

Un ramasse-miettes à génération unique parcourt le tas entier à chaque collecte. Chaque collecte coûte plus cher, mais il n'y a ni barrière d'écriture ni ensemble mémorisé, et les écritures de références sont de simples écritures en mémoire.

OrionGC n'a qu'une seule génération et signale chaque objet du tas comme appartenant à la génération 0. Voir [Statistiques et informations mémoire](../garbage-collector.md#statistics-and-memory-info).
