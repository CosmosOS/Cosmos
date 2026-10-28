---
source_hash: 54cecb45b8a36c017b0cac120a075fa8070ee93d
---
<a id="random-numbers"></a>

# 随机数

本文将介绍 Cosmos Gen3 上随机数的来源：哪些 .NET API 是密码学安全的，为它们提供熵的是什么，以及当机器没有真正的熵时内核打印的警告。

如果你发现 bug 或任何异常，请在我们的仓库中[提交 issue](https://github.com/CosmosOS/Cosmos/issues/new/choose)。

---

<a id="which-api-to-use"></a>

## 应该使用哪个 API

使用标准的 .NET API 即可，不需要任何 Cosmos 特有的东西。

| API | 底层来源 | 适用场景 |
|---|---|---|
| `System.Security.Cryptography.RandomNumberGenerator`（`Fill`、`GetBytes`、`GetInt32`、`GetNonZeroBytes`、`Create()`） | 内核 CSPRNG | 密钥、nonce、令牌，任何需要保密的东西 |
| `Guid.NewGuid()` | 内核 CSPRNG | 唯一标识符 |
| `System.Random`、`Random.Shared`、`HashCode` 以及字符串哈希的种子 | CoreLib 自带的生成器，以基于周期计数器的 xorshift 作为种子 | 游戏、洗牌，任何不需要保密的东西 |

基于 `RandomNumberGenerator` 构建的库无需修改即可工作。BouncyCastle 的每个 `SecureRandom` 都以它作为种子，因此其 TLS 客户端用内核 CSPRNG 生成密钥。

```csharp
using System.Security.Cryptography;

byte[] key = new byte[32];
RandomNumberGenerator.Fill(key);

int die = RandomNumberGenerator.GetInt32(1, 7);
Guid id = Guid.NewGuid();
```

`System.Random`、`HashCode` 和字符串哈希的种子有意保留在廉价的 xorshift 上：运行时在首次使用时获取 `HashCode` 和字符串哈希的种子，这可能早于其他任何部分就绪，因此这条路径必须在启动的任何阶段都能工作。切勿将 `System.Random` 用于机密。

---

<a id="how-the-kernel-csprng-works"></a>

## 内核 CSPRNG 的工作原理

生成器位于 `Cosmos.Kernel.Core/Security`，沿用 Linux random 驱动程序的设计：

- **使用 BLAKE2s-256 哈希的熵池。** 进入熵池的有：
  - CPU 的随机数指令（当 CPU 声明支持时）：在 x64 上为 RDSEED，否则为 RDRAND（CPUID）；在 ARM64 上为 RNDRRS，否则为 RNDR（FEAT_RNG，从 `ID_AA64ISAR0_EL1` 读取）。CPU 未声明支持的指令永远不会被执行。每个字都要经过健康检查：全零、全一（某些 AMD 芯片上已知的 RDRAND 故障）以及与同一次读取已返回的字重复的字都会被拒绝，持续失败的指令将不再使用；
  - CPU 计时抖动，始终加入：在周期计数器上对一次小规模内存遍历计时 4096 次（x64 上为 RDTSC，ARM64 上为 `CNTPCT_EL0`，每次读取都经过序列化，使两次读取恰好包住这次遍历），每个样本计入 1/8 比特的熵。这些计时值要经过三项健康测试：NIST SP 800-90B 的重复计数测试和自适应比例测试，用于发现卡住或占主导的计时值，以及滞后预测测试，用于发现重复出现的计时模式。计入的熵量是一种假设，而不是针对每台机器的测量，通过测试也不能证明定时器不可预测；
  - 每次启动各不相同但不计入熵的值：引导加载程序提供的启动时间、HHDM 偏移量和一个栈地址。
- **采用快速密钥擦除的 ChaCha20 输出。** 从熵池中提取一个 32 字节的密钥。每次请求都运行该密钥的 ChaCha20 密钥流：前 32 字节成为下一个密钥且从不输出，请求获得其余部分，旧密钥随之消失。之后即使截获生成器的状态，也无法得知任何已经分发出去的数据。
- **重新播种。** 每 256 次请求或每输出 1 MiB，新的硬件字和计时值会进入熵池，并提取一个新密钥。当硬件未提供其 256 比特时，重新播种会采集完整的 4096 次计时而不是少量几次，使每个新密钥所依赖的计入熵与第一个密钥一样多。

生成器在首次使用时自行播种，耗时几毫秒；如果没有可用的随机数指令，每次重新播种也大约需要这么久。它从不抛出异常，也从不失败，因此可以安全地在静态构造函数中调用；它可以从任何线程调用，但不能从中断处理程序调用，因为首次调用和每次重新播种所做的工作都超出了中断处理程序应做的范围。

在 QEMU 上，x64 运行在 KVM 的 `host` CPU 或 TCG 的 `max` CPU 下，两者都具备 RDSEED 和 RDRAND。`qemu64` 型号两者皆无，而 ARM64 默认的 `cortex-a72`（与 `cortex-a53` 一样）没有 FEAT_RNG：这些机器只能依靠抖动。ARM64 的 `max` CPU 具备 RNDR。

---

<a id="the-serial-log"></a>

## 串口日志

首次播种会向串口日志写入一行，说明它使用了什么：

```
[Random] Kernel CSPRNG seeded: RDSEED (512 bits) + 4096 jitter samples, health tests passed
```

每个硬件字计为 64 比特，计时值在通过健康测试时计为 512 比特。当首次播种计入的熵少于 256 比特时，即 CPU 没有可用的随机数指令，**并且**定时器未通过抖动健康测试（计数器不走动，或者重复某种模式，QEMU 在 `-icount` 下的计数器常常如此），生成器仍会用已有的内容播种，但会打印一次警告：

```
[Random] WARNING: NO SECURE ENTROPY. This CPU has no working random
[Random] number instruction (RDSEED/RDRAND, RNDRRS/RNDR) and its timer
[Random] failed the jitter health tests: less than 256 bits of the seed
[Random] are credited. ...
```

在这样的机器上，`RandomNumberGenerator`、`Guid.NewGuid` 和 TLS 密钥可能是可预测的：不要在那里依赖它们来保障安全。请使用具备随机数指令的 CPU 型号运行，或者在计数器正常走动的硬件上运行。

当没有随机数指令但计时值通过测试时，种子仅依赖定时器抖动，此时会有一条较短的警告说明这一点：

```
[Random] WARNING: no CPU random number instruction; the seed rests on
[Random] timer jitter alone. ...
```

健康测试能发现卡住或重复短模式的定时器，但不能发现所有可预测的定时器：以确定性方式运行的机器，例如处于 `-icount` 或记录/回放（record/replay）模式下的 QEMU，可能通过这些测试，却在每次启动时都产生相同的输出。
