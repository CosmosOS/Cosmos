---
name: action-watchdog
description: Independently fact-check the autonomous actions the system took (issue closes, labels, capture marks, etc.) against live ground truth, and alarm on any mismatch. Run on a schedule. This is what makes Band-A public actions safe.
---

# action-watchdog

**When to load:** auditing autonomous actions for correctness. Runs the
[watchdog pattern](../../docs/playbooks/watchdog-pattern.md). Band A, deterministic, read-only.
**Build and enable this before promoting any public-write action to Band A.**

## Steps

1. **Read the action ledgers** — the append-only records each autonomous action writes (what it
   closed, labeled, filed, or marked). The ledger is the list of *what to check*, never the evidence of
   correctness.
2. **Re-verify each action against LIVE ground truth, independently:**
   - **Autonomous closes** ([`issue-autoclose`](../issue-autoclose/SKILL.md)): is the issue still
     closed? Is the cited PR actually merged and does it still link the issue? Does the cited release
     tag actually contain the merge commit? Was it reopened with a complaint? → any mismatch = 🔴.
   - **Autonomous labels** ([`label-sync`](../label-sync/SKILL.md)): for each ledger'd label, does it
     still match its source signal, is it within the declared `labels:` allowlist, and was it
     *add-only* — i.e. did the job avoid removing, reassigning, or overriding a human-applied label?
     → a mislabel, an out-of-allowlist label, or an overridden human label = 🟡 (a human's label
     always wins).
   - **Autonomous announcements** ([`release-announce`](../release-announce/SKILL.md)): does each
     ledger'd announcement correspond to a real release tag with merged content? Was it posted to the
     fixed channel **exactly once** (no duplicate for the same release)? Does the posted text match
     the changelog entry it claims to render? → a duplicate, or an announcement of an absent/unshipped
     release, or text that adds claims beyond the changelog = 🔴; a drifted last-announced marker = 🟡.
   - **Autonomous capture marks** ([`issue-capture`](../issue-capture/SKILL.md)): for each ledger'd
     mark, does exactly one durable staged record and terminal `captured` event exist for the same
     adapter + source item + target-repository key, and does the source carry exactly the configured
     marker? Did every vulnerability-diverted item receive **no** public mark? → a mark without a
     staged record, a missing/duplicate mark, or any public mark on a diverted item = 🔴; a stale
     nonterminal capture intent with no public action yet = 🟡.
   - **Autonomous issue files** ([community capture](../../docs/lifecycle/community.md)):
     is the source staged as a HIGH-confidence bug, did the privacy scrub and cross-surface dedupe
     pass, was the per-run filing cap respected, and does exactly one public issue exist with no PII
     or vulnerability content? → a file outside those gates, a duplicate, or a privacy/security leak
     = 🔴; stale filing-ledger bookkeeping with the public issue otherwise correct = 🟡.
   - **(Add a check here for every new autonomous action — same change that adds the action.)**
3. **Classify severities.** 🔴 = correctness failure (an action was *wrong* — act now). 🟡 = drift
   (stale, reconcile soon).
4. **Report only on findings.** Silent when all-clear. Route 🔴 to a human immediately; 🟡 to the
   reconcile queue. (Exception: a dedicated fleet-liveness heartbeat always reports, so its silence
   is the alarm.)
5. **Never fix anything.** The watchdog verifies and surfaces; a human (or a separate, itself-watched
   action) fixes. An auto-fixing watchdog is just another unverified actor.

## Pitfalls
- **Independence is the whole point** — re-derive correctness from live state, don't trust the
  action's own success report.
- **Deterministic, no LLM** where possible — it's comparing recorded actions to live data; no model
  means no injection surface and no nondeterminism.
- **Don't let it page on every run** — alarm fatigue defeats it. Silent-on-all-clear.

## Verification
- Deliberately record a *wrong* action (in a test ledger) and confirm the watchdog flags it 🔴.
- On a clean state, the watchdog is silent.
- Every autonomous public action in the system has a corresponding check here.
