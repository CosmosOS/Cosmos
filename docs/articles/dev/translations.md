# Translating the docs

The documentation is published in English, French and Simplified Chinese, and the English pages are the reference the other two follow. This page explains how a translated page is laid out and built, how its headings keep the English anchors, how it records the English page it was made from, how the docs build reports a translation that has fallen behind, and what a contributor does when an English page changes.

---

## How the languages are laid out

A language is a folder of `docs/` that mirrors the English tree: `docs/fr/articles/user/timers.md` is the French translation of `docs/articles/user/timers.md`, and `docs/zh/` holds the Chinese one at the same relative path. Because the paths are the same, a relative link such as `drivers.md#platform-nodes` reads the same in every language and lands on the page of its own language.

Each language folder is a docfx project of its own, with a `docfx.json` that builds it into a subfolder of the English site, `_site/fr/` and `_site/zh/`, and the root `docs/docfx.json` excludes these folders so that the English site does not publish them a second time. A language takes the images and the videos from the English tree, so a translation references `images/mouse-paint.mp4` exactly as the English page does, and it takes the generated API reference from `docs/api/`, which is published in English in every language because it comes from the XML documentation of the source. On top of the shared `templates/custom` layer, a language adds `templates/<folder>/token.json`, the theme's own strings (the search box, "In this article", the note and warning labels) in that language, and sets `_lang`, which docfx writes on the `<html>` element.

Sharing files with the English tree has two consequences, because docfx looks for a file a page or a toc names on disk, next to that page or toc, before it maps the files into the site. The API entry of a translated root `toc.yml` reads `../api/` where the English one reads `api/`. And in a translated build, every image and video, which only exists in the English tree, would be reported as an `InvalidFileLink` even though the build copies it to the right place in the site, so a language's `docfx.json` lowers that rule to `info`. `docs-i18n.py check` covers what the rule no longer reports: it requires a translation to link to exactly the targets of its English page, which the English build validates, it requires each entry of a translated `toc.yml` to name a page of its own language, or the API reference through `../api/`, and it reports every English page that has no translation.

The `lang` attribute is how `templates/custom/public/main.js` knows which language a page is in. It adds a language dropdown next to the navbar, which opens the same page in the chosen language and falls back to that language's landing page when the page does not exist there, and it puts a notice at the top of every translated article saying that the English page is the reference, with a link to it. The version dropdown keeps the page, its section and its language when the chosen release has them. A release frozen before a language existed has no folder for that language: on its pages the language dropdown disables it, and the version dropdown of a translated page lands on the English page of that release.

Each language also has its own search index, built by docfx from its own pages. The theme searches it with lunr, which splits text into words at whitespace and punctuation; the French site adds lunr's French stemmer to the English one through `lunrLanguages` in `main.js`. Chinese puts no space between words, so lunr would index a whole Chinese sentence as one word and never find a word inside it. The Chinese site therefore replaces the theme's search worker with `templates/zh/public/search-worker.min.js`, which matches substrings and speaks the protocol the theme's search box expects. That protocol belongs to docfx's modern template and may change in any docfx release, which is why `build-docs.yml`, `release.yml` and the `Makefile` install one pinned docfx version. `check` warns when the built theme no longer mentions the worker's file name or its messages; test the Chinese search on a local build before raising the version.

What stays in English is what the reader copies or what is generated: code blocks, including the comments inside them, the SVG diagrams and the API reference. Everything else on a page is translated, headings, tables, link text and image descriptions included.

## Headings keep their English anchors

docfx derives a heading's id from its text, so a translated heading gets a different id than the English one, and a Chinese heading gets an id written in Chinese. Every link of the form `page.md#anchor` written against the English heading would then miss its section, and the language dropdown could not keep the reader at the same place. The translations therefore carry the English ids as well: `.github/scripts/docs-i18n.py stamp` puts an `<a id="english-id"></a>` line in front of each translated heading whose own id differs from the English one, the page title included.

At load time, `main.js` moves each of these ids onto the heading that follows it and drops the anchor, before the theme builds the "In this article" list and the heading links. A translated page then has exactly the ids of the English page; without JavaScript, the anchors alone still take a link to its section. Without that step the list would link to the translated ids, which the language dropdown cannot carry to another language, and a translated heading's own id could shadow the English id of a different heading: the Chinese title of the plugs page, `Plug`, gets the id `plug`, which is the English id of its `[Plug]` section.

`stamp` pairs the headings by position, which is why a translation must have exactly the headings of the English page: the same number, at the same levels, in the same order. A translator never writes these anchors by hand. Running `stamp` again on a file replaces the anchors it wrote before, the ones that carry an English heading id, and leaves any other anchor alone.

`stamp` computes the English ids the way docfx does rather than reading them from the built site, so that it works before a build. When the English site has been built, `check` compares the ids it computes with the ids docfx wrote, and reports a page whose headings use a syntax the script does not follow before a wrong anchor reaches a translation.

## The source hash

`stamp` also records which English page the translation was made from. It writes the git blob hash of the English page as `source_hash` in the translation's front matter, or on a `# source_hash:` comment line at the top of a translated `toc.yml`. The hash changes with any edit of the English page, so comparing it with the current English page tells whether the translation still follows it.

## Checking the translations

`python3 .github/scripts/docs-i18n.py check` walks every English page and reports, for each language, a translation that is missing, one that is stale (its `source_hash` no longer matches the English page), one that is orphaned (the English page is gone), and one whose shape no longer matches the English page: different headings, code blocks, link targets, inline code, table rows or paragraphs, a line left identical to an English line of six words or more, a dash the house style bans, or a toc entry that leaves the language's folder. A file it cannot read, such as one not saved as UTF-8, is reported like any other finding.

The docs workflow runs it after building the three sites. Each page with findings becomes one warning annotation on the workflow run, and the full list goes to the run's summary, since GitHub shows only the first ten annotations of a step. The step never fails the build, because an English change must not wait for someone who can write French and Chinese; the stale translation keeps being published, under its notice, until someone updates it. Locally, `--strict` makes it exit with an error, `--lang fr` limits it to one language, and a list of English paths (relative to `docs/`) limits it to those pages.

## When you change an English page

You are not required to update the translations in the same pull request. If you can, edit both translations so that they say what the English page now says, keeping the headings in step with it, then stamp them:

```bash
python3 .github/scripts/docs-i18n.py stamp docs/fr/articles/user/timers.md docs/zh/articles/user/timers.md
python3 .github/scripts/docs-i18n.py check articles/user/timers.md
```

A new English page goes in the `toc.yml` of its folder as usual. Until it is translated, `check` reports it as missing in each language and reports the translated `toc.yml` as different from the English one; the translated tables of contents only list it once the translation exists, since a translated build reports a missing page only at `info` level and would publish the entry as a broken link (`check` reports such an entry). A page that is renamed or deleted takes its translations with it, or `check` reports them as orphans.

The deploy keeps the files already published, because the released versions live next to the dev site on the Pages repository. A deleted translation therefore stays online, and the language dropdown, which finds a page by asking for its URL, still opens it; remove it from the Pages repository as well.

## Adding a language

A new language needs:

- a folder of `docs/` with its `docfx.json` (copy `docs/fr/docfx.json` and change the output folder, the template layer and `_lang`). The folder name must not look like a version folder (`v1.2.3`, `latest`), which `main.js` takes for a release;
- its folder in the `exclude` list of the root `docs/docfx.json`;
- a root `toc.yml` whose API entry reads `../api/`;
- a `templates/<folder>/token.json` with the theme strings;
- in `main.js`, an entry in the `languages` list, the notice text, and the lunr stemmer of the language in `lunrLanguages` when lunr has one;
- for a language written without spaces between words, a substring search worker such as `templates/zh/public/search-worker.min.js`, and in `docs-i18n.py` the check that a page holds text in that script, like the one for Chinese.

The `Makefile`, `build-docs.yml`, `release.yml` and `docs-i18n.py` find the languages by their `docs/<folder>/docfx.json`, so they need no change.

## Terms

The translations use one term for one concept across all pages. The table below lists the terms they settled on, so that a new or updated translation reads like the pages around it.

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
