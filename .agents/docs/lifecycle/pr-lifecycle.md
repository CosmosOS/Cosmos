---
title: Pull-request lifecycle
layout: default
parent: Lifecycle
nav_order: 2
---

# Pull-request lifecycle

How a proposed change travels from "a PR appeared" to "shipped in a release, author credited." This
is the most safety-critical pipeline in the system — it's the one that mutates the codebase — so the
gates are strict and the merge is never on a single agent's say-so.

> Roles: **Watcher** notices the PR · **Reviewer** evaluates it · **Builder** fixes/rebases it ·
> **Steward** ranks it and runs the release. Merge is **Band B** (human owns the final call) or
> Band A only for the narrowest mechanical cases with a watchdog.

> Author-side counterpart: this page is how a change is *received and shipped*. For how a
> contributor (human or agent) should *write and present* one so it lands in a single round, see
> [Authoring a change that lands](../reference/authoring-changes.md).

---

## The pipeline at a glance

```
 PR opened
    │  Watcher: capture into the review queue, rank it
    ▼
 [0] FIT / SCOPE SCREEN   "does this belong in the project at all?"
    │   → out of scope → close politely (Band C) or escalate
    ▼
 [1] MARGINAL-BENEFIT SCREEN   "what would users lose if we never merged this?"
    │   → "nothing concrete" → maintainer-review / close
    ▼
 [2] ROUTING   small+clean+narrow → fast lane | larger/riskier → deep lane | needs-UI → screenshot gate
    ▼
 [3] DEEP REVIEW   read the whole diff, reproduce, run the quality gates (Reviewer)
    │   → flaws → bounce with an exact fix-spec, or fix it yourself (Builder)
    ▼
 [4] THE AUTHORITATIVE GATE   fresh automated review + adversarial review + FULL test suite
    │   (independent of any prior reviewer's verdict)
    ▼
 [5] MERGE + RELEASE   rebase → merge (preserve attribution) → tag → deploy → verify
    ▼
 [6] CLOSE + CREDIT   close the PR/issue with version + thanks; update the trust ledger
```

Each numbered stage is a gate: a PR only advances when it clears the one before. Most PRs that die,
die early (scope/benefit) — which is correct, because the cheapest review is the one you don't have
to do.

---

## [0] Fit / scope screen

> **Vulnerability pre-check (before anything public).** A PR can itself disclose a live
> vulnerability — a "fix" whose description, diff, or linked issue reveals an exploit, a PoC, or an
> unfixed sibling. Run the [vulnerability divert](../reference/security-spine.md#6-the-vulnerability-divert)
> at PR intake, before any public review comment: on a hit, post no substantive public review that
> confirms the exploit, route it through the complete private disclosure path, and let a human
> coordinate the fix (private patch, security advisory, then merge). This is the third divert entry
> point alongside capture and inbound issues.

Before any code-level review, ask the **scope question**: *does this change belong in the project at
all?* A technically-perfect PR for a feature that doesn't fit the project is still a no.

- Define your project's **scope anchors** up front (the setup interview captures these): the things
  a change must serve to be in-scope. Anything that hits an anchor is on the table; anything that
  hits none gets the philosophy veto.
- **Health fixes bypass the scope gate.** A bug/security/reliability fix on an already-shipped
  feature is always in-scope — you don't re-litigate whether the feature should exist.
- Verdict → action: clearly-in → proceed · clearly-out → close politely (Band C) with a kind
  explanation and an alternative if one exists · **uncertain → escalate to the human with a
  recommendation**, don't guess.

## [1] Marginal-benefit screen

The headline question: **"what would real users concretely lose if we never merged this?"** If the
answer is "nothing concrete," route to maintainer-review or close — regardless of code quality.
Weigh common-vs-niche audience, the visual/complexity cost it adds, and the maintenance tax.

## [2] Routing

Sort the cleared PRs by where they should go:
- **Fast lane** — small diff, narrow scope, no conflicts, no design questions. These can ship same
  session after the gate.
- **Deep lane** — medium/large, touches sensitive subsystems, or needs a design judgment.
- **Screenshot/UX gate** — anything with a visible surface. Request before/after screenshots at the
  relevant viewports; a visible change is verified visually, not just by tests. (See
  [quality gates](quality-gates.md#4-visual-verification).)
- **Hold / draft** — parked or not-ready. Skip entirely; never act on a held or draft PR.

## [3] Deep review (Reviewer + Builder)

Read the **whole diff**, not just the hunks. Reproduce the bug or exercise the change. Read the
files at the PR's head *and* on the trunk to see what changed. Inert reading needs no sandbox;
exercising the change runs the PR's code, so decide first whether it may run under the
[sandbox rule](../reference/security-spine.md#4-the-sandbox-untrusted-code-execution). For every
flaw, decide:
- **Bounce** — leave an exact, reproducible fix-spec and let the author iterate. Reconcile against
  the live thread first so you don't duplicate feedback.
- **Fix it yourself** (Builder) — for mechanical blockers (a rebase, a few failing tests, a small
  nit), fix on the author's branch and preserve their authorship. Don't wait on the contributor for
  a mechanical unblock.

The output of deep review is *warm-up*, not the ship decision. The ship decision is the next stage.

> **Field note — the #1 reason a well-built change dies is a wrong premise, not bad code**
> (contributed by Teknium, co-founder of Nous Research, from high-volume agent-codebase review). A
> change can be clean, tested, and coherent while resting on a *wrong mental model of the system* or
> treating an *intentional design as a gap*. Before accepting the premise, read the history of the
> code that would change (`git log -p -- <path>` and `git blame` recover the commit and the reasoning
> for an absence — it may be load-bearing) and require falsifiable evidence: a reproduction on the
> current trunk plus a traced code path from symptom to cause, not a plausible rationale. On the
> automation boundary, mirror the strict rule this repo already sets — automation may *classify* and
> *recommend* a close (already-fixed, cannot-reproduce, incoherent) but the autonomous *close* stays
> with the [mechanically-provable shipped-fix gate](issue-lifecycle.md#close-with-credit-careful--this-is-irreversible)
> or a human; wrong-premise and design-taste closes always stay with a human. Full version:
> [reviewing at volume](../reference/reviewing-at-volume.md#wrong-premise-beats-bad-code).

## [4] The authoritative gate

**This is the line that protects the trunk.** Run it fresh, immediately before merge, regardless of
what any prior reviewer (human, agent, or cached dossier) concluded — a prior "looks clean" may have
verified only one code path.

The gate is layered and independent (full detail in [quality gates](quality-gates.md)):
1. **Automated code review** — an independent reviewer pass over the diff.
2. **Adversarial review** — a second, differently-tuned pass; it catches different things. On a
   reproduced finding, the stricter verdict wins.
3. **The full test suite** — run to completion, never sampled. It catches regressions the targeted
   checks and the reviewers both miss.
4. **Visual verification** — for any visible surface.

Items 3 and 4 run the PR's code. For an untrusted PR they run inside the
[sandbox](../reference/security-spine.md#4-the-sandbox-untrusted-code-execution) or not at all, and
a gate with a skipped run is not all-clear.

A green CI is necessary but not sufficient — the authoritative gate has repeatedly caught real
regressions that green CI and prior review missed. If the gate finds something, the Builder fixes it
and the gate re-runs. Only an all-clear gate advances to merge.

## [5] Merge + release

- **Rebase** onto the current trunk (the Builder does this itself if the only blocker is staleness).
- **Merge preserving attribution** — the original author's authorship survives the merge (a
  non-squash merge with co-author trailers, or a fix-on-their-branch). See
  [contributor recognition](contributor-recognition.md).
- **Tag → deploy → verify** the change is actually live. "Shipped" means the tag exists *and* the
  merge is real *and* (if you deploy) the running version serves it — never claim shipped from a
  passed gate alone.

> Merge itself is **Band B**: the human owns the final irreversible call on anything substantial.
> The narrowest mechanical merges can graduate to Band A — but only with the
> [watchdog](../playbooks/watchdog-pattern.md) fact-checking each one.

## [6] Close + credit

Close the PR and any issues it resolves with a comment naming the version, and **thank the author by
handle.** Update the [contributor trust ledger](contributor-recognition.md) with the outcome. Clean
up branches/worktrees. The loop is only closed when the contributor has been credited.

---

## Skills for this lifecycle

- `pr-triage` — runs stages [0]–[2]: the fit/benefit screen and routing.
- `pr-deep-review` — runs stages [3]–[4]: deep review and the authoritative gate.
- `release-pipeline` — runs stages [5]–[6]: merge, release, close, credit.

_(Skills live in [`skills/`](https://github.com/nesquena/steward-os/tree/main/skills) and are the runnable form of this playbook.)_

---

_Related: [quality gates](quality-gates.md) · [the triage scoreboard](../playbooks/triage-scoreboard.md)
· [contributor recognition](contributor-recognition.md)._
