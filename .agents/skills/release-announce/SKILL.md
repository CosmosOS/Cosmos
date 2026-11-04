---
name: release-announce
description: Post a templated release announcement to a fixed channel via a secret-isolating helper, gated on a last-announced marker so it fires once per release and never re-posts. Band A, downstream of release-pipeline.
---

# release-announce

**When to load:** a release has been stamped (tag + changelog) by
[`release-pipeline`](../release-pipeline/SKILL.md) and you're posting the routine public
announcement. Runs the announcements section of the [community lifecycle](../../docs/lifecycle/community.md).
Band A — narrow, templated, gated. Steward role.

> Precondition: **the content already exists.** This skill *renders* an announcement from the
> changelog; it never composes original prose. If there's no changelog entry and release tag for
> this release, stop — there is nothing to announce.

> Config: read the release model and changelog from [`config.yaml`](../../setup/config.template.yaml)
> (`project.release_model`) and the band from `autonomy.announcements`. The secret-isolating helper
> reads the bot token from `secrets.credentials_file`; the destination channel is a fixed, hardcoded
> value, not chosen at runtime. The `scheduled_jobs` entry that runs this skill is named
> `announcements` — that job *is* this procedure. Start at the configured band (default C) and
> promote to Band A via the [autonomy ladder](../../docs/playbooks/autonomy-ladder.md) only once the
> [watchdog](../action-watchdog/SKILL.md) check below is live.

## Steps

1. **Determine the release to announce.** Read the release model and changelog from config; find the
   newest release tag. Compare it against the **last-announced marker** (the latest release already
   posted). If this release was already announced → **silent no-op; never re-post.**
2. **Compose from the changelog — don't write anew.** The announcement is a *templated render* of
   the existing changelog entry plus the version/tag and a link. No new claims, no editorializing
   beyond the template. The changelog is the source of truth for what shipped.
3. **Post via the secret-isolating helper to the fixed, hardcoded channel.** The bot token lives
   only in the locked credentials file (`secrets.credentials_file`), read by the helper — it
   **never enters the agent's context.** The destination channel is fixed and hardcoded, not chosen
   at runtime.
4. **Record the announcement and advance the marker.** Append to the **action ledger** (release,
   channel, timestamp, resulting message reference) — the append-only audit trail the
   [watchdog](../action-watchdog/SKILL.md) re-verifies — then advance the last-announced marker to
   this release. The ledger is what makes step 1 idempotent; keep it append-only, not a mutable state
   file.
5. **Stay inside the template.** Anything beyond the templated announcement — replies, follow-up
   conversation, cross-posting to social — is Band B **draft → human sends** (see the far horizon in
   the [community lifecycle](../../docs/lifecycle/community.md)). Never hold a conversation in the
   project's voice.

## Pitfalls
- **Double-announce.** The marker is the guard: check it *before* composing, advance it only *after*
  a confirmed post. If the job crashes between posting and writing the marker, the next run must
  verify against the channel/ledger and **not** blindly re-post.
- **Token leaking into context.** Never read the credentials file directly — only the helper touches
  it. If no secret-isolating helper exists yet, that's the build-nothing dependency to add first, not
  a reason to inline the token.
- **Announcing a release that didn't ship.** The precondition is a real tag with merged content —
  `release-pipeline` verifies "shipped" (tag exists **and** merge is real **and** the live version
  serves it). Don't announce from a changelog edit alone.
- **Composing original prose.** This is a render of existing content, not a writing task.
  Editorializing is exactly where an injected or wrong claim gets published in the project's voice —
  the one irreversible act this whole area is built to gate.

## Verification
- Runs **once per release**: on an already-announced release it is a silent no-op (marker respected).
- The bot token **never appears** in the agent transcript (helper-isolated).
- The posted content matches the changelog entry — no claim absent from the changelog.
- [`action-watchdog`](../action-watchdog/SKILL.md) has a corresponding check for autonomous
  announcements.
- On no new release, silent.
