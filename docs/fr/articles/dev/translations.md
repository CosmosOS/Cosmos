---
source_hash: 764844b2f9b77dcefd260b00130e02ca26a487a6
---
<a id="translating-the-docs"></a>

# Traduire la documentation

La documentation est publiée en anglais, en français et en chinois simplifié, et les pages anglaises sont la référence que suivent les deux autres. Cette page explique comment une page traduite est organisée et générée, comment ses titres conservent les ancres anglaises, comment elle enregistre la page anglaise dont elle est issue, comment le build de la documentation signale une traduction qui a pris du retard, et ce que fait un contributeur quand une page anglaise change.

---

<a id="how-the-languages-are-laid-out"></a>

## Organisation des langues

Une langue est un dossier de `docs/` qui reproduit l'arborescence anglaise : `docs/fr/articles/user/timers.md` est la traduction française de `docs/articles/user/timers.md`, et `docs/zh/` contient la traduction chinoise au même chemin relatif. Comme les chemins sont les mêmes, un lien relatif tel que `drivers.md#platform-nodes` s'écrit de la même façon dans toutes les langues et mène à la page de sa propre langue.

Chaque dossier de langue est un projet docfx à part entière, avec un `docfx.json` qui le génère dans un sous-dossier du site anglais, `_site/fr/` et `_site/zh/`, et le `docs/docfx.json` racine exclut ces dossiers afin que le site anglais ne les publie pas une seconde fois. Une langue reprend les images et les vidéos de l'arborescence anglaise, si bien qu'une traduction référence `images/mouse-paint.mp4` exactement comme la page anglaise, et elle reprend la référence d'API générée dans `docs/api/`, publiée en anglais dans toutes les langues parce qu'elle provient de la documentation XML du code source. Par-dessus la couche commune `templates/custom`, une langue ajoute `templates/<folder>/token.json`, les chaînes propres au thème (la zone de recherche, « Dans cet article », les libellés des notes et des avertissements) dans cette langue, et définit `_lang`, que docfx écrit sur l'élément `<html>`.

Partager des fichiers avec l'arborescence anglaise a deux conséquences, car docfx cherche sur le disque le fichier que nomme une page ou une toc, à côté de cette page ou de cette toc, avant de placer les fichiers dans le site. L'entrée API d'un `toc.yml` racine traduit indique `../api/` là où l'anglais indique `api/`. Et dans un build traduit, chaque image et chaque vidéo, qui n'existe que dans l'arborescence anglaise, serait signalée comme `InvalidFileLink` alors même que le build la copie au bon endroit du site ; le `docfx.json` d'une langue abaisse donc cette règle au niveau `info`. `docs-i18n.py check` couvre ce que la règle ne signale plus : il exige qu'une traduction pointe exactement vers les cibles de sa page anglaise, que le build anglais valide, il exige que chaque entrée d'un `toc.yml` traduit désigne une page de sa propre langue, ou la référence d'API via `../api/`, et il signale chaque page anglaise qui n'a pas de traduction.

C'est par l'attribut `lang` que `templates/custom/public/main.js` sait dans quelle langue est une page. Le script ajoute à côté de la barre de navigation un menu déroulant des langues, qui ouvre la même page dans la langue choisie et se rabat sur la page d'accueil de cette langue quand la page n'y existe pas, et il place en tête de chaque article traduit un bandeau indiquant que la page anglaise fait référence, avec un lien vers elle. Le menu déroulant des versions conserve la page, sa section et sa langue quand la version choisie les possède. Une version figée avant l'arrivée d'une langue n'a pas de dossier pour cette langue : sur ses pages, le menu déroulant des langues la désactive, et le menu des versions d'une page traduite mène à la page anglaise de cette version.

Chaque langue a aussi son propre index de recherche, construit par docfx à partir de ses propres pages. Le thème y cherche avec lunr, qui découpe le texte en mots aux espaces et à la ponctuation ; le site français ajoute le stemmer français de lunr à l'anglais via `lunrLanguages` dans `main.js`. Le chinois ne met pas d'espace entre les mots, si bien que lunr indexerait une phrase chinoise entière comme un seul mot et n'y trouverait jamais un mot qu'elle contient. Le site chinois remplace donc le worker de recherche du thème par `templates/zh/public/search-worker.min.js`, qui recherche des sous-chaînes et parle le protocole qu'attend la zone de recherche du thème. Ce protocole appartient au modèle modern de docfx et peut changer à chaque version de docfx, c'est pourquoi `build-docs.yml`, `release.yml` et le `Makefile` installent une version de docfx épinglée. `check` avertit quand le thème généré ne mentionne plus le nom de fichier du worker ou ses messages ; testez la recherche chinoise sur un build local avant de monter de version.

Ce qui reste en anglais, c'est ce que le lecteur copie ou ce qui est généré : les blocs de code, commentaires compris, les diagrammes SVG et la référence d'API. Tout le reste d'une page est traduit, y compris les titres, les tableaux, les textes de lien et les descriptions d'images.

<a id="headings-keep-their-english-anchors"></a>

## Les titres gardent leurs ancres anglaises

docfx dérive l'identifiant d'un titre de son texte, si bien qu'un titre traduit reçoit un autre identifiant que le titre anglais, et qu'un titre chinois reçoit un identifiant écrit en chinois. Chaque lien de la forme `page.md#anchor` écrit d'après le titre anglais manquerait alors sa section, et le menu déroulant des langues ne pourrait pas garder le lecteur au même endroit. Les traductions portent donc aussi les identifiants anglais : `.github/scripts/docs-i18n.py stamp` place une ligne `<a id="english-id"></a>` devant chaque titre traduit dont l'identifiant propre diffère de l'identifiant anglais, titre de la page compris.

Au chargement, `main.js` reporte chacun de ces identifiants sur le titre qui le suit et supprime l'ancre, avant que le thème ne construise la liste « Dans cet article » et les liens des titres. Une page traduite a alors exactement les identifiants de la page anglaise ; sans JavaScript, les ancres suffisent à mener un lien à sa section. Sans cette étape, la liste pointerait vers les identifiants traduits, que le menu déroulant des langues ne peut pas transposer dans une autre langue, et l'identifiant propre d'un titre traduit pourrait masquer l'identifiant anglais d'un autre titre : le titre chinois de la page des plugs, `Plug`, reçoit l'identifiant `plug`, qui est l'identifiant anglais de sa section `[Plug]`.

`stamp` associe les titres par position, c'est pourquoi une traduction doit avoir exactement les titres de la page anglaise : le même nombre, aux mêmes niveaux, dans le même ordre. Un traducteur n'écrit jamais ces ancres à la main. Relancer `stamp` sur un fichier remplace les ancres qu'il avait écrites auparavant, celles qui portent un identifiant de titre anglais, et laisse toute autre ancre en place.

`stamp` calcule les identifiants anglais comme le fait docfx plutôt que de les lire dans le site généré, afin de fonctionner avant tout build. Quand le site anglais a été généré, `check` compare les identifiants qu'il calcule à ceux que docfx a écrits, et signale une page dont les titres emploient une syntaxe que le script ne suit pas avant qu'une ancre erronée n'atteigne une traduction.

<a id="the-source-hash"></a>

## Le hash de la source

`stamp` enregistre aussi la page anglaise dont la traduction est issue. Il écrit le hash de blob git de la page anglaise comme `source_hash` dans le front matter de la traduction, ou sur une ligne de commentaire `# source_hash:` en tête d'un `toc.yml` traduit. Le hash change à chaque modification de la page anglaise, si bien que le comparer à la page anglaise actuelle indique si la traduction la suit toujours.

Le README du dépôt est lui aussi traduit, dans `README_FR.md` et `README_CN.md` placés à côté de lui, vers lesquels mène la ligne de liens de langues sous les badges de chaque README. Une traduction du README suit `README.md` paragraphe par paragraphe, mais ses liens mènent à la documentation de sa propre langue (`https://cosmosos.github.io/fr/...`) et les libellés de ses badges sont traduits, si bien que `check` ne compare pas sa forme à celle de `README.md`. `stamp README_FR.md` écrit le hash de `README.md` dans un commentaire `<!-- source_hash: ... -->` sur sa première ligne, que GitHub n'affiche pas, et `check` signale une traduction du README manquante, non estampillée ou périmée. Le workflow de la documentation se lance aussi quand un README change, afin qu'une traduction du README périmée apparaisse dans son résumé.

<a id="checking-the-translations"></a>

## Vérifier les traductions

`python3 .github/scripts/docs-i18n.py check` parcourt chaque page anglaise et signale, pour chaque langue, une traduction manquante, une traduction périmée (son `source_hash` ne correspond plus à la page anglaise), une traduction orpheline (la page anglaise n'existe plus) et une traduction dont la forme ne correspond plus à la page anglaise : des titres, des blocs de code, des cibles de liens, du code inline, des lignes de tableau ou des paragraphes différents, une ligne restée identique à une ligne anglaise de six mots ou plus, un tiret que la charte de rédaction interdit, ou une entrée de toc qui sort du dossier de la langue. Un fichier qu'il ne peut pas lire, par exemple un fichier qui n'est pas enregistré en UTF-8, est signalé comme n'importe quel autre constat.

Le workflow de la documentation le lance après avoir généré les trois sites. Chaque page qui a des constats devient une annotation d'avertissement sur l'exécution du workflow, et la liste complète va dans le résumé de l'exécution, puisque GitHub n'affiche que les dix premières annotations d'une étape. L'étape ne fait jamais échouer le build, parce qu'une modification en anglais ne doit pas attendre quelqu'un qui sait écrire le français et le chinois ; la traduction périmée continue d'être publiée, sous son bandeau, jusqu'à ce que quelqu'un la mette à jour. En local, `--strict` le fait sortir avec une erreur, `--lang fr` le limite à une langue, et une liste de chemins anglais (relatifs à `docs/`, ou `README.md`) le limite à ces pages.

<a id="when-you-change-an-english-page"></a>

## Quand vous modifiez une page anglaise

Vous n'êtes pas tenu de mettre à jour les traductions dans la même pull request. Si vous le pouvez, modifiez les deux traductions pour qu'elles disent ce que dit désormais la page anglaise, en gardant leurs titres alignés sur les siens, puis estampillez-les :

```bash
python3 .github/scripts/docs-i18n.py stamp docs/fr/articles/user/timers.md docs/zh/articles/user/timers.md
python3 .github/scripts/docs-i18n.py check articles/user/timers.md
```

Une nouvelle page anglaise va dans le `toc.yml` de son dossier, comme d'habitude. Tant qu'elle n'est pas traduite, `check` la signale comme manquante dans chaque langue et signale le `toc.yml` traduit comme différent de l'anglais ; les tables des matières traduites ne la listent qu'une fois la traduction faite, puisqu'un build traduit ne signale une page absente qu'au niveau `info` et publierait l'entrée comme un lien cassé (`check` signale une telle entrée). Une page renommée ou supprimée emporte ses traductions avec elle, sinon `check` les signale comme orphelines.

Le déploiement conserve les fichiers déjà publiés, parce que les versions publiées sont rangées à côté du site de développement dans le dépôt Pages. Une traduction supprimée reste donc en ligne, et le menu déroulant des langues, qui trouve une page en demandant son URL, l'ouvre encore ; supprimez-la aussi du dépôt Pages.

<a id="adding-a-language"></a>

## Ajouter une langue

Une nouvelle langue demande :

- un dossier de `docs/` avec son `docfx.json` (copiez `docs/fr/docfx.json` et changez le dossier de sortie, la couche de modèle et `_lang`). Le nom du dossier ne doit pas ressembler à un dossier de version (`v1.2.3`, `latest`), que `main.js` prend pour une version publiée ;
- son dossier dans la liste `exclude` du `docs/docfx.json` racine ;
- un `toc.yml` racine dont l'entrée API indique `../api/` ;
- un `templates/<folder>/token.json` avec les chaînes du thème ;
- dans `main.js`, une entrée dans la liste `languages`, le texte du bandeau, et le stemmer lunr de la langue dans `lunrLanguages` quand lunr en a un ;
- pour une langue qui s'écrit sans espace entre les mots, un worker de recherche par sous-chaîne comme `templates/zh/public/search-worker.min.js`, et dans `docs-i18n.py` la vérification qu'une page contient du texte dans cette écriture, comme celle du chinois ;
- une traduction du README à côté de `README.md`, son nom de fichier dans `READMES` dans `docs-i18n.py`, et son lien dans la ligne de langues de chaque README.

Le `Makefile`, `build-docs.yml` et `release.yml` trouvent les langues par leur `docs/<folder>/docfx.json`, tout comme `docs-i18n.py` pour les pages, si bien qu'ils n'ont besoin d'aucune autre modification.

<a id="terms"></a>

## Termes

Les traductions emploient un seul terme pour un même concept sur toutes les pages. Le tableau ci-dessous liste les termes retenus, afin qu'une traduction nouvelle ou mise à jour se lise comme les pages qui l'entourent.

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
