---
title: Community
layout: default
parent: Lifecycle
nav_order: 4
---

# Community

Chat platforms, social mentions, announcements — the surfaces where the project meets its users.
The governing line for this whole area:

> **Reading, finding, and drafting are safe to automate. Writing publicly in the project's voice is
> the irreversible act that stays human (Band C) or autonomous-with-a-watchdog (Band A).**

A single misjudged public post can't be un-sent, and platforms ban bots that auto-reply. So the
system does all the *upstream* work — monitor, classify, dedupe, draft, queue — and keeps a human at
the public-voice membrane.

---

## Chat monitoring (Band A — read + capture only)
`chat-monitor` only reads allowed bug/feedback channels, normalizes each candidate with a stable
source id and target repository, and invokes the shared
[issue-capture contract](issue-lifecycle.md).
`issue-capture` owns the untrusted-data guard, vulnerability divert, classification, confidence,
dedupe, queue, ledger, reaction, and checkpoint. Chat must not implement a second capture path.
- This is the most injection-exposed input adapter, so it never obeys instruction-like text in a
  message.
- **It does not hold public conversations.** After durable staging, `issue-capture` may apply the
  chat-specific `community.chat.capture_reaction` so the reporter and maintainer can see it was
  logged. A suspected vulnerability receives no public mark. Replies in the project's voice stay
  human.
- Chat monitoring stays off until the shared capture core, private state, output index, security
  route, and any marker's watchdog are configured.
- Multi-product communities: route each channel to the *right* repo/queue. Don't capture reports for
  a product you don't own.

## Confidence-tiered capture → action (the safe way to auto-file)
`issue-capture` assigns the tier and stages the record; it never files. A downstream filing action
uses the tier because *filing an issue to the public tracker from a chat report* is a public write:
- **SECURITY** (checked first, before the tiers) → **never auto-file, never draft publicly.** A
  report that looks like a vulnerability — unauthorized access, data exposure, tampering, code
  execution, or attacker-triggerable service loss, by wording or by shape — diverts to the [private disclosure path](../reference/security-spine.md#6-the-vulnerability-divert):
  a PII-scrubbed summary through its complete fail-closed destination chain, a neutral private
  acknowledgement to the reporter, and **no public "captured" reaction**. Missing configuration
  suppresses the file and raises setup privately — it never falls back to the public tracker or a
  silent drop. Detection is high-recall on purpose: a false positive is a private glance, a false
  negative is an irreversible public leak.
- **HIGH** (a concrete, reproducible bug naming a specific surface, that passed dedup) → **auto-file**
  a privacy-safe issue. Two hard constraints: (1) the issue body is a **structured paraphrase, never
  the reporter's raw words**, with **no reporter identity** (handle, id, mention, message link,
  email) — run it through a **fail-closed privacy scrub** that blocks the file if any PII pattern is
  present ("no clean, no file"); (2) label it for triage and enforce
  `issue_capture.max_public_files_per_run` so a busy day can't spray the tracker. A value of `0`
  disables auto-file. Every auto-file is logged and
  [watchdog](../playbooks/watchdog-pattern.md)-verified.
- **MEDIUM** (actionable but vaguer, a feature request, or any doubt / scope call) → **draft it and
  surface to a human to approve** — don't file. The human's one-tap/one-word approval files it.
- **LOW / noise** → capture-to-queue only (or skip); never file, never ping.

The tiering is what lets you get the latency win (real bugs filed promptly) without the risk
(autonomously filing junk or leaking a reporter's identity). Feature requests and "does it do X?"
questions are scope/judgment calls — never auto-filed. And the SECURITY branch is why the *most*
concrete, reproducible reports — the ones the HIGH tier would file fastest — are exactly the ones
checked for a vulnerability first: a live exploit is the one public write you can never take back.

### Human-in-the-loop approval over chat
For the MEDIUM/LOW tiers, the confirmation channel has one non-obvious constraint worth knowing: a
**scheduled job can send a prompt but can't receive the answer** — it's detached from the live chat
connection. So wire approval as *notify-from-the-job, act-on-the-reply*: the job posts "Bug X queued
— reply to approve," and the human's **reply (a fresh live agent turn) does the filing.** Don't build
a job that waits for a response. Rich widgets like inline buttons can be nice, but a tap only acts if
the platform integration already handles it — a typed reply is the build-nothing path that always
works. Keep the durable audit trail of what got filed in an append-only log the
[watchdog](../playbooks/watchdog-pattern.md) re-verifies, not in a mutable state file.



## Mentions sweep (Band A — find + report only)
A scheduled sweep of the open web (forums, social, blogs, search) for discussion of the project,
with a disambiguation step (confirm it's *your* project, not a namesake). Output is a curated digest
to the maintainer — **find and report, never auto-reply.**
- The most useful *next* step (kept human): route confirmed-answerable mentions into a
  draft-reply queue — drafted by the agent, **sent by the human.** This converts passive monitoring
  into active support without any autonomous public posting.

## Announcements (Band A — narrow, templated, gated)
Release announcements are the one routine public write that's safe to automate, because the content
already exists (the changelog) and the action is templated and diff-gated:
- Compose from the changelog + release tags; post to a **fixed, hardcoded** channel via a
  secret-isolating helper (the bot token never enters the agent's context).
- Gate on a "last-announced" marker so it fires once per release, never re-posts.
- Anything beyond the templated announcement (general conversation, replies) stays human.

## The far horizon (deliberately staged)
The system *can* extend much further — drafting release posts for social, drafting replies to users
hitting known issues, aggregating sentiment. All of that is safe **as drafting** (Band B) → **human
sends** (Band C). The one place to resist full autonomy even with guards is **auto-posting public
replies in the project's voice** — keep it draft→approve. See
[anti-patterns](../reference/anti-patterns.md).

---

## Skills for this area
- `chat-monitor` — Watcher adapter: read channels, normalize, and invoke `issue-capture`.
- `mentions-sweep` — Watcher: web sweep, disambiguate, curate a digest.
- `release-announce` — Steward: templated announcement via a secret-isolating helper.

_Related: [issue lifecycle](issue-lifecycle.md) · [contributor recognition](contributor-recognition.md)
· [security spine](../reference/security-spine.md)._
