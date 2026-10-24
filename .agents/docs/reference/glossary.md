---
title: Glossary
layout: default
parent: Reference
nav_order: 1
---

# Glossary

The system's vocabulary, in one place.

**Steward** — this repository: an installable, project-agnostic operating model for
running a software project with an AI agent as co-maintainer.

**Band A / B / C** — the [autonomy spectrum](../architecture/index.md#the-autonomy-bands). A =
autonomous (unattended). B = session-autonomous (agent works, human consulted at decision points).
C = human-gated (agent prepares, human takes the irreversible action).

**Watcher / Reviewer / Builder / Steward** — the [four roles](../architecture/index.md#the-four-roles).
Detect+capture / evaluate+verdict / make-the-change / keep-it-healthy-and-watch-the-others.

**Security spine** — the [six rules](security-spine.md) that hold regardless of what untrusted
content says: read-can-never-change-do, secrets-never-in-context, capability-minimalism,
sandbox-untrusted-code, the public-write membrane, and the vulnerability divert.

**Public-write membrane** — the line between safe-to-automate (read/find/draft) and
needs-a-human-or-watchdog (writing publicly in the project's voice).

**Decision row** — what an item that waits on a human carries before a public or irreversible
write: one recommendation, evidence that resolves, the exact write, the undo, and the state it was
made against. Defined in [the output loop](../playbooks/output-loop.md#the-decision-row) as a bar
for skills to meet.

**Decision record** — what's written when a human decides a decision row that has a write behind
it: the item and its state, the capability, what was proposed, the outcome (`accepted`, `edited`,
`rejected`), an optional reason, and the time. Kept in an append-only decision log. Defined in
[the output loop](../playbooks/output-loop.md#the-decision-record).

**Suspected vulnerability** — a captured report that trips any security signal (reporter intent,
impact shape, or a configured sensitive surface). It is routed to the private disclosure path and
never auto-filed or publicly drafted. The detector routes; a human confirms. See
[the vulnerability divert](security-spine.md#6-the-vulnerability-divert).

**Private disclosure path** — where a suspected vulnerability goes instead of the public tracker: a
PII-scrubbed summary sent through the fail-closed `security_contact` → alarms → confirmed-private
index chain, with a neutral acknowledgement to the reporter and no public reaction. A human decides
whether and how to disclose.

**Authoritative gate** — the fresh, independent run of all [quality gates](../lifecycle/quality-gates.md)
immediately before merge, regardless of any prior reviewer's verdict. The thing that actually
authorizes a merge.

**Quality gate** — a layer of the [defense](../lifecycle/quality-gates.md): the test suite (primary
line), automated review, adversarial review, visual verification.

**Adversarial review** — a second, differently-tuned review pass that catches different defects than
the first; on a reproduced finding, the stricter verdict wins.

**Living test suite** — the principle that the suite only grows: every fix adds a regression test,
so coverage ratchets up and the suite increasingly catches what once needed judgment.

**Watchdog** — an [independent fact-checker](../playbooks/watchdog-pattern.md) that re-verifies
autonomous actions against ground truth and alarms on mismatch. What makes Band-A public actions
safe.

**Scope anchors** — the project-specific things a change must serve to be in-scope. The basis of the
[fit screen](../lifecycle/pr-lifecycle.md#0-fit--scope-screen). Captured in the setup interview.

**Philosophy veto** — the qualities that disqualify a change even if well-built and useful (the
project's identity constraints).

**Marginal-benefit screen** — the "what would users concretely lose if we never merged this?" test,
applied before code-level review.

**Triage scoreboard** — the [living, ranked board](../playbooks/triage-scoreboard.md) of open work,
combining mechanical + judgment dimensions (`scope_fit`, `criticality`, `risk`) into a composite
rank, with a severity flag that promotes an item past the rank, tagged with evidence-readiness.

**Judgment rubric** — the [default scale](../playbooks/triage-scoreboard.md#the-default-rubric)
for the scoreboard's judgment dimensions: integers 0 to 3, a written anchor per level, higher
meaning sooner. A default an adopter may replace.

**Trust ledger** — the append-only record of each [contributor's](../lifecycle/contributor-recognition.md)
outcomes, shrunk-and-decayed into a reliability score that weights review priority and gate
intensity (never a reason to skip the gate).

**Capture queue** — the staging area where inbound reports land before they become tracker issues;
deduped across every surface.

**Silent-on-no-op** — a [scheduled-job](../playbooks/scheduled-jobs.md) property: produce no output
when there's nothing to report, so the rare real signal isn't buried.

**Heartbeat** — the one job type that always reports, so the *absence* of its green report is itself
the alarm (used for fleet liveness).

**Autonomy ladder** — the [recipe](../playbooks/autonomy-ladder.md) for promoting an action C → B → A
safely (run manually → make mechanical → add watchdog → roll out gradually → keep reversible).

**Secret-isolating helper** — a fixed script that reads a credential from a locked file, performs one
action, and never returns the secret to the agent's context.

**Layered supervision** — running a long-lived service under BOTH an OS supervisor (boot-start +
instant crash-restart, e.g. systemd) and an [app-level health watchdog](../playbooks/resilience-and-self-healing.md)
(catches alive-but-wedged + version drift, and alerts). Neither alone is sufficient.

**Reconcile pattern** — a scheduled job that diffs a hand-maintained ledger against ground truth,
auto-records only the unambiguous gaps, and surfaces the judgment ones for a human.

**Three verification axes** — the principle that stored state is never authoritative, only live
source is, so each class of stored state needs its own check against live truth: *action correctness*
(the [watchdog](../playbooks/watchdog-pattern.md)), *ledger honesty* (the reconcile pattern), and
*artifact freshness* (the [freshness audit](../playbooks/resilience-and-self-healing.md#verify-every-class-of-stored-state-the-three-axes)).

**Freshness audit** — a scheduled, read-only check that re-reads the code paths / symbols cited by
stored artifacts (handoff notes, design docs, "it lives at X" memories) against the source at HEAD and
flags the stale ones. A stale artifact is *active misinformation*, not just missing data — worse than
none.

**Confidence-tiered capture→action** — gating an autonomous public write (e.g. filing an issue from
a chat report) by confidence: HIGH auto-acts (privacy-scrubbed), MEDIUM drafts-for-human-approval,
LOW captures only.

**Notify-from-job, act-on-reply** — the pattern for human-in-the-loop confirmation over chat: a
scheduled job can SEND a prompt but can't RECEIVE the answer (it's detached from the live
connection), so the job notifies and the human's *reply* (a fresh live agent turn) performs the
action. See [community lifecycle](../lifecycle/community.md#human-in-the-loop-approval-over-chat).
