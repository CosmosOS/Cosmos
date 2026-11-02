---
source_hash: 7a8ea5072103da736273432b7ddc44efa50597cc
---
<a id="random-numbers"></a>

# Nombres aléatoires

Dans cet article, nous verrons d'où viennent les nombres aléatoires sur Cosmos Gen3 : quelles API .NET sont cryptographiquement sûres, ce qui les alimente, et l'avertissement qu'un noyau affiche lorsque la machine n'offre aucune véritable entropie.

Si vous trouvez des bugs ou quelque chose d'anormal, merci de [soumettre une issue](https://github.com/CosmosOS/Cosmos/issues/new/choose) sur notre dépôt.

---

<a id="which-api-to-use"></a>

## Quelle API utiliser

Utilisez les API .NET standard ; rien de spécifique à Cosmos n'est nécessaire.

| API | Alimentée par | À utiliser pour |
|---|---|---|
| `System.Security.Cryptography.RandomNumberGenerator` (`Fill`, `GetBytes`, `GetInt32`, `GetNonZeroBytes`, `Create()`) | Le CSPRNG du noyau | Clés, nonces, jetons, tout ce qui est secret |
| `Guid.NewGuid()` | Le CSPRNG du noyau | Identifiants uniques |
| `System.Random`, `Random.Shared`, `HashCode` et les graines du hachage des chaînes | Les générateurs propres à CoreLib, dont la graine provient d'un xorshift sur le compteur de cycles | Jeux, mélanges, tout ce qui n'est pas secret |

Les bibliothèques qui s'appuient sur `RandomNumberGenerator` fonctionnent sans modification. BouncyCastle en tire la graine de chaque `SecureRandom`, si bien que son client TLS génère ses clés à partir du CSPRNG du noyau.

```csharp
using System.Security.Cryptography;

byte[] key = new byte[32];
RandomNumberGenerator.Fill(key);

int die = RandomNumberGenerator.GetInt32(1, 7);
Guid id = Guid.NewGuid();
```

Les graines de `System.Random`, de `HashCode` et du hachage des chaînes restent volontairement sur un xorshift peu coûteux : le runtime prend les graines de `HashCode` et du hachage des chaînes à leur première utilisation, qui peut survenir avant que quoi que ce soit d'autre ne soit prêt, et ce chemin doit donc fonctionner à tout moment du démarrage. N'utilisez jamais `System.Random` pour des secrets.

---

<a id="how-the-kernel-csprng-works"></a>

## Comment fonctionne le CSPRNG du noyau

Le générateur se trouve dans `Cosmos.Kernel.Core/Security` et reprend la conception du pilote random de Linux :

- **Un pool d'entropie haché avec BLAKE2s-256.** Y entrent :
  - l'instruction de nombres aléatoires du CPU, lorsque le CPU en annonce une : RDSEED, sinon RDRAND, sur x64 (CPUID) ; RNDRRS, sinon RNDR, sur ARM64 (FEAT_RNG, lu dans `ID_AA64ISAR0_EL1`). Une instruction que le CPU n'annonce pas n'est jamais exécutée. Chaque mot passe un contrôle de santé : zéro, tous les bits à un (une défaillance connue de RDRAND sur certaines puces AMD) et la répétition d'un mot que la même lecture a déjà renvoyé sont rejetés, et une instruction qui échoue de façon répétée n'est plus utilisée ;
  - la gigue de timing du CPU, toujours : 4096 mesures de la durée d'un petit parcours mémoire sur le compteur de cycles (RDTSC sur x64, `CNTPCT_EL0` sur ARM64, chaque lecture étant sérialisée pour que deux lectures encadrent le parcours), chaque échantillon étant crédité de 1/8 de bit. Les mesures passent trois tests de santé : les tests de nombre de répétitions et de proportion adaptative de la norme NIST SP 800-90B, qui détectent une durée bloquée ou dominante, et un test de prédiction avec décalage, qui détecte un motif de durées qui se répète. Le crédit est une hypothèse plutôt qu'une mesure propre à chaque machine, et la réussite des tests ne prouve pas qu'un minuteur est imprévisible ;
  - des valeurs propres à chaque démarrage, qui ne sont pas créditées : l'heure de démarrage fournie par le chargeur d'amorçage, le décalage HHDM et une adresse de pile.
- **Une sortie ChaCha20 avec effacement rapide de la clé.** Une clé de 32 octets est extraite du pool. Chaque requête déroule le flux de clé ChaCha20 de cette clé : les 32 premiers octets deviennent la clé suivante et ne sont jamais renvoyés, la requête reçoit le reste, et l'ancienne clé disparaît. Capturer plus tard l'état du générateur ne révèle rien de ce qui a déjà été distribué.
- **Réensemencement.** Toutes les 256 requêtes ou tous les 1 Mio de sortie, de nouveaux mots matériels et de nouvelles mesures entrent dans le pool et une nouvelle clé est extraite. Lorsque le matériel ne fournit pas ses 256 bits, le réensemencement prend les 4096 mesures complètes au lieu de quelques-unes, afin que chaque nouvelle clé repose sur autant d'entropie créditée que la première.

Le générateur s'initialise à sa première utilisation, ce qui prend quelques millisecondes ; sans instruction de nombres aléatoires fonctionnelle, chaque réensemencement prend à peu près autant de temps. Il ne lève jamais d'exception et n'échoue jamais, il peut donc être appelé sans risque depuis des constructeurs statiques ; il peut être appelé depuis n'importe quel thread, mais pas depuis un gestionnaire d'interruption, car le premier appel et chaque réensemencement font plus de travail qu'un gestionnaire d'interruption ne devrait en faire.

Sur QEMU, x64 tourne sous le CPU `host` de KVM ou le `max` de TCG, qui disposent tous deux de RDSEED et de RDRAND. Le modèle `qemu64` n'a ni l'un ni l'autre, et le `cortex-a72` par défaut d'ARM64 (comme le `cortex-a53`) n'a pas FEAT_RNG : ces machines reposent uniquement sur la gigue. Le CPU `max` d'ARM64 dispose de RNDR.

---

<a id="the-serial-log"></a>

## Le journal série

Le premier ensemencement écrit une ligne dans le journal série, qui indique ce qu'il a utilisé :

```
[Random] Kernel CSPRNG seeded: RDSEED (512 bits) + 4096 jitter samples, health tests passed
```

Chaque mot matériel compte pour 64 bits et les mesures, lorsqu'elles passent leurs tests de santé, pour 512. Lorsque le premier ensemencement est crédité de moins de 256 bits, parce que le CPU n'a pas d'instruction de nombres aléatoires fonctionnelle **et** que le minuteur échoue aux tests de santé de la gigue (un compteur qui n'avance pas, ou qui répète un motif, comme le fait souvent celui de QEMU sous `-icount`), le générateur s'initialise quand même avec ce dont il dispose, mais affiche une seule fois un avertissement :

```
[Random] WARNING: NO SECURE ENTROPY. This CPU has no working random
[Random] number instruction (RDSEED/RDRAND, RNDRRS/RNDR) and its timer
[Random] failed the jitter health tests: less than 256 bits of the seed
[Random] are credited. ...
```

Sur une telle machine, `RandomNumberGenerator`, `Guid.NewGuid` et les clés TLS peuvent être prévisibles : n'en dépendez pas pour la sécurité sur cette machine. Utilisez un modèle de CPU doté d'une instruction de nombres aléatoires, ou un matériel dont le compteur avance.

Lorsqu'il n'y a pas d'instruction de nombres aléatoires mais que les mesures passent les tests, la graine repose uniquement sur la gigue du minuteur, et un avertissement plus court l'indique :

```
[Random] WARNING: no CPU random number instruction; the seed rests on
[Random] timer jitter alone. ...
```

Les tests de santé détectent un minuteur bloqué ou qui répète un motif court, mais pas tous les minuteurs prévisibles : une machine qui s'exécute de façon déterministe, comme QEMU sous `-icount` ou en mode enregistrement/rejeu, peut les réussir et produire malgré tout la même sortie à chaque démarrage.

---

<a id="summary"></a>

## Résumé

| Tâche | Appel |
|---|---|
| Remplir un tampon d'octets aléatoires sûrs | `RandomNumberGenerator.Fill(data)` |
| Obtenir des octets aléatoires sûrs dans un nouveau tableau | `RandomNumberGenerator.GetBytes(count)` |
| Tirer un entier aléatoire sûr dans un intervalle | `RandomNumberGenerator.GetInt32(fromInclusive, toExclusive)` |
| Remplir un tampon d'octets aléatoires sûrs dont aucun n'est nul | `RandomNumberGenerator.Create().GetNonZeroBytes(data)` |
| Obtenir une instance du générateur | `RandomNumberGenerator.Create()` |
| Créer un identifiant unique | `Guid.NewGuid()` |
| Obtenir des nombres aléatoires pour un jeu ou un mélange | `System.Random`, `Random.Shared` |
