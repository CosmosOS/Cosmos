---
_layout: landing
source_hash: 7277f6b5f33ef20c90622f8945755ecf24727d45
---

欢迎来到 Cosmos gen3 wiki！

查看[路线图](roadmap.md)，了解我们迈向首个版本的进展 🚀。

文档按你想用 gen3 做什么分为两部分：

- **[用户指南](articles/user/install.md)**：你想用 Cosmos **构建自己的操作系统**，包括安装工具链、使用文件系统、调试你的内核。
- **[贡献者文档](articles/dev/install-dev.md)**：你想**为 Cosmos 本身做贡献**，或了解它的内部实现，包括架构、构建流水线和运行时子系统。

<a id="user-guide"></a>

## 用户指南

创建、构建和运行你自己的 Cosmos 内核所需的一切：

 - [安装指南](articles/user/install.md)：配置工具链，并在 VS Code 中创建你的第一个内核。
 - [内核启动](articles/user/startup.md)：启动链以及 `BeforeRun`/`Run`/`AfterRun` 生命周期。
 - [文件系统](articles/user/filesystem.md)：挂载磁盘，并使用标准 .NET `System.IO` API（`File`、`Directory`、流）。
 - [网络](articles/user/network.md)：通过标准 .NET `System.Net.Sockets` API 使用 DHCP、UDP 和 TCP，以及 DNS。
 - [定时器与闹钟](articles/user/timers.md)：阻塞一段时间，或稍后从定时器中断（`TimerManager`）或内核线程（`AlarmManager`）中运行回调。
 - [图形](articles/user/graphics.md)：使用 Canvas API 在屏幕上绘制形状、文本和图像。
 - [音频](articles/user/audio.md)：借助 `AudioManager` 和 `Console.Beep`，通过 HD Audio 输出播放音调、WAV 文件以及你自己生成的声音。
 - [键盘](articles/user/keyboard.md)：通过 `Console` 和 `KeyboardManager` 读取整行输入、按键事件和键盘布局。
 - [控制台会话与 Telnet](articles/user/sessions.md)：在虚拟控制台上并排运行多个 shell，并通过 Telnet 将它们提供给远程终端。
 - [鼠标](articles/user/mouse.md)：通过 `MouseManager` 获取指针位置、按键和滚轮。
 - [编写驱动程序](articles/user/drivers.md)：将驱动程序与设备匹配，访问硬件，把设备交给内核，并在没有硬件的情况下进行测试。
 - [编写调度器](articles/user/scheduler-plugging.md)：如何实现并安装调度策略。
 - [使用 VSCode 和 QEMU 调试](articles/user/debugging.md)：通过远程 GDB 在你的内核中设置断点。

<a id="contributor-docs"></a>

## 贡献者文档

架构与内部实现，面向贡献者和好奇的读者：

 - [开发容器配置](articles/dev/install-dev.md)：从源代码构建框架。
 - [内核项目结构](articles/dev/kernel-project-layout.md)：分层的项目依赖图。
 - [编码规范](articles/dev/coding-guidelines.md)：代码风格与架构模式。
 - [Plug](articles/dev/plugs.md)：IL 层面的方法替换系统。
 - [测试](articles/dev/testing.md)：单元测试与 QEMU 内核测试套件。
 - [公共 API 跟踪](articles/dev/public-api.md)：声明的 API 面文件、包验证、带版本的文档。
 - [垃圾回收器](articles/dev/garbage-collector.md)：标记-清除式 GC。
 - [垃圾回收器 - 精确栈扫描](articles/dev/garbage-collector-gcinfo.md)：GCInfo 如何让触发回收的线程的栈扫描做到精确。
 - [垃圾回收器 - 术语表](articles/dev/garbage-collector-glossary.md)：各篇文章所依赖的 GC 概念的背景说明。
 - [调度器](articles/dev/scheduler.md)：抢占式、可插拔的调度器。
 - [调度器 - 术语表](articles/dev/scheduler-glossary.md)：该文章所依赖的调度概念的背景说明。
 - [内核编译步骤](articles/dev/build/kernel-compilation-steps.md)：从 C# 到可启动 ISO 的完整流程。
 - [Cosmos.Build.Asm](articles/dev/build/asm-build.md)、[Cosmos.Build.GCC](articles/dev/build/gcc-build.md)、[Cosmos.Build.Patcher](articles/dev/build/patcher-build.md)、[Cosmos.Build.Ilc](articles/dev/build/ilc-build.md)：构建流水线的各个组件。
 - [驱动程序清单](articles/dev/build/driver-manifest.md)：构建会注册哪些 `[Driver]` 类、按什么顺序注册，以及内核如何排除或显式启用某个驱动程序。

<a id="cross-referencing"></a>

## 交叉引用

API 参考文档在 [`/xrefmap.yml`](https://cosmosos.github.io/xrefmap.yml) 发布了一份 DocFX 交叉引用映射，涵盖每个已文档化的命名空间、类型和成员。其他 DocFX 站点只需将该 URL 添加到自身 `docfx.json` 的 `xref` 列表中，即可使用它：

```json
{
  "build": {
    "xref": [ "https://cosmosos.github.io/xrefmap.yml" ]
  }
}
```

这样，Cosmos 类型就通过 UID 而不是 URL 进行链接，写作 `<xref:Cosmos.Kernel.System.Graphics.Canvas>` 或其简写 `@Cosmos.Kernel.System.Graphics.Canvas`。每个冻结的发布副本都在旁边附带自己的映射，位于 `/vX.Y.Z/xrefmap.yml` 和 `/latest/xrefmap.yml`。

<a id="resources"></a>

## 资源
- [Cosmos Gen3：NativeAOT 时代与 IL2CPU 的终结？](https://valentin.bzh/posts/3)
- [NativeAOT 开发者工作流程](https://github.com/dotnet/runtime/blob/main/docs/workflow/building/coreclr/nativeaot.md)
- [Native AOT 的限制](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/#limitations-of-native-aot-deployment)
