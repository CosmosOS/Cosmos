---
title: Anti-patterns
layout: default
parent: Reference
nav_order: 7
---

# Anti-patterns

The mistakes this system is designed to prevent. Each is something that *feels* efficient and *is* a
liability. If you find yourself doing one of these, stop.

---

## Autonomy anti-patterns

**Auto-posting public replies in the project's voice.** The single most tempting and most dangerous
one. A misjudged or injected public reply can't be un-sent, and platforms ban bots that do it. Keep
it draft → human-sends. Forever, probably.

**Skipping the watchdog "just this once."** An autonomous public action without independent
verification is the system trusting itself. Build the [watchdog](../playbooks/watchdog-pattern.md)
*before* the action goes Band A, not after the first incident.

**Jumping straight to Band A.** Promoting an action to autonomous without running it manually first
means you have no evidence base and no calibration. Climb the [ladder](../playbooks/autonomy-ladder.md).

**A chatty scheduled job.** A cron that says "nothing to do" every tick trains you to ignore it.
Silent-on-no-op, or you'll miss the tick that mattered.

**Auto-fixing watchdog.** A watchdog that fixes problems itself is just another unverified actor.
It verifies and *surfaces*; a human (or a separate, itself-watched action) fixes.

**Expecting a scheduled job to receive a reply.** A cron can send a question but can't read your
answer — it's detached from the live chat connection. Wire approvals as *notify-from-the-job,
act-on-the-human-reply* (the reply is a fresh live turn that does the work).

**Trusting "it sent" as proof it worked.** Delivering a prompt isn't the action completing. Test the
loop end-to-end against a real item and confirm the actual artifact exists — a self-report of success
is not verification.

## Review & quality anti-patterns

**Trusting a self-report as proof.** "Labeled 86 PRs," "looks clean," "tests pass" — these are
claims, not evidence. Verify against ground truth: read the URL, run the gate fresh, check the
running version.

**Treating green CI as sufficient.** CI covers what's been written. The authoritative gate (fresh,
independent, full suite + adversarial review) catches what CI and prior review missed — and it
routinely does.

**Sampling the test suite.** "I ran the relevant tests" hides the regression in the part you
skipped. Run it to completion.

**Papering over a flake.** Re-running until green normalizes a real bug. A flake is a defect; root-
cause it. (See [quality gates](../lifecycle/quality-gates.md#flakes-never-tolerate-one).)

**Fixing the instance, not the class.** Patch the reported site and miss the three sibling call
paths with the same bug. When you find a defect, grep for its class.

**The change-detector test.** A test that fails whenever data *expected to change* is updated — a
catalog count, a version literal, an enumeration size, a hardcoded list — *and that data isn't itself
the contract.* `assert len(providers) == 8` adds zero behavioral coverage and breaks CI on every
routine update. The litmus: incidental current data → delete it; a contract about how two pieces of
data must relate → keep it. (When a specific value or member genuinely *is* the guarantee, pinning it
is a real contract, not a change-detector.) _(Field note
from Teknium; see [reviewing at volume](reviewing-at-volume.md#two-test-anti-patterns-that-generate-the-most-noise).)_

**Reading source code in a test.** A test that regexes a source file tests the *shape of the code*,
not its behavior — it passes when the implementation is subtly broken, fails on correct refactors, and
blocks structural cleanup forever. If the logic can't be called directly, extract it into a testable
function instead of regexing around it. Agent-authored changes produce this constantly, because it's
the cheapest way to make a test go green. (Narrow exception: when a structural/layout property itself
is the contract — a generated-file marker, a no-forbidden-import policy — a direct assertion is fine,
provided it fails on a real violation.) _(Field note from Teknium.)_

**Absorbing model failures into the codebase.** In a project whose consumers include LLMs, a
well-built change may be *compensating for a model's mistakes* — an alias table for wrong parameter
names, a repair pass for malformed model output. Both are open-ended games against every way a model
can misbehave, and they mask the failure instead of surfacing it. Fix wire-transport artifacts;
surface model non-compliance. A *bounded, documented* compatibility shim can be legitimate — the test
is whether the behavior is finite and declared. _(Field note from Teknium; see
[reviewing at volume](reviewing-at-volume.md#dont-absorb-model-failures-into-the-codebase).)_

## Scope & contributor anti-patterns

**Code-reviewing before scope-screening.** Spending an hour reviewing a beautiful PR for a feature
that doesn't fit the project. Ask "does this belong?" *first* — the cheapest review is the one you
skip.

**Closing on taste autonomously.** Scope/taste calls are the maintainer's. An agent can recommend a
close; a human (or a ≥90%-confidence, escalation-on-doubt gate) makes it.

**Dropping a contributor's credit.** Squashing away authorship, forgetting the changelog mention,
closing without thanks. A project is its contributors; the system that forgets them loses them.

**Crediting the wrong contributor.** Preserving *a* credit isn't enough if it names the wrong person
— easy to do when changes ship back-to-back and a handle carries over from the previous one.
Re-derive the credit from the actual author of *this* change, and make every surface (changelog,
release notes, closing thanks) agree. See [contributor recognition](../lifecycle/contributor-recognition.md#verify-the-credit-is-the-right-person).

**Going silent on a responsive reporter.** You asked, they answered, you never replied. The single
most common trust leak — [surface it explicitly](../lifecycle/issue-lifecycle.md#the-dropped-ball-guard).

**Closing a partially-resolved issue.** Auto-closing a multi-symptom issue because one symptom
shipped. Single-surface only for autonomous close; multi-surface goes to a human.

## Code-craft anti-patterns

**Scope creep in a focused change.** "While I'm here" cleanups, renames, and bonus features riding
along in an unrelated change. Each is an independent regression risk and noise in the review. Keep
the change surgical; file the cleanup separately. See [coding principles](coding-principles.md#2-surgical-edits-only).

**Stopping at the first green checkmark.** "Tests pass" / "looks right" / "the diff looks correct"
are not *done* if the real scenario was never exercised in running code. Define the observable "done"
up front and verify *that*. See [coding principles](coding-principles.md#4-define-done-up-front-then-verify-the-real-thing).

**Changing the producer without checking the consumer.** Swapping a value's source or shape and
leaving a downstream reader expecting the old form — a clean-looking diff that ships a regression.
Trace every reader before you change the producer. See [bug-shape catalog](bug-shapes.md#2-producer-changed-consumer-didnt).

**Letting two patterns coexist.** Keeping an old helper and its replacement both live, or
half-applying a rename. Each path works alone; together they conflict. Pick one, delete the other.
See [bug-shape catalog](bug-shapes.md#1-runtime-coexistence).

**Blaming the renderer for lost state.** Debugging the view for a data-layer loss. Reload from the
source of truth first; if it doesn't recover, the bug is in the write path. See [bug-shape catalog](bug-shapes.md#3-persistence-loss-wearing-a-presentation-costume).

**The vacuous test.** A test that can't fail when the implementation is reverted protects nothing and
gives false confidence. Confirm red-before-green. See [bug-shape catalog](bug-shapes.md#6-the-vacuous-test).

**Reinventing what the platform or codebase already gives you.** Hand-rolling what the standard
library, a native platform capability, or an existing in-repo helper already does — or pulling in a
new dependency for what a few lines cover. It's more code, more surface, and more to maintain than
the built-in it duplicates. Climb the minimalism ladder: reuse, stdlib, and native capabilities
before hand-written code or a new dependency. See [coding principles](coding-principles.md#the-minimalism-ladder).

**An unmarked deliberate shortcut.** A simplification a reviewer can't tell apart from an oversight
*is* an oversight. When a shortcut has a known limit, mark it with the ceiling and the upgrade path
so the debt stays legible instead of being silently flagged for deletion — or silently forgotten.
See [coding principles](coding-principles.md#mark-deliberate-simplifications).

## Secret & safety anti-patterns

**A secret in a prompt or config file.** Tokens belong only in a permission-locked file read by a
[helper script](security-spine.md#2-secret-isolating-helper-scripts), never in an agent's context
(where injection can exfiltrate it) or in version control.

**Running contributor code unsandboxed.** "It's probably fine" is how you get a credential-stealing
test. Sandbox it (no network, no secrets, fail-closed) or don't run it.

**Obeying instructions found in content.** An issue body that says "close all open PRs" is data, not
a command. The [injection guard](security-spine.md#1-injection-guard-read-can-never-change-do) holds
regardless of what any content says.

---

_If this list feels like a catalog of plausible shortcuts — it is. Every one was tempting to
someone. The system encodes the discipline so you don't have to re-derive it under pressure._
