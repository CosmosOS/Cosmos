// Swap the diagram SVGs with their baked dark variants when the site theme is
// dark. The SVGs also carry a prefers-color-scheme media query for renderers
// we don't control (github.com), but browsers that pin the media query to
// light (LibreWolf's resistFingerprinting, older engines) would otherwise
// always show the light variant; keying on data-bs-theme follows the site
// toggle everywhere.
const diagramSrc = /^(.*\/images\/diagrams\/(?:gc|sched)-[a-z-]+?)(-dark)?\.svg$/;

function applyDiagramTheme() {
    const dark = document.documentElement.getAttribute('data-bs-theme') === 'dark';
    for (const img of document.querySelectorAll('img[src*="images/diagrams/"]')) {
        const m = img.src.match(diagramSrc);
        if (!m) {
            continue;
        }
        const src = m[1] + (dark ? '-dark' : '') + '.svg';
        if (img.src !== src) {
            img.src = src;
        }
    }
}

// The languages the site is built in. English is the site root; every other
// language is a complete docfx build in its own subfolder (docs/<folder>/
// docfx.json), next to the English pages of the same version. The lang
// attribute docfx writes on <html> (the _lang metadata) says which build the
// page belongs to.
const languages = [
    { lang: 'en', folder: '', label: 'English' },
    { lang: 'fr', folder: 'fr', label: 'Français' },
    { lang: 'zh-Hans', folder: 'zh', label: '简体中文' },
];

const currentLanguage = languages.find(l => l.lang === document.documentElement.lang) ?? languages[0];

// A translated page carries the English heading ids on empty anchors in front
// of its headings (.github/scripts/docs-i18n.py stamps them), each in a
// paragraph of its own once docfx renders it. Moving every id onto its heading
// gives the page the ids of the English one: the "In this article" list and
// the heading links the theme builds after start() then use them, a heading's
// own translated id can no longer shadow the English id of another heading,
// and the hash the language selector carries over always names a section the
// other language has. The theme jumps to the hash again once it has run.
// English pages are left alone: an anchor there is the author's, kept on
// purpose next to the heading's own id.
function adoptEnglishHeadingIds() {
    if (!currentLanguage.folder) {
        return;
    }
    for (const anchor of document.querySelectorAll('article p > a[id]:only-child:empty')) {
        const paragraph = anchor.parentElement;
        const heading = paragraph.nextElementSibling;
        if (paragraph.textContent.trim() !== '' || !heading || !/^H[1-6]$/.test(heading.tagName)) {
            continue;
        }
        heading.id = anchor.id;
        paragraph.remove();
    }
}

// Where the page sits. The site root is taken from the docfx:rel meta. It is
// the root of the page's language build, so on a translated page its last path
// segment is the language folder and the English root of the same version is
// one level up. Released docs are frozen by release.yml into vX.Y.Z/ subfolders
// next to the dev site, plus a latest/ alias, described by a versions.json at
// the outer root; when the English root's last segment is a version folder, the
// outer root (where versions.json and the sibling versions live) is one more
// level up.
const versionFolder = /^(v\d[^/]*|latest)$/;

function locate() {
    const rel = document.querySelector('meta[name="docfx:rel"]')?.content ?? '';
    const siteRoot = new URL(rel || './', location.href);
    const englishRoot = currentLanguage.folder ? new URL('../', siteRoot) : siteRoot;
    const segments = englishRoot.pathname.split('/').filter(s => s !== '');
    const folder = segments.length > 0 ? segments[segments.length - 1] : '';
    const inVersion = versionFolder.test(folder);
    return {
        pagePath: location.pathname.slice(siteRoot.pathname.length),
        englishRoot,
        version: inVersion ? folder : null,
        outerRoot: inVersion ? new URL('../', englishRoot) : englishRoot,
    };
}

function languageRoot(englishRoot, language) {
    return language.folder ? new URL(language.folder + '/', englishRoot) : englishRoot;
}

// Go to the first candidate that exists, the last one unprobed. The candidates
// keep the reader on the same page when the target has it, and fall back to a
// landing page when it does not: a page added after a release, or a release
// frozen before a language existed.
async function goToFirstExisting(candidates) {
    for (const url of candidates.slice(0, -1)) {
        try {
            const probe = await fetch(url, { method: 'HEAD' });
            if (probe.ok) {
                location.href = url;
                return;
            }
        } catch {
            // The next candidate is the fallback.
        }
    }
    location.href = candidates[candidates.length - 1];
}

// One slot next to #navbar holds both dropdowns, so that they keep their order
// whichever is ready first (the version one waits for versions.json).
function navbarSlot() {
    let slot = document.querySelector('.navbar-selects');
    if (!slot) {
        slot = document.createElement('div');
        slot.className = 'navbar-selects';
        document.getElementById('navbar')?.after(slot);
    }
    return slot;
}

// Back after a switch restores the previous page from the back-forward cache
// with the dropdown still showing the option picked on it, so each dropdown
// is set back to the page's own value whenever a page is shown again.
function resetSelectsOnRestore() {
    window.addEventListener('pageshow', event => {
        if (!event.persisted) {
            return;
        }
        for (const select of document.querySelectorAll('.navbar-selects select')) {
            select.value = select.dataset.current;
        }
    });
}

// A release frozen before a language existed has no folder for it, though its
// pages may offer the language (release.yml rebuilds an old tag with the
// current overlay). On a release page, the languages whose landing page is
// missing are disabled; the dev site always has all of them.
async function disableMissingLanguages(select, englishRoot) {
    for (const option of select.options) {
        const language = languages.find(l => l.lang === option.value);
        if (!language.folder || language === currentLanguage) {
            continue;
        }
        try {
            const probe = await fetch(languageRoot(englishRoot, language), { method: 'HEAD' });
            option.disabled = !probe.ok;
        } catch {
            option.disabled = true;
        }
    }
}

// Language selector. Every language has the English heading ids once
// adoptEnglishHeadingIds() has run, so the hash survives the switch.
function initLanguageSelector() {
    const { pagePath, englishRoot, version } = locate();
    const select = document.createElement('select');
    select.className = 'form-select';
    select.setAttribute('aria-label', 'Language');
    for (const language of languages) {
        const option = document.createElement('option');
        option.value = language.lang;
        option.lang = language.lang;
        option.textContent = language.label;
        if (language === currentLanguage) {
            option.setAttribute('selected', '');
        }
        select.append(option);
    }
    select.dataset.current = select.value;

    select.addEventListener('change', () => {
        const target = languages.find(l => l.lang === select.value) ?? languages[0];
        const root = languageRoot(englishRoot, target);
        goToFirstExisting([
            new URL(pagePath + location.hash, root).href,
            root.href,
            new URL(pagePath + location.hash, englishRoot).href,
            englishRoot.href,
        ]);
    });

    const container = document.createElement('div');
    container.className = 'language-select';
    container.append(select);
    navbarSlot().append(container);
    if (version !== null) {
        disableMissingLanguages(select, englishRoot);
    }
}

// A translated article says that the English page is the reference, and links
// to it. The API reference is generated in English in every language, so its
// pages get no notice.
const translationNotices = {
    'fr': {
        text: 'Cette page est une traduction de la version anglaise, qui fait foi.',
        link: 'Lire la version anglaise',
    },
    'zh-Hans': {
        text: '本页译自英文版，内容以英文版为准。',
        link: '阅读英文版',
    },
};

function addTranslationNotice() {
    const notice = translationNotices[currentLanguage.lang];
    const article = document.querySelector('article');
    const { pagePath, englishRoot } = locate();
    if (!notice || !article || pagePath.startsWith('api/')) {
        return;
    }
    const link = document.createElement('a');
    const target = () => new URL(pagePath + location.hash, englishRoot).href;
    link.href = target();
    window.addEventListener('hashchange', () => {
        link.href = target();
    });
    link.textContent = notice.link;
    const box = document.createElement('div');
    box.className = 'alert alert-info translation-notice';
    box.setAttribute('role', 'note');
    box.append(notice.text + ' ', link);
    article.prepend(box);
}

// Version selector. The dropdown only appears once versions.json exists, i.e.
// after the first tagged release. Switching keeps the page and its section,
// and the language when the target version was built in it.
async function initVersionSelector() {
    const { pagePath, version, outerRoot } = locate();

    let manifest;
    try {
        const res = await fetch(new URL('versions.json', outerRoot));
        if (!res.ok) {
            return;
        }
        manifest = await res.json();
    } catch {
        return;
    }
    if (!Array.isArray(manifest.versions) || manifest.versions.length === 0) {
        return;
    }

    const current = version === null ? 'dev'
        : version === 'latest' ? manifest.latest
        : version;
    const select = document.createElement('select');
    select.className = 'form-select';
    select.setAttribute('aria-label', 'Documentation version');
    for (const name of ['dev', ...manifest.versions]) {
        const option = document.createElement('option');
        option.value = name === 'dev' ? './' : name + '/';
        option.textContent = name === manifest.latest ? name + ' (latest)' : name;
        if (name === current) {
            option.setAttribute('selected', '');
        }
        select.append(option);
    }
    select.dataset.current = select.value;

    select.addEventListener('change', () => {
        const targetRoot = new URL(select.value, outerRoot);
        const page = pagePath + location.hash;
        const candidates = [];
        if (currentLanguage.folder) {
            const translatedRoot = languageRoot(targetRoot, currentLanguage);
            candidates.push(new URL(page, translatedRoot).href, translatedRoot.href);
        }
        candidates.push(new URL(page, targetRoot).href, targetRoot.href);
        goToFirstExisting(candidates);
    });

    const container = document.createElement('div');
    container.className = 'version-select';
    container.append(select);
    navbarSlot().prepend(container);
}

export default {
    // lunr's stemmer and stop words for the page's language, on top of the
    // English ones the identifiers and the API reference need.
    lunrLanguages: currentLanguage.lang === 'fr' ? ['en', 'fr'] : ['en'],
    start: () => {
        adoptEnglishHeadingIds();
        new MutationObserver(applyDiagramTheme)
            .observe(document.documentElement, { attributes: true, attributeFilter: ['data-bs-theme'] });
        applyDiagramTheme();
        addTranslationNotice();
        initLanguageSelector();
        initVersionSelector();
        resetSelectsOnRestore();
    }
}
