---
source_hash: 24dfc48bd1d8b598536dcee440b53d3c1af4e5b2
---
<a id="debugging-with-vs-code-and-qemu"></a>

# 使用 VS Code 和 QEMU 调试

内核调试采用远程 GDB：QEMU 在 `localhost:1234` 上提供 GDB 服务器，VS Code 通过 `cppdbg` 连接到它，断点直接在编辑器中设置。`cosmos new` 模板自带所需的 `.vscode/launch.json` 和 `.vscode/tasks.json`，并且已经预先配置好。

---

<a id="prerequisites"></a>

## 前提条件

| 工具 | 用途 | 说明 |
|------|---------|-------|
| `gdb` | x64 调试 | 必须位于 `PATH` 中；启动配置以 `gdb` 这个名称调用它 |
| `gdb-multiarch` | ARM64 调试 | 必须位于 `PATH` 中；aarch64 目标需要它 |
| QEMU | 运行内核 | 由 `cosmos install` 安装 |
| VS Code C/C++ 扩展 | `cppdbg` 调试适配器 | `ms-vscode.cpptools` |

`cosmos check` 会检查工具链。在 Debian/Ubuntu 上，`apt install gdb gdb-multiarch` 即可安装这两个调试器。

---

<a id="debugging-a-kernel-created-with-cosmos-new"></a>

## 调试用 `cosmos new` 创建的内核

1. 在 VS Code 中打开内核文件夹。
2. 在你的内核源代码中设置断点。
3. 打开**运行和调试**视图，选择 **Debug x64 Kernel**（或 **Debug ARM64 Kernel**）。
4. 按 F5。预启动任务会构建内核，以 `-s -S` 启动 QEMU（GDB 服务器，启动时冻结执行），然后调试器附加上去。执行会在你的断点处停下。

---

<a id="serial-log"></a>

## 串口日志

内核的每个启动阶段都会把日志写到串口（x64 上为 COM1，ARM64 上为 PL011），`cosmos run` 会把串口连接到你的终端。当内核无法启动，或者在调试器派上用场之前就崩溃时，首先应该阅读的就是串口日志；逐阶段的讲解以及如何将崩溃地址符号化，参见[内核启动](startup.md)。

---

<a id="writing-to-the-serial-log"></a>

## 写入串口日志

`Log` 把你自己的输出写到同一个流上。它是 `Console.WriteLine` 的对应物，而两者的区别恰恰在你需要日志的那些场合最为关键：`Console` 绘制在帧缓冲区画布上，因此需要图形系统已经就绪，每次写入都会重绘屏幕，而且会分配内存。`Log` 则同步地直接写入串口，对字符串和数字不做任何分配，并且从 `BeforeRun` 的第一行起就能工作，无论是在还没有控制台可供写入之前，还是在崩溃连带帧缓冲区一起失效之后。

```csharp
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.System.Storage;

Log.WriteString("storage: ");
Log.WriteNumber(StorageManager.DeviceCount);
Log.WriteString(" device(s), ");
Log.WriteNumber(StorageManager.Partitions.Count);
Log.WriteString(" partition(s)\n");
```

没有 `WriteLine`：请自行追加 `"\n"`，内核中各处也都是这样做的。

| 调用 | 写入内容 |
|---|---|
| `Log.WriteString(text)` | 一个字符串。参数不能为 null |
| `Log.Write(a, b, c)` | 按顺序写入每个参数的 `ToString()`。接受 null，并为其写入 `null` |
| `Log.WriteNumber(n)` | 十进制数字。有 `int`、`uint`、`long` 和 `ulong` 的重载 |
| `Log.WriteHex(n)` | 十六进制数字 |
| `Log.WriteHexWithPrefix(n)` | 同上，但带有前导 `0x` |
| `Log.WriteBytes(span)` | 原始字节，不做格式化 |

在任何频繁执行或较早执行的路径上，优先使用 `WriteString` 和 `WriteNumber`，而不是 `Write`。`Write` 的参数列表构建在栈上，没有任何开销，但传给它的数字或 `bool` 会先被装箱，而这个装箱对象就是一次堆分配，类型化的重载则不会产生这种分配。

串口本身受 `CosmosEnableUART` 功能开关控制。关闭该开关后，这些调用仍会正常返回，但不会有任何内容到达串口输出流，因此关闭了该开关的内核不应依赖 `Log` 输出任何它需要回读的内容。

---

<a id="known-limitations"></a>

## 已知限制

- VS Code 调试体验中存在源代码映射和变量查看方面的 bug（参见[路线图](../../roadmap.md)）；单步执行和断点可以正常工作，但查看某些局部变量时可能显示错误的值或缺失值。
- 调试以 QEMU 为前提。VMware、VirtualBox 和 Hyper-V 是未经测试的目标。
- 在 TCG 模拟下进行 ARM64 调试速度较慢；在大型内核上执行单步操作时，可能会出现数秒的停顿。
