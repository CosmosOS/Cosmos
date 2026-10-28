---
source_hash: b2df6833e632242a20abaaf6e64a84fd10ad53c3
---
<a id="accessing-internals"></a>

# Accéder aux éléments internes

Tout ce qui se trouve en dehors de la surface supportée décrite dans [Suivi de l'API publique](public-api.md) est `internal` par conception : la visibilité n'est pas le mécanisme d'extension, ce sont les points d'extension qui le sont. Lorsqu'aucun point d'extension ne couvre un besoin, `[UnsafeAccessor]` et `[UnsafeAccessorType]` atteignent directement l'état interne, sans octroi `InternalsVisibleTo`. Cette échappatoire est délibérément disponible et délibérément non supportée : les éléments internes peuvent changer dans n'importe quelle version sans préavis, et il faut s'attendre à ce que le code qui les utilise casse.

---

<a id="the-mechanism"></a>

## Le mécanisme

`[UnsafeAccessor]` (`System.Runtime.CompilerServices`) déclare une méthode `extern` que le runtime lie à un membre autrement inaccessible ; `[UnsafeAccessorType]` étend ce principe aux types eux-mêmes inaccessibles, désignés par leur nom qualifié d'assembly. Les deux sont résolus statiquement par ILC, si bien qu'ils sont compatibles avec l'AOT, contrairement à la réflexion, que le noyau ne peut pas utiliser.

Accéder à un membre statique interne d'un type interne :

```csharp
using System.Runtime.CompilerServices;

internal static class CoreAccessors
{
    // Binds to Cosmos.Kernel.Core.Memory.Heap.Heap.Collect(). The first
    // parameter carries the target type by name and is passed as null for
    // static members.
    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "Collect")]
    internal static extern int HeapCollect(
        [UnsafeAccessorType("Cosmos.Kernel.Core.Memory.Heap.Heap, Cosmos.Kernel.Core")] object? heap);
}

int freed = CoreAccessors.HeapCollect(null);
```

Accéder à un champ d'instance privé d'un type accessible :

```csharp
[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_name")]
private static extern ref string GetName(Partition partition);
```

`UnsafeAccessorKind` couvre les méthodes, les méthodes statiques, les constructeurs et les champs ; les accesseurs de propriété sont désignés comme des méthodes nommées `get_X`/`set_X`.

---

<a id="limits"></a>

## Limites

- **Les retours par référence de types inaccessibles lèvent une exception.** Un accesseur ne peut pas renvoyer par `ref` un champ dont le type lui-même est inaccessible à l'assembly qui le déclare ; le runtime le rejette, conformément à la spécification. C'est pourquoi `Thread.CreateThread` n'est pas remplacé par un plug : le corps d'origine lit le champ privé `StartHelper`, dont le type ne peut pas être renvoyé par référence, si bien que le point d'extension se place plutôt en dessous (voir `SystemNative_CreateThread` dans [libSystemNative.cs](https://github.com/CosmosOS/Cosmos/blob/gen3/src/Cosmos.Kernel.Core/Bridge/Interop/libSystemNative.cs)).
- **Les échecs de résolution apparaissent au site d'appel**, et non sous la forme d'une erreur de compilation : un membre cible renommé ou supprimé transforme l'accesseur en un stub qui lève une exception.
- **Les signatures doivent correspondre exactement**, y compris les modificateurs personnalisés sur les rares membres qui en portent.

---

<a id="when-to-use-it"></a>

## Quand l'utiliser

Par ordre de préférence :

1. La surface supportée (`Cosmos.Kernel.System` ainsi que les interfaces de contrat). S'il lui manque quelque chose dont un noyau a légitimement besoin, ouvrez une issue : étendre l'anneau est la correction prévue.
2. Un point d'extension `[Experimental]` lorsqu'il en existe un (le point d'extension de l'ordonnanceur, [Écrire un ordonnanceur](../user/scheduler-plugging.md) ; le kit de pilotes, [Écrire un pilote](../user/drivers.md)).
3. Un [plug](plugs.md), lorsque le but est de remplacer un comportement plutôt que d'atteindre un état.
4. `[UnsafeAccessor]`/`[UnsafeAccessorType]`, en acceptant que n'importe quelle version puisse le casser.
