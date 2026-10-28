---
source_hash: 00a930312099c5d4ee38e3a38d08d3ea4d403b37
---
<a id="memory-barriers"></a>

# Barrières mémoire

Les lectures et les écritures d'un programme n'atteignent pas forcément la mémoire dans l'ordre où le code source les énumère. Le compilateur peut réordonner des accès qu'il juge indépendants, et le CPU peut rendre une écriture postérieure visible avant une écriture antérieure, ou effectuer une lecture postérieure avant une lecture antérieure, tant que le thread en cours d'exécution ne peut pas s'en apercevoir. Un périphérique, lui, s'en aperçoit : il lit et écrit la mémoire [DMA](dma.md) de lui-même, et ne voit que l'ordre dans lequel les écritures du CPU sont devenues visibles.

La remise d'un [descripteur](descriptor-rings.md) illustre le problème. Le pilote renseigne une adresse et une longueur, puis positionne le drapeau qui confie le descripteur au périphérique ; si le drapeau devient visible en premier, le périphérique agit sur l'ancienne longueur. La complétion est le cas symétrique : le pilote lit un drapeau que le périphérique a positionné, puis les données que ce drapeau déclare prêtes, et un CPU qui a chargé les données en avance renvoie ce qui s'y trouvait avant l'écriture du périphérique. Une barrière d'écriture entre le descripteur et le drapeau rend chaque écriture antérieure visible avant toute écriture postérieure. Une barrière de lecture entre le drapeau et les données empêche les lectures postérieures de passer devant elle. Toutes deux empêchent aussi le compilateur de déplacer des accès d'un côté à l'autre.

Le piège, c'est que x64 conserve de lui-même l'ordre des écritures entre elles et celui des lectures entre elles, si bien qu'une barrière manquante n'y fait aucun mal. L'oubli se révèle sur ARM64, dont le modèle mémoire autorise le réordonnancement des unes comme des autres, et un pilote testé uniquement sur x64 peut porter ce bug longtemps.

Dans le kit de pilotes, `DmaBuffer.WriteBarrier()` et `DmaBuffer.ReadBarrier()` sont les deux barrières : de véritables instructions de barrière sur ARM64, et aucune instruction sur x64, bien qu'elles empêchent toujours le compilateur de déplacer des accès d'un côté à l'autre. Chaque écriture dans une `RegisterWindow` est précédée d'une barrière d'écriture et chaque lecture est suivie d'une barrière de lecture, si bien que l'écriture du doorbell après un descripteur rempli n'a besoin d'aucune barrière propre, alors qu'une `DeviceRegion` n'en porte aucune et que le pilote les place lui-même. Voir [Mémoire DMA](../drivers.md#dma-memory) et [Fenêtres de registres](../drivers.md#register-windows).
