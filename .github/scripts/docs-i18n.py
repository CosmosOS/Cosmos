#!/usr/bin/env python3
"""
Keep the translated documentation (docs/<lang>/) in step with the English pages.

  stamp FILE...     record the git hash of the English source in each translated
                    file, and put the English heading ids in front of its
                    translated headings, so that links of the form page.md#anchor
                    and the language switcher land on the same section in every
                    language
  check [PATH...]   report translations that are missing, stale (the English page
                    changed since the translation was stamped), orphaned (the
                    English page is gone) or no longer shaped like the English page

A language is a folder of docs/ holding a docfx.json. The English page of
docs/<lang>/<path> is docs/<path>. check exits 0 unless --strict is given, so the
docs CI reports drift without failing on it. When the site has been built
(docs/_site), check also compares the heading ids it computes with the ids
docfx wrote, and the theme's search protocol with the one a language's own
search worker speaks.

The repository README has translations too, at the root (READMES below).
stamp records the hash of README.md on a comment on their first line, which
GitHub does not display, and check reports a README translation that is
missing, not stamped or stale. Their shape is not compared: their links lead
to the documentation of their own language, and their badge labels are
translated.
"""
import argparse
import collections
import html
import os
import re
import subprocess
import sys
import unicodedata

DOCS = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "docs"))
SITE = os.path.join(DOCS, "_site")
ROOT = os.path.dirname(DOCS)

# The translations of the repository README, by language folder, and the
# hidden comment that carries their source hash.
README = os.path.join(ROOT, "README.md")
READMES = {"fr": "README_FR.md", "zh": "README_CN.md"}
README_HASH = re.compile(r"\A<!-- source_hash: (\w+) -->\n")

# Folders of docs/ that hold no English page to translate: generated output,
# the generated API reference (published in English in every language), and
# the templates.
NOT_SOURCES = {"_site", "api", "templates", "images"}

# A fence may be indented: inside a list item the block is indented to the
# item's content, four spaces or more.
FENCE = re.compile(r"^\s*(`{3,}|~{3,})(.*)$")
HEADING = re.compile(r"^ {0,3}(#{1,6})(?:[ \t]+(.*?))?(?:[ \t]+#+)?[ \t]*$")
ANCHOR_LINE = re.compile(r'^<a id="([^"]*)"></a>$')
LINK_TARGET = re.compile(r'!?\[(?:[^\[\]]|\[[^\]]*\])*\]\(\s*<?([^)\s>]+)>?(?:\s+"[^"]*")?\s*\)')
REFERENCE_DEFINITION = re.compile(r"^ {0,3}\[[^\]]+\]:\s*<?(\S+?)>?(?:\s|$)")
HTML_TARGET = re.compile(r'\b(?:href|src)\s*=\s*"([^"]*)"')
AUTOLINK = re.compile(r"<(https?://[^>\s]+)>")
INLINE_CODE = re.compile(r"(`+)(.+?)\1")
HAN = re.compile(r"[\u3000-\u303f\u4e00-\u9fff\uff00-\uffef]")
WORD = re.compile(r"[A-Za-z]{2,}")
SITE_HEADING = re.compile(r'<h[1-6][^>]*\bid="([^"]*)"')

# The theme's search box and its worker exchange these messages
# (public/search.min.js in docfx's modern template). A language that replaces
# the worker (templates/<lang>/public/search-worker.min.js) speaks them too.
SEARCH_PROTOCOL = ("search-worker.min.js", "index-ready", "query-ready")


class Unreadable(Exception):
    pass


def languages():
    return sorted(
        name for name in os.listdir(DOCS)
        if os.path.isfile(os.path.join(DOCS, name, "docfx.json")))


def english_sources(langs):
    """Paths, relative to docs/, of every English page and toc a language mirrors."""
    skip = NOT_SOURCES | set(langs)
    sources = []
    for root, dirs, files in os.walk(DOCS):
        rel_root = os.path.relpath(root, DOCS)
        if rel_root == ".":
            dirs[:] = [d for d in dirs if d not in skip]
        for name in files:
            if name.endswith(".md") or name == "toc.yml":
                sources.append(os.path.normpath(os.path.join(rel_root, name)))
    return sorted(sources)


def translated_files(lang):
    base = os.path.join(DOCS, lang)
    found = []
    for root, dirs, files in os.walk(base):
        dirs[:] = [d for d in dirs if d not in NOT_SOURCES]
        for name in files:
            if name.endswith(".md") or name == "toc.yml":
                found.append(os.path.relpath(os.path.join(root, name), base))
    return sorted(found)


def read(path):
    # utf-8-sig: an editor that saves a byte order mark must not hide the
    # front matter, which has to be the first thing in the file.
    try:
        with open(path, encoding="utf-8-sig") as f:
            return f.read().replace("\r\n", "\n")
    except (OSError, UnicodeDecodeError) as e:
        raise Unreadable(f"cannot read {os.path.relpath(path, DOCS)}: {e}") from e


def write(path, text):
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write(text)


def git_hash(path):
    """The blob hash git stores for the file (line endings normalized, like the checkout)."""
    try:
        return subprocess.run(
            ["git", "hash-object", path], cwd=DOCS, check=True,
            capture_output=True, text=True).stdout.strip()
    except (OSError, subprocess.CalledProcessError) as e:
        raise Unreadable(f"git hash-object failed on {os.path.relpath(path, DOCS)}: {e}") from e


# ---------------------------------------------------------------------------
# Front matter (Markdown) and the hash comment (toc.yml)

def split_front_matter(text):
    """Return (front matter lines, body) for a Markdown page."""
    if text.startswith("---\n---\n"):
        return [], text[8:]
    if text.startswith("---\n"):
        end = text.find("\n---\n", 3)
        if end != -1:
            return text[4:end].split("\n"), text[end + 5:]
    return [], text


def front_matter_length(lines):
    """Lines the front matter takes at the top of the file."""
    return len(lines) + 2 if lines else 0


def join_front_matter(lines, body):
    if not lines:
        return body
    return "---\n" + "\n".join(lines) + "\n---\n" + body


def stored_hash(rel, text):
    if rel.endswith(".yml"):
        m = re.match(r"# source_hash: (\w+)\n", text)
        return m.group(1) if m else None
    lines, _ = split_front_matter(text)
    for line in lines:
        if line.startswith("source_hash:"):
            return line.split(":", 1)[1].strip()
    return None


def with_hash(rel, text, sha):
    if rel.endswith(".yml"):
        text = re.sub(r"\A# source_hash: \w+\n", "", text)
        return f"# source_hash: {sha}\n" + text
    lines, body = split_front_matter(text)
    lines = [line for line in lines if not line.startswith("source_hash:")]
    return join_front_matter(lines + [f"source_hash: {sha}"], body)


# ---------------------------------------------------------------------------
# Markdown structure

def prose_lines(body):
    """Yield (index, line, in_code) for the body, tracking fenced code blocks."""
    fence = None
    for i, line in enumerate(body.split("\n")):
        m = FENCE.match(line)
        if fence is None and m:
            fence = m.group(1)
            yield i, line, True
        elif fence is not None:
            if m and m.group(1)[0] == fence[0] and len(m.group(1)) >= len(fence) and not m.group(2).strip():
                fence = None
            yield i, line, True
        else:
            yield i, line, False


def code_blocks(body):
    blocks, current = [], []
    for _, line, in_code in prose_lines(body):
        if in_code:
            current.append(line)
        elif current:
            blocks.append("\n".join(current))
            current = []
    if current:
        blocks.append("\n".join(current))
    return blocks


def heading_text(inline):
    """The plain text docfx renders for a heading's inline Markdown."""
    text = INLINE_CODE.sub(lambda m: m.group(2).strip(), inline)
    text = re.sub(r"!\[([^\]]*)\]\([^)]*\)", r"\1", text)
    text = re.sub(r"\[([^\]]*)\]\([^)]*\)", r"\1", text)
    text = re.sub(r"\[([^\]]*)\]\[[^\]]*\]", r"\1", text)
    text = AUTOLINK.sub(r"\1", text)
    text = re.sub(r"<[^>]+>", "", text)
    text = re.sub(r"\\(.)", r"\1", text)
    text = re.sub(r"\*+", "", text)
    text = re.sub(r"(?<![A-Za-z0-9])_+|_+(?![A-Za-z0-9])", "", text)
    return html.unescape(text).strip()


def is_letter_or_digit(c):
    # .NET char.IsLetterOrDigit works on UTF-16 code units, so a character
    # outside the Basic Multilingual Plane (two surrogates) is never one.
    if ord(c) > 0xFFFF:
        return False
    category = unicodedata.category(c)
    return category.startswith("L") or category == "Nd"


def slug(text):
    """docfx's heading id: markdig's GitHub-style auto identifier."""
    return "".join(
        "-" if c == " " else c.lower()
        for c in text
        if is_letter_or_digit(c) or c in " -_")


def headings(body):
    """[(line index, level, id)] for the ATX headings of a page body, ids deduplicated like markdig."""
    found, seen = [], set()
    for i, line, in_code in prose_lines(body):
        if in_code:
            continue
        m = HEADING.match(line)
        if not m:
            continue
        base = slug(heading_text(m.group(2) or "")) or "section"
        hid, n = base, 0
        while hid in seen:
            n += 1
            hid = f"{base}-{n}"
        seen.add(hid)
        found.append((i, len(m.group(1)), hid))
    return found


def strip_code_spans(line):
    return INLINE_CODE.sub(" ", line)


def link_targets(body):
    targets = collections.Counter()
    for _, line, in_code in prose_lines(body):
        if in_code:
            continue
        line = strip_code_spans(line)
        for rx in (LINK_TARGET, HTML_TARGET, AUTOLINK):
            targets.update(rx.findall(line))
        m = REFERENCE_DEFINITION.match(line)
        if m:
            targets[m.group(1)] += 1
    return targets


def inline_code(body):
    spans = collections.Counter()
    for _, line, in_code in prose_lines(body):
        if not in_code:
            spans.update(m.group(2).strip() for m in INLINE_CODE.finditer(line))
    return spans


def remove_anchors(body, ids):
    """Drop the anchor lines stamp writes in front of headings: those carrying one of
    the English page's heading ids. Returns the body and, for each line kept, its index
    in the original body. Any other anchor is the author's and stays."""
    lines = body.split("\n")
    in_code = {i for i, _, code in prose_lines(body) if code}
    kept, origin, i = [], [], 0
    while i < len(lines):
        m = ANCHOR_LINE.match(lines[i]) if i not in in_code else None
        if m and m.group(1) in ids:
            j = i + 1
            while j < len(lines) and not lines[j].strip():
                j += 1
            if j < len(lines) and HEADING.match(lines[j]):
                i = j
                continue
        kept.append(lines[i])
        origin.append(i)
        i += 1
    return "\n".join(kept), origin


def english_ids(english_text):
    _, body = split_front_matter(english_text)
    return {hid for _, _, hid in headings(body)}


# ---------------------------------------------------------------------------
# stamp

def source_of(path):
    """(lang, path relative to docs/) of the English page a translated file mirrors."""
    rel = os.path.relpath(os.path.abspath(path), DOCS)
    parts = rel.split(os.sep)
    if len(parts) < 2 or parts[0] not in languages():
        raise SystemExit(f"{path}: not under docs/<lang>/")
    if not (rel.endswith(".md") or parts[-1] == "toc.yml"):
        raise SystemExit(f"{path}: only Markdown pages and toc.yml files are stamped")
    return parts[0], os.path.join(*parts[1:])


def readme_translations():
    return {os.path.join(ROOT, name) for name in READMES.values()}


def stamp_readme(path):
    text = README_HASH.sub("", read(path))
    write(path, f"<!-- source_hash: {git_hash(README)} -->\n" + text)
    print(f"stamped {os.path.basename(path)}")


def stamp(path):
    if os.path.abspath(path) in readme_translations():
        stamp_readme(path)
        return
    lang, rel = source_of(path)
    source = os.path.join(DOCS, rel)
    if not os.path.isfile(source):
        raise SystemExit(f"{path}: no English page at docs/{rel}")
    text = read(path)
    if rel.endswith(".md"):
        english_text = read(source)
        lines, body = split_front_matter(text)
        body, _ = remove_anchors(body, english_ids(english_text))
        _, english_body = split_front_matter(english_text)
        english, translated = headings(english_body), headings(body)
        if [lvl for _, lvl, _ in english] != [lvl for _, lvl, _ in translated]:
            raise SystemExit(f"{path}: headings do not match docs/{rel} one for one; run check on it")
        out = list[str](body.split("\n"))
        for (_, _, en_id), (index, _, own_id) in reversed(list(zip(english, translated))):
            if en_id == own_id:
                continue
            # The anchor must be a paragraph of its own: right after a line
            # of text it would join that paragraph, and main.js would not
            # find it in front of the heading.
            anchor = [f'<a id="{en_id}"></a>', ""]
            if index > 0 and out[index - 1].strip():
                anchor.insert(0, "")
            out[index:index] = anchor
        text = join_front_matter(lines, "\n".join(out))
    write(path, with_hash(rel, text, git_hash(source)))
    print(f"stamped docs/{lang}/{rel}")


# ---------------------------------------------------------------------------
# check

def compare_toc(rel, english_text, translated_text, lang):
    """A translated toc lists the pages of its own language, in the English order.
    It reaches the shared API reference as ../api/, since docfx resolves a toc's
    folder reference on disk next to the toc, before it maps files into the site."""
    problems = []
    href = re.compile(r"^\s*-?\s*(?:href|topicHref):\s*(\S+)", re.M)

    def resolve(root, target):
        return os.path.normpath(os.path.join(root, os.path.dirname(rel), target))

    english = [t if "://" in t else os.path.relpath(resolve(DOCS, t), DOCS) for t in href.findall(english_text)]
    lang_root = os.path.join(DOCS, lang)
    api_root = os.path.join(DOCS, "api")
    translated = []
    for target in href.findall(translated_text):
        if "://" in target:
            translated.append(target)
            continue
        path = resolve(lang_root, target)
        if path == api_root or path.startswith(api_root + os.sep):
            translated.append(os.path.relpath(path, DOCS))
        elif path.startswith(lang_root + os.sep):
            translated.append(os.path.relpath(path, lang_root))
            if not os.path.exists(path):
                problems.append(f"toc entry {target} names a page that does not exist in docs/{lang}/")
        else:
            translated.append(None)
            problems.append(f"toc entry {target} points outside docs/{lang}/ (the API reference is ../api/)")
    if [t for t in translated if t is not None] != english:
        problems.append("toc entries differ from the English toc")
    return problems


def compare(rel, english_text, translated_text, lang):
    """Structural differences between an English page and its translation, as
    (message, file line or None) pairs."""
    if rel.endswith(".yml"):
        return [(message, None) for message in compare_toc(rel, english_text, translated_text, lang)]

    problems = []
    ids = english_ids(english_text)
    en_lines, en_body = split_front_matter(english_text)
    tr_lines, tr_body = split_front_matter(translated_text)
    en_body, _ = remove_anchors(en_body, ids)
    tr_body, origin = remove_anchors(tr_body, ids)
    offset = front_matter_length(tr_lines)

    def file_line(index):
        return offset + origin[index] + 1

    if en_lines != [line for line in tr_lines if not line.startswith("source_hash:")]:
        problems.append(("front matter differs from the English page", 1))

    en_levels = [lvl for _, lvl, _ in headings(en_body)]
    tr_levels = [lvl for _, lvl, _ in headings(tr_body)]
    if en_levels != tr_levels:
        problems.append((f"headings differ: {len(en_levels)} in English, {len(tr_levels)} here, or not at the same levels", None))

    en_code, tr_code = code_blocks(en_body), code_blocks(tr_body)
    if en_code != tr_code:
        changed = sum(1 for a, b in zip(en_code, tr_code) if a != b) + abs(len(en_code) - len(tr_code))
        problems.append((f"code blocks differ from the English page ({changed} of {len(en_code)})", None))

    en_links, tr_links = link_targets(en_body), link_targets(tr_body)
    if en_links != tr_links:
        missing = sorted((en_links - tr_links).elements())
        extra = sorted((tr_links - en_links).elements())
        problems.append((f"link targets differ: missing {missing[:5]}, extra {extra[:5]}", None))

    en_spans, tr_spans = inline_code(en_body), inline_code(tr_body)
    if set(en_spans) != set(tr_spans):
        missing = sorted(set(en_spans) - set(tr_spans))
        extra = sorted(set(tr_spans) - set(en_spans))
        problems.append((f"inline code differs: missing {missing[:5]}, extra {extra[:5]}", None))

    def table_rows(body):
        return sum(1 for _, line, code in prose_lines(body) if not code and line.lstrip().startswith("|"))
    if table_rows(en_body) != table_rows(tr_body):
        problems.append(("table rows differ from the English page", None))

    # A paragraph, list or table dropped or merged into its neighbor. Counted by
    # blank-line separated blocks, since a translation may join the lines of a
    # hard-wrapped English paragraph. A heading is a block of its own, as stamp
    # may put a blank line above it.
    def prose_blocks(body):
        count, in_block = 0, False
        for _, line, code in prose_lines(body):
            if not code and HEADING.match(line):
                count, in_block = count + 1, False
                continue
            starts = not code and bool(line.strip())
            if starts and not in_block:
                count += 1
            in_block = starts
        return count
    if prose_blocks(en_body) != prose_blocks(tr_body):
        problems.append((f"paragraphs differ: {prose_blocks(en_body)} in English, {prose_blocks(tr_body)} here", None))

    def readable_words(line):
        """The words a reader sees on the line: link targets, tags and code spans removed."""
        line = strip_code_spans(line)
        line = re.sub(r"\]\([^)]*\)", "]", line)
        line = re.sub(r"<[^>]*>", " ", line)
        line = AUTOLINK.sub(" ", line)
        return WORD.findall(line)

    # A whole line identical to an English line of six words or more. Lines that
    # already mix languages in the English page (the term table of
    # translations.md) legitimately stay as they are in every translation.
    english_prose = {
        line.strip() for _, line, code in prose_lines(en_body)
        if not code and not HAN.search(line)}
    for i, line, code in prose_lines(tr_body):
        if not code and line.strip() in english_prose and len(readable_words(line)) >= 6:
            problems.append(("line left in English", file_line(i)))

    for i, line, code in prose_lines(tr_body):
        if not code and re.search(r"\u2014|\u2015| \u2013 ", strip_code_spans(line)):
            problems.append(("dash the house style bans", file_line(i)))

    if lang.startswith("zh") and not HAN.search(tr_body):
        problems.append(("no Chinese text", None))
    return problems


def site_ids(rel):
    """The heading ids docfx wrote for an English page, or None when the site is not built."""
    page = os.path.join(SITE, rel[:-3] + ".html")
    if not os.path.isfile(page):
        return None
    text = read(page)
    article = text[text.find("<article"):text.find("</article>")]
    return SITE_HEADING.findall(article)


def search_protocol_problems():
    """A language that replaces the theme's search worker relies on the theme's
    messages; report a built theme that no longer sends them."""
    problems = []
    for lang in languages():
        worker = os.path.join(DOCS, "templates", lang, "public", "search-worker.min.js")
        theme = os.path.join(SITE, lang, "public", "search.min.js")
        if not os.path.isfile(worker) or not os.path.isfile(theme):
            continue
        text = read(theme)
        missing = [token for token in SEARCH_PROTOCOL if token not in text]
        if missing:
            problems.append((lang, f"the theme's search.min.js no longer mentions {missing}: "
                                   f"templates/{lang}/public/search-worker.min.js must follow its new protocol"))
    return problems


def check(args):
    langs = [args.lang] if args.lang else languages()
    github = os.environ.get("GITHUB_ACTIONS") == "true" and not args.strict
    findings = []

    sources = english_sources(languages())
    scoped = bool(args.paths)
    readme_wanted = not scoped
    wanted = set()
    for path in args.paths:
        if os.path.abspath(path) in readme_translations() | {README}:
            readme_wanted = True
            continue
        rel = os.path.normpath(path).removeprefix("docs" + os.sep)
        head = rel.split(os.sep)[0]
        if head in languages():
            rel = rel[len(head) + 1:]
        if rel not in sources:
            raise SystemExit(f"{path}: no English page docs/{rel} (give paths relative to docs/)")
        wanted.add(rel)

    def report(shown, message, line=None):
        findings.append((shown, line, message))

    for lang in langs:
        for rel in sources:
            if scoped and rel not in wanted:
                continue
            shown = f"docs/{lang}/{rel}"
            translated = os.path.join(DOCS, lang, rel)
            if not os.path.isfile(translated):
                report(shown, f"missing translation of docs/{rel}")
                continue
            try:
                english_text, translated_text = read(os.path.join(DOCS, rel)), read(translated)
                if not args.no_hash:
                    sha = stored_hash(rel, translated_text)
                    if sha is None:
                        report(shown, "not stamped (no source_hash)")
                    elif sha != git_hash(os.path.join(DOCS, rel)):
                        report(shown, f"stale: docs/{rel} changed since this translation")
                for message, line in compare(rel, english_text, translated_text, lang):
                    report(shown, message, line)
            except Unreadable as e:
                report(shown, str(e))
        if not scoped:
            for rel in translated_files(lang):
                if rel not in sources:
                    report(f"docs/{lang}/{rel}", f"orphan: docs/{rel} does not exist")
        name = READMES.get(lang)
        if readme_wanted and name is not None:
            translated = os.path.join(ROOT, name)
            if not os.path.isfile(translated):
                report(name, "missing translation of README.md")
            elif not args.no_hash:
                try:
                    m = README_HASH.match(read(translated))
                    if m is None:
                        report(name, "not stamped (no source_hash comment on the first line)")
                    elif m.group(1) != git_hash(README):
                        report(name, "stale: README.md changed since this translation")
                except Unreadable as e:
                    report(name, str(e))

    # The anchors stamp writes are only as good as the ids computed here.
    for rel in sources:
        if rel.endswith(".md") and (not scoped or rel in wanted):
            built = site_ids(rel)
            if built is not None:
                _, body = split_front_matter(read(os.path.join(DOCS, rel)))
                ours = [hid for _, _, hid in headings(body)]
                if ours != built:
                    differ = [f"{a} / {b}" for a, b in zip(ours, built) if a != b][:3]
                    report(f"docs/{rel}", f"docs-i18n.py finds {len(ours)} heading ids where docfx wrote "
                                          f"{len(built)}, first differences (ours / docfx): {differ}; "
                                          "fix headings() before stamping")
    for lang, message in search_protocol_problems():
        report(f"docs/templates/{lang}/public/search-worker.min.js", message)

    # GitHub keeps ten annotations per step: one per page, and the full list in
    # the step summary.
    by_page = collections.defaultdict(list)
    for shown, line, message in findings:
        by_page[shown].append(f"{message} (line {line})" if line else message)
    for shown, messages in by_page.items():
        if github:
            print(f"::warning file={shown}::{'; '.join(messages)}")
        else:
            for message in messages:
                print(f"{shown}: {message}")
    summary = os.environ.get("GITHUB_STEP_SUMMARY")
    if summary and findings:
        with open(summary, "a", encoding="utf-8") as f:
            f.write(f"### Translations: {len(findings)} problem(s)\n\n| File | Line | Problem |\n|---|---|---|\n")
            for shown, line, message in findings:
                f.write(f"| `{shown}` | {line or ''} | {message.replace('|', '/')} |\n")

    count = len(findings)
    print(f"{count} translation problem(s)" if count else "translations in step with the English pages")
    return 1 if args.strict and count else 0


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    sub = parser.add_subparsers(dest="command", required=True)
    p_stamp = sub.add_parser("stamp", help="stamp translated files after translating or updating them")
    p_stamp.add_argument("files", nargs="+")
    p_check = sub.add_parser("check", help="report missing, stale, orphan and misshapen translations")
    p_check.add_argument("paths", nargs="*", help="English pages to check, relative to docs/ (default: all)")
    p_check.add_argument("--lang", help="check one language only")
    p_check.add_argument("--no-hash", action="store_true", help="skip the source_hash comparison")
    p_check.add_argument("--strict", action="store_true", help="exit 1 when there is a problem")
    p_heads = sub.add_parser("ids", help="print the heading ids docfx gives a page (for debugging)")
    p_heads.add_argument("file")
    args = parser.parse_args()

    try:
        if args.command == "stamp":
            for path in args.files:
                stamp(path)
            return 0
        if args.command == "ids":
            _, body = split_front_matter(read(args.file))
            for _, level, hid in headings(body):
                print(level, hid)
            return 0
        return check(args)
    except Unreadable as e:
        raise SystemExit(str(e)) from e


if __name__ == "__main__":
    sys.exit(main())
