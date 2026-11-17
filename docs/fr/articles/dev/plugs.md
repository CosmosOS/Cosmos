---
source_hash: eae72e86c2918abf1159faad629ffe89dcd7bb51
---
# Plugs

Un plug remplace des membres d'un type existant au niveau IL : le patcher (`cosmos-patcher`, construit sur Mono.Cecil) réécrit l'assembly cible avant qu'ILC ne le compile, si bien que le code de la BCL et du runtime reçoit des implémentations propres au noyau sans aucune modification des sources. Le mécanisme, le pipeline du patcher et la place des plugs dans le build sont traités dans la documentation du pipeline de build ; cette page explique comment en écrire un.

---

<a id="attributes"></a>

## Attributs

Gen3 implémente trois attributs liés aux plugs, tous définis dans [Cosmos.Build.API](https://github.com/CosmosOS/Cosmos/tree/gen3/src/Cosmos.Build.API/Attributes) :

| Attribut | Appliqué à | Effet |
|-----------|------------|--------|
| `[Plug]` | classe | Désigne la classe comme source de remplacement pour un type cible |
| `[PlugMember]` | méthode, propriété, champ | Remplace le membre correspondant du type cible |
| `[PlatformSpecific]` | classe ou membre | Ne conserve l'élément que lors d'un patch pour les architectures indiquées |

### `[Plug]`

Nomme le type cible, soit sous la forme `typeof(...)` lorsque le type est accessible publiquement, soit sous la forme d'une chaîne contenant le nom qualifié complet pour les types internes (`[Plug("Internal.Runtime.CompilerHelpers.StartupCodeHelpers")]`). `IsOptional = true` ignore le plug en silence lorsque le type cible n'existe pas dans la compilation ; un `TargetName` mal orthographié sur un plug non optionnel fait échouer le build. Voir [PlugAttribute.cs](https://github.com/CosmosOS/Cosmos/blob/gen3/src/Cosmos.Build.API/Attributes/PlugAttribute.cs).

### `[PlugMember]`

Remplace le membre cible dont le nom et la signature correspondent à ceux du membre du plug. Un argument de type chaîne prend le pas sur la correspondance par nom, et c'est ainsi que l'on désigne les constructeurs et les accesseurs de propriété : `[PlugMember(".ctor")]`, `[PlugMember("get_Address")]`. Les signatures doivent correspondre exactement à celle du membre cible, à un ajout près pour les membres d'instance, décrit plus bas ; en cas de différence, le patcher ne peut pas raccorder le membre. Voir [PlugMemberAttribute.cs](https://github.com/CosmosOS/Cosmos/blob/gen3/src/Cosmos.Build.API/Attributes/PlugMemberAttribute.cs).

### `[PlatformSpecific]`

Restreint une classe ou un membre de plug à certaines architectures au moment du patch : `[PlatformSpecific(PlatformArchitecture.X64)]`. Voir [PlatformSpecificAttribute.cs](https://github.com/CosmosOS/Cosmos/blob/gen3/src/Cosmos.Build.API/Attributes/PlatformSpecificAttribute.cs).

---

<a id="the-athis-convention"></a>

## La convention `aThis`

Les classes de plug sont statiques : un membre d'instance est donc remplacé par une méthode statique dont le premier paramètre est l'instance cible, nommé par convention `aThis`. Le patcher redirige les références à `this` vers ce paramètre :

```csharp
using System.Net;
using Cosmos.Build.API.Attributes;

namespace Cosmos.Kernel.Plugs.System.Net;

[Plug(typeof(IPEndPoint))]
public static class IPEndPointPlug
{
    [PlugMember(".ctor")]
    public static void Ctor(IPEndPoint aThis, IPAddress address, int port)
    {
        // runs in place of the target constructor
    }

    [PlugMember("get_Address")]
    public static IPAddress? get_Address(IPEndPoint aThis)
    {
        // runs in place of the property getter
        return null;
    }

    [PlugMember]
    public static long Seek(IPEndPoint aThis, long offset)
    {
        // instance method: name and remaining signature match the target
        return 0;
    }

    [PlugMember]
    public static bool StaticMethod()
    {
        // static member: no aThis
        return true;
    }
}
```

---

<a id="accessing-private-and-internal-state"></a>

## Accéder à l'état privé et interne

Les attributs `[Expose]` (injecter de nouveaux membres privés dans le type cible) et `[FieldAccess]` (lier un paramètre d'une méthode de plug à un champ privé de la cible) de Gen2 ne sont pas implémentés dans le patcher de gen3 ; leur portage est suivi dans [#458](https://github.com/valentinbreiz/nativeaot-patcher/issues/458). D'ici là, un plug dispose de deux options :

- **Stockage annexe.** Conservez l'état propre à chaque instance dans la classe de plug elle-même, indexé par l'instance ([IPEndPointPlug.cs](https://github.com/CosmosOS/Cosmos/blob/gen3/src/Cosmos.Kernel.Plugs/System/Net/IPEndPointPlug.cs) le fait avec des dictionnaires). Cela remplace entièrement l'état de la cible au lieu de le lire : chaque membre qui touche à cet état doit donc lui aussi être remplacé par un plug.
- **`[UnsafeAccessor]` / `[UnsafeAccessorType]`.** Le mécanisme d'accesseurs du runtime atteint directement les membres privés et internes. C'est l'échappatoire non supportée de la [politique d'API publique](public-api.md) ; voir [Accéder aux éléments internes](accessing-internals.md) pour ce qu'il permet et ne permet pas (notamment : il ne peut pas renvoyer par référence un champ dont le type est lui-même inaccessible).

---

<a id="pitfalls"></a>

## Pièges

- Un plug dont le `TargetName` ne se résout pas fait échouer le build, sauf si `IsOptional` est défini ; un plug optionnel qui cesse en silence de correspondre continue de compiler pendant que la cible s'exécute sans patch, c'est pourquoi il vaut mieux utiliser des plugs non optionnels pour tout ce dont dépend le bon fonctionnement.
- Une classe `[Plug]` dont les membres n'ont pas `[PlugMember]` ne patche rien, et aucune erreur n'est levée : la classe compile, la cible s'exécute sans patch. Cela a déjà provoqué de véritables échecs silencieux (un `NativeMemory` sans plug est parti dans une récursion jusqu'à une triple faute) : vérifiez donc que l'attribut est présent sur chaque membre censé remplacer quelque chose.
- Une différence de signature (y compris un paramètre `aThis` manquant ou de mauvais type) signifie que le membre n'est pas raccordé.
