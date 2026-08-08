# Safety Notes — Why `git` Is Local-Only

This skill's scope was fixed as a prior decision (see the seed intent that produced
it): local git operations only. This file exists so future authors don't quietly widen
the whitelist without re-deriving why it was narrow in the first place.

## Why No Remote Operations

`push`, `pull`, `fetch`, `clone`, and `remote add/remove` are excluded because:

- A remote operation can affect state outside the local machine (a shared repo, a
  team's branch, a CI trigger) in a way that a local `commit` or `branch` cannot.
  The blast radius is fundamentally different, so it doesn't belong in the same
  whitelist as read-only/local-only operations.
- Per MAF's own governance guidance (see `maf-agent-skills.md` in the `skill-creator`
  skill), script execution should be treated like third-party code and scoped as
  narrowly as the task allows. Local-only git is the narrowest scope that still covers
  routine commit/branch/diff/log work.
- If remote operations are needed later, they should be their own explicit decision —
  a new skill or an explicit widening of this one — not something this skill drifts
  into supporting because a whitelist entry got added casually.

## Why No History Rewriting

`rebase -i`, `reset --hard`, `push --force`, and `commit --amend` are excluded because
they change or destroy commits that may already be relied on elsewhere (even locally,
by another checkout or by the requester's own mental model of what happened). If a
requester genuinely needs one of these, it should go through a general-purpose terminal
skill with human approval per the Colony's `TerminalCommandPolicy` tiers (`RequiresHil`),
not through this skill's fixed whitelist.

## What To Do If One Is Requested

If a request implies a remote or history-rewriting operation:

1. Do not attempt to approximate it with a whitelisted command.
2. Report `status: out_of_scope` (per `SKILL.md`'s Required Result Format) and name the
   specific git operation that's needed.
3. Let the requester decide whether to use a different tool/skill or explicitly extend
   this one — don't make that call silently.

## Merge Conflicts

This skill's `status` and `diff` operations will surface a conflict (git reports it
directly), but nothing in the whitelist resolves one. Conflict resolution requires
judgment about which changes should win — that's a human or a higher-level agent
decision, not a mechanical git operation this skill should automate.
