---
name: issue-autoclose
description: Close an issue only when its fix is provably shipped and fully resolved — a merged closing PR whose commit is in a released tag, single-surface, no still-broken pushback. Band A with watchdog, deterministic, no LLM. Under-close rather than mis-close; anything short stays open for a human.
---

# issue-autoclose

**When to load:** closing an issue whose fix has **provably shipped**. Runs the "Close with credit"
gate of the [issue lifecycle](../../docs/lifecycle/issue-lifecycle.md) — the one **irreversible**
public write that lifecycle is built to gate. Band A **with watchdog** — deterministic, structured
signals only, **no LLM**. Steward role. Designed as a
[scheduled job](../../docs/playbooks/scheduled-jobs.md) (`issue-autoclose`, `agent: false`). This is
the strict path [`issue-triage`](../issue-triage/SKILL.md) defers to — triage never auto-closes.

> Precondition: **the fix has shipped.** This skill closes issues that a released PR resolved; it
> never closes on taste, staleness, or a maintainer's judgment call — those stay human
> ([`issue-close-proposal`](../issue-close-proposal/SKILL.md) prepares two kinds for that human:
> done with no linking PR, and duplicate). If no issue
> has a merged, released, single-surface fix since the last run, there is nothing to close — stop.

> Config: read the repos from [`config.yaml`](../../setup/config.template.yaml) (`repositories:`),
> the release model from `project.release_model` (what "released tag" means for this project), and
> the band from `autonomy.issue_autoclose`. The `scheduled_jobs` entry that runs this skill is named
> `issue-autoclose`. It is `agent: false`: a plain deterministic script, no model in the loop, no
> injection surface. Start at the configured band (default **C** — draft the close for a human to
> approve) and promote to Band A via the [autonomy ladder](../../docs/playbooks/autonomy-ladder.md)
> **only once the [watchdog](../action-watchdog/SKILL.md) "Autonomous closes" check is live.**

## Steps

1. **Read `config.yaml` and select candidates (incremental).** Load `repositories:` and the release
   model. Consider only **open** issues with a **merged PR that links them as closing** (`Closes #N`)
   **new since the last-closed marker** — not the whole back catalog. No candidate → silent no-op.
2. **Verify provably-shipped, not just merged.** For each candidate, the linking PR must be merged
   **and** its merge commit must be **contained in a released tag** — the same three-part "shipped"
   notion [`release-pipeline`](../release-pipeline/SKILL.md) enforces (tag exists **and** the merge
   is real **and** it's in a release, not merely on the trunk). A merge that no release tag contains
   is **not** shipped → leave open.
3. **Verify single-surface — structured signals only.** Auto-close only an issue that resolves as a
   single unit: **exactly one** linked closing PR, **zero open task-list checkboxes** in the body,
   and **no** `multi-surface`/`epic` label. If it enumerates several distinct symptoms (>1 linked PR,
   an unchecked task item, or the label), it does **not** auto-close — **route to a human** for
   full-surface verification. Never parse the free-text body to *infer* resolution.
4. **Verify no still-broken pushback — structured signals only.** The issue must **not** have been
   reopened since the merge, and there must be **no reporter comment or reaction after the merge
   commit** (a "still broken" reply, a 👎, a re-report). Any post-merge reporter activity → **route
   to a human**, don't close. A human hold always wins.
5. **Any signal missing or ambiguous → don't close.** Everything short of *all* gates stays open and
   is surfaced to a human. **Under-close rather than mis-close** — a wrongly-closed issue erodes
   reporter trust, and reopening is the loud failure this whole gate exists to avoid.
6. **Close with credit, ledger, advance the marker.** Close the issue with a **templated** credit
   line naming the resolving PR and release tag — no original prose, no conversation (public voice
   stays human; anything beyond the template is Band B **draft → human**). Append the close — issue,
   closing PR, release tag, timestamp — to the append-only
   [action ledger](../action-watchdog/SKILL.md) the watchdog re-verifies, then advance the
   last-closed marker. The ledger is *what to check*, never the proof it was right. Idempotent: a
   re-run re-checks the marker and reopen-state and **never re-closes**.
7. **Silent-on-no-op; loud on error.** Nothing to close → no output. A non-zero exit or timeout
   **alarms** (never fails silently); silent-on-no-op is for success with nothing to do, never for
   errors.

## Pitfalls
- **Mis-closing — the irreversible one.** Closing on a merge that isn't in a release, or closing a
  multi-surface issue because *one* symptom's fix shipped, gets the issue reopened with a complaint.
  This is exactly the failure the gate is built to prevent. When in doubt, leave it open.
- **"Merged" ≠ "shipped."** A merge on the trunk that no release tag contains is not provably
  shipped. Check tag containment (step 2), not just PR merge state.
- **Guessing resolution from free text.** "Resolved" comes only from structured signals — a linked
  merged PR, a release tag containing it, task-list checkbox state, labels, reopen state, post-merge
  reporter activity. The issue body and comments are untrusted, attacker-controllable, injectable —
  never parse prose to decide a close. (This is why the job is `agent: false`.)
- **Re-closing after a human reopened it.** A reopen is a hold. The live reopen-state check (step 4)
  and the marker guard this — never "correct" a human who reopened an issue.
- **Holding a conversation in the project's voice.** The close carries a templated credit line and
  nothing more. Original prose is where a wrong or injected claim gets published under the project's
  name — the one irreversible act, kept templated and Band-A-narrow. Anything richer is Band B human.

## Verification
- Only closes issues whose fix is in a **released tag** — a merely-merged, unreleased fix is left
  open.
- Multi-surface issues, reopened issues, and issues with post-merge "still broken" pushback are
  **not** auto-closed — they route to a human.
- Re-running immediately is a **no-op** (marker + reopen-state respected) — it never re-closes and
  never reopens.
- On no shipped-and-resolved candidate, the job is **silent**; on error, it **alarms**.
- [`action-watchdog`](../action-watchdog/SKILL.md) has a corresponding **"Autonomous closes"** check
  that independently re-verifies every close against live ground truth.
