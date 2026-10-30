---
title: Contributor recognition
layout: default
parent: Lifecycle
nav_order: 5
---

# Contributor recognition

The system's relationship with the people who contribute. Two jobs: **never lose a contributor to
neglect**, and **let trust earned over time make review more efficient.**

> Credit is non-negotiable. A system that forgets to credit contributors loses them — and a project
> is its contributors.

---

## Credit: the unbreakable rule
Original authorship is **always** preserved, on every surface:
- On the branch — fix on the author's branch when you can, or merge with co-author trailers when you
  can't.
- In the changelog and release notes — name the contributor.
- In the closing comment — thank them by handle, name the version their work shipped in.

This holds even when the system did substantial work on top of a contributor's PR (rebased it, fixed
tests, resolved conflicts). The original author still gets credit; the system's help is additive,
not a takeover.

### Verify the credit is the *right* person
Preserving *a* credit isn't enough if it names the wrong contributor. Crediting the wrong person is a
distinct, easy failure — especially when several changes ship back-to-back and a handle gets carried
over from the previous one. A present co-author trailer proves a trailer exists; it does not prove
the name is correct.

- **Re-derive the credit from the work being shipped**, every release — look up the actual author of
  *this* change from the source. Never reuse the last release's handle from memory.
- **Every credit surface must name the same, correct handle**: changelog, release notes, and the
  closing thank-you. They have to agree.
- Where you can, make it a check that *fails the release* if a credited handle matches no author of
  the commits being shipped. This is part of [`release-pipeline`](https://github.com/nesquena/steward-os/blob/main/skills/release-pipeline/SKILL.md).

## The trust ledger
Track each contributor's reliability over time as a lightweight, append-only ledger of outcomes:
- **Positive** — clean ship, shipped-with-minor-fixes, good rework after a bounce.
- **Negative** — didn't actually fix it, went stale, introduced a regression, critical flaw.
- **Unscored** — neutral, superseded, shelved, work-in-progress, a concept call.

One event per PR's terminal outcome (a bounce that later converges is *one* good-rework event, not a
negative then a positive). Apply shrinkage toward a neutral prior so a contributor with one data
point isn't over-judged, and decay old events so the score reflects *recent* reliability.

**What the score is for:** it weights review *priority* and *gate intensity* — a long-trusted
contributor's small fix can move faster; a brand-new or low-reliability author's change gets more
scrutiny. It is an input to triage, never an excuse to skip the authoritative gate. **Every** change
still passes the gates regardless of who wrote it.

## The contributor funnel
Recognition is also growth. The same ledger and the community surfaces feed a funnel:
- surface first-time contributors for a warm welcome and a careful, encouraging review;
- periodically refresh and celebrate the contributor list (never drop anyone — union with the prior
  list as a floor; credit even indirectly-merged/cherry-picked work);
- promote reliable contributors toward more trust over time.

## Bounce with care
When you send a PR back, the *how* matters for whether the contributor comes back:
- give an **exact, reproducible fix-spec**, not a vague "needs work";
- reconcile against the live thread first so you don't pile on duplicate feedback;
- if the blocker is purely mechanical (a rebase, a couple of test fixes), consider fixing it
  yourself on their branch instead of bouncing — and still credit them.

> **Field note — default to salvaging, not bouncing** (contributed by Teknium, co-founder of Nous
> Research, from high-volume agent-codebase review). On a fast repo, most external changes are stale
> against the trunk — that's the normal case, not a defect, and asking every contributor to rebase
> creates weeks of latency and abandoned work. The flow that scales: sweep for duplicates *first*
> (popular bugs draw 2–4 fixes; search open and closed with synonym variants; credit the author of
> the commits you actually ship and acknowledge earlier submitters separately), cherry-pick their
> commits onto the current trunk so authorship survives, widen to sibling sites as your own commit,
> **preserve contributor authorship through the merge and verify it survived** (a rebase/merge that
> keeps the commits is simplest; if squash is forced, carry `Co-authored-by` trailers), then run a
> **mandatory post-merge sweep** for duplicates that arrived while you built. The riskiest shape is
> the investigation that quietly became a fix — you never entered "review mode," so the pre-flight
> dedupe never fired. Full version:
> [reviewing at volume](../reference/reviewing-at-volume.md#salvage-over-request-changes).

> **Field note — fabricated contributor identities** (contributed by Teknium). A contributor may open
> a burst of changes with commits authored as invented, official-looking identities (impersonating a
> maintainer or CI bot). Close on *evidence* — confirmed impersonation, misattributed authorship, or
> unverifiable provenance — not on appearance: close all and salvage nothing only when it's genuinely
> one of those, because bundled commits can mix appropriated work with unverified changes. An unusual
> name or email is not itself proof (a newcomer, a pseudonym, a privacy identity, or a *known*
> contributor's declared agent-assisted alt looks the same); when only the appearance is odd, ask for
> an authorship attestation and judge the commits on their merits rather than locking out a legitimate
> first-time contributor.

---

## Skills for this area
- `contributor-trust` — compute the trust map from logged outcomes and surface it into triage.
- `release-pipeline` — preserves attribution at merge and runs the credit-verification gate.
- (Credit handling is also woven into `pr-deep-review`.)

_Related: [PR lifecycle](pr-lifecycle.md) · [community](community.md) ·
[the triage scoreboard](../playbooks/triage-scoreboard.md)._
