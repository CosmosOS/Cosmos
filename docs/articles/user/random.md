# Random numbers

In this article, we will discuss where random numbers come from on Cosmos Gen3: which .NET APIs are cryptographically secure, what feeds them, and the warning a kernel prints when the machine offers no real entropy.

If you find bugs or something abnormal, please [submit an issue](https://github.com/CosmosOS/Cosmos/issues/new/choose) on our repository.

---

## Which API to use

Use the standard .NET APIs; nothing Cosmos-specific is needed.

| API | Backed by | Use it for |
|---|---|---|
| `System.Security.Cryptography.RandomNumberGenerator` (`Fill`, `GetBytes`, `GetInt32`, `GetNonZeroBytes`, `Create()`) | The kernel CSPRNG | Keys, nonces, tokens, anything secret |
| `Guid.NewGuid()` | The kernel CSPRNG | Unique identifiers |
| `System.Random`, `Random.Shared`, `HashCode` and string hashing seeds | CoreLib's own generators, seeded from a xorshift over the cycle counter | Games, shuffles, anything that is not a secret |

Libraries that build on `RandomNumberGenerator` work unchanged. BouncyCastle seeds every `SecureRandom` from it, so its TLS client generates its keys from the kernel CSPRNG.

```csharp
using System.Security.Cryptography;

byte[] key = new byte[32];
RandomNumberGenerator.Fill(key);

int die = RandomNumberGenerator.GetInt32(1, 7);
Guid id = Guid.NewGuid();
```

The seeds of `System.Random`, `HashCode` and string hashing stay on a cheap xorshift on purpose: the runtime takes the `HashCode` and string hashing seeds on first use, which can come before anything else is ready, so that path must work at any point of the boot. Never use `System.Random` for secrets.

---

## How the kernel CSPRNG works

The generator lives in `Cosmos.Kernel.Core/Security` and follows the design of Linux's random driver:

- **An entropy pool hashed with BLAKE2s-256.** Into it go:
  - the CPU's random number instruction, when the CPU advertises one: RDSEED, else RDRAND, on x64 (CPUID); RNDRRS, else RNDR, on ARM64 (FEAT_RNG, read from `ID_AA64ISAR0_EL1`). An instruction the CPU does not advertise is never executed. Every word is health checked: zero, all ones (a known RDRAND failure on some AMD parts) and a repeat of a word the same read already returned are rejected, and an instruction that keeps failing is no longer used;
  - CPU timing jitter, always: 4096 timings of a small memory walk on the cycle counter (RDTSC on x64, `CNTPCT_EL0` on ARM64, each read serialized so that two reads bracket the walk), each sample credited with 1/8 bit. The timings go through three health tests: the repetition count and adaptive proportion tests of NIST SP 800-90B, which catch a stuck or dominant timing, and a lag prediction test, which catches a timing pattern that repeats. The credit is an assumption rather than a measurement of each machine, and passing the tests does not prove a timer unpredictable;
  - per-boot values that are not credited: the bootloader's boot time, the HHDM offset and a stack address.
- **ChaCha20 output with fast key erasure.** A 32-byte key is extracted from the pool. Each request runs that key's ChaCha20 keystream: the first 32 bytes become the next key and are never output, the request gets the rest, and the old key is gone. Capturing the generator's state later reveals nothing already handed out.
- **Reseeding.** Every 256 requests or 1 MiB of output, fresh hardware words and timings go into the pool and a new key is extracted. When the hardware does not supply its 256 bits, the reseed takes the full 4096 timings instead of a few, so that each new key rests on as much credited entropy as the first.

The generator seeds itself on first use, which takes a few milliseconds; without a working random number instruction, every reseed takes about as long. It never throws and never fails, so it is safe to reach from static constructors; it may be called from any thread, but not from an interrupt handler, since the first call and every reseed do more work than an interrupt handler should.

On QEMU, x64 runs under KVM's `host` CPU or TCG's `max`, which both have RDSEED and RDRAND. The `qemu64` model has neither, and ARM64's default `cortex-a72` (like `cortex-a53`) has no FEAT_RNG: those machines rely on jitter alone. The ARM64 `max` CPU has RNDR.

---

## The serial log

The first seeding writes one line to the serial log, naming what it used:

```
[Random] Kernel CSPRNG seeded: RDSEED (512 bits) + 4096 jitter samples, health tests passed
```

Each hardware word counts for 64 bits and the timings, when they pass their health tests, for 512. When the first seeding is credited with less than 256 bits, because the CPU has no working random number instruction **and** the timer fails the jitter health tests (a counter that does not tick, or one that repeats a pattern, as QEMU's under `-icount` often does), the generator still seeds from what it has, but prints a warning once:

```
[Random] WARNING: NO SECURE ENTROPY. This CPU has no working random
[Random] number instruction (RDSEED/RDRAND, RNDRRS/RNDR) and its timer
[Random] failed the jitter health tests: less than 256 bits of the seed
[Random] are credited. ...
```

On such a machine, `RandomNumberGenerator`, `Guid.NewGuid` and TLS keys may be predictable: do not rely on them for security there. Run with a CPU model that has a random number instruction, or on hardware whose counter ticks.

When there is no random number instruction but the timings pass, the seed rests on timer jitter alone, and a shorter warning says so:

```
[Random] WARNING: no CPU random number instruction; the seed rests on
[Random] timer jitter alone. ...
```

The health tests catch a timer that is stuck or repeats a short pattern, not every predictable one: a machine that runs deterministically, such as QEMU under `-icount` or record/replay, can pass them and still produce the same output on every boot.
