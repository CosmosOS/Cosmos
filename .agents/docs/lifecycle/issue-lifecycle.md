---
title: Issue lifecycle
layout: default
parent: Lifecycle
nav_order: 1
---

# Issue lifecycle

How a problem travels from "someone mentioned it somewhere" to "filed, triaged, fixed, closed with
credit." The issue tracker is the project's memory; this playbook keeps it accurate and keeps
reporters feeling heard.

```
 [conception]  a vague report on chat / the web
     │  Watcher: capture to a staging queue, dedupe, classify
     ▼
 [capture queue]  pre-tracker staging
     │  Builder/human: investigate, reproduce, draft a clean issue
     ▼
 [filed]  a real tracker issue
     │  Steward: initial reply, label, milestone, "sprint-candidate" if a fix is starting
     ▼
 [triaged]
     │  serious + nobody building it? → Builder opens a PR (enters the PR lifecycle)
     ▼
 [fix merged + released]
     │  Watcher/Steward: confirm fully resolved, close with version + credit
     ▼
 [closed with credit]
```

---

## Capture (Band A)
The Watcher reads inbound channels, classifies each item (bug / feature / question / noise), assigns
a HIGH / MEDIUM / LOW confidence tier, and captures genuinely new actionable items to a staging
queue. Capture itself never files a tracker issue; the tier controls the downstream filing gate.
Two rules make capture trustworthy:
- **Dedupe across every surface before capturing** — the tracker (open *and* closed), the staging
  queue, and the capture ledger. The same report arriving twice should match an existing reference,
  not create a duplicate.
- **Mark what you captured.** Leave a lightweight, visible signal (a "captured" reaction/ack) so the
  reporter and the maintainer can see it was logged — without the agent writing a full public reply
  (keep public voice human; see [community](community.md)). **Exception — a suspected vulnerability
  gets no public mark.** Run the [vulnerability divert](../reference/security-spine.md#6-the-vulnerability-divert)
  *before* reacting: on a hit, leave no public reaction (a visible mark is itself a partial
  disclosure) — the reporter gets only the neutral private acknowledgement on the disclosure path.
  Every autonomous normal-capture mark is ledgered and independently
  [watchdog-verified](../playbooks/watchdog-pattern.md).

## Investigate & file (Band B → C)
Before any tier decision, apply the [vulnerability divert](../reference/security-spine.md#6-the-vulnerability-divert):
a suspected vulnerability (unauthorized access, data exposure, tampering, code execution, or
attacker-triggerable service loss — by wording or shape) is never filed to the public tracker. It
goes to the private disclosure path; a human decides disclosure. If no private destination is
configured, it is still suppressed — never a fall-back to public-filing.

Before a staging item becomes a tracker issue: reproduce it if possible, check it isn't a duplicate
or already-fixed, and draft a clean, structured issue. **Filing borderline items is human-gated** —
a human glances before it lands on the public tracker, because a noisy or wrong issue costs more
than the few seconds saved.

## Triage (Band A for the safe parts)
The Steward keeps the tracker healthy:
- **Vulnerability check first** — the [divert](../reference/security-spine.md#6-the-vulnerability-divert)
  also runs on issues opened *directly* on the public tracker (a reporter who skips the private path).
  On a hit: **no substantive public reply and no label commentary** — a code-grounded reply would
  publicly confirm exploitability. Route the item through the complete private disclosure path and
  leave the next move (lock, minimize, edit, advisory) to a human.
- **Initial reply** — a substantive, code-grounded response (or a targeted needinfo question), under
  a confidence gate: if you can't ground it in actual code, say nothing rather than post a guess.
- **Label + milestone** — mechanical, reversible metadata. A good candidate for autonomous,
  deterministic labeling (size, area, type) with a tight allowlist that never overrides a human's
  label.
- **Sprint-candidate** — mark confirmed bugs that have a fix starting.

## Fix → resolve
A serious issue that nobody is building becomes a Builder task → it enters the
[PR lifecycle](pr-lifecycle.md). The fix, the regression test, and the release all happen there.

## Close with credit (careful — this is irreversible)
An issue closes without a human only when its fix is **provably shipped** and it's **fully
resolved**, never partially.
The safe autonomous gate (Band A *with watchdog*) is mechanically strict:
- a merged PR explicitly links the issue as closing it, **and**
- the merge commit is contained in a released tag (provably shipped, not just on the trunk), **and**
- the issue is **single-surface** — if it enumerates several distinct symptoms, it does *not*
  auto-close; it routes to a human for full-surface verification, **and**
- no "still broken" pushback after the fix, and no hold.

Anything short of that stays open and is surfaced to a human. **Under-close rather than mis-close** —
a wrongly-closed issue erodes reporter trust. Every autonomous close is logged and
[fact-checked by the watchdog](../playbooks/watchdog-pattern.md).

A human can still close an issue that falls short of that gate. For two cases, an issue that's
done with no change linking it and an issue filed twice, the `issue-close-proposal` skill prepares
the [decision row](../playbooks/output-loop.md#the-decision-row) the human decides from. That
capability stays Band C. Its recommendation rests on a model's reading of issues and code, so it
never becomes the unattended path.

## The dropped-ball guard
The most common trust leak is an issue where *you* asked the reporter a question, they answered, and
then you went silent. Surface these explicitly: an open issue where a maintainer replied, the
reporter answered after, and no PR exists — sorted by how long they've been waiting. This is a
pure-detection Band-A signal (never an auto-reply); it just tells the maintainer who to get back to.

---

## Skills for this lifecycle
- `issue-capture` — Watcher: stage non-tracker inbound, dedupe, and mark only after durable capture.
- `issue-triage` — Steward: reply, label, milestone, sprint-candidate.
- `issue-autoclose` — the strict, watchdog-verified shipped-issue closer.
- `issue-close-proposal`: Steward. Prepares an evidence-backed close (done, or duplicate) for a human to decide.

_Related: [PR lifecycle](pr-lifecycle.md) · [community](community.md) ·
[the watchdog pattern](../playbooks/watchdog-pattern.md)._
