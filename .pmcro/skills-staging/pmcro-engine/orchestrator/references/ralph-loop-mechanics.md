# Ralph-Loop Mechanics (Re-Implemented via Native /goal)

Some cycles genuinely need more sustained multi-turn iteration than one
Plan->Make->Check->Reflect pass comfortably holds -- e.g. scaffolding a
full new domain plugin end to end during `/ceo:evolve-colony`. This
document is the Colony's re-implementation of that persistent-iteration
pattern (the technique the community calls "Ralph" / "Ralph Wiggum"),
built on Claude Code's **native `/goal` command** rather than the
community `ralph-loop`/`ralph-wiggum` plugin.

## Why Not the `ralph-loop` Plugin

The plugin works by a Stop hook that intercepts session exit and re-feeds
the same prompt until an exact `<promise>TEXT</promise>` string match,
capped by `--max-iterations`. Two things make it the wrong fit here:

- **Fragile exact-string completion signal.** A typo or slight rephrasing
  and the loop never terminates on its own -- `--max-iterations` becomes
  the only real safety net, not a genuine "done" signal.
- **Platform dependency.** The plugin has a documented `jq` dependency
  that breaks under Windows/Git Bash -- an unmet dependency is exactly the
  kind of silent failure mode EC-VERIFY-FIRST-001 exists to catch before
  it causes a stuck loop.

## The Native `/goal` Mechanism (what this Colony uses instead)

Claude Code's `/goal` command lets you set a completion **condition**;
after every turn a small, fast model checks whether that condition holds
against the transcript. If not, Claude starts another turn instead of
returning control. The goal clears automatically once the condition is
met.

```
/goal <verifiable completion condition for this cycle>
```

## How This Composes With the PMCR-O Roles

`/goal` is a session-level mechanism; it doesn't replace `checker` or
`reflector`, it holds the session open long enough for a full
Plan->Make->Check->Reflect sequence (possibly several looped cycles) to
complete without the human re-prompting between turns.

1. `orchestrator` sets a `/goal` condition matching this cycle's intended
   `disposition.json` outcome.
2. Each turn within that `/goal` still runs a full
   Plan->Make->Check->Reflect pass, sequenced exactly as `SKILL.md`
   describes -- `/goal` does not change cycle discipline.
3. **EC-009 still applies inside a `/goal` session.** Cap cycles at 3 the
   same way as any other trail.
4. A `/goal` clearing is equivalent to a cycle's Checker returning `pass`
   -- it is **not** equivalent to `disposition: accept`. Anything this
   cycle touched under `catalog/` or a `marketplace.json` still seals
   `needs-approval`, regardless of how cleanly `/goal` resolved.

## Writing a Good Condition

Same discipline `checker` already applies to plan steps: the condition
must be something Claude's own output can demonstrate, not something that
requires external verification Claude can't run. Prefer conditions phrased
as "X passes Y check" over "X is good."
