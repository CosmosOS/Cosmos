---
source_hash: a26414dec2e20dc7b931459cf194b7489982135c
---
<a id="translating-the-docs"></a>

# 翻译文档

文档以英文、法文和简体中文发布，英文页面是另外两种语言遵循的参考。本页说明翻译页面如何组织和构建、其标题如何保留英文锚点、它如何记录自己所依据的英文页面、文档构建如何报告落后于英文的翻译，以及英文页面变更时贡献者要做什么。

---

<a id="how-the-languages-are-laid-out"></a>

## 各语言的组织方式

每种语言是 `docs/` 下的一个文件夹，镜像英文目录树：`docs/fr/articles/user/timers.md` 是 `docs/articles/user/timers.md` 的法文翻译，`docs/zh/` 在相同的相对路径下存放中文翻译。由于路径相同，`drivers.md#platform-nodes` 这样的相对链接在每种语言中写法都一样，并且会落到其自身语言的页面上。

每个语言文件夹都是一个独立的 docfx 项目，有自己的 `docfx.json`，把它构建到英文站点的一个子文件夹中，即 `_site/fr/` 和 `_site/zh/`；根目录的 `docs/docfx.json` 排除这些文件夹，使英文站点不会再发布它们一次。语言使用英文目录树中的图片和视频，因此翻译引用 `images/mouse-paint.mp4` 的方式与英文页面完全相同；语言也使用 `docs/api/` 中生成的 API 参考，它在每种语言中都以英文发布，因为它来自源代码的 XML 文档。在共享的 `templates/custom` 层之上，语言还会添加 `templates/<folder>/token.json`，即该语言的主题字符串（搜索框、“本文内容”、注意和警告标签），并设置 `_lang`，docfx 会把它写到 `<html>` 元素上。

与英文目录树共享文件带来两个后果，因为 docfx 在把文件映射到站点之前，会先在磁盘上、在页面或 toc 旁边查找它们所指的文件。翻译版根 `toc.yml` 的 API 条目写作 `../api/`，而英文版写作 `api/`。此外，在翻译版的构建中，每张图片和每个视频都只存在于英文目录树中，因此会被报告为 `InvalidFileLink`，尽管构建会把它们复制到站点中的正确位置；所以语言的 `docfx.json` 把这条规则降为 `info`。这条规则不再报告的内容由 `docs-i18n.py check` 负责：它要求翻译的链接目标与其英文页面完全一致，而英文构建会校验这些目标；它要求翻译版 `toc.yml` 的每个条目都指向其自身语言的页面，或通过 `../api/` 指向 API 参考；它还会报告每个尚无翻译的英文页面。

`templates/custom/public/main.js` 正是通过 `lang` 属性得知页面所用的语言。它在导航栏旁添加一个语言下拉菜单，用所选语言打开同一页面，当该页面在那种语言中不存在时则回退到那种语言的首页；它还在每篇翻译文章的顶部放置一个提示横幅，说明英文页面才是参考，并附上指向英文页面的链接。当所选版本具备当前页面、章节和语言时，版本下拉菜单会保留它们。在某种语言出现之前冻结的版本没有该语言的文件夹：在这个版本的页面上，语言下拉菜单会禁用该语言，而翻译页面的版本下拉菜单会落到该版本的英文页面。

每种语言还有自己的搜索索引，由 docfx 根据该语言自己的页面构建。主题使用 lunr 搜索该索引，lunr 按空白和标点把文本切分成词；法文站点通过 `main.js` 中的 `lunrLanguages`，在英文词干提取器之外加上 lunr 的法文词干提取器。中文词与词之间没有空格，因此 lunr 会把整个中文句子索引为一个词，永远找不到其中的某个词。为此，中文站点用 `templates/zh/public/search-worker.min.js` 替换主题的搜索 worker，它按子串匹配，并使用主题搜索框所期望的协议。这个协议属于 docfx 的 modern 模板，任何一个 docfx 版本都可能改变它，因此 `build-docs.yml`、`release.yml` 和 `Makefile` 安装的是固定版本的 docfx。当构建出的主题不再提到该 worker 的文件名或其消息时，`check` 会发出警告；升级版本之前，请先在本地构建上测试中文搜索。

保留英文的是读者会复制的内容或生成的内容：代码块（包括其中的注释）、SVG 图表和 API 参考。页面上的其他一切都要翻译，包括标题、表格、链接文本和图片描述。

<a id="headings-keep-their-english-anchors"></a>

## 标题保留英文锚点

docfx 根据标题文本生成标题的 id，因此翻译后的标题会得到与英文标题不同的 id，中文标题得到的 id 甚至是用中文写的。这样，每个按英文标题写成的 `page.md#anchor` 形式的链接都会找不到对应章节，语言下拉菜单也无法让读者停留在同一位置。因此翻译同时带有英文 id：`.github/scripts/docs-i18n.py stamp` 会在每个自身 id 与英文 id 不同的翻译标题前插入一行 `<a id="english-id"></a>`，页面标题也包括在内。

页面加载时，`main.js` 会把每个这样的 id 移到其后的标题上并删除该锚点，这一步发生在主题构建“本文内容”列表和标题链接之前。这样，翻译页面的 id 就与英文页面完全相同；没有 JavaScript 时，仅靠这些锚点也能让链接到达其章节。如果没有这一步，该列表会链接到翻译后的 id，而语言下拉菜单无法把这些 id 带到另一种语言中；翻译标题自身的 id 还可能遮蔽另一个标题的英文 id：plug 页面的中文标题 `Plug` 得到的 id 是 `plug`，而这正是其 `[Plug]` 章节的英文 id。

`stamp` 按位置配对标题，因此翻译必须与英文页面的标题完全一致：数量相同、级别相同、顺序相同。译者从不手写这些锚点。对文件再次运行 `stamp` 会替换它之前写入的锚点，即带有英文标题 id 的那些锚点，其他锚点则保持不变。

`stamp` 按照 docfx 的方式计算英文 id，而不是从构建出的站点中读取，这样它在构建之前也能工作。英文站点构建完成后，`check` 会把它计算出的 id 与 docfx 写出的 id 进行比较；如果某个页面的标题使用了脚本未遵循的语法，它会在错误的锚点进入翻译之前报告该页面。

<a id="the-source-hash"></a>

## 源哈希

`stamp` 还会记录翻译所依据的英文页面。它把英文页面的 git blob 哈希作为 `source_hash` 写入翻译的前置元数据（front matter），对于翻译版的 `toc.yml`，则写在文件顶部的一行 `# source_hash:` 注释中。英文页面的任何修改都会改变这个哈希，因此将它与当前英文页面比较，就能知道翻译是否仍与英文页面同步。

<a id="checking-the-translations"></a>

## 检查翻译

`python3 .github/scripts/docs-i18n.py check` 遍历每个英文页面，并针对每种语言报告以下几类翻译：缺失的、过时的（其 `source_hash` 与英文页面不再匹配）、孤立的（英文页面已不存在），以及结构与英文页面不再一致的，即标题、代码块、链接目标、行内代码、表格行或段落不同，有一行与六个词或更多词的英文行完全相同，使用了文档风格规范禁止的破折号，或者有 toc 条目指向该语言文件夹之外。它无法读取的文件，例如未以 UTF-8 保存的文件，会像其他发现一样被报告。

文档工作流在构建完三个站点后运行它。每个有发现的页面都会成为工作流运行上的一条警告注释，完整列表则写入该运行的摘要，因为 GitHub 只显示一个步骤的前十条注释。这一步从不让构建失败，因为英文的修改不应等待能写法文和中文的人；过时的翻译会继续在其提示横幅下发布，直到有人更新它。在本地，`--strict` 让它以错误状态退出，`--lang fr` 将它限制在一种语言，而一组英文路径（相对于 `docs/`）则将它限制在这些页面。

<a id="when-you-change-an-english-page"></a>

## 修改英文页面时

你不必在同一个拉取请求中更新翻译。如果可以，请修改两份翻译，使其内容与英文页面现在的内容一致，并保持标题与英文页面同步，然后为它们打上标记：

```bash
python3 .github/scripts/docs-i18n.py stamp docs/fr/articles/user/timers.md docs/zh/articles/user/timers.md
python3 .github/scripts/docs-i18n.py check articles/user/timers.md
```

新的英文页面照常加入其所在文件夹的 `toc.yml`。在它被翻译之前，`check` 会在每种语言中把它报告为缺失，并报告翻译版的 `toc.yml` 与英文版不同；翻译版的目录只有在翻译存在后才会列出它，因为翻译版的构建只会以 `info` 级别报告缺失的页面，并会把该条目作为失效链接发布（`check` 会报告这样的条目）。页面被重命名或删除时，其翻译也要随之重命名或删除，否则 `check` 会把它们报告为孤立翻译。

部署会保留已经发布的文件，因为已发布的各个版本与开发版站点一起存放在 Pages 仓库中。因此，被删除的翻译仍会留在线上，而语言下拉菜单通过请求页面的 URL 来查找页面，仍然会打开它；请同时把它从 Pages 仓库中删除。

<a id="adding-a-language"></a>

## 添加语言

新增一种语言需要：

- `docs/` 下的一个文件夹及其 `docfx.json`（复制 `docs/fr/docfx.json`，修改输出文件夹、模板层和 `_lang`）。文件夹名称不能像版本文件夹（`v1.2.3`、`latest`），否则 `main.js` 会把它当作一个已发布版本；
- 在根目录 `docs/docfx.json` 的 `exclude` 列表中加入其文件夹；
- 一个根 `toc.yml`，其 API 条目写作 `../api/`；
- 一个包含主题字符串的 `templates/<folder>/token.json`；
- 在 `main.js` 中，在 `languages` 列表里加入一个条目，添加提示横幅文本，并在 lunr 有该语言的词干提取器时把它加入 `lunrLanguages`；
- 对于词与词之间不加空格的语言，需要一个按子串匹配的搜索 worker，例如 `templates/zh/public/search-worker.min.js`，并在 `docs-i18n.py` 中加入检查页面是否包含该文字的规则，就像中文的那条一样。

`Makefile`、`build-docs.yml`、`release.yml` 和 `docs-i18n.py` 通过 `docs/<folder>/docfx.json` 找到各种语言，因此它们无需修改。

<a id="terms"></a>

## 术语

翻译在所有页面中对同一概念使用同一个术语。下表列出了已确定的术语，使新的或更新后的翻译读起来与周围的页面一致。

| English | Français | 简体中文 |
|---|---|---|
| kernel | noyau | 内核 |
| driver | pilote | 驱动程序 |
| device | périphérique | 设备 |
| driver kit | kit de pilotes | 驱动工具包 |
| binding (a driver to a device) | liaison | 绑定 |
| match, matching | correspondance | 匹配 |
| probe | sonder | 探测 |
| hot-plug | branchement à chaud | 热插拔 |
| host controller | contrôleur hôte | 主机控制器 |
| register | registre | 寄存器 |
| descriptor ring | anneau de descripteurs | 描述符环 |
| interrupt | interruption | 中断 |
| interrupt context | contexte d'interruption | 中断上下文 |
| interrupt handler | gestionnaire d'interruption | 中断处理程序 |
| kind (device kind, bus kind) | type (type de périphérique, type de bus) | 种类（设备种类，总线种类） |
| thread | thread | 线程 |
| scheduler | ordonnanceur | 调度器 |
| scheduling policy | politique d'ordonnancement | 调度策略 |
| run queue | file d'exécution | 运行队列 |
| preemption | préemption | 抢占 |
| tick source | source de ticks | 节拍源 |
| context switch | changement de contexte | 上下文切换 |
| lock, spinlock | verrou, spinlock | 锁，自旋锁 |
| timer, alarm | minuteur, alarme | 定时器，闹钟 |
| callback | callback | 回调 |
| garbage collector | ramasse-miettes | 垃圾回收器 |
| heap, stack | tas, pile | 堆，栈 |
| GC root | racine | 根 |
| safepoint | point sûr | 安全点 |
| write barrier | barrière d'écriture | 写屏障 |
| finalizer | finaliseur | 终结器 |
| plug, patcher | plug, patcher | plug，patcher |
| feature switch | commutateur de fonctionnalité | 功能开关 |
| public API surface | surface d'API publique | 公共 API 面 |
| experimental seam | point d'extension expérimental | 实验性扩展点 |
| framebuffer | framebuffer | 帧缓冲区 |
| block device | périphérique bloc | 块设备 |
| filesystem | système de fichiers | 文件系统 |
| test kernel, test suite | noyau de test, suite de tests | 测试内核，测试套件 |
