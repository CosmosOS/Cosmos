---
source_hash: 30a9cec1ba1f8691c5aea814f8b0fb80b2309605
---
<a id="trimming-and-feature-switches"></a>

# 裁剪与功能开关

AOT（ahead-of-time，提前）编译器在程序运行之前就能看到整个程序。它从入口点出发，沿着每一次调用、每一个被构造的类型以及每一个可能被到达的虚方法前进，编译它所到达的部分，并把其余一切排除在镜像之外。这就是裁剪（trimming）。它让基于大型库构建的内核保持小巧，同时也意味着代码必须可达才会存在：一个没有任何地方引用的类不会出现在镜像中，无论它来自哪个程序集。

功能开关在构建时决定可达性。它是一个具名设置，在编译程序时固定下来，并通过一个编译器知道的属性来读取。编译器把对该属性的每一次读取都替换为开关的值，因此由关闭的开关守卫的分支测试的是常量 `false`，永远不会执行，并会被删除，只有该分支才能到达的所有内容也随之删除。用这种方式关闭一个子系统，是把它的代码从镜像中移除，而不是在运行时跳过它。

陷阱在于守卫之外的引用。裁剪删除的是不可达的代码，而不是被守卫的代码：只要留在镜像中的任何代码引用了某个类型，即使它的开关已关闭，该类型也会保留下来。开关同时也是一项构建设置，因此关闭某个功能后编译的内核无法在运行时打开它；代码根本不在那里。

在 Cosmos 中，内核 `.csproj` 中的每个 `CosmosEnable*` 属性都会成为一个标记为 `Trim="true"` 的 `RuntimeHostConfigurationOption`，并作为 `--feature:` 参数传给 NativeAOT 编译器 ILC（[Cosmos.Build.Ilc](../../dev/build/ilc-build.md)）。ILC 替换的属性位于 Core 的 `CosmosFeatures` 中，标记为 `[FeatureSwitchDefinition]`；`Cosmos.Kernel.System` 中的 `KernelFeatures` 则是内核读取的公共视图。有些开关会关闭其他开关：关闭 `CosmosEnableInterrupts` 时，`Sdk.targets` 还会关闭定时器、键盘、鼠标、网络、存储、调度器和 PCI 的开关；关闭 `CosmosEnableScheduler` 时，则会关闭终结器线程的开关（`CosmosEnableFinalizerThread`）。驱动程序清单把这些开关用作守卫：声明为 `[Driver(Feature = DriverFeature.Network)]` 的驱动程序在 `if (KernelFeatures.Network)` 内注册，而由于没有其他代码会构造内置驱动程序，关闭 `CosmosEnableNetwork` 构建的内核根本不包含任何网络驱动程序。参见[特性与清单](../drivers.md#the-attribute-and-the-manifest)和[功能守卫](../../dev/build/driver-manifest.md#feature-guards)。
