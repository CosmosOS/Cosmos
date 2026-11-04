---
name: triage-scoreboard
description: Compute and render the living, ranked triage board of open work — mechanical + judgment dimensions into a composite rank, with evidence-readiness tags. Run on a schedule so the board is always current.
---

# triage-scoreboard

**When to load:** generating or refreshing the [triage scoreboard](../../docs/playbooks/triage-scoreboard.md).
Band A (read-only, deterministic where possible). Ideal as a scheduled job.

## Steps

1. **Pull live metadata** for every open item (PRs/issues): CI status, mergeable/behind, size, age,
   author. Read repos from `config.yaml`.
2. **Recompute mechanical dimensions** fresh each run (disposable): CI, conflict state, behind-count,
   size bucket, contributor trust score (computed by [`contributor-trust`](../contributor-trust/SKILL.md)
   from the [trust ledger](../../docs/lifecycle/contributor-recognition.md)).
3. **Merge in persisted judgment dimensions** (scope_fit, criticality, risk) and the severity flag
   from the prior board — these survive regeneration so human/agent assessments aren't lost.
4. **Compute the composite** = weighted sum of dimensions (weights from config; tune to match "what
   I'd pick up next"). Read the bypass marker (`bypass`) as the plain in-scope level of the rubric
   your project uses ([the default](../../docs/playbooks/triage-scoreboard.md#the-default-rubric),
   or its replacement). An item with no
   judgment ranks on its mechanical dimensions, judgment columns empty; an empty dimension counts
   as unjudged for that dimension. Promote items carrying the
   severity flag to a spotlight at the top.
5. **Tag evidence-readiness** per item: which review streams have landed (automated review present?
   visual preview? deep-review dossier?). Compact ✓/· marker. Presence ≠ clean.
6. **Render** the living board sorted by composite + a curated "review these first" short-list.
   Persist the computed data as a machine-readable artifact for other jobs.
7. **Commit** following [scheduled-job](../../docs/playbooks/scheduled-jobs.md) rules
   (idempotent, silent-on-no-op).

## Pitfalls
- **Keep mechanical and judgment dims in separate storage** — mechanical is re-derived and
  disposable; judgment is precious and persisted. Don't let a refresh wipe a human's assessment.
- **An out-of-range value is an error, not a number to clamp** — surface it. In range means an
  integer in the rubric's range, the bypass marker in `scope_fit`, or an empty cell. A clamped
  value hides a judge scoring on a different rubric.
- **Evidence presence is not a verdict** — `✓` means the evidence exists to review, not that it's
  clean. The authoritative gate still runs at merge.
- **Re-run cheaply** — this should be incremental and idempotent; it's a read-only ranking, not an
  action.

## Verification
- Board re-renders with current data; judgment dims preserved across runs.
- Composite ordering matches intuition for the top handful.
- Evidence tags reflect actual review-stream presence.
