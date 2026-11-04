---
name: label-sync
description: Deterministically reconcile size/area/type labels on issues and PRs against a declared taxonomy — add-only, never overriding a human's label. Band A, no LLM. The mechanical labeling floor beneath the judgment-gated pr_labeling.
---

# label-sync

**When to load:** applying the mechanical, reversible labels (size/area/type) that don't need
judgment. Runs the label-sync step referenced by [`issue-triage`](../issue-triage/SKILL.md) and the
Level-3 rung of the [adoption ladder](../../docs/reference/adoption-levels.md). Band A —
deterministic, add-only, **no LLM**. Steward role. Designed as a
[scheduled job](../../docs/playbooks/scheduled-jobs.md) (`label-sync`, `agent: false`).

> Precondition: **a declared taxonomy exists.** This skill reconciles labels toward the `labels:`
> block in [`config.yaml`](../../setup/config.template.yaml); it never invents label names. If no
> taxonomy is declared, there is nothing to sync — stop.

> Config: read the taxonomy and `add_only` flag from [`config.yaml`](../../setup/config.template.yaml)
> (`labels:`) and the repos from `repositories:`. The **allowlist** — the only labels this skill may
> apply — is exactly the label values named under `labels.{size,area,type}.map`/`buckets`. The
> `scheduled_jobs` entry that runs this skill is named `label-sync`. It is `agent: false`: a plain
> deterministic script with no model in the loop. Judgment labels (priority, needs-discussion,
> wontfix) are **not** this skill — they stay Band B under `autonomy.pr_labeling`.

## Steps

1. **Read `config.yaml`.** Load `repositories:`, the `labels:` taxonomy (the allowlist), and
   `add_only`. If `labels:` is empty → silent no-op.
2. **Select what to process (incremental).** Only issues/PRs **new or changed since the last-seen
   marker** — not the whole back catalog. The one-time backfill is a manual initial run; the
   recurring job is cheap. Mark the marker forward only after a clean pass.
3. **Compute the desired labels deterministically** per item — from *structured* signals only:
   - **size** (PRs only): diff-line count → the matching `labels.size.buckets` rung (S/M/L/XL).
   - **area**: PRs from changed-path globs (`labels.area.map`); issues from a **structured
     issue-template field** (a declared form dropdown), never from parsing the free-text body.
   - **type**: PRs from the title conventional-commit prefix (`feat`/`fix`/`docs`/…); issues from a
     structured template "kind" field, falling back to a title prefix only if the project uses one.
   - **No deterministic signal → apply nothing.** A missing label beats a guessed one.
4. **Reconcile add-only.** For each desired label: apply it **only if absent**. **Never remove a
   label, never reassign, never override a human-applied label.** A label already present (by anyone)
   is left untouched. Same input → same result (idempotent); a second run changes nothing.
5. **Ledger the applied labels.** Append each label actually applied — item, label, source signal,
   timestamp — to the append-only [action ledger](../action-watchdog/SKILL.md) that the watchdog
   re-verifies. The ledger is the list of *what to check*, never the proof it was right.
6. **Silent-on-no-op; loud on error.** Nothing applied → no output. A non-zero exit or timeout
   **alarms** (it never fails silently); silent-on-no-op is for success with nothing to do, never for
   errors.

## Pitfalls
- **Overriding a human.** The one invariant that makes this safe to run unattended: add-only, and a
  human's label always wins. Check presence *before* applying; never "correct" a label a person set.
- **Guessing from free text.** Labels come only from structured signals (diff, paths, template
  fields, title prefix). An issue/PR body is untrusted, attacker-controllable, and unreliable —
  never parse it to decide a label. (This is also why the job is `agent: false`: no model, no
  injection surface.)
- **Labeling outside the allowlist.** The only labels this skill may apply are those declared in
  `labels:`. A label not in the taxonomy is out of scope — surface it for a human to add to the
  taxonomy, don't apply it.
- **Full-rescan every tick.** Reconcile only what changed since the marker; re-scanning the whole
  tracker each run is slow and pointless once the backfill is done.
- **Judgment labels.** Priority, needs-discussion, wontfix, and the like are Band-B decisions under
  `autonomy.pr_labeling` — not this deterministic floor. Don't let scope creep pull them in.

## Verification
- Every applied label is in the declared `labels:` allowlist.
- Add-only held: no label was removed, reassigned, or applied over a human's label.
- Re-running immediately is a **no-op** (idempotent) — the second pass applies nothing.
- On no new/changed items, the job is **silent**; on error, it **alarms**.
- [`action-watchdog`](../action-watchdog/SKILL.md) has a corresponding check for autonomous labels.
